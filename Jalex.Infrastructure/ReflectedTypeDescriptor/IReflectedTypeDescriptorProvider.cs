using System;

namespace Jalex.Infrastructure.ReflectedTypeDescriptor
{
    public interface IReflectedTypeDescriptorProvider
    {
        IReflectedTypeDescriptor<T> GetReflectedTypeDescriptor<T>();
        IReflectedTypeDescriptor GetReflectedTypeDescriptor(Type type);
    }
}
