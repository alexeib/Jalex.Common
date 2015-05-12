using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EmitMapper;
using Jalex.Infrastructure.Expressions;
using Jalex.Infrastructure.Extensions;
using Jalex.Infrastructure.Logging;
using Jalex.Infrastructure.Objects;
using Jalex.Infrastructure.ReflectedTypeDescriptor;
using Jalex.Infrastructure.Repository;
using Guard = Magnum.Guard;

namespace Jalex.Services.Repository
{
    public class MappingResponsibility<TClass, TEntity> : IQueryableRepository<TClass> 
        where TClass : class
        where TEntity : class
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

        public async Task<TClass> GetByIdAsync(Guid id)
        {
            TEntity entity = await _entityRepository.GetByIdAsync(id).ConfigureAwait(false);

            TClass item = null;

            if (entity != null)
            {
                item = _entityToClassMapper.Map(entity);
            }

            return item;
        }

        public async Task<IEnumerable<TClass>> GetAllAsync()
        {
            var entities = await _entityRepository.GetAllAsync().ConfigureAwait(false);
            var classes = entities.Select(_entityToClassMapper.Map).ToCollection();
            return classes;
        }

        #endregion

        #region Implementation of IDeleter<TClass>

        public Task<OperationResult> DeleteAsync(Guid id)
        {
            var result = _entityRepository.DeleteAsync(id);
            return result;
        }

        /// <summary>
        /// Deletes all items that match a given expression
        /// </summary>
        /// <param name="expression">The expression to match</param>
        /// <returns>Whether the operation executed successfully or not</returns>
        public Task<OperationResult> DeleteWhereAsync(Expression<Func<TClass, bool>> expression)
        {
            var entityQuery = ExpressionUtils.ChangeType<TClass, TEntity, bool>(expression, _reflectedTypeDescriptorProvider);

            var result = _entityRepository.DeleteWhereAsync(entityQuery);
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

        #region Implementation of IQueryableReader<TClass>

        public async Task<IEnumerable<TClass>> QueryAsync(Expression<Func<TClass, bool>> query)
        {
            var entityQuery = ExpressionUtils.ChangeType<TClass, TEntity, bool>(query, _reflectedTypeDescriptorProvider);

            var entities = await _entityRepository.QueryAsync(entityQuery).ConfigureAwait(false);
            var classes = entities.Select(_entityToClassMapper.Map).ToCollection();
            return classes;
        }

        public async Task<IEnumerable<TProjection>> ProjectAsync<TProjection>(Expression<Func<TClass, TProjection>> projection, Expression<Func<TClass, bool>> query)
        {
            var entityQuery = ExpressionUtils.ChangeType<TClass, TEntity, bool>(query, _reflectedTypeDescriptorProvider);
            var entityProjection = ExpressionUtils.ChangeType<TClass, TEntity, TProjection>(projection, _reflectedTypeDescriptorProvider);

            var projections = await _entityRepository.ProjectAsync(entityProjection, entityQuery)
                                                     .ConfigureAwait(false);
            return projections.ToCollection();
        }

        /// <summary>
        /// Returns the first object stored in the repository that satisfies a given query, or default value for T if no such object is found
        /// </summary>
        /// <param name="query">The query that must be satisfied</param>
        /// <returns>The object in the repository that satisfies the query or the default value for T if no such object is found</returns>
        public async Task<TClass> FirstOrDefaultAsync(Expression<Func<TClass, bool>> query)
        {
            var entityQuery = ExpressionUtils.ChangeType<TClass, TEntity, bool>(query, _reflectedTypeDescriptorProvider);

            var entity = await _entityRepository.FirstOrDefaultAsync(entityQuery).ConfigureAwait(false);
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
        public virtual async Task<OperationResult<Guid>> SaveAsync(TClass obj, WriteMode writeMode)
        {
            var entity = _classToEntityMapper.Map(obj);
            var result = await _entityRepository.SaveAsync(entity, writeMode).ConfigureAwait(false);

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
        public virtual async Task<IEnumerable<OperationResult<Guid>>> SaveManyAsync(IEnumerable<TClass> objects, WriteMode writeMode)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            Guard.AgainstNull(objects);

            // ReSharper disable once PossibleMultipleEnumeration
            var objArray = objects.ToArrayEfficient();

            var entities = objArray.Select(_classToEntityMapper.Map).ToArray();
            var results = await _entityRepository.SaveManyAsync(entities, writeMode).ConfigureAwait(false);

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
