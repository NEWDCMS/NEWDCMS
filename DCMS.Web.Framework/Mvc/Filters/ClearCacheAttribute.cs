using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Net;

namespace DCMS.Web.Framework.Mvc.Filters
{

    public class ClearCacheAttribute : TypeFilterAttribute
    {

        private readonly bool _ignoreFilter;
        private readonly string _patternFilter;

        public ClearCacheAttribute(string pattern, bool ignore = false) : base(typeof(ClearCacheFilter))
        {
            _ignoreFilter = ignore;
            _patternFilter = pattern;
            Arguments = new object[] { pattern, ignore };
        }

        /// <summary>
        /// 是否忽略筛选器操作的执行
        /// </summary>
        public bool IgnoreFilter => _ignoreFilter;

        public string PatternFilter => _patternFilter;


        private class ClearCacheFilter : IActionFilter
        {
            private readonly bool _ignoreFilter;
            
            private readonly string _patternFilter;
            protected readonly IStaticCacheManager _cacheManager;

            public ClearCacheFilter(string patternFilter, bool ignoreFilter, IStaticCacheManager cacheManager)
            {
                _ignoreFilter = ignoreFilter;
                _patternFilter = patternFilter;
                _cacheManager = cacheManager;
            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                if (context.HttpContext.Request == null)
                {
                    return;
                }

                if (!context.HttpContext.Request.Method.Equals(WebRequestMethods.Http.Get, StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }

                if (context.HttpContext.Request.QueryString.Value?.ToLower().Contains("cache") ?? false)
                {
                    var currentStore = EngineContext.Current.Resolve<IStoreContext>().CurrentStore;
                    _cacheManager.RemoveByPrefix(string.Format(_patternFilter, currentStore?.Id ?? 0));

                }
            }

            public void OnActionExecuted(ActionExecutedContext context)
            {

            }
        }
    }
}