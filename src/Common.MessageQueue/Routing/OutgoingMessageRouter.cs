
namespace Medusa.Enap.Common.MessageQueue.Routing;

public record MessageRoute(string Exchange, string RoutingKey);

public abstract class OutgoingMessageRouter()
{
    private readonly Dictionary<Type, Func<MessageRoute>> _msgRoutes = [];
    private readonly Dictionary<Type, Func<MessageRoute>> _deadLetterRoutes = [];

    public MessageRoute GetRoute(object msg)
    {
        return msg is DeadLetter dl
            ? GetDeadLetterRoute(dl.MessageType)
            : GetMessageRoute(msg.GetType());
    }

    public MessageRoute GetMessageRoute<TMessage>()
        => GetMessageRoute(typeof(TMessage));

    public MessageRoute GetMessageRoute(Type type)
        => _msgRoutes[type].Invoke();

    public MessageRoute GetDeadLetterRoute<TMessage>()
        => GetDeadLetterRoute(typeof(TMessage));

    public MessageRoute GetDeadLetterRoute(Type msgType)
        => _deadLetterRoutes[msgType].Invoke();

    protected void MapToExchange<TMessage>(Func<ExchangeConfig> exchangeConfigSelector, string routingKeyId = "")
    {
        _msgRoutes[typeof(TMessage)] = () =>
        {
            var exchangeConfig = exchangeConfigSelector();
            return new MessageRoute(exchangeConfig.Exchange, exchangeConfig.GetRoutingKey(routingKeyId));
        };
    }

    protected void MapDeadLetter<TMessage>(Func<ExchangeConfig> dlxSelector, string routingKeyId)
    {
        _deadLetterRoutes[typeof(TMessage)] = () =>
        {
            var dlx = dlxSelector();
            return new MessageRoute(dlx.Exchange, dlx.GetRoutingKey(routingKeyId));
        };
    }

    internal string[] GetAllExchangeNames()
    {
        var exchanges = _msgRoutes.Select(c => c.Value.Invoke().Exchange);
        var deadLetterExchanges = _deadLetterRoutes.Select(c => c.Value.Invoke().Exchange);
        return [.. exchanges.Concat(deadLetterExchanges).Distinct()];
    }
}
