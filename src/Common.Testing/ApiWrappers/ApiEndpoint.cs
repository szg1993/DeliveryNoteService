using Common.Core;
using Common.Core.Utils;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text.Json;

namespace Common.Testing.ApiWrappers;

public class ApiEndpoint(string url, HttpClient client, JsonSerializerOptions? jsonOptions = null)
{
    public string Url { get; private init; } = url;
    public HttpClient Client { get; private init; } = client;
    protected JsonSerializerOptions JsonOptions { get; private init; } = jsonOptions ?? JsonConfiguration.GetOptions();

    #region Get

    public async Task<TResponse> GetAsync<TResponse>(string? path = null, QueryString? queryString = null)
    {
        var response = await GetRawAsync(path, queryString);
        return await response.ReadAsync<TResponse>(JsonOptions);
    }

    public Task<HttpResponseMessage> GetRawAsync(QueryString? queryString = null)
        => GetRawAsync(path: null, queryString);

    public Task<HttpResponseMessage> GetRawAsync(string? path, QueryString? queryString = null)
        => Client.GetAsync($"{Url}/{path}?{queryString?.ToString().TrimStart('?')}");

    #endregion

    #region Post

    public Task<HttpResponseMessage> PostRawAsync(object dto)
        => PostRawAsync(path: null, dto);

    public Task<HttpResponseMessage> PostRawAsync(string? path, object dto)
        => Client.PostAsync($"{Url}/{path}", JsonContent(dto));

    #endregion

    #region Put

    public Task<HttpResponseMessage> PutRawAsync(object dto)
        => Client.PutAsync($"{Url}", JsonContent(dto));

    public Task<HttpResponseMessage> PutRawAsync(int id, object dto)
        => Client.PutAsync($"{Url}/{id}", JsonContent(dto));

    public Task<HttpResponseMessage> PutRawAsync(string path, object dto)
        => Client.PutAsync($"{Url}/{path}", JsonContent(dto));

    #endregion

    #region Patch

    public Task<HttpResponseMessage> PatchRawAsync(object dto)
        => Client.PatchAsync($"{Url}", JsonContent(dto));

    public Task<HttpResponseMessage> PatchRawAsync(int id, object dto)
        => Client.PatchAsync($"{Url}/{id}", JsonContent(dto));

    public Task<HttpResponseMessage> PatchRawAsync(string path, object dto)
        => Client.PatchAsync($"{Url}/{path}", JsonContent(dto));

    #endregion

    #region Delete

    public Task DeleteAsync()
        => DeleteRawAsync()
            .Then(r => r.AssertSuccessStatusCode());

    public Task<HttpResponseMessage> DeleteRawAsync()
        => Client.DeleteAsync(Url);

    public async Task DeleteAsync(int id)
    {
        var r = await Client.DeleteAsync($"{Url}/{id}");
        await r.AssertStatusCode(HttpStatusCode.OK);
    }

    public Task<HttpResponseMessage> DeleteRawAsync(int id)
    {
        return Client.DeleteAsync($"{Url}/{id}");
    }

    public async Task BatchDeleteAsync(IEnumerable<int> ids)
    {
        var r = await BatchDeleteRawAsync(ids);
        await r.AssertStatusCode(HttpStatusCode.OK);
    }

    public Task<HttpResponseMessage> BatchDeleteRawAsync(IEnumerable<int> ids)
    {
        return Client.PostAsync($"{Url}/Delete", JsonContent(new { ids }));
    }

    #endregion

    protected StringContent JsonContent(object dto)
    {
        var json = JsonSerializer.Serialize(dto, dto.GetType(), JsonOptions);
        return new StringContent(json, new MediaTypeHeaderValue(MediaTypeNames.Application.Json));
    }
}