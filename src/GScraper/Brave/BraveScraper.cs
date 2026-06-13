using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace GScraper.Brave;

// TODO: Add support for cancellation tokens and regular search method

/// <summary>
/// Represents a Brave Search scraper.
/// </summary>
[PublicAPI]
public class BraveScraper : IDisposable
{
    /// <summary>
    /// Returns the default API endpoint.
    /// </summary>
    public const string DefaultApiEndpoint = "https://search.brave.com/";

    private const string DefaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36";
    private static readonly Uri DefaultBaseAddress = new(DefaultApiEndpoint);

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
        GScraperGuards.NotNull(client);

        _httpClient = client;
        _httpClient.BaseAddress = DefaultBaseAddress;

        var headers = _httpClient.DefaultRequestHeaders;
        if (headers.UserAgent.Count == 0)
            headers.UserAgent.ParseAdd(DefaultUserAgent);

        if (headers.Accept.Count == 0)
            headers.Accept.ParseAdd("application/json");

        if (headers.AcceptLanguage.Count == 0)
            headers.AcceptLanguage.ParseAdd("en-US,en;q=0.5");
    }

    /// <summary>
    /// Gets images from Brave Search.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="safeSearch">The safe search level.</param>
    /// <param name="country">The country. <see cref="BraveCountries"/> contains the countries that can be used here.</param>
    /// <returns>A task representing the asynchronous operation. The result contains an <see cref="IEnumerable{T}"/> of <see cref="BraveImageResult"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="query"/> is null or empty.</exception>
    /// <exception cref="GScraperException">An error occurred during the scraping process.</exception>
    public async Task<IEnumerable<BraveImageResult>> GetImagesAsync(string query, SafeSearchLevel safeSearch = SafeSearchLevel.Moderate, string? country = null)
    {
        GScraperGuards.NotNull(query);

        string safeSearchValue = safeSearch switch
        {
            SafeSearchLevel.Off => "off",
            SafeSearchLevel.Strict => "strict",
            _ => "moderate"
        };

        string uri = $"images/__data.json?q={Uri.EscapeDataString(query)}&safesearch={safeSearchValue}&useLocation=false&source=web";
        if (!string.IsNullOrEmpty(country))
            uri += $"&country={Uri.EscapeDataString(country)}";

        using var stream = await _httpClient.GetStreamAsync(new Uri(uri, UriKind.Relative)).ConfigureAwait(false);
        using var doc = await JsonDocument.ParseAsync(stream).ConfigureAwait(false);

        // nodes[0] = layout data, nodes[1] = page data (image results)
        var pageData = doc.RootElement.GetProperty("nodes"u8)[1].GetProperty("data"u8);

        var root = Deobfuscate(pageData[0], pageData);
        var pageRoot = root!.Deserialize(BraveImageSearchResponseContext.Default.BravePageRoot)!;

        var resultModels = pageRoot.Body.Response.Results;
        if (resultModels.Length == 0)
            return [];

        var results = new List<BraveImageResult>(resultModels.Length);
        foreach (var result in resultModels)
        {
            string? url = result.Properties?.Url;
            if (url is null)
                continue;

            results.Add(new BraveImageResult(
                url: url,
                title: result.Title ?? "",
                width: result.Properties?.Width ?? 0,
                height: result.Properties?.Height ?? 0,
                sourceUrl: result.Url ?? "",
                source: result.Source ?? "",
                thumbnailUrl: result.Thumbnail?.Src ?? "",
                resizedUrl: result.Properties?.Resized ?? ""
            ));
        }

        return results.AsReadOnly();
    }

    private static JsonNode? Deobfuscate(JsonElement current, JsonElement root)
    {
        if (current.ValueKind == JsonValueKind.Object)
        {
            var obj = new JsonObject();
            foreach (var property in current.EnumerateObject())
            {
                int idx = property.Value.GetInt32();
                obj.Add(property.Name, idx < 0 ? null : Deobfuscate(root[idx], root));
            }

            return obj;
        }

        if (current.ValueKind == JsonValueKind.Array)
        {
            var array = new JsonArray();
            foreach (var element in current.EnumerateArray())
            {
                int idx = element.GetInt32();
                array.Add(idx < 0 ? null : Deobfuscate(root[idx], root));
            }

            return array;
        }

        return JsonValue.Create(current);
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