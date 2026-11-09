using Common.Core.Logging;
using Common.Core.Utils;
using Common.MessageQueue.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Diagnostics.CodeAnalysis;

namespace Common.MessageQueue.RabbitMq;

[ExcludeFromCodeCoverage(Justification = "No test environment yet.")]
public sealed class RabbitMqClient : IDisposable
{
    private readonly RabbitMqFacade _facade;
    private readonly IServiceProvider _services;
    private bool _isValidatingTopology = false;
    private readonly Dictionary<string, RabbitMqConsumer> _consumers = [];
    private readonly Dictionary<string, RabbitMqPublisher> _publishers = [];

    public RabbitMqClient(
        RabbitMqFacade facade,
        IHostApplicationLifetime lifetime,
        IServiceProvider services
    )
    {
        _facade = facade;
        _services = services;
        lifetime.ApplicationStopping.Register(OnStopping);
        lifetime.ApplicationStopped.Register(OnStopped);
    }

    public static async Task<RabbitMqClient> InitAsync(IServiceProvider services)
    {
        var rmqClient = services.GetRequiredService<RabbitMqClient>();
        await rmqClient.ConnectAsync();
        await SubscribeToQueues(rmqClient, services);
        await ConnectToExchanges(rmqClient, services);

        return rmqClient;
    }

    public Task PublishAsync(string exchange, string routingKey, object msg, string correlationId)
    {
        if (NoConnection() && EnvironmentUtils.IsDevelopment())
            return Task.CompletedTask;

        if (!_publishers.TryGetValue(exchange, out var p))
            throw new InvalidOperationException($"A channel does not exist for exchange ${exchange}");

        return p.PublishAsync(routingKey, msg, correlationId);
    }

    private async Task ConnectAsync()
    {
        if (_facade.IsConnected())
            return;

        try
        {
            await _facade.ConnectAsync();

            LogConnectionStarted();

            _facade.SetConnectionShutdownCallback(HandleConnectionShutdownAsync);
            _facade.SetRecoverySucceededCallback(HandleRecoverySucceededAsync);
            _facade.SetConnectionRecoveryErrorCallback(HandleConnectionRecoveryErrorAsync);
            _facade.SetChannelShutdownCallback(LogChannelShutdownAsync);
        }
        catch (BrokerUnreachableException)
        {
            if (!EnvironmentUtils.IsDevelopment())
                throw;
        }
    }

    private static async Task SubscribeToQueues(RabbitMqClient conn, IServiceProvider services)
    {
        var map = services.GetRequiredService<QueueMessageMap>();
        foreach (var (msgType, queue) in map.GetAll())
            await conn.SubscribeToQueue(msgType, queue);
    }

    private static async Task ConnectToExchanges(RabbitMqClient conn, IServiceProvider services)
    {
        var router = services.GetRequiredService<OutgoingMessageRouter>();
        foreach (var exchange in router.GetAllExchangeNames())
            await conn.ConnectToExchange(exchange);
    }

    private async Task SubscribeToQueue(Type msgType, string queue)
    {
        if (NoConnection() && EnvironmentUtils.IsDevelopment())
            return;

        if (_consumers.ContainsKey(queue))
            throw new InvalidOperationException($"Already subscribed to queue ${queue}");

        try
        {
            var consumer = await _facade.CreateConsumer(queue);
            await consumer.StartAsync(msgType); // This will throw if queue does not exist.
            _consumers.Add(queue, consumer);
        }
        catch (Exception ex)
        {
            if (!EnvironmentUtils.IsDevelopment())
                throw new InvalidOperationException($"Queue unavailable: {queue}", ex);
        }
    }

    private async Task ConnectToExchange(string exchange)
    {
        if (NoConnection() && EnvironmentUtils.IsDevelopment())
            return;

        await EnsureExchangeExists(exchange);

        if (!_publishers.TryAdd(exchange, await _facade.CreatePublisher(exchange)))
            throw new InvalidOperationException($"Already connected to exchange ${exchange}");
    }

    private async Task EnsureExchangeExists(string exchange)
    {
        var probePublisher = await _facade.CreatePublisher(exchange);

        try
        {
            _isValidatingTopology = true;
            await probePublisher.ProbeExchange(); // This will throw if exchange does not exist.
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Exchange unavailable: {exchange}", ex);
        }
        finally
        {
            probePublisher.Dispose();
            _isValidatingTopology = false;
        }
    }

    private bool NoConnection()
        => !_facade.IsConnected();

    public void Dispose()
    {
        _facade.Dispose();
        IEnumerable<IDisposable> toDispose =
        [
            .. _consumers.Select(c => c.Value),
            .. _publishers.Select(c => c.Value)
        ];

        foreach (var d in toDispose)
            d.Dispose();
    }

    private void OnStopping()
    {
        var consumerSoppingTasks = _consumers
            .Select(c => c.Value.StopAsync())
            .ToArray()
            ;

        // Synchronously waiting here since we are in a synchronous method
        Task.WaitAll(consumerSoppingTasks);
    }

    private void OnStopped()
    {
        // Optional: explicit close here.
        // At this point requests should be done.
        Dispose();
    }

    #region callbacks

    private Task HandleConnectionShutdownAsync(object sender, ShutdownEventArgs @event)
    {
        LogTechnical(LogLevel.Warning, "RabbitMQ connection closed", logParams: new() { { "Reply", @event.ReplyText }, { "Code", @event.ReplyCode.ToString() } });

        return Task.CompletedTask;
    }

    private Task HandleRecoverySucceededAsync(object sender, AsyncEventArgs _)
    {
        LogConnectionStarted();

        return Task.CompletedTask;
    }

    private Task HandleConnectionRecoveryErrorAsync(object sender, ConnectionRecoveryErrorEventArgs e)
    {
        LogTechnical(LogLevel.Error, "RabbitMQ reconnect failed.", ex: e.Exception);

        return Task.CompletedTask;
    }

    private Task LogChannelShutdownAsync(object sender, ShutdownEventArgs @event)
    {
        if (_isValidatingTopology)
            return Task.CompletedTask;

        LogTechnical(LogLevel.Warning, "A channel was shut down.", logParams: new() { { "Reply", @event.ReplyText } });

        return Task.CompletedTask;
    }

    #endregion callbacks

    #region logging

    private void LogConnectionStarted()
    {
        LogTechnical(LogLevel.Information, "RabbitMQ connection started.");
    }

    private void LogTechnical(LogLevel logLevel, string message, Exception? ex = null, Dictionary<string, object?>? logParams = null)
    {
        using var scope = _services.CreateScope();
        var logger = scope.GetLogger();

        if (ex is null)
            logger.LogTechnical(logLevel, message, logParams);
        else
            logger.LogTechnical(logLevel, ex, message, logParams);
    }

    #endregion logging
}

file static class Extensions
{
    public static MedusaLogger<RabbitMqClient> GetLogger(this IServiceScope self)
    {
        return self.ServiceProvider.GetRequiredService<MedusaLogger<RabbitMqClient>>();
    }
}