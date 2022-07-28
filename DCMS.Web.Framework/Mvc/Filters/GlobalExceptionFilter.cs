using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace DCMS.Web.Framework.Mvc.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            //var ex = context.Exception;
            context.ExceptionHandled = true;

            //通过HTTP请求头来判断是否为Ajax请求，Ajax请求的request headers里都会有一个key为x-requested-with，值“XMLHttpRequest”
            var requestData = context.HttpContext.Request.Headers.ContainsKey("x-requested-with");
            bool IsAjax = false;
            if (requestData)
            {
                IsAjax = context.HttpContext.Request.Headers["x-requested-with"] == "XMLHttpRequest" ? true : false;
            }
            if (!IsAjax)//不是异步请求则跳转页面，异步请求则返回json
            {
                context.RouteData = new Microsoft.AspNetCore.Routing.RouteData();
                context.RouteData.Values.Add("Controller", "Error");
                context.RouteData.Values.Add("Action", "Error");
                context.Result = RedirectHelper.UrlFail(context.RouteData);
            }
            else
            {
                context.Result = RedirectHelper.JsonError();
            }
        }
    }

    public class RedirectHelper
    {

        public static AjaxModel AjaxModel(int code, string msg, dynamic data = null, string url = null)
        {
            return new AjaxModel()
            {
                Code = code,
                Msg = msg,
                Data = data,
                Url = url
            };
        }
        public static ActionResult JsonError()
        {
            JsonResult json = new JsonResult(AjaxModel(500, "系统出错啦"));
            return json;
        }

        public static ActionResult UrlFail(RouteData route)
        {
            RedirectToActionResult result = new RedirectToActionResult(route.Values["action"].ToString(), route.Values["controller"].ToString(), null);
            return result;
        }

    }

    /// <summary>
    /// Json返回结构
    /// </summary>
    public class AjaxModel
    {
        public int Code { get; set; }
        public string Msg { get; set; }
        public object Data { get; set; }
        public string Url { get; set; }
    }
}
