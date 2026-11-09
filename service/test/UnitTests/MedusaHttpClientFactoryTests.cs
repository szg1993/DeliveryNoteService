using FluentAssertions;
using Common.Core.Integration;
using Microsoft.Extensions.Options;
using Moq;

namespace UnitTests;

public class MedusaHttpClientFactoryTests
{
    private const string _testUrl = "http://test.com/";
    private const string _correlationId = "test-correlation";

    [Fact]
    public void Created()
    {
        var sp = new Mock<IServiceProvider>();
        sp.Setup(s => s
            .GetService(typeof(IOptions<string>)))
            .Returns(Mock.Of<IOptions<string>>()
       );
        var clientFactory = new Mock<IHttpClientFactory>();
        clientFactory.Setup(f => f
            .CreateClient(It.IsAny<string>()))
            .Returns(new HttpClient());
        var accessor = new CorrelationIdAccessor { CorrelationId = _correlationId };
        var medusaClientFactory = new MedusaApiClientFactory(sp.Object, clientFactory.Object, accessor);

        var client = medusaClientFactory.CreateApiClient<string, HttpClient>(_ => _testUrl, _ => _);

        client.Should().NotBeNull();
        client.BaseAddress!.ToString().Should().Be(_testUrl);
        client.DefaultRequestHeaders.Should().ContainEquivalentOf(KeyValuePair.Create("X-Correlation-ID", new string[] { _correlationId }));
    }
}