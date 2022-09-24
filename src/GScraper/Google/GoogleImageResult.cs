using System.Diagnostics;
using System.Drawing;

namespace GScraper;

/// <summary>
/// Represents an image result from Google Images.
/// </summary>
[DebuggerDisplay("Title: {Title}, Url: {Url}")]
public class GoogleImageResult : IImageResult
{
    internal GoogleImageResult(string url, string title, int width, int height,
        Color? color, string displayUrl, string sourceUrl, string thumbnailUrl)
    {
        Url = url;
        Title = title;
        Width = width;
        Height = height;
        Color = color;
        DisplayUrl = displayUrl;
        SourceUrl = sourceUrl;
        ThumbnailUrl = thumbnailUrl;
    }

    /// <inheritdoc/>
    public string Url { get; }

    /// <inheritdoc/>
    public string Title { get; }

    /// <inheritdoc/>
    public int Width { get; }

    /// <inheritdoc/>
    public int Height { get; }

    /// <summary>
    /// Gets the background color of this result.
    /// </summary>
    public Color? Color { get; }

    /// <summary>
    /// Gets an abridged version of <see cref="SourceUrl"/>, e.g. www.example.com.
    /// </summary>
    public string DisplayUrl { get; }

    /// <summary>
    /// Gets a URL pointing to the webpage hosting the image.
    /// </summary>
    public string SourceUrl { get; }

    /// <summary>
    /// Gets a URL pointing to the thumbnail image.
    /// </summary>
    public string ThumbnailUrl { get; }

    /// <summary>
    /// Returns the URL of this result.
    /// </summary>
    /// <returns>The URL of this result.</returns>
    public override string ToString() => Url;
}