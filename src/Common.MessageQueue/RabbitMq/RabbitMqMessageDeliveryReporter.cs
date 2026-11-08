using RabbitMQ.Client;

namespace Medusa.Enap.Common.MessageQueue.RabbitMq;

// The purpose of this class is to provide testability. See derived type.
public class RabbitMqMessageDeliveryReporter
{
    public virtual Task ReportConsumeSuccess(Type msgType, IChannel channel, ulong deliveryTag, string json, CancellationToken ct)
    {
        return channel.BasicAckAsync(deliveryTag, false, ct).AsTask();
    }

    public virtual Task ReportConsumeFailure(IChannel channel, ulong deliveryTag, Type messageType, CancellationToken ct)
    {
        return channel.BasicNackAsync(deliveryTag, false, true, ct).AsTask();
    }

    public virtual void ReportPublishSuccess(string exchange, string routing, object msg)
    {
        //NO-OP in case of live RMQ instance.
    }
}
