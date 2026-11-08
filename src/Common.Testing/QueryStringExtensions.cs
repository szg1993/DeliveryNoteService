using Medusa.Enap.Common.Core.Messaging;
using Microsoft.AspNetCore.Http;

namespace Medusa.Enap.Common.Testing;

public static class QueryStringExtensions
{
    public static QueryString Page(this QueryString qs, int page, int limit)
        => qs
            .Add("page", page.ToString())
            .Add("limit", limit.ToString());

    public static QueryString Sort(this QueryString qs, string sortBy, SortDirection sortDir)
        => qs
            .Add("sortBy", sortBy.ToString())
            .Add("sortDir", sortDir.ToString());

    public static QueryString Unpaged(this QueryString qs)
        => qs.Page(0, 0);

    /// <summary>
    /// Adds a collection to the query string. E.g.: ?ids=1&ids=2&ids=42
    /// </summary>
    public static QueryString AddArray<T>(this QueryString qs, string arrayParamName, IEnumerable<T> values)
    {
        foreach (var v in values)
            qs = qs.Add(arrayParamName, v?.ToString() ?? "");

        return qs;
    }
}