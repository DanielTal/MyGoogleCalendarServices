namespace MyGoogleCalendarServices.Web.Responses
{
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Web.Http.ModelBinding;

    public class ValidationError
    {
        public string Error { get; set; }
    }


    public class UpdateEvents2Response
    {
        #region Properties
        public string AppId { get; set; }
        public string Success { get; set; }
        public string StatusId { get; set; }
        //public List<string> ValidationErrors { get; set; }
        public List<ValidationError> ValidationErrors { get; set; }
        public List<EmailEntry> Results { get; set; }
        #endregion

        public UpdateEvents2Response()
        {
            //ValidationErrors = new List<string>();
            ValidationErrors = new List<ValidationError>();
            Results = new List<EmailEntry>();
            this.AppId = string.Empty;
            this.Success = "ok";
            this.StatusId = StatusCodes.noErrors;
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
        internal void SetFailed(string statusCode, string message)
        {
            SetFailed(statusCode, message, null, null);
        }
        internal void SetFailed(string statusCode, string success, ModelStateDictionary modelState = null, Exception ex = null)
        {
            Success = success;
            StatusId = statusCode;
            if (modelState == null) return;
            foreach (var item in modelState)
            {
                foreach (var errorMessage in item.Value.Errors)
                {
                    var s1 = string.Format("{0} : {1}", item.Key, errorMessage.ErrorMessage);
                    //ValidationErrors.Add(s1);
                    ValidationErrors.Add(new ValidationError { Error = s1 });
                }
            }
            if (ex != null)
            {
                System.Threading.Tasks.Task.Factory.StartNew(WriteException, ex);
            }
        }
    }
}