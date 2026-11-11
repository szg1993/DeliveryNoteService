using DeliveryNoteService.Core.UseCases.Notes;
using DeliveryNoteService.Core.UseCases.Recipients.Data;
using System.Net.Http.Json;

namespace ApiTests.TestSuite;

public static class ConcurrentCreation
{
    public static Task<HttpResponseMessage[]> CreateRecipientsConcurrently(
        this DeliveryNoteServiceApi api,
        CreateRecipientDto first,
        CreateRecipientDto second
    ) => PostConcurrently(api, "api/recipients", first, second);

    public static Task<HttpResponseMessage[]> CreateNotesConcurrently(
        this DeliveryNoteServiceApi api,
        CreateDeliveryNoteDto first,
        CreateDeliveryNoteDto second
    ) => PostConcurrently(api, "api/delivery-notes", first, second);

    private static async Task<HttpResponseMessage[]> PostConcurrently<TRequest>(
        DeliveryNoteServiceApi api,
        string path,
        TRequest first,
        TRequest second
    )
    {
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var requests = new[] { SendAfterStart(first), SendAfterStart(second) };
        start.SetResult();
        try
        {
            return await Task.WhenAll(requests);
        }
        catch
        {
            foreach (var request in requests.Where(request => request.IsCompletedSuccessfully))
                request.Result.Dispose();
            throw;
        }

        async Task<HttpResponseMessage> SendAfterStart(TRequest request)
        {
            await start.Task.WaitAsync(deadline.Token);
            return await api.Client.PostAsJsonAsync(path, request, deadline.Token);
        }
    }
}