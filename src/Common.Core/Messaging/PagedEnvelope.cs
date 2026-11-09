namespace Common.Core.Messaging;

public class PagedEnvelope<T> : Envelope<IEnumerable<T>>
{
    public PageDetails Paging { get; set; }

    public PagedEnvelope(IEnumerable<T> content, PageRequest request, int totalCount)
        : this(content, new PageDetails(request, totalCount))
    { }

    public PagedEnvelope(IEnumerable<T> content, PageDetails paging)
        : base(content)
    {
        Paging = paging;
    }

    // For deserialization in API tests
    public PagedEnvelope() : base(null!)
    {
        Paging = new();
    }
}