using Medusa.Enap.DemoService.Application.Recipients;
using System.Net.Http.Json;

namespace ApiTests.TestSuite;

public static class RecipientConcurrency
{
    public static async Task<HttpResponseMessage[]> CreateRecipientsConcurrently(
        this DemoServiceApi api,
        CreateRecipientRequest first,
        CreateRecipientRequest second
    )
    {
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var requests = new[] { SendAfterStart(first), SendAfterStart(second) };
        start.SetResult();
        return await Task.WhenAll(requests).WaitAsync(deadline.Token);

        async Task<HttpResponseMessage> SendAfterStart(CreateRecipientRequest request)
        {
            await start.Task.WaitAsync(deadline.Token);
            return await api.Client.PostAsJsonAsync("api/recipients", request, deadline.Token);
        }
    }
}