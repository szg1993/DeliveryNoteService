using Common.Core.Utils;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Common.Core.DependencyInjection;

public static class AddAutoRegisteredServicesHelper
{
    /// <summary>
    /// Should be called after most of the other AddService methods, so that the assemblies are
    /// already loaded to the current AppDomain.
    /// Registers all services into the DI, which are marked with AutoRegisterAttribute
    /// </summary>
    public static IServiceCollection AddMedusaAutoRegisteredServices(this IServiceCollection services)
    {
        foreach (var assembly in AssemblyUtils.GetMedusaAssemblies())
            AddAutoRegisteredServices(services, assembly);

        return services;
    }

    public static IServiceCollection AddAutoRegisteredServices(this IServiceCollection services, Assembly assembly)
    {
        var typesWithAttribute = assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<AutoRegisterAttribute>() is not null);

        foreach (var type in typesWithAttribute)
        {
            var attribute = type.GetCustomAttribute<AutoRegisterAttribute>();
            services.Add(new ServiceDescriptor(attribute!.ServiceKey ?? type, type, attribute!.Lifetime));
        }

        return services;
    }
}