﻿namespace MyGoogleCalendarServices.Web.Logic
{
    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Calendar.v3;
    using Google.Apis.Calendar.v3.Data;
    using Google.Apis.Services;
    using Models;
    using Requests;
    using Responses;
    using System;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Web.Http.ModelBinding;
    public class CalendarLogic
    {
        private static CalendarService GoogleCalService = null;
        private ModelStateDictionary ModelState = null;
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
        public CalendarLogic(ModelStateDictionary ModelState)
        {
            this.ModelState = ModelState;
        }
        #region Private methods
        private void CreateAttendeLog(UpdateEvents2Response response, string email, string googleId, string eventId)
        {
            try
            {
                var rec1 = db1.EventsAttendees.FirstOrDefault(x => x.Email == email && x.EventAppId == eventId);
                if (rec1 != null)
                {
                    rec1.UpdateDate = DateTime.Now;
                    rec1.Status = StatusCodes.eventForEmailUpdatedSuccessfully;
                    db1.SaveChanges();
                    response.Results.Add(new EmailEntry { EmailStatusId = StatusCodes.eventForEmailUpdatedSuccessfully, Email = email, EmailSuccess = "ok" });
                }
                else
                {
                    rec1 = new DataAccess.EventsAttendee
                    {
                        Email = email,
                        GoogleId = googleId,
                        EventAppId = eventId,
                        Status = StatusCodes.eventForEmailCreatedSuccessfully,
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now,
                    };
                    db1.EventsAttendees.Add(rec1);
                    db1.SaveChanges();
                    response.Results.Add(new EmailEntry { EmailStatusId = StatusCodes.eventForEmailCreatedSuccessfully, Email = email, EmailSuccess = "ok" });
                }
            }
            catch(Exception ex)
            {
                LogError(ex, response, eventId, email, StatusCodes.eventForEmailUpdateFailed, "Error");
            }
        }
        private DataAccess.Event CrateEventLog(Responses.UpdateEvents2Response response, UpdateEvents2Request request)
        {
            DataAccess.Event rec1 = null;
            try
            {
                rec1 = db1.Events.FirstOrDefault(x => x.AppId == request.AppId);
                if (rec1 == null)
                {
                    rec1 = new DataAccess.Event
                    {
                        AppId = request.AppId,
                        CreatedDate = DateTime.Now,
                        Summary = request.EventName,
                        UpdateDate = DateTime.Now,
                        Status = "crated",
                        ColorId = request.ColorId.Value,
                        Description = request.Description,
                        EndTime = request.EndTimeObj,
                        Location = request.Location,
                        SoftClientNum = request.SoftClientNum.Value,
                        SoftDbNum = request.SoftDbNum.Value,
                        StartTime = request.StartTimeObj
                    };
                    db1.Events.Add(rec1);
                    db1.SaveChanges();
                }
                else
                {
                    rec1.ColorId = request.ColorId.Value;
                    rec1.Description = request.Description;
                    rec1.EndTime = request.EndTimeObj;
                    rec1.Location = request.Location;
                    rec1.SoftClientNum = request.SoftClientNum.Value;
                    rec1.SoftDbNum = request.SoftDbNum.Value;
                    rec1.StartTime = request.StartTimeObj;
                    rec1.Status = "updated";
                    rec1.Summary = request.EventName;
                    rec1.UpdateDate = DateTime.Now;
                    db1.SaveChanges();
                }
            }
            catch (Exception ex2)
            {
                LogError(ex2, response, request.AppId, "", "", "Error");
                rec1 = null;
            }
            return rec1;
        }
        private void CreateSingleEvent(UpdateEvents2Request request, UpdateEvents2Response response, EmailEntry email, DataAccess.Event eventRec)
        {
            try
            {
                var cal1 = GoogleCalService.Calendars.Get(email.Email).Execute();
                Event myEvent = new Event { Summary = request.EventName, Description = request.Description, Location = request.Location, ColorId = request.ColorId.ToString() };
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
                myEvent = GoogleCalService.Events.Insert(myEvent, cal1.Id).Execute();
                CreateAttendeLog(response, email.Email, myEvent.Id, eventRec.AppId);
            }
            catch (Exception ex)
            {
                LogError(ex, response, eventRec.AppId, email.Email, StatusCodes.eventForEmailCreatedFailed, "Error");
            }
        }
        private void DeleteEvent(UpdateEvents2Request request, UpdateEvents2Response response)
        {
            try
            {
                var eventRec1 = db1.Events.FirstOrDefault(x => x.AppId == request.AppId);
                if (eventRec1 == null)
                    response.SetFailed(StatusCodes.eventIdNotFound, "Error");
                else
                {
                    var itemsForDelete = eventRec1.EventsAttendees.ToArray();
                    foreach (var e in itemsForDelete)
                    {
                        DeleteSingleEvent(request, response, e);
                    }
                    db1.Events.Remove(eventRec1);
                    db1.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                LogError(ex, response, request.AppId, "", "", "Error");
            }
        }

        private void DeleteSingleEvent(UpdateEvents2Request request, UpdateEvents2Response response, DataAccess.EventsAttendee attendee, System.Net.HttpStatusCode HttpStatusCode)
        {
            try
            {
                switch (HttpStatusCode)
                {
                    case System.Net.HttpStatusCode.OK:
                        db1.EventsAttendees.Remove(attendee);
                        db1.SaveChanges();
                        response.Results.Add(new EmailEntry { EmailStatusId = StatusCodes.eventForEmailDeletedSuccessfully, Email = attendee.Email, EmailSuccess = "ok" });
                        break;
                    case System.Net.HttpStatusCode.Gone:
                        db1.EventsAttendees.Remove(attendee);
                        db1.SaveChanges();
                        response.Results.Add(new EmailEntry { EmailStatusId = StatusCodes.eventForEmailDeleteFailed, Email = attendee.Email, EmailSuccess = "ok" });
                        break;
                    default:
                        response.Results.Add(new EmailEntry { EmailStatusId = StatusCodes.eventForEmailDeleteFailed, Email = attendee.Email, EmailSuccess = "Error" });
                        break;
                }
            }
            catch
            {
            }
        }
        private void DeleteSingleEvent(UpdateEvents2Request request, UpdateEvents2Response response, DataAccess.EventsAttendee attendee)
        {
            System.Net.HttpStatusCode HttpStatusCode = System.Net.HttpStatusCode.OK;
            try
            {
                var deletedEvent = GoogleCalService.Events.Delete(attendee.Email, attendee.GoogleId).Execute();
            }
            catch (Google.GoogleApiException ex1)
            {
                HttpStatusCode = ex1.HttpStatusCode;
            }
            catch
            {
                HttpStatusCode = System.Net.HttpStatusCode.InternalServerError;
            }
            finally
            {
                DeleteSingleEvent(request, response, attendee, HttpStatusCode);
            }
        }
        private bool IntializeGoogeService(UpdateEvents2Response response)
        {
            try
            {
                if (GoogleCalService == null)
                {
                    var path1 = System.Configuration.ConfigurationManager.AppSettings["google_key_file"];
                    var keyFile = System.IO.File.ReadAllBytes(path1);
                    var certificate = new X509Certificate2(keyFile, "notasecret", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);
                    var ServiceAccountCredentialInitializer = new ServiceAccountCredential.Initializer("serviceaccount01@softegg-eruim-services.iam.gserviceaccount.com");
                    ServiceAccountCredentialInitializer.Scopes = new string[] { CalendarService.Scope.Calendar };
                    var credential = new ServiceAccountCredential(ServiceAccountCredentialInitializer.FromCertificate(certificate));
                    GoogleCalService = new CalendarService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "GoogleCalendarServices03",
                    });
                }
            }
            catch(Exception ex)
            {
                LogError(ex, response, "", "", "", "Error");
            }
            return GoogleCalService != null;
        }
        private void LogError(Exception ex = null, UpdateEvents2Response respponse = null, string appId = "", string email = "", string action = "", string success = "")
        {
            if (respponse != null)
            {
                if (string.IsNullOrEmpty(email))
                    respponse.SetFailed(StatusCodes.unhandlesException, success);
                else
                {
                    var emailResult = new EmailEntry
                    {
                        Email = email,
                        EmailSuccess = success,
                        EmailStatusId = action
                    };
                    respponse.Results.Add(emailResult);
                }
            }
            try
            {
                DataAccess.ErrorLog r1 = new DataAccess.ErrorLog
                {
                    CreateDate = DateTime.Now,
                    ExceptionMessage = ex.ToString(),
                    Info = success,
                    AppId = appId,
                    email = email
                };
                db1.ErrorLogs.Add(r1);
                db1.SaveChanges();
            }
            catch
            {
            }
        }
        private void UpdateEvents(UpdateEvents2Request request, UpdateEvents2Response response, DataAccess.Event eventRec)
        {
            DataAccess.EventsAttendee attendeeRec = null;
            foreach (var item in request.Entries)
            {
                attendeeRec = eventRec.EventsAttendees.FirstOrDefault(x => x.Email == item.Email);
                if (attendeeRec == null)
                    CreateSingleEvent(request, response, item, eventRec);
                else
                    UpdateSingleEvent(request, response, attendeeRec);
            }
            var emails = request.Entries.Select(c1 => c1.Email).ToArray();
            var attendeesForDelete = eventRec.EventsAttendees.Where(c1 => !emails.Contains(c1.Email)).ToArray();
            foreach (var item in attendeesForDelete)
            {
                DeleteSingleEvent(request, response, item);
            }
        }
        private void UpdateEvents(UpdateEvents2Request request, UpdateEvents2Response  response)
        {
            if(IntializeGoogeService(response))
            {
                if (request.Action == "delete")
                    DeleteEvent(request, response);
                else
                {
                    var eventRec = CrateEventLog(response, request);
                    if (eventRec != null) UpdateEvents(request, response, eventRec);
                }
            }
        }
        private void UpdateSingleEvent(UpdateEvents2Request request, UpdateEvents2Response response, DataAccess.EventsAttendee attendeeRec)
        {
            try
            {
                var event1 = GoogleCalService.Events.Get(attendeeRec.Email, attendeeRec.GoogleId).Execute();
                if (event1.Status == "cancelled")
                    event1.Status = "confirmed";
                event1.Summary = request.EventName;
                event1.Location = request.Location;
                event1.Description = request.Description;
                event1.Start = new EventDateTime { DateTime = request.StartTimeObj };
                event1.End = new EventDateTime { DateTime = request.EndTimeObj };
                event1.ColorId = request.ColorId.ToString();
                var result = GoogleCalService.Events.Update(event1, attendeeRec.Email, attendeeRec.GoogleId).Execute();
                CreateAttendeLog(response, attendeeRec.Email, attendeeRec.GoogleId, attendeeRec.EventAppId);
            }
            catch (Exception ex)
            {
                LogError(ex, response, attendeeRec.EventAppId, attendeeRec.Email, StatusCodes.eventForEmailUpdateFailed, "Error");
            }
        }
        #endregion
        public UpdateEvents2Response UpdateEvents(UpdateEvents2Request request)
        {
            UpdateEvents2Response response = new UpdateEvents2Response();
            if (request == null)
                response.SetFailed(StatusCodes.requestMissing, "Error");
            else
            {
                response.AppId = request.AppId;
                if (ModelState.IsValid)
                    UpdateEvents(request, response);
                else
                    response.SetFailed(StatusCodes.validationErrors, "Error", ModelState);
            }
            return response;
        }
    }
}