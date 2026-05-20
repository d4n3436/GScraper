using System;
using System.Runtime.CompilerServices;

namespace GScraper;

internal static class GScraperGuards
{
    public static void NotNull<T>(T? obj, [CallerArgumentExpression(nameof(obj))] string? parameterName = null) where T : class
    {
        if (obj is null)
        {
            throw new ArgumentNullException(parameterName);
        }
    }

    public static void NotNullOrEmpty(string? str, [CallerArgumentExpression(nameof(str))] string? parameterName = null)
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