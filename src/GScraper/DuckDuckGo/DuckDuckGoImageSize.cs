using JetBrains.Annotations;

namespace GScraper.DuckDuckGo;

/// <summary>
/// Specifies the image sizes in DuckDuckGo.
/// </summary>
[PublicAPI]
public enum DuckDuckGoImageSize
{
    /// <summary>
    /// All sizes.
    /// </summary>
    All,

    /// <summary>
    /// Small sizes.
    /// </summary>
    Small,

    /// <summary>
    /// Medium sizes.
    /// </summary>
    Medium,

    /// <summary>
    /// Large sizes.
    /// </summary>
    Large,

    /// <summary>
    /// Wallpaper sizes.
    /// </summary>
    Wallpaper
}