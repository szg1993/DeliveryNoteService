using Common.Core.Exceptions;
using DeliveryNoteService.Core.Errors;
using System.Diagnostics.CodeAnalysis;

namespace DeliveryNoteService.Core.UseCases.Notes;

internal static class NoteInput
{
    internal static void Require([DoesNotReturnIf(false)] bool valid, string field)
    {
        if (!valid)
            throw new DataValidationException(ErrorCodes.Note.InvalidInput, $"Invalid {field}.");
    }

    internal static void Text(string? value, int maxLength, string field, bool optional)
    {
        Require(optional || !string.IsNullOrWhiteSpace(value), field);
        Require(value?.Trim().Length <= maxLength || value is null, field);
    }

    internal static void Number(decimal? value, string field, bool positive)
    {
        Require(value.HasValue && (positive ? value > 0 : value >= 0), field);
    }

    internal static string? Optional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}