using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Jalex.Repository.IdProviders
{
    public class ObjectIdIdProvider : IIdProvider, IIdGenerator
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

        #region Implementation of IIdGenerator

        public object GenerateId(object container, object document)
        {
            return GenerateNewId();
        }

        public bool IsEmpty(object id)
        {
            return string.IsNullOrEmpty(id as string);
        }

        #endregion
    }
}
