using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace GScraper.Brave;

// TODO: Add support for cancellation tokens and regular search method

/// <summary>
/// Represents a Brave Search scraper.
/// </summary>
public class BraveScraper : IDisposable
{
    /// <summary>
    /// Returns the default API endpoint.
    /// </summary>
    public const string DefaultApiEndpoint = "https://search.brave.com/api/";

    private const string _defaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36";
    private static readonly Uri _defaultBaseAddress = new(DefaultApiEndpoint);

    private readonly HttpClient _httpClient;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="BraveScraper"/> class.
    /// </summary>
    public BraveScraper()
        : this(new HttpClient())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BraveScraper"/> class using the provided <see cref="HttpClient"/>.
    /// </summary>
    public BraveScraper(HttpClient client)
    {
        _httpClient = client;
        Init(_httpClient, _defaultBaseAddress);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BraveScraper"/> class using the provided <see cref="HttpClient"/> and API endpoint.
    /// </summary>
    [Obsolete("This constructor is deprecated and it will be removed in a future version. Use BraveScraper(HttpClient) instead.")]
    public BraveScraper(HttpClient client, string apiEndpoint)
    {
        _httpClient = client;
        Init(_httpClient, new Uri(apiEndpoint));
    }

    private void Init(HttpClient client, Uri apiEndpoint)
    {
        GScraperGuards.NotNull(client, nameof(client));
        GScraperGuards.NotNull(apiEndpoint, nameof(apiEndpoint));

        _httpClient.BaseAddress = apiEndpoint;

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

        var uri = new Uri(BuildImageQuery(query, safeSearch, country, size, type, layout, color, license), UriKind.Relative);

        using var stream = await _httpClient.GetStreamAsync(uri).ConfigureAwait(false);

        var response = (await JsonSerializer.DeserializeAsync(stream, BraveImageSearchResponseContext.Default.BraveImageSearchResponse).ConfigureAwait(false))!;

        return Array.AsReadOnly(response.Results);
    }

    private static string BuildImageQuery(string query, SafeSearchLevel safeSearch, string? country, BraveImageSize size, BraveImageType type,
        BraveImageLayout layout, BraveImageColor color, BraveImageLicense license)
    {
        string url = $"images?q={Uri.EscapeDataString(query)}";

        if (safeSearch != SafeSearchLevel.Moderate)
            url += $"&safesearch={safeSearch.ToString().ToLowerInvariant()}";

        if (!string.IsNullOrEmpty(country))
            url += $"&country={country}";

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