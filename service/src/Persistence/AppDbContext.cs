using Medusa.Enap.Common.Infrastructure.Persistence;
using Medusa.Enap.Common.Infrastructure.Scheduling;
using Medusa.Enap.DemoService.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Medusa.Enap.DemoService.Persistence;

public class AppDbContext : AppDbContextBase
{
    public DbSet<DemoModel> DemoModels => Set<DemoModel>();
    public DbSet<Recipient> Recipients => Set<Recipient>();

    public AppDbContext(DbContextOptions options, NewEntityLogger logger)
        : base(logger, options)
    { }

    [ExcludeFromCodeCoverage(Justification = "Only for design time migration generation.")]
    public AppDbContext(DbContextOptions options) : this(options, null!)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddJobExecutionLedger();
        modelBuilder.Entity<DemoModel>().HasIndex(value => value.UniqueId).IsUnique();

        modelBuilder.Entity<Recipient>(recipient =>
        {
            recipient.Property(value => value.Code).HasMaxLength(RecipientLimits.Code).IsRequired();
            recipient.Property(value => value.Name).HasMaxLength(RecipientLimits.Name).IsRequired();
            recipient.Property(value => value.Country).HasMaxLength(RecipientLimits.Country).IsRequired();
            recipient.Property(value => value.Zip).HasMaxLength(RecipientLimits.Zip).IsRequired();
            recipient.Property(value => value.City).HasMaxLength(RecipientLimits.City).IsRequired();
            recipient.Property(value => value.Address).HasMaxLength(RecipientLimits.Address).IsRequired();
            recipient.HasIndex(value => value.Code).IsUnique();
        });

        base.OnModelCreating(modelBuilder);
    }
}