using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text.Json.Serialization;

namespace GScraper.Google;

internal class GoogleImageSearchResponse
{
    [JsonPropertyName("ischj")]
    public Ischj Ischj { get; set; } = null!;
}

internal class Ischj
{
    [JsonPropertyName("metadata")]
    public List<GoogleImageResultModel>? Metadata { get; set; }
}

internal class GoogleImageResultModel : GoogleImageResult
{
    [JsonConstructor]
    public GoogleImageResultModel(Color? color, GoogleOriginalImage originalImage, GoogleInternalImageResult result, GoogleImageThumbnail thumbnail)
        : base(color, originalImage, result, thumbnail)
    {
        OriginalImage = null!;
        Result = null!;
        Thumbnail = null!;
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    [JsonPropertyName("original_image")]
    public GoogleOriginalImage OriginalImage { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    [JsonPropertyName("result")]
    public GoogleInternalImageResult Result { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    [JsonPropertyName("thumbnail")]
    public GoogleImageThumbnail Thumbnail { get; }
}

internal class GoogleOriginalImage
{
    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = null!;

    [JsonPropertyName("width")]
    public int Width { get; set; }
}

internal class GoogleInternalImageResult
{
    [JsonPropertyName("image_source_url")]
    public string ImageSourceUrl { get; set; } = null!;

    [JsonPropertyName("page_title")]
    public string PageTitle { get; set; } = null!;

    [JsonPropertyName("referrer_url")]
    public string ReferrerUrl { get; set; } = null!;
}

internal class GoogleImageThumbnail
{
    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = null!;

    [JsonPropertyName("width")]
    public int Width { get; set; }
}