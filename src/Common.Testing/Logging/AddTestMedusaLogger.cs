using Common.AspNetCore.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Compact;
using System.Text.RegularExpressions;
using Xunit.Abstractions;

namespace Common.Testing.Logging;

public static class TestMedusaLoggerHelper
{
    public static TestLogStore GetLogStore(this IServiceProvider sp) => sp.GetRequiredService<TestLogStore>();

    public static IServiceCollection AddTestMedusaLogging(this IServiceCollection services, string testId, ITestOutputHelper output)
    {
        return services
                .AddSingleton<TestLogStore>()
                .AddTransient<TestLogger>()
                .AddSerilog((sp, serilogConfig) =>
                {
                    var config = sp.GetService<IConfiguration>();

                    if (config is not null)
                        serilogConfig.ReadFrom.Configuration(config);

                    serilogConfig
                        .Enrich.WithProperty("Module", "Api Test")
                        .Enrich.With<ClassNameEnricher>()
                        .Enrich.With<ParamsSeparatorEnricher>()
                        .WriteTo.Sink(new TestLogSink(sp.GetRequiredService<TestLogStore>()))
                        .WriteTo.TestOutput(output, outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] [{ClassName}] {Message:lj} {ErrorCode}{paramsWithSeparator}{NewLine}{Exception}")
                        //.WriteTo.TestOutput(output, new MedusaLogFormatter())
                        //.WriteToSeq(config?.GetSection("Logging"))
                        .WriteTo.File(FileLogger.Formatter, FileLogger.GetTestLogFilePath(testId, output));

                    if (config is not null)
                        serilogConfig.CopyMicrosoftMinimumLogLevels(config);
                }, preserveStaticLogger: true
                )
                ;
    }
}

file static class FileLogger
{
    public static ITextFormatter Formatter => new RenderedCompactJsonFormatter();

    public static string GetTestLogFilePath(string testId, ITestOutputHelper output)
    {
        var testName = NormalizeFileName(output.GetTestCase()!.DisplayName);
        return Path.Combine("Logs", $"{testName}_{testId}.log");
    }

    private static string NormalizeFileName(string fileName)
    {
        // Replace invalid characters with '_'
        var invalidChars = new string(Path.GetInvalidFileNameChars()) + "·";
        var sanitizedFileName = Regex.Replace(fileName, $"[{Regex.Escape(invalidChars)}]", "_");

        // Truncate file name to 255 characters (if needed)
        var maxFileNameLength = 255 - $"_{Guid.Empty}".Length;
        if (sanitizedFileName.Length > maxFileNameLength)
            sanitizedFileName = sanitizedFileName.Substring(0, maxFileNameLength);

        return sanitizedFileName;
    }
}

file class ClassNameEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent.Properties.TryGetValue("SourceContext", out var sourceContext))
        {
            var fullName = sourceContext.ToString().Trim('"');
            var className = fullName.Split('.').Last();
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ClassName", className));
        }
    }
}

file class ParamsSeparatorEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent.Properties.TryGetValue("Params", out var logParams)
            && logParams is DictionaryValue dv
            && dv.Elements.Count > 0
        )
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("paramsWithSeparator", " | params: " + logParams));
        }
    }
}