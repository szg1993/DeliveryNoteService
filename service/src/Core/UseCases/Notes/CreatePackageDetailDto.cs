using DeliveryNoteService.Core.Models;
using System.Text.Json.Serialization;

namespace DeliveryNoteService.Core.UseCases.Notes;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public record CreatePackageDetailDto
{
    public decimal? TareWgt { get; init; }

    public string TareWgtUm { get; init; } = null!;

    public decimal? PkgQty { get; init; }

    public string PkgScale { get; init; } = null!;

    public decimal? PkgSizeX { get; init; }

    public decimal? PkgSizeY { get; init; }

    public decimal? PkgSizeZ { get; init; }

    public string PkgSizeUm { get; init; } = null!;

    public void Validate()
    {
        NoteInput.Number(TareWgt, nameof(TareWgt), false);
        NoteInput.Text(TareWgtUm, NoteLimits.TareWgtUm, nameof(TareWgtUm), false);
        NoteInput.Number(PkgQty, nameof(PkgQty), true);
        NoteInput.Text(PkgScale, NoteLimits.PkgScale, nameof(PkgScale), false);
        NoteInput.Number(PkgSizeX, nameof(PkgSizeX), false);
        NoteInput.Number(PkgSizeY, nameof(PkgSizeY), false);
        NoteInput.Number(PkgSizeZ, nameof(PkgSizeZ), false);
        NoteInput.Text(PkgSizeUm, NoteLimits.PkgSizeUm, nameof(PkgSizeUm), false);
    }

    public NotePackageDetail ToModel() => MapTo(new NotePackageDetail());

    public NotePackageDetail MapTo(NotePackageDetail model)
    {
        model.TareWgt = TareWgt!.Value;
        model.TareWgtUm = TareWgtUm.Trim();
        model.PkgQty = PkgQty!.Value;
        model.PkgScale = PkgScale.Trim();
        model.PkgSizeX = PkgSizeX!.Value;
        model.PkgSizeY = PkgSizeY!.Value;
        model.PkgSizeZ = PkgSizeZ!.Value;
        model.PkgSizeUm = PkgSizeUm.Trim();

        return model;
    }
}