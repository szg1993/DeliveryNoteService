using Medusa.Enap.Common.Core.Configuration;
using Medusa.Enap.Common.Core.Messaging;
using Medusa.Enap.Common.Core.Utils;

namespace Medusa.Enap.Common.Testing.ApiWrappers;

public static class HttpResponseExtensions
{
    public static Task<PagedEnvelope<T>> ReadPaged<T>(this Task<HttpResponseMessage> responseTask)
        => responseTask.Then(ReadPagedEnvelopeAsync<T>);

    public static Task<PagedEnvelope<T>> ReadPagedEnvelopeAsync<T>(this HttpResponseMessage response)
        => response.ReadAsync<PagedEnvelope<T>>(JsonConfiguration.GetOptions());
}
