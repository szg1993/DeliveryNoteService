using Medusa.Enap.Common.Core.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace Medusa.Enap.Common.Core.Integration;

public class MedusaApiClientFactory(
    IServiceProvider sp,
    IHttpClientFactory factory,
    CorrelationIdAccessor correlationIdAccessor
)
{
    public TApiClient CreateApiClient<TApiSettings, TApiClient>(
        Func<TApiSettings, string> getBaseAddress, Func<HttpClient, TApiClient> createApiClient)
            where TApiSettings : class
            where TApiClient : class
    {
        var client = factory.CreateClient();
        var settings = sp.GetRequiredService<IOptions<TApiSettings>>();       
       
        client.BaseAddress = new Uri(getBaseAddress(settings.Value));
        client.DefaultRequestHeaders.Add("X-Correlation-ID", correlationIdAccessor.CorrelationId);

        SetBearerTokenIfAvailable(sp, client);
        
        return createApiClient(client);
    }

    private static void SetBearerTokenIfAvailable(IServiceProvider sp, HttpClient client)
    {
        var identityContext = sp.GetService<IIdentityContext>();
        var bearerToken = identityContext?.AuthToken;

        if (bearerToken is not null)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
    }
}