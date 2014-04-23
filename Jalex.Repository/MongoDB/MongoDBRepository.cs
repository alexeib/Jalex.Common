﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Jalex.Infrastructure.Attributes;
using Jalex.Infrastructure.Extensions;
using Jalex.Infrastructure.Objects;
using Jalex.Infrastructure.Utils;
using Jalex.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;

namespace Jalex.Repository.MongoDB
{
    public class MongoDBRepository<T> : BaseMongoDBRepository, IQueryableRepository<T>
    {
        private const string _defaultCollectionPrefix = "mongo-collection-";
        
        // ReSharper disable once StaticFieldInGenericType
        protected static readonly ILogger _staticLogger = LogManager.GetCurrentClassLogger();

        // ReSharper disable once StaticFieldInGenericType
        private static readonly string _typeName;

        // ReSharper disable once StaticFieldInGenericType
        private static readonly IEnumerable<Tuple<IMongoIndexKeys, IMongoIndexOptions>> _indices;

        // ReSharper disable once StaticFieldInGenericType
        protected static readonly Func<T, string> _idGetter;
        protected static readonly Expression<Func<T, string>> _idGetterExpression;
        protected ILogger _instanceLogger;

        // ReSharper disable once StaticFieldInGenericType
        private static readonly string[] _idFieldNames = {"Id", "ID", "id"};
        private bool _indicesEnsured;        

        // NOTE: this is going to get called once for each distinct T
        static MongoDBRepository()
        {
            MongoSetup.EnsureInitialized();

            Type typeOfT = typeof (T);
            _typeName = typeOfT.Name;
            PropertyInfo[] classProps = typeOfT.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            bool isIdAutoGenerated;
            string idPropertyName = getIdPropertyName(classProps, out isIdAutoGenerated);

            _idGetterExpression = ExpressionProperties.GetPropertyGetterExpression<T, string>(idPropertyName);
            _idGetter = _idGetterExpression.Compile();

            // generate getters for ignored properties so we can map them properly
            IEnumerable<string> ignoredPropertyNames = getIgnoredPropertyNames(classProps);
            IEnumerable<Expression<Func<T, string>>> ignoredPropGetters =
                ignoredPropertyNames.Select(ExpressionProperties.GetPropertyGetterExpression<T, string>);

            registerConventions();
            // class map must be registered after convention
            registerClassMap(idPropertyName, isIdAutoGenerated, ignoredPropGetters);

            _indices = createIndices(classProps);
        }

        public ILogger Logger
        {
            get { return _instanceLogger ?? _staticLogger; }
            set { _instanceLogger = value; }
        }

        public string CollectionName { get; set; }

        public IEnumerable<T> GetByIds(IEnumerable<string> ids)
        {
            ParameterChecker.CheckForVoid(() => ids);

            string[] idsArr = ids.ToArray();

            MongoCollection<T> collection = getMongoCollection();

            IMongoQuery query = Query<T>.In(_idGetterExpression, idsArr);

            return collection.Find(query).SetLimit(idsArr.Length).ToArray();
        }

        public IEnumerable<T> Query(Func<T, bool> query)
        {
            MongoCollection<T> collection = getMongoCollection();
            return collection.AsQueryable().Where(query).ToArray();
        }

        public IEnumerable<OperationResult<string>> Create(IEnumerable<T> newObjects)
        {
            ParameterChecker.CheckForVoid(() => newObjects);

            T[] objectArr = newObjects.ToArray();

            MongoCollection<T> collection = getMongoCollection();

            try
            {
                collection.InsertBatch(objectArr);
            }
            catch (WriteConcernException wce)
            {
                Logger.ErrorException("Error when creating " + _typeName, wce);
                return objectArr.Select(r =>
                    new OperationResult<string>
                    {
                        Success = false,
                        Value = null,
                        Messages = new[]
                        {
                            new Message(Severity.Error,
                                string.Format("Failed to create {0} {1}", _typeName, r.ToString()))
                        }
                    }).ToArray();
            }

            return objectArr.Select(r => new OperationResult<string> {Success = true, Value = _idGetter(r)}).ToArray();
        }

        public OperationResult Update(T objectToUpdate)
        {
            ParameterChecker.CheckForVoid(() => objectToUpdate);
            ParameterChecker.CheckForNullOrEmpty(_idGetter(objectToUpdate), "objectToUpdate.Id");

            MongoCollection<T> collection = getMongoCollection();
            bool success;

            try
            {
                WriteConcernResult wcr = collection.Update(Query<T>.EQ(_idGetterExpression, _idGetter(objectToUpdate)),
                    Update<T>.Replace(objectToUpdate));
                success = wcr.DocumentsAffected > 0;
            }
            catch (WriteConcernException wce)
            {
                Logger.ErrorException("Error when updating " + _typeName, wce);
                return new OperationResult
                {
                    Success = false,
                    Messages = new[]
                    {
                        new Message(Severity.Error, string.Format("Failed to update {0} {1}", _typeName, objectToUpdate))
                    }
                };
            }

            var result = new OperationResult {Success = success};

            if (!success)
            {
                result.Messages = new[]
                {
                    new Message(Severity.Warning,
                        string.Format("Could not update {0} because it was not found", objectToUpdate))
                };
            }

            return result;
        }

