namespace Medusa.Enap.Common.Core.Exceptions;

public class ConcurrentProcessingException(
    Exception? innerException = null
)
    : MedusaException(CommonErrorCodes.ConcurrentProcessingError, CommonErrorCodes.ConcurrentProcessingError, innerException)
{
}
