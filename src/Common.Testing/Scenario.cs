namespace Medusa.Enap.Common.Testing;

public interface IScenario
{
    IServiceProvider Services { get; }

    Api Api { get; }
}

public class Scenario<TApi>(TApi api, IServiceProvider services) : IScenario where TApi : Api
{
    public IServiceProvider Services => services;

    public TApi Api => api;

    Api IScenario.Api => api;
}