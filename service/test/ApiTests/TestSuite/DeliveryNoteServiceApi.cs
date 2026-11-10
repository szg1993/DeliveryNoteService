using Common.Testing.ApiWrappers;
using DeliveryNoteService.Core.UseCases.Recipients;

namespace ApiTests.TestSuite;

public class DeliveryNoteServiceApi(HttpClient client) : Api(client)
{
    public CollectionEndpoint<RecipientDetailsDto> Recipients() => new("api/recipients", Client);

    public ResourceEndpoint<RecipientDetailsDto> Recipients(int id) => new($"api/recipients/{id}", Client);
}