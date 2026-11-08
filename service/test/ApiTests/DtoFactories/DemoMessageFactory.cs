using Medusa.Enap.DemoService.Application;

namespace ApiTests.DtoFactories;

public static class DemoMessageFactory
{
    public static DemoMessage Create() => new() { Name = "dummy" };

    public static DemoMessage WithName(this DemoMessage self, string name)
        => self with { Name = name };

    public static DemoMessage WithApiCall(this DemoMessage self)
        => self with { MakeApiCall = true };
}