using System;
using System.Linq;
using System.Text.Json;

namespace GScraper
{
    internal static class GScraperExtensions
    {
        public static JsonElement FirstOrDefault(this JsonElement element)
            => element.ValueKind == JsonValueKind.Array ? element.EnumerateArray().FirstOrDefault() : default;

        public static JsonElement ElementAtOrDefault(this JsonElement element, int index)
            => element.ValueKind == JsonValueKind.Array ? element.EnumerateArray().ElementAtOrDefault(index) : default;

        public static JsonElement GetPropertyOrDefault(this JsonElement element, string propertyName)
            => element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out var value) ? value : default;

        public static string GetStringOrDefault(this JsonElement element, string defaultValue = "")
            => element.ValueKind is JsonValueKind.String or JsonValueKind.Null ? element.GetString() ?? defaultValue : defaultValue;

        public static int GetInt32OrDefault(this JsonElement element)
            => element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out int value) ? value : default;

        public static DateTimeOffset GetDateTimeOffsetOrDefault(this JsonElement element)
            => element.ValueKind == JsonValueKind.String && element.TryGetDateTimeOffset(out var value) ? value : default;
    }
}