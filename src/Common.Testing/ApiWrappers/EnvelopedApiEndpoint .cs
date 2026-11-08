using Medusa.Enap.Common.Core.Messaging;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;

namespace Medusa.Enap.Common.Testing.ApiWrappers;

public class EnvelopedApiEndpoint(string url, HttpClient client, JsonSerializerOptions? jsonOptions = null)
    : ApiEndpoint(url, client, jsonOptions)
{
    #region Get

    public async Task<PagedEnvelope<TResponse>> GetPagedAsync<TResponse>(QueryString? queryString = null)
    {
        var response = await GetRawAsync(path: null, queryString);
        return await response.ReadAsync<PagedEnvelope<TResponse>>(JsonOptions);
    }

    public async Task<TResponse> GetAsync<TResponse>(QueryString? queryString = null)
    {
        var response = await GetRawAsync(queryString);
        var msg = await response.ReadAsync<Envelope<TResponse>>(JsonOptions);
        return msg.Content;
    }

    #endregion

    #region Post

    public Task<TResponse> PostAsync<TResponse>(object dto)
        => PostAsync<TResponse>(path: null, dto);

    public async Task<TResponse> PostAsync<TResponse>(string? path, object dto)
    {
        var response = await PostRawAsync(path, dto);
        await response.AssertStatusCode(HttpStatusCode.Created);
        var msg = await response.ReadAsync<Envelope<TResponse>>(JsonOptions);

        return msg.Content;
    }

    #endregion

    #region Put

    public async Task<TResponse> PutAsync<TResponse>(int id, object dto)
    {
        var response = await PutRawAsync(id, dto);
        var result = await response.ReadAsync<Envelope<TResponse>>(JsonOptions);
        return result.Content;
    }

    public async Task<TResponse> PutAsync<TResponse>(string path, object dto)
    {
        var response = await PutRawAsync(path, dto);
        var result = await response.ReadAsync<Envelope<TResponse>>(JsonOptions);
        return result.Content;
    }

    #endregion
}