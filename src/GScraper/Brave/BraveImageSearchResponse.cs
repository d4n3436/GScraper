using System;
using System.Diagnostics;
using System.Drawing;
using System.Text.Json.Serialization;

namespace GScraper.Brave;

internal class BraveImageSearchResponse
{
    [JsonPropertyName("results")]
    public BraveImageResultModel[] Results { get; set; } = null!;
}

internal class BraveImageResultModel : BraveImageResult
{
    [JsonConstructor]
    public BraveImageResultModel(DateTimeOffset? pageAge, BraveImageProperties properties, string source,
        BraveThumbnail thumbnail, string title, string pageUrl)
        : base(pageAge, properties, source, thumbnail, title, pageUrl)
    {
        PageUrl = null!;
        Properties = null!;
        Thumbnail = null!;
    }

    // This property should be deserialized directly into BraveImageResult.SourceUrl but it doesn't work,
    // probably because it collides with BraveImageResult.Url
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    [JsonPropertyName("url")]
    public string PageUrl { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    [JsonPropertyName("properties")]
    public BraveImageProperties Properties { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    [JsonPropertyName("thumbnail")]
    public BraveThumbnail Thumbnail { get; }
}

internal class BraveImageProperties
{
    [JsonPropertyName("format")]
    public string Format { get; set; } = null!;

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("resized")]
    public string Resized { get; set; } = null!;

    [JsonPropertyName("url")]
    public string Url { get; set; } = null!;

    [JsonPropertyName("width")]
    public int Width { get; set; }
}

internal class BraveThumbnail
{
    [JsonPropertyName("bg_color")]
    [JsonConverter(typeof(BraveColorConverter))]
    public Color BgColor { get; set; }

    [JsonPropertyName("src")]
    public string Src { get; set; } = null!;
}