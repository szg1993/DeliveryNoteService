namespace Common.Core.Exceptions;

public class AuthorizationException(
    string errorCode = CommonErrorCodes.InsufficientPrivileges,
    Exception? innerException = null
) : MedusaException(errorCode, message: "Authorization failed.", innerException: innerException);