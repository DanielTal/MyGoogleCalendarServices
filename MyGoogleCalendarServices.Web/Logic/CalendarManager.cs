using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using MyGoogleCalendarServices.Web.Models;
using MyGoogleCalendarServices.Web.Requests;
using MyGoogleCalendarServices.Web.Responses;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace MyGoogleCalendarServices.Web.Logic
{
    public sealed partial class CalendarManager
    {
        #region Static members
        private static volatile CalendarManager instance;
        private static object syncRoot = new Object();
        public static CalendarManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new CalendarManager();
                            instance.Initialize();
                        }
                    }
                }
                return instance;
            }
        }
        #endregion

        #region Private members
        private ServiceAccountCredential credential = null;
        private CalendarService service = null;
        private static readonly string SERVIVE_EMAIL = "softegg.services@gmail.com";
        #endregion

        #region Constructors
        private CalendarManager()
        {
        }
        #endregion

        #region Private methods
        private bool Initialize()
        {
            var success = GetGredential() && GetSerivice();
            return success;
        }
        private bool GetGredential()
        {
            var path1 = System.Configuration.ConfigurationManager.AppSettings["google_key_file"];
            var keyFile = System.IO.File.ReadAllBytes(path1);
            var certificate = new X509Certificate2(keyFile, "notasecret", X509KeyStorageFlags.Exportable);
            var ServiceAccountCredentialInitializer = new ServiceAccountCredential.Initializer("serviceaccount01@softegg-eruim-services.iam.gserviceaccount.com");
            ServiceAccountCredentialInitializer.Scopes = new string[] { CalendarService.Scope.Calendar };
            credential = new ServiceAccountCredential(ServiceAccountCredentialInitializer.FromCertificate(certificate));
            return credential != null;
        }
        private bool GetSerivice()
        {
            service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "GoogleCalendarServices03",
            });
            return service != null;
        }
        #region Version 2
        #region Private methods
        private void CreateEvent2(UpdateEvents2Request request, UpdateEvents2Response response)
        {
            var cal1 = service.Calendars.Get(SERVIVE_EMAIL).Execute();
            if (cal1 == null)
                response.SetStatus(-3, "כתובת חשבון שירות לא נמצאה");
            else
            {
                Event myEvent = new Event { Summary = request.EventName, Description = request.Description, Location = request.Location, ColorId = request.ColorId };
                if (request.StartTimeObj == request.EndTimeObj)
                {
                    myEvent.Start = new EventDateTime() { Date = request.StartTimeObj.Date.ToString("yyyy-MM-dd") };
                    myEvent.End = new EventDateTime() { Date = request.EndTimeObj.Date.ToString("yyyy-MM-dd") };
                }
                else
                {
                    myEvent.Start = new EventDateTime() { DateTime = request.StartTimeObj.Date };
                    myEvent.End = new EventDateTime() { DateTime = request.EndTimeObj.Date };
                }
                myEvent.Attendees = SetAttendess(request);
                myEvent = service.Events.Insert(myEvent, cal1.Id).Execute();
                response.SetStatus(0, "אירוע נרשם בהצלחה");
                response.GoogleId = myEvent.Id;
            }
        }
        private void CreateEvent(UpdateEvents2Request request, UpdateEvents2Response response)
        {
            foreach (var email in request.Entries)
            {
                CreateSingleEvent(request, response, email);
            }
        }

        private void CreateSingleEvent(UpdateEvents2Request request, UpdateEvents2Response response, EmailEntry email)
        {
            try
            {
                var cal1 = service.Calendars.Get(email.Email).Execute();
                Event myEvent = new Event { Summary = request.EventName, Description = request.Description, Location = request.Location, ColorId = request.ColorId };
                if (request.StartTimeObj == request.EndTimeObj)
                {
                    myEvent.Start = new EventDateTime() { Date = request.StartTimeObj.Date.ToString("yyyy-MM-dd") };
                    myEvent.End = new EventDateTime() { Date = request.EndTimeObj.Date.ToString("yyyy-MM-dd") };
                }
                else
                {
                    myEvent.Start = new EventDateTime() { DateTime = request.StartTimeObj };
                    myEvent.End = new EventDateTime() { DateTime = request.EndTimeObj };
                }
                myEvent = service.Events.Insert(myEvent, cal1.Id).Execute();
                response.Results.Add(new EmailEntry { Action = "create", Email = email.Email, GoogleId = myEvent.Id, Status = "אירוע נוצר בהצלחה" });
            }
            catch
            {
                response.Results.Add(new EmailEntry { Action = "create", Email = email.Email, GoogleId = "", Status = "כתובת לא נמצאה" });
            }
        }

        private void DeleteEvent(UpdateEvents2Request request, UpdateEvents2Response response, string email, string GoogleId)
        {
            try
            {
                var deletedEvent = service.Events.Delete(email, GoogleId).Execute();
                if (email == SERVIVE_EMAIL)
                    response.SetStatus(0, "אירוע נמחק בהצלחה");
                else
                    response.Results.Add(new EmailEntry { Action = "delete", Email = email, GoogleId = GoogleId, Status = "אירוע נמחק בהצלחה" });
            }
            catch(Exception ex)
            {
                response.Results.Add(new EmailEntry { Action = "delete", Email = email, GoogleId = GoogleId, Status = "כשל במחיקת אירוע, ייתכן שהאירוע נמחק כבר על ידי המשתמש" });
            }
        }

        private void DeleteEvent(UpdateEvents2Request request, UpdateEvents2Response response)
        {
            foreach (var item in request.Entries)
            {
                DeleteEvent(request, response, item.Email, item.GoogleId);
            }
        }
        private void UpdateEvent(UpdateEvents2Request request, UpdateEvents2Response response,  string email, string GoogleId)
        {
            try
            {
                var event1 = service.Events.Get(email, GoogleId).Execute();
                event1.Summary = request.EventName;
                event1.Location = request.Location;
                event1.Description = request.Description;
                event1.Start = new EventDateTime { DateTime = request.StartTimeObj };
                event1.End = new EventDateTime { DateTime = request.EndTimeObj };
                event1.ColorId = request.ColorId;
                if (request.Entries != null && request.Entries.Count > 0 && email == SERVIVE_EMAIL)
                    event1.Attendees = SetAttendess(request);
                var result = service.Events.Update(event1, email, GoogleId).Execute();
                if (email == SERVIVE_EMAIL)
                    response.SetStatus(0, "האירוע עודכן בהצלחה");
                else
                    response.Results.Add(new EmailEntry { Action = "update", Email = email, GoogleId = GoogleId, Status = "אירוע עודכן בהצלחה" });
            }
            catch(Exception ex)
            {
                response.Results.Add(new EmailEntry { Action = "update", Email = email, GoogleId = GoogleId, Status = "כשל בעדכון אירוע, ייתכן שהמשתמש כבר מחק את האירוע" });
            }
        }
        private void UpdateEvent(UpdateEvents2Request request, UpdateEvents2Response response)
        {
            foreach (var email in request.Entries)
            {
                switch (email.Action)
                {
                    case "update":
                        UpdateEvent(request, response, email.Email, email.GoogleId);
                        break;
                    case "delete":
                        DeleteEvent(request, response, email.Email, email.GoogleId);
                        break;
                }
            }
        }
        private List<EventAttendee> SetAttendess(UpdateEvents2Request request)
        {
            EventAttendee attendee1 = null;
            List<EventAttendee> attendees1 = new List<EventAttendee>();
            foreach (var entry in request.Entries)
            {
                attendee1 = new EventAttendee
                {
                    Comment = entry.Email,
                    DisplayName = "מוזמן : " + entry.Email,
                    Email = entry.Email,
                    Optional = false,
                    ResponseStatus = "accepted",
                    Organizer = true,
                    Self = true
                };
                attendees1.Add(attendee1);
            }
            return attendees1;
        }
        #endregion

        #region Internal methods
        internal void UpdateEvents(UpdateEvents2Request request, UpdateEvents2Response response)
        {
            switch (request.Action)
            {
                case "create":
                    CreateEvent(request, response);
                    response.SetStatus(0, "בדוק סטאטוסים ברמת הנמענים");
                    break;
                case "update":
                    UpdateEvent(request, response);
                    response.SetStatus(0, "בדוק סטאטוסים ברמת הנמענים");
                    break;
                case "delete":
                    DeleteEvent(request, response);
                    break;
                case "create2":
                    CreateEvent2(request, response);
                    break;
                case "update2":
                    UpdateEvent(request, response, SERVIVE_EMAIL, request.GoogleId);
                    break;
            }
        }
        #endregion
        #endregion
        #endregion
    }
}