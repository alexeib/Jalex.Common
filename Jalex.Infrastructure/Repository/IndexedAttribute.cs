using System;

namespace Jalex.Infrastructure.Repository
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class IndexedAttribute : Attribute
    {
        public enum Order
        {
            Ascending,
            Descending
        }

        public IndexedAttribute()
        {
            SortOrder = Order.Ascending;
            Index = -1;
        }

        public int Index { get; set; }
        public Order SortOrder { get; set; }
        public string Name { get; set; }
        public bool IsClustered { get; set; }
    }
}
