using Medusa.Enap.Common.MessageQueue.Routing;
using Medusa.Enap.DemoService.Application;

namespace Medusa.Enap.DemoService.WebApp.Messaging;

public class Channels
{
    public Outgoing Outgoing { get; set; } = null!;
    public Incoming Incoming { get; set; } = null!;
}

public class Outgoing : OutgoingMessageRouter
{
    public ExchangeConfig DemoOutgoingExchange { get; set; } = null!;
    public ExchangeConfig DeadLettersDemoIncoming { get; set; } = null!;

    public Outgoing()
    {
        MapToExchange<DemoMessage>(() => DemoOutgoingExchange, routingKeyId: "DemoMessage");

        MapDeadLetter<DemoMessage>(() => DeadLettersDemoIncoming, routingKeyId: "DemoMessage");
    }
}

public class Incoming : QueueMessageMap
{
    public string DemoIncomingQueue { get; set; } = null!;

    public Incoming()
    {
        Map<DemoMessage>(() => DemoIncomingQueue);
    }
}