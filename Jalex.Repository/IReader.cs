using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jalex.Repository
{
    public interface IReader<out T>
    {
        /// <summary>
        /// Retrieves an object by Ids. 
        /// </summary>
        /// <param name="ids">the ids of the objects to retrieve</param>
        /// <returns>The requested objects (the ones that weren't found will not be included in the set)</returns>
        IEnumerable<T> GetByIds(IEnumerable<string> ids);
    }
}
