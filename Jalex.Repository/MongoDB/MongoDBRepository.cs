using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Jalex.Infrastructure.Extensions;
using Jalex.Infrastructure.Objects;
using Jalex.Infrastructure.ReflectedTypeDescriptor;
using Jalex.Infrastructure.Repository;
using Jalex.Repository.IdProviders;
using Magnum.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Jalex.Repository.MongoDB
{
    public sealed class MongoDBRepository<T> : BaseRepository<T>, IQueryableRepository<T> 
        where T : class
    {
        private const string _defaultCollectionPrefix = "mongo-collection-";

        // ReSharper disable once StaticFieldInGenericType
        private static IEnumerable<Tuple<IndexKeysDefinition<T>, CreateIndexOptions>> _indices;

        // ReSharper disable StaticMemberInGenericType
        private static bool _isInitialized; // one per Repository
        private static readonly object _syncRoot = new object();
        private static bool _indicesEnsured;
        // ReSharper restore StaticMemberInGenericType

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

        public async Task<T> GetByIdAsync(Guid id)
        {
            var collection = getMongoCollection();
            var filter = Builders<T>.Filter.Eq(_typeDescriptor.IdGetterExpression, id);
            var result = await collection.Find(filter)
                                         .Limit(1)
                                         .ToListAsync()
                                         .ConfigureAwait(false);
            return result.FirstOrDefault();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            var collection = getMongoCollection();
            var result = await collection.FindAsync(new BsonDocument()).ConfigureAwait(false);
            return await result.ToListAsync().ConfigureAwait(false);
        }

        public async Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> query)
        {
            var collection = getMongoCollection();
            var result = await collection.Find(query)
                                         .ToListAsync()
                                         .ConfigureAwait(false);
            return result;
        }

        public async Task<IEnumerable<TProjection>> ProjectAsync<TProjection>(Expression<Func<T, TProjection>> projection, Expression<Func<T, bool>> query)
        {
            var collection = getMongoCollection();
            var result = await collection.Find(query)
                                         .Project(projection)
                                         .ToListAsync()
                                         .ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Projects a subset of all objects in the repository
        /// </summary>
        public async Task<IEnumerable<TProjection>> ProjectAsync<TProjection>(Expression<Func<T, TProjection>> projection)
        {
            var collection = getMongoCollection();
            var result = await collection.Find(new BsonDocument())
                                         .Project(projection)
                                         .ToListAsync()
                                         .ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Returns the first object stored in the repository that satisfies a given query, or default value for T if no such object is found
        /// </summary>
        /// <param name="query">The query that must be satisfied</param>
        /// <returns>The object in the repository that satisfies the query or the default value for T if no such object is found</returns>
        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> query)
        {
            var collection = getMongoCollection();
            var result = await collection.Find(query)
                                         .Limit(1)
                                         .ToListAsync()
                                         .ConfigureAwait(false);
            return result.FirstOrDefault();
        }

        public async Task<OperationResult> DeleteAsync(Guid id)
        {
            var collection = getMongoCollection();
            var filter = Builders<T>.Filter.Eq(_typeDescriptor.IdGetterExpression, id);

            try
            {
                var result = await collection.DeleteOneAsync(filter).ConfigureAwait(false);

                if (result.DeletedCount > 0)
                {
                    return new OperationResult(true);
                }
                return new OperationResult(false, Severity.Warning, "Could not delete {0} with id {1} as it was not found using expression", _typeDescriptor.TypeName, id.ToString());
            }
            catch (MongoWriteConcernException wce)
            {
                Logger.Error(wce, "Error when deleting " + _typeDescriptor.TypeName);
                return new OperationResult(false, Severity.Error, string.Format("Failed to delete {0} with id {1}", _typeDescriptor.TypeName, id));
            }
        }

        /// <summary>
        /// Deletes all items that match a given expression
        /// </summary>
        /// <param name="expression">The expression to match</param>
        /// <returns>Whether the operation executed successfully or not</returns>
        public async Task<OperationResult> DeleteWhereAsync(Expression<Func<T, bool>> expression)
        {
            var collection = getMongoCollection();

            try
            {
                var result = await collection.DeleteOneAsync(expression).ConfigureAwait(false);

                if (result.DeletedCount > 0)
                {
                    return new OperationResult(true);
                }
                return new OperationResult(false, Severity.Warning, "Could not delete {0} {1} as it was not found using expression", _typeDescriptor.TypeName, expression.ToString());
            }
            catch (MongoWriteConcernException wce)
            {
                Logger.Error(wce, "Error when deleting " + _typeDescriptor.TypeName);
                return new OperationResult(false, Severity.Error, string.Format("Failed to delete {0} using expression {1}", _typeDescriptor.TypeName, expression));
            }
        }

        #region Implementation of IWriter<in T>

        /// <summary>
        /// Saves an object
        /// </summary>
        /// <param name="obj">object to save</param>
        /// <param name="writeMode">writing mode. inserting an object that exists or updating an object that does not exist will fail. Defaults to upsert</param>
        /// <returns>Operation result with id of the new object in order of the objects given to this function</returns>
        public async Task<OperationResult<Guid>> SaveAsync(T obj, WriteMode writeMode)
        {
            return (await SaveManyAsync(new[] { obj }, writeMode).ConfigureAwait(false)).Single();
        }

        /// <summary>
        /// Saves objects
        /// </summary>
        /// <param name="objects">objects to save</param>
        /// <param name="writeMode">writing mode. inserting an object that exists or updating an object that does not exist will fail. Defaults to upsert</param>
        /// <returns>Operation result with ids of the new objects in order of the objects given to this function</returns>
        public async Task<IEnumerable<OperationResult<Guid>>> SaveManyAsync(IEnumerable<T> objects, WriteMode writeMode)
        {
            var collection = getMongoCollection();
            T[] objectArr = objects.ToArray();

            try
            {
                ensureObjectIds(writeMode, objectArr);

                switch (writeMode)
                {
                    case WriteMode.Insert:
                        await collection.InsertManyAsync(objectArr).ConfigureAwait(false);
                        return objectArr
                                    .Select(r => new OperationResult<Guid>(true, _typeDescriptor.GetId(r)))
                                    .ToCollection();
                    case WriteMode.Update:
                        return await updateManyAsync(objectArr, collection, false).ConfigureAwait(false);
                    case WriteMode.Upsert:
                        return await updateManyAsync(objectArr, collection, true).ConfigureAwait(false);
                    default:
                        throw new ArgumentOutOfRangeException("writeMode");
                }

            }
            catch (FormatException fe)
            {
                Logger.Error(fe, "Formatting error when creating " + _typeDescriptor.TypeName);
                throw new IdFormatException(fe.Message);
            }
            catch (MongoBulkWriteException wce)
            {
                Logger.Error(wce, "Error when creating " + _typeDescriptor.TypeName);
                return objectArr.Select(r =>
                                        new OperationResult<Guid>(
                                            false,
                                            Guid.Empty,
                                            Severity.Error,
                                            string.Format("Failed to create {0} {1}", _typeDescriptor.TypeName, r.ToString())))
                                .ToArray();
            }            
        }

        private async Task<IEnumerable<OperationResult<Guid>>> updateManyAsync(T[] objectArr, IMongoCollection<T> collection, bool isUpsert)
        {
            var tasks = objectArr.Select(o => updateObject(o, collection, isUpsert))
                                 .ToCollection();
            await Task.WhenAll(tasks).ConfigureAwait(false);
            var results = tasks.Select(t => t.Result);
            return results.Select(createUpdateOperationResult)
                          .ToCollection();
        }

        private static OperationResult<Guid> createUpdateOperationResult(Tuple<ReplaceOneResult, Guid> resultAndId)
        {
            var replaceResult = resultAndId.Item1;
            var id = replaceResult.UpsertedId?.AsGuid ?? resultAndId.Item2;

            var success = replaceResult.MatchedCount == 1 || replaceResult.UpsertedId != null;
            var operationResult = new OperationResult<Guid>(success, id);

            if (!success)
            {
                operationResult.Messages = new[] {new Message(Severity.Warning, "Entity with id " + id + " does not exist")};
            }

            return operationResult;
        }

        private async Task<Tuple<ReplaceOneResult, Guid>> updateObject(T o, IMongoCollection<T> collection, bool isUpsert)
        {
            var id = _typeDescriptor.GetId(o);
            var idEquals = Builders<T>.Filter.Eq(_typeDescriptor.IdGetterExpression, id);
            var result = await collection.ReplaceOneAsync(idEquals,
                                                          o,
                                                          new UpdateOptions
                                                          {
                                                              IsUpsert = isUpsert
                                                          }).ConfigureAwait(false);
            return new Tuple<ReplaceOneResult, Guid>(result, id);
        }

        #endregion
        
        private void initialize()
        {
            MongoSetup.EnsureInitialized();

            registerConventions();
            // class map must be registered after convention
            registerClassMap(_typeDescriptor.IdPropertyName, _typeDescriptor.IsIdAutoGenerated);

            _indices = createIndices(_typeDescriptor.Properties);
        }

        private void ensureInitialized()
        {
            if (!_isInitialized)
            {
                lock (_syncRoot)
                {
                    if (!_isInitialized)
                    {
                        initialize();
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
                cm.MapMember(_typeDescriptor.IdGetterExpression);
                cm.SetIdMember(cm.GetMemberMap(idPropertyName));
                if (isIdAutoGenerated)
                {
                    var provider = _idProvider as IIdGenerator;
                    if (provider != null)
                    {
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
                var type = m.MemberType.GetNullableUnderlyingType();
                if (type.IsEnum)
                {
                    var serializer = (IBsonSerializer)FastActivator.Create(typeof (EnumSerializer<>), new[] {type}, BsonType.String);
                    m.SetSerializer(serializer);
                }
            });

            ConventionRegistry.Register(typeof(T).FullName, entityConventionPack, t => typeof(T).IsAssignableFrom(t));
        }

        private IEnumerable<Tuple<IndexKeysDefinition<T>, CreateIndexOptions>> createIndices(
            IEnumerable<PropertyInfo> classProps)
        {
            var indexedProps = (from prop in classProps
                                let indexedAttr = prop.GetCustomAttributes(true).FirstOrDefault(p => p is IndexedAttribute)
                                where indexedAttr != null
                                select new { PropertyName = prop.Name, IndexedAttribute = (IndexedAttribute)indexedAttr })
                .ToArray();

            var mongoIndices = new List<Tuple<IndexKeysDefinition<T>, CreateIndexOptions>>();

            foreach (var indexGroup in indexedProps.GroupBy(i => i.IndexedAttribute.Name))
            {
                // if name is null/empty then we are just creating an index on a single prop, otherwise its a combination in
                if (string.IsNullOrEmpty(indexGroup.Key))
                {
                    // ReSharper disable once LoopCanBeConvertedToQuery
                    foreach (var index in indexGroup)
                    {
                        var mongoIndex = createIndex(index.IndexedAttribute, index.PropertyName);
                        var mongoIndexTuple = new Tuple<IndexKeysDefinition<T>, CreateIndexOptions>(mongoIndex, new CreateIndexOptions());
                        mongoIndices.Add(mongoIndexTuple);
                    }
                }
                else
                {
                    var definitions = indexGroup.OrderBy(p => p.IndexedAttribute.Index)
                                                .Select(x => createIndex(x.IndexedAttribute, x.PropertyName));

                    Tuple<IndexKeysDefinition<T>, CreateIndexOptions> mongoIndexTuple =
                        new Tuple<IndexKeysDefinition<T>, CreateIndexOptions>(
                            Builders<T>.IndexKeys.Combine(definitions),
                            new CreateIndexOptions
                            {
                                Name = indexGroup.Key
                            });
                    mongoIndices.Add(mongoIndexTuple);
                }
            }
            return mongoIndices;
        }

        private static IndexKeysDefinition<T> createIndex(IndexedAttribute indexedAttribute, string propertyName)
        {
            var definition = indexedAttribute.SortOrder == IndexedAttribute.Order.Descending
                                                 ? Builders<T>.IndexKeys.Descending(propertyName)
                                                 : Builders<T>.IndexKeys.Ascending(propertyName);
            return definition;
        }

        private void ensureIndices(IMongoCollection<T> collection)
        {
            if (!_indicesEnsured)
            {
                lock (_syncRoot)
                {
                    foreach (var index in _indices)
                    {
                        collection.Indexes.CreateOneAsync(index.Item1, index.Item2);
                    }
                }
                _indicesEnsured = true;
            }
        }

        private IMongoCollection<T> getMongoCollection()
        {
            string collectionName = CollectionName ??
                                    ConfigurationManager.AppSettings[_defaultCollectionPrefix + _typeDescriptor.TypeName] ??
                                    string.Format("{0}s", _typeDescriptor.TypeName);

            IMongoDatabase db = _helper.GetMongoDatabase();
            IMongoCollection<T> collection = db.GetCollection<T>(collectionName);
            ensureIndices(collection);

            return collection;
        }
    }
}