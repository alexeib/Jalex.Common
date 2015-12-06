using System;

namespace Jalex.Infrastructure.Specifications
{
    public class NotSpecification<T> : ISpecification<T>
    {
        private readonly ISpecification<T> _original;

        public NotSpecification(ISpecification<T> original)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            _original = original;
        }

        #region Implementation of ISpecification<in T>

        /// <summary>
        /// Returns true if the instance satisfies the given specification
        /// </summary>
        /// <param name="instance">The instance to check</param>
        /// <returns>True if the instance satisifies the given specification</returns>
        public bool IsSatisfiedBy(T instance)
        {
            return !_original.IsSatisfiedBy(instance);
        }

        #endregion
    }
}
