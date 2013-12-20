using System;

namespace Jalex.Infrastructure.Objects
{
    public class JalexException : ApplicationException
    {
        public JalexException(string message)
            : base(message)
        {
            
        }
    }
}
