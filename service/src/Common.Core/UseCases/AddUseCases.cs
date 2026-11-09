using Common.Core.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Core.UseCases;

public static class AddUseCases
{
    public static IServiceCollection AddMedusaUseCases(this IServiceCollection services)
    {
        var interfaceType = typeof(IUseCase);
        var useCaseTypes = AssemblyUtils
            .GetMedusaAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => interfaceType.IsAssignableFrom(type) && type.IsClass && !type.IsAbstract);

        foreach (var type in useCaseTypes)
            services.AddScoped(type);

        return services;
    }
}