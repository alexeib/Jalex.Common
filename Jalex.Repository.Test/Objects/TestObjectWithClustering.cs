using System;
using Jalex.Infrastructure.Repository;

namespace Jalex.Repository.Test.Objects
{
    public class TestObjectWithClustering : IObjectWithIdAndName
    {
        public Guid Id { get; set; }

        [Indexed]
        public string Name { get; set; }

        [Indexed(IndexType = IndexType.Clustered)]
        public string RefId { get; set; }
        [Indexed(IndexType = IndexType.Clustered)]
        public int Number { get; set; }

        public TestObject NestedObject { get; set; }
    }
}
