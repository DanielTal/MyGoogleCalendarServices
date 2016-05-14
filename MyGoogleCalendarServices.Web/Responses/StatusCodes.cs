namespace MyGoogleCalendarServices.Web.Responses
{
    public sealed class StatusCodes
    {
        public static readonly string noErrors = "noErrors";
        public static readonly string requestMissing = "requestMissing";
        public static readonly string validationErrors = "validationErrors";
        public static readonly string unhandlesException = "unhandlesException";
        public static readonly string eventIdNotFound = "eventIdNotFound";
        public static readonly string googleSerivceInitFailed = "googleSerivceInitFailed";
        public static readonly string eventForEmailCreatedSuccessfully = "eventForEmailCreatedSuccessfully";
        public static readonly string eventForEmailCreatedFailed = "eventForEmailCreatedFailed";
        public static readonly string eventForEmailUpdatedSuccessfully = "eventForEmailUpdatedSuccessfully";
        public static readonly string eventForEmailUpdateFailed = "eventForEmailUpdateFailed";
        public static readonly string eventForEmailDeletedSuccessfully = "eventForEmailDeletedSuccessfully";
        public static readonly string eventForEmailDeleteFailed = "eventForEmailDeleteFailed";
    }
}