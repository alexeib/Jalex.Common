using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jalex.Infrastructure.Objects;

namespace Jalex.Infrastructure.Repository
{
    public interface IWriterWithTtl<in T> : IWriter<T>
    {
        /// <summary>
        /// Saves objects
        /// </summary>
        /// <param name="objects">objects to save</param>
        /// <param name="writeMode">writing mode. inserting an object that exists or updating an object that does not exist will fail. Defaults to upsert</param>
        /// <param name="timeToLive">The lifetime of all objects to be saved before they are deleted. If null, objects never expire</param>
        /// <returns>Operation result with ids of the new objects in order of the objects given to this function</returns>
        Task<IEnumerable<OperationResult<Guid>>> SaveManyAsync(IEnumerable<T> objects, WriteMode writeMode, TimeSpan? timeToLive);
    }
}