namespace Medusa.Enap.Common.Core.Exceptions;

public class UniqueKeyConflictException(Exception? innerException) 
    : MedusaException(CommonErrorCodes.UniqueKeyConflict, CommonErrorCodes.UniqueKeyConflict, innerException)
{ }
