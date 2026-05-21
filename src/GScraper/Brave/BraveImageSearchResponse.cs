using System.Text.Json.Serialization;

namespace GScraper.Brave;

internal sealed class BravePageRoot
{
    [JsonPropertyName("body")]
    public required BravePageBody Body { get; set; }
}

internal sealed class BravePageBody
{
    [JsonPropertyName("response")]
    public required BravePageResponse Response { get; set; }
}

internal sealed class BravePageResponse
{
    [JsonPropertyName("results")]
    public required BraveImageResultModel[] Results { get; set; }
}

internal sealed class BraveImageResultModel
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("thumbnail")]
    public BraveImageThumbnailModel? Thumbnail { get; set; }

    [JsonPropertyName("properties")]
    public BraveImagePropertiesModel? Properties { get; set; }
}

internal sealed class BraveImageThumbnailModel
{
    [JsonPropertyName("src")]
    public string? Src { get; set; }
}

internal sealed class BraveImagePropertiesModel
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("resized")]
    public string? Resized { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }
}