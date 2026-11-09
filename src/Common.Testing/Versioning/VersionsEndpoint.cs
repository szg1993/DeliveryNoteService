using Common.Core.Versioning;
using Common.Testing.ApiWrappers;
using System.Text.Json;

namespace Common.Testing.Versioning;

public class VersionsEndpoint(HttpClient client, JsonSerializerOptions jsonOptions)
{
    readonly ApiEndpoint _endpoint = new("api/v1/Versions", client, jsonOptions);

    public Task<VersionInfo> GetAsync(bool connectedServices = false)
        => _endpoint.GetAsync<VersionInfo>(path: connectedServices.ToString().ToLowerInvariant());

    public Task<HttpResponseMessage> GetRawAsync(bool connectedServices = false)
        => _endpoint.GetRawAsync(path: connectedServices.ToString().ToLowerInvariant());
}