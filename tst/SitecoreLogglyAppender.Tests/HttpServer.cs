using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;

namespace SitecoreLogglyAppender.Tests
{
    public class HttpServer
    {
        private readonly string _listenUri;

        public HttpServer(string listenUri)
        {
            _listenUri = listenUri;
        }

        public RequestResult WaitFor(Action request, int timeout)
        {
            var result = new RequestResult();

            using (var listener = new HttpListener())
            {
                var server = listener;

                Action test = () =>
                                  {
                                      server.Prefixes.Add(_listenUri);
                                      server.Start();

                                      request();

                                      var context = server.GetContext();

                                      result.ContentType = context.Request.ContentType;
                                      result.HttpMethod = context.Request.HttpMethod;

                                      try
                                      {
                                          using (var reader = new StreamReader(context.Request.InputStream))
                                          {
                                              var body = reader.ReadToEnd();
                                              var serializer = new JavaScriptSerializer();

                                              result.Json = serializer.Deserialize<Dictionary<string, object>>(body);
                                          }

                                          context.Response.StatusCode = 200;
                                      }
                                      catch
                                      {
                                          context.Response.StatusCode = 500;
                                      }
                                      finally
                                      {
                                          context.Response.Close();
                                      }
                                  };

                result.TimedOut = Wait(test, timeout);

                listener.Stop();
                listener.Close();
            }

            return result;
        }

        private bool Wait(Action action, int timeout)
        {
            var handle = action.BeginInvoke(null, null);

            if (handle.AsyncWaitHandle.WaitOne(timeout))
            {
                action.EndInvoke(handle);

                return false;
            }

            return true;
        }

        public class RequestResult
        {
            public RequestResult()
            {
                Json = new Dictionary<string, object>();
            }

            public string ContentType { get; set; }
            public string HttpMethod { get; set; }
            public Dictionary<string, object> Json { get; set; }
            public bool TimedOut { get; set; }
        }
    }
}