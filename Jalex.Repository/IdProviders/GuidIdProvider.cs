using System;

namespace Jalex.Repository.IdProviders
{
    public class GuidIdProvider : IIdProvider
    {
        #region Implementation of IIdProvider

        public string GenerateNewId()
        {
            var id = Guid.NewGuid().ToString("N");
            return id;
        }

        public bool IsIdValid(string id)
        {
            Guid g;
            bool isParsable = Guid.TryParseExact(id, "N", out g);
            return isParsable;
        }

        #endregion
    }
}
