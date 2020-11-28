using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace GScraper
{
    /// <summary>
    /// Represents the extension methods that <see cref="GoogleScraper"/> uses.
    /// </summary>
    internal static class GScraperExtensions
    {
        /// <summary>
        /// Gets the value of <paramref name="token"/> converted to the specified type or gets the default value if <paramref name="token"/> is not a <see cref="JValue"/>.
        /// </summary>
        /// <typeparam name="T">The type to convert the value to.</typeparam>
        /// <param name="token">A <see cref="JToken"/> cast as a <see cref="IEnumerable{T}"/> of <see cref="JToken"/>.</param>
        /// <returns>default(<typeparamref name="T"/>) if <paramref name="token"/> is not a <see cref="JValue"/>; otherwise, the converted value.</returns>
        public static T ValueOrDefault<T>(this IEnumerable<JToken> token)
            => token is JValue ? token.Value<T>() : default;

        /// <summary>
        /// Gets the <see cref="JToken"/> with the specified key converted to the specified type or the default value.
        /// </summary>
        /// <typeparam name="T">The type to convert the token to.</typeparam>
        /// <param name="token">The token.</param>
        /// <param name="key">The token key.</param>
        /// <returns>
        /// The converted token value if <paramref name="key"/> is an <see cref="int"/> and <paramref name="token"/> is a <see cref="JArray"/>,
        /// or if <paramref name="key"/> is a <see cref="string"/> and <paramref name="token"/> is a <see cref="JValue"/> or a <see cref="JObject"/>;
        /// otherwise, default(<typeparamref name="T"/>).</returns>
        public static T ValueOrDefault<T>(this JToken token, object key)
            => (key is int && token is JArray) || (key is string && token is JValue) || (key is string && token is JObject) ? token.Value<T>(key) : default;

        /// <inheritdoc cref="ValueOrDefault{T}(JToken, object)"/>
        public static JToken ValueOrDefault(this JToken token, object key) => ValueOrDefault<JToken>(token, key);
    }
}