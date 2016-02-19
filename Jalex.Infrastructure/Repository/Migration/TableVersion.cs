using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jalex.Infrastructure.Repository.Migration
{
    public class TableVersion
    {
        public Guid Id { get; set; }

        [Indexed]
        public string TableName { get; set; }

        public Version Version { get; set; }

        public TableVersion()
        {
            Version = new Version();
        }
    }
}
