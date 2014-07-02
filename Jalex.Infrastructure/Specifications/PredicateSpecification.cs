using System;

namespace Jalex.Infrastructure.Specifications
{
    public class PredicateSpecification<T> : ISpecification<T>
    {
        private readonly Func<T, bool> _predicate;

        public PredicateSpecification(Func<T, bool> predicate)
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
            return _predicate(instance);
        }

        #endregion
    }
}
