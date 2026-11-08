namespace Medusa.Enap.Common.Core.Exceptions;

public static class CommonErrorCodes
{
    private const string _prefix = "error:DNC::";

    public const string Unknown = _prefix + nameof(Unknown);

    public const string AuthenticationRequired = _prefix + nameof(AuthenticationRequired);

    public const string InsufficientPrivileges = _prefix + nameof(InsufficientPrivileges);

    public const string ReadAccessRequired = _prefix + nameof(ReadAccessRequired);

    public const string WriteAccessRequired = _prefix + nameof(WriteAccessRequired);

    public const string AuthTokenExpired = _prefix + nameof(AuthTokenExpired);

    public const string FieldMissing = _prefix + nameof(FieldMissing);

    public const string InvalidDateFormat = _prefix + nameof(InvalidDateFormat);

    public const string InvalidPageRequest = _prefix + nameof(InvalidPageRequest);

    public const string CannotDeserializeMessage = _prefix + nameof(CannotDeserializeMessage);

    public const string InvalidSortField = _prefix + nameof(InvalidSortField);

    public const string FieldTooLong = _prefix + nameof(FieldTooLong);

    public const string UpdateConcurrencyError = _prefix + nameof(UpdateConcurrencyError);

    public const string ConcurrentProcessingError = _prefix + nameof(ConcurrentProcessingError);

    public const string UniqueKeyConflict = _prefix + nameof(UniqueKeyConflict);

    public const string BatchProcessingFailed = _prefix + nameof(BatchProcessingFailed);
}