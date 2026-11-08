using System.Collections;

namespace Medusa.Enap.Common.Core.Exceptions;

public static class Extensions
{
    private const string _errorCode = "ErrorCode";

    public static string GetErrorCode(this Exception exception) => exception.Data.GetErrorCode();

    public static string GetErrorCode(this IDictionary data) => data[_errorCode]?.ToString() ?? CommonErrorCodes.Unknown;

    public static Exception SetErrorCode(this Exception exception, string errorCode)
    {
        exception.Data[_errorCode] = errorCode;
        return exception;
    }

    public static Exception SetData(this Exception exception, string key, object data)
    {
        if (string.IsNullOrEmpty(key))
            return exception;

        exception.Data[key] = data;

        return exception;
    }

    public static Dictionary<string, object> GetData(this Exception exception)
    {
        Dictionary<string, object> exceptionData = [];

        foreach (var key in exception.Data.Keys)
        {
            if (key.ToString() == _errorCode)
                continue;

            exceptionData.Add(key.ToString()!, exception.Data[key]!);
        }

        return exceptionData;
    }

    public static MedusaError WithData(this MedusaError error, string key, object data)
    {
        if (string.IsNullOrEmpty(key))
            return error;

        error.Data[key] = data;

        return error;
    }
}
