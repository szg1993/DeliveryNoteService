using Common.Core.Exceptions;
using Common.Core.Utils;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Common.Core.UseCases;

public static class Guard
{
    public static T ThrowIfNull<T>([NotNull] T? obj, [CallerArgumentExpression(nameof(obj))] string? objectName = null)
    {
        if (obj is null)
            throw new InvalidOperationException($"{objectName} must not be null!");

        return obj;
    }

    public static TModel ThrowIfNotLoaded<TModel>([NotNull] TModel? property, [CallerArgumentExpression(nameof(property))] string? propertyName = null)
    {
        if (property is null)
            throw new InvalidOperationException($"Must include {propertyName}!");

        return property;
    }

    public static void ThrowIfMissing([NotNull] string? property, [CallerArgumentExpression(nameof(property))] string? propertyName = null)
    {
        if (string.IsNullOrEmpty(property))
            ThrowFieldMissing(propertyName!);
    }

    public static void ThrowIfTooLong(
        string? property,
        int maxLength,
        [CallerArgumentExpression(nameof(property))] string? propertyName = null
    )
        => property.ValidateMaxLength(maxLength, GetUnqualifiedPropertyName(propertyName!));

    public static void ThrowIfMissing<TProperty>([NotNull] TProperty? property, [CallerArgumentExpression(nameof(property))] string? propertyName = null)
    {
        if (property is null)
            ThrowFieldMissing(propertyName!);
    }

    public static void ThrowIfMissingOrEmpty<TProperty>(
        [NotNull] IEnumerable<TProperty>? property,
        [CallerArgumentExpression(nameof(property))] string? propertyName = null
    )
    {
        if (property is null || !property.Any())
            ThrowFieldMissing(propertyName!);
    }

    public static void ThrowIfDefault<TProperty>(
        [NotNull] TProperty? property,
        [CallerArgumentExpression(nameof(property))] string? propertyName = null
    )
    {
        if (property is null || EqualityComparer<TProperty>.Default.Equals(property, default))
            ThrowFieldMissing(propertyName!);
    }

    public static void ThrowIfInvalidEnumValue<TEnum>(
        string enumProperty,
        string errorCode,
        [CallerArgumentExpression(nameof(enumProperty))] string? propertyName = null
    )
    {
        if (!Enum.IsDefined(typeof(TEnum), enumProperty))
        {
            throw new DataValidationException(errorCode)
                .SetData(GetUnqualifiedPropertyName(propertyName!), enumProperty);
        }
    }

    public static void ThrowIfDoesNotMatch(
        string property,
        Regex regex,
        string errorCode,
        [CallerArgumentExpression(nameof(property))] string? propertyName = null
    )
    {
        if (regex.IsMatch(property))
            return;

        throw new DataValidationException(errorCode)
            .SetData(GetUnqualifiedPropertyName(propertyName!), property);
    }

    [DoesNotReturn]
    private static void ThrowFieldMissing(string propertyName)
        => throw new FieldMissingException(GetUnqualifiedPropertyName(propertyName));

    private static string GetUnqualifiedPropertyName(string propertyName)
    {
        return propertyName.Split('.')[^1];
    }
}