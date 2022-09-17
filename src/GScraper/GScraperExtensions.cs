using System.Text.Json;

namespace GScraper;

internal static class GScraperExtensions
{
    public static JsonElement Last(in this JsonElement element)
    {
        int length = element.GetArrayLength();
        return element[length - 1];
    }
}