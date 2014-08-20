using Jalex.Infrastructure.Objects;

namespace Jalex.Infrastructure.ServiceComposition
{
    public class ComposableServiceNotFoundException<T>: JalexException
    {
        public ComposableServiceNotFoundException() 
            : base(string.Format("A composable service for type {0} was not found", typeof(T)))
        {
        }
    }
}
