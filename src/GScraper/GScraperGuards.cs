using System;

namespace GScraper
{
    internal static class GScraperGuards
    {
        public static void NotNull<T>(T? obj, string parameterName) where T : class
        {
            if (obj is null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        public static void NotNullOrEmpty(string? str, string parameterName)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        public static void ArgumentInRange(int length, int max, string parameterName, string message)
        {
            if (length > max)
            {
                throw new ArgumentOutOfRangeException(parameterName, message);
            }
        }
    }
}