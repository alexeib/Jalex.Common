using FluentAssertions;

namespace Jalex.TestUtils
{
    public static class EquivalencyExtensions
    {
        public static bool IsEquivalentTo<T>(this T a, T b)
        {
            if (Equals(a, b)) return true;
            try
            {
                a.ShouldBeEquivalentTo(b);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
