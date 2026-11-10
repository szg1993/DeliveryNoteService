using DeliveryNoteService.Core.Models;

namespace DeliveryNoteService.Core.UseCases.Notes;

public record ContactInfoDetailsDto
{
    public int Id { get; init; }

    public string AssignTo { get; init; } = null!;

    public string AssignToPhone { get; init; } = null!;

    public string Contact { get; init; } = null!;

    public string ContactPhone { get; init; } = null!;

    public static ContactInfoDetailsDto FromModel(NoteContactInfo model)
    {
        return new ContactInfoDetailsDto
        {
            Id = model.Id,
            AssignTo = model.AssignTo,
            AssignToPhone = model.AssignToPhone,
            Contact = model.Contact,
            ContactPhone = model.ContactPhone,
        };
    }
}