using System.Diagnostics.CodeAnalysis;

namespace Medusa.Enap.Common.Core.Utils;

/*
 * ONLY general Enumerable extensions, no domain specific functions 
 */
public static class EnumerableExtensions
{
    public static bool None<T>(this IEnumerable<T> source)
        => !source.Any();

    public static bool None<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        => !source.Any(predicate);

    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? source)
        => source is null || source.None();

    public static IEnumerable<T> ExceptNull<T>(this IEnumerable<T> source)
        where T : class => source.Where(l => l is not null);

    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
            action(item);
    }

    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> input)
        => input.SelectMany(l => l);

    public static void AddTo<T>(this IEnumerable<T> source, ICollection<T> target)
        => source.ForEach(target.Add);
}
