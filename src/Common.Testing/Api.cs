using Medusa.Enap.Common.Core.Configuration;
using Medusa.Enap.Common.Testing.ApiWrappers;
using Medusa.Enap.Common.Testing.Versioning;

namespace Medusa.Enap.Common.Testing;

public class Api(HttpClient client)
{
    public HttpClient Client { get; } = client;

    public VersionsEndpoint Versions { get; } = new VersionsEndpoint(client, JsonConfiguration.GetOptions());

    public ApiEndpoint Health { get; } = new("health", client);
}

public static class HealthEndpointExtensions
{
    public static ApiEndpoint Liveness(this ApiEndpoint parent)
        => new($"{parent.Url}/liveness", parent.Client);

    public static ApiEndpoint Readiness(this ApiEndpoint parent)
        => new($"{parent.Url}/readiness", parent.Client);
}