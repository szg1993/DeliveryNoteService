using Common.Core;
using Common.Core.Messaging;
using Common.Core.Utils;

namespace Common.Testing.ApiWrappers;

public static class HttpResponseExtensions
{
    public static Task<PagedEnvelope<T>> ReadPaged<T>(this Task<HttpResponseMessage> responseTask)
        => responseTask.Then(ReadPagedEnvelopeAsync<T>);

    public static Task<PagedEnvelope<T>> ReadPagedEnvelopeAsync<T>(this HttpResponseMessage response)
        => response.ReadAsync<PagedEnvelope<T>>(JsonConfiguration.GetOptions());
}