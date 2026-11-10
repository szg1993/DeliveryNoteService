using DeliveryNoteService.Core.Models;

namespace DeliveryNoteService.Core.UseCases.Notes;

public record PackageDetailDetailsDto
{
    public int Id { get; init; }

    public decimal TareWgt { get; init; }

    public string TareWgtUm { get; init; } = null!;

    public decimal PkgQty { get; init; }

    public string PkgScale { get; init; } = null!;

    public decimal PkgSizeX { get; init; }

    public decimal PkgSizeY { get; init; }

    public decimal PkgSizeZ { get; init; }

    public string PkgSizeUm { get; init; } = null!;

    public static PackageDetailDetailsDto FromModel(NotePackageDetail model)
    {
        return new PackageDetailDetailsDto
        {
            Id = model.Id,
            TareWgt = model.TareWgt,
            TareWgtUm = model.TareWgtUm,
            PkgQty = model.PkgQty,
            PkgScale = model.PkgScale,
            PkgSizeX = model.PkgSizeX,
            PkgSizeY = model.PkgSizeY,
            PkgSizeZ = model.PkgSizeZ,
            PkgSizeUm = model.PkgSizeUm,
        };
    }
}