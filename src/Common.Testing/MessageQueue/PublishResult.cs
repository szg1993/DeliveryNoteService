using FluentAssertions;
using Medusa.Enap.Common.Core.Exceptions;
using Medusa.Enap.Common.Core.Utils;

namespace Medusa.Enap.Common.Testing.MessageQueue;

public record PublishResult<TMessage>(TMessage Message, TestMessageQueue MQ, string? CorrelationId);

public static class PublishResultExtensions
{
    public static async Task<Exception> AssertFailed<TMessage>(this Task<PublishResult<TMessage>> publishResultTask, string expectedErrorCode)
        where TMessage : class
    {
        var p = await publishResultTask;
        var ex = await p.MQ.AssertFailed<TMessage>();
        ex.GetErrorCode().Should().Be(expectedErrorCode);
        return ex;
    }

    public static Task<TMessage> AssertConsumed<TMessage>(this Task<PublishResult<TMessage>> publishResultTask)
        where TMessage : class
            => publishResultTask.Then(pr => pr.AssertConsumed());

    public static Task<TMessage> AssertConsumed<TMessage>(this PublishResult<TMessage> pr)
        where TMessage : class
            => pr.MQ.AssertConsumed<TMessage>(consumed => SerializedEquals(pr.Message, consumed));

    public static async Task WithData(this Task<Exception> ex, string key, string expectedValue)
    {
        var error = await ex;
        error.Data[key]!.Should().BeEquivalentTo(expectedValue);
    }

    private static bool SerializedEquals<TMessage>(TMessage m1, TMessage m2)
      where TMessage : class
    {
        return string.Equals(EnapSerializer.ToJson(m1), EnapSerializer.ToJson(m2));
    }
}