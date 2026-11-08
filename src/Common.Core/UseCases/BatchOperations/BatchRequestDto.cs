namespace Medusa.Enap.Common.Core.UseCases.BatchOperations;

public class BatchRequestDto<T>()
{
    public IEnumerable<T> Items { get; set; } = [];

    public BatchRequestDto(params T[] items) : this()
    {
        Items = items;
    }
}