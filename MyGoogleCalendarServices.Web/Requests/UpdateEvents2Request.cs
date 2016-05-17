using MyGoogleCalendarServices.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace MyGoogleCalendarServices.Web.Requests
{
    public class UpdateEvents2Request : System.ComponentModel.DataAnnotations.IValidatableObject
    {
        public string Action { get; set; }
        public string EventName { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public short? ColorId { get; set; }
        public string AppId { get; set; }
        public string GoogleId { get; set; }
        public string StartTime { get; set; }
        [IgnoreDataMember]
        public DateTime StartTimeObj { get; set; }
        public string EndTime { get; set; }
        [IgnoreDataMember]
        public DateTime EndTimeObj { get; set; }
        public List<EmailEntry> Entries { get; set; }
        public List<string> ValidationErrors { get; private set; }
        public int? SoftClientNum { get; set; }
        public short? SoftDbNum { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validations = new List<ValidationResult>();
            var r1 = validationContext.ObjectInstance as UpdateEvents2Request;
            switch (r1.Action)
            {
                case "delete":
                    ValidateDeleteRequest(validations, validationContext);
                    break;
                default:
                    if (r1.Action == null)
                        ValidateCreateRequest(validations, validationContext);
                    else
                        validations.Add(new ValidationResult("קוד פעולה יכול להיות delete בלבד, לכול פעולה אחרת אין לשלוח אותו"));
                    break;

            }
            return validations;
        }
        private void ValidateDeleteRequest(List<ValidationResult> validarionErrors, ValidationContext validationContext)
        {
            var r1 = validationContext.ObjectInstance as UpdateEvents2Request;
            if (string.IsNullOrEmpty(r1.AppId))
                validarionErrors.Add(new ValidationResult("במחיקת אירוע, חובה להעביר מזהה אירוע אפליקטיבי", new string[] { "AppId" }));
        }
        private void ValidateCreateRequest(List<ValidationResult> validations, ValidationContext validationContext)
        {
            var r1 = validationContext.ObjectInstance as UpdateEvents2Request;

            //  מועד התחלה
            DateTime dt0 = DateTime.MinValue;
            if (DateTime.TryParseExact(r1.StartTime, "yyyyMMddHHmm", null, System.Globalization.DateTimeStyles.None, out dt0))
                r1.StartTimeObj = dt0;
            else
                validations.Add(new ValidationResult("מועד תחילת אירוע, לא בפורמט yyyyMMddHHmm", new string[] { "StartTime" }));

            //  מועד סיום
            if (DateTime.TryParseExact(r1.EndTime, "yyyyMMddHHmm", null, System.Globalization.DateTimeStyles.None, out dt0))
                r1.EndTimeObj = dt0;
            else
                validations.Add(new ValidationResult("מועד סיום אירוע, לא בפורמט yyyyMMddHHmm", new string[] { "EndTime" }));

            //  רשימת נמענים
            if (r1.Entries == null || r1.Entries.Count == 0)
                validations.Add(new ValidationResult("רשימת מוזמנים ריקה, יש לספק לפחות כתובת נמען אחת", new string[] { "Entries " }));

            //  שם אירוע
            if (string.IsNullOrEmpty(r1.EventName))
                validations.Add(new ValidationResult("שם אירוע חסר", new string[] { "EventName" }));

            //  AppId
            if (string.IsNullOrEmpty(r1.AppId))
                validations.Add(new ValidationResult("קוד זיהוי אפליקציה, חסר", new string[] { "AppId" }));

            //  ColorId
            if (!ColorId.HasValue)
                validations.Add(new ValidationResult("קוד זיהוי צבע, חסר", new string[] { "ColorId" }));

            //  תיאור
            if (string.IsNullOrEmpty(r1.Description))
                validations.Add(new ValidationResult("תיאור, חסר", new string[] { "Description" }));

            //  מיקום
            if (string.IsNullOrEmpty(r1.Location))
                validations.Add(new ValidationResult("מיקום , חסר", new string[] { "Location" }));

            if (!SoftClientNum.HasValue)
                validations.Add(new ValidationResult("זיהוי לקוח , חסר", new string[] { "SoftClientNum" }));

            if (!SoftDbNum.HasValue)
                validations.Add(new ValidationResult("מספר בסיס נתונים , חסר", new string[] { "SoftDbNum" }));
        }
    }
}