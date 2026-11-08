namespace Medusa.Enap.Common.Core.Exceptions;

public class MedusaException : Exception
{
    public MedusaException(string errorCode, string? message = null, Exception? innerException = null)
        : base(message ?? errorCode, innerException)
    {
        this.SetErrorCode(errorCode);
    }
}
