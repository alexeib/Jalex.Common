using Jalex.Infrastructure.Attributes;

namespace Jalex.Repository.Test
{
    public class TestEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }

        [Ignore]
        public string IgnoredProperty { get; set; }
    }
}