        public IEnumerable<OperationResult> Delete(IEnumerable<string> ids)
        {
            ParameterChecker.CheckForVoid(() => ids);

            string[] idsArr = ids.ToArray();

            MongoCollection<T> collection = getMongoCollection();
            bool success;

            try
            {
                WriteConcernResult wcr = collection.Remove(Query<T>.In(_idGetterExpression, idsArr));
                success = wcr.DocumentsAffected > 0;
            }
            catch (WriteConcernException wce)
            {
                Logger.ErrorException("Error when deleting " + _typeName, wce);
                return idsArr.Select(r =>
                    new OperationResult
                    {
                        Success = false,
                        Messages = new[]
                        {
                            new Message(Severity.Error,
                                string.Format("Failed to delete {0} {1}", _typeName,
                                    r.ToString(CultureInfo.InvariantCulture)))
                        }
                    }).ToArray();
            }

            // TODO: success may not be really true if we are trying to delete multiple things
            return idsArr.Select(i => new OperationResult {Success = success}).ToArray();
        }

        private static void registerClassMap(string idPropertyName, bool isIdAutoGenerated,
            IEnumerable<Expression<Func<T, string>>> ignoredPropGetters)
        {
            BsonClassMap.RegisterClassMap<T>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(idPropertyName));
                if (isIdAutoGenerated)
                {
                    cm.IdMemberMap.SetRepresentation(BsonType.ObjectId);
                    cm.IdMemberMap.SetIdGenerator(StringObjectIdGenerator.Instance);
                }

                foreach (var ignoredPropGetter in ignoredPropGetters)
                {
                    cm.UnmapProperty(ignoredPropGetter);
                }
            });
        }

        private static void registerConventions()
        {
            var entityConventionPack = new ConventionPack();

            entityConventionPack.AddMemberMapConvention("Enum as string", m =>
            {
                if (m.MemberType.GetNullableUnderlyingType().IsEnum)
                {
                    m.SetRepresentation(BsonType.String);
                }
            });

            ConventionRegistry.Register(typeof (T).FullName, entityConventionPack, t => typeof (T).IsAssignableFrom(t));
        }

        private static string getIdPropertyName(PropertyInfo[] classProps, out bool isAutoGenerated)
        {
            PropertyInfo idProperty;

            // try to get id propert through attribute annotation first
            var idPropertyAndAttribute = (from prop in classProps
                let idAttribute = (IdAttribute) prop.GetCustomAttributes(true).FirstOrDefault(a => a is IdAttribute)
                where idAttribute != null
                select new {Property = prop, IdAttribute = idAttribute}).FirstOrDefault();

            if (idPropertyAndAttribute != null)
            {
                idProperty = idPropertyAndAttribute.Property;
                isAutoGenerated = idPropertyAndAttribute.IdAttribute.IsAutoGenerated;
            }
            else
            {
                // if no attribute is present, try using convention
                idProperty = classProps.FirstOrDefault(m => _idFieldNames.Contains(m.Name));
                isAutoGenerated = true;
            }

            if (idProperty == null)
            {
                throw new RepositoryException("Id property not found (must be one of " +
                                              string.Join(", ", _idFieldNames) +
                                              "). Alternatively, set a IdAttribute on the key property.");
            }

            string idPropertyName = idProperty.Name;

            if (idProperty.PropertyType != typeof (string))
            {
                throw new RepositoryException("Id property " + idPropertyName + " must be of type string");
            }
            return idPropertyName;
        }

        private static IEnumerable<string> getIgnoredPropertyNames(IEnumerable<PropertyInfo> classProps)
        {
            return (from prop in classProps
                where Attribute.IsDefined(prop, typeof (IgnoreAttribute))
                select prop.Name);
        }

        private static IEnumerable<Tuple<IMongoIndexKeys, IMongoIndexOptions>> createIndices(
            IEnumerable<PropertyInfo> classProps)
        {
            var indexedProps = (from prop in classProps
                let indexedAttr = prop.GetCustomAttributes(true).FirstOrDefault(p => p is IndexedAttribute)
                where indexedAttr != null
                select new {PropertyName = prop.Name, IndexedAttribute = (IndexedAttribute) indexedAttr})
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
                        collection.EnsureIndex(index.Item1, index.Item2);
                    }
                }
                _indicesEnsured = true;
            }
        }

        protected MongoCollection<T> getMongoCollection()
        {
            string collectionName = CollectionName ??
                                    ConfigurationManager.AppSettings[_defaultCollectionPrefix + _typeName] ??
                                    string.Format("{0}s", _typeName);

            MongoDatabase db = getMongoDatabase();
            MongoCollection<T> collection = db.GetCollection<T>(collectionName);
            ensureIndices(collection);

            return collection;
        }
    }
}