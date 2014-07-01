using Jalex.Infrastructure.Utils;

namespace Jalex.Infrastructure.Specifications
{
    public class ExclusiveOrSpecification<T> : ISpecification<T>
    {
        private readonly ISpecification<T> _first;
        private readonly ISpecification<T> _second;

        public ExclusiveOrSpecification(ISpecification<T> first, ISpecification<T> second)
        {
            ParameterChecker.CheckForNull(first, "first");
            ParameterChecker.CheckForNull(second, "second");

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
            if (_first.IsSatisfiedBy(instance))
            {
                return !_second.IsSatisfiedBy(instance);
            }
            return _second.IsSatisfiedBy(instance);
        }

        #endregion
    }
}
