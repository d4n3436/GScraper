using JetBrains.Annotations;
using System;
using System.Diagnostics;
using System.Drawing;

namespace GScraper.Brave;

/// <summary>
/// Represents an image result from Brave.
/// </summary>
[PublicAPI]
[DebuggerDisplay($"{nameof(Title)}: {{Title}}, {nameof(Url)}: {{Url}}")]
public class BraveImageResult : IImageResult
{
    internal BraveImageResult(string url, string title, int width, int height,
        string sourceUrl, string source, string thumbnailUrl, string resizedUrl)
    {
        Url = url;
        Title = title;
        Width = width;
        Height = height;
        SourceUrl = sourceUrl;
        Source = source;
        ThumbnailUrl = thumbnailUrl;
        ResizedUrl = resizedUrl;
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
    [Obsolete("Brave no longer returns the background color of the image.")]
    public Color Color { get; } = Color.Empty;

    /// <summary>
    /// Gets a URL pointing to the webpage hosting the image.
    /// </summary>
    public string SourceUrl { get; }

    /// <summary>
    /// Gets the page age.
    /// </summary>
    [Obsolete("Brave no longer returns the page age.")]
    public DateTimeOffset? PageAge { get; }

    /// <summary>
    /// Gets the name or the root URL of the website this image comes from.
    /// </summary>
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
    [Obsolete("Brave no longer returns the format of the image.")]
    public string Format { get; } = string.Empty;

    /// <summary>
    /// Returns the URL of this result.
    /// </summary>
    /// <returns>The URL of this result.</returns>
    public override string ToString() => Url;
}