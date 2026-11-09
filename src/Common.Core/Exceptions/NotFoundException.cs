namespace Common.Core.Exceptions;

public class NotFoundException(string errorCode, string message = "", Exception? innerException = null)
    : MedusaException(errorCode, message, innerException);