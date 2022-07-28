using DCMS.Core.Data;
using DCMS.Services.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;


namespace DCMS.Web.Framework.Mvc.Filters
{
    /// <summary>
    /// 表示确认访问公用页面的筛选器属性
    /// </summary>
    public class CheckAccessPublicStoreAttribute : TypeFilterAttribute
    {

        private readonly bool _ignoreFilter;
        public bool IgnoreFilter => _ignoreFilter;


        public CheckAccessPublicStoreAttribute(bool ignore = false) : base(typeof(CheckAccessPublicStoreFilter))
        {
            _ignoreFilter = ignore;
            Arguments = new object[] { ignore };
        }

        private class CheckAccessPublicStoreFilter : IAuthorizationFilter
        {

            private readonly bool _ignoreFilter;
            private readonly IPermissionService _permissionService;



            public CheckAccessPublicStoreFilter(bool ignoreFilter, IPermissionService permissionService)
            {
                _ignoreFilter = ignoreFilter;
                _permissionService = permissionService;
            }



            /// <summary>
            /// 在筛选器管道的早期调用以确认请求是否已授权
            /// </summary>
            /// <param name="filterContext">Authorization filter context</param>
            public void OnAuthorization(AuthorizationFilterContext filterContext)
            {
                if (filterContext == null)
                {
                    throw new ArgumentNullException(nameof(filterContext));
                }

                //检查是否已为此操作重写此筛选器
                var actionFilter = filterContext.ActionDescriptor.FilterDescriptors
                    .Where(filterDescriptor => filterDescriptor.Scope == FilterScope.Action)
                    .Select(filterDescriptor => filterDescriptor.Filter).OfType<CheckAccessPublicStoreAttribute>().FirstOrDefault();

                //是否忽略过滤器
                if (actionFilter?.IgnoreFilter ?? _ignoreFilter)
                {
                    return;
                }

                if (!DataSettingsManager.DatabaseIsInstalled)
                {
                    return;
                }

                //检查当前用户是否可以访问站点
                if (_permissionService.Authorize(StandardPermissionProvider.PublicStoreAllowNavigation))
                {
                    return;
                }

                //未授权，拒绝访问站点
                filterContext.Result = new ChallengeResult();
            }


        }

    }




    //[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    //public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    //{
    //    public void OnAuthorization(AuthorizationFilterContext context)
    //    {
    //        var user = (User)context.HttpContext.Items["User"];
    //        if (user == null)
    //        {
    //            // not logged in
    //            context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
    //        }
    //    }
    //}
}