using System;

namespace Jalex.Infrastructure.Attributes
{
    public class IndexedAttribute : Attribute
    {
        public bool IsUnique { get; set; }
        public IndexedAttribute(bool isUnique)
        {
            IsUnique = isUnique;
        }
    }
}
