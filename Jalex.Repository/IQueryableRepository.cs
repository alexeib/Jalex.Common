using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jalex.Repository
{
    public interface IQueryableRepository<T> : ISimpleRepository<T>, IQueryable<T>
    {
    }
}
