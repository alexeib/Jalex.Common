using System;
using Jalex.Infrastructure.Repository;

namespace Jalex.Infrastructure.Test.Objects
{
    public class TestObject
    {
        public Guid Id { get; set; }
        [Indexed]
        public string Name { get; set; }
        public string RefId { get; set; }
        public int Number { get; set; }
    }
}
