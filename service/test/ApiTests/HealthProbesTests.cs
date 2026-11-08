using Medusa.Enap.Common.Core.HealthProbes;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace ApiTests;

public class HealthProbesTests : DemoServiceApiTestBase
{
    [Fact]
    public Task Liveness()
    {
        return Api.Health.Liveness().GetRawAsync(null)
            .AssertSuccessStatusCode();
    }

    [Fact]
    public Task Readiness_WhenReady()
    {
        GivenReadinessState(true);

        return Api.Health.Readiness().GetRawAsync(null)
            .AssertSuccessStatusCode();
    }

    [Fact]
    public Task Readiness_WhenNotReadyYet()
    {
        GivenReadinessState(false);

        return Api.Health.Readiness().GetRawAsync(null)
            .AssertStatusCode(HttpStatusCode.ServiceUnavailable);
    }

    private void GivenReadinessState(bool readiness)
    {
        var readinessService = Services.GetRequiredService<IReadinessService>();
        readinessService.IsReady = readiness;
    }
}
