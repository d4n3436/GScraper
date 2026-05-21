using System.Text.Json.Serialization;

namespace GScraper.Brave;

[JsonSerializable(typeof(BravePageRoot))]
internal partial class BraveImageSearchResponseContext : JsonSerializerContext;