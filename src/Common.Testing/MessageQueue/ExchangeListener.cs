using Medusa.Enap.Common.Core.Exceptions;
using Medusa.Enap.Common.Core.Logging;
using Medusa.Enap.Common.Core.Utils;
using Medusa.Enap.Common.MessageQueue.RabbitMq;
using Medusa.Enap.Common.Testing.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;
using Xunit.Sdk;

namespace Medusa.Enap.Common.Testing.MessageQueue;

public sealed class ExchangeListener<TMessage> : IDisposable
{
    private readonly RabbitMqConnectionFactory _connectionFactory;
    private readonly TestLogger _logger;
    private readonly ConcurrentQueue<string> _receivedMessages = new();
    private readonly CancellationTokenRegistration _applicationStoppingRegistration;

    private string? _exchange;
    private string? _routingKey;
    private IConnection? _connection;
    private IChannel? _channel;
    private AsyncEventingBasicConsumer? _consumer;
    private bool _disposed;

    public ExchangeListener(
        RabbitMqConnectionFactory connectionFactory,
        TestLogger logger,
        IHostApplicationLifetime applicationLifetime
    )
    {
        _connectionFactory = connectionFactory;
        _logger = logger;

        _applicationStoppingRegistration = applicationLifetime.ApplicationStopping.Register(Dispose);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        _applicationStoppingRegistration.Dispose();

        Unsubscribe();

        _channel?.Dispose();
        _connection?.Dispose();
        _channel = null;
        _connection = null;

        GC.SuppressFinalize(this);
    }

    public async Task StartListeningAsync(string exchange, string routingKey)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _exchange = exchange;
        _routingKey = routingKey;

        _connection = await _connectionFactory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        var queueName = await CreateListenerQueue(_channel, exchange, routingKey);

        await SubscribeAsync(_channel, queueName);
    }

    public TMessage AssertPublished(Func<TMessage, bool>? predicate = null)
        => AssertPublished(EnapSerializer.FromJson<TMessage>, predicate);

    public string AssertPublished(string expectedMessage)
        => AssertPublished(json => json, msg => msg == expectedMessage);

    public void AssertNotPublished(Func<TMessage, bool>? predicate = null)
        => AssertNotPublished(EnapSerializer.FromJson<TMessage>, predicate);

    private static async Task<string> CreateListenerQueue(IChannel channel, string exchange, string routingKey)
    {
        var queue = await channel.QueueDeclareAsync(
            queue: "",
            durable: false,
            exclusive: true,
            autoDelete: true);

        await channel.QueueBindAsync(queue.QueueName, exchange, routingKey);

        return queue.QueueName;
    }

    private async Task SubscribeAsync(IChannel channel, string queueName)
    {
        _consumer = new AsyncEventingBasicConsumer(channel);
        _consumer.ReceivedAsync += OnReceivedAsync;

        await channel.BasicConsumeAsync(queueName, autoAck: false, _consumer);

        _logger.LogTechnical(
            LogLevel.Information,
            $"Started listening to exchange '{_exchange}' with routing key '{_routingKey}'"
        );
    }

    private void Unsubscribe()
    {
        if (_consumer is null)
            return;

        _consumer.ReceivedAsync -= OnReceivedAsync;
        _consumer = null;

        _logger.LogTechnical(
            LogLevel.Information,
            $"Stopped listening to exchange '{_exchange}' with routing key '{_routingKey}'"
        );
    }

    private Task OnReceivedAsync(object sender, BasicDeliverEventArgs args)
    {
        var json = Encoding.UTF8.GetString(args.Body.ToArray());

        _receivedMessages.Enqueue(json);

        _logger.LogTechnical(
            LogLevel.Information,
            $"Message published to exchange '{_exchange}' with routing key '{_routingKey}'",
            LogParams.Create("json", json)
        );

        return Task.CompletedTask;
    }

    private T AssertPublished<T>(
        Func<string, T?> jsonToMessage,
        Func<T, bool>? predicate = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ThrowIfNotStarted();
        
        predicate ??= _ => true;

        foreach (var json in _receivedMessages.ToArray())
        {
            var msg = jsonToMessage(json);
            if (msg is not null && predicate(msg))
                return msg;
        }

        throw new XunitException($"Expected message was not published to '{_exchange}' : '{_routingKey}'.");
    }

    private void AssertNotPublished<T>(
        Func<string, T?> jsonToMessage,
        Func<T, bool>? predicate = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ThrowIfNotStarted();

        predicate ??= _ => true;

        foreach (var json in _receivedMessages.ToArray())
        {
            var msg = jsonToMessage(json);
            if (msg is not null && predicate(msg))
                throw new XunitException($"Expected message NOT to be published to '{_exchange}' : '{_routingKey}'.")
                    .SetData("Json", json);
        }
    }

    private void ThrowIfNotStarted()
    {
        if (_consumer is null)
            throw new InvalidOperationException("Listener is not started yet.");
    }
}