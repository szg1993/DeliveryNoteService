using Common.Core.Exceptions;
using Common.Core.UseCases;
using FluentAssertions;
using System.Text.RegularExpressions;

namespace UnitTests;

public class GuardTests
{
    private class TestDto
    {
        public DateOnly TestDate { get; set; }

        public DateTimeOffset TestDateTimeOffset { get; set; }

        public object TestObject { get; set; } = null!;

        public string TestString { get; set; } = null!;

        public IEnumerable<string>? TestList { get; set; }
    }

    private enum TestValues
    {
        Alpha, Beta
    }

    [Theory]
    [InlineData("", false)]
    [InlineData(null, true)]
    public void ThrowIfNull(string field, bool shouldThrow)
    {
        var act = () => Guard.ThrowIfNull(field);

        if (shouldThrow)
            act.Should().Throw<InvalidOperationException>($"field must not be null!");
        else
            act().Should().Be(field);
    }

    [Fact]
    public void ThrowIfMissing_Basic()
    {
        object testValue = new TestDto { };

        var act = () => Guard.ThrowIfMissing(testValue);

        act.Should().NotThrow<FieldMissingException>();
    }

    [Fact]
    public void ThrowIfMissing_WithString_Basic()
    {
        string testValue = "test";

        var act = () => Guard.ThrowIfMissing(testValue);

        act.Should().NotThrow<FieldMissingException>();
    }

    [Fact]
    public void ThrowIfMissing_DtoProperty()
    {
        var dto = new TestDto();

        var act = () => Guard.ThrowIfMissing(dto.TestObject);

        act.Should().Throw<FieldMissingException>($"Field missing. ({nameof(TestDto.TestObject)})");
    }

    [Fact]
    public void ThrowIfMissing_WithString_DtoProperty()
    {
        var dto = new TestDto();

        var act = () => Guard.ThrowIfMissing(dto.TestString);

        act.Should().Throw<FieldMissingException>($"Field missing. ({nameof(TestDto.TestString)})");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void ThrowIfMissing_WithString_Validation(string field)
    {
        var act = () => Guard.ThrowIfMissing(field);

        act.Should().Throw<FieldMissingException>($"Field missing. (field)");
    }

    [Theory]
    [InlineData("", false)]
    [InlineData(null, true)]
    public void ThrowIfNotLoaded(string field, bool shouldThrow)
    {
        var act = () => Guard.ThrowIfNotLoaded(field);

        if (shouldThrow)
            act.Should().Throw<InvalidOperationException>($"Must include testValue!");
        else
            act().Should().Be(field);
    }

    [Fact]
    public void ThrowIfDefault_Basic()
    {
        var testValue = new DateOnly(2026, 03, 24);

        var act = () => Guard.ThrowIfDefault(testValue);

        act.Should().NotThrow<FieldMissingException>();
    }

    [Fact]
    public void ThrowIfDefault_DtoProperty_DateOnly()
    {
        var dto = new TestDto() { TestDate = DateOnly.MinValue };

        var act = () => Guard.ThrowIfDefault(dto.TestDate);

        act.Should().Throw<FieldMissingException>($"Field missing. ({nameof(TestDto.TestDate)})");
    }

    [Fact]
    public void ThrowIfDefault_DtoProperty_DateTimeOffset()
    {
        var dto = new TestDto() { TestDateTimeOffset = DateTimeOffset.MinValue };

        var act = () => Guard.ThrowIfDefault(dto.TestDate);

        act.Should().Throw<FieldMissingException>($"Field missing. ({nameof(TestDto.TestDateTimeOffset)})");
    }

    [Fact]
    public void ThrowIfMissingOrEmpty_Basic()
    {
        var testList = new List<string>() { "Test" };

        var act = () => Guard.ThrowIfMissingOrEmpty(testList);

        act.Should().NotThrow<FieldMissingException>();
    }

    [Fact]
    public void ThrowIfMissingOrEmpty_DtoProperty_IsNull()
    {
        var dto = new TestDto() { TestList = null };

        var act = () => Guard.ThrowIfMissingOrEmpty(dto.TestList!);

        act.Should().Throw<FieldMissingException>($"Field missing. ({nameof(TestDto.TestList)})");
    }

    [Fact]
    public void ThrowIfMissingOrEmpty_DtoProperty_IsEmpty()
    {
        var dto = new TestDto() { TestList = [] };

        var act = () => Guard.ThrowIfMissingOrEmpty(dto.TestList);

        act.Should().Throw<FieldMissingException>($"Field missing. ({nameof(TestDto.TestList)})");
    }

    [Theory]
    [InlineData("Alpha", false)]
    [InlineData("alpha", true)]
    [InlineData("InvalidValue", true)]
    public void ThrowIfInvalidEnumValue(string enumValue, bool shouldThrow)
    {
        var testErrorCode = "test error code";

        var act = () => Guard.ThrowIfInvalidEnumValue<TestValues>(enumValue, testErrorCode);

        if (shouldThrow)
        {
            var ex = act.Should().Throw<DataValidationException>().Which;
            ex.Data.Contains("enumValue").Should().Be(true);
            ex.Data["enumValue"].Should().Be(enumValue);
        }
        else
        {
            act();
        }
    }

    [Theory]
    [InlineData("123", false)]
    [InlineData("text", true)]
    [InlineData("", true)]
    public void ThrowIfDoesNotMatch_NumberOnly(string property, bool shouldThrow)
    {
        var regex = new Regex(@"^\d+$");
        var testErrorCode = "test error code";

        var act = () => Guard.ThrowIfDoesNotMatch(property, regex, testErrorCode);

        if (shouldThrow)
        {
            var ex = act.Should().Throw<DataValidationException>().Which;
            ex.Data.Contains("property").Should().Be(true);
            ex.Data["property"].Should().Be(property);
        }
        else
        {
            act();
        }
    }
}