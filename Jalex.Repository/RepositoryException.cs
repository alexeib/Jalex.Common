using Jalex.Infrastructure.Objects;

namespace Jalex.Repository
{
    public class RepositoryException : JalexException
    {
        public RepositoryException(string message) 
            : base(message)
        {
            
        }
    }
}
