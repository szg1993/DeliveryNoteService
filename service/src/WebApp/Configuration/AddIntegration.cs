using Common.Core.Integration;

namespace DeliveryNoteService.WebApp.Configuration;

public static class AddIntegration
{
    public static IServiceCollection AddMedusaHttp(this IServiceCollection services)
    {
        return services
            .AddHttpClient()
            .AddScoped<MedusaApiClientFactory>();
    }
}