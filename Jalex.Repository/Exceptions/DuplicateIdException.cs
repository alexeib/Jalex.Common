namespace Jalex.Repository.Exceptions
{
    public class DuplicateIdException : RepositoryException
    {
        public DuplicateIdException(string message) : base(message)
        {
        }
    }
}
