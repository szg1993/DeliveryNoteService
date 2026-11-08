using Medusa.Enap.Common.Core.Utils;
using Medusa.Enap.Common.MessageQueue.RabbitMq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Diagnostics.CodeAnalysis;

namespace Medusa.Enap.Common.Testing.MessageQueue;

internal class TestRabbitMqMessageDeliveryReporter(TestMessageQueue mq) : RabbitMqMessageDeliveryReporter
{
    public async override Task ReportConsumeSuccess(Type msgType, IChannel channel, ulong deliveryTag, string json, CancellationToken ct)
    {
        await base.ReportConsumeSuccess(msgType, channel, deliveryTag, json, ct);

        mq.Consumed.Enqueue(TryParse(msgType, json, out var parsed) ? parsed : json);
    }

    public async override Task ReportConsumeFailure(IChannel channel, ulong deliveryTag, Type messageType, CancellationToken ct)
    {
        await base.ReportConsumeFailure(channel, deliveryTag, messageType, ct);

        mq.Services.GetRequiredService<ILogger<TestMessageQueue>>().LogError($"Failed to consume message of type '{messageType.Name}'");
    }

    private static bool TryParse(Type msgType, string json, [NotNullWhen(true)] out object? parsed)
    {
        try
        {
            parsed = EnapSerializer.FromJson(msgType, json);
        }
        catch (Exception)
        {
            parsed = null;
        }

        return parsed != null;
    }
}
