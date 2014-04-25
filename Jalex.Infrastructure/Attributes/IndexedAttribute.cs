using System;

namespace Jalex.Infrastructure.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class IndexedAttribute : Attribute
    {
        public string IndexGroup { get; set; }
        public int IndexOrder { get; set; }
        public bool IsUnique { get; set; }
        public IndexedAttribute(bool isUnique)
        {
            IsUnique = isUnique;
        }

        public IndexedAttribute(string indexGroup, int indexOrder)
        {
            IndexGroup = indexGroup;
            IndexOrder = indexOrder;
            IsUnique = false;
        }
    }
}
