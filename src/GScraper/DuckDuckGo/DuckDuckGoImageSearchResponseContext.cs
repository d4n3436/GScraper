using System.Text.Json.Serialization;

namespace GScraper.DuckDuckGo;

[JsonSerializable(typeof(DuckDuckGoImageSearchResponse))]
internal partial class DuckDuckGoImageSearchResponseContext : JsonSerializerContext
{
}