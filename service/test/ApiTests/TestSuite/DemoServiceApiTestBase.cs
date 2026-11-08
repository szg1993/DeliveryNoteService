using Medusa.Enap.Common.Testing.MessageQueue;
using Medusa.Enap.Common.Testing.MessageQueue.Fixtures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApiTests.TestSuite;

public abstract class DemoServiceApiTestBase : ApiTestBase<Program, DemoServiceApi>
{
    protected TestMessageQueue MQ => Services.MQ();
    public required MessagingFixture MessagingFixture { get; init; }

    protected override DemoServiceApi CreateApiWrapper(HttpClient client)
        => new(client);

    protected override void ConfigureTestHostConfiguration(IConfigurationBuilder configBuilder)
    {
        base.ConfigureTestHostConfiguration(configBuilder);
        configBuilder.DisableScheduledJob("DemoCron");
        MessagingFixture.ConfigureTestHostConfiguration(TestId, configBuilder);
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services.AddTestScheduling(DatabaseFixture);
        MessagingFixture.ConfigureTestServices(TestId, services);
    }

    public override async Task DisposeAsync()
    {
        await MessagingFixture.Clear(TestId);
        await base.DisposeAsync();
    }
}
