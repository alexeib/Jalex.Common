namespace Jalex.Infrastructure.Specifications
{
    public class ConstantSpecification<T> : ISpecification<T>
    {
        private readonly bool _constantValue;

        public ConstantSpecification(bool constantValue)
        {
            _constantValue = constantValue;
        }

        #region Implementation of ISpecification<in T>

        public bool IsSatisfiedBy(T instance)
        {
            return _constantValue;
        }

        #endregion
    }
}
