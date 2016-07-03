using System;

namespace Jalex.Infrastructure.Repository
{
    [Flags]
    public enum IndexType
    {
        Secondary = 1,
        Clustered = 2
    }
}
