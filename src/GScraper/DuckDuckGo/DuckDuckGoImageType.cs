using JetBrains.Annotations;

namespace GScraper.DuckDuckGo;

/// <summary>
/// Specifies the image types in DuckDuckGo.
/// </summary>
[PublicAPI]
public enum DuckDuckGoImageType
{
    /// <summary>
    /// All types.
    /// </summary>
    All,

    /// <summary>
    /// Photograph.
    /// </summary>
    Photo,

    /// <summary>
    /// Clip Art.
    /// </summary>
    Clipart,

    /// <summary>
    /// Animated GIF.
    /// </summary>
    Gif,

    /// <summary>
    /// Transparent.
    /// </summary>
    Transparent,

    /// <summary>
    /// Line drawing.
    /// </summary>
    Line
}