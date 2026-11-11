using Common.Core.Messaging;
using FluentAssertions;

namespace Common.Testing.ApiWrappers;

public static class EnvelopeExtensions
{
    public static void ShouldBeEmpty<T>(this PagedEnvelope<T> envelope)
    {
        envelope.Content.Should().BeEmpty();
        envelope.Paging.TotalCount.Should().Be(0);
        envelope.Paging.TotalPages.Should().Be(0);
    }
}