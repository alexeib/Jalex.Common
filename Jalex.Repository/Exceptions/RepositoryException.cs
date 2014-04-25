using Jalex.Infrastructure.Objects;

namespace Jalex.Repository.Exceptions
{
    public class RepositoryException : JalexException
    {
        public RepositoryException(string message) 
            : base(message)
        {
            
        }
    }
}
