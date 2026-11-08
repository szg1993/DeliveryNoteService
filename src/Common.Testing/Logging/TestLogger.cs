using Medusa.Enap.Common.Core.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Medusa.Enap.Common.Testing.Logging;

public class TestLogger : MedusaLogger<TestLogger>
{
    public TestLogger(ILogger<TestLogger> logger) : base(logger)
    {
        PushCorrelationIdProperty(correlationId: Guid.NewGuid().ToString());
    }

    public void LogInfo(string message)
        => LogTechnical(LogLevel.Information, message);
}

public static class TestLoggerHelper
{
    public static void WriteTestLog(this IScenario scenario, string message) => scenario.GetTestLogger().LogInfo(message);

    public static TestLogger GetTestLogger(this IScenario scenario) => scenario.Services.GetTestLogger();

    public static TestLogger GetTestLogger(this IServiceProvider sp) => sp.GetRequiredService<TestLogger>();
}