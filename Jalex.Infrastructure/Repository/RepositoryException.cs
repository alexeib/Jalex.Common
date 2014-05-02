using Jalex.Infrastructure.Objects;

namespace Jalex.Infrastructure.Repository
{
    public class RepositoryException : JalexException
    {
        public RepositoryException(string message) 
            : base(message)
        {
            
        }
    }
}
