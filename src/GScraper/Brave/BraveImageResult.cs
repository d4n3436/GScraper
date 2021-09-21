using System;
using System.Diagnostics;

namespace GScraper.Brave
{
    /// <summary>
    /// Represents an image result from Brave.
    /// </summary>
    [DebuggerDisplay("Title: {Title}, Url: {Url}")]
    public class BraveImageResult : IImageResult
    {
        internal BraveImageResult(string url, string title, int width, int height, string sourceUrl,
            DateTimeOffset pageAge, string source, string thumbnailUrl, string resizedUrl, string format)
        {
            Url = url;
            Title = title;
            Width = width;
            Height = height;
            SourceUrl = sourceUrl;
            PageAge = pageAge;
            Source = source;
            ThumbnailUrl = thumbnailUrl;
            ResizedUrl = resizedUrl;
            Format = format;
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
        /// Gets a URL pointing to the webpage hosting the image.
        /// </summary>
        public string SourceUrl { get; }

        /// <summary>
        /// Gets the page age.
        /// </summary>
        public DateTimeOffset PageAge { get; }

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
        public string Format { get; }

        /// <summary>
        /// Returns the URL of this result.
        /// </summary>
        /// <returns>The URL of this result.</returns>
        public override string ToString() => Url;
    }
}