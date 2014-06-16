using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using EmitMapper;
using Jalex.Infrastructure.Logging;
using Jalex.Infrastructure.Objects;
using Jalex.Infrastructure.ReflectedTypeDescriptor;
using Jalex.Infrastructure.Repository;
using Jalex.Infrastructure.Utils;

namespace Jalex.Repository
{
    public class MappingResponsibility<TClass, TEntity> : IQueryableRepository<TClass>
    {
        protected readonly IQueryableRepository<TEntity> _entityRepository;
        protected readonly ObjectsMapper<TClass, TEntity> _classToEntityMapper;
        protected readonly ObjectsMapper<TEntity, TClass> _entityToClassMapper;
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

        public IEnumerable<TClass> GetByIds(IEnumerable<string> ids)
        {
            var entities = _entityRepository.GetByIds(ids);
            var classes = entities.Select(_entityToClassMapper.Map).ToArray();
            return classes;
        }

        public IEnumerable<TClass> GetAll()
        {
            var entities = _entityRepository.GetAll();
            var classes = entities.Select(_entityToClassMapper.Map).ToArray();
            return classes;
        }

        #endregion

        #region Implementation of IDeleter<TClass>

        public IEnumerable<OperationResult> Delete(IEnumerable<string> ids)
        {
            var results = _entityRepository.Delete(ids);
            return results;
        }

        #endregion

        #region Implementation of IUpdater<in TClass>

        public IEnumerable<OperationResult> Update(IEnumerable<TClass> objectsToUpdate)
        {
            ParameterChecker.CheckForVoid(() => objectsToUpdate);

            var entities = objectsToUpdate.Select(_classToEntityMapper.Map).ToArray();
            var results = _entityRepository.Update(entities);
            return results;
        }

        #endregion

        #region Implementation of IInserter<in TClass>

        public IEnumerable<OperationResult<string>> Create(IEnumerable<TClass> newObjects)
        {
            ParameterChecker.CheckForVoid(() => newObjects);

            var objArray = newObjects.ToArray();

            var entities = objArray.Select(_classToEntityMapper.Map).ToArray();
            var results = _entityRepository.Create(entities);

            for (int i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                var stock = objArray[i];

                // ReSharper disable CompareNonConstrainedGenericWithNull
                if (entity != null && stock != null)
                // ReSharper restore CompareNonConstrainedGenericWithNull
                {
                    _entityToClassMapper.Map(entity, stock);
                }
            }

            return results;
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
    }
}
