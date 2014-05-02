using System.Collections.Generic;
using System.Linq;
using EmitMapper;
using Jalex.Infrastructure.Logging;
using Jalex.Infrastructure.Objects;
using Jalex.Infrastructure.Repository;
using Jalex.Infrastructure.Utils;
using Jalex.Logging;

namespace Jalex.Repository
{
    public class MappingSimpleRepository<TClass, TEntity> : ISimpleRepository<TClass>
    {
        private readonly ISimpleRepository<TEntity> _entityRepository;
        private readonly ObjectsMapper<TClass, TEntity> _classToEntityMapper;
        private readonly ObjectsMapper<TEntity, TClass> _entityToClassMapper;

        public MappingSimpleRepository(
            ISimpleRepository<TEntity> entityRepository,
            ObjectsMapper<TClass, TEntity> classToEntityMapper,
            ObjectsMapper<TEntity, TClass> entityToClassMapper)
        {
            _entityRepository = entityRepository;

            _classToEntityMapper = classToEntityMapper;
            _entityToClassMapper = entityToClassMapper;

        }

        #region Implementation of IReader<out TClass>

        public IEnumerable<TClass> GetByIds(IEnumerable<string> ids)
        {
            var entities = _entityRepository.GetByIds(ids);
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

        public OperationResult Update(TClass objectToUpdate)
        {
            var entity = _classToEntityMapper.Map(objectToUpdate);
            var result = _entityRepository.Update(entity);
            return result;
        }

        #endregion

        #region Implementation of IInserter<in TClass>

        public IEnumerable<OperationResult<string>> Create(IEnumerable<TClass> newObjects)
        {
            ParameterChecker.CheckForVoid(() => newObjects);

            var stockArray = newObjects.ToArray();

            var entities = stockArray.Select(_classToEntityMapper.Map).ToArray();
            var results = _entityRepository.Create(entities);

            for (int i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                var stock = stockArray[i];

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
    }
}
