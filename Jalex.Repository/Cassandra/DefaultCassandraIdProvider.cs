using System;

namespace Jalex.Repository.Cassandra
{
    public class DefaultCassandraIdProvider : ICassandraIdProvider
    {
        #region Implementation of ICassandraIdProvider

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
