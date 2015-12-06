using System;
using System.Collections.Generic;

namespace Jalex.Infrastructure.Containers
{
    public static class TypedInstanceContainerExtensions
    {
        public static void SetMany<TKey, TInstance>(this TypedInstanceContainer<TKey, TInstance> container, IEnumerable<TInstance> instances)
            where TKey : IEquatable<TKey> 
            where TInstance : class
        {
            if (instances == null) throw new ArgumentNullException(nameof(instances));
            foreach (var instance in instances)
            {
                container.Set(instance);
            }
        }
    }
}
