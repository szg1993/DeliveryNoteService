using Common.Core.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Common.Infrastructure.Persistence;

public class EfILikeComparer : ICaseInsensitiveComparer
{
    public Expression<Func<T, bool>> Compare<T>(Expression<Func<T, string?>> selector, string? value)
    {
        ArgumentNullException.ThrowIfNull(selector);

        // entity parameter (same as selector.Parameters[0])
        var param = selector.Parameters[0];

        // selector(entity)
        var selected = selector.Body; // string?

        // selector(entity) == null
        var selectedIsNull = Expression.Equal(
            selected,
            Expression.Constant(null, typeof(string))
        );

        // If value is null => selector(entity) == null
        if (value is null)
        {
            return Expression.Lambda<Func<T, bool>>(selectedIsNull, param);
        }

        // Build: EF.Functions
        var efFunctions = Expression.Property(
            null,
            typeof(EF).GetProperty(nameof(EF.Functions))!
        );

        // Find NpgsqlDbFunctionsExtensions.ILike(DbFunctions, string, string)
        var iLikeMethod =
            typeof(NpgsqlDbFunctionsExtensions).GetMethod(
                nameof(NpgsqlDbFunctionsExtensions.ILike),
                new[] { typeof(DbFunctions), typeof(string), typeof(string) }
            )
            ?? throw new InvalidOperationException("Could not find ILike method.");

        // EF.Functions.ILike(selector(entity), value)
        var iLikeCall = Expression.Call(
            iLikeMethod,
            efFunctions,
            selected,
            Expression.Constant(value, typeof(string))
        );

        // selector(entity) != null AND ILike(...)
        var selectedIsNotNull = Expression.Not(selectedIsNull);
        var body = Expression.AndAlso(selectedIsNotNull, iLikeCall);

        return Expression.Lambda<Func<T, bool>>(body, param);
    }
}