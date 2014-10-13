using Jalex.Infrastructure.Utils;

namespace Jalex.Infrastructure.Specifications
{
    public class AndSpecification<T> : ISpecification<T>
    {
        private readonly ISpecification<T> _first;
        private readonly ISpecification<T> _second;

        public AndSpecification(ISpecification<T> first, ISpecification<T> second)
        {
            Guard.AgainstNull(first, "first");
            Guard.AgainstNull(second, "second");

            _first = first;
            _second = second;
        }

        #region Implementation of ISpecification<in T>

        /// <summary>
        /// Returns true if the instance satisfies the given specification
        /// </summary>
        /// <param name="instance">The instance to check</param>
        /// <returns>True if the instance satisifies the given specification</returns>
        public bool IsSatisfiedBy(T instance)
        {
            return _first.IsSatisfiedBy(instance) && _second.IsSatisfiedBy(instance);
        }

        #endregion
    }
}
