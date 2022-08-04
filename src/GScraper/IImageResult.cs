namespace GScraper;

/// <summary>
/// Represents an image result.
/// </summary>
public interface IImageResult
{
    /// <summary>
    /// Gets a URL pointing to the image.
    /// </summary>
    string Url { get; }

    /// <summary>
    /// Gets the title of the image result.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Gets the width of the image, in pixels.
    /// </summary>
    int Width { get; }

    /// <summary>
    /// Gets the height of the image, in pixels.
    /// </summary>
    int Height { get; }
}