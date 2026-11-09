namespace Common.Core.Messaging;

public record Sorting(string? SortBy, SortDirection? SortDir);

public record Sorting<TSortBy>(TSortBy? SortBy = null, SortDirection? SortDir = null)
    where TSortBy : struct
{
    public static implicit operator Sorting(Sorting<TSortBy> s) => new(s.SortBy?.ToString(), s.SortDir);

    public static Sorting<TSortBy> Unsorted { get; } = new(null, null);
};