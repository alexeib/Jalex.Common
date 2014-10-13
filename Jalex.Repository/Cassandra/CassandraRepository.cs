using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Cassandra;
using Jalex.Infrastructure.Extensions;
using Jalex.Infrastructure.Objects;
using Jalex.Infrastructure.ReflectedTypeDescriptor;
using Jalex.Infrastructure.Repository;
using Jalex.Infrastructure.Utils;
using Jalex.Repository.Cassandra.DataStax.Linq;
using Jalex.Repository.IdProviders;
using Magnum;
using Guard = Jalex.Infrastructure.Utils.Guard;

namespace Jalex.Repository.Cassandra
{
    public class CassandraRepository<T> : BaseRepository<T>, IQueryableRepository<T> where T : class
    {
        private const string _defaultKeyspaceSettingNane = "cassandra-keyspace";

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public string Keyspace { get; set; }

        private readonly Session _session;

        public CassandraRepository(
            IIdProvider idProvider,
            IReflectedTypeDescriptorProvider typeDescriptorProvider)
            : base(idProvider, typeDescriptorProvider)
        {
            _session = getCassandraSession();

            createTableIfNotExist();
        }

        #region Implementation of IReader<out T>

        public bool TryGetById(string id, out T obj)
        {
            Magnum.Guard.AgainstNull(id, "id");

            var context = new Context(_session);
            var table = context.AddTable<T>();

            var query = getCqlQueryForSingleId(id, table);
            var resultArr = query.Execute().ToArrayEfficient();

            if (resultArr.Length == 1)
            {
                obj = resultArr[0];
                return true;
            }
            if (resultArr.Length == 0)
            {
                obj = default(T);
                return false;
            }

            throw new InvalidDataException(string.Format("multiple items with id {0} were retrieved.", id));
        }

        public IEnumerable<T> GetAll()
        {
            var context = new Context(_session);
            var table = context.AddTable<T>();
            var results = table.Execute();
            return results;
        }

        #endregion

        #region Implementation of IDeleter<T>

        public OperationResult Delete(string id)
        {
            T existingEntity;
            var exists = TryGetById(id, out existingEntity);

            if (!exists)
            {
                return new OperationResult(false);
            }

            var context = new Context(_session);
            var table = context.AddTable<T>();

            table.Delete(existingEntity);

            try
            {
                context.SaveChanges();
            }
            catch (CqlArgumentException cae)
            {
                Logger.ErrorException(cae, "Error when deleting " + _typeDescriptor.TypeName);
                return new OperationResult(false, Severity.Error, string.Format("Failed to delete {0} {1}", _typeDescriptor.TypeName, id));
            }

            var results = new OperationResult(true);
            return results;
        }

        #endregion

        #region Implementation of IQueryable<out T>

        public IEnumerable<T> Query(Expression<Func<T, bool>> query)
        {
            var context = new Context(_session);
            var table = context.AddTable<T>();

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
            var context = new Context(_session);
            var table = context.AddTable<T>();

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
        public OperationResult<string> Save(T obj, WriteMode writeMode = WriteMode.Upsert)
        {
            return SaveMany(new[] {obj}, writeMode).Single();
        }

        /// <summary>
        /// Saves objects
        /// </summary>
        /// <param name="objects">objects to save</param>
        /// <param name="writeMode">writing mode. inserting an object that exists or updating an object that does not exist will fail. Defaults to upsert</param>
        /// <returns>Operation result with ids of the new objects in order of the objects given to this function</returns>
        public IEnumerable<OperationResult<string>> SaveMany(IEnumerable<T> objects, WriteMode writeMode)
        {
            Guard.AgainstNull(objects, "objects");

            var context = new Context(_session);
            var table = context.AddTable<T>();
            var objectArr = objects as T[] ?? objects.ToArray();

            var results = createResults(writeMode, objectArr, doesObjectWithIdExist, obj => writeObject(obj, table));

            try
            {
                context.SaveChanges();
            }
            catch (CqlArgumentException cae)
            {
                Logger.ErrorException(cae, "Error when saving " + _typeDescriptor.TypeName);
                return objectArr.Select(r =>
                                        new OperationResult<string>(
                                            false,
                                            null,
                                            Severity.Error,
                                            string.Format("Failed to create {0} {1}", _typeDescriptor.TypeName, r.ToString())))
                                .ToArray();
            }


            return results;
        }        

        #endregion

        private bool doesObjectWithIdExist(string id)
        {
            T dummy;
            return TryGetById(id, out dummy);
        }

        private bool writeObject(T obj, ContextTable<T> table)
        {
            table.AddNew(obj);
            return true;
        }

        private Session getCassandraSession()
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

        private void createTableIfNotExist()
        {
            var context = new Context(_session);
            context.AddTable<T>();
            context.CreateTablesIfNotExist();
        }

        private CqlQuery<T> getCqlQueryForSingleId(string id, ContextTable<T> table)
        {
            var paramExpr = Expression.Parameter(typeof (T));
            var idValueExpr = Expression.Constant(id);
            var idEqualsValueExpr = Expression.Equal(_typeDescriptor.IdPropertyExpression, idValueExpr);
            var lambda = Expression.Lambda<Func<T, bool>>(idEqualsValueExpr, paramExpr);

            var results = table.Where(lambda);
            return results;
        }
    }
}
