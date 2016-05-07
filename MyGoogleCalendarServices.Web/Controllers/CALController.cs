using MyGoogleCalendarServices.Web.Logic;
using MyGoogleCalendarServices.Web.Requests;
using MyGoogleCalendarServices.Web.Responses;
using System;
using System.Net;
using System.Web.Http;

namespace MyGoogleCalendarServices.Web.Controllers
{
    public class CALController : ApiController
    {
        [HttpPost]
        [Route("api/cal/UpdateEvents")]
        public IHttpActionResult UpdateEvents(UpdateEvents2Request request)
        {
            UpdateEvents2Response response = new UpdateEvents2Response();
            if (request == null)
            {
                response.SetFailed("פרמטר בקשה לא נשלח עם הבקשה");
            }
            else
            {
                response.AppId = request.AppId;
                if (ModelState.IsValid)
                {
                    try
                    {
                        CalendarManager.Instance.UpdateEvents(request, response);
                    }
                    catch (Exception ex)
                    {
                        response.SetStatus(-1, "תקלה בשירות", ex);
                    }
                }
                else
                {
                    response.SetFailed(ModelState);
                }
            }
            return Content(HttpStatusCode.OK, response, Configuration.Formatters.XmlFormatter);
        }

        [Route("api/cal/geta")]
        public string GetA()
        {
            return "A";
        }
    }
}
