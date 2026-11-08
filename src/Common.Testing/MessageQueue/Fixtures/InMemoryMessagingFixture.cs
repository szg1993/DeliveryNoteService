using Medusa.Enap.Common.MessageQueue.RabbitMq;
using Medusa.Enap.Common.Testing.MessageQueue.FakeRabbitMq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Medusa.Enap.Common.Testing.MessageQueue.Fixtures;

internal class InMemoryMessagingFixture(CreateTopologyConfig createTopologyConfig) : IMessagingFixture
{
    private readonly ConcurrentDictionary<string, InMemoryRabbitMqServer> _inMemoryServers = [];

    public Task InitializeAsync()
        => Task.CompletedTask;

    public Task DisposeAsync()
        => Task.CompletedTask;

    public void ConfigureTestHostConfiguration(string testId, IConfigurationBuilder configBuilder)
    { }

    public void ConfigureTestServices(string testId, IServiceCollection serviceCollection)
    {
        _inMemoryServers[testId] = new(createTopologyConfig.ShouldCreateTopology(testId));
        serviceCollection.AddSingleton(_inMemoryServers[testId]);
        serviceCollection.AddSingleton<RabbitMqConnectionFactory, InMemoryRabbitMqConnectionFactory>();
    }

    public Task Clear(string testId)
    {
        _inMemoryServers.TryRemove(testId, out _);
        return Task.CompletedTask;
    }

    public Task DeleteExchangeAsync(string testId, string exchange)
    {
        _inMemoryServers[testId].DeleteExchange(exchange);
        return Task.CompletedTask;
    }
}
