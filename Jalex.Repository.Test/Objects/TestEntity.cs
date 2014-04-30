﻿using Jalex.Infrastructure.Attributes;

namespace Jalex.Repository.Test.Objects
{
    public class TestEntity
    {
        public string Id { get; set; }
        [Indexed(false)]
        public string Name { get; set; }
    }
}
