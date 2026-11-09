using DeliveryNoteService.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DeliveryNoteService.Persistence;

internal static class ModelBuilderExtensions
{
    internal static ModelBuilder ConfigureRecipients(this ModelBuilder builder)
    {
        builder.Entity<Recipient>(recipient =>
        {
            recipient.Property(value => value.Code).HasMaxLength(RecipientLimits.Code).IsRequired();
            recipient.Property(value => value.Name).HasMaxLength(RecipientLimits.Name).IsRequired();
            recipient.Property(value => value.Country).HasMaxLength(RecipientLimits.Country).IsRequired();
            recipient.Property(value => value.Zip).HasMaxLength(RecipientLimits.Zip).IsRequired();
            recipient.Property(value => value.City).HasMaxLength(RecipientLimits.City).IsRequired();
            recipient.Property(value => value.Address).HasMaxLength(RecipientLimits.Address).IsRequired();
            recipient.HasIndex(value => value.Code).IsUnique();
        });

        return builder;
    }
}
