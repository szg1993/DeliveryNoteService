using Common.Core.Exceptions;
using Common.Core.Infrastructure;
using Common.Core.UseCases;
using DeliveryNoteService.Core.Errors;
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
        Guard.ThrowIfTooLong(EkaerNbr?.Trim(), NoteLimits.EkaerNbr, nameof(EkaerNbr));
        Guard.ThrowIfMissing(TakeoverPlace?.Trim(), nameof(TakeoverPlace));
        Guard.ThrowIfTooLong(TakeoverPlace?.Trim(), NoteLimits.TakeoverPlace, nameof(TakeoverPlace));
        Guard.ThrowIfMissing(Category?.Trim(), nameof(Category));
        Guard.ThrowIfTooLong(Category?.Trim(), NoteLimits.Category, nameof(Category));
        Guard.ThrowIfTooLong(Comments?.Trim(), NoteLimits.Comments, nameof(Comments));
        if (RecipientId <= 0)
            throw new DataValidationException(ErrorCodes.Note.InvalidInput, "Invalid RecipientId.");
        Guard.ThrowIfMissing(ContactInfo);
        Guard.ThrowIfMissing(PackageDetail);
        Guard.ThrowIfMissing(Deadline);
        Guard.ThrowIfMissingOrEmpty(NoteLines);
        if (NoteLines.Any(line => line is null))
            throw new DataValidationException(ErrorCodes.Note.InvalidInput, "Invalid NoteLines.");
        ContactInfo.Validate();
        PackageDetail.Validate();
        Deadline.Validate();
        foreach (var line in NoteLines)
            line.Validate();
    }

    public Note ToModel(ITimeService timeService) => MapTo(new Note(), timeService);

    public Note MapTo(Note model, ITimeService timeService)
    {
        model.EkaerNbr = string.IsNullOrWhiteSpace(EkaerNbr) ? null : EkaerNbr.Trim();
        model.TakeoverPlace = TakeoverPlace.Trim();
        model.Category = Category.Trim();
        model.Comments = string.IsNullOrWhiteSpace(Comments) ? null : Comments.Trim();
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