using Medusa.Enap.Common.Core.DependencyInjection;
using Medusa.Enap.Common.Core.Exceptions;
using Medusa.Enap.Common.Core.Integration;
using Medusa.Enap.Common.Core.Logging;
using Medusa.Enap.Common.Core.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace Medusa.Enap.Common.MessageQueue;

[AutoRegister]
public class MessageHandler(
    IServiceProvider sp,
    CorrelationIdAccessor corr,
    MedusaLogger<MessageHandler> logger
)
{
    private const string _queueName = "QueueName";
    private static readonly HashSet<string> _networkExceptionTypeNames =
    [
        "Npgsql.NpgsqlException",
        "MongoDB.Driver.MongoConnectionException"
    ];

    public async Task<bool> HandleMessageAsync(Type msgType, string queue, string? correlationId, string json, CancellationToken ct)
    {
        List<IDisposable> disposables = [];
        try
        {
            disposables.Add(InitCorrelationId(correlationId));

            logger.LogTechnical(LogLevel.Information, "Received message from RabbitMQ", new() { { _queueName, queue } });

            var msg = EnapSerializer.FromJson(msgType, json)
                ?? throw new DeserializationException();

            if (correlationId == null)
                disposables.Add(ExtractCorrelationIdFromMessage(msg));

            var consumerType = typeof(MedusaConsumer<>).MakeGenericType(msgType);
            var consumer = (IMedusaConsumer)sp.GetRequiredService(consumerType);
            await consumer.Consume(msg, json, queue, ct);

            logger.LogTechnical(LogLevel.Information, "Consumed message from RabbitMQ", new() { { _queueName, queue } });
            return true;
        }
        catch (Exception ex)
        {
            logger.LogTechnical(LogLevel.Warning, ex, "Failed to process message from RabbitMQ", new() { { _queueName, queue } });

            if (IsNetworkError(ex) || IsConcurrentProcessingException(ex))
                return false;

            await SendToDeadLetterExchange(msgType, json);
            return true;
        }
        finally
        {
            foreach (var d in disposables)
                d.Dispose();
        }
    }

    private IDisposable InitCorrelationId(string? correlationId)
    {
        corr.CorrelationId = correlationId!;
        var d = logger.PushCorrelationIdProperty(corr.CorrelationId);
        return d;
    }

    private IDisposable ExtractCorrelationIdFromMessage(object msg)
    {
        if (sp.GetService<ICorrelationIdExtractor>() is not ICorrelationIdExtractor extractor)
            return new DisposableNullObject();

        corr.CorrelationId = extractor.GetCorrelationId(msg);
        return logger.PushCorrelationIdProperty(corr.CorrelationId);
    }

    private async Task SendToDeadLetterExchange(Type msgType, string json)
    {
        var dlp = sp.GetService<DeadLetterPublisher>();
        if (dlp is null)
            logger.LogTechnical(LogLevel.Warning, "Dead Letter Publisher not configured. Input json goes to void.");
        else
            await dlp.Publish(msgType, json);
    }

    private static bool IsNetworkError(Exception ex)
    {
        var baseEx = ex.GetBaseException();

        if (baseEx is HttpRequestException or SocketException or TimeoutException)
            return true;

        var fullName = baseEx.GetType().FullName;

        if (fullName is null)
            return false;

        return _networkExceptionTypeNames.Contains(fullName);
    }

    private static bool IsConcurrentProcessingException(Exception ex)
    {
        return ex is ConcurrentProcessingException;
    }
}

file sealed class DisposableNullObject : IDisposable
{
    public void Dispose()
    { }
}
