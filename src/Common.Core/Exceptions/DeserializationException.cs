namespace Common.Core.Exceptions;

public class DeserializationException(Exception? innerException = null)
    : MedusaException(CommonErrorCodes.CannotDeserializeMessage, innerException: innerException);