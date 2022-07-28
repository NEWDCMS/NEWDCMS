using DCMS.Core;
using DCMS.Core.Data;
using DCMS.Services.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using System;

namespace DCMS.Web.Framework.Mvc.Filters
{
    /// <summary>
    /// 表示验证用户密码过期的筛选器属性
    /// </summary>
    public class ValidatePasswordAttribute : TypeFilterAttribute
    {

        public ValidatePasswordAttribute() : base(typeof(ValidatePasswordFilter))
        {
        }

        private class ValidatePasswordFilter : IActionFilter
        {


            private readonly IUserService _userService;
            private readonly IUrlHelperFactory _urlHelperFactory;
            private readonly IWorkContext _workContext;


            public ValidatePasswordFilter(IUserService userService,
                IUrlHelperFactory urlHelperFactory,
                IWorkContext workContext)
            {
                _userService = userService;
                _urlHelperFactory = urlHelperFactory;
                _workContext = workContext;
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

                if (!DataSettingsManager.DatabaseIsInstalled)
                {
                    return;
                }

                //get action and controller names
                var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
                var actionName = actionDescriptor?.ActionName;
                var controllerName = actionDescriptor?.ControllerName;

                if (string.IsNullOrEmpty(actionName) || string.IsNullOrEmpty(controllerName))
                {
                    return;
                }


                //if (_workContext.CurrentUser == null)
                //{
                //    var loginUrl = _urlHelperFactory.GetUrlHelper(context).RouteUrl("Login");
                //    context.Result = new RedirectResult(loginUrl);
                //    //return;
                //}

                //don't validate on ChangePassword page
                if (!(controllerName.Equals("User", StringComparison.InvariantCultureIgnoreCase) &&
                    actionName.Equals("ChangePassword", StringComparison.InvariantCultureIgnoreCase)))
                {
                    //检查密码是否过期
                    if (_workContext.CurrentUser != null && _userService.PasswordIsExpired(_workContext.CurrentUser))
                    {
                        //
                        var changePasswordUrl = _urlHelperFactory.GetUrlHelper(context).RouteUrl("UserChangePassword");
                        context.Result = new RedirectResult(changePasswordUrl);
                    }
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


        }


    }
}