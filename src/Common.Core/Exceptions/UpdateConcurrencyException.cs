namespace Common.Core.Exceptions;

public class UpdateConcurrencyException(Exception? inner = null)
    : DataValidationException(CommonErrorCodes.UpdateConcurrencyError, null!, inner);