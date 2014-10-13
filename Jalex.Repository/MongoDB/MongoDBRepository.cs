using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Jalex.Infrastructure.Extensions;
using Jalex.Infrastructure.Objects;
using Jalex.Infrastructure.ReflectedTypeDescriptor;
using Jalex.Infrastructure.Repository;
using Jalex.Infrastructure.Utils;
using Jalex.Repository.IdProviders;
using Magnum;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;

namespace Jalex.Repository.MongoDB
{
    public sealed class MongoDBRepository<T> : BaseRepository<T>, IQueryableRepository<T>
    {
        private const string _defaultCollectionPrefix = "mongo-collection-";

        // ReSharper disable once StaticFieldInGenericType
        private static IEnumerable<Tuple<IMongoIndexKeys, IMongoIndexOptions>> _indices;

        // ReSharper disable StaticFieldInGenericType
        private static bool _isInitialized; // one per T
        private static readonly object _syncRoot = new object();
        // ReSharper restore StaticFieldInGenericType

        private bool _indicesEnsured;

        private readonly MongoHelper _helper = new MongoHelper();

        public string ConnectionString
        {
            get { return _helper.ConnectionString; }
            set { _helper.ConnectionString = value; }
        }

        public string DatabaseName
        {
            get { return _helper.DatabaseName; }
            set { _helper.DatabaseName = value; }
        }

