namespace Medusa.Enap.Common.MessageQueue.Routing;

public class ExchangeConfig
{
    public string Exchange { get; set; } = null!;
    public Dictionary<string, string>? RoutingKeys { get; set; }

    public string GetRoutingKey(string routingKeyId)
    {
        if (RoutingKeys is null)
            return string.Empty;

        return RoutingKeys.TryGetValue(routingKeyId, out var value)
            ? value ?? string.Empty
            : string.Empty;
    }
}