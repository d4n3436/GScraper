using AngleSharp.Html.Parser;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Net.Http;
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

    private const string DefaultUserAgent = "Opera/12.02 (Android 4.1; Linux; Opera Mobi/ADR-1111101157; U; en-US) Presto/2.9.201 Version/12.02";
    private const string ThumbnailEndpoint = "https://encrypted-tbn0.gstatic.com/images?q=tbn:";
    private static readonly Uri _defaultBaseAddress = new(DefaultApiEndpoint); 

    private readonly HtmlParser _htmlParser = new();
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
        _httpClient.BaseAddress = _defaultBaseAddress;

        if (_httpClient.DefaultRequestHeaders.UserAgent.Count == 0)
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(DefaultUserAgent);
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
    /// <exception cref="ArgumentNullException"><paramref name="query"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="GScraperException">An error occurred during the scraping process.</exception>
    public async Task<IEnumerable<GoogleImageResult>> GetImagesAsync(string query, SafeSearchLevel safeSearch = SafeSearchLevel.Off, GoogleImageSize size = GoogleImageSize.Any,
        string? color = null, GoogleImageType type = GoogleImageType.Any, GoogleImageTime time = GoogleImageTime.Any,
        string? license = null, string? language = null)
    {
        GScraperGuards.NotNull(query);

        var uri = new Uri(BuildImageQuery(query, safeSearch, size, color, type, time, license, language), UriKind.Relative);
        using var stream = await _httpClient.GetStreamAsync(uri).ConfigureAwait(false);
        using var document = await _htmlParser.ParseDocumentAsync(stream).ConfigureAwait(false);

        var elements = document.GetElementsByClassName("isv-r");
        var images = new List<GoogleImageResult>(elements.Length);

        foreach (var element in elements)
        {
            string? url = element.GetAttribute("data-ou");
            if (url is null || !url.StartsWith("http", StringComparison.Ordinal))
                continue;

            int width = int.Parse(element.GetAttribute("data-ow")!);
            int height = int.Parse(element.GetAttribute("data-oh")!);

            string sourceUrl = element.GetAttribute("data-ru") ?? "";
            string? thumbnailId = element.GetAttribute("data-tbnid");

            images.Add(new GoogleImageResult(
                url: url,
                title: element.GetAttribute("data-pt") ?? "",
                width: width,
                height: height,
                color: null,
                displayUrl: Uri.TryCreate(sourceUrl, UriKind.Absolute, out var sourceUri) ? sourceUri.Host : "",
                sourceUrl: sourceUrl,
                thumbnailUrl: thumbnailId is null ? "" : $"{ThumbnailEndpoint}{thumbnailId}"));
        }

        // Google returns a page that requires JavaScript when it stops serving the legacy page to the user agent
        if (images.Count == 0 && document.QuerySelector("meta[content*='/httpservice/retry/enablejs']") is not null)
        {
            throw new GScraperException("Failed to get the image results. Google returned a page that requires JavaScript.", "Google");
        }

        return images.AsReadOnly();
    }

    private static string BuildImageQuery(string query, SafeSearchLevel safeSearch, GoogleImageSize size, string? color,
        GoogleImageType type, GoogleImageTime time, string? license, string? language)
    {
        string url = $"?q={Uri.EscapeDataString(query)}&tbm=isch&ie=UTF-8&oe=UTF-8&tbs=";

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