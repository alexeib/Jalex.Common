using Jalex.Infrastructure.Attributes;

namespace Jalex.Repository.Test
{
    public class TestEntity
    {
        public string Id { get; set; }
        [Indexed(false)]
        public string Name { get; set; }
    }
}
