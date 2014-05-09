using MongoDB.Bson;

namespace Jalex.Repository.IdProviders
{
    public class ObjectIdIdProvider : IIdProvider
    {
        #region Implementation of IIdProvider

        public string GenerateNewId()
        {
            var objectId = ObjectId.GenerateNewId();
            return objectId.ToString();
        }

        public bool IsIdValid(string id)
        {
            ObjectId d;
            return ObjectId.TryParse(id, out d);
        }

        #endregion
    }
}
