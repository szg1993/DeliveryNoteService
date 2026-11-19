using System.Net;

namespace ApiTests;

public class ConcurrentCreationTests
{
    [Fact]
    public async Task PeerRequestFailure_DisposesAlreadyCompletedResponse()
    {
        using var content = new TrackedContent();
        using var completedResponse = new HttpResponseMessage(HttpStatusCode.Created) { Content = content };
        using var handler = new FailingPeerHandler(completedResponse);
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://test.invalid") };
        var api = new DeliveryNoteServiceApi(client);

        var operation = () => api.CreateNotesConcurrently(NoteRequests.Valid(1), NoteRequests.Valid(1));

        await operation.Should().ThrowAsync<HttpRequestException>();
        content.IsDisposed.Should().BeTrue();
    }

    private sealed class FailingPeerHandler(HttpResponseMessage completedResponse) : HttpMessageHandler
    {
        private int _requestCount;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Interlocked.Increment(ref _requestCount) == 1
                ? Task.FromResult(completedResponse)
                : Task.FromException<HttpResponseMessage>(new HttpRequestException("The peer request failed."));
        }
    }

    private sealed class TrackedContent() : StringContent("created")
    {
        public bool IsDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }
}