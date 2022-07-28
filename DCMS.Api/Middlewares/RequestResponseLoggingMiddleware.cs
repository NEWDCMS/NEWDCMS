using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCMS.Api.Middlewares
{

    public class RequestResponseLog
    {
        public string Url { get; set; }
        public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public string Method { get; set; }
        public string RequestBody { get; set; }
        public string ResponseBody { get; set; }
        public DateTime ExcuteStartTime { get; set; }
        public DateTime ExcuteEndTime { get; set; }
        public override string ToString()
        {
            string headers = "[" + string.Join(",", Headers.Select(i => "{" + $"\"{i.Key}\":\"{i.Value}\"" + "}")) + "]";
            return $"Url: {Url},\r\nHeaders: {headers},\r\nMethod: {Method},\r\nRequestBody: {RequestBody},\r\nResponseBody: {ResponseBody},\r\nExcuteStartTime: {ExcuteStartTime.ToString("yyyy-MM-dd HH:mm:ss.fff")},\r\nExcuteStartTime: {ExcuteEndTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}";
        }
    }

    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private RequestResponseLog _logInfo;

        public RequestResponseLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            _logInfo = new RequestResponseLog();

            HttpRequest request = context.Request;
            _logInfo.Url = request.Path.ToString();
            _logInfo.Headers = request.Headers.ToDictionary(k => k.Key, v => string.Join(";", v.Value.ToList()));
            _logInfo.Method = request.Method;
            _logInfo.ExcuteStartTime = DateTime.Now;

            //获取request.Body内容
            if (request.Method.ToLower().Equals("post"))
            {
                //启用倒带功能，就可以让 Request.Body 可以再次读取
                //request.EnableRewind(); 
                Stream stream = request.Body;
                byte[] buffer = new byte[request.ContentLength.Value];
                stream.Read(buffer, 0, buffer.Length);
                _logInfo.RequestBody = Encoding.UTF8.GetString(buffer);
                request.Body.Position = 0;

            }
            else if (request.Method.ToLower().Equals("get"))
            {
                _logInfo.RequestBody = request.QueryString.Value;
            }

            //获取Response.Body内容
            var originalBodyStream = context.Response.Body;
            using (var responseBody = new MemoryStream())
            {
                context.Response.Body = responseBody;

                await _next(context);

                _logInfo.ResponseBody = await FormatResponse(context.Response);
                _logInfo.ExcuteEndTime = DateTime.Now;
                //Log4Net.LogInfo($"VisitLog: {_logInfo.ToString()}");
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }

        private async Task<string> FormatResponse(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            return text;
        }
    }

    public static class RequestResponseLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestResponseLoggingMiddleware>();
        }
    }
}
