namespace Medusa.Enap.Common.Core.Messaging;

public class PageDetails
{
    public int Page { get; set; }

    public int Limit { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }

    public PageDetails() { }

    public PageDetails(PageRequest pageRequest, int totalCount)
    {
        Page = pageRequest.Page;
        Limit = pageRequest.Limit;
        TotalCount = totalCount;
        TotalPages = pageRequest.Limit == 0 ? 1 : (int)Math.Ceiling(totalCount / (double)pageRequest.Limit);
    }

    public static PageDetails Unpaged(int totalCount)
        => new(PageRequest.Unpaged, totalCount);
}
