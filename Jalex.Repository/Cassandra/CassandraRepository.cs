﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using Cassandra;
using Jalex.Infrastructure.Objects;
using Jalex.Infrastructure.Utils;
using Jalex.Repository.Cassandra.DataStax.Linq;
using Jalex.Repository.Exceptions;
using Jalex.Repository.Extensions;
using Jalex.Repository.Utils;

namespace Jalex.Repository.Cassandra
{
    public class CassandraRepository<T> : BaseRepository, IQueryableRepository<T> where T : class
    {
        private const string _defaultKeyspaceSettingNane = "cassandra-keyspace";

        private static readonly ReflectedTypeDescriptor<T> _typeDescriptor;

        public string Keyspace { get; set; }

        private readonly Context _context;
        private readonly ContextTable<T> _table;

        private readonly ICassandraIdProvider _idProvider;

        static CassandraRepository()
        {
            _typeDescriptor = new ReflectedTypeDescriptor<T>();
        }

        public CassandraRepository(ICassandraIdProvider idProvider)
        {
            _idProvider = idProvider;

            var session = getCassandraSession();
            _context = new Context(session);

            _table = _context.AddTable<T>();
            _context.CreateTablesIfNotExist();
        }

        #region Implementation of IReader<out T>

        public IEnumerable<T> GetByIds(IEnumerable<string> ids)
        {
            ParameterChecker.CheckForVoid(() => ids);

            var query = getCqlQueryForIds(ids);
            var results = query.Execute();

            return results;
        }

        #endregion

        #region Implementation of IDeleter<T>

