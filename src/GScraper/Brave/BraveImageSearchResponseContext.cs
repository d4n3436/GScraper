using System.Text.Json.Serialization;

namespace GScraper.Brave;

[JsonSerializable(typeof(BraveImageSearchResponse))]
internal partial class BraveImageSearchResponseContext : JsonSerializerContext
{
}