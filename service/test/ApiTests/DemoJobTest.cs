using Medusa.Enap.DemoService.Domain.Models;
using Medusa.Enap.DemoService.WebApp.Configuration;
using Microsoft.Extensions.Configuration;

namespace ApiTests;

public class DemoJobTestBase : DemoServiceApiTestBase
{
    protected override void ConfigureTestHostConfiguration(IConfigurationBuilder configBuilder)
    {
        base.ConfigureTestHostConfiguration(configBuilder);
        configBuilder.Add("Features:DemoJob:Enabled", "True"); ;
    }

    protected Task AssertClusteredCounter(int expectedCounter) => AssertModelUpdated(expectedCounter, m => m.ClusteredJobCounter);

    protected Task AssertLocalCounter(int expectedCounter) => AssertModelUpdated(expectedCounter, m => m.LocalJobCounter);

    private Task AssertModelUpdated(int expectedCounter, Func<DemoModel, int> selector)
    {
        return Wait.For("demo model to be updated", async (ct) =>
        // the job is scheduled to run next time in 2099... but it starts immediately
        // wait for the job to create the model
        {
            await Task.Delay(10, ct);
            var r = await Api.DemoModels().GetAsync();
            var m = r.Content.SingleOrDefault();

            return m is not null && selector(m) == expectedCounter;
        });
    }
}

public class DemoJobTest : DemoJobTestBase
{
    protected override void ConfigureTestHostConfiguration(IConfigurationBuilder configBuilder)
    {
        base.ConfigureTestHostConfiguration(configBuilder);
        configBuilder.DisableStartupJobs(false);
    }

    [Fact]
    public async Task LocalInitialStartAndSubsequentTriggers()
    {
        await AssertLocalCounter(1);

        await TriggerJob<LocalDemoJob>();
        await AssertLocalCounter(2);

        await TriggerJob<LocalDemoJob>();
        await AssertLocalCounter(3);
    }

    [Fact]
    public async Task ClusteredInitialStartAndSubsequentTriggers()
    {
        await AssertClusteredCounter(1);

        await TriggerJob<ClusteredDemoJob>();
        await AssertClusteredCounter(2);

        await TriggerJob<ClusteredDemoJob>();
        await AssertClusteredCounter(3);
    }
}

public class DisabledStartupJobsTest : DemoJobTestBase
{
    protected override void ConfigureTestHostConfiguration(IConfigurationBuilder configBuilder)
    {
        base.ConfigureTestHostConfiguration(configBuilder);
        configBuilder.DisableStartupJobs(true);
    }

    [Fact]
    public async Task LocalTriggers()
    {
        await TriggerJob<LocalDemoJob>();
        await AssertLocalCounter(1);

        await TriggerJob<LocalDemoJob>();
        await AssertLocalCounter(2);
    }

    [Fact]
    public async Task ClusteredTriggers()
    {
        await TriggerJob<ClusteredDemoJob>();
        await AssertClusteredCounter(1);

        await TriggerJob<ClusteredDemoJob>();
        await AssertClusteredCounter(2);
    }
}