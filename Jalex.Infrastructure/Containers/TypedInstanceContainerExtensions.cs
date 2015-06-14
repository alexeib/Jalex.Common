using System;
using System.Collections.Generic;
using Jalex.Infrastructure.Utils;

namespace Jalex.Infrastructure.Containers
{
    public static class TypedInstanceContainerExtensions
    {
        public static void SetMany<TKey, TInstance>(this TypedInstanceContainer<TKey, TInstance> container, IEnumerable<TInstance> instances)
            where TKey : IEquatable<TKey> 
            where TInstance : class
        {
            Guard.AgainstNull(instances, "instances");

            foreach (var instance in instances)
            {
                container.Set(instance);
            }
        }
    }
}
