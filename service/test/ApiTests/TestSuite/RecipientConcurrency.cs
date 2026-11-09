using DeliveryNoteService.Core.UseCases.Recipients;
using System.Net.Http.Json;

namespace ApiTests.TestSuite;

public static class RecipientConcurrency
{
    public static async Task<HttpResponseMessage[]> CreateRecipientsConcurrently(
        this DeliveryNoteServiceApi api,
        CreateRecipientDto first,
        CreateRecipientDto second
    )
    {
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var requests = new[] { SendAfterStart(first), SendAfterStart(second) };
        start.SetResult();
        return await Task.WhenAll(requests).WaitAsync(deadline.Token);

        async Task<HttpResponseMessage> SendAfterStart(CreateRecipientDto request)
        {
            await start.Task.WaitAsync(deadline.Token);
            return await api.Client.PostAsJsonAsync("api/recipients", request, deadline.Token);
        }
    }
}