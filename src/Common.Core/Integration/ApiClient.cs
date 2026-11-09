using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Common.Core.Integration;

public class ApiClient : IDisposable
{
    private bool _disposed = false;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonConfiguration.GetOptions())
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    protected HttpClient HttpClient { get; private set; }

    public ApiClient(
        IHttpClientFactory httpClientFactory,
        CorrelationIdAccessor correlationIdAccessor,
        string? bearerToken = null
)
    {
        HttpClient = httpClientFactory.CreateClient();
        HttpClient.DefaultRequestHeaders.Add("X-Correlation-ID", correlationIdAccessor.CorrelationId);

        if (bearerToken is not null)
            SetAuthorizationBearerToken(bearerToken);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                HttpClient?.Dispose();
            }

            _disposed = true;
        }
    }

    public void SetAuthorizationBearerToken(string bearerToken)
    {
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
    }

    public Task PostAsync(string url, CancellationToken ct)
        => PostAsync(dto: null, url, ct);

    public async Task<TContent> PostAsync<TContent>(object dto, string url, CancellationToken ct)
    {
        var response = await PostAsync(dto, url, ct);
        return await GetJsonContentAsync<TContent>(response, ct);
    }

    public async Task<HttpResponseMessage> PostAsync(object? dto, string url, CancellationToken ct)
    {
        var response = await SendAsync(CreateRequest(HttpMethod.Post, url, dto), ct);
        await EnsureSuccessAsync(response);
        return response;
    }

    public async Task<TContent> GetAsync<TContent>(string url, CancellationToken ct)
    {
        var response = await SendAsync(CreateRequest(HttpMethod.Get, url), ct);
        await EnsureSuccessAsync(response);
        return await GetJsonContentAsync<TContent>(response, ct);
    }

    public async Task<string> GetAsync(string url, CancellationToken ct)
    {
        var response = await SendAsync(CreateRequest(HttpMethod.Get, url), ct);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadAsStringAsync(ct);
    }

    public async Task<Stream> GetStreamAsync(string url, CancellationToken ct)
    {
        var response = await SendAsync(CreateRequest(HttpMethod.Get, url), ct);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadAsStreamAsync(ct);
    }

    public async Task<(string fileName, Stream content)> GetFileAsync(string url, CancellationToken ct)
    {
        var response = await SendAsync(CreateRequest(HttpMethod.Get, url), ct);
        await EnsureSuccessAsync(response);
        return (response.Content.Headers.ContentDisposition?.FileName ?? "downloaded", await response.Content.ReadAsStreamAsync(ct));
    }

    public async Task<TContent> PutAsync<TContent>(object dto, string url, CancellationToken ct)
    {
        var response = await PutAsync(dto, url, ct);
        return await GetJsonContentAsync<TContent>(response, ct);
    }

    public async Task<HttpResponseMessage> PutAsync(object dto, string url, CancellationToken ct)
    {
        var response = await SendAsync(CreateRequest(HttpMethod.Put, url, dto), ct);
        await EnsureSuccessAsync(response);
        return response;
    }

    public Task DeleteAsync(string url, CancellationToken ct)
        => DeleteAsync(dto: null, url: url, ct: ct);

    public async Task DeleteAsync(object? dto, string url, CancellationToken ct)
    {
        var response = await SendAsync(CreateRequest(HttpMethod.Delete, url, dto), ct);

        await EnsureSuccessAsync(response);
    }

    private static HttpRequestMessage CreateRequest(HttpMethod httpMethod, string url, object? dto = null)
    {
        var rm = new HttpRequestMessage(httpMethod, url);
        if (dto is not null)
            rm.Content = CreateJsonContent(dto);

        return rm;
    }

    private static StringContent CreateJsonContent(object dto)
    {
        var json = JsonSerializer.Serialize(dto, _jsonOptions);
        return new StringContent(
            mediaType: MediaTypeNames.Application.Json,
            encoding: Encoding.UTF8,
            content: json
        );
    }

    private static async Task<TContent> GetJsonContentAsync<TContent>(HttpResponseMessage response, CancellationToken ct)
    {
        string? json = null;
        try
        {
            json = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<TContent>(json, _jsonOptions)
                ?? throw new ResponseDeserializationException(json);
        }
        catch (Exception ex)
        {
            throw new ResponseDeserializationException(json, ex);
        }
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage responseMessage)
    {
        if (!responseMessage.IsSuccessStatusCode)
        {
            var body = await responseMessage.Content.ReadAsStringAsync();
            throw new NonSuccessResponseException(responseMessage.StatusCode, body, responseMessage.RequestMessage?.RequestUri?.AbsoluteUri);
        }
    }

    protected virtual Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        => HttpClient.SendAsync(request, ct);
}