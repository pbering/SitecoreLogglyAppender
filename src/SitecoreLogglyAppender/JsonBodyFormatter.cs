using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml;
using log4net.spi;

namespace SitecoreLogglyAppender
{
    public class JsonBodyFormatter
    {
        private readonly HttpContextBase _httpContext;
        private readonly JavaScriptSerializer _serializer;

        public JsonBodyFormatter()
        {
            _serializer = new JavaScriptSerializer();

            var context = HttpContext.Current;

            if (context != null)
            {
                _httpContext = new HttpContextWrapper(context);
            }
        }

        public string ContentType
        {
            get { return "application/json"; }
        }

        public string Create(LoggingEvent loggingEvent)
        {
            var data = new Dictionary<string, object>();

            data["level"] = loggingEvent.Level.ToString();
            data["machine"] = Environment.MachineName;
            data["message"] = loggingEvent.RenderedMessage;
            data["timestamp"] = ConvertDateTime(loggingEvent.TimeStamp);
            data["thread"] = loggingEvent.ThreadName;
            data["logger"] = loggingEvent.LoggerName;

            if (loggingEvent.LocationInformation != null)
            {
                data["file"] = loggingEvent.LocationInformation.FileName;
                data["line"] = loggingEvent.LocationInformation.LineNumber;
                data["method"] = loggingEvent.LocationInformation.MethodName;
                data["class"] = loggingEvent.LocationInformation.ClassName;
            }

            var exceptionString = loggingEvent.GetExceptionStrRep();

            if (!string.IsNullOrEmpty(exceptionString))
            {
                data["exception"] = exceptionString;
            }

            AddHttpUrl(data);
            AddHttpUser(data);
            AddHttpUrlReferrer(data);

            return _serializer.Serialize(data);
        }

        private void AddHttpUrlReferrer(Dictionary<string, object> data)
        {
            if (_httpContext == null)
            {
                return;
            }

            if (_httpContext.Request == null)
            {
                return;
            }

            if (_httpContext.Request.UrlReferrer == null)
            {
                return;
            }

            data["http_referrer"] = _httpContext.Request.UrlReferrer;
        }

        private static string ConvertDateTime(DateTime date)
        {
            return XmlConvert.ToString(date, XmlDateTimeSerializationMode.Unspecified);
        }

        private void AddHttpUser(Dictionary<string, object> data)
        {
            if (_httpContext == null)
            {
                return;
            }

            if (_httpContext.User == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(_httpContext.User.Identity.Name))
            {
                return;
            }

            data["http_user"] = _httpContext.User.Identity.Name;
        }

        private void AddHttpUrl(Dictionary<string, object> data)
        {
            if (_httpContext == null)
            {
                return;
            }

            if (_httpContext.Request == null)
            {
                return;
            }

            if (_httpContext.Request.Url == null)
            {
                return;
            }

            string url;

            try
            {
                url = new Uri(_httpContext.Request.Url, _httpContext.Request.RawUrl).ToString();
            }
            catch
            {
                url = string.Empty;
            }

            data["http_url"] = url;
        }
    }
}