namespace DeliveryNoteService.Core.Errors;

public static class ErrorCodes
{
    public static class Recipient
    {
        private const string _prefix = nameof(Recipient) + ".";

        public const string NotFound = _prefix + "NotFound";
        public const string InvalidCode = _prefix + "InvalidCode";
        public const string InvalidCountry = _prefix + "InvalidCountry";
        public const string AlreadyExists = _prefix + "AlreadyExists";
    }
    public static class Note
    {
        private const string _prefix = nameof(Note) + ".";
        public const string NotFound = _prefix + "NotFound";
        public const string InvalidInput = _prefix + "InvalidInput";
        public const string NumberExhausted = _prefix + "NumberExhausted";
    }
}