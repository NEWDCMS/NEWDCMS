using DCMS.Core;
using DCMS.Core.Infrastructure;
using DCMS.Web.Framework.Models;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Framework.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Net;
using System.Text.Encodings.Web;

namespace DCMS.Web.Framework.Controllers
{
    /// <summary>
    /// 控制器基类
    /// </summary>
    [PublishModelEvents]
    //[SignOutFromExternalAuthentication]
    [ValidatePassword]
    //[SaveIpAddress]
    //[SaveLastActivity]
    //[SaveLastVisitedPage]
    public abstract class BaseController : Controller
    {
        #region 视图呈现

        /// <summary>
        /// 将组件呈现为字符串
        /// </summary>
        /// <param name="componentName">Component name</param>
        /// <param name="arguments">Arguments</param>
        /// <returns>Result</returns>
        protected virtual string RenderViewComponentToString(string componentName, object arguments = null)
        {
            //original implementation: https://github.com/aspnet/Mvc/blob/dev/src/Microsoft.AspNetCore.Mvc.ViewFeatures/Internal/ViewComponentResultExecutor.cs
            //we customized it to allow running from controllers

            //TODO add support for parameters (pass ViewComponent as input parameter)
            if (string.IsNullOrEmpty(componentName))
            {
                throw new ArgumentNullException(nameof(componentName));
            }

            var actionContextAccessor = HttpContext.RequestServices.GetService(typeof(IActionContextAccessor)) as IActionContextAccessor;
            if (actionContextAccessor == null)
            {
                throw new Exception("IActionContextAccessor cannot be resolved");
            }

            var context = actionContextAccessor.ActionContext;

            var viewComponentResult = ViewComponent(componentName, arguments);

            var viewData = ViewData;
            if (viewData == null)
            {
                throw new NotImplementedException();
                //TODO viewData = new ViewDataDictionary(_modelMetadataProvider, context.ModelState);
            }

            var tempData = TempData;
            if (tempData == null)
            {
                throw new NotImplementedException();
                //TODO tempData = _tempDataDictionaryFactory.GetTempData(context.HttpContext);
            }

            using (var writer = new StringWriter())
            {
                var viewContext = new ViewContext(
                    context,
                    NullView.Instance,
                    viewData,
                    tempData,
                    writer,
                    new HtmlHelperOptions());

                // IViewComponentHelper is stateful, we want to make sure to retrieve it every time we need it.
                var viewComponentHelper = context.HttpContext.RequestServices.GetRequiredService<IViewComponentHelper>();
                (viewComponentHelper as IViewContextAware)?.Contextualize(viewContext);

                var result = viewComponentResult.ViewComponentType == null ?
                    viewComponentHelper.InvokeAsync(viewComponentResult.ViewComponentName, viewComponentResult.Arguments) :
                    viewComponentHelper.InvokeAsync(viewComponentResult.ViewComponentType, viewComponentResult.Arguments);

                result.Result.WriteTo(writer, HtmlEncoder.Default);
                return writer.ToString();
            }
        }

        /// <summary>
        /// 呈现部分视图为字符串
        /// </summary>
        /// <returns>Result</returns>
        protected virtual string RenderPartialViewToString()
        {
            return RenderPartialViewToString(null, null);
        }

        /// <summary>
        /// 呈现部分视图为字符串
        /// </summary>
        /// <param name="viewName">View name</param>
        /// <returns>Result</returns>
        protected virtual string RenderPartialViewToString(string viewName)
        {
            return RenderPartialViewToString(viewName, null);
        }

        /// <summary>
        /// 呈现部分视图为字符串
        /// </summary>
        /// <param name="model">Model</param>
        /// <returns>Result</returns>
        protected virtual string RenderPartialViewToString(object model)
        {
            return RenderPartialViewToString(null, model);
        }

        /// <summary>
        /// 呈现部分视图为字符串
        /// </summary>
        /// <param name="viewName">View name</param>
        /// <param name="model">Model</param>
        /// <returns>Result</returns>
        protected virtual string RenderPartialViewToString(string viewName, object model)
        {
            //get Razor view engine
            var razorViewEngine = EngineContext.Current.Resolve<IRazorViewEngine>();

            //create action context
            var actionContext = new ActionContext(HttpContext, RouteData, ControllerContext.ActionDescriptor, ModelState);

            //set view name as action name in case if not passed
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = ControllerContext.ActionDescriptor.ActionName;
            }

            //set model
            ViewData.Model = model;

            //try to get a view by the name
            var viewResult = razorViewEngine.FindView(actionContext, viewName, false);
            if (viewResult.View == null)
            {
                //or try to get a view by the path
                viewResult = razorViewEngine.GetView(null, viewName, false);
                if (viewResult.View == null)
                {
                    throw new ArgumentNullException($"{viewName} view was not found");
                }
            }
            using (var stringWriter = new StringWriter())
            {
                var viewContext = new ViewContext(actionContext, viewResult.View, ViewData, TempData, stringWriter, new HtmlHelperOptions());

                var t = viewResult.View.RenderAsync(viewContext);
                t.Wait();
                return stringWriter.GetStringBuilder().ToString();
            }
        }

        #endregion

        #region 通知

