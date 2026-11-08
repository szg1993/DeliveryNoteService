using Medusa.Enap.Common.MessageQueue.RabbitMq;
using Medusa.Enap.Common.Testing.TestSuite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Medusa.Enap.Common.Testing.MessageQueue.Fixtures;

public class CreateTopologyConfig
{
    private readonly HashSet<string> _disabledTopologyForTest = [];
    public void Disable(string testId) => _disabledTopologyForTest.Add(testId);
    public bool ShouldCreateTopology(string testId) => !_disabledTopologyForTest.Contains(testId);
}

public interface IMessagingFixture : IAsyncLifetime
{
    void ConfigureTestHostConfiguration(string testId, IConfigurationBuilder configBuilder);
    void ConfigureTestServices(string testId, IServiceCollection serviceCollection);
    Task Clear(string testId);
    Task DeleteExchangeAsync(string testId, string exchange);
}

public class MessagingFixture : IMessagingFixture
{
    private readonly IMessagingFixture _fixture;
    public readonly CreateTopologyConfig CreateTopologyConfig = new();

    private static RunMode RunMode => RunMode.InMemory;

    public MessagingFixture()
    {
        _fixture = RunMode switch
        {
            RunMode.InMemory => new InMemoryMessagingFixture(CreateTopologyConfig),
            RunMode.TestContainer => new TestContainerMessagingFixture(CreateTopologyConfig),
            RunMode.DevTestEnv or _ => throw new InvalidOperationException("Cannot run messaging tests on dev test env RMQ instance.") 
        };
    }

    public Task InitializeAsync()
        => _fixture.InitializeAsync();

    public Task DisposeAsync()
        => _fixture.DisposeAsync();

    public void ConfigureTestHostConfiguration(string testId, IConfigurationBuilder configBuilder)
        => _fixture.ConfigureTestHostConfiguration(testId, configBuilder);

    public void ConfigureTestServices(string testId, IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddSingleton<TestMessageQueue>()
            .AddSingleton<RabbitMqMessageDeliveryReporter, TestRabbitMqMessageDeliveryReporter>()
            .AddTransient(typeof(ExchangeListener<>));

        _fixture.ConfigureTestServices(testId, serviceCollection);
    }

    public Task Clear(string testId)
        => _fixture.Clear(testId);

    public Task DeleteExchangeAsync(string testId, string exchange)
        => _fixture.DeleteExchangeAsync(testId, exchange);
}
