using System;
using System.Configuration;
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

        [Test]
        public void should_log_error_on_exception()
        {
            //// Arrange
            Action request = () =>
                                 {
                                     try
                                     {
                                         throw new Exception("My Exception Message");
                                     }
                                     catch (Exception e)
                                     {
                                         _logger.Error("My Error Message", e);
                                     }
                                 };

            //// Act
            var result = new HttpServer("http://localhost:42421/").WaitFor(request, 1000);

            //// Assert
            Assert.That(result.TimedOut, Is.False, "Timed out");

            foreach (var key in result.Json.Keys)
            {
                Console.WriteLine("{0}: {1}", key, result.Json[key]);
            }

            Assert.That(result.ContentType, Is.EqualTo("application/json"));
            Assert.That(result.HttpMethod, Is.EqualTo("POST"));
            Assert.That(result.Json["level"], Is.EqualTo("ERROR"));
            Assert.That(result.Json["message"], Is.EqualTo("My Error Message"));
            Assert.That(result.Json["machine"], Is.EqualTo(Environment.MachineName));
            Assert.That(result.Json["exception"], Is.StringContaining("Exception: System.Exception").And.StringContaining("Message: My Exception Message"));
        }

        [Test]
        public void should_log_information()
        {
            //// Arrange
            Action request = () => _logger.Info("My Information Message");

            //// Act
            var result = new HttpServer("http://localhost:42421/").WaitFor(request, 1000);

            //// Assert
            Assert.That(result.TimedOut, Is.False, "Timed out");

            foreach (var key in result.Json.Keys)
            {
                Console.WriteLine("{0}: {1}", key, result.Json[key]);
            }

            Assert.That(result.ContentType, Is.EqualTo("application/json"));
            Assert.That(result.HttpMethod, Is.EqualTo("POST"));
            Assert.That(result.Json["level"], Is.EqualTo("INFO"));
            Assert.That(result.Json["message"], Is.EqualTo("My Information Message"));
            Assert.That(result.Json["machine"], Is.EqualTo(Environment.MachineName));
        }
    }
}