using Medusa.Enap.DemoService.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Medusa.Enap.DemoService.Persistence;

internal static class ModelBuilderExtensions
{
    internal static ModelBuilder ConfigureDemoModel(this ModelBuilder builder)
    {
        builder
            .Entity<DemoModel>()
            .HasIndex(dm => dm.UniqueId)
            .IsUnique();

        return builder;
    }
}
