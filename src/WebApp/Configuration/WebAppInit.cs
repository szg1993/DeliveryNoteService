using DeliveryNoteService.WebApp.RestApi;

namespace DeliveryNoteService.WebApp.Configuration;

public static class WebAppInit
{
    public static IServiceCollection AddDeliveryNoteServiceWebApp(this IServiceCollection services)
    {
        return services.AddSwaggerGen(options => options.SchemaFilter<NoteContractSchema>());
    }
}