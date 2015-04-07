using System;

namespace Jalex.Infrastructure.Extensions
{
    public static class GuidExtensions
    {
        public static bool IsNotEmpty(this Guid guid)
        {
            return guid != Guid.Empty;
        }
    }
}
