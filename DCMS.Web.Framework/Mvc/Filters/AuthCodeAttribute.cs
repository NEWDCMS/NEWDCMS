using DCMS.Core;
using DCMS.Core.Data;
using DCMS.Core.Domain.Users;
using DCMS.Core.Infrastructure;
using DCMS.Services.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;

namespace DCMS.Web.Framework.Mvc.Filters
{

    public class AuthCodeAttribute : TypeFilterAttribute
    {


        private readonly bool _ignoreFilter;
        private readonly int _codeFilter;


        public AuthCodeAttribute(int code, bool ignore = false) : base(typeof(AuthCodeFilter))
        {
            _ignoreFilter = ignore;
            _codeFilter = code;
            Arguments = new object[] { code, ignore };
        }

        public AuthCodeAttribute(AccessGranularityEnum code, bool ignore = false)
            : base(typeof(AuthCodeFilter))
        {
            _ignoreFilter = ignore;
            _codeFilter = (int)code;
            Arguments = new object[] { code, ignore };
        }

        /// <summary>
        /// 是否忽略筛选器操作的执行
        /// </summary>
        public bool IgnoreFilter => _ignoreFilter;

        public int CodeFilter => _codeFilter;


        private class AuthCodeFilter : IAuthorizationFilter
        {


            private readonly bool _ignoreFilter;
            private readonly IPermissionService _permissionService;
            private readonly int _codeFilter;


            public AuthCodeFilter(int codeFilter, bool ignoreFilter, IPermissionService permissionService)
            {
                _ignoreFilter = ignoreFilter;
                _codeFilter = codeFilter;
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
                if (filterContext.Filters.Any(filter => filter is AuthCodeFilter))
                {
                    //授权访问管理区域的权限
                    var currentUser = EngineContext.Current.Resolve<IWorkContext>().CurrentUser;

                    //#if RELEASE
                    //排除管理员
                    if (currentUser.IsAdmin())
                    {
                        return;
                    }
                    //#endif

                    var authorizeCodes = _permissionService.GetUserAuthorizeCodesByUserId(currentUser.StoreId, currentUser != null ? currentUser.Id : 0, false);
                    if (authorizeCodes.Where(a => a == _codeFilter.ToString()).ToList().Count <= 0)
                    {
                        new RedirectToPageResult("AccessDeniedView");
                        //var webHelper = EngineContext.Current.Resolve<IWebHelper>();
                        //filterContext.Result = new RedirectToActionResult("AccessDeniedView", "Security", new { pageUrl = webHelper.GetRawUrl(filterContext.HttpContext.Request) });
                    }
                }
            }
        }
    }
}