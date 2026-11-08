namespace Medusa.Enap.Common.Core.Exceptions;

public class BatchItemError<TBatchItem>
{
    public TBatchItem Item { get; set; } = default!;

    public string ErrorCode { get; set; } = null!;

    public object? ErrorData { get; set; } = default!;
}