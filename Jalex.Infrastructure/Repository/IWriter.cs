﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jalex.Infrastructure.Objects;

namespace Jalex.Infrastructure.Repository
{
    public interface IWriter<in T>
    {
        /// <summary>
        /// Saves objects
        /// </summary>
        /// <param name="objects">objects to save</param>
        /// <param name="writeMode">writing mode. inserting an object that exists or updating an object that does not exist will fail. Defaults to upsert</param>
        /// <returns>Operation result with ids of the new objects in order of the objects given to this function</returns>
        Task<IEnumerable<OperationResult<Guid>>> SaveManyAsync(IEnumerable<T> objects, WriteMode writeMode);
    }
}
