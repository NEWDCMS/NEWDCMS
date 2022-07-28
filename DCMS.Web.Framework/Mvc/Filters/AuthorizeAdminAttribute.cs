using DCMS.Core.Data;
using DCMS.Services.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;

namespace DCMS.Web.Framework.Mvc.Filters
{
    /// <summary>
    /// 用于访问管理区域过滤器
    /// </summary>
    public class AuthorizeAdminAttribute : TypeFilterAttribute
    {

        private readonly bool _ignoreFilter;

        public AuthorizeAdminAttribute(bool ignore = false) : base(typeof(AuthorizeAdminFilter))
        {
            _ignoreFilter = ignore;
            Arguments = new object[] { ignore };
        }

        /// <summary>
        /// 是否忽略筛选器操作的执行
        /// </summary>
        public bool IgnoreFilter => _ignoreFilter;


        /// <summary>
        /// 表示确认访问管理面板的筛选器
        /// </summary>
        private class AuthorizeAdminFilter : IAuthorizationFilter
        {

            private readonly bool _ignoreFilter;
            private readonly IPermissionService _permissionService;


            public AuthorizeAdminFilter(bool ignoreFilter, IPermissionService permissionService)
            {
                _ignoreFilter = ignoreFilter;
                _permissionService = permissionService;
            }

            /// <summary>
            /// 在筛选器管道的早期调用以确认请求已授权
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
                    .Select(filterDescriptor => filterDescriptor.Filter).OfType<AuthorizeAdminAttribute>().FirstOrDefault();

                //忽略筛选器（即使用户没有访问管理区域，该操作也可用）
                if (actionFilter?.IgnoreFilter ?? _ignoreFilter)
                {
                    return;
                }

                if (!DataSettingsManager.DatabaseIsInstalled)
                {
                    return;
                }

                //检查访问权限
                if (filterContext.Filters.Any(filter => filter is AuthorizeAdminFilter))
                {
                    //授权访问管理区域的权限
                    if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                    {
                        filterContext.Result = new ChallengeResult();
                    }
                }
            }

        }
    }
}