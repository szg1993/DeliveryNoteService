using Common.Core.Model;

namespace DeliveryNoteService.Core.Models;

public class NotePackageDetail : IModel
{
    public int Id { get; set; }
    public int NoteId { get; set; }

    public decimal TareWgt { get; set; }

    public string TareWgtUm { get; set; } = null!;

    public decimal PkgQty { get; set; }

    public string PkgScale { get; set; } = null!;

    public decimal PkgSizeX { get; set; }

    public decimal PkgSizeY { get; set; }

    public decimal PkgSizeZ { get; set; }

    public string PkgSizeUm { get; set; } = null!;

}