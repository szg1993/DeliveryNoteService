using Medusa.Enap.Common.Core.Utils;
using RabbitMQ.Client;

namespace Medusa.Enap.Common.Testing.MessageQueue.FakeRabbitMq;

internal class InMemoryRabbitMqServer(bool createTopology)
{
    private sealed record Message(
        string Exchange,
        string RoutingKey,
        BasicProperties Properties,
        ReadOnlyMemory<byte> Payload
    );

    private readonly List<(string queue, IAsyncBasicConsumer consumer)> _subscriptions = [];
    private readonly List<(string exchange, string routingKey, string queue)> _bindings = [];
    private readonly List<string> _deletedExchanges = [];

    private readonly Dictionary<string, Queue<Message>> _undeliveredMessagesByQueue = [];

    public void DeleteExchange(string exchange)
    {
        _deletedExchanges.Add(exchange);
    }

    public Task SubscribeToQueue(string queue, IAsyncBasicConsumer consumer)
    {
        if (!createTopology)
            throw new InvalidOperationException("Create Topology Disabled");

        _subscriptions.Add((queue, consumer));

        return DeliverUndeliveredMessages(queue, consumer);
    }

    public Task Publish(string exchange, string routingKey, BasicProperties prop, ReadOnlyMemory<byte> payload, CancellationToken ct)
    {
        if (!createTopology)
            throw new InvalidOperationException("Create Topology Disabled");

        if (_deletedExchanges.Contains(exchange))
            return Task.FromException(new Exception("exchange deleted"));

        var targetQueues = GetTargetQueues(exchange, routingKey);
        var message = new Message(exchange, routingKey, prop, payload);
        var deliveries = targetQueues.SelectMany(q => DeliverToSubscribers(q, message, ct));

        return Task.WhenAll(deliveries);
    }

    public void Bind(string exchange, string routingKey, string queue)
    {
        _bindings.Add((exchange, routingKey, queue));
    }

    private IEnumerable<Task> DeliverToSubscribers(string targetQueue, Message message, CancellationToken ct)
    {
        var subscribers = GetSubscribers(targetQueue);

        if (subscribers.None())
        {
            StoreUndeliveredMessage(targetQueue, message);
            return [];
        }

        var deliveryTag = (uint)Random.Shared.NextInt64();
        return [.. subscribers.Select(s => Deliver(s, deliveryTag, message, ct)
        )];
    }

    private async Task DeliverUndeliveredMessages(string queue, IAsyncBasicConsumer consumer)
    {
        if (!_undeliveredMessagesByQueue.Remove(queue, out var undeliveredMessages))
            return;

        while (undeliveredMessages.Count > 0)
        {
            var message = undeliveredMessages.Dequeue();
            var deliveryTag = (uint)Random.Shared.NextInt64();
            await Deliver(consumer, deliveryTag, message, CancellationToken.None);
        }
    }

    private static Task Deliver(IAsyncBasicConsumer s, uint deliveryTag, Message message, CancellationToken ct)
    {
        return s.HandleBasicDeliverAsync(
            "fakeConsumerTag", deliveryTag, false,
            message.Exchange, message.RoutingKey, message.Properties, message.Payload,
            ct
        );
    }

    private string[] GetTargetQueues(string exchange, string routingKey)
    {
        if (exchange == "")
            return [routingKey];

        return [
            .. _bindings
                .Where(b => b.exchange == exchange && b.routingKey == routingKey)
                .Select(b => b.queue)
                .Distinct()
        ];
    }

    private IEnumerable<IAsyncBasicConsumer> GetSubscribers(string targetQueue)
    {
        return _subscriptions
            .Where(s => s.queue == targetQueue)
            .Select(s => s.consumer);
    }

    private void StoreUndeliveredMessage(string queue, Message message)
    {
        if (!_undeliveredMessagesByQueue.TryGetValue(queue, out var messages))
        {
            messages = new Queue<Message>();
            _undeliveredMessagesByQueue[queue] = messages;
        }

        messages.Enqueue(message);
    }
}