using System.Text.Json.Serialization;

namespace GScraper.Google;

[JsonSerializable(typeof(GoogleImageSearchResponse))]
internal partial class GoogleImageSearchResponseContext : JsonSerializerContext
{
}