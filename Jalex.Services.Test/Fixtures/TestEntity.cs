using System;
using Jalex.Infrastructure.Repository;

namespace Jalex.Services.Test.Fixtures
{
    public class TestEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double NumValue { get; set; }

        [Indexed(IsClustered = true)]
        public string ClusteredKey { get; set; }

        [Indexed(IsClustered = true)]
        public string ClusteredKey2 { get; set; }
    }
}
