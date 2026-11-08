using Medusa.Enap.Common.Core.Exceptions;
using Medusa.Enap.Common.Core.Messaging;
using Medusa.Enap.Common.Core.Persistence;
using Medusa.Enap.Common.Core.UseCases;
using Medusa.Enap.DemoService.Application.Errors;
using Medusa.Enap.DemoService.Domain.Models;

namespace Medusa.Enap.DemoService.Application.Recipients;

public class GetRecipient(IRepository<Recipient> recipients) : IUseCase
{
    public async Task<Envelope<RecipientDetails>> RunAsync(int id, CancellationToken ct)
    {
        if (id <= 0)
            throw new DataValidationException(ErrorCodes.InvalidRecipientId, "Recipient ID must be positive.");
        var recipient = await recipients.GetAsync(id, ct)
            ?? throw new NotFoundException(ErrorCodes.RecipientNotFound, "Recipient not found.");
        return new Envelope<RecipientDetails>(RecipientDetails.From(recipient));
    }
}