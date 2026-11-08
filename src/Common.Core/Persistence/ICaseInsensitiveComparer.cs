using System.Linq.Expressions;

namespace Medusa.Enap.Common.Core.Persistence;

public interface ICaseInsensitiveComparer
{
    Expression<Func<T, bool>> Compare<T>(Expression<Func<T, string?>> selector, string? value);
}
