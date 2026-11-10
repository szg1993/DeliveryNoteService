namespace Common.Core.Exceptions;

public class FieldMissingException : DataValidationException
{
    public FieldMissingException(string fieldName)
        : base(CommonErrorCodes.FieldMissing, $"Field missing. ({fieldName})")
    {
        this.SetData("Field", fieldName);
    }
}