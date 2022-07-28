using DCMS.Web.Framework.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DCMS.Web.Controllers
{
    public partial class ErrorController : Controller
    {
        private readonly string warring = $"<!DOCTYPE html>" +
           $"<html>" +
           $"<head><title>Error</title></head><body><div align=\"center\"><br /><br />" +
           $"<a href=\"#\"><img src=\"{LayoutExtensions.ResourceServerUrl("/content/images/500.gif")}\" style=\"border:none;\" /></a>" +
           $"<br /><br />" +
           $"<p style=\"font-size: 16pt; font-weight: bold;\">We're sorry, an internal error occurred.</p>" +
           $"<p style=\"font-size: 14pt;\">Our supporting staff has been notified of this error and will address the issue shortly.<br /><br />We  apologize for the inconvenience.<br /><br />Please try clicking your browsers 'back' button or try reloading the home page.<br /><br />If you continue to receive this message, please try again in a little while." +
           $"<br /><br />" +
           $"Thank you for your patience.</p></div>" +
           $"</body>" +
           $"</html>";
        public virtual IActionResult Error()
        {
            Response.StatusCode = StatusCodes.Status500InternalServerError;
            return Content(warring, "text/html");
        }
    }
}