using ApiTests.DtoFactories;
using Medusa.Enap.Common.Core.MessageQueue;
using Microsoft.Extensions.DependencyInjection;

namespace ApiTests;

public class ExchangeDeletedTests : DemoServiceApiTestBase
{
    private const string _demoExchange = "demo-outgoing-exchange";
    private const string _demoRoutingKey = "demo-key";

    [Fact]
    public async Task ExchangeWasDeletedMeantime()
    {
        using var scope = Services.CreateScope();
        var mb = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();

        await mb.PublishAsync(DemoMessageFactory.Create()); // success
        await MessagingFixture.DeleteExchangeAsync(TestId, _demoExchange);
        await mb.PublishAsync(DemoMessageFactory.Create()); // error

        AssertLog("Published to RabbitMQ exchange.", _demoExchange, _demoRoutingKey);
        AssertLog("Publish to RabbitMQ exchange failed.", _demoExchange, _demoRoutingKey);
    }

    private void AssertLog(string message, string exchange, string routing)
    {
        LogStore.GetTechnicalEntry(message)!.Params.Should()
            .Contain(
                KeyValuePair.Create("ExchangeName", exchange),
                KeyValuePair.Create("RoutingKey", routing)
            );
    }
}
