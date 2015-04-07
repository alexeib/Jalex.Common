using System;

namespace Jalex.Repository.IdProviders
{
    public interface IIdProvider
    {
        Guid GenerateNewId();
        bool IsIdValid(Guid id);
    }
}
