using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Medusa.Enap.Common.Infrastructure.Persistence;

/// <summary>
/// Used for design time migration creation and apply. No startup project needed.
/// Standing in the repository root folder:
/// To create migration run: dotnet ef migrations add <MIGRATION_NAME> -p .\src\Persistence\Persistence.csproj
/// To apply migrations run: dotnet ef database update -p .\src\Persistence\Persistence.csproj
/// </summary>
public abstract class DesignTimeDbContextFactory<TDbContext> : IDesignTimeDbContextFactory<TDbContext>
    where TDbContext : DbContext
{
    public TDbContext CreateDbContext(string[] args)
    {
        var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "..\\WebApp");

        var devConnectionString = new ConfigurationBuilder()
            .SetBasePath(webRootPath)
            .AddJsonFile("appsettings.Development.json")
            .Build()
            .GetSection("Persistence")
            .GetConnectionString("AppDb");

        var options = new DbContextOptionsBuilder<TDbContext>().UseNpgsql(devConnectionString).Options;
        return (TDbContext)Activator.CreateInstance(typeof(TDbContext), args: [options])!;
    }
}