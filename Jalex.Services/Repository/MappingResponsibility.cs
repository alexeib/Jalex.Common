using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using EmitMapper;
using Jalex.Infrastructure.Expressions;
using Jalex.Infrastructure.Extensions;
using Jalex.Infrastructure.Logging;
using Jalex.Infrastructure.Objects;
using Jalex.Infrastructure.ReflectedTypeDescriptor;
using Jalex.Infrastructure.Repository;
using Jalex.Infrastructure.Utils;
using Magnum;
using Guard = Magnum.Guard;

namespace Jalex.Services.Repository
{
    public class MappingResponsibility<TClass, TEntity> : IQueryableRepository<TClass>
    {
        private readonly IQueryableRepository<TEntity> _entityRepository;
        private readonly ObjectsMapper<TClass, TEntity> _classToEntityMapper;
        private readonly ObjectsMapper<TEntity, TClass> _entityToClassMapper;
        private readonly IReflectedTypeDescriptorProvider _reflectedTypeDescriptorProvider;

        public MappingResponsibility(
            IQueryableRepository<TEntity> entityRepository,
            ObjectsMapper<TClass, TEntity> classToEntityMapper,
            ObjectsMapper<TEntity, TClass> entityToClassMapper,
            IReflectedTypeDescriptorProvider reflectedTypeDescriptorProvider)
        {
            _entityRepository = entityRepository;

            _classToEntityMapper = classToEntityMapper;
            _entityToClassMapper = entityToClassMapper;
            _reflectedTypeDescriptorProvider = reflectedTypeDescriptorProvider;

        }

        #region Implementation of IReader<out TClass>

        public bool TryGetById(Guid id, out TClass item)
        {
            TEntity entity;
            bool success = _entityRepository.TryGetById(id, out entity);

            if (success)
            {
                item = _entityToClassMapper.Map(entity);
                return true;
            }

            item = default(TClass);
            return false;
        }

        public IEnumerable<TClass> GetAll()
        {
            var entities = _entityRepository.GetAll();
            var classes = entities.Select(_entityToClassMapper.Map).ToArray();
            return classes;
        }

        #endregion

        #region Implementation of IDeleter<TClass>

        public OperationResult Delete(Guid id)
        {
            var result = _entityRepository.Delete(id);
            return result;
        }

        #endregion

        #region Implementation of IInjectableLogger

        public ILogger Logger
        {
            get { return _entityRepository.Logger; }
            set { _entityRepository.Logger = value; }
        }

        #endregion

        #region Implementation of IQueryable<TClass>

        public IEnumerable<TClass> Query(Expression<Func<TClass, bool>> query)
        {
            var entityQuery = ExpressionUtils.ChangeType<TClass, TEntity, bool>(query, _reflectedTypeDescriptorProvider);

            var entities = _entityRepository.Query(entityQuery);
            var classes = entities.Select(_entityToClassMapper.Map).ToArray();
            return classes;
        }

        /// <summary>
        /// Returns the first object stored in the repository that satisfies a given query, or default value for T if no such object is found
        /// </summary>
        /// <param name="query">The query that must be satisfied</param>
        /// <returns>The object in the repository that satisfies the query or the default value for T if no such object is found</returns>
        public TClass FirstOrDefault(Expression<Func<TClass, bool>> query)
        {
            var entityQuery = ExpressionUtils.ChangeType<TClass, TEntity, bool>(query, _reflectedTypeDescriptorProvider);

            var entity = _entityRepository.FirstOrDefault(entityQuery);
            var @class = _entityToClassMapper.Map(entity);
            return @class;
        }

        #endregion

        #region Implementation of IWriter<in TClass>

        /// <summary>
        /// Saves an object
        /// </summary>
        /// <param name="obj">object to save</param>
        /// <param name="writeMode">writing mode. inserting an object that exists or updating an object that does not exist will fail. Defaults to upsert</param>
        /// <returns>Operation result with id of the new object in order of the objects given to this function</returns>
        public virtual OperationResult<Guid> Save(TClass obj, WriteMode writeMode)
        {
            var entity = _classToEntityMapper.Map(obj);
            var result = _entityRepository.Save(entity, writeMode);

            if (result.Success)
            {
                var classDescriptor = _reflectedTypeDescriptorProvider.GetReflectedTypeDescriptor<TClass>();
                classDescriptor.SetId(obj, result.Value);
            }
            return result;
        }

        /// <summary>
        /// Saves objects
        /// </summary>
        /// <param name="objects">objects to save</param>
        /// <param name="writeMode">writing mode. inserting an object that exists or updating an object that does not exist will fail. Defaults to upsert</param>
        /// <returns>Operation result with ids of the new objects in order of the objects given to this function</returns>
        public virtual IEnumerable<OperationResult<Guid>> SaveMany(IEnumerable<TClass> objects, WriteMode writeMode)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            Guard.AgainstNull(objects);

            // ReSharper disable once PossibleMultipleEnumeration
            var objArray = objects.ToArrayEfficient();

            var entities = objArray.Select(_classToEntityMapper.Map).ToArray();
            var results = _entityRepository.SaveMany(entities, writeMode);

            var entityDescriptor = _reflectedTypeDescriptorProvider.GetReflectedTypeDescriptor<TEntity>();
            var classDescriptor = _reflectedTypeDescriptorProvider.GetReflectedTypeDescriptor<TClass>();

            for (int i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                var @class = objArray[i];

                var id = entityDescriptor.GetId(entity);
                classDescriptor.SetId(@class, id);
            }

            return results;
        }

        #endregion
    }
}
