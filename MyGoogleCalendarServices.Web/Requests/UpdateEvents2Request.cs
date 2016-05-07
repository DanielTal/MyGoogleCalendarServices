﻿using MyGoogleCalendarServices.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;

namespace MyGoogleCalendarServices.Web.Requests
{
    public class UpdateEvents2Request : System.ComponentModel.DataAnnotations.IValidatableObject
    {
        public string Action { get; set; }
        public string EventName { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string ColorId { get; set; }
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
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validations = new List<ValidationResult>();
            var r1 = validationContext.ObjectInstance as UpdateEvents2Request;
            var validActions = new string[] { "create", "create2", "update", "update2", "delete" };
            if (!validActions.Contains(r1.Action))
            {
                validations.Add(new ValidationResult("חסר קוד פעולה, ערכים אפשריים : create, update, delete", new string[] { "Action" }));
            }
            else
            {
                switch (r1.Action)
                {
                    case "create":
                    case "create2":
                        ValidateCreateRequest(validations, validationContext);
                        break;
                    case "update":
                        ValidateUpdateRequest(validations, validationContext);
                        break;
                    case "update2":
                        ValidateUpdate2Request(validations, validationContext);
                        break;
                    case "delete":
                        ValidateDeleteRequest(validations, validationContext);
                        break;
                    case "delete2":
                        ValidateDelete2Request(validations, validationContext);
                        break;
                }
            }
            return validations;
        }

        private void ValidateDeleteRequest(List<ValidationResult> validations, ValidationContext validationContext)
        {
            var r1 = validationContext.ObjectInstance as UpdateEvents2Request;
            if (r1.Entries == null || r1.Entries.Count == 0)
            {
                validations.Add(new ValidationResult("במחיקת אירוע, יש לספק רשימת נמענים כולל זיהוי גוגל", new string[] { "Entries" }));
            }
            else
            {
                foreach (var entry in r1.Entries)
                {
                    if (string.IsNullOrEmpty(entry.Email))
                    {
                        validations.Add(new ValidationResult("במחיקת אירוע יש לספק כתובת מייל" + entry.Email, new string[] { "Email" }));
                    }

                    if (string.IsNullOrEmpty(entry.GoogleId))
                    {
                        validations.Add(new ValidationResult("במחיקת אירוע יש לספק מזהה גוגל" + entry.Email, new string[] { "GoogleId" }));
                    }
                }
            }
        }

        private void ValidateDelete2Request(List<ValidationResult> validarionErrors, ValidationContext validationContext)
        {
            var r1 = validationContext.ObjectInstance as UpdateEvents2Request;
            if (string.IsNullOrEmpty(r1.GoogleId))
                validarionErrors.Add(new ValidationResult("במחיקת אירוע, חובה להעביר מזהה אירוע של גוגל", new string[] { "GoogleId" }));
        }

        private void ValidateUpdate2Request(List<ValidationResult> validations, ValidationContext validationContext)
        {
            var r1 = validationContext.ObjectInstance as UpdateEvents2Request;
            DateTime dt0 = DateTime.MinValue;

            if (DateTime.TryParseExact(r1.StartTime, "yyyyMMddHHmm", null, System.Globalization.DateTimeStyles.None, out dt0))
                r1.StartTimeObj = dt0;
            else
                validations.Add(new ValidationResult("מועד תחילת אירוע, לא בפורמט yyyyMMddHHmm", new string[] { "StartTime" }));

            if (DateTime.TryParseExact(r1.EndTime, "yyyyMMddHHmm", null, System.Globalization.DateTimeStyles.None, out dt0))
                r1.EndTimeObj = dt0;
            else
                validations.Add(new ValidationResult("מועד סיום אירוע, לא בפורמט yyyyMMddHHmm", new string[] { "EndTime" }));

            if (string.IsNullOrEmpty(r1.GoogleId))
                validations.Add(new ValidationResult("בעדכון אירוע, חובה להעביר מזהה אירוע של גוגל", new string[] { "GoogleId" }));
        }
        private void ValidateUpdateRequest(List<ValidationResult> validations, ValidationContext validationContext)
        {
            var r1 = validationContext.ObjectInstance as UpdateEvents2Request;
            DateTime dt0 = DateTime.MinValue;

            if (DateTime.TryParseExact(r1.StartTime, "yyyyMMddHHmm", null, System.Globalization.DateTimeStyles.None, out dt0))
                r1.StartTimeObj = dt0;
            else
                validations.Add(new ValidationResult("מועד תחילת אירוע, לא בפורמט yyyyMMddHHmm", new string[] { "StartTime" }));

            if (DateTime.TryParseExact(r1.EndTime, "yyyyMMddHHmm", null, System.Globalization.DateTimeStyles.None, out dt0))
                r1.EndTimeObj = dt0;
            else
                validations.Add(new ValidationResult("מועד סיום אירוע, לא בפורמט yyyyMMddHHmm", new string[] { "EndTime" }));

            if(r1.Entries == null || r1.Entries.Count == 0)
            {
                validations.Add(new ValidationResult("בעדכון אירוע מסוג בודד לכל נמען, יש לספק רשימת נמענים כולל זיהוי גוגל וקוד פעולה", new string[] { "Entries" }));
            }
            else
            {
                string[] validActions = { "update", "delete" };
                foreach (var entry in r1.Entries)
                {
                    if (string.IsNullOrEmpty(entry.Email))
                    {
                        validations.Add(new ValidationResult("בעדכון אירוע מסוג בודד לכל נמען, חסר כתובת מייל" + entry.Email, new string[] { "Email" }));
                    }

                    if (string.IsNullOrEmpty(entry.Email))
                    {
                        validations.Add(new ValidationResult("בעדכון אירוע מסוג בודד לכל נמען, חסר מזהה גוול" + entry.Email, new string[] { "GoogleId" }));
                    }

                    if (!validActions.Contains (entry.Action))
                    {
                        validations.Add(new ValidationResult("בעדכון אירוע מסוג בודד לכל נמען, קוד פעולה לא תקין עבור " + entry.Email, new string[] { "Action" }));
                    }
                }
            }
        }

        private void ValidateCreateRequest(List<ValidationResult> validations, ValidationContext validationContext)
        {
            var r1 = validationContext.ObjectInstance as UpdateEvents2Request;
            DateTime dt0 = DateTime.MinValue;
            if (DateTime.TryParseExact(r1.StartTime, "yyyyMMddHHmm", null, System.Globalization.DateTimeStyles.None, out dt0))
                r1.StartTimeObj = dt0;
            else
                validations.Add(new ValidationResult("מועד תחילת אירוע, לא בפורמט yyyyMMddHHmm", new string[] { "StartTime" }));

            if (DateTime.TryParseExact(r1.EndTime, "yyyyMMddHHmm", null, System.Globalization.DateTimeStyles.None, out dt0))
                r1.EndTimeObj = dt0;
            else
                validations.Add(new ValidationResult("מועד סיום אירוע, לא בפורמט yyyyMMddHHmm", new string[] { "EndTime" }));

            if (r1.Entries == null || r1.Entries.Count == 0)
                validations.Add(new ValidationResult("רשימת מוזמנים ריקה, יש לספק לפחות כתובת נמען אחת", new string[] { "Entries " }));
        }
    }
}