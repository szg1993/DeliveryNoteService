using Medusa.Enap.Common.MessageQueue;
using Medusa.Enap.Common.MessageQueue.RabbitMq;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Medusa.Enap.Common.Testing.MessageQueue.FakeRabbitMq;

internal class InMemoryRabbitMqConnectionFactory(
    IOptions<MessageBrokerSettings> settings,
    InMemoryRabbitMqServer fakeRabbitMqServer
)
    : RabbitMqConnectionFactory(settings)
{
    public override Task<IConnection> CreateConnectionAsync()
    {
        return Task.FromResult(FakeConnection.Create(fakeRabbitMqServer));
    }
}