using Common.Core.Model;

namespace DeliveryNoteService.Core.Models;

public class Note : IModel
{
    public int Id { get; set; }

    public string? EkaerNbr { get; set; }

    public string TakeoverPlace { get; set; } = null!;

    public string Category { get; set; } = null!;

    public string? Comments { get; set; }

    public bool IsUrgent { get; set; }

    public bool IsCrackable { get; set; }

    public bool IsDangerous { get; set; }

    public bool IsOwnCost { get; set; }

    public int RecipientId { get; set; }
    public Recipient Recipient { get; set; } = null!;
    public DateTimeOffset CreatedDate { get; set; }
    public NoteStatus NoteStatusId { get; set; }
    public string NoteNbr => "N" + Id.ToString("D5", System.Globalization.CultureInfo.InvariantCulture);
    public NoteContactInfo ContactInfo { get; set; } = null!;
    public NotePackageDetail PackageDetail { get; set; } = null!;
    public NoteDeadline Deadline { get; set; } = null!;
    public ICollection<NoteLine> NoteLines { get; set; } = new List<NoteLine>();
}