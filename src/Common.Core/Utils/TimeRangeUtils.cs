namespace Medusa.Enap.Common.Core.Utils;

public interface IDateRanged
{
    DateOnly ValidFrom { get; }
    DateOnly? ValidUntil { get; }
}

public record DateRange(DateOnly From, DateOnly? To) : IDateRanged
{
    DateOnly IDateRanged.ValidFrom => From;
    DateOnly? IDateRanged.ValidUntil => To;
}

public static class TimeRangeUtils
{
    public static bool Overlaps(this IDateRanged a, IDateRanged b)
        => Overlaps([a, b]);

    public static bool Overlaps(this IEnumerable<IDateRanged> all)
    {
        if (all is null || !all.Any())
            return false;

        var curr = all.First();
        var rest = all.Skip(1);

        if (rest.Any(i => Overlaps(i.ValidFrom, curr.ValidFrom, i.ValidUntil, curr.ValidUntil)))
            return true;

        return Overlaps(rest);
    }

    public static bool Overlaps(DateOnly startDateA, DateOnly startDateB, DateOnly? endDateA, DateOnly? endDateB)
    {
        endDateA ??= DateOnly.MaxValue;
        endDateB ??= DateOnly.MaxValue;
        var start = Max(startDateA, startDateB);
        var end = Min(endDateA.Value, endDateB.Value);

        return start <= end;
    }

    public static bool AreConsecutive(DateOnly? first, DateOnly? second)
    {
        if (first is null || second is null)
            return false;

        return first.Value.AddDays(1) == second.Value;
    }

    private static DateOnly Max(DateOnly a, DateOnly b)
        => a > b ? a : b;

    private static DateOnly Min(DateOnly a, DateOnly b)
        => a < b ? a : b;
}
