namespace MyGoogleCalendarServices.Web.Responses
{
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Web.Http.ModelBinding;

    public class UpdateEvents2Response
    {
        public string Status { get; set; }
        public int StatusId { get; set; }
        public List<string> ValidationErrors { get; set; }
        public string GoogleId { get; set; }
        public string AppId { get; set; }
        public void SetStatus(int StatusId, string StatusMessage, Exception ex = null)
        {
            this.Status = StatusMessage;
            this.StatusId = StatusId;
            if (ex != null)
            {
                this.Status += Environment.NewLine + ex.Message;
                System.Threading.Tasks.Task.Factory.StartNew(WriteException, ex);
            }
        }
        public List<EmailEntry> Results { get; set; }
        public UpdateEvents2Response()
        {
            ValidationErrors = new List<string>();
            Results = new List<EmailEntry>();
            this.AppId = string.Empty;
            this.GoogleId = string.Empty;
            this.Status = "אותחל";
            this.StatusId = 0;
        }
        private void WriteException(object ex)
        {
            string logFileName = @"C:\inetpub\logs\GoogleCalendarServices02.WebService01.log";
            try
            {
                if (System.IO.File.Exists(logFileName))
                {
                    var x1 = new System.IO.FileInfo(logFileName);
                    if (x1.Length > 1)
                    {
                        System.IO.File.Delete(logFileName);
                    }
                }
            }
            catch
            {

            }
            try
            {
                System.IO.File.WriteAllText(logFileName, (ex as Exception).ToString());
            }
            catch
            {

            }
        }
        internal void SetFailed(string message)
        {
            Status = message;
            StatusId = -2;
        }
        internal void SetFailed(ModelStateDictionary modelState)
        {
            if (modelState == null) return;
            Status = "שגיאות וולידציה";
            StatusId = -2;
            foreach (var item in modelState)
            {
                foreach (var errorMessage in item.Value.Errors)
                {
                    ValidationErrors.Add(string.Format("{0} : {1}", item.Key, errorMessage.ErrorMessage));
                }
            }
        }
    }
}