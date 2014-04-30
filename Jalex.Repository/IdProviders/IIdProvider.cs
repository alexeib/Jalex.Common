namespace Jalex.Repository.IdProviders
{
    public interface IIdProvider
    {
        string GenerateNewId();
        bool IsIdValid(string id);
    }
}
