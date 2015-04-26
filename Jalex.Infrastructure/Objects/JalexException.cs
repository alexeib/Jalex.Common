using System;

namespace Jalex.Infrastructure.Objects
{
    public abstract class JalexException : Exception
    {
        protected JalexException(string message)
            : base(message)
        {
            
        }
    }
}
