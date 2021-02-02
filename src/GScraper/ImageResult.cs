namespace GScraper
{
    /// <summary>
    /// Represents a Google Images result.
    /// </summary>
    public class ImageResult
    {
        internal ImageResult(int? height, int? width, string link, string title, string displayLink, string contextLink, string thumbnailLink)
        {
            Height = height;
            Width = width;
            Link = link;
            Title = title;
            DisplayLink = displayLink;
            ContextLink = contextLink;
            ThumbnailLink = thumbnailLink;
        }

        /// <summary>
        /// Gets the height of the image, in pixels.
        /// </summary>
        public int? Height { get; }

        /// <summary>
        /// Gets the width of the image, in pixels.
        /// </summary>
        public int? Width { get; }

        /// <summary>
        /// Gets a URL pointing to the image.
        /// </summary>
        public string Link { get; }

        /// <summary>
        /// Gets the title of the image result.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets an abridged version of <see cref="ContextLink"/>, e.g. www.example.com.
        /// </summary>
        public string DisplayLink { get; }

        /// <summary>
        /// Gets a URL pointing to the webpage hosting the image.
        /// </summary>
        public string ContextLink { get; }

        /// <summary>
        /// Gets a URL pointing to the thumbnail image.
        /// </summary>
        public string ThumbnailLink { get; }
    }
}