using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace GScraper.Google;

// TODO: Add support for cancellation tokens and regular search method

/// <summary>
/// Represents a Google Search scraper.
/// </summary>
[PublicAPI]
public class GoogleScraper : IDisposable
{
    /// <summary>
    /// Returns the default API endpoint.
    /// </summary>
    public const string DefaultApiEndpoint = "https://www.google.com/search";

    private const string DefaultUserAgent = "NSTN/3.60.474802233.release Dalvik/2.1.0 (Linux; U; Android 12) gzip";
    private static readonly Uri DefaultBaseAddress = new(DefaultApiEndpoint);

    private readonly HttpClient _httpClient;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="GoogleScraper"/> class.
    /// </summary>
    public GoogleScraper()
        : this(new HttpClient())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GoogleScraper"/> class using the provided <see cref="HttpClient"/>.
    /// </summary>
    public GoogleScraper(HttpClient client)
    {
        GScraperGuards.NotNull(client);

        _httpClient = client;
        _httpClient.BaseAddress = DefaultBaseAddress;

        var headers = _httpClient.DefaultRequestHeaders;
        if (headers.UserAgent.Count == 0)
        {
            headers.UserAgent.ParseAdd(DefaultUserAgent);
        }

        if (headers.Accept.Count == 0)
        {
            headers.Accept.ParseAdd("*/*");
        }
    }

    /// <summary>
    /// Gets images from Google Images.
    /// </summary>
    /// <remarks>This method returns at most 100 image results.</remarks>
    /// <param name="query">The search query.</param>
    /// <param name="safeSearch">The safe search level.</param>
    /// <param name="size">The image size.</param>
    /// <param name="color">The image color. <see cref="GoogleImageColors"/> contains the colors that can be used here.</param>
    /// <param name="type">The image type.</param>
    /// <param name="time">The image time.</param>
    /// <param name="license">The image license. <see cref="GoogleImageLicenses"/> contains the licenses that can be used here.</param>
    /// <param name="language">The language code to use. <see cref="GoogleLanguages"/> contains the language codes that can be used here.</param>
    /// <returns>A task representing the asynchronous operation. The result contains an <see cref="IEnumerable{T}"/> of <see cref="GoogleImageResult"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="query"/> is null or empty.</exception>
    /// <exception cref="GScraperException">An error occurred during the scraping process.</exception>
    public async Task<IEnumerable<GoogleImageResult>> GetImagesAsync(string query, SafeSearchLevel safeSearch = SafeSearchLevel.Off, GoogleImageSize size = GoogleImageSize.Any,
        string? color = null, GoogleImageType type = GoogleImageType.Any, GoogleImageTime time = GoogleImageTime.Any,
        string? license = null, string? language = null)
    {
        // TODO: Use pagination
        GScraperGuards.NotNull(query);

        var uri = new Uri(BuildImageQuery(query, safeSearch, size, color, type, time, license, language), UriKind.Relative);
        byte[] bytes = await _httpClient.GetByteArrayAsync(uri).ConfigureAwait(false);

        var images = JsonSerializer.Deserialize(bytes.AsSpan(5, bytes.Length - 5), GoogleImageSearchResponseContext.Default.GoogleImageSearchResponse)!.Ischj.Metadata;
        images?.RemoveAll(static x => !x.Url.StartsWith("http", StringComparison.Ordinal));

        return images is null ? Array.Empty<GoogleImageResultModel>() : images.AsReadOnly();
    }

    private static string BuildImageQuery(string query, SafeSearchLevel safeSearch, GoogleImageSize size, string? color,
        GoogleImageType type, GoogleImageTime time, string? license, string? language)
    {
        string hl = string.IsNullOrEmpty(language) ? "en" : language!;
        string lr = string.IsNullOrEmpty(language) ? "" : $"lang_{language}";

        string url = $"?q={Uri.EscapeDataString(query)}&tbm=isch&hl={hl}&lr={lr}&cr=&ie=utf8&oe=utf8&asearch=isch&async=_fmt:json,p:1,ijn:0&tbs=";

        url += size == GoogleImageSize.Any ? ',' : $"isz:{(char)size},";
        url += string.IsNullOrEmpty(color) ? ',' : $"ic:{color},";
        url += type == GoogleImageType.Any ? ',' : $"itp:{type.ToString().ToLowerInvariant()},";
        url += time == GoogleImageTime.Any ? ',' : $"qdr:{(char)time},";
        url += string.IsNullOrEmpty(license) ? "" : $"il:{license}";

        url += "&safe=" + safeSearch switch
        {
            SafeSearchLevel.Off => "off",
            _ => "active"
        };

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