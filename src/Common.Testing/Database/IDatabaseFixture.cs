using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Common.Testing.Database;

public interface IDatabaseFixture : IAsyncLifetime
{
    NullSortingStyle NullSortingStyle { get; }

    bool SupportsCluster { get; }

    Task<string> CreateTestDbAsync(string testId);

    Task ClearTestDbAsync(string testId);

    void ConfigureTestServices(string testId, IServiceCollection serviceCollection);
}