using Common.Core.Infrastructure;
using DeliveryNoteService.Core.Models;
using System.Text.Json.Serialization;

namespace DeliveryNoteService.Core.UseCases.Notes;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public record CreateNoteDto
{
    public string? EkaerNbr { get; init; }

    public string TakeoverPlace { get; init; } = null!;

    public string Category { get; init; } = null!;

    public string? Comments { get; init; }

    public bool IsUrgent { get; init; }

    public bool IsCrackable { get; init; }

    public bool IsDangerous { get; init; }

    public bool IsOwnCost { get; init; }

    public int RecipientId { get; init; }
    public CreateContactInfoDto ContactInfo { get; init; } = null!;
    public CreatePackageDetailDto PackageDetail { get; init; } = null!;
    public CreateDeadlineDto Deadline { get; init; } = null!;
    public IReadOnlyList<CreateNoteLineDto> NoteLines { get; init; } = null!;

    public void Validate()
    {
        NoteInput.Text(EkaerNbr, NoteLimits.EkaerNbr, nameof(EkaerNbr), true);
        NoteInput.Text(TakeoverPlace, NoteLimits.TakeoverPlace, nameof(TakeoverPlace), false);
        NoteInput.Text(Category, NoteLimits.Category, nameof(Category), false);
        NoteInput.Text(Comments, NoteLimits.Comments, nameof(Comments), true);
        NoteInput.Require(RecipientId > 0, nameof(RecipientId));
        NoteInput.Require(ContactInfo is not null && PackageDetail is not null && Deadline is not null, "Details");
        NoteInput.Require(NoteLines is { Count: > 0 } && NoteLines.All(line => line is not null), nameof(NoteLines));
        ContactInfo.Validate();
        PackageDetail.Validate();
        Deadline.Validate();
        foreach (var line in NoteLines)
            line.Validate();
    }

    public Note ToModel(ITimeService timeService) => MapTo(new Note(), timeService);

    public Note MapTo(Note model, ITimeService timeService)
    {
        model.EkaerNbr = NoteInput.Optional(EkaerNbr);
        model.TakeoverPlace = TakeoverPlace.Trim();
        model.Category = Category.Trim();
        model.Comments = NoteInput.Optional(Comments);
        model.IsUrgent = IsUrgent;
        model.IsCrackable = IsCrackable;
        model.IsDangerous = IsDangerous;
        model.IsOwnCost = IsOwnCost;
        model.RecipientId = RecipientId;
        model.CreatedDate = timeService.UtcNow;
        model.NoteStatusId = NoteStatus.Pending;
        model.ContactInfo = ContactInfo.ToModel();
        model.PackageDetail = PackageDetail.ToModel();
        model.Deadline = Deadline.ToModel();
        model.NoteLines = NoteLines.Select((line, index) =>
        {
            var result = line.ToModel();
            result.Position = index;
            return result;
        }).ToList();

        return model;
    }
}