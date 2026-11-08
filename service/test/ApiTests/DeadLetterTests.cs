using ApiTests.DtoFactories;
using Medusa.Enap.Common.Core.Exceptions;
using Medusa.Enap.Common.MessageQueue;
using Medusa.Enap.Common.Testing.MessageQueue;
using Medusa.Enap.DemoService.Application;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace ApiTests;

public class DeadLetterTests : DemoServiceApiTestBase
{
    private readonly Mock<ICorrelationIdExtractor> _extractorMock = new();
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services.AddSingleton(_extractorMock.Object);
    }

    [Fact]
    public async Task ExternalCorrelationId()
    {
        var externalCorrelationId = Guid.NewGuid();
        var dlxListener = await MQ.ListenToDeadLetterExchange<DemoMessage>();

        await MQ
            .PublishAsync(CreateFaultyMsg(), correlationId: externalCorrelationId)
            .AssertDeadLetter(dlxListener);

        AssertBusinessLogs(businessId: externalCorrelationId.ToString());
    }

    [Fact]
    public async Task MessageId()
    {
        _extractorMock
            .Setup(e => e.GetCorrelationId(It.IsAny<object>()))
            .Returns("message id");
        var dlxListener = await MQ.ListenToDeadLetterExchange<DemoMessage>();

        await MQ
            .PublishAsync(CreateFaultyMsg(), correlationId: null)
            .AssertDeadLetter(dlxListener);

        AssertBusinessLogs(businessId: "message id");
    }

    [Fact]
    public async Task Malformed()
    {
        var dlxListener = await MQ.ListenToDeadLetterExchange<DemoMessage>();

        await MQ
            .PublishAsync<DemoMessage>("malformed")
            .AssertDeadLetter(dlxListener, CommonErrorCodes.CannotDeserializeMessage);
    }

    private static DemoMessage CreateFaultyMsg()
    {
        return DemoMessageFactory.Create().WithName(null!);
    }

    private void AssertBusinessLogs(string businessId)
    {
        LogStore.AssertBusinessEntry("demoServiceDlqCategory", ["demoServiceDlqLabel", "dlq"], "dlqSaveRun", businessId, "correlationId");
        LogStore.AssertBusinessEntry("demoServiceDlqCategory", ["demoServiceDlqLabel", "dlq"], "dlqSaveSuccess", businessId, "correlationId");
    }
}
