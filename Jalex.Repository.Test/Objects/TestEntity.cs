using System;
using Jalex.Infrastructure.Repository;

namespace Jalex.Repository.Test.Objects
{
    public class TestEntity
    {
        public Guid Id { get; set; }
        [Indexed]
        public string Name { get; set; }
        public string RefId { get; set; }
        public int Number { get; set; }
    }
}
