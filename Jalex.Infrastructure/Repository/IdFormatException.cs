namespace Jalex.Infrastructure.Repository
{
    public class IdFormatException : RepositoryException
    {
        public IdFormatException(string message) : base(message)
        {
        }
    }
}
