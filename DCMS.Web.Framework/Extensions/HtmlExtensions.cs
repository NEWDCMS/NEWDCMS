using DCMS.Core.Infrastructure;
using DCMS.Services.Stores;
using DCMS.Web.Framework.Events;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Encodings.Web;


namespace DCMS.Web.Framework.Extensions
{
    /// <summary>
    /// HTML 扩展
    /// </summary>
    public static class HtmlExtensions
    {
        #region 管理域扩展

        /// <summary>
        /// 为实体生成本地化编辑器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TLocalizedModelLocal"></typeparam>
        /// <param name="helper"></param>
        /// <param name="name"></param>
        /// <param name="localizedTemplate"></param>
        /// <param name="standardTemplate"></param>
        /// <param name="ignoreIfSeveralStores"></param>
        /// <param name="cssClass"></param>
        /// <returns></returns>
        public static IHtmlContent LocalizedEditor<T, TLocalizedModelLocal>(this IHtmlHelper<T> helper,
            string name,
            Func<int, HelperResult> localizedTemplate,
            Func<T, HelperResult> standardTemplate,
            bool ignoreIfSeveralStores = false, string cssClass = "")
            where T : ILocalizedModel<TLocalizedModelLocal>
            where TLocalizedModelLocal : ILocalizedLocaleModel
        {
            var localizationSupported = helper.ViewData.Model.Locales.Count > 1;
            if (ignoreIfSeveralStores)
            {
                var storeService = EngineContext.Current.Resolve<IStoreService>();
                if (storeService.GetAllStores(true).Count >= 2)
                {
                    localizationSupported = false;
                }
            }
            if (localizationSupported)
            {
                var tabStrip = new StringBuilder();
                var cssClassWithSpace = !string.IsNullOrEmpty(cssClass) ? " " + cssClass : null;
                tabStrip.AppendLine($"<div id=\"{name}\" class=\"nav-tabs-custom nav-tabs-localized-fields{cssClassWithSpace}\">");

                //render input contains selected tab name
                var tabNameToSelect = GetSelectedTabName(helper, name);
                var selectedTabInput = new TagBuilder("input");
                selectedTabInput.Attributes.Add("type", "hidden");
                selectedTabInput.Attributes.Add("id", $"selected-tab-name-{name}");
                selectedTabInput.Attributes.Add("name", $"selected-tab-name-{name}");
                selectedTabInput.Attributes.Add("value", tabNameToSelect);
                tabStrip.AppendLine(selectedTabInput.RenderHtmlContent());

                tabStrip.AppendLine("<ul class=\"nav nav-tabs\">");

                //default tab
                var standardTabName = $"{name}-standard-tab";
                var standardTabSelected = string.IsNullOrEmpty(tabNameToSelect) || standardTabName == tabNameToSelect;
                tabStrip.AppendLine(string.Format("<li{0}>", standardTabSelected ? " class=\"active\"" : null));
                tabStrip.AppendLine($"<a data-tab-name=\"{standardTabName}\" href=\"#{standardTabName}\" data-toggle=\"tab\">{standardTabName}</a>");
                tabStrip.AppendLine("</li>");


                var urlHelper = EngineContext.Current.Resolve<IUrlHelperFactory>().GetUrlHelper(helper.ViewContext);

                foreach (var locale in helper.ViewData.Model.Locales)
                {
                    var localizedTabName = $"{name}-zh-tab";
                    tabStrip.AppendLine(string.Format("<li{0}>", localizedTabName == tabNameToSelect ? " class=\"active\"" : null));

                    tabStrip.AppendLine($"<a data-tab-name=\"{localizedTabName}\" href=\"#{localizedTabName}\" data-toggle=\"tab\">{localizedTabName}</a>");

                    tabStrip.AppendLine("</li>");
                }
                tabStrip.AppendLine("</ul>");

                //default tab
                tabStrip.AppendLine("<div class=\"tab-content\">");
                tabStrip.AppendLine(string.Format("<div class=\"tab-pane{0}\" id=\"{1}\">", standardTabSelected ? " active" : null, standardTabName));
                tabStrip.AppendLine(standardTemplate(helper.ViewData.Model).ToHtmlString());
                tabStrip.AppendLine("</div>");

                for (var i = 0; i < helper.ViewData.Model.Locales.Count; i++)
                {
                    //languages

                    var localizedTabName = $"{name}-zh-tab";
                    tabStrip.AppendLine(string.Format("<div class=\"tab-pane{0}\" id=\"{1}\">", localizedTabName == tabNameToSelect ? " active" : null, localizedTabName));
                    tabStrip.AppendLine(localizedTemplate(i).ToHtmlString());
                    tabStrip.AppendLine("</div>");
                }
                tabStrip.AppendLine("</div>");
                tabStrip.AppendLine("</div>");

                //render tabs script
                var script = new TagBuilder("script");
                script.InnerHtml.AppendHtml("$(document).ready(function () {bindBootstrapTabSelectEvent('" + name + "', 'selected-tab-name-" + name + "');});");
                tabStrip.AppendLine(script.RenderHtmlContent());

                return new HtmlString(tabStrip.ToString());
            }
            else
            {
                return new HtmlString(standardTemplate(helper.ViewData.Model).RenderHtmlContent());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="helper"></param>
        /// <returns></returns>
        public static string GetSelectedPanelName(this IHtmlHelper helper)
        {
            //keep this method synchronized with
            //"SaveSelectedPanelName" method of \Area\Admin\Controllers\BaseAdminController.cs
            var tabName = string.Empty;
            const string dataKey = "dcms.selected-panel-name";

            if (helper.ViewData.ContainsKey(dataKey))
            {
                tabName = helper.ViewData[dataKey].ToString();
            }

            if (helper.ViewContext.TempData.ContainsKey(dataKey))
            {
                tabName = helper.ViewContext.TempData[dataKey].ToString();
            }

            return tabName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="dataKeyPrefix"></param>
        /// <returns></returns>
        public static string GetSelectedTabName(this IHtmlHelper helper, string dataKeyPrefix = null)
        {
            //keep this method synchronized with
            //"SaveSelectedTab" method of \Area\Admin\Controllers\BaseAdminController.cs
            var tabName = string.Empty;
            var dataKey = "dcms.selected-tab-name";
            if (!string.IsNullOrEmpty(dataKeyPrefix))
            {
                dataKey += $"-{dataKeyPrefix}";
            }

            if (helper.ViewData.ContainsKey(dataKey))
            {
                tabName = helper.ViewData[dataKey].ToString();
            }

            if (helper.ViewContext.TempData.ContainsKey(dataKey))
            {
                tabName = helper.ViewContext.TempData[dataKey].ToString();
            }

            return tabName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventMessage"></param>
        /// <param name="tabId"></param>
        /// <param name="tabName"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static IHtmlContent TabContentByURL(this AdminTabStripCreated eventMessage, string tabId, string tabName, string url)
        {
            return new HtmlString($@"
                <script>
                    $(document).ready(function() {{
                        $('<li><a data-tab-name='{tabId}' data-toggle='tab' href='#{tabId}'>{tabName}</a></li>').appendTo('#{eventMessage.TabStripName} .nav-tabs:first');
                        $.get('{url}', function(result) {{
                            $(`<div class='tab-pane' id='{tabId}'>` + result + `</div>`).appendTo('#{eventMessage.TabStripName} .tab-content:first');
                        }});
                    }});
                </script>");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventMessage"></param>
        /// <param name="tabId"></param>
        /// <param name="tabName"></param>
        /// <param name="contentModel"></param>
        /// <returns></returns>
        public static IHtmlContent TabContentByModel(this AdminTabStripCreated eventMessage, string tabId, string tabName, string contentModel)
        {
            return new HtmlString($@"
                <script>
                    $(document).ready(function() {{
                        $(`<li><a data-tab-name='{tabId}' data-toggle='tab' href='#{tabId}'>{tabName}</a></li>`).appendTo('#{eventMessage.TabStripName} .nav-tabs:first');
                        $(`<div class='tab-pane' id='{tabId}'>{contentModel}</div>`).appendTo('#{eventMessage.TabStripName} .tab-content:first');
                    }});
                </script>");
        }

        #region 表单字段

        /// <summary>
        /// 生成Hint控件
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IHtmlContent Hint(this IHtmlHelper helper, string value)
        {
            //create tag builder
            var builder = new TagBuilder("div");
            builder.MergeAttribute("title", value);
            builder.MergeAttribute("class", "ico-help");
            builder.MergeAttribute("data-toggle", "tooltip");
            var icon = new StringBuilder();
            icon.Append("<i class='fa fa-question-circle'></i>");
            builder.InnerHtml.AppendHtml(icon.ToString());
            //render tag
            return new HtmlString(builder.ToHtmlString());
        }

        #endregion

        #endregion

        #region 公共扩展

        /// <summary>
        /// 
        /// </summary>
        /// <param name="htmlContent"></param>
        /// <returns></returns>
        public static string RenderHtmlContent(this IHtmlContent htmlContent)
        {
            using var writer = new StringWriter();
            htmlContent.WriteTo(writer, HtmlEncoder.Default);
            var htmlOutput = writer.ToString();
            return htmlOutput;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static string ToHtmlString(this IHtmlContent tag)
        {
            using var writer = new StringWriter();
            tag.WriteTo(writer, HtmlEncoder.Default);
            return writer.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="helper"></param>
        /// <param name="expression"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static IHtmlContent CustomLabelFor<TModel, TProperty>(this IHtmlHelper<TModel> helper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes)
        {
            string result;
            TagBuilder div = new TagBuilder("div");
            div.MergeAttribute("class", "form-group");
            var label = helper.LabelFor(expression, new { @class = "control-label col-lg-1" });
            div.InnerHtml.AppendHtml(label);
            using (var sw = new System.IO.StringWriter())
            {
                div.WriteTo(sw, System.Text.Encodings.Web.HtmlEncoder.Default);
                result = sw.ToString();
            }
            return new HtmlString(result);
        }


        /// <summary>
        /// 自定义验证
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="helper"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static IHtmlContent DCMSValidationMessageFor<TModel, TProperty>(this IHtmlHelper<TModel> helper, Expression<Func<TModel, TProperty>> expression)
        {
            TagBuilder containerDivBuilder = new TagBuilder("div");
            containerDivBuilder.AddCssClass("alert alert-border-left alert-danger light alert-micro field-validation-alert hide");

            TagBuilder topDivBuilder = new TagBuilder("button")
            {
                TagRenderMode = TagRenderMode.Normal
            };
            topDivBuilder.AddCssClass("close");
            topDivBuilder.MergeAttribute("type", "button");
            topDivBuilder.MergeAttribute("data-dismiss", "data-dismiss");
            topDivBuilder.MergeAttribute("aria-hidden", "true");

            TagBuilder midDivBuilder = new TagBuilder("i");
            midDivBuilder.AddCssClass("fa fa-warning pr10");
            midDivBuilder.TagRenderMode = TagRenderMode.Normal;

            TagBuilder wellDivBuilder = new TagBuilder("strong");
            wellDivBuilder.InnerHtml.SetContent("提示! ");

            //containerDivBuilder.InnerHtml.AppendHtml(topDivBuilder.ToString());
            //containerDivBuilder.InnerHtml.AppendHtml(midDivBuilder.ToString());
            containerDivBuilder.InnerHtml.AppendHtml(topDivBuilder.ToHtmlString());
            containerDivBuilder.InnerHtml.AppendHtml(midDivBuilder.ToHtmlString());

            containerDivBuilder.InnerHtml.AppendHtml(helper.ValidationMessageFor(expression));
            containerDivBuilder.TagRenderMode = TagRenderMode.Normal;

            //return MvcHtmlString.Create(containerDivBuilder.ToString(TagRenderMode.Normal));

            string result;
            using (var sw = new System.IO.StringWriter())
            {
                containerDivBuilder.WriteTo(sw, System.Text.Encodings.Web.HtmlEncoder.Default);
                result = sw.ToString();
            }
            return new HtmlString(result);
        }


        /// <summary>
        /// 自定义DropDownListFor
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="helper"></param>
        /// <param name="expression"></param>
        /// <param name="defaultText"></param>
        /// <param name="trueText"></param>
        /// <param name="falseText"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static IHtmlContent DCMSBoolDropDownListFor<TModel, TProperty>(this IHtmlHelper<TModel> helper, Expression<Func<TModel, TProperty>> expression, string defaultText = "", string trueText = "", string falseText = "", object htmlAttributes = null)
        {
            List<SelectListItem> items = new List<SelectListItem>
            {
                new SelectListItem() { Text = defaultText, Value = (null as bool?).ToString(), Selected = true },
                new SelectListItem() { Text = trueText, Value = "true" },
                new SelectListItem() { Text = falseText, Value = "false" }
            };

            return helper.DropDownListFor(expression, new SelectList(items, "Value", "Text"), htmlAttributes);
        }
        #endregion
    }

    public static class ExHtmlHelper
    {
        /// <summary>
        /// 转描述
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="selectListItem"></param>
        /// <returns></returns>
        public static IEnumerable<SelectListItem> ToDescription<TModel>(this IEnumerable<SelectListItem> selectListItem) where TModel : struct
        {
            return selectListItem.Select(s =>
              {
                  s.Text = DCMS.Core.CommonHelper.GetEnumDescription<TModel>(int.Parse(s.Value));
                  return s;
              });
        }

        /// <summary>
        /// 转描述
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="selectListItem"></param>
        /// <returns></returns>
        public static SelectList ToSelectListDescription<TModel>(this IEnumerable<SelectListItem> selectListItem) where TModel : struct
        {
            return new SelectList(selectListItem.Select(s =>
            {
                s.Text = DCMS.Core.CommonHelper.GetEnumDescription<TModel>(int.Parse(s.Value));
                return s;
            }).ToList(), "Value", "Text");
        }
    }

    public static class UrlHelperExtensions
    {
        /// <summary>
        /// 使用指定的操作名称、控制器名称和路由值生成操作方法的完全限定URL
        /// </summary>
        /// <param name="url"></param>
        /// <param name="actionName"></param>
        /// <param name="controllerName"></param>
        /// <param name="routeValues"></param>
        /// <returns></returns>
        public static string AbsoluteAction(
            this IUrlHelper url,
            string actionName,
            string controllerName,
            object routeValues = null)
        {
            return url.Action(actionName, controllerName, routeValues, url.ActionContext.HttpContext.Request.Scheme);
        }

        /// <summary>
        /// 使用指定的内容路径生成指定内容的完全限定URL。将虚拟（相对）路径转换为应用程序绝对路径
        /// </summary>
        /// <param name="url"></param>
        /// <param name="contentPath"></param>
        /// <returns></returns>
        public static string AbsoluteContent(
            this IUrlHelper url,
            string contentPath)
        {
            var request = url.ActionContext.HttpContext.Request;
            return new Uri(new Uri(request.Scheme + "://" + request.Host.Value), url.Content(contentPath)).ToString();
        }

        /// <summary>
        /// 使用路由名称和路由值生成指定路由的完全限定URL
        /// </summary>
        /// <param name="url"></param>
        /// <param name="routeName"></param>
        /// <param name="routeValues"></param>
        /// <returns></returns>
        public static string AbsoluteRouteUrl(
            this IUrlHelper url,
            string routeName,
            object routeValues = null)
        {
            return url.RouteUrl(routeName, routeValues, url.ActionContext.HttpContext.Request.Scheme);
        }

        public static string GetUrl(this HttpRequest request)
        {
            return request.GetDisplayUrl().Replace("http://", "").Replace("/", "_");
        }

        public static string GetAbsoluteUri(this HttpRequest request)
        {
            return new StringBuilder()
                .Append(request.Scheme)
                .Append("://")
                .Append(request.Host)
                .Append(request.PathBase)
                .Append(request.Path)
                .Append(request.QueryString)
                .ToString();
        }

    }
}