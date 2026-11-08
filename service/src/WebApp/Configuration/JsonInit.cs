using Medusa.Enap.Common.Core.Configuration;

namespace Medusa.Enap.DemoService.WebApp.Configuration;

public static class JsonInit
{
    public static IServiceCollection AddMedusaJsonOptions(this IServiceCollection services)
        => services.ConfigureHttpJsonOptions(opt => JsonConfiguration.Configure(opt.SerializerOptions));
}
