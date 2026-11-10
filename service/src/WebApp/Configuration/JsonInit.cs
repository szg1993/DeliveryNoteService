using Common.Core;

namespace DeliveryNoteService.WebApp.Configuration;

public static class JsonInit
{
    public static IServiceCollection AddJsonOptions(this IServiceCollection services)
        => services.ConfigureHttpJsonOptions(opt => JsonConfiguration.Configure(opt.SerializerOptions));
}