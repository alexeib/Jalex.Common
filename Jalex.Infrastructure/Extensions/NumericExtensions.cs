namespace Jalex.Infrastructure.Extensions
{
    public static class NumericExtensions
    {
        public static bool IsNaNOrInfinity(this double val)
        {
            return double.IsNaN(val) || double.IsInfinity(val);
        }
    }
}