        /// <summary>
        /// 错误Json
        /// </summary>
        /// <param name="error">Error text</param>
        /// <returns>Error's JSON data</returns>
        protected JsonResult ErrorJson(string error)
        {
            return Json(new
            {
                error = error
            });
        }

        /// <summary>
        /// 错误Json
        /// </summary>
        /// <param name="errors">Error messages</param>
        /// <returns>Error's JSON data</returns>
        protected JsonResult ErrorJson(object errors)
        {
            return Json(new
            {
                error = errors
            });
        }

        protected virtual void DisplayEditLink(string editPageUrl)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();

            pageHeadBuilder.AddEditPageUrl(editPageUrl);
        }

        #endregion

        #region 安全

        /// <summary>
        /// 拒绝访问视图
        /// </summary>
        /// <returns>Access denied view</returns>
        protected virtual IActionResult AccessDeniedView()
        {
            var webHelper = EngineContext.Current.Resolve<IWebHelper>();
            return RedirectToAction("AccessDenied", "Security", new { pageUrl = webHelper.GetRawUrl(Request) });
        }

        /// <summary>
        /// 拒绝访问数据表的json数据
        /// </summary>
        /// <returns>Access denied JSON data</returns>
        protected JsonResult AccessDeniedDataTablesJson()
        {
            return ErrorJson("Admin.AccessDenied.Description");
        }

        #endregion

        #region 面板

        /// <summary>
        /// 保存所选面板名称
        /// </summary>
        /// <param name="panelName">Panel name to save</param>
        /// <param name="persistForTheNextRequest">A value indicating whether a message should be persisted for the next request. Pass null to ignore</param>
        public virtual void SaveSelectedPanelName(string tabName, bool persistForTheNextRequest = true)
        {
            //keep this method synchronized with
            //"GetSelectedPanelName" method of \DCMS.Web.Framework\Extensions\HtmlExtensions.cs
            if (string.IsNullOrEmpty(tabName))
            {
                throw new ArgumentNullException(nameof(tabName));
            }

            const string dataKey = "dcms.selected-panel-name";
            if (persistForTheNextRequest)
            {
                TempData[dataKey] = tabName;
            }
            else
            {
                ViewData[dataKey] = tabName;
            }
        }

        /// <summary>
        /// 保存所选面板名称
        /// </summary>
        /// <param name="tabName">Tab name to save; empty to automatically detect it</param>
        /// <param name="persistForTheNextRequest">A value indicating whether a message should be persisted for the next request. Pass null to ignore</param>
        public virtual void SaveSelectedTabName(string tabName = "", bool persistForTheNextRequest = true)
        {
            //default root tab
            SaveSelectedTabName(tabName, "selected-tab-name", null, persistForTheNextRequest);
            //child tabs (usually used for localization)
            //Form is available for POST only
            if (!Request.Method.Equals(WebRequestMethods.Http.Post, StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            foreach (var key in Request.Form.Keys)
            {
                if (key.StartsWith("selected-tab-name-", StringComparison.InvariantCultureIgnoreCase))
                {
                    SaveSelectedTabName(null, key, key.Substring("selected-tab-name-".Length), persistForTheNextRequest);
                }
            }
        }

        /// <summary>
        /// 保存所选面板名称
        /// </summary>
        /// <param name="tabName">Tab name to save; empty to automatically detect it</param>
        /// <param name="persistForTheNextRequest">A value indicating whether a message should be persisted for the next request. Pass null to ignore</param>
        /// <param name="formKey">Form key where selected tab name is stored</param>
        /// <param name="dataKeyPrefix">A prefix for child tab to process</param>
        protected virtual void SaveSelectedTabName(string tabName, string formKey, string dataKeyPrefix, bool persistForTheNextRequest)
        {
            //keep this method synchronized with
            //"GetSelectedTabName" method of \DCMS.Web.Framework\Extensions\HtmlExtensions.cs
            if (string.IsNullOrEmpty(tabName))
            {
                tabName = Request.Form[formKey];
            }

            if (string.IsNullOrEmpty(tabName))
            {
                return;
            }

            var dataKey = "dcms.selected-tab-name";
            if (!string.IsNullOrEmpty(dataKeyPrefix))
            {
                dataKey += $"-{dataKeyPrefix}";
            }

            if (persistForTheNextRequest)
            {
                TempData[dataKey] = tabName;
            }
            else
            {
                ViewData[dataKey] = tabName;
            }
        }

        #endregion

        #region 数据表

        /// <summary>
        /// 创建将指定对象序列化为json的对象，该json用于序列化数据表的数据
        /// </summary>
        /// <typeparam name="T">Model type</typeparam>
        /// <param name="model">The model to serialize.</param>
        /// <returns>The created object that serializes the specified data to JSON format for the response.</returns>
        public JsonResult Json<T>(BasePagedListModel<T> model) where T : BaseModel
        {
            return Json(new
            {
                draw = model.Draw,
                recordsTotal = model.RecordsTotal,
                recordsFiltered = model.RecordsFiltered,
                data = model.Data,

                //TODO: remove after moving to DataTables grids
                Total = model.Total,
                Data = model.Data
            });
        }

        #endregion

        protected virtual JsonResult BillChecking<T, Items>(T bill, BillStates operation, string authCode = "") where T : DCMS.Core.BaseBill<Items> where Items : BaseEntity
        {
            return new JsonResult(null);
        }
    }
}