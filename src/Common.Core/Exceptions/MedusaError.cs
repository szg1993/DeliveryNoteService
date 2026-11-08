namespace Medusa.Enap.Common.Core.Exceptions;

public class MedusaError(string errorCode)
{
    public string ErrorCode { get; set; } = errorCode;
    public Dictionary<string, object> Data { get; init; } = [];

    /// <summary>
    /// NOT INTENDED TO BE USED DIRECTLY. For serialization purposes.
    /// </summary>
    public MedusaError() : this("") { }

    public MedusaError(Exception exception) : this(exception.GetErrorCode())
    {
        Data = exception.GetData();
    }

    public MedusaError WithData(string key, object data)
    {
        if (!string.IsNullOrEmpty(key))
            Data[key] = data;

        return this;
    }

    public static MedusaError FieldMissing(string fieldName)
    {
        return new MedusaError(CommonErrorCodes.FieldMissing).WithData("Field", fieldName);
    }

    public static MedusaError Unknown() => new(CommonErrorCodes.Unknown);
}