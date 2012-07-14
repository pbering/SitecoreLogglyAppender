using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;
using System.Xml;
using NUnit.Framework;
using log4net;
using log4net.Config;

namespace SitecoreLogglyAppender.Tests
{
    [TestFixture]
    public class LogglyAppenderTest
    {
        private ILog _logger;

        [TestFixtureSetUp]
        public void Setup()
        {
            DOMConfigurator.Configure((XmlElement)ConfigurationManager.GetSection("log4net"));

            _logger = LogManager.GetLogger(typeof(LogglyAppenderTest));
        }

        private void Run(Action action, int timeout)
        {
            var handle = action.BeginInvoke(null, null);

            if (handle.AsyncWaitHandle.WaitOne(timeout))
            {
                action.EndInvoke(handle);

                return;
            }

            Assert.Fail("Failed to complete in the timeout specified.");
        }

        [Test]
        public void should_log_error_on_exception()
        {
            //// Arrange
            Action test = () =>
                              {
                                  using (var listener = new HttpListener())
                                  {
                                      listener.Prefixes.Add("http://localhost:42421/");
                                      listener.Start();

                                      try
                                      {
                                          throw new Exception("MyExceptionMessage");
                                      }
                                      catch (Exception e)
                                      {
                                          //// Act
                                          _logger.Error("MyErrorMessage", e);
                                      }

                                      listener.IgnoreWriteExceptions = true;

                                      var context = listener.GetContext();

                                      using (var reader = new StreamReader(context.Request.InputStream))
                                      {
                                          var body = reader.ReadToEnd();
                                          var serializer = new JavaScriptSerializer();
                                          var dictionary = serializer.Deserialize<Dictionary<string, object>>(body);

                                          foreach (var key in dictionary.Keys)
                                          {
                                              Console.WriteLine("{0}: {1}", key, dictionary[key]);
                                          }

                                          //// Assert
                                          Assert.That(context.Request.ContentType, Is.EqualTo("application/json"));
                                          Assert.That(dictionary["level"], Is.EqualTo("ERROR"));
                                          Assert.That(dictionary["message"], Is.EqualTo("MyErrorMessage"));
                                          Assert.That(dictionary["machine"], Is.EqualTo(Environment.MachineName));
                                          Assert.That(dictionary["exception"], Is.StringContaining("MyExceptionMessage"));
                                      }

                                      context.Response.StatusCode = 200;
                                      context.Response.Close();
                                  }
                              };

            Run(test, 1000);
        }

        [Test]
        public void should_log_information()
        {
            //// Arrange
            Action test = () =>
                              {
                                  using (var listener = new HttpListener())
                                  {
                                      listener.Prefixes.Add("http://localhost:42421/");
                                      listener.Start();

                                      //// Act
                                      _logger.Info("MyInformationMessage");

                                      var context = listener.GetContext();

                                      using (var reader = new StreamReader(context.Request.InputStream))
                                      {
                                          var body = reader.ReadToEnd();
                                          var serializer = new JavaScriptSerializer();
                                          var dictionary = serializer.Deserialize<Dictionary<string, object>>(body);

                                          foreach (var key in dictionary.Keys)
                                          {
                                              Console.WriteLine("{0}: {1}", key, dictionary[key]);
                                          }

                                          //// Assert
                                          Assert.That(context.Request.ContentType, Is.EqualTo("application/json"));
                                          Assert.That(dictionary["level"], Is.EqualTo("INFO"));
                                          Assert.That(dictionary["message"], Is.EqualTo("MyInformationMessage"));
                                          Assert.That(dictionary["machine"], Is.EqualTo(Environment.MachineName));
                                      }

                                      context.Response.StatusCode = 200;
                                      context.Response.Close();
                                  }
                              };

            Run(test, 1000);
        }
    }
}