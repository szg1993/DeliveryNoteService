using Medusa.Enap.Common.AspNetCore;
using Medusa.Enap.DemoService.Application.Errors;

namespace ApiTests;

public class DemoApiTests : DemoServiceApiTestBase
{

    [Fact]
    public Task DemoModelNotFound()
    {
        return Api.DemoModels(1234).GetRawAsync()
            .AssertNotFoundAsync(ErrorCodes.DemoModelNotFound);
    }

    [Fact]
    public Task RequiredHeader()
    {
        this.Api.Client.DefaultRequestHeaders.Add("X-Demo-Header-Parameter", "dummy");
        this.Api.Client.DefaultRequestHeaders.Add("X-Demo-Required-Header", "dummy");

        return Api.DemoRpcEndpoint()
            .PostRawAsync()
            .AssertSuccessStatusCode();
    }

    [Fact]
    public Task HeaderParameter_Missing()
    {
        this.Api.Client.DefaultRequestHeaders.Add("X-Demo-Required-Header", "dummy");
        return Api.DemoRpcEndpoint().PostRawAsync()
            .AssertBadRequestAsync(CommonWebErrorCodes.InvalidHttpRequest);
    }

    [Fact]
    public Task RequiredHeader_Missing()
    {
        this.Api.Client.DefaultRequestHeaders.Add("X-Demo-Header-Parameter", "dummy");
        return Api.DemoRpcEndpoint().PostRawAsync()
            .AssertBadRequestAsync(CommonWebErrorCodes.MissingRequiredHeader);
    }
}