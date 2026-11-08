using Medusa.Enap.Common.Core.Utils;
using Medusa.Enap.Common.Testing.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Medusa.Enap.Common.Testing.MessageQueue;

public static class DeadLetterTestUtils
{
    public static Task<PublishResult<TParsedMessage>> AssertDeadLetter<TMessage, TParsedMessage>(this Task<PublishResult<TParsedMessage>> prTask,
        ExchangeListener<TMessage> dlxListener,
        string? expectedErrorCode = null, params (string key, string value)[] expectedErrorData
    )
        where TMessage : class
        where TParsedMessage : class
        => prTask.Then(pr => pr.AssertDeadLetter(dlxListener, pr.Message is string s ? s : EnapSerializer.ToJson(pr.Message), expectedErrorCode, expectedErrorData));

    private static async Task<PublishResult<TParsedMessage>> AssertDeadLetter<TMessage, TParsedMessage>(this PublishResult<TParsedMessage> pr,
        ExchangeListener<TMessage> dlxListener,
        string expectedMessage, string? expectedErrorCode, (string key, string value)[] expectedErrorData
    )
    where TMessage : class
    where TParsedMessage : class
    {
        await pr.AssertConsumed();

        pr.MQ.AssertDeadLetter(dlxListener, expectedMessage, expectedErrorCode, expectedErrorData);

        return pr;
    }

    public static void AssertDeadLetter<TMessage>(this TestMessageQueue mq,
        ExchangeListener<TMessage> dlxListener,
        string expectedMessage, string? expectedErrorCode, params (string key, string value)[] expectedErrorData
    )
    {
        dlxListener.AssertPublished(expectedMessage);

        var logStore = mq.Services.GetRequiredService<TestLogStore>();
        if (expectedErrorCode is not null)
            logStore.AssertError(expectedErrorCode, expectedErrorData);
    }
}
