using Common.Core.UseCases;
using DeliveryNoteService.Core.Models;
using System.Text.Json.Serialization;

namespace DeliveryNoteService.Core.UseCases.Notes;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public record CreateContactInfoDto
{
    public string AssignTo { get; init; } = null!;

    public string AssignToPhone { get; init; } = null!;

    public string Contact { get; init; } = null!;

    public string ContactPhone { get; init; } = null!;

    public void Validate()
    {
        Guard.ThrowIfMissing(AssignTo?.Trim(), nameof(AssignTo));
        Guard.ThrowIfTooLong(AssignTo?.Trim(), NoteLimits.AssignTo, nameof(AssignTo));
        Guard.ThrowIfMissing(AssignToPhone?.Trim(), nameof(AssignToPhone));
        Guard.ThrowIfTooLong(AssignToPhone?.Trim(), NoteLimits.AssignToPhone, nameof(AssignToPhone));
        Guard.ThrowIfMissing(Contact?.Trim(), nameof(Contact));
        Guard.ThrowIfTooLong(Contact?.Trim(), NoteLimits.Contact, nameof(Contact));
        Guard.ThrowIfMissing(ContactPhone?.Trim(), nameof(ContactPhone));
        Guard.ThrowIfTooLong(ContactPhone?.Trim(), NoteLimits.ContactPhone, nameof(ContactPhone));
    }

    public NoteContactInfo ToModel() => MapTo(new NoteContactInfo());

    public NoteContactInfo MapTo(NoteContactInfo model)
    {
        model.AssignTo = AssignTo.Trim();
        model.AssignToPhone = AssignToPhone.Trim();
        model.Contact = Contact.Trim();
        model.ContactPhone = ContactPhone.Trim();

        return model;
    }
}