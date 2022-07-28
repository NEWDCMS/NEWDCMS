using DCMS.Core;
using DCMS.Core.Data;
using DCMS.Core.Domain.Users;
using DCMS.Services.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Net;

namespace DCMS.Web.Framework.Mvc.Filters
{
    /// <summary>
    /// 表示保存用户最后一个IP地址的筛选器属性
    /// </summary>
    public class SaveIpAddressAttribute : TypeFilterAttribute
    {
        #region Ctor

        /// <summary>
        /// Create instance of the filter attribute
        /// </summary>
        public SaveIpAddressAttribute() : base(typeof(SaveIpAddressFilter))
        {
        }

        #endregion

        #region Nested filter

        /// <summary>
        /// Represents a filter that saves last IP address of user
        /// </summary>
        private class SaveIpAddressFilter : IActionFilter
        {
            #region Fields

            private readonly IUserService _userService;
            private readonly IWebHelper _webHelper;
            private readonly IWorkContext _workContext;
            private readonly UserSettings _userSettings;

            #endregion

            #region Ctor

            public SaveIpAddressFilter(IUserService userService,
                IWebHelper webHelper,
                IWorkContext workContext,
                UserSettings userSettings)
            {
                _userService = userService;
                _webHelper = webHelper;
                _workContext = workContext;
                _userSettings = userSettings;
            }

            #endregion

            #region Methods

            /// <summary>
            /// Called before the action executes, after model binding is complete
            /// </summary>
            /// <param name="context">A context for action filters</param>
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

                //only in GET requests
                if (!context.HttpContext.Request.Method.Equals(WebRequestMethods.Http.Get, StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }

                if (!DataSettingsManager.DatabaseIsInstalled)
                {
                    return;
                }

                //check whether we store IP addresses
                if (!_userSettings.StoreIpAddresses)
                {
                    return;
                }

                //get current IP address
                var currentIpAddress = _webHelper.GetCurrentIpAddress();
                if (string.IsNullOrEmpty(currentIpAddress))
                {
                    return;
                }


                //update user's IP address
                if (_workContext.OriginalUserIfImpersonated == null &&
                     !currentIpAddress.Equals(_workContext.CurrentUser.LastIpAddress, StringComparison.InvariantCultureIgnoreCase))
                {
                    _workContext.CurrentUser.LastIpAddress = currentIpAddress;
                    _userService.UpdateUser(_workContext.CurrentUser);
                }
            }

            /// <summary>
            /// Called after the action executes, before the action result
            /// </summary>
            /// <param name="context">A context for action filters</param>
            public void OnActionExecuted(ActionExecutedContext context)
            {
                //do nothing
            }

            #endregion
        }

        #endregion
    }
}