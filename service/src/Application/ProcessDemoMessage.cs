using Medusa.Enap.Common.Core.Exceptions;
using Medusa.Enap.Common.Core.Integration;
using Medusa.Enap.Common.Core.Persistence;
using Medusa.Enap.Common.Core.UseCases;
using Medusa.Enap.DemoService.Domain.Models;

namespace Medusa.Enap.DemoService.Application;

public class ProcessDemoMessage(
    IRepository<DemoModel> repository,
    IUnitOfWork unitOfWork,
    IHttpClientFactory httpClientFactory,
    CorrelationIdAccessor correlationIdAccessor
)
    : IUseCase
{
    public async Task RunAsync(DemoMessage message, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(message.Name))
            throw new FieldMissingException(nameof(message.Name));
        repository.Add(ToModel(message));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (message.MakeApiCall)
        {
            ApiClient apiClient = new ApiClient(httpClientFactory, correlationIdAccessor);
            await apiClient.GetAsync("http://test.com/path?query", cancellationToken);
        }
    }

    private DemoModel ToModel(DemoMessage message)
    {
        return new DemoModel { Name = message.Name };
    }
}
