using DeliveryNoteService.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DeliveryNoteService.Persistence;

internal static class NoteMapping
{
    internal const string NumberRangeConstraint = "CK_Notes_NumberRange";

    internal static void ConfigureNotes(this ModelBuilder builder)
    {
        builder.Entity<Note>(note =>
        {
            note.ToTable("Notes", table => table.HasCheckConstraint(NumberRangeConstraint, "\"Id\" >= 1 AND \"Id\" <= 99999"));
            note.Ignore(value => value.NoteNbr);
            note.HasOne(value => value.Recipient).WithMany().HasForeignKey(value => value.RecipientId).OnDelete(DeleteBehavior.Restrict);
            note.HasOne(value => value.ContactInfo).WithOne().HasForeignKey<NoteContactInfo>(value => value.NoteId).IsRequired();
            note.HasOne(value => value.PackageDetail).WithOne().HasForeignKey<NotePackageDetail>(value => value.NoteId).IsRequired();
            note.HasOne(value => value.Deadline).WithOne().HasForeignKey<NoteDeadline>(value => value.NoteId).IsRequired();
            note.HasMany(value => value.NoteLines).WithOne().HasForeignKey(value => value.NoteId).IsRequired();
        });
        builder.Entity<NoteContactInfo>().Property(value => value.AssignTo).HasMaxLength(NoteLimits.AssignTo).IsRequired();
        builder.Entity<NoteContactInfo>().Property(value => value.AssignToPhone).HasMaxLength(NoteLimits.AssignToPhone).IsRequired();
        builder.Entity<NoteContactInfo>().Property(value => value.Contact).HasMaxLength(NoteLimits.Contact).IsRequired();
        builder.Entity<NoteContactInfo>().Property(value => value.ContactPhone).HasMaxLength(NoteLimits.ContactPhone).IsRequired();
        builder.Entity<NotePackageDetail>().Property(value => value.TareWgtUm).HasMaxLength(NoteLimits.TareWgtUm).IsRequired();
        builder.Entity<NotePackageDetail>().Property(value => value.PkgScale).HasMaxLength(NoteLimits.PkgScale).IsRequired();
        builder.Entity<NotePackageDetail>().Property(value => value.PkgSizeUm).HasMaxLength(NoteLimits.PkgSizeUm).IsRequired();

        builder.Entity<NoteLine>().Property(value => value.PartCode).HasMaxLength(NoteLimits.PartCode).IsRequired();
        builder.Entity<NoteLine>().Property(value => value.PartDesc).HasMaxLength(NoteLimits.PartDesc).IsRequired();
        builder.Entity<NoteLine>().Property(value => value.PartCmt).HasMaxLength(NoteLimits.PartCmt);
        builder.Entity<NoteLine>().Property(value => value.PartQtyUm).HasMaxLength(NoteLimits.PartQtyUm).IsRequired();
        builder.Entity<NoteLine>().Property(value => value.PartWgtUm).HasMaxLength(NoteLimits.PartWgtUm).IsRequired();
        builder.Entity<Note>().Property(value => value.EkaerNbr).HasMaxLength(NoteLimits.EkaerNbr);
        builder.Entity<Note>().Property(value => value.TakeoverPlace).HasMaxLength(NoteLimits.TakeoverPlace).IsRequired();
        builder.Entity<Note>().Property(value => value.Category).HasMaxLength(NoteLimits.Category).IsRequired();
        builder.Entity<Note>().Property(value => value.Comments).HasMaxLength(NoteLimits.Comments);
        builder.Entity<NoteLine>().HasIndex(value => new { value.NoteId, value.Position }).IsUnique();
    }
}