using Common.Core.Versioning;
using Common.Testing.Versioning;
using RichardSzalay.MockHttp;

namespace ApiTests;

public class VersionsTests : DeliveryNoteServiceApiTestBase
{
    [Fact]
    public async Task ConnectedServices_False()
    {
        Given.Version(new Version(1, 2, 3, 4));

        var r = await Api.Versions.GetAsync(connectedServices: false);

        r.ComponentName.Should().Be("Demo Service");
        r.Version.Should().Be("1.2.3.4");
        r.ConnectedServices.Should().BeNull();
    }

    [Fact]
    public async Task ConnectedServices_True_WithNoConnectedServices()
    {
        Given.Config<VersionSettings>(s => s
            .ConnectedServiceVersionEndpoints = []);

        var r = await Api.Versions.GetAsync(connectedServices: true);

        r.ConnectedServices.Should().BeEmpty();
    }

    [Fact]
    public async Task ConnectedServices_True()
    {
        Given.Config<VersionSettings>(s => s
            .ConnectedServiceVersionEndpoints = ["http://test.com/path?query"]);
        Given.HttpClient(h => h
            .When("http://test.com/path?query")
            .Respond("application/json",
                @" {
                    ""componentName"": ""Test Component"",
                    ""version"": ""test version"",
                    ""connectedServices"": null
                }"));

        var r = await Api.Versions.GetAsync(connectedServices: true);

        r.ConnectedServices.Should().NotBeNull();
        var connectedService = r.ConnectedServices!.Single();
        connectedService.ComponentName.Should().Be("Test Component");
        connectedService.Version.Should().Be("test version");
        connectedService.ConnectedServices.Should().BeNull();
    }

    [Fact]
    public async Task CorrelationId_ReturnedInResponse()
    {
        Api.Client.DefaultRequestHeaders.Add("X-Correlation-ID", "test value");

        var r = await Api.Versions.GetRawAsync(connectedServices: false);

        r.Headers.TryGetValues("X-Correlation-ID", out var correlationIdHeader).Should().BeTrue();
        correlationIdHeader?.First().Should().Be("test value");
    }

    [Fact]
    public async Task CorrelationId_Missing_ThenNewGuidGenerated()
    {
        var r = await Api.Versions.GetRawAsync(connectedServices: true);

        r.Headers.TryGetValues("X-Correlation-ID", out var correlationIdHeader).Should().BeTrue();
        Guid.TryParse(correlationIdHeader!.First(), out _).Should().BeTrue();
    }

    [Fact]
    public async Task CorrelationId_PassedTo_OutboundApiCalls()
    {
        bool wasCorrelationIdPassedOn = false;
        Api.Client.DefaultRequestHeaders.Add("X-Correlation-ID", "test value");
        Given.Config<VersionSettings>(s => s
            .ConnectedServiceVersionEndpoints = ["http://test.com/path?query"]);
        Given.HttpClient(h => h
            .When("http://test.com/path?query")
            .WithHeaders("X-Correlation-ID", "test value")
            .With(_ => wasCorrelationIdPassedOn = true));

        _ = await Api.Versions.GetAsync(connectedServices: true);

        wasCorrelationIdPassedOn.Should().BeTrue();
    }
}