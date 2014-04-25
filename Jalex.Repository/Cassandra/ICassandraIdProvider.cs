namespace Jalex.Repository.Cassandra
{
    public interface ICassandraIdProvider
    {
        string GenerateNewId();
        bool IsIdValid(string id);
    }
}
