using GScraper.Google;
using System.Diagnostics;
using System.Drawing;
using System.Text.Json.Serialization;

namespace GScraper; // TODO: Fix namespace

/// <summary>
/// Represents an image result from Google Images.
/// </summary>
[DebuggerDisplay("Title: {Title}, Url: {Url}")]
public class GoogleImageResult : IImageResult
{
    internal GoogleImageResult(Color? color, GoogleOriginalImage originalImage, GoogleInternalImageResult result, GoogleImageThumbnail thumbnail)
    {
        Url = originalImage.Url;
        Title = result.PageTitle;
        Width = originalImage.Width;
        Height = originalImage.Height;
        Color = color;
        DisplayUrl = result.ImageSourceUrl;
        SourceUrl = result.ReferrerUrl;
        ThumbnailUrl = thumbnail.Url;
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
    [JsonConverter(typeof(GoogleColorConverter))]
    [JsonPropertyName("background_color")]
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