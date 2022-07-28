using DCMS.Core;
using DCMS.Core.Infrastructure;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using System.Linq;
namespace DCMS.Web.Framework.UI
{
    /// <summary>
    /// 用于页面布局的扩展方法
    /// </summary>
    public static class LayoutExtensions
    {
        #region Meta
        public static void AddTitleParts(this IHtmlHelper html, string part)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            pageHeadBuilder.AddTitleParts(part);
        }
        public static void AppendTitleParts(this IHtmlHelper html, string part)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            pageHeadBuilder.AppendTitleParts(part);
        }
        public static IHtmlContent DCMSTitle(this IHtmlHelper html, bool addDefaultTitle = true, string part = "")
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            html.AppendTitleParts(part);
            return new HtmlString(html.Encode(pageHeadBuilder.GenerateTitle(addDefaultTitle)));
        }
        public static void AddMetaDescriptionParts(this IHtmlHelper html, string part)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            pageHeadBuilder.AddMetaDescriptionParts(part);
        }
        public static void AppendMetaDescriptionParts(this IHtmlHelper html, string part)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            pageHeadBuilder.AppendMetaDescriptionParts(part);
        }
        public static IHtmlContent DCMSMetaDescription(this IHtmlHelper html, string part = "")
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            html.AppendMetaDescriptionParts(part);
            return new HtmlString(html.Encode(pageHeadBuilder.GenerateMetaDescription()));
        }
        public static string CurrentStoreTitle(this IHtmlHelper html)
        {
            var _storeContext = EngineContext.Current.Resolve<IStoreContext>();
            return _storeContext.CurrentStore == null ? "" : _storeContext.CurrentStore.Name;
        }
        public static void AddMetaKeywordParts(this IHtmlHelper html, string part)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            pageHeadBuilder.AddMetaKeywordParts(part);
        }
        public static void AppendMetaKeywordParts(this IHtmlHelper html, string part)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            pageHeadBuilder.AppendMetaKeywordParts(part);
        }
        public static IHtmlContent DCMSMetaKeywords(this IHtmlHelper html, string part = "")
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            html.AppendMetaKeywordParts(part);
            return new HtmlString(html.Encode(pageHeadBuilder.GenerateMetaKeywords()));
        }
        #endregion



        #region Script
        public static void AddScriptParts(this IHtmlHelper html, string src, string debugSrc = "",
            bool excludeFromBundle = false, bool isAsync = false, int order = 0)
        {
            AddScriptParts(html, ResourceLocation.Head, src, debugSrc, excludeFromBundle, isAsync, order);
        }
        public static void AddScriptParts(this IHtmlHelper html, ResourceLocation location,
            string src, string debugSrc = "", bool excludeFromBundle = false, bool isAsync = false, int order = 0)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            pageHeadBuilder.AddScriptParts(location, src, debugSrc, excludeFromBundle, isAsync, order);
        }
        public static void AppendScriptParts(this IHtmlHelper html, string src, string debugSrc = "",
            bool excludeFromBundle = false, bool isAsync = false, int order = 0)
        {
            AppendScriptParts(html, ResourceLocation.Head, src, debugSrc, excludeFromBundle, isAsync, order);
        }
        public static void AppendScriptParts(this IHtmlHelper html, ResourceLocation location,
            string src, string debugSrc = "", bool excludeFromBundle = false, bool isAsync = false, int order = 0)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            pageHeadBuilder.AppendScriptParts(location, src, debugSrc, excludeFromBundle, isAsync, order);
        }
        public static IHtmlContent DCMSScripts(this IHtmlHelper html, ResourceLocation location, bool? bundleFiles = null)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            return new HtmlString(pageHeadBuilder.GenerateScripts(location, bundleFiles));
        }
        public static void AddInlineScriptParts(this IHtmlHelper html, ResourceLocation location, string script)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            pageHeadBuilder.AddInlineScriptParts(location, script);
        }
        public static void AppendInlineScriptParts(this IHtmlHelper html, ResourceLocation location, string script)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            pageHeadBuilder.AppendInlineScriptParts(location, script);
        }
        public static IHtmlContent DCMSInlineScripts(this IHtmlHelper html, ResourceLocation location)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            return new HtmlString(pageHeadBuilder.GenerateInlineScripts(location));
        }
        #endregion


        #region CSS
        public static void AddCssFileParts(this IHtmlHelper html, string src, string debugSrc = "", bool excludeFromBundle = false)
        {
            AddCssFileParts(html, ResourceLocation.Head, src, debugSrc, excludeFromBundle);
        }
        public static void AddCssFileParts(this IHtmlHelper html, ResourceLocation location,
            string src, string debugSrc = "", bool excludeFromBundle = false)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            pageHeadBuilder.AddCssFileParts(location, src, debugSrc, excludeFromBundle);
        }
        public static void AppendCssFileParts(this IHtmlHelper html, string src, string debugSrc = "", bool excludeFromBundle = false)
        {
            AppendCssFileParts(html, ResourceLocation.Head, src, debugSrc, excludeFromBundle);
        }
        public static void AppendCssFileParts(this IHtmlHelper html, ResourceLocation location,
            string src, string debugSrc = "", bool excludeFromBundle = false)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            pageHeadBuilder.AppendCssFileParts(location, src, debugSrc, excludeFromBundle);
        }
        public static IHtmlContent DCMSCssFiles(this IHtmlHelper html, ResourceLocation location, bool? bundleFiles = null)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            return new HtmlString(pageHeadBuilder.GenerateCssFiles(location, bundleFiles));
        }
        #endregion


        public static void AddCanonicalUrlParts(this IHtmlHelper html, string part, bool withQueryString = false)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();

            if (withQueryString)
            {
                //add ordered query string parameters
                var queryParameters = html.ViewContext.HttpContext.Request.Query.OrderBy(parameter => parameter.Key)
                    .ToDictionary(parameter => parameter.Key, parameter => parameter.Value.ToString());
                part = QueryHelpers.AddQueryString(part, queryParameters);
            }

            pageHeadBuilder.AddCanonicalUrlParts(part);
        }
        public static void AppendCanonicalUrlParts(this IHtmlHelper html, string part)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            pageHeadBuilder.AppendCanonicalUrlParts(part);
        }
        public static IHtmlContent DCMSCanonicalUrls(this IHtmlHelper html, string part = "")
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            html.AppendCanonicalUrlParts(part);
            return new HtmlString(pageHeadBuilder.GenerateCanonicalUrls());
        }


        public static void AddHeadCustomParts(this IHtmlHelper html, string part)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            pageHeadBuilder.AddHeadCustomParts(part);
        }
        public static void AppendHeadCustomParts(this IHtmlHelper html, string part)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            pageHeadBuilder.AppendHeadCustomParts(part);
        }
        public static IHtmlContent DCMSHeadCustom(this IHtmlHelper html)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            return new HtmlString(pageHeadBuilder.GenerateHeadCustom());
        }



        public static void AddPageCssClassParts(this IHtmlHelper html, string part)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            pageHeadBuilder.AddPageCssClassParts(part);
        }
        public static void AppendPageCssClassParts(this IHtmlHelper html, string part)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            pageHeadBuilder.AppendPageCssClassParts(part);
        }
        public static IHtmlContent DCMSPageCssClasses(this IHtmlHelper html, string part = "", bool includeClassElement = true)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            html.AppendPageCssClassParts(part);
            var classes = pageHeadBuilder.GeneratePageCssClasses();

            if (string.IsNullOrEmpty(classes))
            {
                return null;
            }

            var result = includeClassElement ? $"class=\"{classes}\"" : classes;
            return new HtmlString(result);
        }




        public static void SetActiveMenuItemSystemName(this IHtmlHelper html, string systemName)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            pageHeadBuilder.SetActiveMenuItemSystemName(systemName);
        }
        public static string GetActiveMenuItemSystemName(this IHtmlHelper html)
        {
            var pageHeadBuilder = EngineContext.Current.Resolve<IPageHeadBuilder>();
            return pageHeadBuilder.GetActiveMenuItemSystemName();
        }
        public static string ResourceServerUrl(this IHtmlHelper html, string part)
        {
            return ResourceServerUrl(part);
        }

        public static string ResourceServerUrl(string part)
        {
            var server = EngineContext.GetStaticResourceServer;
            if (string.IsNullOrEmpty(server))
            {
                return $"{part}";
            }
            else
            {
                return $"{server}{part.Replace("~", "")}";
            }
        }
    }
}