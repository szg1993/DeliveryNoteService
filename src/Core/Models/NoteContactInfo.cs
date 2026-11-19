using Common.Core.Model;

namespace DeliveryNoteService.Core.Models;

public class NoteContactInfo : IModel
{
    public int Id { get; set; }
    public int NoteId { get; set; }

    public string AssignTo { get; set; } = null!;

    public string AssignToPhone { get; set; } = null!;

    public string Contact { get; set; } = null!;

    public string ContactPhone { get; set; } = null!;

}