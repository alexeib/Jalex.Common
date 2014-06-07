using System;
using System.Collections.Concurrent;

namespace Jalex.Infrastructure.ReflectedTypeDescriptor
{
    public class ReflectedTypeDescriptorProvider : IReflectedTypeDescriptorProvider
    {
        private static readonly ConcurrentDictionary<Type, IReflectedTypeDescriptor> _typeDescriptorCache = new ConcurrentDictionary<Type, IReflectedTypeDescriptor>();

        public IReflectedTypeDescriptor<T> GetReflectedTypeDescriptor<T>()
        {
            var dictKey = typeof(T);

            var typeDescriptor = _typeDescriptorCache.GetOrAdd(dictKey, _ => new ReflectedTypeDescriptor<T>());
            return (IReflectedTypeDescriptor<T>)typeDescriptor;
        }

        public IReflectedTypeDescriptor GetReflectedTypeDescriptor(Type type)
        {
            var typeDescriptor = _typeDescriptorCache.GetOrAdd(type, t =>
                                                                           {
                                                                               Type generic = typeof (ReflectedTypeDescriptor<>);
                                                                               Type typed = generic.MakeGenericType(t);
                                                                               return (IReflectedTypeDescriptor)Activator.CreateInstance(typed);
                                                                           });
            return typeDescriptor;
        }
    }
}
