using Medusa.Enap.Common.Core.MessageQueue;
using Medusa.Enap.Common.MessageQueue.RabbitMq;
using Medusa.Enap.Common.MessageQueue.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Medusa.Enap.Common.MessageQueue;

public static class AddMessageQueue
{
    public static IServiceCollection AddMedusaMessageQueue(this IServiceCollection services,
        IConfiguration configuration,
        Func<IServiceProvider, QueueMessageMap> getIncomingMap,
        Func<IServiceProvider, OutgoingMessageRouter> getOutgoingRouter
    )
    {
        return services
            .Configure<MessageBrokerSettings>(configuration.GetSection("MessageBroker"))
            .AddSingleton<RabbitMqClient>()
            .AddSingleton<RabbitMqFacade>()
            .AddSingleton<RabbitMqMessageDeliveryReporter>()
            .AddTransient<RabbitMqConnectionFactory>()
            .AddTransient<QueueMessageMap>(getIncomingMap)
            .AddTransient<OutgoingMessageRouter>(getOutgoingRouter)
            .AddScoped<IMessagePublisher, MedusaMessageBus>();
    }
}