using DCMS.Api.Model;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace DCMS.Api.Middlewares
{
    /// <summary>
    /// 错误/异常处理程序中间件
    /// </summary>
    public class ExceptionHandlerMiddleware
    {
        private const string JsonContentType = "application/json";
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionHandlerMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next.</param>
        public ExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Invokes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public Task Invoke(HttpContext context) => InvokeAsync(context);

        async Task InvokeAsync(HttpContext context)
        {
            try
            {
                //var request = context.Request;
                //if (request.Method.ToLower().Equals("post"))
                //{
                //    request.EnableRewind();
                //    Stream stream = request.Body;
                //    byte[] buffer = new byte[request.ContentLength.Value];
                //    stream.Read(buffer, 0, buffer.Length);
                //    var content = Encoding.UTF8.GetString(buffer);
                //    request.Body.Position = 0;
                //    var data = JsonConvert.DeserializeObject<JObject>(content);
                //}

                await _next(context);

            }
            catch (Exception exception)
            {
                var httpStatusCode = ConfigurateExceptionTypes(exception);

                // set http status code and content type
                context.Response.StatusCode = httpStatusCode;
                context.Response.ContentType = JsonContentType;

                // writes / returns error model to the response
                await context.Response.WriteAsync(
                    JsonConvert.SerializeObject(new ErrorModelViewModel
                    {
                        Message = exception.Message
                    }));

                // This is throwing readonly exception
                //context.Response.Headers.Clear();
            }
        }

        /// <summary>
        /// Configurates/maps exception to the proper HTTP error Type
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        private static int ConfigurateExceptionTypes(Exception exception)
        {
            int httpStatusCode;

            // Exception type To Http Status configuration 
            switch (exception)
            {
                case var _ when exception is ValidationException:
                    httpStatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                default:
                    httpStatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            return httpStatusCode;
        }
    }


    public static class ExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlerMiddleware>();
        }
    }
}
