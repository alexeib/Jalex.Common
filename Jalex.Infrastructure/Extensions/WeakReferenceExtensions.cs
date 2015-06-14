using System;

namespace Jalex.Infrastructure.Extensions
{
    public static class WeakReferenceExtensions
    {
        public static T GetTargetOrDefault<T>(this WeakReference<T> weakReference) where T : class
        {
            T val;
            weakReference.TryGetTarget(out val);
            return val;
        }
    }
}
