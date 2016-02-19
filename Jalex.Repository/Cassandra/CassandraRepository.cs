using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Cassandra;
using Cassandra.Data.Linq;
using Cassandra.Mapping;
using Jalex.Infrastructure.Expressions;
using Jalex.Infrastructure.Extensions;
using Jalex.Infrastructure.Objects;
using Jalex.Infrastructure.ReflectedTypeDescriptor;
using Jalex.Infrastructure.Repository;
using Jalex.Repository.IdProviders;

namespace Jalex.Repository.Cassandra
{
    public class CassandraRepository<T> : BaseRepository<T>, IQueryableRepositoryWithTtl<T> where T : class
    {        
        // ReSharper disable once StaticMemberInGenericType
        private static readonly HashSet<Type> _nativelySupportedTypes = new HashSet<Type>
                                                                        {
                                                                            typeof (string),
                                                                            typeof (long),
                                                                            typeof (byte[]),
                                                                            typeof (bool),
                                                                            typeof (double),
                                                                            typeof (float),
                                                                            typeof (IPAddress),
                                                                            typeof (int),
                                                                            typeof (DateTimeOffset),
                                                                            typeof (DateTime),
                                                                            typeof (Guid),
                                                                            typeof (TimeUuid),
                                                                            TypeAdapters.DecimalTypeAdapter.GetDataType(),
                                                                            TypeAdapters.VarIntTypeAdapter.GetDataType()
                                                                        };

        static CassandraRepository()
        {
            CassandraSetup.EnsureInitialized();
        }

        // ReSharper disable StaticFieldInGenericType
        private static bool _isInitialized; // one per Repository
        private static bool _isTableCreated; //one per Repository
        private static readonly object _initializeSyncRoot = new object();
        private static readonly object _createTableSyncRoot = new object();
        // ReSharper restore StaticFieldInGenericType

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public string Keyspace { get; set; }

        private readonly Lazy<ISession> _session;

        public CassandraRepository(
            IIdProvider idProvider,
            IReflectedTypeDescriptorProvider typeDescriptorProvider)
            : base(idProvider, typeDescriptorProvider)
        {
            _session = new Lazy<ISession>(() =>
                                         {
                                             ensureInitialized();
                                             var session = getCassandraSession();
                                             createTableIfNotExist(session);
                                             return session;
                                         });
        }

        #region Implementation of IReader<out T>

        public Task<T> GetByIdAsync(Guid id)
        {
            var table = new Table<T>(_session.Value);

            var idEquals = _typeDescriptor.GetExpressionForIdEquality(id);

            return table.FirstOrDefault(idEquals)
                        .ExecuteAsync();
        }

        public Task<IEnumerable<T>> GetAllAsync()
        {
            var table = new Table<T>(_session.Value);
            return table.ExecuteAsync();
        }

        #endregion

        #region Implementation of IDeleter<T>

        public async Task<OperationResult> DeleteAsync(Guid id)
        {

            T existingEntity = await GetByIdAsync(id).ConfigureAwait(false);

            if (existingEntity == null)
            {
                return new OperationResult(false);
            }

            var table = new Table<T>(_session.Value);
            var idEquals = _typeDescriptor.GetExpressionForIdEquality(id);
            var statement = table.Where(idEquals).Delete();

            try
            {
                await statement.ExecuteAsync().ConfigureAwait(false);
                return new OperationResult(true);
            }
            catch (CqlArgumentException cae)
            {
                Logger.Error(cae, "Error when deleting " + _typeDescriptor.TypeName);
                return new OperationResult(false, Severity.Error, string.Format("Failed to delete {0} {1}", _typeDescriptor.TypeName, id));
            }
        }

        public async Task<OperationResult> DeleteWhereAsync(Expression<Func<T, bool>> expression)
        {
            var table = new Table<T>(_session.Value);
            var statement = table.Where(expression)
                                 .Delete();

            try
            {
                await statement.ExecuteAsync().ConfigureAwait(false);
                return new OperationResult(true);
            }
            catch (CqlArgumentException cae)
            {
                Logger.Error(cae, "Error when deleting " + _typeDescriptor.TypeName);
                return new OperationResult(false, Severity.Error, string.Format("Failed to delete {0} by expression {1}", _typeDescriptor.TypeName, expression));
            }
        }

