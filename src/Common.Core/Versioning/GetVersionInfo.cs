using Medusa.Enap.Common.Core.Integration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Medusa.Enap.Common.Core.Versioning;

public class GetVersionInfo(
    ILocalVersionService versionService,
    IOptions<VersionSettings> settings,
    ILogger<GetVersionInfo> logger,
    IHttpClientFactory clientFactory,
    CorrelationIdAccessor correlationIdAccessor
)
{
    public async Task<VersionInfo> RunAsync(string componentName, bool connectedServices, CancellationToken cancellationToken)
    {
        return new VersionInfo()
        {
            ComponentName = componentName,
            Version = versionService.GetVersion().ToString(),
            ConnectedServices = connectedServices
                ? await GetConnectedServiceVersionsAsync(settings.Value.ConnectedServiceVersionEndpoints, cancellationToken)
                : null
        };
    }

    public async Task<IEnumerable<VersionInfo>> GetConnectedServiceVersionsAsync(IEnumerable<string> versionAddresses, CancellationToken cancellationToken)
    {
        List<VersionInfo> result = [];

        using var apiClient = new ApiClient(clientFactory, correlationIdAccessor);

        foreach (var address in versionAddresses)
        {
            try
            {
                var serviceVersion = await apiClient.GetAsync<VersionInfo>(address, cancellationToken);
                result.Add(serviceVersion);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Could not get version for connected service: '{Service}'", address);
                continue;
            }
        }

        return result;
    }
}