using JetBrains.Annotations;

namespace GScraper.Google;

/// <summary>
/// Specifies the possible times in Google Images.
/// </summary>
[PublicAPI]
public enum GoogleImageTime
{
    /// <summary>
    /// Any time.
    /// </summary>
    Any,

    /// <summary>
    /// Past 24 hours.
    /// </summary>
    Day = 'd',

    /// <summary>
    /// Past week.
    /// </summary>
    Week = 'w',

    /// <summary>
    /// Past month.
    /// </summary>
    Month = 'm',

    /// <summary>
    /// Past year.
    /// </summary>
    Year = 'y'
}