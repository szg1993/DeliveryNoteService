using Common.Core.Exceptions;
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
        if (string.IsNullOrWhiteSpace(AssignTo))
            throw new FieldMissingException(nameof(AssignTo));
        Guard.ThrowIfTooLong(AssignTo, NoteLimits.AssignTo);
        if (string.IsNullOrWhiteSpace(AssignToPhone))
            throw new FieldMissingException(nameof(AssignToPhone));
        Guard.ThrowIfTooLong(AssignToPhone, NoteLimits.AssignToPhone);
        if (string.IsNullOrWhiteSpace(Contact))
            throw new FieldMissingException(nameof(Contact));
        Guard.ThrowIfTooLong(Contact, NoteLimits.Contact);
        if (string.IsNullOrWhiteSpace(ContactPhone))
            throw new FieldMissingException(nameof(ContactPhone));
        Guard.ThrowIfTooLong(ContactPhone, NoteLimits.ContactPhone);
    }

    public NoteContactInfo ToModel() => MapTo(new NoteContactInfo());

    public NoteContactInfo MapTo(NoteContactInfo model)
    {
        model.AssignTo = AssignTo;
        model.AssignToPhone = AssignToPhone;
        model.Contact = Contact;
        model.ContactPhone = ContactPhone;

        return model;
    }
}