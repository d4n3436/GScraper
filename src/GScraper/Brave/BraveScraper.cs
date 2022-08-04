using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace GScraper.Brave;

/// <summary>
/// Represents a Brave Search scraper.
/// </summary>
public class BraveScraper : IDisposable
{
    /// <summary>
    /// Returns the default API endpoint.
    /// </summary>
    public const string DefaultApiEndpoint = "https://search.brave.com/api/";

    private readonly HttpClient _httpClient;
    private const string _defaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.114 Safari/537.36";
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="BraveScraper"/> class.
    /// </summary>
    public BraveScraper() : this(new HttpClient())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BraveScraper"/> class using the provided <see cref="HttpClient"/>.
    /// </summary>
    public BraveScraper(HttpClient client) : this(client, DefaultApiEndpoint)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BraveScraper"/> class using the provided <see cref="HttpClient"/> and API endpoint.
    /// </summary>
    public BraveScraper(HttpClient client, string apiEndpoint)
    {
        GScraperGuards.NotNull(client, nameof(client));
        GScraperGuards.NotNullOrEmpty(apiEndpoint, nameof(apiEndpoint));
        _httpClient = client;
        _httpClient.BaseAddress = new Uri(apiEndpoint);
        if (_httpClient.DefaultRequestHeaders.UserAgent.Count == 0)
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_defaultUserAgent);
        }
    }

    /// <summary>
    /// Gets images from Brave Search.
    /// </summary>
    /// <remarks>This method returns at most 150 image results (unless Brave changes something in their API).</remarks>
    /// <param name="query">The search query.</param>
    /// <param name="safeSearch">The safe search level.</param>
    /// <param name="country">The country. <see cref="BraveCountries"/> contains the countries that can be used here.</param>
    /// <param name="size">The image size.</param>
    /// <param name="type">The image type.</param>
    /// <param name="layout">The image layout.</param>
    /// <param name="color">The image color.</param>
    /// <param name="license">The image license.</param>
    /// <returns>A task representing the asynchronous operation. The result contains an <see cref="IEnumerable{T}"/> of <see cref="BraveImageResult"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="query"/> is null or empty.</exception>
    /// <exception cref="GScraperException">An error occurred during the scraping process.</exception>
    public async Task<IEnumerable<BraveImageResult>> GetImagesAsync(string query, SafeSearchLevel safeSearch = SafeSearchLevel.Moderate,
        string? country = null, BraveImageSize size = BraveImageSize.All, BraveImageType type = BraveImageType.All,
        BraveImageLayout layout = BraveImageLayout.All, BraveImageColor color = BraveImageColor.All, BraveImageLicense license = BraveImageLicense.All)
    {
        GScraperGuards.NotNull(query, nameof(query));

        byte[] bytes;
        using (var request = new HttpRequestMessage())
        {
            string cookie = $"safesearch={safeSearch.ToString().ToLowerInvariant()}";
            if (!string.IsNullOrEmpty(country))
            {
                cookie += $"; country={country}";
            }

            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri(BuildImageQuery(query, size, type, layout, color, license), UriKind.Relative);
            request.Headers.Add("cookie", cookie);

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        }

        var document = JsonDocument.Parse(bytes);
        var results = document.RootElement.GetPropertyOrDefault("results");

        return EnumerateResults(results);
    }

    private static string BuildImageQuery(string query, BraveImageSize size, BraveImageType type,
        BraveImageLayout layout, BraveImageColor color, BraveImageLicense license)
    {
        string url = $"images?q={Uri.EscapeDataString(query)}";

        // Doesn't seem to work via query strings
        /*
        if (level != SafeSearchLevel.Moderate)
            url += $"&safesearch={level.ToString().ToLowerInvariant()}";

        if (!string.IsNullOrEmpty(country))
            url += $"&country={country}";
        */

        if (size != BraveImageSize.All)
            url += $"&size={size}";

        if (type != BraveImageType.All)
            url += $"&_type={type}";

        if (layout != BraveImageLayout.All)
            url += $"&layout={layout}";

        if (color != BraveImageColor.All)
            url += $"&color={color}";

        if (license != BraveImageLicense.All)
            url += $"&license={license}";

        return url;
    }

    private static IEnumerable<BraveImageResult> EnumerateResults(JsonElement results)
    {
        if (results.ValueKind != JsonValueKind.Array)
        {
            yield break;
        }

        foreach (var result in results.EnumerateArray())
        {
            var properties = result.GetPropertyOrDefault("properties");
            string url = properties.GetPropertyOrDefault("url").GetStringOrDefault();
            string title = result.GetPropertyOrDefault("title").GetStringOrDefault();
            int width = properties.GetPropertyOrDefault("width").GetInt32OrDefault();
            int height = properties.GetPropertyOrDefault("height").GetInt32OrDefault();
            string sourceUrl = result.GetPropertyOrDefault("url").GetStringOrDefault();
            var pageAge = result.GetPropertyOrDefault("page_age").GetDateTimeOffsetOrDefault();
            string source = result.GetPropertyOrDefault("source").GetStringOrDefault();
            string thumbnailUrl = result.GetPropertyOrDefault("thumbnail").GetPropertyOrDefault("src").GetStringOrDefault();
            string resizedUrl = properties.GetPropertyOrDefault("resized").GetStringOrDefault();
            string format = properties.GetPropertyOrDefault("format").GetStringOrDefault();

            yield return new BraveImageResult(url, title, width, height, sourceUrl, pageAge, source, thumbnailUrl, resizedUrl, format);
        }
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