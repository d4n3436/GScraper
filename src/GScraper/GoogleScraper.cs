using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GScraper
{
    /// <summary>
    /// Represents a simple Google Images scraper.
    /// </summary>
    public class GoogleScraper : IDisposable
    {
        /// <summary>
        /// Returns the maximum number of images that can be returned per request.
        /// </summary>
        public const int ImageLimit = 100;

        private readonly HttpClient _httpClient = new HttpClient();
        private const string _defaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.104 Safari/537.36";
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleScraper"/> class.
        /// </summary>
        public GoogleScraper()
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_defaultUserAgent);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleScraper"/> class with the provided User-Agent.
        /// </summary>
        /// <param name="userAgent">The User-Agent to use in the requests.</param>
        public GoogleScraper(string userAgent)
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
        }

        /// <summary>
        /// Gets images from Google Images.
        /// </summary>
        /// <param name="query">The keywords.</param>
        /// <param name="limit">The results limit.</param>
        /// <param name="safeSearch">Whether to use safe search filter.</param>
        /// <returns>A task representing the asynchronous operation. The result contains a read-only list of <see cref="ImageResult"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="query"/> is null or empty.</exception>
        /// <exception cref="HttpRequestException"/>
        /// <exception cref="GScraperException">Thrown when an error occurs during the scraping process.</exception>
        public async Task<IReadOnlyList<ImageResult>> GetImagesAsync(string query, int limit = ImageLimit, bool safeSearch = false)
        {
            if (string.IsNullOrEmpty(query))
            {
                throw new ArgumentNullException(nameof(query));
            }

            string url = BuildSearchUrl(query, safeSearch);
            Debug.WriteLine($"[GScraper] Obtaining image objects from: {url}");

            string page = await _httpClient.GetStringAsync(new Uri(url)).ConfigureAwait(false);
            IEnumerable<JToken> rawImages;
            try
            {
                rawImages = GetImageObjectsFromPack(ExtractDataPack(page));
            }
            catch (Exception e) when (e is ArgumentOutOfRangeException || e is JsonReaderException)
            {
                throw new GScraperException("Failed to unpack the image object data.", e);
            }

            limit = Math.Max(limit, 1);
            limit = Math.Min(limit, ImageLimit);

            var formattedImages = new List<ImageResult>();

            foreach (var rawImage in rawImages)
            {
                if (formattedImages.Count == limit)
                    break;

                var image = FormatImageObject(rawImage);

                if (image != null)
                    formattedImages.Add(image);
            }

            Debug.WriteLine($"[GScraper] {formattedImages.Count}/{limit} image objects.");

            return new ReadOnlyCollection<ImageResult>(formattedImages);
        }

        private static string ExtractDataPack(string page)
        {
            // Extract the JSON data pack from the page.
            int startLine = page.IndexOf("AF_initDataCallback({key: 'ds:1'", StringComparison.OrdinalIgnoreCase) - 10;
            int startObject = page.IndexOf('[', startLine + 1);
            int endObject = page.LastIndexOf(']', page.IndexOf("</script>", startObject + 1, StringComparison.OrdinalIgnoreCase)) + 1;
            string rawObject = page.Substring(startObject, endObject - startObject);

            // This will prevent Regex.Unescape() to unescape escaped quotes (\")
            rawObject = rawObject.Replace("\\\"", "\\\\\"");

            return Regex.Unescape(rawObject);
        }

        private static IEnumerable<JToken> GetImageObjectsFromPack(string data)
        {
            // Extract the raw image objects from the JSON data pack.
            return JToken.Parse(data)
                .ElementAtOrDefault(31)?
                .FirstOrDefault()?
                .ElementAtOrDefault(12)?
                .ElementAtOrDefault(2)?
                .Where(x => x?.FirstOrDefault()?.ValueOrDefault<int>() == 1) ?? Enumerable.Empty<JToken>();
        }

        private static ImageResult FormatImageObject(JToken obj)
        {
            var data = obj?.ElementAtOrDefault(1);
            var main = data?.ElementAtOrDefault(3);
            var info = data?.ElementAtOrDefault(9);

            if (data == null)
                return null;

            if (string.IsNullOrEmpty(info?.ToString()))
                info = data.ElementAtOrDefault(11);

            return new ImageResult(
                main?.ElementAtOrDefault(2)?.ValueOrDefault<int?>(),
                main?.ElementAtOrDefault(1)?.ValueOrDefault<int?>(),
                main?.FirstOrDefault()?.ValueOrDefault<string>(),
                info?.ValueOrDefault("2003")?.ElementAtOrDefault(3)?.ValueOrDefault<string>(),
                info?.ValueOrDefault("183836587")?.FirstOrDefault()?.ValueOrDefault<string>(),
                info?.ValueOrDefault("2003")?.ElementAtOrDefault(2)?.ValueOrDefault<string>(),
                data.ElementAtOrDefault(2)?.FirstOrDefault()?.ValueOrDefault<string>());
        }

        private static string BuildSearchUrl(string query, bool safeSearch)
        {
            string url = "https://www.google.com/search" +
                         $"?q={Uri.EscapeDataString(query)}" +
                         "&espv=2" +
                         "&biw=1366" +
                         "&bih=667" +
                         "&site=webhp" +
                         "&source=lnms" +
                         "&tbm=isch" +
                         "&sa=X" +
                         "&ei=XosDVaCXD8TasATItgE" +
                         "&ved=0CAcQ_AUoAg";

            if (safeSearch)
                url += "&safe=active";

            return url;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc cref="Dispose()"/>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
                _httpClient.Dispose();

            _disposed = true;
        }
    }
}