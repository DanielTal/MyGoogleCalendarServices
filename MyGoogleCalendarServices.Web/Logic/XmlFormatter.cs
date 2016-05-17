namespace MyGoogleCalendarServices.Web.Logic
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Threading.Tasks;
    using System.Xml.Serialization;

    public class CustomXmlMediaTypeFormatter : XmlMediaTypeFormatter
    {
        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            try
            {
                var task = Task.Factory.StartNew(() =>
                {
                    var xns = new XmlSerializerNamespaces();
                    var serializer = new XmlSerializer(type);
                    xns.Add(string.Empty, string.Empty);
                    serializer.Serialize(writeStream, value, xns);
                });
                return task;
            }
            catch (Exception)
            {
                return base.WriteToStreamAsync(type, value, writeStream, content, transportContext);
            }
        }
    }
}