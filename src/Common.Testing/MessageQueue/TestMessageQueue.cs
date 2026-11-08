using FluentAssertions;
using Medusa.Enap.Common.AspNetCore.Logging;
using Medusa.Enap.Common.MessageQueue.RabbitMq;
using Medusa.Enap.Common.MessageQueue.Routing;
using Medusa.Enap.Common.Testing.Logging;
using Medusa.Enap.Common.Testing.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Medusa.Enap.Common.Testing.MessageQueue;

public record PublishedMessage(string Exchange, string RoutingKey, object Message);

public static class TestMessageQueueHelper
{
    public static TestMessageQueue MQ(this IServiceProvider services)
        => services.GetRequiredService<TestMessageQueue>();
}

public class TestMessageQueue(
    IServiceProvider services,
    RabbitMqFacade facade
)
{
    public ConcurrentQueue<object> Consumed { get; } = [];
    public ConcurrentDictionary<object, Exception> Failed { get; } = [];
    public IServiceProvider Services { get; } = services;

    public async Task<PublishResult<TMessage>> PublishAsync<TMessage>(TMessage msg, Guid? correlationId = null)
        where TMessage : class, IEquatable<TMessage>
    {
        var result = await PublishAsync<TMessage>((object)msg, correlationId);

        return new(msg, this, result.CorrelationId);
    }

    public async Task<PublishResult<string>> PublishAsync<TMessage>(string json, Guid? correlationId = null)
        where TMessage : class, IEquatable<TMessage>
    {
        var result = await PublishAsync<TMessage>((object)json);

        return new(json, this, result.CorrelationId);
    }

    public async Task<PublishResult<object>> PublishAsync<TMessage>(object msg, Guid? correlationId = null)
        where TMessage : class, IEquatable<TMessage>
    {
        try
        {
            var map = Services.GetRequiredService<QueueMessageMap>();
            var p = await facade.CreatePublisher("");
            await p.PublishAsync(map.GetQueueName<TMessage>(), msg, correlationId?.ToString());
        }
        catch (Exception ex)
        {
            Services.GetRequiredService<ILogger<TestMessageQueue>>().LogError(ex, $"Failed to consume message of type '{typeof(TMessage).Name}'");
            Failed[msg] = ex;
        }

        return new(msg, this, correlationId?.ToString());
    }

    public Task<ExchangeListener<TMessage>> ListenToDeadLetterExchange<TMessage>(OutgoingMessageRouter router)
    {
        var r = router.GetDeadLetterRoute<TMessage>();
        return ListenToExchange<TMessage>(r.Exchange, r.RoutingKey);
    }

    public Task<ExchangeListener<TMessage>> ListenToExchange<TMessage>(OutgoingMessageRouter router)
    {
        var r = router.GetMessageRoute<TMessage>();
        return ListenToExchange<TMessage>(r.Exchange, r.RoutingKey);
    }

    public async Task<ExchangeListener<TMessage>> ListenToExchange<TMessage>(string exchange, string routingKey = "")
    {
        var listener = Services.GetRequiredService<ExchangeListener<TMessage>>();
        await listener.StartListeningAsync(exchange, routingKey);
        return listener;
    }

    public Task<TMessage> AssertConsumed<TMessage>(Func<TMessage, bool> predicate) where TMessage : class
    {
        return Wait.For(
            $"consumed message of type '{typeof(TMessage)}'",
            ct =>
            {
                var msg = Consumed.OfType<TMessage>().FirstOrDefault(predicate);

                if (msg is not null)
                    return Task.FromResult(msg)!;

                FindFailure(predicate).Should().BeNull();

                return Task.FromResult<TMessage?>(null);
            },
            () =>
            {
                var msg = "Consumed messages:"
                    + string.Concat(Consumed.ToArray().Select(c => Environment.NewLine + "\t- " + MessageToString(c)));
                msg += Environment.NewLine + Environment.NewLine;

                var testLogStore = Services.GetRequiredService<TestLogStore>();
                msg += "Logged errors:"
                    + string.Concat(testLogStore.GetErrors().Select(err => Environment.NewLine + "\t- " + ErrorToString(err)));

                return msg;
            }
        );
    }

    public Task<Exception> AssertFailed<TMessage>() where TMessage : class
        => AssertFailed<TMessage>(_ => true);

    public async Task<Exception> AssertFailed<TMessage>(Func<TMessage, bool> predicate) where TMessage : class
    {
        var ex = await Wait.For(
            $"failed message of type '{typeof(TMessage)}'",
            ct => Task.FromResult(FindFailure(predicate))
        );

        return ex;
    }

    private Exception? FindFailure<TMessage>(Func<TMessage, bool> predicate) where TMessage : class
    {
        return Failed.Where(kvp => kvp.Key is TMessage msg && predicate(msg)).Select(kvp => kvp.Value).FirstOrDefault();
    }

    private static string MessageToString(object msg) => $"{msg.GetType().Name}: {JsonSerializer.Serialize(msg, msg.GetType())}";

    private static string ErrorToString(LogEntry err) => $"{err.ErrorCode}: {JsonSerializer.Serialize(err, err.GetType())}";
}
