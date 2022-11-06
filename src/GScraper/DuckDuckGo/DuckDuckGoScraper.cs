using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GScraper.DuckDuckGo;

// TODO: Add support for cancellation tokens and regular search method

/// <summary>
/// Represents a DuckDuckGo scraper.
/// </summary>
public class DuckDuckGoScraper : IDisposable
{
    /// <summary>
    /// Returns the default API endpoint.
    /// </summary>
    public const string DefaultApiEndpoint = "https://duckduckgo.com";

    /// <summary>
    /// Returns the maximum query length.
    /// </summary>
    public const int MaxQueryLength = 500;

    private static ReadOnlySpan<byte> TokenStart => new[] { (byte)'v', (byte)'q', (byte)'d', (byte)'=', (byte)'\'' };

    private const string _defaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36";
    private static  readonly Uri _defaultBaseAddress = new(DefaultApiEndpoint);

    private readonly HttpClient _httpClient;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="DuckDuckGoScraper"/> class.
    /// </summary>
    public DuckDuckGoScraper()
        : this(new HttpClient())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DuckDuckGoScraper"/> class using the provided <see cref="HttpClient"/>.
    /// </summary>
    public DuckDuckGoScraper(HttpClient client)
    {
        _httpClient = client;
        Init(_httpClient, _defaultBaseAddress);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DuckDuckGoScraper"/> class using the provided <see cref="HttpClient"/> and API endpoint.
    /// </summary>
    [Obsolete("This constructor is deprecated and it will be removed in a future version. Use DuckDuckGoScraper(HttpClient) instead.")]
    public DuckDuckGoScraper(HttpClient client, string apiEndpoint)
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

        _httpClient.DefaultRequestHeaders.Referrer ??= _httpClient.BaseAddress;
    }

    /// <summary>
    /// Gets images from DuckDuckGo.
    /// </summary>
    /// <remarks>This method returns at most 100 image results.</remarks>
    /// <param name="query">The search query.</param>
    /// <param name="safeSearch">The safe search level.</param>
    /// <param name="time">The image time.</param>
    /// <param name="size">The image size.</param>
    /// <param name="color">The image color.</param>
    /// <param name="type">The image type.</param>
    /// <param name="layout">The image layout.</param>
    /// <param name="license">The image license.</param>
    /// <param name="region">The region. <see cref="DuckDuckGoRegions"/> contains the regions that can be used here.</param>
    /// <returns>A task representing the asynchronous operation. The result contains an <see cref="IEnumerable{T}"/> of <see cref="DuckDuckGoImageResult"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="query"/> is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="query"/> is larger than <see cref="MaxQueryLength"/>.</exception>
    /// <exception cref="GScraperException">An error occurred during the scraping process.</exception>
    public async Task<IEnumerable<DuckDuckGoImageResult>> GetImagesAsync(string query, SafeSearchLevel safeSearch = SafeSearchLevel.Moderate,
        DuckDuckGoImageTime time = DuckDuckGoImageTime.Any, DuckDuckGoImageSize size = DuckDuckGoImageSize.All, DuckDuckGoImageColor color = DuckDuckGoImageColor.All,
        DuckDuckGoImageType type = DuckDuckGoImageType.All, DuckDuckGoImageLayout layout = DuckDuckGoImageLayout.All, DuckDuckGoImageLicense license = DuckDuckGoImageLicense.All,
        string region = DuckDuckGoRegions.UsEnglish)
    {
        GScraperGuards.NotNull(query, nameof(query));
        GScraperGuards.NotNullOrEmpty(region, nameof(region));
        GScraperGuards.ArgumentInRange(query.Length, MaxQueryLength, nameof(query), $"The query cannot be larger than {MaxQueryLength}.");

        string token = await GetTokenAsync(query).ConfigureAwait(false);
        var uri = new Uri(BuildImageQuery(token, query, safeSearch, time, size, color, type, layout, license, region), UriKind.Relative);

        using var stream = await _httpClient.GetStreamAsync(uri).ConfigureAwait(false);

        var response = (await JsonSerializer.DeserializeAsync(stream, DuckDuckGoImageSearchResponseContext.Default.DuckDuckGoImageSearchResponse).ConfigureAwait(false))!;

        return Array.AsReadOnly(response.Results);
    }

    private static string BuildImageQuery(string token, string query, SafeSearchLevel safeSearch, DuckDuckGoImageTime time, DuckDuckGoImageSize size,
        DuckDuckGoImageColor color, DuckDuckGoImageType type, DuckDuckGoImageLayout layout, DuckDuckGoImageLicense license, string region)
    {
        string url = $"i.js?l={region}" +
                     "&o=json" +
                     $"&q={Uri.EscapeDataString(query)}" +
                     $"&vqd={token}" +
                     "&f=";

        url += time == DuckDuckGoImageTime.Any ? ',' : $"time:{time},";
        url += size == DuckDuckGoImageSize.All ? ',' : $"size:{size},";
        url += color == DuckDuckGoImageColor.All ? ',' : $"color:{color.ToString().ToLowerInvariant()},";
        url += type == DuckDuckGoImageType.All ? ',' : $"type:{type},";
        url += layout == DuckDuckGoImageLayout.All ? ',' : $"layout:{layout},";
        url += license == DuckDuckGoImageLicense.All ? "" : $"license:{license}";
        url += $"&p={(safeSearch == SafeSearchLevel.Off ? "-1" : "1")}";

        return url;
    }
    
    private async Task<string> GetTokenAsync(string query)
    {
        byte[] bytes = await _httpClient.GetByteArrayAsync(new Uri($"?q={Uri.EscapeDataString(query)}", UriKind.Relative)).ConfigureAwait(false);
        return GetToken(bytes);
    }
    
    private static string GetToken(ReadOnlySpan<byte> rawHtml)
    {
        int startIndex = rawHtml.IndexOf(TokenStart);

        if (startIndex == -1)
        {
            throw new GScraperException("Failed to get the DuckDuckGo token.", "DuckDuckGo");
        }

        var sliced = rawHtml.Slice(startIndex + TokenStart.Length);
        int endIndex = sliced.IndexOf((byte)'\'');

        if (endIndex == -1)
        {
            throw new GScraperException("Failed to get the DuckDuckGo token.", "DuckDuckGo");
        }

#if NETSTANDARD2_1_OR_GREATER
        return Encoding.UTF8.GetString(sliced.Slice(0, endIndex));
#else
        return Encoding.UTF8.GetString(sliced.Slice(0, endIndex).ToArray());
#endif
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