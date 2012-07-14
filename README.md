# SitecoreLogglyAppender

A log4net [loggly](http://www.loggly.com/) appender for [Sitecore](http://www.sitecore.net).

## Installation ##

* Clone
* Place Sitecore.Logging.dll in \lib\Sitecore\
# Compile and place SitecoreLogglyAppender in the bin folder
# Merge this configuration with you default Sitecore log4net section:

```xml
<log4net>
	<appender name="LogglyAppender" type="SitecoreLogglyAppender.LogglyAppender, SitecoreLogglyAppender">
		<url value="https://logs.loggly.com/inputs/YOUR-API-KEY-HERE" />
		<threshold value="WARN" />
	</appender>
	<root>
		<level value="DEBUG" />
		<appender-ref ref="LogglyAppender" />
	</root>
</log4net>
```

Enjoy!
