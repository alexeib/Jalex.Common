namespace Jalex.Infrastructure.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNotNullOrEmpty(this string s)
        {
            return !string.IsNullOrEmpty(s);
        }
    }
}