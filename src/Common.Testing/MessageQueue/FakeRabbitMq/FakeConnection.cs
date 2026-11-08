using Moq;
using RabbitMQ.Client;

namespace Medusa.Enap.Common.Testing.MessageQueue.FakeRabbitMq;

internal static class FakeConnection
{
    public static IConnection Create(InMemoryRabbitMqServer fakeRabbitMqServer)
    {
        var mockConn = new Mock<IConnection>();

        mockConn
            .Setup(conn => conn.CreateChannelAsync(
                It.IsAny<CreateChannelOptions>(),
                It.IsAny<CancellationToken>())
            )
            .Returns(() =>
            {
                var fakeChannel = FakeChannel.Create(fakeRabbitMqServer);
                return Task.FromResult(fakeChannel);
            });

        return mockConn.Object;
    }
}