        public MongoDBRepository(
            IIdProvider idProvider,
            IReflectedTypeDescriptorProvider typeDescriptorProvider)
            : base(idProvider, typeDescriptorProvider)
        {
            ensureInitialized();
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public string CollectionName { get; set; }

        public bool TryGetById(string id, out T item)
        {
            Guard.AgainstNull(id, "id");

            MongoCollection<T> collection = getMongoCollection();

            IMongoQuery query = Query<T>.EQ(_typeDescriptor.IdGetterExpression, id);

            item = collection.FindOne(query);
            // ReSharper disable once CompareNonConstrainedGenericWithNull
            return item != null;
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

        /// <summary>
        /// Returns the first object stored in the repository that satisfies a given query, or default value for T if no such object is found
        /// </summary>
        /// <param name="query">The query that must be satisfied</param>
        /// <returns>The object in the repository that satisfies the query or the default value for T if no such object is found</returns>
        public T FirstOrDefault(Expression<Func<T, bool>> query)
        {
            MongoCollection<T> collection = getMongoCollection();
            var result = collection.AsQueryable().FirstOrDefault(query);
            return result;
        }

        public OperationResult Delete(string id)
        {
            ParameterChecker.CheckForNull(id, "id");
            MongoCollection<T> collection = getMongoCollection();

            try
            {
                WriteConcernResult wcr = collection.Remove(Query<T>.EQ(_typeDescriptor.IdGetterExpression, id));

                if (wcr.DocumentsAffected > 0)
                {
                    return new OperationResult(true);
                }
                return new OperationResult(false, Severity.Warning, "Could not delete {0} {1} as it was not found", _typeDescriptor.TypeName, id);
            }
            catch (WriteConcernException wce)
            {
                Logger.ErrorException(wce, "Error when deleting " + _typeDescriptor.TypeName);
                return new OperationResult(false, Severity.Error, string.Format("Failed to delete {0} {1}", _typeDescriptor.TypeName, id));
            }


        }

        #region Implementation of IWriter<in T>

        /// <summary>
        /// Saves an object
        /// </summary>
        /// <param name="obj">object to save</param>
        /// <param name="writeMode">writing mode. inserting an object that exists or updating an object that does not exist will fail. Defaults to upsert</param>
        /// <returns>Operation result with id of the new object in order of the objects given to this function</returns>
        public OperationResult<string> Save(T obj, WriteMode writeMode)
        {
            return SaveMany(new[] { obj }, writeMode).Single();
        }

        /// <summary>
        /// Saves objects
        /// </summary>
        /// <param name="objects">objects to save</param>
        /// <param name="writeMode">writing mode. inserting an object that exists or updating an object that does not exist will fail. Defaults to upsert</param>
        /// <returns>Operation result with ids of the new objects in order of the objects given to this function</returns>
        public IEnumerable<OperationResult<string>> SaveMany(IEnumerable<T> objects, WriteMode writeMode)
        {
            // ReSharper disable PossibleMultipleEnumeration
            Guard.AgainstNull(objects, "objects");

            MongoCollection<T> collection = getMongoCollection();
            T[] objectArr = objects.ToArray();

            try
            {
                switch (writeMode)
                {
                    case WriteMode.Insert:
                        ensureObjectIds(writeMode, objectArr);
                        collection.InsertBatch(objectArr);
                        return objectArr
                                    .Select(r => new OperationResult<string>(true, _typeDescriptor.GetId(r)))
                                    .ToArray();
                    case WriteMode.Update:
                        return createResults(writeMode, objectArr, doesObjectWithIdExist, obj => updateObject(obj, false, collection));
                    case WriteMode.Upsert:
                        return createResults(writeMode, objectArr, doesObjectWithIdExist, obj => updateObject(obj, true, collection));
                    default:
                        throw new ArgumentOutOfRangeException("writeMode");
                }

            }
            catch (WriteConcernException wce)
            {
                Logger.ErrorException(wce, "Error when creating " + _typeDescriptor.TypeName);
                return objectArr.Select(r =>
                                        new OperationResult<string>(
                                            false,
                                            null,
                                            Severity.Error,
                                            string.Format("Failed to create {0} {1}", _typeDescriptor.TypeName, r.ToString())))
                                .ToArray();
            }
            catch (FormatException fe)
            {
                Logger.ErrorException(fe, "Formatting error when creating " + _typeDescriptor.TypeName);
                throw new IdFormatException(fe.Message);
            }
        }

        #endregion

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

            foreach (var indexGroup in indexedProps.GroupBy(i => i.IndexedAttribute.Name))
            {
                // if name is null/empty then we are just creating an index on a single prop, otherwise its a combination in
                if (string.IsNullOrEmpty(indexGroup.Key))
                {
                    // ReSharper disable once LoopCanBeConvertedToQuery
                    foreach (var index in indexGroup)
                    {
                        var builder = new IndexKeysBuilder();
                        if (index.IndexedAttribute.SortOrder == IndexedAttribute.Order.Descending)
                        {
                            builder.Descending(index.PropertyName);
                        }
                        else
                        {
                            builder.Ascending(index.PropertyName);
                        }

                        Tuple<IMongoIndexKeys, IMongoIndexOptions> mongoIndexTuple = Tuple
                            .Create<IMongoIndexKeys, IMongoIndexOptions>(
                                builder,
                                IndexOptions.Null);
                        mongoIndices.Add(mongoIndexTuple);
                    }
                }
                else
                {
                    var builder = new IndexKeysBuilder();
                    foreach (var index in indexGroup.OrderBy(p => p.IndexedAttribute.Index))
                    {
                        if (index.IndexedAttribute.SortOrder == IndexedAttribute.Order.Descending)
                        {
                            builder.Descending(index.PropertyName);
                        }
                        else
                        {
                            builder.Ascending(index.PropertyName);
                        }
                    }

                    Tuple<IMongoIndexKeys, IMongoIndexOptions> mongoIndexTuple = Tuple
                            .Create<IMongoIndexKeys, IMongoIndexOptions>(
                                builder,
                                IndexOptions.Null);
                    mongoIndices.Add(mongoIndexTuple);
                }
            }
            return mongoIndices;
        }

        private void ensureIndices(MongoCollection<T> collection)
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

        private MongoCollection<T> getMongoCollection()
        {
            string collectionName = CollectionName ??
                                    ConfigurationManager.AppSettings[_defaultCollectionPrefix + _typeDescriptor.TypeName] ??
                                    string.Format("{0}s", _typeDescriptor.TypeName);

            MongoDatabase db = _helper.GetMongoDatabase();
            MongoCollection<T> collection = db.GetCollection<T>(collectionName);
            ensureIndices(collection);

            return collection;
        }        

        private bool doesObjectWithIdExist(string id)
        {
            T dummy;
            return TryGetById(id, out dummy);
        }

        private bool updateObject(T obj, bool upsert, MongoCollection<T> collection)
        {
            WriteConcernResult wcr =
                collection
                .Update(
                        Query<T>.EQ(_typeDescriptor.IdGetterExpression, _typeDescriptor.GetId(obj)),
                        Update<T>.Replace(obj),
                        upsert ? UpdateFlags.Upsert : UpdateFlags.None);
            bool success = wcr.DocumentsAffected > 0;
            return success;
        }
    }
}