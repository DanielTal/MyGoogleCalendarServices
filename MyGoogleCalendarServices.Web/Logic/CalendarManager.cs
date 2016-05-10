using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using MyGoogleCalendarServices.Web.Models;
using MyGoogleCalendarServices.Web.Requests;
using MyGoogleCalendarServices.Web.Responses;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Data.Entity.Validation;

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
        private DataAccess.DBEntities _db1 = null;
        private DataAccess.DBEntities db1
        {
            get
            {
                if (_db1 == null)
                    _db1 = new DataAccess.DBEntities();
                return _db1;
            }
        }
        #endregion

        #region Constructors
        private CalendarManager()
        {
        }
        #endregion

        #region Private methods
        private DataAccess.Event AddEvent(Responses.UpdateEvents2Response response, string appId, string summary)
        {
            DataAccess.Event rec1 = null;
            try
            {
                rec1 = db1.Events.FirstOrDefault(x => x.AppId == appId);
                if (rec1 == null)
                {
                    rec1 = new DataAccess.Event
                    {
                        AppId = appId,
                        CreatedDate = DateTime.Now,
                        Summary = summary,
                        UpdateDate = DateTime.Now
                    };
                    db1.Events.Add(rec1);
                    db1.SaveChanges();
                }
                else
                {
                    rec1.Summary = summary;
                    db1.SaveChanges();
                }
            }
            catch (Exception ex2)
            {
                LogError(ex2);
                response.SetFailed(StatusCodes.unhandlesException, "תקלה ברישום או עדכון בדטבייס", null, ex2);
                rec1 = null;
            }
            return rec1;
        }
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
        //private void SetExtendedProperties(Event myEvent)
        //{
        //    var x1 = new Event.ExtendedPropertiesData();
        //    x1.Private__ = new System.Collections.Generic.Dictionary<string, string>();
        //    x1.Shared = new System.Collections.Generic.Dictionary<string, string>();
        //    x1.Private__.Add("KEY1", "aaa");
        //    x1.Private__.Add("KEY2", "bbb");
        //    x1.Private__.Add("KEY3", "ccc");
        //    x1.Shared.Add("KEY4", "000");
        //    x1.Shared.Add("KEY5", "111");
        //    x1.Shared.Add("KEY6", "222");
        //    myEvent.ExtendedProperties = x1;
        //}

        //private void TEST1()
        //{
        //    try
        //    {
        //        //var events = service.Events.List("serviceaccount01@softegg-eruim-services.iam.gserviceaccount.com").Execute();
        //        var events = service.Events.List("primary").Execute();
        //        var x1 = service.Acl.List("primary").Execute();
        //        foreach (var event1 in events.Items)
        //        {
        //            System.Diagnostics.Debug.WriteLine(event1.Summary);
        //        }
        //    }
        //    catch(Exception ex)
        //    {
        //    }
        //}

        //private void CreateEvent2(UpdateEvents2Request request, UpdateEvents2Response response)
        //{
        //    var cal1 = service.Calendars.Get(SERVIVE_EMAIL).Execute();
        //    if (cal1 == null)
        //        response.SetStatus(-3, "כתובת חשבון שירות לא נמצאה");
        //    else
        //    {
        //        Event myEvent = new Event { Summary = request.EventName, Description = request.Description, Location = request.Location, ColorId = request.ColorId };
        //        if (request.StartTimeObj == request.EndTimeObj)
        //        {
        //            myEvent.Start = new EventDateTime() { Date = request.StartTimeObj.Date.ToString("yyyy-MM-dd") };
        //            myEvent.End = new EventDateTime() { Date = request.EndTimeObj.Date.ToString("yyyy-MM-dd") };
        //        }
        //        else
        //        {
        //            myEvent.Start = new EventDateTime() { DateTime = request.StartTimeObj.Date };
        //            myEvent.End = new EventDateTime() { DateTime = request.EndTimeObj.Date };
        //        }
        //        myEvent.Attendees = SetAttendess(request);
        //        myEvent = service.Events.Insert(myEvent, cal1.Id).Execute();
        //        response.SetStatus(0, "אירוע נרשם בהצלחה");
        //        response.GoogleId = myEvent.Id;
        //    }
        //}
        //private void CreateEvent(UpdateEvents2Request request, UpdateEvents2Response response)
        //{
        //    foreach (var email in request.Entries)
        //    {
        //        CreateSingleEvent(request, response, email);
        //    }
        //}

        private void CreateSingleEvent(UpdateEvents2Request request, UpdateEvents2Response response, EmailEntry email, DataAccess.Event eventRec)
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
                //SetExtendedProperties(myEvent);
                myEvent = service.Events.Insert(myEvent, cal1.Id).Execute();
                eventRec.EventsAttendees.Add(new DataAccess.EventsAttendee
                {
                    Email = email.Email,
                    GoogleId = myEvent.Id,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                });
                db1.SaveChanges();
                response.Results.Add(new EmailEntry { Action = "create", Email = email.Email, Status = "אירוע נוצר בהצלחה" });
            }
            catch(Exception ex)
            {
                LogError(ex);
                response.Results.Add(new EmailEntry { Action = "create", Email = email.Email, Status = "תקלה ברישום אירוע לנמען" });
            }
        }
        private void LogError(Exception ex)
        {
            try
            {
                DataAccess.ErrorLog r1 = new DataAccess.ErrorLog();
                r1.ExceptionMessage = ex.ToString();
                db1.ErrorLogs.Add(r1);
                db1.SaveChanges();
            }
            catch
            {

            }
        }

        private void DeleteEvent(UpdateEvents2Request request, UpdateEvents2Response response, DataAccess.EventsAttendee attendee)
        {
            try
            {
                var deletedEvent = service.Events.Delete(attendee.Email, attendee.GoogleId).Execute();
                response.Results.Add(new EmailEntry { Action = "delete", Email = attendee.Email, Status = "אירוע נמחק בהצלחה" });
                db1.EventsAttendees.Remove(attendee);
                db1.SaveChanges();
            }
            catch(Exception ex)
            {
                LogError(ex);
                response.Results.Add(new EmailEntry { Action = "delete", Email = attendee.Email, Status = "כשל במחיקת אירוע, ייתכן שהאירוע נמחק כבר על ידי המשתמש" });
            }
        }

        private void DeleteEvent(UpdateEvents2Request request, UpdateEvents2Response response)
        {
            try
            {
                var eventRec1 = db1.Events.FirstOrDefault(x => x.AppId == request.AppId);
                if (eventRec1 == null)
                    response.SetFailed(StatusCodes.eventIdNotFound, "לא נמצא אירוע");
                else
                {
                    var itemsForDelete = eventRec1.EventsAttendees.ToArray();
                    foreach (var e in itemsForDelete)
                    {
                        DeleteEvent(request, response, e);
                    }
                    db1.Events.Remove(eventRec1);
                    db1.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                response.SetFailed(StatusCodes.unhandlesException, "תקלה במחיקת אירוע", null, ex);
            }
        }

        //private void DeleteEvent(UpdateEvents2Request request, UpdateEvents2Response response)
        //{
        //    foreach (var item in request.Entries)
        //    {
        //        DeleteEvent(request, response, item.Email, item.GoogleId);
        //    }
        //}
        private void UpdateEvent(UpdateEvents2Request request, UpdateEvents2Response response,   DataAccess.EventsAttendee attendeeRec)
        {
            try
            {
                var event1 = service.Events.Get(attendeeRec.Email, attendeeRec.GoogleId).Execute();
                event1.Summary = request.EventName;
                event1.Location = request.Location;
                event1.Description = request.Description;
                event1.Start = new EventDateTime { DateTime = request.StartTimeObj };
                event1.End = new EventDateTime { DateTime = request.EndTimeObj };
                event1.ColorId = request.ColorId;
                var result = service.Events.Update(event1, attendeeRec.Email, attendeeRec.GoogleId).Execute();
                response.Results.Add(new EmailEntry { Action = "update", Email = attendeeRec.Email, Status = "אירוע עודכן בהצלחה" });
            }
            catch(Exception ex)
            {
                LogError(ex);
                response.Results.Add(new EmailEntry { Action = "update", Email = attendeeRec.Email, Status = "כשל בעדכון אירוע, ייתכן שהמשתמש כבר מחק את האירוע" });
            }
        }
        //private void UpdateEvent(UpdateEvents2Request request, UpdateEvents2Response response)
        //{
        //    foreach (var email in request.Entries)
        //    {
        //        switch (email.Action)
        //        {
        //            case "update":
        //                UpdateEvent(request, response, email.Email, email.GoogleId);
        //                break;
        //            case "delete":
        //                DeleteEvent(request, response, email.Email, email.GoogleId);
        //                break;
        //        }
        //    }
        //}
        //private List<EventAttendee> SetAttendess(UpdateEvents2Request request)
        //{
        //    EventAttendee attendee1 = null;
        //    List<EventAttendee> attendees1 = new List<EventAttendee>();
        //    foreach (var entry in request.Entries)
        //    {
        //        attendee1 = new EventAttendee
        //        {
        //            Comment = entry.Email,
        //            DisplayName = "מוזמן : " + entry.Email,
        //            Email = entry.Email,
        //            Optional = false,
        //            ResponseStatus = "accepted",
        //            Organizer = true,
        //            Self = true
        //        };
        //        attendees1.Add(attendee1);
        //    }
        //    return attendees1;
        //}
        private void UpdateEvents(UpdateEvents2Request request, UpdateEvents2Response response, DataAccess.Event eventRec)
        {
            DataAccess.EventsAttendee attendeeRec = null;
            foreach (var item in request.Entries)
            {
                attendeeRec = eventRec.EventsAttendees.FirstOrDefault(x => x.Email == item.Email);
                if (attendeeRec == null)
                    CreateSingleEvent(request, response, item, eventRec);
                else
                    UpdateEvent(request, response, attendeeRec);
            }
            var emails = request.Entries.Select(c1 => c1.Email).ToArray();
            var attendeesForDelete = eventRec.EventsAttendees.Where(c1 => !emails.Contains(c1.Email)).ToArray();
            foreach (var item in attendeesForDelete)
            {
                DeleteEvent(request, response, item);
            }
        }
        #endregion

        #region Internal methods
        internal void UpdateEvents(UpdateEvents2Request request, UpdateEvents2Response response)
        {
            //TEST1();
            //switch (request.Action)
            //{
            //    case "create":
            //        CreateEvent(request, response);
            //        response.SetStatus(0, "בדוק סטאטוסים ברמת הנמענים");
            //        break;
            //    case "update":
            //        UpdateEvent(request, response);
            //        response.SetStatus(0, "בדוק סטאטוסים ברמת הנמענים");
            //        break;
            //    case "delete":
            //        DeleteEvent(request, response);
            //        break;
            //    case "create2":
            //        CreateEvent2(request, response);
            //        break;
            //    case "update2":
            //        UpdateEvent(request, response, SERVIVE_EMAIL, request.GoogleId);
            //        break;
            //}

            if (service == null)
            {
                LogError(new ApplicationException("Google Service is null"));
                response.SetFailed(StatusCodes.unhandlesException, "שירות גוגל לא אותחל כראוי");
                return;
            }

            if (request.Action == "delete")
                DeleteEvent(request, response);
            else
            {
                var eventRec = AddEvent(response, request.AppId, request.EventName);
                if (eventRec != null) UpdateEvents(request, response, eventRec);
            }
        }
        #endregion
        #endregion
        #endregion
    }
}