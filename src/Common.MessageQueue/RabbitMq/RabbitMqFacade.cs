using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Medusa.Enap.Common.MessageQueue.RabbitMq;

public class RabbitMqFacade(
    IOptions<MessageBrokerSettings> settings,
    RabbitMqConnectionFactory connectionFactory,
    IServiceProvider serviceProvider
)
    : IDisposable
{
    private Func<object, ShutdownEventArgs, Task> _channelShutdownCallback = null!;
    private readonly MessageBrokerSettings _settings = settings.Value;
    private IConnection? _connection;
    private bool _disposedValue;

    public virtual async Task ConnectAsync()
    {
        _connection = await connectionFactory.CreateConnectionAsync();
    }

    public bool IsConnected()
        => _connection is not null && _connection.IsOpen;

    public void SetConnectionShutdownCallback(Func<object, ShutdownEventArgs, Task> callback)
        => _connection!.ConnectionShutdownAsync += callback.Invoke;

    public void SetRecoverySucceededCallback(Func<object, AsyncEventArgs, Task> callback)
        => _connection!.RecoverySucceededAsync += callback.Invoke;

    public void SetConnectionRecoveryErrorCallback(Func<object, ConnectionRecoveryErrorEventArgs, Task> callback)
        => _connection!.ConnectionRecoveryErrorAsync += callback.Invoke;

    public void SetChannelShutdownCallback(Func<object, ShutdownEventArgs, Task> callback)
        => _channelShutdownCallback = callback;

    public async Task<RabbitMqConsumer> CreateConsumer(string queue)
        => new RabbitMqConsumer(
            await CreateChannelAsync(),
            queue,
            _settings.MessageRetryDelaySeconds,
            serviceProvider.GetRequiredService<RabbitMqMessageDeliveryReporter>(),
            serviceProvider
        );

    public async Task<RabbitMqPublisher> CreatePublisher(string exchange)
        => new RabbitMqPublisher(await CreateChannelAsync(), exchange);

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _connection?.Dispose();
            }

            _disposedValue = true;
        }
    }

    private async Task<IChannel> CreateChannelAsync()
    {
        if (_connection is null)
            throw new InvalidOperationException("RabbitMQ connection is not initialized");

        if (_channelShutdownCallback is null)
            throw new InvalidOperationException("RabbitMQ channel shutdown handling callback has not been set");

        var channel = await _connection!.CreateChannelAsync(
            new CreateChannelOptions(
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true
            )
        );

        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, false);
        channel.ChannelShutdownAsync += _channelShutdownCallback.Invoke;

        return channel;
    }
}
