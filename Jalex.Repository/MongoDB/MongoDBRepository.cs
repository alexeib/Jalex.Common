﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Jalex.Infrastructure.Attributes;
using Jalex.Infrastructure.Objects;
using Jalex.Infrastructure.Utils;
using Jalex.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Jalex.Repository.MongoDB
{
    public class MongoDBRepository<T> : BaseMongoDBRepository, IRepository<T>
    {
        // ReSharper disable once StaticFieldInGenericType
        protected static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private const string _defaultCollectionPrefix = "mongo-collection-";

        // ReSharper disable once StaticFieldInGenericType
        private static readonly string _typeName;

        public string CollectionName { get; set; }

        // ReSharper disable once StaticFieldInGenericType
        private static readonly IEnumerable<Tuple<IMongoIndexKeys, IMongoIndexOptions>> _indices;

        private bool _indicesEnsured;

        // ReSharper disable once StaticFieldInGenericType
        protected static readonly Func<T, string> _idGetter;
        protected static readonly Expression<Func<T, string>> _idGetterExpression;

        // ReSharper disable once StaticFieldInGenericType
        private static readonly string[] _idFieldNames = { "Id", "ID", "id" };

        // NOTE: this is going to get called once for each distinct T
        static MongoDBRepository()
        {
            MongoSetup.EnsureInitialized();
            _typeName = typeof(T).Name;

            var classProps = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);

            var idProperty = classProps.FirstOrDefault(m => m.GetCustomAttributes(true).Any(n => n is KeyAttribute)) ??
                             classProps.FirstOrDefault(m => _idFieldNames.Contains(m.Name));

            if (idProperty == null)
            {
                throw new RepositoryException("Id property not found (must be one of " + string.Join(", ", _idFieldNames) + "). Alternatively, set a KeyAttribute on the key property.");
            }

            var idPropertyName = idProperty.Name;

            if (idProperty.PropertyType != typeof(string))
            {
                throw new RepositoryException("Id property " + idPropertyName + " must be of type string");
            }

            _idGetterExpression = ExpressionProperties.GetPropertyGetterExpression<T, string>(idPropertyName);
            _idGetter = _idGetterExpression.Compile();

            var entityConventionPack = new ConventionPack();

            entityConventionPack.AddMemberMapConvention("Id representation", m =>
            {
                if (m.MemberName == idPropertyName)
                {
                    m.SetRepresentation(BsonType.ObjectId);
                }
            });

            ConventionRegistry.Register(typeof(T).FullName, entityConventionPack, t => typeof(T).IsAssignableFrom(t));

            var indexedProps = (from prop in classProps
                                let indexedAttr = prop.GetCustomAttributes(true).FirstOrDefault(p => p is IndexedAttribute)
                                where indexedAttr != null
                                select new { PropertyName = prop.Name, IndexedAttribute = (IndexedAttribute)indexedAttr })
                               .ToArray();

            _indices = indexedProps
                            .Select(indexProp =>
                                Tuple.Create<IMongoIndexKeys, IMongoIndexOptions>(
                                    new IndexKeysBuilder().Ascending(indexProp.PropertyName),
                                    IndexOptions.SetUnique(indexProp.IndexedAttribute.IsUnique)))
                            .ToList();

        }

        protected virtual void ensureIndices(MongoCollection<T> collection)
        {
            lock (_indices)
            {
                foreach (var index in _indices)
                {
                    collection.EnsureIndex(index.Item1, index.Item2);
                }
            }
            _indicesEnsured = true;
        }

        protected MongoCollection<T> getMongoCollection()
        {
            string collectionName = CollectionName ?? ConfigurationManager.AppSettings[_defaultCollectionPrefix + _typeName];

            var db = getMongoDatabase();
            var collection = db.GetCollection<T>(collectionName);

            if (!_indicesEnsured)
            {
                ensureIndices(collection);
            }

            return collection;
        }

        public IEnumerable<T> GetByIds(IEnumerable<string> ids)
        {
            ParameterChecker.CheckForVoid(() => ids);

            var idsArr = ids.ToArray();

            var collection = getMongoCollection();

            var query = Query<T>.In(_idGetterExpression, idsArr);

            return collection.Find(query).SetLimit(idsArr.Length).ToArray();
        }

        public IEnumerable<OperationResult<string>> Create(IEnumerable<T> newObjects)
        {
            ParameterChecker.CheckForVoid(() => newObjects);

            var objectArr = newObjects.ToArray();

            var collection = getMongoCollection();

            try
            {
                collection.InsertBatch(objectArr);
            }
            catch (WriteConcernException wce)
            {
                _logger.ErrorException("Error when creating " + _typeName, wce);
                return objectArr.Select(r =>
                    new OperationResult<string>
                    {
                        Success = false,
                        Value = null,
                        Messages = new[]
                        {
                            new Message(Severity.Error, string.Format("Failed to create {0} {1}", _typeName, r.ToString()))
                        }
                    }).ToArray();
            }

            return objectArr.Select(r => new OperationResult<string> { Success = true, Value = _idGetter(r) }).ToArray();
        }

        public OperationResult Update(T objectToUpdate)
        {
            ParameterChecker.CheckForVoid(() => objectToUpdate);
            ParameterChecker.CheckForNullOrEmpty(_idGetter(objectToUpdate), "objectToUpdate.Id");

            var collection = getMongoCollection();
            bool success;

            try
            {

                var wcr = collection.Update(Query<T>.EQ(_idGetterExpression, _idGetter(objectToUpdate)), Update<T>.Replace(objectToUpdate));
                success = wcr.DocumentsAffected > 0;
            }
            catch (WriteConcernException wce)
            {
                _logger.ErrorException("Error when updating " + _typeName, wce);
                return new OperationResult
                {
                    Success = false,
                    Messages = new[]
                    {
                        new Message(Severity.Error, string.Format("Failed to update {0} {1}", _typeName, objectToUpdate))
                    }
                };
            }

            var result = new OperationResult { Success = success };

            if (!success)
            {
                result.Messages = new[] { new Message(Severity.Warning, string.Format("Could not update {0} because it was not found", objectToUpdate)) };
            }

            return result;
        }

        public IEnumerable<OperationResult> Delete(IEnumerable<string> ids)
        {
            ParameterChecker.CheckForVoid(() => ids);

            var idsArr = ids.ToArray();

            var collection = getMongoCollection();
            bool success;

            try
            {
                var wcr = collection.Remove(Query<T>.In(_idGetterExpression, idsArr));
                success = wcr.DocumentsAffected > 0;
            }
            catch (WriteConcernException wce)
            {
                _logger.ErrorException("Error when deleting " + _typeName, wce);
                return idsArr.Select(r =>
                    new OperationResult
                    {
                        Success = false,
                        Messages = new[]
                        {
                            new Message(Severity.Error, string.Format("Failed to delete {0} {1}", _typeName, r.ToString(CultureInfo.InvariantCulture)))
                        }
                    }).ToArray();
            }

            // TODO: success may not be really true if we are trying to delete multiple things
            return idsArr.Select(i => new OperationResult { Success = success }).ToArray();
        }
    }
}
