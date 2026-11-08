using Medusa.Enap.Common.Testing.ApiWrappers;
using Medusa.Enap.DemoService.Domain.Models;

namespace ApiTests.TestSuite;

public class DemoServiceApi(HttpClient client) : Api(client)
{
    public ResourceEndpoint<DemoModel> DemoModels(int id) => new($"api/demo-models/{id}", Client);
    public CollectionEndpoint<DemoModel> DemoModels() => new("api/demo-models", Client);
    public RpcEndpoint DemoRpcEndpoint() => new("api/demo-rpc-endpoint", Client);
}
