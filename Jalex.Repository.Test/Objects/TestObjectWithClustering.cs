using System;
using Jalex.Infrastructure.Repository;

namespace Jalex.Repository.Test.Objects
{
    public class TestObjectWithClustering : IObjectWithIdAndName
    {
        public Guid Id { get; set; }

        [Indexed]
        public string Name { get; set; }

        [Indexed(IsClustered = true)]
        public string RefId { get; set; }
        [Indexed(IsClustered = true)]
        public int Number { get; set; }
    }
}
