using Medusa.Enap.Common.Core.Versioning;
using Microsoft.Extensions.DependencyInjection;

namespace Medusa.Enap.Common.Testing.Versioning;

public static class TestVersionServiceHelper
{
    public static IServiceCollection AddTestVersionService(this IServiceCollection services)
        => services.AddSingleton<ILocalVersionService, TestVersionService>();

    public static void Version(this IScenario scenario, Version version)
        => scenario.Services.GetTestVersionService().SetVersion(version);

    private static TestVersionService GetTestVersionService(this IServiceProvider services)
        => services.GetRequiredService<ILocalVersionService>() as TestVersionService
            ?? throw new InvalidOperationException("TestVersionService is not registered");
}