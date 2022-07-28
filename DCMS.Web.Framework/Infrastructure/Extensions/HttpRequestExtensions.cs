using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DCMS.Web.Framework.Infrastructure.Extensions
{
    public static class HttpRequestExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static async Task<string> GetRawBodyStringAsyn(this HttpRequest httpRequest, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            using (StreamReader reader = new StreamReader(httpRequest.Body, encoding))
            {
                return await reader.ReadToEndAsync();
            }
        }
        /// <summary>
        /// 二进制
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static async Task<byte[]> GetRawBodyBinaryAsyn(this HttpRequest httpRequest, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            using (StreamReader reader = new StreamReader(httpRequest.Body, encoding))
            {
                using (var ms = new MemoryStream(2048))
                {
                    await httpRequest.Body.CopyToAsync(ms);
                    return ms.ToArray();  // returns base64 encoded string JSON result
                }
            }
        }
    }

    /// <summary>
    /// 格式化程序
    /// </summary>
    public class MyInputFormatter : IInputFormatter
    {

        public bool CanRead(InputFormatterContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("argument is Null");
            }

            var contentType = context.HttpContext.Request.ContentType;
            if (string.IsNullOrEmpty(contentType) || contentType == "text/plain" || contentType == "application/json" || contentType == "application/octet-stream")
            {
                return true;
            }

            return false;
        }

        public async Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;
            var contentType = context.HttpContext.Request.ContentType;
            if (string.IsNullOrEmpty(contentType) || contentType.ToLower() == "text/plain")
            {
                using (StreamReader reader = new StreamReader(request.Body, Encoding.UTF8))
                {
                    var content = await reader.ReadToEndAsync();
                    return await InputFormatterResult.SuccessAsync(content);
                }
            }

            if (string.IsNullOrEmpty(contentType) || contentType.ToLower() == "application/json")
            {
                var optons = new JsonSerializerSettings()
                {
                    DateFormatString = "yyyy-MM-dd HH:mm:ss",
                    NullValueHandling = NullValueHandling.Ignore
                };
                using (var reader = context.ReaderFactory(request.Body, Encoding.UTF8))
                {
                    var content = await reader.ReadToEndAsync();
                    var result = JsonConvert.DeserializeObject(content, context.ModelType, optons);
                    return await InputFormatterResult.SuccessAsync(result);
                }
            }

            if (contentType == "application/octet-stream")
            {
                using (StreamReader reader = new StreamReader(request.Body, Encoding.UTF8))
                {
                    using (var ms = new MemoryStream(2048))
                    {
                        await request.Body.CopyToAsync(ms);
                        var content = ms.ToArray();

                        return await InputFormatterResult.SuccessAsync(content);
                    }
                }
            }
            return await InputFormatterResult.FailureAsync();
        }
    }


    //public class MyOutputFormatter : IOutputFormatter
    //{
    //    public bool CanWriteResult(OutputFormatterCanWriteContext context)
    //    {
    //        return true;
    //    }

    //    public Task WriteAsync(OutputFormatterWriteContext context)
    //    {
    //        if (context == null)
    //        {
    //            throw new ArgumentNullException(nameof(context));
    //        }

    //        var response = context.HttpContext.Response;
    //        response.ContentType = "application/json";

    //        if (context.Object == null)
    //        {
    //            response.Body.WriteByte(192);
    //            return Task.CompletedTask;
    //        }

    //        using (var writer = context.WriterFactory(response.Body, Encoding.UTF8))
    //        {
    //            //var optons = new JsonSerializerSettings() { };
    //            JsonConvert.SerializeObject(context.Object, writer, response.ContentType, Formatting.None);
    //            return Task.CompletedTask;
    //        }
    //    }
    //}
}
