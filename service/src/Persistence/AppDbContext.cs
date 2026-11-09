using Common.Infrastructure.Persistence;
using DeliveryNoteService.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace DeliveryNoteService.Persistence;

public class AppDbContext : AppDbContextBase
{
    public DbSet<Recipient> Recipients => Set<Recipient>();

    public AppDbContext(DbContextOptions options, NewEntityLogger logger)
        : base(logger, options)
    { }

    [ExcludeFromCodeCoverage(Justification = "Only for design time migration generation.")]
    public AppDbContext(DbContextOptions options) : this(options, null!)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureRecipients();

        base.OnModelCreating(modelBuilder);
    }
}