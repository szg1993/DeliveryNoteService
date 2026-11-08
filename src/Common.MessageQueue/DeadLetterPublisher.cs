using Medusa.Enap.Common.Core.Integration;
using Medusa.Enap.Common.Core.Logging;
using Medusa.Enap.Common.Core.MessageQueue;
using Microsoft.Extensions.DependencyInjection;

namespace Medusa.Enap.Common.MessageQueue;

public static class DeadLetterInit
{
    public static IServiceCollection AddDeadLetterPublisher(this IServiceCollection services,
        string logCategory, string logLabel
    )
    {
        return services
            .AddSingleton(new DeadLetterExchangeSettings(logCategory, logLabel))
            .AddScoped<DeadLetterPublisher>();
    }
}

public record DeadLetterExchangeSettings(string LogCategory, string LogLabel);

public class DeadLetterPublisher(
    DeadLetterExchangeSettings settings,
    CorrelationIdAccessor corrIdAccessor,
    IMessagePublisher messagePublisher,
    BusinessEventLogger<DeadLetterPublisher> logger
)
{
    public Task Publish(Type msgType, string json)
    {
        return logger
            .Init(settings.LogCategory, ["dlq", settings.LogLabel], "dlqSave")
            .WithBusinessId("correlationId", corrIdAccessor.CorrelationId)
            .RunAsync(
                LogBeginEvent.Run,
                LogEndEvent.Success,
                () => messagePublisher.PublishAsync(new DeadLetter(json, msgType))
            );
    }
}