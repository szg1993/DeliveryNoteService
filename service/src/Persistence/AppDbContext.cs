using Common.Infrastructure.Persistence;
using DeliveryNoteService.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace DeliveryNoteService.Persistence;

public class AppDbContext : AppDbContextBase
{
    public DbSet<Note> Notes => Set<Note>();

    public DbSet<Recipient> Recipients => Set<Recipient>();

    public AppDbContext(DbContextOptions options, NewEntityLogger logger)
        : base(logger, options)
    { }

    [ExcludeFromCodeCoverage(Justification = "Only for design time migration generation.")]
    public AppDbContext(DbContextOptions options) : this(options, null!)
    { }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsNoteNumberExhausted(exception))
        {
            throw new Common.Core.Exceptions.DataValidationException(
                DeliveryNoteService.Core.Errors.ErrorCodes.Note.NumberExhausted,
                "The permanent delivery-note number sequence is exhausted."
            );
        }
    }

    private static bool IsNoteNumberExhausted(DbUpdateException exception)
    {
        return exception.InnerException switch
        {
            Npgsql.PostgresException postgres => postgres.SqlState == Npgsql.PostgresErrorCodes.CheckViolation &&
                postgres.ConstraintName == NoteMapping.NumberRangeConstraint,
            Microsoft.Data.Sqlite.SqliteException sqlite => sqlite.SqliteExtendedErrorCode == 275 &&
                sqlite.Message.Contains("CHECK constraint failed: " + NoteMapping.NumberRangeConstraint, StringComparison.Ordinal),
            _ => false
        };
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureRecipients();
        modelBuilder.ConfigureNotes();

        base.OnModelCreating(modelBuilder);
    }
}