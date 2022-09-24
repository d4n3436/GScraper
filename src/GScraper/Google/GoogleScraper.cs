using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
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

    private const string _defaultUserAgent = "NSTN/3.62.475170463.release Dalvik/2.1.0 (Linux; U; Android 12) Mobile";
    private static readonly Uri _defaultBaseAddress = new(DefaultApiEndpoint);

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
        _httpClient = client;
        Init(_httpClient, _defaultBaseAddress);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GoogleScraper"/> class using the provided <see cref="HttpClient"/> and API endpoint.
    /// </summary>
    [Obsolete("This constructor is deprecated and it will be removed in a future version. Use GoogleScraper(HttpClient) instead.")]
    public GoogleScraper(HttpClient client, string apiEndpoint)
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
        byte[] bytes = await _httpClient.GetByteArrayAsync(uri).ConfigureAwait(false);

        using var document = JsonDocument.Parse(bytes.AsMemory(5, bytes.Length - 5));

        bool any = document
            .RootElement
            .GetProperty("ischj")
            .TryGetProperty("metadata", out var rawResults);

        if (!any)
        {
            return Array.Empty<GoogleImageResult>();
        }

        int length = rawResults.GetArrayLength();
        var results = new GoogleImageResult[length];

        for (int i = 0; i < length; i++)
        {
            results[i] = GetImageResult(rawResults[i]);
        }

        return Array.AsReadOnly(results);
    }

    private static GoogleImageResult GetImageResult(in JsonElement element)
    {
        var result = element.GetProperty("result");
        var originalImage = element.GetProperty("original_image");

        var color = GetColor(element.GetProperty("background_color").GetString().AsSpan());
        int height = originalImage.GetProperty("height").GetInt32();
        string url = originalImage.GetProperty("url").GetString() ?? throw new GScraperException("Unable to get the URL of the image result.", "Google");
        int width = originalImage.GetProperty("width").GetInt32();
        string sourceUrl = result.GetProperty("referrer_url").GetString() ?? throw new GScraperException("Unable to get the source URL of the image result.", "Google");
        string displayUrl = result.GetProperty("image_source_url").GetString() ?? throw new GScraperException("Unable to get the display URL of the image result.", "Google");
        string title = result.GetProperty("page_title").GetString() ?? throw new GScraperException("Unable to get the title of the image result.", "Google");
        string thumbnailUrl = element.GetProperty("thumbnail").GetProperty("url").GetString() ?? throw new GScraperException("Unable to get the thumbnail URL of the image result.", "Google");

        return new GoogleImageResult(url, title, width, height, color, displayUrl, sourceUrl, thumbnailUrl);
    }

    private static Color? GetColor(ReadOnlySpan<char> rgb)
    {
        const string start = "rgb(";

        if (!rgb.StartsWith(start.AsSpan())) return default;

        rgb = rgb.Slice(start.Length);

        // R
        int index = rgb.IndexOf(',');
        if (index == -1 || !TryParseInt(rgb.Slice(0, index), out int r)) return default;

        rgb = rgb.Slice(index + 1);

        // G
        index = rgb.IndexOf(',');
        if (index == -1 || !TryParseInt(rgb.Slice(0, index), out int g)) return default;

        rgb = rgb.Slice(index + 1);

        // B
        index = rgb.IndexOf(')');
        if (index == -1 || !TryParseInt(rgb.Slice(0, index), out int b)) return default;

        return Color.FromArgb(r, g, b);
    }

    private static bool TryParseInt(ReadOnlySpan<char> s, out int result)
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        return int.TryParse(s, out result);
#else
        return int.TryParse(s.ToString(), out result);
#endif
    }

    private static string BuildImageQuery(string query, SafeSearchLevel safeSearch, GoogleImageSize size, string? color,
        GoogleImageType type, GoogleImageTime time, string? license, string? language)
    {
        string url = $"?q={Uri.EscapeDataString(query)}&tbm=isch&asearch=isch&async=_fmt:json,p:1&tbs=";

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