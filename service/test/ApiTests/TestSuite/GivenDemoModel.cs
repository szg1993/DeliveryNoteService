using Medusa.Enap.DemoService.Domain.Models;
using Medusa.Enap.DemoService.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace ApiTests.TestSuite;

internal static class GivenDemoModel
{
    public static async Task<IEnumerable<DemoModel>> DemoModels(this IScenario scenario, int count)
    {
        var db = scenario.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();
        var models = Enumerable.Range(0, count).Select(i => new DemoModel() { Name = "dummy model name " + i });
        db.DemoModels.AddRange(models);
        await db.SaveChangesAsync();
        return models;
    }
}
