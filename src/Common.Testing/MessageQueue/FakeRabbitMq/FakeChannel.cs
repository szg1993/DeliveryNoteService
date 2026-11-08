using Moq;
using RabbitMQ.Client;

namespace Medusa.Enap.Common.Testing.MessageQueue.FakeRabbitMq;

#pragma warning disable S107 // too many arguments (8)

internal static class FakeChannel
{
    public static IChannel Create(InMemoryRabbitMqServer fakeRabbitMqServer)
    {
        var channelMock = new Mock<IChannel>(MockBehavior.Strict);

        channelMock
            .Setup(c => c.BasicQosAsync(0, 1, false, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        channelMock
            .Setup(c => c.ExchangeDeclarePassiveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        channelMock
            .Setup(c => c.BasicAckAsync(It.IsAny<ulong>(), false, It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);
        channelMock
            .Setup(c => c.BasicNackAsync(It.IsAny<ulong>(), false, true, It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        SetupAnonymousQueueDeclare(channelMock);
        SetupAnonymousQueueBinding(fakeRabbitMqServer, channelMock);

        SetupConsume(channelMock, fakeRabbitMqServer);
        SetupPublish(channelMock, fakeRabbitMqServer);

        return channelMock.Object;
    }

    private static void SetupAnonymousQueueDeclare(Mock<IChannel> channelMock)
    {
        var queue = "";
        channelMock
            .Setup(c => c.QueueDeclareAsync(queue, false, true, true, null, false, false, It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult(new QueueDeclareOk("dummy queue name " + Guid.NewGuid(), 0, 0)));
    }

    private static void SetupAnonymousQueueBinding(InMemoryRabbitMqServer fakeRabbitMqServer, Mock<IChannel> channelMock)
    {
        channelMock
                    .Setup(c => c.QueueBindAsync(It.Is<string>(s => s.StartsWith("dummy")), It.IsAny<string>(), It.IsAny<string>(), null, false, It.IsAny<CancellationToken>()))
                    .Returns((
                        string queue,
                        string exchange,
                        string routingKey,
                        IDictionary<string, object?>? _,
                        bool _,
                        CancellationToken _
                    ) =>
                    {
                        fakeRabbitMqServer.Bind(exchange, routingKey, queue);
                        return Task.CompletedTask;
                    });
        channelMock
            .Setup(c => c.Dispose());
    }

    private static void SetupConsume(Mock<IChannel> channelMock, InMemoryRabbitMqServer fakeRabbitMqServer)
    {
        channelMock.Setup(c => c.BasicConsumeAsync(
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<IDictionary<string, object?>>(),
            It.IsAny<IAsyncBasicConsumer>(),
            It.IsAny<CancellationToken>()
            )
        )
        .Returns(async (
            string queue,
            bool _,
            string consumerTag,
            bool _,
            bool _,
            IDictionary<string, object?>? _,
            IAsyncBasicConsumer consumer,
            CancellationToken _
        ) =>
        {
            await fakeRabbitMqServer.SubscribeToQueue(queue, consumer);
            return consumerTag;
        });
    }

    private static void SetupPublish(Mock<IChannel> publishChannel, InMemoryRabbitMqServer fakeRabbitMqServer)
    {
        publishChannel.Setup(c => c.BasicPublishAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<BasicProperties>(),
            It.IsAny<ReadOnlyMemory<byte>>(),
            It.IsAny<CancellationToken>()
            )
        ).Returns((
            string exchange,
            string routingKey,
            bool mandatory,
            BasicProperties prop,
            ReadOnlyMemory<byte> payload,
            CancellationToken ct
        ) =>
        {
            return new(fakeRabbitMqServer.Publish(exchange, routingKey, prop, payload, ct));
        });
    }
}

#pragma warning restore S107
