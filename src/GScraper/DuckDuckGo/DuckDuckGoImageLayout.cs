using JetBrains.Annotations;

namespace GScraper.DuckDuckGo;

/// <summary>
/// Specifies the image layouts in DuckDuckGo.
/// </summary>
[PublicAPI]
public enum DuckDuckGoImageLayout
{
    /// <summary>
    /// All layouts
    /// </summary>
    All,

    /// <summary>
    /// Square layout.
    /// </summary>
    Square,

    /// <summary>
    /// Tall layout.
    /// </summary>
    Tall,

    /// <summary>
    /// Wide layout.
    /// </summary>
    Wide
}