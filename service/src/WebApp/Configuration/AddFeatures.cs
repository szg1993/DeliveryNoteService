using Medusa.Enap.DemoService.Application.UseCases;

namespace Medusa.Enap.DemoService.WebApp.Configuration;

public static class AddFeatures
{
    public static IServiceCollection AddDemoServiceFeatures(this IServiceCollection services, IConfiguration configuration)
    {
        return services.Configure<DemoJobSettings>(configuration.GetSection("DemoJob"));
    }
}