using System;
using MongoDB.Bson.Serialization;

namespace Jalex.Repository.IdProviders
{
    public class GuidIdProvider : IIdProvider, IIdGenerator
    {
        #region Implementation of IIdProvider

        public Guid GenerateNewId()
        {
            var id = Guid.NewGuid();
            return id;
        }

        public bool IsIdValid(Guid id)
        {
            return id != Guid.Empty;
        }

        #endregion

        #region Implementation of IIdGenerator

        /// <summary>
        /// Generates an Id for a document.
        /// </summary>
        /// <param name="container">The container of the document (will be a MongoCollection when called from the C# driver). </param><param name="document">The document.</param>
        /// <returns>
        /// An Id.
        /// </returns>
        public object GenerateId(object container, object document)
        {
            return GenerateNewId();
        }

        /// <summary>
        /// Tests whether an Id is empty.
        /// </summary>
        /// <param name="id">The Id.</param>
        /// <returns>
        /// True if the Id is empty.
        /// </returns>
        public bool IsEmpty(object id)
        {
            return (Guid) id == Guid.Empty;
        }

        #endregion
    }
}
