using Medusa.Enap.Common.Core.Exceptions;
using Medusa.Enap.Common.Core.Logging;
using Microsoft.Extensions.Logging;

namespace Medusa.Enap.Common.Core.UseCases.BatchOperations;

public class CollectionPatchOperation<TResourceDto>(IMedusaLogger logger)
{
    public BatchResult<TResourceDto> Result { get; } = new();

    public required Func<TResourceDto, Task> AddAction { get; set; }
    public required Func<TResourceDto, Task> UpdateAction { get; set; }
    public required Func<TResourceDto, Task> DeleteAction { get; set; }

    public async Task ExecuteAsync(CollectionPatchRequest<TResourceDto> request)
    {
        await ExecuteAction(request.Add, AddAction!);
        await ExecuteAction(request.Modify, UpdateAction!);
        await ExecuteAction(request.Delete, DeleteAction!);
    }

    private async Task ExecuteAction(IEnumerable<TResourceDto> dtos, Func<TResourceDto, Task> action)
    {
        foreach (var dto in dtos)
            await ExecuteSingleAsync(dto, () => action(dto));
    }

    private async Task ExecuteSingleAsync(TResourceDto dto, Func<Task> action)
    {
        try
        {
            await action();
            Result.Add(dto);
        }
        catch (Exception ex)
        {
            logger.LogTechnical(ex is MedusaException ? LogLevel.Debug : LogLevel.Warning, ex, "Error occurred while executing CollectionPatchOperation");
            Result.Add(dto, ex.GetErrorCode(), ex.GetData());
        }
    }
}
