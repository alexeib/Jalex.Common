namespace Jalex.Infrastructure.Specifications
{
    /// <summary>
    /// Specification pattern
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISpecification<in T>
    {
        /// <summary>
        /// Returns true if the instance satisfies the given specification
        /// </summary>
        /// <param name="instance">The instance to check</param>
        /// <returns>True if the instance satisifies the given specification</returns>
        bool IsSatisfiedBy(T instance);
    }
}