namespace Medusa.Enap.Common.Core.Exceptions;

public class UpdateConcurrencyException(Exception? inner = null)
    : DataValidationException(CommonErrorCodes.UpdateConcurrencyError, null!, inner);
