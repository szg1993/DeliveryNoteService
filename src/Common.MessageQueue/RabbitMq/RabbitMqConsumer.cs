using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;

namespace Common.MessageQueue.RabbitMq;

public class RabbitMqConsumer(
    IChannel channel,
    string queue,
    int messageRetryDelaySeconds,
    RabbitMqMessageDeliveryReporter reporter,
    IServiceProvider services
)
    : IDisposable
{
    private readonly AsyncEventingBasicConsumer _consumer = new(channel);
    private readonly ConcurrentDictionary<Task, Task> _runningHandles = [];
    private string? _consumerTag = null;
    private bool _disposedValue;

    public Task StartAsync<TMessage>() where TMessage : class
        => StartAsync(typeof(TMessage));

    public async Task StartAsync(Type msgType)
    {
        _consumer.ReceivedAsync += (sender, args) =>
        {
            var runningHandle = HandleMessageAsync(
                msgType,
                queue,
                args.BasicProperties.CorrelationId,
                Encoding.UTF8.GetString(args.Body.ToArray()),
                args.DeliveryTag,
                args.CancellationToken
            );

            runningHandle.ContinueWith(_ => _runningHandles.TryRemove(runningHandle, out var _));
            _runningHandles.TryAdd(runningHandle, runningHandle);

            return runningHandle;
        };

        _consumerTag = await channel.BasicConsumeAsync(queue, false, _consumer);
    }

    public async Task StopAsync()
    {
        if (string.IsNullOrEmpty(_consumerTag))
            return;

        await channel.BasicCancelAsync(_consumerTag);
        await Task.WhenAll(_runningHandles.Select(r => r.Value));
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private async Task HandleMessageAsync(Type msgType, string queue, string? correlationId, string json, ulong deliveryTag, CancellationToken ct)
    {
        using var scope = services.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<MessageHandler>();

        var handledSuccessfully = await handler.HandleMessageAsync(msgType, queue, correlationId, json, ct);

        if (handledSuccessfully)
        {
            await reporter.ReportConsumeSuccess(msgType, channel, deliveryTag, json, ct);
        }
        else
        {
            await Task.Delay(TimeSpan.FromSeconds(messageRetryDelaySeconds), ct);
            await reporter.ReportConsumeFailure(channel, deliveryTag, msgType, ct);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                channel.Dispose();
            }
            _disposedValue = true;
        }
    }
}