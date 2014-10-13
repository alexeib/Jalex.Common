using System.Collections.Generic;
using Jalex.Infrastructure.Objects;

namespace Jalex.Infrastructure.Repository
{
    public interface IWriter<in T>
    {
        /// <summary>
        /// Saves an object
        /// </summary>
        /// <param name="obj">object to save</param>
        /// <param name="writeMode">writing mode. inserting an object that exists or updating an object that does not exist will fail. Defaults to upsert</param>
        /// <returns>Operation result with id of the new object in order of the objects given to this function</returns>
        OperationResult<string> Save(T obj, WriteMode writeMode);

        /// <summary>
        /// Saves objects
        /// </summary>
        /// <param name="objects">objects to save</param>
        /// <param name="writeMode">writing mode. inserting an object that exists or updating an object that does not exist will fail. Defaults to upsert</param>
        /// <returns>Operation result with ids of the new objects in order of the objects given to this function</returns>
        IEnumerable<OperationResult<string>> SaveMany(IEnumerable<T> objects, WriteMode writeMode);
    }
}
