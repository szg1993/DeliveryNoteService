using FluentAssertions;
using Common.Core.Exceptions;
using Common.Core.Integration;
using Common.Core.Utils;
using System.Net;

namespace UnitTests;

public class NonSuccessResponseExceptionTests
{
    [Fact]
    public void Basic()
    {
        var e = new NonSuccessResponseException(HttpStatusCode.NotFound, "test body", "test Url");

        e.StatusCode.Should().Be(HttpStatusCode.NotFound);
        e.ResponseBody.Should().Be("test body");
        e.Url.Should().Be("test Url");
        e.GetErrorCode().Should().Be("error:DNC::Unknown");
    }

    [Fact]
    public void ErrorCodeFromResponseBody()
    {
        var responseBody = CommonSerializer.ToJson(new MedusaError { ErrorCode = "test error code" });

        var e = new NonSuccessResponseException(HttpStatusCode.NotFound, responseBody, "dummy url");

        e.GetErrorCode().Should().Be("test error code");
    }
}