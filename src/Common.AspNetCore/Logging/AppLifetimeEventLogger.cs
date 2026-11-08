using Medusa.Enap.Common.Core.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Medusa.Enap.Common.AspNetCore.Logging;

public static class AppLifetimeEventLogger
{
    public static WebApplication LogLifetimeEvents(this WebApplication app)
    {
        var logger = app.Services.GetRequiredService<AppLogger>();
        logger.LogTechnical(LogLevel.Information, "Application starting");

        app.Lifetime.ApplicationStarted.Register(()
            => logger.LogTechnical(LogLevel.Information, "Application started")
        );
        app.Lifetime.ApplicationStopping.Register(()
            => logger.LogTechnical(LogLevel.Information, "Application stopping")
        );
        app.Lifetime.ApplicationStopped.Register(()
            => logger.LogTechnical(LogLevel.Information, "Application stopped")
        );

        return app;
    }
}
