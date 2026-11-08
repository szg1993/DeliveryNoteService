using System.ComponentModel;

namespace Medusa.Enap.Common.Core.UseCases.BatchOperations;

public class BatchResultItem<T>
{
    public T Item { get; set; } = default!;

    public string? ErrorCode { get; set; }

    [Description("OBSOLETE: backend-side error localization removed.")]
    public string ErrorMessage { get; set; } = string.Empty;

    public Dictionary<string, string>? ErrorData { get; set; }
}
