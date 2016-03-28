using System;
using Jalex.Infrastructure.Repository;

namespace Jalex.Repository.Migration
{
    public class TableVersion
    {
        public Guid Id { get; set; }

        [Indexed]
        public string TableName { get; set; }

        public int Version { get; set; }
    }
}
