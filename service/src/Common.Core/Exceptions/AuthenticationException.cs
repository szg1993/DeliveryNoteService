namespace Common.Core.Exceptions;

public class AuthenticationException(string errorCode, Exception? innerException = null)
    : MedusaException(errorCode, message: "Authentication failed.", innerException: innerException);