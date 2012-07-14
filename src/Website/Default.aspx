<%@ Page Language="C#" %>
<%@ Import Namespace="System.Xml" %>
<%@ Import Namespace="log4net" %>
<%@ Import Namespace="log4net.Config" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        DOMConfigurator.Configure((XmlElement)ConfigurationManager.GetSection("log4net"));
        
        var logger = LogManager.GetLogger(GetType());

        logger.Info("Hello Info!");
        logger.Warn("Hello Warn!");
        logger.Error("Hello Error!");

        try
        {
            throw new Exception("WebMyExceptionMessage");
        }
        catch (Exception ex)
        {
            logger.Error("WebMyErrorMessage", ex);
        }
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
    <head runat="server">
        <title>Title</title>
    </head>
    <body>
        <form id="HtmlForm" runat="server">
            <div>
                <a href="/">Home</a>
            </div>
        </form>
    </body>
</html>