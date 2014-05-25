using Jalex.Infrastructure.Repository;

namespace Jalex.Repository.Test
{
    public class TestObject
    {
        public string Id { get; set; }
        [Indexed]
        public string Name { get; set; }
    }
}
