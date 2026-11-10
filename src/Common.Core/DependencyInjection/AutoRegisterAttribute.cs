using Microsoft.Extensions.DependencyInjection;

namespace Common.Core.DependencyInjection;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class AutoRegisterAttribute(ServiceLifetime lifetime = ServiceLifetime.Scoped, Type? serviceKey = null) : Attribute
{
    public ServiceLifetime Lifetime { get; } = lifetime;

    public Type? ServiceKey { get; } = serviceKey;
}