using Common.Core.Persistence;
using System.Linq.Expressions;

namespace Common.Testing.Database.SQLite;

public class InMemoryEfILikeComparer : ICaseInsensitiveComparer
{
    public Expression<Func<T, bool>> Compare<T>(
     Expression<Func<T, string?>> selector,
     string? value)
    {
        ArgumentNullException.ThrowIfNull(selector);

        var parameter = selector.Parameters[0];
        var toLowerMethod = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes);

        // selector.ToLower()
        var left = Expression.Call(selector.Body, toLowerMethod!);

        // value.ToLower()
        var right = Expression.Constant(value?.ToLower());

        // selector.ToLower() == value.ToLower()
        var body = Expression.Equal(left, right);

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}