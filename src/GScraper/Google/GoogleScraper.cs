using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GScraper.Google;

/// <summary>
/// Represents a Google Search scraper.
/// </summary>
public class GoogleScraper : IDisposable
{
    /// <summary>
    /// Returns the default API endpoint.
    /// </summary>
    public const string DefaultApiEndpoint = "https://www.google.com/search";

    private static ReadOnlySpan<byte> CallbackStart => Encoding.UTF8.GetBytes("AF_initDataCallback({key: 'ds:1'");
    private static ReadOnlySpan<byte> CallbackEnd => Encoding.UTF8.GetBytes(", sideChannel: {}});</script>");

    private readonly HttpClient _httpClient;
    private const string _defaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.114 Safari/537.36";
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="GoogleScraper"/> class.
    /// </summary>
    public GoogleScraper()
        : this(new HttpClient(new HttpClientHandler { UseCookies = false })) // Disable cookies so we can set the consent cookie in the request.
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GoogleScraper"/> class using the provided <see cref="HttpClient"/>.
    /// </summary>
    public GoogleScraper(HttpClient client)
    {
        _httpClient = client;
        Init(_httpClient, DefaultApiEndpoint);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GoogleScraper"/> class using the provided <see cref="HttpClient"/> and API endpoint.
    /// </summary>
    [Obsolete("This constructor is deprecated and it will be removed in a future version. Use GoogleScraper(HttpClient) instead.")]
    public GoogleScraper(HttpClient client, string apiEndpoint)
    {
        _httpClient = client;
        Init(_httpClient, apiEndpoint);
    }

    private void Init(HttpClient client, string apiEndpoint)
    {
        GScraperGuards.NotNull(client, nameof(client));
        GScraperGuards.NotNullOrEmpty(apiEndpoint, nameof(apiEndpoint));

        _httpClient.BaseAddress = new Uri(apiEndpoint);

        if (_httpClient.DefaultRequestHeaders.UserAgent.Count == 0)
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_defaultUserAgent);
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
        GScraperGuards.NotNull(query, nameof(query));

        var uri = new Uri(BuildImageQuery(query, safeSearch, size, color, type, time, license, language), UriKind.Relative);

        using var request = new HttpRequestMessage(HttpMethod.Get, uri);

        // Set the CONSENT cookie in the request to bypass the cookie consent page.
        // This might now work if the scraper is instantiated with a HttpClient handler that has cookies enabled.
        // On newer version of .NET this cookie will be added regardless of the setting mentioned above.
        request.Headers.Add("Cookie", "CONSENT=YES+");

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        byte[] page = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

        var rawImages = ExtractDataPack(page);

        return EnumerateResults(rawImages);
    }

    private static IEnumerable<GoogleImageResult> EnumerateResults(JsonElement rawImages)
    {
        foreach (var rawImage in rawImages.EnumerateArray())
        {
            if (rawImage[0].GetInt32() != 1)
            {
                continue;
            }

            var image = FormatImageObject(rawImage);

            if (image != null)
            {
                yield return image;
            }
        }
    }

    private static JsonElement ExtractDataPack(byte[] page)
    {
        // Extract the JSON data pack from the page.
        var span = page.AsSpan();

        int callbackStartIndex = span.IndexOf(CallbackStart);
        if (callbackStartIndex == -1)
        {
            throw new GScraperException("Failed to extract the data pack.", "Google");
        }

        int start = span.Slice(callbackStartIndex).IndexOf((byte)'[');
        if (start == -1)
        {
            throw new GScraperException("Failed to extract the data pack.", "Google");
        }

        start += callbackStartIndex;

        int callbackEndIndex = span.Slice(start).IndexOf(CallbackEnd);
        if (callbackEndIndex == -1)
        {
            throw new GScraperException("Failed to extract the data pack.", "Google");
        }

        int end = span.Slice(0, callbackEndIndex + start).LastIndexOf((byte)']') + 1;
        if (end == -1)
        {
            throw new GScraperException("Failed to extract the data pack.", "Google");
        }

        var rawObject = page.AsMemory(start, end - start);

        try
        {
            return JsonDocument.Parse(rawObject).RootElement[31].Last()[12][2];
        }
        catch (JsonException e)
        {
            throw new GScraperException("Failed to unpack the image object data.", "Google", e);
        }
    }

    private static GoogleImageResult? FormatImageObject(in JsonElement element)
    {
        var data = element[1];
        if (data.ValueKind != JsonValueKind.Array)
            return null;

        var main = data[3];
        var info = data[9];

        if (info.ValueKind != JsonValueKind.Object)
            info = data[11];

        string url = main[0].GetString() ?? string.Empty;

        string title = info
            .GetProperty("2003")[3]
            .GetString() ?? string.Empty;

        int width = main[2].GetInt32();

        int height = main[1].GetInt32();

        string displayUrl = info
            .GetProperty("2003")[17]
            .GetString() ?? string.Empty;

        string sourceUrl = info
            .GetProperty("2003")[2]
            .GetString() ?? string.Empty;

        string thumbnailUrl = data[2][0]
            .GetString() ?? string.Empty;

        return new GoogleImageResult(url, title, width, height, displayUrl, sourceUrl, thumbnailUrl);
    }

    private static string BuildImageQuery(string query, SafeSearchLevel safeSearch, GoogleImageSize size, string? color,
        GoogleImageType type, GoogleImageTime time, string? license, string? language)
    {
        string url = $"?q={Uri.EscapeDataString(query)}&tbs=";

        url += size == GoogleImageSize.Any ? ',' : $"isz:{(char)size},";
        url += string.IsNullOrEmpty(color) ? ',' : $"ic:{color},";
        url += type == GoogleImageType.Any ? ',' : $"itp:{type.ToString().ToLowerInvariant()},";
        url += time == GoogleImageTime.Any ? ',' : $"qdr:{(char)time},";
        url += string.IsNullOrEmpty(license) ? "" : $"il:{license}";

        url += "&site=webhp" +
               "&source=lnms" +
               "&tbm=isch" +
               "&sa=X";

        url += "&safe=" + safeSearch switch
        {
            SafeSearchLevel.Off => "off",
            SafeSearchLevel.Moderate => "medium",
            SafeSearchLevel.Strict => "high",
            _ => throw new ArgumentException("Invalid safe search level.", nameof(safeSearch))
        };

        if (!string.IsNullOrEmpty(language))
            url += $"&lr=lang_{language}&hl={language}";

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