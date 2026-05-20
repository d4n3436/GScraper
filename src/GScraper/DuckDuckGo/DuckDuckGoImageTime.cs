using JetBrains.Annotations;

namespace GScraper.DuckDuckGo;

/// <summary>
/// Specifies the possible times in DuckDuckGo.
/// </summary>
[PublicAPI]
public enum DuckDuckGoImageTime
{
    /// <summary>
    /// Any time.
    /// </summary>
    Any,

    /// <summary>
    /// Past day.
    /// </summary>
    Day,

    /// <summary>
    /// Past week.
    /// </summary>
    Week,

    /// <summary>
    /// Past month.
    /// </summary>
    Month
}