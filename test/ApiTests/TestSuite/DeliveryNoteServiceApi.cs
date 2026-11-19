using Common.Testing.ApiWrappers;
using DeliveryNoteService.Core.UseCases.Notes;
using DeliveryNoteService.Core.UseCases.Recipients.Data;

namespace ApiTests.TestSuite;

public class DeliveryNoteServiceApi(HttpClient client) : Api(client)
{
    public CollectionEndpoint<NoteDetailsDto> Notes() => new("api/delivery-notes", Client);

    public ResourceEndpoint<NoteDetailsDto> Notes(int id) => new($"api/delivery-notes/{id}", Client);

    public CollectionEndpoint<RecipientDetailsDto> Recipients() => new("api/recipients", Client);

    public ResourceEndpoint<RecipientDetailsDto> Recipients(int id) => new($"api/recipients/{id}", Client);
}