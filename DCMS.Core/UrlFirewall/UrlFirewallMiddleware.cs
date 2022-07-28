using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace DCMS.Core.UrlFirewall
{
    public class UrlFirewallMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IUrlFirewallValidator _validator;
        private readonly ILogger<UrlFirewallMiddleware> _logger;

        private readonly string warring = $"<!DOCTYPE html>" +
            $"<html>" +
            $"<head><title>Error</title></head><body><div align=\"center\"><br /><br />" +
            $"<a href=\"#\"><img src=\"http://resources.jsdcms.com/content/images/500.gif\" style=\"border:none;\" /></a>" +
            $"<br /><br />" +
            $"<p style=\"font-size: 16pt; font-weight: bold;\">We're sorry, an internal error occurred.</p>" +
            $"<p style=\"font-size: 14pt;\">Our supporting staff has been notified of this error and will address the issue shortly.<br /><br />We  apologize for the inconvenience.<br /><br />Please try clicking your browsers 'back' button or try reloading the home page.<br /><br />If you continue to receive this message, please try again in a little while." +
            $"<br /><br />" +
            $"Thank you for your patience.</p></div>" +
            $"</body>" +
            $"</html>";

        public UrlFirewallMiddleware(RequestDelegate next,
            IUrlFirewallValidator validator,
            ILogger<UrlFirewallMiddleware> logger)
        {
            _next = next;
            _validator = validator;
            _logger = logger;
        }

        public Task Invoke(HttpContext context)
        {
            string path = context.Request.Path.ToString().ToLower();
            string method = context.Request.Method.ToLower();
            if (!_validator.ValidateUrl(path, method))
            {
                _logger.LogInformation($"The path {path} invalid.");
                using (StreamWriter sw = new StreamWriter(context.Response.Body))
                {
                    sw.Write(warring);
                }
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                return Task.CompletedTask; ;
            }
            string ip = UrlFirewallMiddlewareExtensions.GetClientIp(context);
            if (!_validator.ValidateIp(ip, method))
            {
                _logger.LogInformation($"Warning, your IP {ip} is blocked by the administrator.");
                using (StreamWriter sw = new StreamWriter(context.Response.Body))
                {
                    sw.Write(warring);
                }
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                return Task.CompletedTask; ;
            }
            return _next(context);
        }
    }
}