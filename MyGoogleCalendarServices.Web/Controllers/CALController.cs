namespace MyGoogleCalendarServices.Web.Controllers
{
    using MyGoogleCalendarServices.Web.Logic;
    using MyGoogleCalendarServices.Web.Requests;
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
            CalendarLogic x1 = new CalendarLogic(ModelState);
            var response = x1.UpdateEvents(request);
            return Content(HttpStatusCode.OK, response, new CustomXmlMediaTypeFormatter(), "text/xml");
        }

        [Route("api/cal/geta")]
        public string GetA()
        {
            return "A";
        }
    }
}
