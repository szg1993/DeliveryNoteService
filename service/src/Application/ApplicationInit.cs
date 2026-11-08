using Microsoft.Extensions.DependencyInjection;

namespace Medusa.Enap.DemoService.Application;
public static class ApplicationInit
{
    public static IServiceCollection AddDemoServiceApplication(this IServiceCollection services)
    {
        // add services to IoC here
        return services;
    }
}
