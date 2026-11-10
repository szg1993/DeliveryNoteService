using Common.Core.Exceptions;
using Common.Core.UseCases;
using DeliveryNoteService.Core.Errors;
using DeliveryNoteService.Core.Models;
using System.Text.Json.Serialization;

namespace DeliveryNoteService.Core.UseCases.Notes;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public record CreatePackageDetailDto
{
    [JsonConverter(typeof(ExactNoteDecimalConverter))]
    public decimal? TareWgt { get; init; }

    public string TareWgtUm { get; init; } = null!;

    [JsonConverter(typeof(ExactNoteDecimalConverter))]
    public decimal? PkgQty { get; init; }

    public string PkgScale { get; init; } = null!;

    [JsonConverter(typeof(ExactNoteDecimalConverter))]
    public decimal? PkgSizeX { get; init; }

    [JsonConverter(typeof(ExactNoteDecimalConverter))]
    public decimal? PkgSizeY { get; init; }

    [JsonConverter(typeof(ExactNoteDecimalConverter))]
    public decimal? PkgSizeZ { get; init; }

    public string PkgSizeUm { get; init; } = null!;

    public void Validate()
    {
        Guard.ThrowIfMissing(TareWgt);
        Guard.ThrowIfMissing(PkgQty);
        Guard.ThrowIfMissing(PkgSizeX);
        Guard.ThrowIfMissing(PkgSizeY);
        Guard.ThrowIfMissing(PkgSizeZ);
        if (PkgQty <= 0 || TareWgt < 0 || PkgSizeX < 0 || PkgSizeY < 0 || PkgSizeZ < 0)
            throw new DataValidationException(ErrorCodes.Note.InvalidInput, "Invalid package quantity, weight or dimensions.");
        Guard.ThrowIfMissing(TareWgtUm?.Trim(), nameof(TareWgtUm));
        Guard.ThrowIfTooLong(TareWgtUm?.Trim(), NoteLimits.TareWgtUm, nameof(TareWgtUm));
        Guard.ThrowIfMissing(PkgScale?.Trim(), nameof(PkgScale));
        Guard.ThrowIfTooLong(PkgScale?.Trim(), NoteLimits.PkgScale, nameof(PkgScale));
        Guard.ThrowIfMissing(PkgSizeUm?.Trim(), nameof(PkgSizeUm));
        Guard.ThrowIfTooLong(PkgSizeUm?.Trim(), NoteLimits.PkgSizeUm, nameof(PkgSizeUm));
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