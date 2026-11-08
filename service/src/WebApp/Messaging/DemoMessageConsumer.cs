using Medusa.Enap.Common.Core.DependencyInjection;
using Medusa.Enap.Common.MessageQueue;
using Medusa.Enap.DemoService.Application;

namespace Medusa.Enap.DemoService.WebApp.Messaging;

[AutoRegister(ServiceLifetime.Transient, typeof(MedusaConsumer<DemoMessage>))]
public class DemoMessageConsumer(
    ProcessDemoMessage processDemoMessage
)
    : MedusaConsumer<DemoMessage>
{
    protected override Task Consume(ConsumeContext<DemoMessage> ctx)
    {
        return processDemoMessage.RunAsync(ctx.Message, ctx.CancellationToken);
    }
}
