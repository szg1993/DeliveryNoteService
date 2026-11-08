using FluentAssertions;
using Medusa.Enap.Common.Core.Exceptions;
using Medusa.Enap.Common.Core.UseCases;
using Medusa.Enap.Common.Testing.Errors;
using Xunit;

namespace Medusa.Enap.Common.Testing.TestSuite;

public static class IValidatableExtensions
{
    public static DataValidationException AssertDataValidationException(this IValidatable self, string errorCode)
    {
        var ex = Assert.ThrowsAny<DataValidationException>(self.Validate);

        ex.GetErrorCode().Should().Be(errorCode);

        return ex;
    }

    public static DataValidationException AssertFieldMissing(this IValidatable self, string fieldName)
    {
        var ex = self.AssertDataValidationException(CommonErrorCodes.FieldMissing);

        ex.AssertData("Field", fieldName);

        return ex;
    }

    public static DataValidationException AssertFieldTooLong(this IValidatable self, string fieldName)
    {
        var ex = self.AssertDataValidationException(CommonErrorCodes.FieldTooLong);

        ex.AssertData("Field", fieldName);

        return ex;
    }

    public static void AssertValid(this IValidatable self)
        => Assert.Null(Record.Exception(self.Validate));
}