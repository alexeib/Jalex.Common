using System;
using Jalex.Infrastructure.Repository;
using Jalex.Repository.Test.Objects;

namespace Jalex.Repository.Test
{
    public class TestObject : IObjectWithIdAndName
    {
        public string Id { get; set; }
        [Indexed]
        public string Name { get; set; }
        public string RefId { get; set; }
        public int Number { get; set; }
    }
}
