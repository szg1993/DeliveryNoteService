namespace Common.Core.Exceptions;

public abstract class BatchProcessingFailedException(string message = "Data validation failed.", Exception? inner = null)
    : DataValidationException(CommonErrorCodes.BatchProcessingFailed, message, inner)
{ }

public class BatchProcessingFailedException<TBatchItem>(
    string message = "Data validation failed.",
    Exception? inner = null,
    IEnumerable<BatchItemError<TBatchItem>>? items = null
)
    : BatchProcessingFailedException(message, inner)
{
    public List<BatchItemError<TBatchItem>> Items { get; set; } = items?.ToList() ?? [];

    public string CorrelationId { get; set; } = null!;
}