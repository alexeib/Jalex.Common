using System;

namespace Jalex.Infrastructure.Test
{
    public class Entity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Money { get; set; }
        public DateTime Created { get; set; }
        public ChildEntity Child { get; set; }
    }
}