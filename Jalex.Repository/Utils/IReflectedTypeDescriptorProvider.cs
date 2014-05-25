using System;

namespace Jalex.Repository.Utils
{
    public interface IReflectedTypeDescriptorProvider
    {
        IReflectedTypeDescriptor<T> GetReflectedTypeDescriptor<T>();
        IReflectedTypeDescriptor GetReflectedTypeDescriptor(Type type);
    }
}
