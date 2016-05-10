namespace MyGoogleCalendarServices.Web.Responses
{
    public sealed class StatusCodes
    {
        public static readonly string noErrors = "noErrors";
        public static readonly string requestMissing = "requestMissing";
        public static readonly string validationErrors = "validationErrors";
        public static readonly string unhandlesException = "unhandlesException";
        public static readonly string eventIdNotFound= "eventIdNotFound";
    }
}