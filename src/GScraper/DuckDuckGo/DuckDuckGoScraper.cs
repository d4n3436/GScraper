using JetBrains.Annotations;
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
[PublicAPI]
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

    private static ReadOnlySpan<byte> TokenStart => "vqd=\""u8;

    private const string DefaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36";
    private static readonly Uri _defaultBaseAddress = new(DefaultApiEndpoint);

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
        GScraperGuards.NotNull(_httpClient);

        _httpClient = client;
        _httpClient.BaseAddress = _defaultBaseAddress;

        var headers = _httpClient.DefaultRequestHeaders;
        if (headers.UserAgent.Count == 0)
        {
            headers.UserAgent.ParseAdd(DefaultUserAgent);
        }

        headers.Referrer ??= _httpClient.BaseAddress;

        if (headers.Accept.Count == 0)
        {
            headers.Accept.ParseAdd("*/*");
        }

        if (headers.AcceptLanguage.Count == 0)
        {
            headers.AcceptLanguage.ParseAdd("en-US,en;q=0.5");
        }
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
        GScraperGuards.NotNull(query);
        GScraperGuards.NotNullOrEmpty(region);
        GScraperGuards.ArgumentInRange(query.Length, MaxQueryLength, nameof(query), $"The query cannot be larger than {MaxQueryLength}.");

        string token = await GetTokenAsync(query).ConfigureAwait(false);
        var uri = new Uri(BuildImageQuery(token, query, safeSearch, time, size, color, type, layout, license, region), UriKind.Relative);

        using var imageRequest = new HttpRequestMessage(HttpMethod.Get, uri);
        imageRequest.Headers.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
        imageRequest.Headers.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
        imageRequest.Headers.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");

        using var imageResponse = await _httpClient.SendAsync(imageRequest, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        imageResponse.EnsureSuccessStatusCode();

        using var stream = await imageResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);
        var response = (await JsonSerializer.DeserializeAsync(stream, DuckDuckGoImageSearchResponseContext.Default.DuckDuckGoImageSearchResponse).ConfigureAwait(false))!;

        return Array.AsReadOnly(response.Results);
    }

    private static string BuildImageQuery(string token, string query, SafeSearchLevel safeSearch, DuckDuckGoImageTime time, DuckDuckGoImageSize size,
        DuckDuckGoImageColor color, DuckDuckGoImageType type, DuckDuckGoImageLayout layout, DuckDuckGoImageLicense license, string region)
    {
        string[] regionParts = region.Split('-');
        string ct = regionParts.Length >= 2 ? regionParts[^1].ToUpperInvariant() : "EN";
        if (ct == "WT") ct = "EN";

        string url = "i.js?" +
                     "o=json" +
                     $"&q={Uri.EscapeDataString(query)}" +
                     "&u=bing" +
                     $"&l={region}" +
                     "&bpia=1" +
                     $"&vqd={token}" +
                     "&a=h_" +
                     $"&ct={ct}";

        string filters = BuildFilters(time, size, color, type, layout, license);
        if (filters.Length > 0)
            url += $"&f={filters}";

        if (safeSearch != SafeSearchLevel.Moderate)
            url += $"&p={(safeSearch == SafeSearchLevel.Off ? "-2" : "1")}";

        return url;
    }

    private static string BuildFilters(DuckDuckGoImageTime time, DuckDuckGoImageSize size, DuckDuckGoImageColor color,
        DuckDuckGoImageType type, DuckDuckGoImageLayout layout, DuckDuckGoImageLicense license)
    {
        string filter = "";
        if (time != DuckDuckGoImageTime.Any) filter += $"time:{time},";
        if (size != DuckDuckGoImageSize.All) filter += $"size:{size},";
        if (color != DuckDuckGoImageColor.All) filter += $"color:{color.ToString().ToLowerInvariant()},";
        if (type != DuckDuckGoImageType.All) filter += $"type:{type},";
        if (layout != DuckDuckGoImageLayout.All) filter += $"layout:{layout},";
        if (license != DuckDuckGoImageLicense.All) filter += $"license:{license},";
        return filter.TrimEnd(',');
    }

    private async Task<string> GetTokenAsync(string query)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"?q={Uri.EscapeDataString(query)}&iar=images&t=h_", UriKind.Relative));
        request.Headers.TryAddWithoutValidation("Sec-Fetch-Dest", "document");
        request.Headers.TryAddWithoutValidation("Sec-Fetch-Mode", "navigate");
        request.Headers.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");
        request.Headers.TryAddWithoutValidation("Sec-Fetch-User", "?1");

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        byte[] bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        return GetToken(bytes);
    }

    private static string GetToken(ReadOnlySpan<byte> rawHtml)
    {
        int startIndex = rawHtml.IndexOf(TokenStart);

        if (startIndex == -1)
        {
            throw new GScraperException("Failed to get the DuckDuckGo token.", "DuckDuckGo");
        }

        var sliced = rawHtml[(startIndex + TokenStart.Length)..];
        int endIndex = sliced.IndexOf((byte)'"');

        if (endIndex == -1)
        {
            throw new GScraperException("Failed to get the DuckDuckGo token.", "DuckDuckGo");
        }

#if NETSTANDARD2_1_OR_GREATER || NET8_0_OR_GREATER
        return Encoding.UTF8.GetString(sliced[..endIndex]);
#else
        return Encoding.UTF8.GetString(sliced[..endIndex].ToArray());
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