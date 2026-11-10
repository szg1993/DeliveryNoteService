using Common.Core.Exceptions;
using Common.Core.Utils;
using System.Net;

namespace Common.Core.Integration;

public class IntegrationException(string message, Exception? innerException = null)
    : Exception(message, innerException)
{ }

public class NonSuccessResponseException : IntegrationException
{
    public HttpStatusCode StatusCode { get; }

    public string ResponseBody { get; }

    public string Url { get; set; }

    public NonSuccessResponseException(HttpStatusCode statusCode, string responseBody, string? url) : base("Response status code does not indicate success: " + (int)statusCode)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
        Url = url ?? "";

        try
        {
            var s = CommonSerializer.FromJson<MedusaError>(ResponseBody);
            this.SetErrorCode(s?.ErrorCode ?? CommonErrorCodes.Unknown);
        }
        catch (Exception)
        {
            this.SetErrorCode(CommonErrorCodes.Unknown);
        }
    }
}

public class ResponseDeserializationException : IntegrationException
{
    public string? Json { get; }

    public ResponseDeserializationException(string? json, Exception ex)
        : base("Failed to deserialize response", ex)
    {
        Json = json;
    }

    public ResponseDeserializationException(string? json)
        : base("Failed to deserialize response")
    {
        Json = json;
    }
}