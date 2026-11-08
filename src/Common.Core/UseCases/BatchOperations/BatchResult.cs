namespace Medusa.Enap.Common.Core.UseCases.BatchOperations;

public class BatchResult<T>
{
    public List<BatchResultItem<T>> Items { get; set; } = [];

    public void AddRange(BatchResult<T> result)
        => AddRange(result.Items);

    public void AddRange(IEnumerable<BatchResultItem<T>> items)
        => Items.AddRange(items);

    public void Add(BatchResultItem<T> item)
        => Items.Add(item);

    public void Add(T item) => Add(item, errorCode: null, errorData: null);

    public void Add(T item, string? errorCode, Dictionary<string, object>? errorData)
        => Add(new BatchResultItem<T>()
        {
            Item = item,
            ErrorCode = errorCode,
            ErrorData = errorData?.ToDictionary(e => e.Key, e => e.Value.ToString() ?? "")
        });
}
