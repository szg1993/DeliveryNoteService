using Common.Core.DependencyInjection;
using Common.Core.Exceptions;
using Common.Core.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Common.Core.UseCases.BatchOperations;

[AutoRegister(ServiceLifetime.Transient)]
public class BatchOperation<TRequestDto>(MedusaLogger<BatchOperation<TRequestDto>> logger)
{
    public BatchResult<TRequestDto> Result { get; } = new();

    public async Task ExecuteAsync(IEnumerable<TRequestDto> requestDtos, Func<TRequestDto, Task> action)
    {
        foreach (var dto in requestDtos)
            await ExecuteSingleAsync(dto, () => action(dto));
    }

    private async Task ExecuteSingleAsync(TRequestDto dto, Func<Task> action)
    {
        try
        {
            await action();
            Result.Add(dto);
        }
        catch (MedusaException ex)
        {
            Result.Add(dto, ex.GetErrorCode(), ex.GetData());
        }
        catch (Exception ex)
        {
            logger.LogTechnical(LogLevel.Warning, ex, "Unexpected error occurred while executing batch update");
            Result.Add(dto, CommonErrorCodes.Unknown, ex.GetData());
        }
    }
}