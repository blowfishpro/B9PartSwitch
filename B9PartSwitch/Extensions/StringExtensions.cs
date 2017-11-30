using System;

namespace B9PartSwitch
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string s) => string.IsNullOrEmpty(s);

        public static void ThrowIfNullOrEmpty(this string s, string paramName)
        {
            if (s.IsNullOrEmpty()) throw new ArgumentNullException("Value cannot be null or empty string", paramName);
        }
    }
}
