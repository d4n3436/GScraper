using System.Diagnostics;
using System.Text.Json.Serialization;

namespace GScraper.DuckDuckGo;

/// <summary>
/// Represents an image result from DuckDuckGo.
/// </summary>
[DebuggerDisplay("Title: {Title}, Url: {Url}")]
public class DuckDuckGoImageResult : IImageResult
{
    internal DuckDuckGoImageResult(string url, string title, int width,
        int height,string sourceUrl, string thumbnailUrl, string source)
    {
        Url = url;
        Title = title;
        Width = width;
        Height = height;
        SourceUrl = sourceUrl;
        ThumbnailUrl = thumbnailUrl;
        Source = source;
    }

    /// <inheritdoc/>
    [JsonPropertyName("image")]
    public string Url { get; }

    /// <inheritdoc/>
    [JsonPropertyName("title")]
    public string Title { get; }

    /// <inheritdoc/>
    [JsonPropertyName("width")]
    public int Width { get; }

    /// <inheritdoc/>
    [JsonPropertyName("height")]
    public int Height { get; }

    /// <summary>
    /// Gets a URL pointing to the webpage hosting the image.
    /// </summary>
    [JsonPropertyName("url")]
    public string SourceUrl { get; }

    /// <summary>
    /// Gets a URL pointing to the thumbnail image.
    /// </summary>
    [JsonPropertyName("thumbnail")]
    public string ThumbnailUrl { get; }

    /// <summary>
    /// Gets the search engine this result comes from.
    /// </summary>
    [JsonPropertyName("source")]
    public string Source { get; }

    /// <summary>
    /// Returns the URL of this result.
    /// </summary>
    /// <returns>The URL of this result.</returns>
    public override string ToString() => Url;
}