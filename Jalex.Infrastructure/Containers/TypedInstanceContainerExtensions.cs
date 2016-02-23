using System;
using System.Collections.Generic;

namespace Jalex.Infrastructure.Containers
{
    public static class TypedInstanceContainerExtensions
    {
        public static void AddMany<TInstance>(this TypedInstanceContainer<TInstance> container, IEnumerable<TInstance> instances)
            where TInstance : class
        {
            if (instances == null) throw new ArgumentNullException(nameof(instances));
            foreach (var instance in instances)
            {
                container.Add(instance);
            }
        }
    }
}
