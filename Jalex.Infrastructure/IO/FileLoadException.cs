using System;

namespace Jalex.Infrastructure.IO
{
    public class FileLoadException : ApplicationException
    {
        public FileLoadException(string message)
            : base(message)
        {
        }
    }
}
