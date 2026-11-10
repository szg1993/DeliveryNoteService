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
        NoteInput.Text(AssignTo, NoteLimits.AssignTo, nameof(AssignTo), false);
        NoteInput.Text(AssignToPhone, NoteLimits.AssignToPhone, nameof(AssignToPhone), false);
        NoteInput.Text(Contact, NoteLimits.Contact, nameof(Contact), false);
        NoteInput.Text(ContactPhone, NoteLimits.ContactPhone, nameof(ContactPhone), false);
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