using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Medusa.Enap.Common.Infrastructure.Persistence;

public record DbSetInfo(string Name, Type EntityType)
{
    public static DbSetInfo[] Get<TContext>() where TContext : DbContext
        => Get(typeof(TContext));

    public static DbSetInfo[] Get(DbContext context)
        => Get(context.GetType());

    public static DbSetInfo[] Get(Type contextType)
        => contextType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType.IsGenericType &&
                        p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(p => new DbSetInfo(p.Name, p.PropertyType.GetGenericArguments()[0]))
            .ToArray();
}