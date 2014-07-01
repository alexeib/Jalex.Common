using System;

namespace Jalex.Infrastructure.Specifications
{
    public class PredicateSpecification<T> : ISpecification<T>
    {
        private readonly Predicate<T> _predicate;

        public PredicateSpecification(Predicate<T> predicate)
        {
            _predicate = predicate;
        }

        #region Implementation of ISpecification<in T>

        /// <summary>
        /// Returns true if the instance satisfies the given specification
        /// </summary>
        /// <param name="instance">The instance to check</param>
        /// <returns>True if the instance satisifies the given specification</returns>
        public bool IsSatisfiedBy(T instance)
        {
            return _predicate.Invoke(instance);
        }

        #endregion
    }
}
