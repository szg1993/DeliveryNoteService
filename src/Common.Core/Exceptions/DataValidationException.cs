namespace Medusa.Enap.Common.Core.Exceptions;

public class DataValidationException(string errorCode, string message = "Data validation failed.", Exception? inner = null)
    : MedusaException(errorCode, message, inner);