        public IEnumerable<OperationResult> Delete(IEnumerable<string> ids)
        {
            ParameterChecker.CheckForVoid(() => ids);
            string[] idsArr = ids.ToArray();

            var existingEntities = GetByIds(idsArr);
            var existingIds = new HashSet<string>(existingEntities.Select(e => _typeDescriptor.GetId(e)));

            var cqlQuery = getCqlQueryForIds(existingIds);
            var deleteCommand = cqlQuery.Delete();

            try
            {
                deleteCommand.Execute();
            }
            catch (CqlArgumentException cae)
            {
                Logger.ErrorException("Error when deleting " + _typeDescriptor.TypeName, cae);
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

            var results = idsArr.Select(id => new OperationResult { Success = existingIds.Contains(id) }).ToArray();
            return results;
        }

        #endregion

        #region Implementation of IUpdater<in T>

        public OperationResult Update(T objectToUpdate)
        {
            ParameterChecker.CheckForVoid(() => objectToUpdate);
            
            string id = _typeDescriptor.GetId(objectToUpdate);
            
            ParameterChecker.CheckForVoid(() => id);

            T entity = this.GetById(id);

            OperationResult result;

            if (entity == null)
            {
                result = new OperationResult(false, new Message(Severity.Warning, string.Format("Could not update {0} because it was not found", objectToUpdate)));
            }
            else
            {
                _table.Attach(entity, EntityUpdateMode.ModifiedOnly);

                var mapper = EmitMapper.ObjectMapperManager.DefaultInstance.GetMapper<T, T>();
                mapper.Map(objectToUpdate, entity);                

                try
                {
                    _context.SaveChanges();
                }
                catch (CqlArgumentException cae)
                {
                    Logger.ErrorException("Error when updating " + _typeDescriptor.TypeName, cae);
                    return new OperationResult
                    {
                        Success = false,
                        Messages = new[]
                    {
                        new Message(Severity.Error, string.Format("Failed to update {0} {1}", _typeDescriptor.TypeName, objectToUpdate))
                    }
                    };
                }                
   
                return new OperationResult(true);
            }            

            return result;
        }

        #endregion

        #region Implementation of IInserter<in T>

        public IEnumerable<OperationResult<string>> Create(IEnumerable<T> newObjects)
        {
            ParameterChecker.CheckForVoid(() => newObjects);

            var newObjArr = newObjects as T[] ?? newObjects.ToArray();
            HashSet<string> existingIds = new HashSet<string>();

            foreach (var newObj in newObjArr)
            {
                string id = _typeDescriptor.GetId(newObj);

                if (!string.IsNullOrEmpty(id))
                {
                    if (!existingIds.Add(id))
                    {
                        throw new DuplicateIdException("Attempting to create multiple objects with id " + id + " is not allowed");
                    }
                }

                if (_typeDescriptor.IsIdAutoGenerated)
                {
                    checkOrGenerateIdForEntity(id, newObj);
                }
            }

            if (existingIds.Count > 0)
            {
                var existingEntities = GetByIds(existingIds);
                existingIds = new HashSet<string>(existingEntities.Select(e => _typeDescriptor.GetId(e)));
            }

            List<OperationResult<string>> results = new List<OperationResult<string>>(newObjArr.Length);

            try
            {
                foreach (var newObj in newObjArr)
                {
                    string id = _typeDescriptor.GetId(newObj);
                    if (existingIds.Contains(id))
                    {
                        string message = string.Format("Failed to create {0} with ID {1} because it already exists.", _typeDescriptor.TypeName, id);

                        Logger.Info(message);

                        var failResult = new OperationResult<string>
                        {
                            Success = false,
                            Value = null,
                            Messages = new[]
                                {
                                    new Message(Severity.Error, message)
                                }
                        };
                        results.Add(failResult);
                    }
                    else
                    {
                        var successResult = new OperationResult<string> { Success = true, Value = id };
                        results.Add(successResult);
                        _table.AddNew(newObj);
                    }
                }

                _context.SaveChanges();
            }
            catch (CqlArgumentException cae)
            {
                Logger.ErrorException("Error when creating " + _typeDescriptor.TypeName, cae);
                return newObjArr.Select(r =>
                    new OperationResult<string>
                    {
                        Success = false,
                        Value = null,
                        Messages = new[]
                        {
                            new Message(Severity.Error,
                                string.Format("Failed to create {0} {1}", _typeDescriptor.TypeName, r.ToString()))
                        }
                    }).ToArray();
            }


            return results;
        }

        #endregion

        #region Implementation of IQueryable<out T>

        public IEnumerable<T> Query(Expression<Func<T, bool>> query)
        {
            var queryCommand = _table.Where(query);
            var results = queryCommand.Execute();
            return results;
        }

        #endregion

        protected Session getCassandraSession()
        {
            string keyspace = Keyspace ?? ConfigurationManager.AppSettings[_defaultKeyspaceSettingNane];

            if (string.IsNullOrEmpty(keyspace))
            {
                throw new InvalidOperationException("Must specify Cassandra keyspace by providing a value in the Keyspace property or populating the " + _defaultKeyspaceSettingNane + " app setting");
            }

            var session = CassandraSessionPool.GetSessionForKeyspace(keyspace);

            return session;
        }

        private CqlQuery<T> getCqlQueryForIds(IEnumerable<string> ids)
        {
            var idsExpr = Expression.Constant(ids);
            var call = Expression.Call(typeof(Enumerable), "Contains", new[] { typeof(string) }, idsExpr, _typeDescriptor.IdPropertyExpression);
            var lambda = Expression.Lambda<Func<T, bool>>(call, _typeDescriptor.TypeParameter);

            var results = _table.Where(lambda);
            return results;
        }

        private void checkOrGenerateIdForEntity(string id, T newObj)
        {
            if (string.IsNullOrEmpty(id))
            {
                string generatedId = _idProvider.GenerateNewId();
                _typeDescriptor.SetId(newObj, generatedId);
            }
            else if (!_idProvider.IsIdValid(id))
            {
                throw new IdFormatException(id + " is not a valid identifier (validated using " + _idProvider.GetType().Name + ")");
            }
        }
    }
}