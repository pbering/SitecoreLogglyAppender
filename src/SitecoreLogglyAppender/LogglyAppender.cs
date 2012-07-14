using System;
using System.Net;
using System.Text;
using log4net.Appender;
using log4net.spi;

namespace SitecoreLogglyAppender
{
    public class LogglyAppender : AppenderSkeleton
    {
        private JsonBodyFormatter BodyFormatter
        {
            get { return new JsonBodyFormatter(); }
        }

        public string Url { get; set; }

        protected override void Append(LoggingEvent loggingEvent)
        {
            byte[] bodyBytes;

            try
            {
                var bodyString = BodyFormatter.Create(loggingEvent);

                bodyBytes = Encoding.UTF8.GetBytes(bodyString);
            }
            catch (Exception e)
            {
                ErrorHandler.Error("Failed to creating JSON", e);

                return;
            }
            
            var request = BuildRequest();

            request.BeginGetRequestStream(r =>
                                              {
                                                  try
                                                  {
                                                      var stream = request.EndGetRequestStream(r);

                                                      stream.BeginWrite(bodyBytes, 0, bodyBytes.Length, c =>
                                                                                                            {
                                                                                                                try
                                                                                                                {
                                                                                                                    stream.Dispose();

                                                                                                                    request.BeginGetResponse(a =>
                                                                                                                                                 {
                                                                                                                                                     try
                                                                                                                                                     {
                                                                                                                                                         request.EndGetResponse(a);
                                                                                                                                                     }
                                                                                                                                                     catch (Exception e)
                                                                                                                                                     {
                                                                                                                                                         ErrorHandler.Error(
                                                                                                                                                             "Failed to get response",
                                                                                                                                                             e);
                                                                                                                                                     }
                                                                                                                                                 }, null);
                                                                                                                    stream.EndWrite(c);
                                                                                                                }
                                                                                                                catch (Exception e)
                                                                                                                {
                                                                                                                    ErrorHandler.Error("Failed to write", e);
                                                                                                                }
                                                                                                            }, null);
                                                  }
                                                  catch (Exception e)
                                                  {
                                                      ErrorHandler.Error("Failed to connect", e);
                                                  }
                                              }, null);
        }

        private HttpWebRequest BuildRequest()
        {
            var request = (HttpWebRequest)WebRequest.Create(Url);

            request.Method = "POST";
            request.ContentType = BodyFormatter.ContentType;
            
            return request;
        }
    }
}