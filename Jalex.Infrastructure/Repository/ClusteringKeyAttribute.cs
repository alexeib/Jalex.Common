using System;

namespace Jalex.Infrastructure.Repository
{
    /// <summary>
    /// Note that this attribute is used ONLY by Cassandra repository
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class ClusteringKeyAttribute : Attribute
    {
        public enum Order
        {
            None,
            Ascending,
            Descending
        }

        public ClusteringKeyAttribute(int index) { Index = index; }
        /// <summary>
        /// Sets the clustering key and optionally a clustering order for it.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="order">Use "DESC" for descending order and "ASC" for ascending order.</param>
        public ClusteringKeyAttribute(int index, Order order)
        {
            Index = index;
            ClusteringOrder = order;
        }
        public int Index = -1;
        public Order ClusteringOrder = Order.None;
        public string Name;
    }
}
