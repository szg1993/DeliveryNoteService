using Microsoft.AspNetCore.Http;

namespace Medusa.Enap.Common.Testing.ApiWrappers;

public class ResourceEndpoint<TResourceDetails>(string url, HttpClient client)
    : EnvelopedApiEndpoint(url, client)
{
    public Task<TResourceDetails> GetAsync(QueryString? queryString = null)
        => GetAsync<TResourceDetails>(queryString);

    public Task<TResourceDetails> PutAsync(object dto)
        => PutAsync<TResourceDetails>(path: null!, dto);
}
