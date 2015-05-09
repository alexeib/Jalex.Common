using System;

namespace Jalex.Infrastructure.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNotNullOrEmpty(this string s)
        {
            return !string.IsNullOrEmpty(s);
        }

        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        public static Guid ToGuid(this string s)
        {
            return Guid.Parse(s);
        }

        public static string Params(this string s, params object[] args)
        {
            return string.Format(s, args);
        }
    }
}