using Medusa.Enap.Common.Core.Infrastructure;
using Medusa.Enap.Common.Infrastructure.Scheduling.Clustering;
using Medusa.Enap.Common.Testing.Database;
using Microsoft.Extensions.DependencyInjection;

namespace Medusa.Enap.Common.Testing.Infrastructure;

public static class TestScheduling
{
    public static IServiceCollection AddTestScheduling(this IServiceCollection services, IDatabaseFixture database)
    {
        if(!database.SupportsCluster)
            services.AddKeyedSingleton<ILockProvider>(ClusteredLockProvider.Key, (sp, _) => sp.GetRequiredKeyedService<ILockProvider>(LocalLockProvider.Key));

        return services;
    }
}