        #endregion

        #region Implementation of IQueryableReader<out T>

        public Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> query)
        {
            var table = new Table<T>(_session.Value);

            var queryCommand = table.Where(query);
            return queryCommand.ExecuteAsync();
        }

        public Task<IEnumerable<TProjection>> ProjectAsync<TProjection>(Expression<Func<T, TProjection>> projection, Expression<Func<T, bool>> query)
        {
            var table = new Table<T>(_session.Value);

            var queryCommand = table.Where(query)
                                    .Select(projection);
            return queryCommand.ExecuteAsync();
        }

        /// <summary>
        /// Projects a subset of all objects in the repository
        /// </summary>
        public Task<IEnumerable<TProjection>> ProjectAsync<TProjection>(Expression<Func<T, TProjection>> projection)
        {
            var table = new Table<T>(_session.Value);

            var queryCommand = table.Select(projection);
            return queryCommand.ExecuteAsync();
        }

        /// <summary>
        /// Returns the first object stored in the repository that satisfies a given query, or default value for T if no such object is found
        /// </summary>
        /// <param name="query">The query that must be satisfied</param>
        /// <returns>The object in the repository that satisfies the query or the default value for T if no such object is found</returns>
        public Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> query)
        {
            var table = new Table<T>(_session.Value);

            var firstOrDefaultCommand = table.FirstOrDefault(query);
            return firstOrDefaultCommand.ExecuteAsync();
        }

        #endregion

        #region Implementation of IWriter<in T>


        /// <summary>
        /// Saves objects
        /// </summary>
        /// <param name="objects">objects to save</param>
        /// <param name="writeMode">writing mode. inserting an object that exists or updating an object that does not exist will fail. Defaults to upsert</param>
        /// <returns>Operation result with ids of the new objects in order of the objects given to this function</returns>
        public Task<IEnumerable<OperationResult<Guid>>> SaveManyAsync(IEnumerable<T> objects, WriteMode writeMode)
        {
            return SaveManyAsync(objects, writeMode, null);
        }

        #endregion

        #region Implementation of IWriterWithTtl<in T>

        /// <summary>
        /// Saves objects
        /// </summary>
        /// <param name="objects">objects to save</param>
        /// <param name="writeMode">writing mode. inserting an object that exists or updating an object that does not exist will fail. Defaults to upsert</param>
        /// <param name="timeToLive">The lifetime of all objects to be saved before they are deleted. If null, objects never expire</param>
        /// <returns>Operation result with ids of the new objects in order of the objects given to this function</returns>
        public async Task<IEnumerable<OperationResult<Guid>>> SaveManyAsync(IEnumerable<T> objects, WriteMode writeMode, TimeSpan? timeToLive)
        {
            if (objects == null) throw new ArgumentNullException(nameof(objects));

            var objCollection = objects.ToCollection();
            ensureObjectIds(writeMode, objCollection);

            try
            {
                switch (writeMode)
                {
                    case WriteMode.Upsert:
                        return await insertAsync(objCollection, timeToLive).ConfigureAwait(false);
                    case WriteMode.Insert:
                        var objGroups = objCollection.GroupBy(o => _typeDescriptor.GetId(o) == Guid.Empty);
                        var tasks = objGroups.Select(g => g.Key ? insertAsync(g.ToCollection(), timeToLive) : insertIfNotExistsAsync(g, timeToLive))
                                             .ToCollection();
                        await Task.WhenAll(tasks).ConfigureAwait(false);
                        return tasks.SelectMany(r => r.Result.ToCollection())
                                    .ToCollection();
                    case WriteMode.Update:
                        return await updateAsync(objCollection, timeToLive).ConfigureAwait(false);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(writeMode), writeMode, "not supported");
                }
            }
            catch (CqlArgumentException cae)
            {
                Logger.Error(cae, "Error when saving " + _typeDescriptor.TypeName);
                return objCollection.Select(r =>
                                            new OperationResult<Guid>(
                                                false,
                                                _typeDescriptor.GetId(r),
                                                Severity.Error,
                                                $"Failed to create {_typeDescriptor.TypeName} {r.ToString()}"))
                                    .ToArray();
            }
        }

        /// <summary>
        /// Refreshes and updates time to live for a given set of ids
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="timeToLive"></param>
        /// <returns></returns>
        public async Task<IEnumerable<OperationResult<Guid>>> UpdateTtlAsync(IEnumerable<Guid> ids, TimeSpan? timeToLive)
        {
            if (ids == null) throw new ArgumentNullException(nameof(ids));

            var items = await this.GetManyByIdAsync(ids);
            return await SaveManyAsync(items, WriteMode.Update, timeToLive);
        }

        private async Task<IEnumerable<OperationResult<Guid>>> insertAsync(IReadOnlyCollection<T> objCollection, TimeSpan? timeToLive)
        {
            var table = new Table<T>(_session.Value);
            var tasks = objCollection.Select(o => insertSingleAsync(o, table, timeToLive))
                                     .ToCollection();
            await Task.WhenAll(tasks).ConfigureAwait(false);
            return tasks.Select(t => t.Result);
        }

        private async Task<IEnumerable<OperationResult<Guid>>> insertIfNotExistsAsync(IEnumerable<T> objCollection, TimeSpan? timeToLive)
        {
            var table = new Table<T>(_session.Value);
            var tasks = objCollection.Select(o => insertSingleIfNotExistsAsync(o, table, timeToLive))
                                     .ToCollection();
            await Task.WhenAll(tasks).ConfigureAwait(false);
            return tasks.Select(t => t.Result);
        }

        private async Task<OperationResult<Guid>> insertSingleIfNotExistsAsync(T o, Table<T> table, TimeSpan? timeToLive)
        {
            var id = _typeDescriptor.GetId(o);
            var cmd = table.Insert(o)
                           .IfNotExists();
            if (timeToLive.HasValue)
            {
                cmd.SetTTL(Math.Max(1, (int)timeToLive.Value.TotalSeconds));
            }
            var result = await cmd.ExecuteAsync().ConfigureAwait(false);
            if (result.Applied) return new OperationResult<Guid>(true, id);
            return new OperationResult<Guid>(false, id, Severity.Error, "Object already exists");
        }

        private async Task<OperationResult<Guid>> insertSingleAsync(T o, Table<T> table, TimeSpan? timeToLive)
        {
            var id = _typeDescriptor.GetId(o);
            var cmd = table.Insert(o);
            if (timeToLive.HasValue)
            {
                cmd.SetTTL(Math.Max(1, (int)timeToLive.Value.TotalSeconds));
            }
            var result = await cmd.ExecuteAsync().ConfigureAwait(false);
            if (result.Info.AchievedConsistency.HasFlag(ConsistencyLevel.Any)) return new OperationResult<Guid>(true, id);
            return new OperationResult<Guid>(false, id, Severity.Error, "Object already exists");
        }

        private async Task<IEnumerable<OperationResult<Guid>>> updateAsync(IReadOnlyCollection<T> objCollection, TimeSpan? timeToLive)
        {
            var table = new Table<T>(_session.Value);
            var tasks = objCollection.Select(o => updateSingleAsync(o, table, timeToLive))
                                     .ToCollection();
            await Task.WhenAll(tasks).ConfigureAwait(false);
            return tasks.Select(t => t.Result);
        }

        private async Task<OperationResult<Guid>> updateSingleAsync(T o, Table<T> table, TimeSpan? timeToLive)
        {
            var id = _typeDescriptor.GetId(o);

            T existingEntity = await GetByIdAsync(id).ConfigureAwait(false);

            if (existingEntity == null)
            {
                return new OperationResult<Guid>(false, id, Severity.Warning, "Entity does not exist");
            }

            var cmd = table.Insert(o);
            if (timeToLive.HasValue)
            {
                cmd.SetTTL(Math.Max(1, (int)timeToLive.Value.TotalSeconds));
            }

            await cmd.ExecuteAsync().ConfigureAwait(false);

            return new OperationResult<Guid>(true, id);
        }

        #endregion

        private void initialize()
        {
            var helper = new CassandraHelper(_typeDescriptor);
            defineMap(helper);
        }

        private void ensureInitialized()
        {
            if (!_isInitialized)
            {
                Debug.WriteLine("Before initialize sync");
                lock (_initializeSyncRoot)
                {
                    if (!_isInitialized)
                    {
                        Debug.WriteLine("Initializing...");
                        initialize();
                        _isInitialized = true;
                    }
                }
                Debug.WriteLine("After initialize sync");
            }
        }

        private ISession getCassandraSession()
        {
            var session = CassandraSessionPool.GetSession(Keyspace);
            return session;
        }

        private void createTableIfNotExist(ISession session)
        {
            if (!_isTableCreated)
            {
                Debug.WriteLine("Before create table sync");
                lock (_createTableSyncRoot)
                {
                    if (!_isTableCreated)
                    {
                        var existingTable = session.Cluster.Metadata.GetTable(session.Keyspace, typeof (T).Name.ToLowerInvariant());

                        if (existingTable == null)
                        {
                            Debug.WriteLine("Creating table " + typeof(T));
                            var table = new Table<T>(session);
                            table.CreateIfNotExists();
                            _isTableCreated = true;
                        }
                    }
                }
                Debug.WriteLine("After create table sync");
            }
        }

        private void defineMap(CassandraHelper helper)
        {
            var map = new Map<T>();

            foreach (var prop in _typeDescriptor.Properties)
            {
                if (helper.IsPropertyPartitionKey(prop.Name))
                {
                    map.PartitionKey(prop.Name);
                }
                else if (helper.IsPropertyClusteringKey(prop.Name))
                {
                    mapClusteringKey(helper, prop, map);
                }

                if (helper.IsPropertySecondaryIndex(prop.Name))
                {
                    mapSecondaryIndex(prop, map);
                }

                var propTypes = getAssociatedPropertyTypes(prop.PropertyType);
                if (propTypes.Any(propType => !_nativelySupportedTypes.Contains(propType)))
                {
                    mapAsJson(prop, map);
                }
            }

            MappingConfiguration.Global.Define(map);
        }

        private static void mapClusteringKey(CassandraHelper helper, PropertyInfo prop, Map<T> map)
        {
            var clusteringAttribs = helper.GetClusteringKeyAttribute(prop.Name);
            SortOrder sortOrder;
            switch (clusteringAttribs.SortOrder)
            {
                case IndexedAttribute.Order.Ascending:
                    sortOrder = SortOrder.Ascending;
                    break;
                case IndexedAttribute.Order.Descending:
                    sortOrder = SortOrder.Descending;
                    break;
                case IndexedAttribute.Order.Unspecified:
                    sortOrder = SortOrder.Unspecified;
                    break;
                default:
                    throw new IndexOutOfRangeException(clusteringAttribs.SortOrder.ToString());
            }

            map.ClusteringKey(Tuple.Create(prop.Name, sortOrder));
        }

        private static void mapSecondaryIndex(PropertyInfo prop, Map<T> map)
        {
            var expr = ExpressionUtils.GetPropertyGetterExpression<T, object>(prop.Name);
            map.Column(expr, cc => cc.WithSecondaryIndex());
        }

        private void mapAsJson(PropertyInfo prop, Map<T> map)
        {
            var expr = ExpressionUtils.GetPropertyGetterExpression<T, object>(prop.Name);
            map.Column(expr, cc => cc.WithDbType<string>());
        }

        private IEnumerable<Type> getAssociatedPropertyTypes(Type propertyType)
        {
            if (propertyType.IsGenericType && propertyType.GetInterface("IEnumerable`1") != null)
            {
                foreach (var arg in propertyType.GetGenericArguments())
                {
                    yield return arg;
                }
            }
            else
            {
                yield return propertyType.GetNullableUnderlyingType();
            }
        }
    }
}
