using Medusa.Enap.Common.MessageQueue;
using Medusa.Enap.DemoService.WebApp.Messaging;
using Microsoft.Extensions.Options;

namespace Medusa.Enap.DemoService.WebApp.Configuration;

public static class AddMessageQueue
{
    public static IServiceCollection AddDemoServiceMessageQueue(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .Configure<Channels>(configuration.GetSection("Channels"))
            .AddMedusaMessageQueue(configuration, 
                sp => sp.GetRequiredService<IOptions<Channels>>().Value.Incoming,
                sp => sp.GetRequiredService<IOptions<Channels>>().Value.Outgoing
            )
            .AddDeadLetterPublisher("demoServiceDlqCategory", "demoServiceDlqLabel")
            ;
    }
}