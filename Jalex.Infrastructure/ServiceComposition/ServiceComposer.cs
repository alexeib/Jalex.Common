using System.Collections.Generic;
using System.Linq;
using Jalex.Infrastructure.Utils;

namespace Jalex.Infrastructure.ServiceComposition
{
    /// <summary>
    /// Represents an interface that provides service aggregation functionality
    /// </summary>
    public class ServiceComposer<T> : IComposableService<T>
    {
        private readonly IEnumerable<IComposableService<T>> _composableServices;

        public ServiceComposer(IEnumerable<IComposableService<T>> composableServices)
        {
            Guard.AgainstNull(composableServices, "composableServices");
            _composableServices = composableServices;
        }

        public virtual bool CanProcess(T item)
        {
            return _composableServices.Any(s => s.CanProcess(item));
        }

        public virtual void Process(T item)
        {
            var service = _composableServices.FirstOrDefault(s => s.CanProcess(item));
            if (service == null)
            {
                throw new ComposableServiceNotFoundException<T>();
            }
            service.Process(item);
        }
    }
}
