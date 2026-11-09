using Common.Core.Messaging;
using Common.Core.UseCases.BatchOperations;
using Microsoft.AspNetCore.Http;

namespace Common.Testing.ApiWrappers;

public class CollectionEndpoint<TResource>(string url, HttpClient client)
    : CollectionEndpoint<TResource, TResource>(url, client);

public class CollectionEndpoint<TResourceOverview, TResourceDetails>(string url, HttpClient client)
    : EnvelopedApiEndpoint(url, client)
{
    public async Task<PagedEnvelope<TResourceOverview>> GetAsync(QueryString? queryString = null)
    {
        var response = await GetRawAsync(queryString);
        return await response.ReadAsync<PagedEnvelope<TResourceOverview>>(JsonOptions);
    }

    [Obsolete("Use the overload with QueryString class")]
    public async Task<PagedEnvelope<TResourceOverview>> GetAsync(string? queryString)
    {
        var response = await GetRawAsync(new QueryString("?" + queryString));
        return await response.ReadAsync<PagedEnvelope<TResourceOverview>>(JsonOptions);
    }

    public Task<TResourceDetails> PostAsync(object dto) => PostAsync<TResourceDetails>(dto);

    public Task PutAsync(object dto) => PutRawAsync(dto).AssertSuccessStatusCode();

    public RpcEndpoint<object, IEnumerable<TResourceOverview>> Search() => Search<object, TResourceOverview>();

    public RpcEndpoint<TSearchFilter, IEnumerable<TSearchResult>> Search<TSearchFilter, TSearchResult>() => new(Url + "/search", Client);

    public async Task<Envelope<BatchResult<TResourceDetails>>> PatchAsync(CollectionPatchRequest<TResourceDetails> dto)
    {
        var response = await PatchRawAsync(dto);

        await response.AssertSuccessStatusCode();

        return await response.ReadAsync<Envelope<BatchResult<TResourceDetails>>>();
    }
}