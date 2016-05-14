namespace MyGoogleCalendarServices.Web.Controllers
{
    using MyGoogleCalendarServices.Web.Logic;
    using MyGoogleCalendarServices.Web.Requests;
    using MyGoogleCalendarServices.Web.Responses;
    using System;
    using System.Net;
    using System.Web.Http;
    
    public class CALController : ApiController
    {
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

        [HttpPost]
        [Route("api/cal/UpdateEvents")]
        public IHttpActionResult UpdateEvents(UpdateEvents2Request request)
        {
            //UpdateEvents2Response response = new UpdateEvents2Response();
            //if (request == null)
            //{
            //    response.SetFailed(StatusCodes.requestMissing, "פרמטר בקשה לא נשלח עם הבקשה");
            //}
            //else
            //{
            //    response.AppId = request.AppId;
            //    if (ModelState.IsValid)
            //    {
            //        try
            //        {
            //            CalendarManager.Instance.UpdateEvents(request, response);
            //        }
            //        catch (Exception ex)
            //        {
            //            LogError(ex);
            //            response.SetFailed(StatusCodes.unhandlesException, "תקלה כללית בשירות", null, ex);
            //        }
            //    }
            //    else
            //        response.SetFailed(StatusCodes.validationErrors, "שגיאות וולידציה", ModelState);
            //}
            //return Content(HttpStatusCode.OK, response, Configuration.Formatters.XmlFormatter);
            CalendarLogic x1 = new CalendarLogic(ModelState);
            var response = x1.UpdateEvents(request);
            return Content(HttpStatusCode.OK, response, Configuration.Formatters.XmlFormatter);
        }

        [Route("api/cal/geta")]
        public string GetA()
        {
            return "A";
        }
    }
}
