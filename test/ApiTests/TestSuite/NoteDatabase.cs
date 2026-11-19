using DeliveryNoteService.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ApiTests.TestSuite;

public static class NoteDatabase
{
    public static async Task SetNoteIdentity(this DeliveryNoteServiceApiTestBase test, int lastId)
    {
        using var scope = test.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>().Database;
        await database.ExecuteSqlRawAsync("INSERT INTO sqlite_sequence(name, seq) SELECT 'Notes', 0 WHERE NOT EXISTS (SELECT 1 FROM sqlite_sequence WHERE name='Notes')");
        await database.ExecuteSqlInterpolatedAsync($"UPDATE sqlite_sequence SET seq={lastId} WHERE name='Notes'");
    }

    public static async Task RejectNotesWithUnrelatedConstraint(this DeliveryNoteServiceApiTestBase test)
    {
        using var scope = test.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>().Database;
        await database.ExecuteSqlRawAsync(
            """
            CREATE TABLE UnrelatedNoteGate (
                Id INTEGER CONSTRAINT CK_Notes_NumberRange_Other CHECK (Id < 0)
            );
            CREATE TRIGGER RejectNoteForUnrelatedConstraint BEFORE INSERT ON Notes
            BEGIN
                INSERT INTO UnrelatedNoteGate(Id) VALUES (1);
            END;
            """
        );
    }
}