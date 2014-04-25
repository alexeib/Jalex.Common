﻿using System;

namespace Jalex.Infrastructure.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class IdAttribute : Attribute
    {
        public bool IsAutoGenerated { get; set; }
    }
}
