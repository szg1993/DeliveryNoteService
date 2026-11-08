using FluentAssertions;
using Medusa.Enap.Common.Core.Messaging;

namespace Medusa.Enap.Common.Testing.ApiWrappers;

public static class EnvelopeExtensions
{
    public static void ShouldBeEmpty<T>(this PagedEnvelope<T> envelope)
    {
        envelope.Content.Should().BeEmpty();
        envelope.Paging.TotalCount.Should().Be(0);
        envelope.Paging.TotalPages.Should().Be(0);
    }
}
