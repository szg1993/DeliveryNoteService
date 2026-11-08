using FluentAssertions;
using Medusa.Enap.Common.Core.Exceptions;
using Medusa.Enap.Common.Core.Utils;

namespace Medusa.Enap.Common.Testing.MessageQueue;

public static class ProcessResultExtensions
{
    public static Task<MedusaError> AssertError<TResult>(this Task<TResult> processResultTask, string expectedErrorCode)
        where TResult : IHaveErrorList
        => processResultTask.Then(pr => AssertError(pr, expectedErrorCode));
    
    public static MedusaError AssertError<TResult>(this TResult processResult, string expectedErrorCode)
        where TResult : IHaveErrorList
    {
        processResult.Errors.Should().HaveCount(1);
        var e = processResult.Errors.Single();
        e.ErrorCode.Should().Be(expectedErrorCode);
        return e;
    }
}
