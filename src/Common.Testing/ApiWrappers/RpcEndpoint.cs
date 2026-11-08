using Medusa.Enap.Common.Core.Configuration;
using Medusa.Enap.Common.Core.Messaging;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http.Json;

namespace Medusa.Enap.Common.Testing.ApiWrappers;

/// <summary>
/// RpcEndpoint wrapper without request/response DTOs
/// </summary>
public class RpcEndpoint(string url, HttpClient client)
{
    public string Url => url;

    public HttpClient Client => client;

    /// <summary>
    /// Represents a READ-ONLY remote procedure call without a request body.
    /// </summary>
    public Task<HttpResponseMessage> GetRawAsync(QueryString? queryString)
        => client.GetAsync($"{url}{queryString}", CancellationToken.None);

    public Task<HttpResponseMessage> PostRawAsync()
        => client.PostAsync(url, content: null, CancellationToken.None);

    public Task<HttpResponseMessage> PostRawAsync(object? dto)
        => PostRawAsync(null, dto);

    public Task<HttpResponseMessage> PostRawAsync(QueryString queryString)
        => PostRawAsync(queryString, null);

    public Task<HttpResponseMessage> PostRawAsync(QueryString? queryString, object? dto)
        => client.PostAsJsonAsync($"{url}{queryString}", dto, JsonConfiguration.GetOptions(), CancellationToken.None);

    public Task<HttpResponseMessage> PostFileRawAsync(string filePath, CancellationToken ct = default)
        => PostFileRawAsync(File.ReadAllBytes(filePath), Path.GetFileName(filePath), ct);

    public Task<HttpResponseMessage> PostFileRawAsync(byte[] fileContent, string filename, CancellationToken ct = default)
        => client.PostAsync(url, new MultipartFormDataContent
        {
            {
                new ByteArrayContent(fileContent),
                "importFile",
                filename
            }
        }, ct);

    public Task PostAsync(QueryString queryString)
        => PostRawAsync(queryString).AssertSuccessStatusCode();

    public async Task<TResponse> PostAsync<TResponse>(QueryString? queryString, object? dto)
    {
        var response = await PostRawAsync(queryString, dto);
        var envelope = await response.ReadAsync<Envelope<TResponse>>(JsonConfiguration.GetOptions());
        return envelope.Content;
    }
}

/// <summary>
/// RpcEndpoint wrapper with TRequest dto and an enveloped TResponse
/// </summary>
/// <typeparam name="TRequest">Request DTO type</typeparam>
/// <typeparam name="TResponse">Response DTO type within an envelope</typeparam>
public class RpcEndpoint<TRequest, TResponse>(string url, HttpClient client)
    : RpcEndpoint(url, client)
{
    public Task<TResponse> PostAsync(TRequest dto)
        => PostAsync(null, dto);

    public Task<TResponse> PostAsync(QueryString? queryString, TRequest dto)
        => PostAsync<TResponse>(queryString, dto);
}

/// <summary>
/// RpcEndpoint wrapper with TRequest dto and no response body
/// </summary>
/// <typeparam name="TRequest">Request DTO type</typeparam>
public class RpcEndpointWithNoResponse<TRequest>(string url, HttpClient client)
    : RpcEndpoint(url, client)
{
    public Task PostAsync(TRequest dto)
        => PostRawAsync(dto).AssertStatusCode(HttpStatusCode.NoContent);

    public Task<HttpResponseMessage> PostRawAsync(TRequest dto)
        => base.PostRawAsync(dto);
}

/// <summary>
/// An RPC endpoint with no request body presents the opportunity for a readonly GET endpoint
/// </summary>
/// <typeparam name="TResponse">Response DTO type</typeparam>
public class RpcEndpointWithNoRequest<TResponse>(string url, HttpClient client)
    : RpcEndpoint(url, client)
{
    public Task<TResponse> GetAsync(QueryString? queryString = null)
        => GetRawAsync(queryString)
            .AssertSuccessStatusCode()
            .ReadAsync<TResponse>();
}
