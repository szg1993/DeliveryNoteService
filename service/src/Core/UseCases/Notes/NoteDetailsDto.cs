using DeliveryNoteService.Core.Models;
using DeliveryNoteService.Core.UseCases.Recipients;

namespace DeliveryNoteService.Core.UseCases.Notes;

public record NoteDetailsDto
{
    public int Id { get; init; }
    public string NoteNbr { get; init; } = null!;
    public DateTimeOffset CreatedDate { get; init; }
    public NoteStatus NoteStatusId { get; init; }
    public int RecipientId { get; init; }
    public RecipientDetailsDto Recipient { get; init; } = null!;
    public ContactInfoDetailsDto ContactInfo { get; init; } = null!;
    public PackageDetailDetailsDto PackageDetail { get; init; } = null!;
    public DeadlineDetailsDto Deadline { get; init; } = null!;
    public IReadOnlyList<NoteLineDetailsDto> NoteLines { get; init; } = null!;
    public string? EkaerNbr { get; init; }

    public string TakeoverPlace { get; init; } = null!;

    public string Category { get; init; } = null!;

    public string? Comments { get; init; }

    public bool IsUrgent { get; init; }

    public bool IsCrackable { get; init; }

    public bool IsDangerous { get; init; }

    public bool IsOwnCost { get; init; }

    public static NoteDetailsDto FromModel(Note model)
    {
        return new NoteDetailsDto
        {
            Id = model.Id,
            NoteNbr = model.NoteNbr,
            CreatedDate = model.CreatedDate,
            NoteStatusId = model.NoteStatusId,
            RecipientId = model.RecipientId,
            Recipient = RecipientDetailsDto.FromModel(model.Recipient),
            ContactInfo = ContactInfoDetailsDto.FromModel(model.ContactInfo),
            PackageDetail = PackageDetailDetailsDto.FromModel(model.PackageDetail),
            Deadline = DeadlineDetailsDto.FromModel(model.Deadline),
            NoteLines = model.NoteLines.OrderBy(line => line.Position).Select(NoteLineDetailsDto.FromModel).ToList(),
            EkaerNbr = model.EkaerNbr,
            TakeoverPlace = model.TakeoverPlace,
            Category = model.Category,
            Comments = model.Comments,
            IsUrgent = model.IsUrgent,
            IsCrackable = model.IsCrackable,
            IsDangerous = model.IsDangerous,
            IsOwnCost = model.IsOwnCost,
        };
    }
}