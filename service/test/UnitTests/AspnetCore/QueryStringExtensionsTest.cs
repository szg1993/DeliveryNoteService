using FluentAssertions;
using Medusa.Enap.Common.Core.Messaging;
using Medusa.Enap.Common.Testing;
using Microsoft.AspNetCore.Http;

namespace UnitTests.AspnetCore;

public class QueryStringExtensionsTest
{
    [Fact]
    public void Page()
    {
        QueryString.Empty
            .Page(1, 2)
            .Value.Should().Be("?page=1&limit=2");
    }

    [Fact]
    public void Unpaged()
    {
        QueryString.Empty
            .Unpaged()
            .Value.Should().Be("?page=0&limit=0");
    }

    [Fact]
    public void AddArray()
    {
        QueryString.Empty
            .AddArray("ids", [1, 2, 42])
            .Value.Should().Be("?ids=1&ids=2&ids=42");
    }

    [Fact]
    public void Sorting()
    {
        QueryString.Empty
            .Sort("Id", SortDirection.Desc)
            .Value.Should().Be("?sortBy=Id&sortDir=Desc");
    }
}
