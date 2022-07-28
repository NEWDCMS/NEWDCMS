using DCMS.Core;
using DCMS.Core.Data;
using DCMS.Services.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Net;

namespace DCMS.Web.Framework.Mvc.Filters
{
    /// <summary>
    /// 表示保存上次用户活动日期的筛选器属性
    /// </summary>
    public class SaveLastActivityAttribute : TypeFilterAttribute
    {
        #region Ctor

        /// <summary>
        /// Create instance of the filter attribute
        /// </summary>
        public SaveLastActivityAttribute() : base(typeof(SaveLastActivityFilter))
        {
        }

        #endregion

        #region Nested filter

        /// <summary>
        /// Represents a filter that saves last user activity date
        /// </summary>
        private class SaveLastActivityFilter : IActionFilter
        {
            #region Fields

            private readonly IUserService _userService;
            private readonly IWorkContext _workContext;

            #endregion

            #region Ctor

            public SaveLastActivityFilter(IUserService userService,
                IWorkContext workContext)
            {
                _userService = userService;
                _workContext = workContext;
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

                //update last activity date
                if (_workContext.CurrentUser != null && _workContext.CurrentUser.LastActivityDateUtc.AddMinutes(1.0) < DateTime.UtcNow)
                {
                    _workContext.CurrentUser.LastActivityDateUtc = DateTime.UtcNow;
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