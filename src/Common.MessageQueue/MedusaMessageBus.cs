using Common.Core.Integration;
using Common.Core.Logging;
using Common.Core.MessageQueue;
using Common.MessageQueue.RabbitMq;
using Common.MessageQueue.Routing;
using Microsoft.Extensions.Logging;

namespace Common.MessageQueue;

public record DeadLetter(string Json, Type MessageType);

public class MedusaMessageBus(
    RabbitMqClient mqClient,
    OutgoingMessageRouter router,
    CorrelationIdAccessor correlationIdAccessor,
    MedusaLogger<MedusaMessageBus> logger
)
    : IMessagePublisher
{
    public Task PublishAsync(IEnumerable<object> messages)
        => Task.WhenAll(messages.Select(PublishAsync));

    public Task PublishAsync(object message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var route = router.GetRoute(message);

        return PublishToExchangeAsync(route.Exchange, route.RoutingKey, message is DeadLetter dl ? dl.Json : message);
    }

    private async Task PublishToExchangeAsync(string exchange, string routingKey, object msg)
    {
        try
        {
            await mqClient.PublishAsync(
                exchange,
                routingKey,
                msg,
                correlationIdAccessor.CorrelationId
            );
            logger.LogTechnical(LogLevel.Information, "Published to RabbitMQ exchange.", new() {
                { "ExchangeName", exchange },
                { "RoutingKey", routingKey }
            });
        }
        catch (Exception ex)
        {
            logger.LogTechnical(LogLevel.Warning, ex, "Publish to RabbitMQ exchange failed.", new() {
                { "ExchangeName", exchange },
                { "RoutingKey", routingKey }
            });
        }
    }
}