using Medusa.Enap.Common.Core.Integration;

namespace Medusa.Enap.DemoService.WebApp.Configuration;

public static class AddIntegration
{
    public static IServiceCollection AddMedusaHttp(this IServiceCollection services)
    {
        return services
            .AddHttpClient()
            .AddScoped<MedusaApiClientFactory>();
    }
}
