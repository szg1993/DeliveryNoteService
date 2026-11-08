using ApiTests.DtoFactories;
using Medusa.Enap.Common.Core.Exceptions;
using Medusa.Enap.Common.Testing.MessageQueue;
using Medusa.Enap.DemoService.Application;
using RichardSzalay.MockHttp;
using System.Net;

namespace ApiTests;

public class DemoMessageTests : DemoServiceApiTestBase
{
    [Fact]
    public async Task DemoTestMethod()
    {
        await MQ.PublishAsync(DemoMessageFactory.Create()
            .WithName("test name")
        ).AssertConsumed();

        var demoModel = (await Api.DemoModels().GetAsync()).Content.Single();
        demoModel.Name.Should().Be("test name");
    }

    [Fact]
    public async Task DemoTestMethodWithError()
    {
        var dlxListener = await MQ.ListenToDeadLetterExchange<DemoMessage>();
        var msg = DemoMessageFactory.Create().WithName(null!);

        await MQ
            .PublishAsync(msg)
            .AssertDeadLetter(dlxListener, CommonErrorCodes.FieldMissing, ("Field", "Name"));
    }

    [Fact]
    public async Task CorrelationId_PassedOn()
    {
        var correlationId = Guid.NewGuid();
        var wasCorrelationIdPassedOn = false;
        Given.HttpClient(h => h
            .When("http://test.com/path?query")
            .With(msg => wasCorrelationIdPassedOn =
                msg.Headers.TryGetValues("X-Correlation-ID", out var headerValue)
                && Guid.TryParse(headerValue.Single(), out Guid parsedHeaderValue)
                && parsedHeaderValue.Equals(correlationId)
            )
            .Respond(HttpStatusCode.OK)
        );

        await MQ.PublishAsync(DemoMessageFactory.Create()
            .WithApiCall(), correlationId
        ).AssertConsumed();

        wasCorrelationIdPassedOn.Should().BeTrue();
    }
}
