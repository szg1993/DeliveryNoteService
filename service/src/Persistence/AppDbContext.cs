using Medusa.Enap.Common.Infrastructure.Persistence;
using Medusa.Enap.Common.Infrastructure.Scheduling;
using Medusa.Enap.DemoService.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Medusa.Enap.DemoService.Persistence;

public class AppDbContext : AppDbContextBase
{
    public DbSet<DemoModel> DemoModels { get; set; }

    public AppDbContext(DbContextOptions options, NewEntityLogger logger)
        : base(logger, options)
    { }

    [ExcludeFromCodeCoverage(Justification = "Only for design time migration generation.")]
    public AppDbContext(DbContextOptions options) : this(options, null!)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddJobExecutionLedger();

        base.OnModelCreating(modelBuilder);
    }
}
