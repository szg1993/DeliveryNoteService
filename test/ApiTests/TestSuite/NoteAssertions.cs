using Common.Core.Messaging;
using DeliveryNoteService.Core.UseCases.Notes;
using System.Net;

namespace ApiTests.TestSuite;

public static class NoteAssertions
{
    public static async Task<NoteDetailsDto> AssertSavedNote(
        this DeliveryNoteServiceApi api,
        HttpResponseMessage createdResponse
    )
    {
        await createdResponse.AssertStatusCode(HttpStatusCode.Created);
        createdResponse.Headers.Location.Should().NotBeNull();
        var created = (await createdResponse.ReadAsync<Envelope<NoteDetailsDto>>()).Content;
        using var response = await api.Client.GetAsync(createdResponse.Headers.Location);
        await response.AssertStatusCode(HttpStatusCode.OK);
        var saved = (await response.ReadAsync<Envelope<NoteDetailsDto>>()).Content;
        saved.Should().BeEquivalentTo(created, options => options.WithStrictOrdering());
        return saved;
    }
}