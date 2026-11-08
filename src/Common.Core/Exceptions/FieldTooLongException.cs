namespace Medusa.Enap.Common.Core.Exceptions;

public class FieldTooLongException : DataValidationException
{
    public FieldTooLongException(string fieldName, int maxLength)
        : base(CommonErrorCodes.FieldTooLong, $"Field is too long. ({fieldName})")
    {
        this
            .SetData("Field", fieldName)
            .SetData("MaxLength", maxLength);
    }
}