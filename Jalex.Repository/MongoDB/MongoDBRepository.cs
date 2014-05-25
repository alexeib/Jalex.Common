using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Jalex.Infrastructure.Extensions;
using Jalex.Infrastructure.Objects;
using Jalex.Infrastructure.Repository;
using Jalex.Infrastructure.Utils;
using Jalex.Repository.IdProviders;
using Jalex.Repository.Utils;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;

namespace Jalex.Repository.MongoDB
{
    public class MongoDBRepository<T> : BaseMongoDBRepository, IQueryableRepository<T>
    {
        private const string _defaultCollectionPrefix = "mongo-collection-";

        // ReSharper disable once StaticFieldInGenericType
        private static IEnumerable<Tuple<IMongoIndexKeys, IMongoIndexOptions>> _indices;

        // ReSharper disable StaticFieldInGenericType
        private static bool _isInitialized; // one per T
        private static readonly object _syncRoot = new object();
        // ReSharper restore StaticFieldInGenericType

        private bool _indicesEnsured;

        private readonly IIdProvider _idProvider;
        private readonly IReflectedTypeDescriptor<T> _typeDescriptor;

        public MongoDBRepository(
            IIdProvider idProvider,
            IReflectedTypeDescriptorProvider typeDescriptorProvider)
        {
            _idProvider = idProvider;
            _typeDescriptor = typeDescriptorProvider.GetReflectedTypeDescriptor<T>();

            ensureInitialized();
        }

        public string CollectionName { get; set; }

        public IEnumerable<T> GetByIds(IEnumerable<string> ids)
        {
            ParameterChecker.CheckForVoid(() => ids);

            string[] idsArr = ids.ToArray();

            MongoCollection<T> collection = getMongoCollection();

            IMongoQuery query = Query<T>.In(_typeDescriptor.IdGetterExpression, idsArr);

            return collection.Find(query).SetLimit(idsArr.Length);
        }

        public IEnumerable<T> GetAll()
        {
            MongoCollection<T> collection = getMongoCollection();
            return collection.FindAll();
        }

        public IEnumerable<T> Query(Expression<Func<T, bool>> query)
        {
            MongoCollection<T> collection = getMongoCollection();
            return collection.AsQueryable().Where(query);
        }

        public IEnumerable<OperationResult<string>> Create(IEnumerable<T> newObjects)
        {
            ParameterChecker.CheckForVoid(() => newObjects);

            T[] objectArr = newObjects.ToArray();
            HashSet<string> existingIds = new HashSet<string>();


            foreach (var newObj in objectArr)
            {
                string id = _typeDescriptor.GetId(newObj);

                if (!string.IsNullOrEmpty(id))
                {
                    if (!existingIds.Add(id))
                    {
                        throw new DuplicateIdException("Attempting to create multiple objects with id " + id + " is not allowed");
                    }
                }
            }

            MongoCollection<T> collection = getMongoCollection();

            try
            {
                collection.InsertBatch(objectArr);
            }
            catch (WriteConcernException wce)
            {
                Logger.ErrorException(wce, "Error when creating " + _typeDescriptor.TypeName);
                return objectArr.Select(r =>
                                        new OperationResult<string>
                                        {
                                            Success = false,
                                            Value = null,
                                            Messages = new[]
                                                       {
                                                           new Message(Severity.Error,
                                                                       string.Format("Failed to create {0} {1}",
                                                                                     _typeDescriptor.TypeName,
                                                                                     r.ToString()))
                                                       }
                                        }).ToArray();
            }
            catch (FormatException fe)
            {
                Logger.ErrorException(fe, "Formatting error when creating " + _typeDescriptor.TypeName);
                throw new IdFormatException(fe.Message);
            }

            return objectArr.Select(r => new OperationResult<string> { Success = true, Value = _typeDescriptor.GetId(r) }).ToArray();
        }

        public IEnumerable<OperationResult> Update(IEnumerable<T> objectsToUpdate)
        {
            ParameterChecker.CheckForVoid(() => objectsToUpdate);

            List<OperationResult> results = new List<OperationResult>();

            MongoCollection<T> collection = getMongoCollection();

            foreach (var objectToUpdate in objectsToUpdate)
            {
                OperationResult result;

                try
                {
                    WriteConcernResult wcr = collection.Update(Query<T>.EQ(_typeDescriptor.IdGetterExpression, _typeDescriptor.GetId(objectToUpdate)),
                                                               Update<T>.Replace(objectToUpdate));
                    bool success = wcr.DocumentsAffected > 0;
                    result = new OperationResult { Success = success };

                    if (!success)
                    {
                        result.Messages = new[]
                                      {
                                          new Message(Severity.Warning,
                                                      string.Format("Could not update {0} because it was not found", objectToUpdate))
                                      };
                    }
                }
                catch (WriteConcernException wce)
                {
                    Logger.ErrorException(wce, "Error when updating " + _typeDescriptor.TypeName);
                    result = new OperationResult
                           {
                               Success = false,
                               Messages = new[]
                                          {
                                              new Message(Severity.Error, string.Format("Failed to update {0} {1}", _typeDescriptor.TypeName, objectToUpdate))
                                          }
                           };
                }

                results.Add(result);
            }

            return results;
        }

