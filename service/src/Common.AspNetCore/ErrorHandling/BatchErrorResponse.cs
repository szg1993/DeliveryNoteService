using Common.Core.Exceptions;

namespace Common.AspNetCore.ErrorHandling;

public class BatchErrorResponse<TBatchItem> : MedusaErrorResponse
{
    public List<BatchItemError<TBatchItem>> InnerErrors { get; init; } = [];

    public BatchErrorResponse(BatchProcessingFailedException<TBatchItem> ex, string correlationId)
        : base(ex)
    {
        InnerErrors = ex.Items;
        ErrorCode = ex.GetErrorCode();
        CorrelationId = correlationId;
    }

    // For serialization
    public BatchErrorResponse() : base("")
    { }
}