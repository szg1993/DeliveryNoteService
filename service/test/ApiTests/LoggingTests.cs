using Medusa.Enap.Common.AspNetCore.Logging;
using Medusa.Enap.Common.Core.Exceptions;
using Medusa.Enap.Common.Core.Logging;
using Medusa.Enap.Common.Testing.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ApiTests;

public class LoggingTests : DemoServiceApiTestBase
{
    protected override void ConfigureTestHostConfiguration(IConfigurationBuilder configBuilder)
    {
        base.ConfigureTestHostConfiguration(configBuilder);
        configBuilder.Add("Diag:RequestLoggingEnabled", "true");
    }

    [Fact]
    public void Startup()
    {
        LogStore.AssertTechnicalEntry("Application starting");
        LogStore.AssertTechnicalEntry("Application started");
        LogStore.AssertNoErrors();
    }

    [Fact]
    public async Task RequestLogging()
    {
        await Api.DemoModels().GetAsync(QueryString.Create("testParam", "42"));

        LogStore.AssertTechnicalEntry("Incoming request")
            .AssertParam("url", p => p.Should().Contain("?testParam=42"));
    }

    [Fact]
    public void AssertParams()
    {
        using var scope = Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<MedusaLogger<LoggingTests>>();
        var ex = new MedusaException("test error code", "")
            .SetData("test_data", "test value");

        logger.LogTechnical(LogLevel.Error, ex, "dummy msg", new() { { "test_data_2", "test value 2" } });

        LogStore.AssertError("test error code")
            .AssertParam("test_data", "test value")
            .AssertParam("test_data_2", "test value 2");
    }

    [Fact]
    public async Task TechnicalUseCaseLogging_Successful_Async()
    {
        using var scope = Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<TechnicalUseCaseLogger<LoggingTests>>();

        await logger.RunAsync(() => Task.CompletedTask);

        LogStore.AssertNoErrors();
        LogStore.AssertTechnicalEntry("LoggingTests started.");
        LogStore.AssertTechnicalEntry("LoggingTests finished.");
    }

    [Fact]
    public void TechnicalUseCaseLogging_Successful_Synchronous()
    {
        using var scope = Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<TechnicalUseCaseLogger<LoggingTests>>();

        logger.Run(() => "TEST");

        LogStore.AssertNoErrors();
        LogStore.AssertTechnicalEntry("LoggingTests started.");
        LogStore.AssertTechnicalEntry("LoggingTests finished.");
    }

    [Fact]
    public async Task TechnicalUseCaseLogging_Failed()
    {
        using var scope = Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<TechnicalUseCaseLogger<LoggingTests>>();

        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            logger.RunAsync(() => 
                Task.FromException(new InvalidOperationException())
            )
        );

        LogStore.AssertTechnicalEntry("LoggingTests started.");
        LogStore
            .AssertTechnicalEntry("LoggingTests failed.")
            .AssertLevel(LogLevels.Warn);
    }

    [Fact]
    public void AssertBusinessLog()
    {
        using var scope = Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<MedusaLogger<LoggingTests>>();

        logger.Log(LogLevel.Error, "category", ["label1", "label2"], "event id", "business id", "business id type");

        LogStore.AssertBusinessEntry("category", ["label1", "label2"], "event id", "business id", "business id type");
    }
}
