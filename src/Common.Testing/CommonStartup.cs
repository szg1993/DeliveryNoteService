using Common.Core.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Testing;

public static class CommonStartup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        ThreadPool.SetMinThreads(100, 100);

        if (Directory.Exists("Logs"))
            Directory.GetFiles("Logs")
                .ForEach(File.Delete);
    }
}