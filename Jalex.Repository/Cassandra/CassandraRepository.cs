using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using Cassandra;
using Cassandra.Data.Linq;
using Cassandra.Mapping;
using Jalex.Infrastructure.Expressions;
using Jalex.Infrastructure.Extensions;
using Jalex.Infrastructure.Objects;
using Jalex.Infrastructure.ReflectedTypeDescriptor;
using Jalex.Infrastructure.Repository;
using Jalex.Infrastructure.Utils;
using Jalex.Repository.IdProviders;
using Magnum.Extensions;

namespace Jalex.Repository.Cassandra
{
    public class CassandraRepository<T> : BaseRepository<T>, IQueryableRepository<T> where T : class
    {
        private const string _defaultKeyspaceSettingNane = "cassandra-keyspace";

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
                                             var session = getCassandraSession();
                                             createTableIfNotExist(session);
                                             return session;
                                         });

        }

        #region Implementation of IReader<out T>

        public bool TryGetById(Guid id, out T obj)
        {
            var table = new Table<T>(_session.Value);

            var idEquals = getExpressionForIdEquality(id);

            var result = table.Where(idEquals)
                              .Execute()
                              .Take(2)
                              .ToCollection();

            obj = result.FirstOrDefault();
            return obj != default(T);
        }

        public IEnumerable<T> GetAll()
        {
            var table = new Table<T>(_session.Value);
            var results = table.Execute();
            return results;
        }

        #endregion

        #region Implementation of IDeleter<T>

        public OperationResult Delete(Guid id)
        {

            T existingEntity;
            var exists = TryGetById(id, out existingEntity);

            if (!exists)
            {
                return new OperationResult(false);
            }

            var table = new Table<T>(_session.Value);
            var idEquals = getExpressionForIdEquality(id);
            var statement = table.Where(idEquals)
                                 .Delete();

            try
            {
                statement.Execute();
                return new OperationResult(true);
            }
            catch (CqlArgumentException cae)
            {
                Logger.ErrorException(cae, "Error when deleting " + _typeDescriptor.TypeName);
                return new OperationResult(false, Severity.Error, string.Format("Failed to delete {0} {1}", _typeDescriptor.TypeName, id));
            }
        }

        public OperationResult DeleteWhere(Expression<Func<T, bool>> expression)
        {
            var table = new Table<T>(_session.Value);
            var statement = table.Where(expression)
                                 .Delete();

            try
            {
                statement.Execute();
                return new OperationResult(true);
            }
            catch (CqlArgumentException cae)
            {
                Logger.ErrorException(cae, "Error when deleting " + _typeDescriptor.TypeName);
                return new OperationResult(false, Severity.Error, string.Format("Failed to delete {0} by expression {1}", _typeDescriptor.TypeName, expression));
            }
        }

        #endregion

        #region Implementation of IQueryable<out T>

        public IEnumerable<T> Query(Expression<Func<T, bool>> query)
        {
            var table = new Table<T>(_session.Value);

            var queryCommand = table.Where(query);
            var results = queryCommand.Execute();
            return results;
        }

        /// <summary>
        /// Returns the first object stored in the repository that satisfies a given query, or default value for T if no such object is found
        /// </summary>
        /// <param name="query">The query that must be satisfied</param>
        /// <returns>The object in the repository that satisfies the query or the default value for T if no such object is found</returns>
        public T FirstOrDefault(Expression<Func<T, bool>> query)
        {
            var table = new Table<T>(_session.Value);

            var firstOrDefaultCommand = table.FirstOrDefault(query);
            var result = firstOrDefaultCommand.Execute();
            return result;
        }

        #endregion

        #region Implementation of IWriter<in T>

        /// <summary>
        /// Saves an object
        /// </summary>
        /// <param name="obj">object to save</param>
        /// <param name="writeMode">writing mode. inserting an object that exists or updating an object that does not exist will fail. Defaults to upsert</param>
        /// <returns>Operation result with id of the new object in order of the objects given to this function</returns>
        public OperationResult<Guid> Save(T obj, WriteMode writeMode = WriteMode.Upsert)
        {
            return SaveMany(new[] { obj }, writeMode).Single();
        }

        /// <summary>
        /// Saves objects
        /// </summary>
        /// <param name="objects">objects to save</param>
        /// <param name="writeMode">writing mode. inserting an object that exists or updating an object that does not exist will fail. Defaults to upsert</param>
        /// <returns>Operation result with ids of the new objects in order of the objects given to this function</returns>
        public IEnumerable<OperationResult<Guid>> SaveMany(IEnumerable<T> objects, WriteMode writeMode)
        {
            Guard.AgainstNull(objects, "objects");

            var objCollection = objects.ToCollection();
            ensureObjectIds(writeMode, objCollection);

            try
            {
                switch (writeMode)
                {
                    case WriteMode.Upsert:
                        return insertBatch(objCollection);
                    case WriteMode.Insert:
                        return insertIfNotExists(objCollection);
                    case WriteMode.Update:
                        return update(objCollection);
                    default:
                        throw new ArgumentOutOfRangeException("writeMode", writeMode, "not supported");
                }
            }
            catch (CqlArgumentException cae)
            {
                Logger.ErrorException(cae, "Error when saving " + _typeDescriptor.TypeName);
                return objCollection.Select(r =>
                                            new OperationResult<Guid>(
                                                false,
                                                _typeDescriptor.GetId(r),
                                                Severity.Error,
                                                string.Format("Failed to create {0} {1}", _typeDescriptor.TypeName, r.ToString())))
                                    .ToArray();
            }
        }

        private IEnumerable<OperationResult<Guid>> insertBatch(IReadOnlyCollection<T> objCollection)
        {
            var table = new Table<T>(_session.Value);
            BatchStatement batch = new BatchStatement();
            objCollection.Each(o => batch.Add(table.Insert(o)));
            _session.Value.Execute(batch);
            return objCollection.Select(o => new OperationResult<Guid>(true, _typeDescriptor.GetId(o)));
        }

        private IEnumerable<OperationResult<Guid>> insertIfNotExists(IReadOnlyCollection<T> objCollection)
        {
            var table = new Table<T>(_session.Value);
            return objCollection.Select(o =>
                                        {
                                            var id = _typeDescriptor.GetId(o);
                                            var result = table.Insert(o)
                                                              .IfNotExists()
                                                              .Execute();
                                            if (result.Applied)
                                            {
                                                return new OperationResult<Guid>(true, id);
                                            }
                                            return new OperationResult<Guid>(false, id, Severity.Error, "Object already exists");
                                        })
                                .ToCollection();
        }

        private IEnumerable<OperationResult<Guid>> update(IReadOnlyCollection<T> objCollection)
        {
            var table = new Table<T>(_session.Value);
            return objCollection.Select(o =>
                                        {
                                            var id = _typeDescriptor.GetId(o);

                                            T existingEntity;
                                            var exists = TryGetById(id, out existingEntity);

                                            if (!exists)
                                            {
                                                return new OperationResult<Guid>(false, id, Severity.Warning, "Entity does not exist");
                                            }

                                            table.Insert(o)
                                                 .Execute();

                                            return new OperationResult<Guid>(true, id);
                                        })
                                .ToCollection();
        }

        #endregion

        #region Overrides of BaseRepository<T>

        protected override void Initialize()
        {
            base.Initialize();

            var helper = new CassandraHelper(_typeDescriptor);

            var map = new Map<T>();

            foreach (var prop in _typeDescriptor.Properties)
            {
                if (helper.IsPropertyPartitionKey(prop.Name))
                {
                    map.PartitionKey(prop.Name);
                }
                else if (helper.IsPropertyClusteringKey(prop.Name))
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

                if (helper.IsPropertySecondaryIndex(prop.Name))
                {
                    var expr = ExpressionUtils.GetPropertyGetterExpression<T, object>(prop.Name);
                    map.Column(expr, cc => cc.WithSecondaryIndex());
                }
            }

            MappingConfiguration.Global.Define(map);
        }

        #endregion


        private ISession getCassandraSession()
        {
            string keyspace = Keyspace ?? ConfigurationManager.AppSettings[_defaultKeyspaceSettingNane];

            if (string.IsNullOrEmpty(keyspace))
            {
                throw new InvalidOperationException("Must specify Cassandra keyspace by providing a value in the Keyspace property or populating the " + _defaultKeyspaceSettingNane +
                                                    " app setting");
            }

            var session = CassandraSessionPool.GetSessionForKeyspace(keyspace);

            return session;
        }

        private void createTableIfNotExist(ISession session)
        {
            var table = new Table<T>(session);
            table.CreateIfNotExists();
        }

        private Expression<Func<T, bool>> getExpressionForIdEquality(Guid id)
        {
            var paramExpr = Expression.Parameter(typeof(T));
            var idValueExpr = Expression.Constant(id);
            var idEqualsValueExpr = Expression.Equal(_typeDescriptor.IdPropertyExpression, idValueExpr);
            var lambda = Expression.Lambda<Func<T, bool>>(idEqualsValueExpr, paramExpr);
            return lambda;
        }
    }
}
