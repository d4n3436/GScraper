using System.Text.Json.Serialization;

namespace GScraper.DuckDuckGo;

[JsonSerializable(typeof(DuckDuckGoImageResultModel[]))]
internal partial class DuckDuckGoImageResultModelContext : JsonSerializerContext
{
}