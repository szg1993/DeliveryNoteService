using FluentAssertions;
using Common.Core.Exceptions;
using Common.Core.Utils;
using Common.Testing.TestSuite;
using System.Text.Json;

namespace UnitTests.Core.Utils;

public partial class JsonTests
{
    public class TestDto
    {
        public int Id { get; set; }
        public DateOnly TestDate { get; set; }
        public DateOnly? NullableTestDate { get; set; }

        public static TestDto WithTestData() => new()
        {
            Id = 42,
            TestDate = new DateOnly(2025, 11, 20),
            NullableTestDate = new DateOnly(2025, 11, 21),
        };
    }

    [Fact]
    public void Serialize()
    {
        var json = EnapSerializer.ToJson(TestDto.WithTestData());
        json.Should().Be("""
            {
              "id": 42,
              "testDate": "2025-11-20",
              "nullableTestDate": "2025-11-21"
            }
            """
        );
    }

    [Fact]
    public void Deserialize()
    {
        var msg = EnapSerializer.FromJson<TestDto>("""
            {
              "Id": 42,
              "tEsTDAte": "2025-11-20",
              "nullableTestDate": "2025-11-21"
            }
            """
        );
        msg.Should().BeEquivalentTo(TestDto.WithTestData());
    }

    [Fact]
    public void DeserializationError()
    {
        Action act = () => EnapSerializer.FromJson<TestDto>("malformed");
        act.Should().Throw<DeserializationException>()
            .WithInnerException<JsonException>();
    }

    [Theory]
    [InlineData("1711922399", TestDateStrings.Mar31)] // 2024-03-31 23:59:59 UTC+1 (Hungarian time)
    [InlineData("1711922400", TestDateStrings.Apr1)] // 2024-04-01 00:00:00 UTC+1 (Hungarian time)
    public void Date_UnixTimeSeconds(string dateString, string expectedDate)
    {
        var deserialized = EnapSerializer.FromJson<TestDto>("{" +
            "\"testDate\":" + dateString +
            "}"
        );
        deserialized.Should().NotBeNull();
        deserialized!.TestDate.Should().Be(expectedDate.ParseDateOnly());
    }

    [Fact]
    public void Date_Null_Serialize()
    {
        var json = EnapSerializer.ToJson(new TestDto());
        json.Should().Be("""
            {
              "id": 0,
              "testDate": "0001-01-01",
              "nullableTestDate": null
            }
            """
        );
    }

    [Fact]
    public void Date_Null_Deserialize()
    {
        var deserialized = EnapSerializer.FromJson<TestDto>("""
            {            
                "testDate": null,
                "nullableTestDate": null
            }
            """
        );
        deserialized.Should().NotBeNull();
        deserialized!.TestDate.Should().Be(DateOnly.MinValue);
        deserialized!.NullableTestDate.Should().BeNull();
    }

    [Fact]
    public void Date_EmptyString_Deserialize()
    {
        var deserialized = EnapSerializer.FromJson<TestDto>("""
            {            
                "testDate": "",
                "nullableTestDate": ""
            }
            """
        );
        deserialized.Should().NotBeNull();
        deserialized!.TestDate.Should().Be(DateOnly.MinValue);
        deserialized!.NullableTestDate.Should().BeNull();
    }

    [Fact]
    public void Date_WhiteSpaces_Deserialize()
    {
        var deserialized = EnapSerializer.FromJson<TestDto>("""
            {            
                "testDate": " ",
                "nullableTestDate": "   "
            }
            """
        );
        deserialized.Should().NotBeNull();
        deserialized!.TestDate.Should().Be(DateOnly.MinValue);
        deserialized!.NullableTestDate.Should().BeNull();
    }
}