using Medusa.Enap.Common.Testing.MessageQueue;
using Medusa.Enap.DemoService.WebApp.Messaging;

namespace ApiTests.TestSuite.Messaging;

internal static class ExchangeListenerUtils
{
    public static Task<ExchangeListener<TMessage>> ListenToDeadLetterExchange<TMessage>(this TestMessageQueue mq)
        => mq.ListenToDeadLetterExchange<TMessage>(mq.Services.GetConfig<Channels>().Outgoing);

    public static Task<ExchangeListener<TMessage>> ListenToExchange<TMessage>(this TestMessageQueue mq)
        => mq.ListenToExchange<TMessage>(mq.Services.GetConfig<Channels>().Outgoing);
}
