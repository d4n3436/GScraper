using JetBrains.Annotations;

namespace GScraper.Google;

/// <summary>
/// Specifies the image sizes in Google Images.
/// </summary>
[PublicAPI]
public enum GoogleImageSize
{
    /// <summary>
    /// Any size.
    /// </summary>
    Any,

    /// <summary>
    /// Large size.
    /// </summary>
    Large = 'l',

    /// <summary>
    /// Medium size.
    /// </summary>
    Medium = 'm',

    /// <summary>
    /// Icon size.
    /// </summary>
    Icon = 'i'
}