        public IEnumerable<OperationResult> Delete(IEnumerable<string> ids)
        {
            ParameterChecker.CheckForVoid(() => ids);

            string[] idsArr = ids.ToArray();

            MongoCollection<T> collection = getMongoCollection();
            bool success;

            try
            {
                WriteConcernResult wcr = collection.Remove(Query<T>.In(_typeDescriptor.IdGetterExpression, idsArr));
                success = wcr.DocumentsAffected > 0;
            }
            catch (WriteConcernException wce)
            {
                Logger.ErrorException(wce, "Error when deleting " + _typeDescriptor.TypeName);
                return idsArr.Select(r =>
                    new OperationResult
                    {
                        Success = false,
                        Messages = new[]
                        {
                            new Message(Severity.Error,
                                string.Format("Failed to delete {0} {1}", _typeDescriptor.TypeName,
                                    r.ToString(CultureInfo.InvariantCulture)))
                        }
                    }).ToArray();
            }

            // TODO: success may not be really true if we are trying to delete multiple things
            return idsArr.Select(i => new OperationResult { Success = success }).ToArray();
        }

        private void ensureInitialized()
        {
            if (!_isInitialized)
            {
                lock (_syncRoot)
                {
                    if (!_isInitialized)
                    {                        
                        MongoSetup.EnsureInitialized();

                        registerConventions();
                        // class map must be registered after convention
                        registerClassMap(_typeDescriptor.IdPropertyName, _typeDescriptor.IsIdAutoGenerated);

                        _indices = createIndices(_typeDescriptor.Properties);

                        _isInitialized = true;
                    }
                }
            }
        }

        private void registerClassMap(string idPropertyName, bool isIdAutoGenerated)
        {
            BsonClassMap.RegisterClassMap<T>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(idPropertyName));
                if (isIdAutoGenerated)
                {
                    var provider = _idProvider as IIdGenerator;
                    if (provider != null)
                    {
                        if (provider is ObjectIdIdProvider)
                        {
                            cm.IdMemberMap.SetRepresentation(BsonType.ObjectId);
                        }
                        cm.IdMemberMap.SetIdGenerator(provider);
                    }
                    else
                    {
                        throw new InvalidOperationException("Id provider should implement IIdGenerator to be compatible with MongoDB");
                    }
                }
            });
        }

        private void registerConventions()
        {
            var entityConventionPack = new ConventionPack();

            entityConventionPack.AddMemberMapConvention("Enum as string", m =>
            {
                if (m.MemberType.GetNullableUnderlyingType().IsEnum)
                {
                    m.SetRepresentation(BsonType.String);
                }
            });

            ConventionRegistry.Register(typeof(T).FullName, entityConventionPack, t => typeof(T).IsAssignableFrom(t));
        }

        private IEnumerable<Tuple<IMongoIndexKeys, IMongoIndexOptions>> createIndices(
            IEnumerable<PropertyInfo> classProps)
        {
            var indexedProps = (from prop in classProps
                                let indexedAttr = prop.GetCustomAttributes(true).FirstOrDefault(p => p is IndexedAttribute)
                                where indexedAttr != null
                                select new { PropertyName = prop.Name, IndexedAttribute = (IndexedAttribute)indexedAttr })
                .ToArray();

            var mongoIndices = new List<Tuple<IMongoIndexKeys, IMongoIndexOptions>>();

            foreach (var indexGroup in indexedProps.GroupBy(i => i.IndexedAttribute.IndexGroup))
            {
                // if group name is null/empty then we are just creating an index on a single prop, otherwise its a combination in
                if (string.IsNullOrEmpty(indexGroup.Key))
                {
                    // ReSharper disable once LoopCanBeConvertedToQuery
                    foreach (var index in indexGroup)
                    {
                        Tuple<IMongoIndexKeys, IMongoIndexOptions> mongoIndexTuple = Tuple
                            .Create<IMongoIndexKeys, IMongoIndexOptions>(
                                new IndexKeysBuilder().Ascending(index.PropertyName),
                                IndexOptions.SetUnique(index.IndexedAttribute.IsUnique));
                        mongoIndices.Add(mongoIndexTuple);
                    }
                }
                else
                {
                    Tuple<IMongoIndexKeys, IMongoIndexOptions> mongoIndexTuple = Tuple
                        .Create<IMongoIndexKeys, IMongoIndexOptions>(
                            new IndexKeysBuilder().Ascending(
                                indexGroup.Select(i => i.PropertyName).OrderBy(p => p).ToArray()),
                            IndexOptions.Null);
                    mongoIndices.Add(mongoIndexTuple);
                }
            }

            return mongoIndices;
        }

        protected virtual void ensureIndices(MongoCollection<T> collection)
        {
            if (!_indicesEnsured)
            {
                lock (_indices)
                {
                    foreach (var index in _indices)
                    {
                        collection.CreateIndex(index.Item1, index.Item2);
                    }
                }
                _indicesEnsured = true;
            }
        }

        protected MongoCollection<T> getMongoCollection()
        {
            string collectionName = CollectionName ??
                                    ConfigurationManager.AppSettings[_defaultCollectionPrefix + _typeDescriptor.TypeName] ??
                                    string.Format("{0}s", _typeDescriptor.TypeName);

            MongoDatabase db = getMongoDatabase();
            MongoCollection<T> collection = db.GetCollection<T>(collectionName);
            ensureIndices(collection);

            return collection;
        }
    }
}