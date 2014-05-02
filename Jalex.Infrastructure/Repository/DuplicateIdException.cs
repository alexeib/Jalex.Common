namespace Jalex.Infrastructure.Repository
{
    public class DuplicateIdException : RepositoryException
    {
        public DuplicateIdException(string message) : base(message)
        {
        }
    }
}
