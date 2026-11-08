using Medusa.Enap.Common.Core.Integration;
using Medusa.Enap.Common.Core.Logging;
using Medusa.Enap.Common.Core.MessageQueue;
using Medusa.Enap.Common.MessageQueue.RabbitMq;
using Medusa.Enap.Common.MessageQueue.Routing;
using Microsoft.Extensions.Logging;

namespace Medusa.Enap.Common.MessageQueue;

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
