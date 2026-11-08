using Medusa.Enap.Common.Core.Exceptions;
using Medusa.Enap.Common.Core.Messaging;
using Medusa.Enap.Common.Core.Persistence;
using Medusa.Enap.Common.Core.UseCases;
using Medusa.Enap.DemoService.Domain.Models;
using Medusa.Enap.DemoService.Application.Errors;

namespace Medusa.Enap.DemoService.Application;
public class GetDemoModel(IRepository<DemoModel> r) : IUseCase
{
    public async Task<Envelope<DemoModel>> RunAsync(int id, CancellationToken cancellationToken)
    {
        var result = await r.GetAsync(id, cancellationToken)
            ?? throw new NotFoundException(ErrorCodes.DemoModelNotFound, "demo model not found");

        return new Envelope<DemoModel>(result);
    }
}