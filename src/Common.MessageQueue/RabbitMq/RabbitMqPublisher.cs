using Common.Core.Utils;
using RabbitMQ.Client;
using System.Text;

namespace Common.MessageQueue.RabbitMq;

public class RabbitMqPublisher(
    IChannel channel,
    string exchange
)
    : IDisposable
{
    private bool _disposedValue;

    public async Task PublishAsync(string routingKey, object msg, string? correlationId)
    {
        var payload = SerializeMessage(msg);

        await channel.BasicPublishAsync(
           exchange,
           routingKey,
           mandatory: false,
           body: payload,
           basicProperties: new BasicProperties { CorrelationId = correlationId }
        );
    }

    public Task ProbeExchange()
        => channel.ExchangeDeclarePassiveAsync(exchange);

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private static byte[] SerializeMessage(object msg)
    {
        var serialized = msg is string s ? s : EnapSerializer.ToJson(msg);
        var payload = Encoding.UTF8.GetBytes(serialized);

        return payload;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                channel?.Dispose();
            }

            _disposedValue = true;
        }
    }
}