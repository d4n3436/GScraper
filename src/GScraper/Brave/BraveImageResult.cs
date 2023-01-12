using System;
using System.Diagnostics;
using System.Drawing;
using System.Text.Json.Serialization;

namespace GScraper.Brave;

/// <summary>
/// Represents an image result from Brave.
/// </summary>
[DebuggerDisplay($"{nameof(Title)}: {{Title}}, {nameof(Url)}: {{Url}}")]
public class BraveImageResult : IImageResult
{
    internal BraveImageResult(DateTimeOffset pageAge, BraveImageProperties properties, string source, BraveThumbnail thumbnail, string title, string pageUrl)
    {
        Url = properties.Url;
        Title = title;
        Width = properties.Width;
        Height = properties.Height;
        Color = thumbnail.BgColor;
        SourceUrl = pageUrl;
        PageAge = pageAge;
        Source = source;
        ThumbnailUrl = thumbnail.Src;
        ResizedUrl = properties.Resized;
        Format = properties.Format;
    }

    /// <inheritdoc/>
    public string Url { get; }

    /// <inheritdoc/>
    [JsonPropertyName("title")]
    public string Title { get; }

    /// <inheritdoc/>
    public int Width { get; }

    /// <inheritdoc/>
    public int Height { get; }

    /// <summary>
    /// Gets the background color of this result.
    /// </summary>
    [JsonConverter(typeof(BraveColorConverter))]
    public Color Color { get; }

    /// <summary>
    /// Gets a URL pointing to the webpage hosting the image.
    /// </summary>
    public string SourceUrl { get; }

    /// <summary>
    /// Gets the page age.
    /// </summary>
    [JsonPropertyName("page_age")]
    public DateTimeOffset PageAge { get; }

    /// <summary>
    /// Gets the name or the root URL of the website this image comes from.
    /// </summary>
    [JsonPropertyName("source")]
    public string Source { get; }

    /// <summary>
    /// Gets a URL pointing to the thumbnail image.
    /// </summary>
    public string ThumbnailUrl { get; }

    /// <summary>
    /// Gets a URL pointing to the resized image.
    /// </summary>
    public string ResizedUrl { get; }

    /// <summary>
    /// Gets the format of the image.
    /// </summary>
    public string Format { get; }

    /// <summary>
    /// Returns the URL of this result.
    /// </summary>
    /// <returns>The URL of this result.</returns>
    public override string ToString() => Url;
}