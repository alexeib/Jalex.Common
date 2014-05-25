using Jalex.Infrastructure.Repository;

namespace Jalex.Repository.Test.Objects
{
    public class TestEntity
    {
        public string Id { get; set; }
        [Indexed]
        public string Name { get; set; }
    }
}
