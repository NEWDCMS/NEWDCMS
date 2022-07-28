using DCMS.Core;
using DCMS.Core.Data;
using DCMS.Core.Domain;
using DCMS.Services.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Linq;



namespace DCMS.Web.Framework.Mvc.Filters
{
    /// <summary>
    /// 表示确认访问关闭页面的筛选器属性
    /// </summary>
    public class CheckAccessClosedStoreAttribute : TypeFilterAttribute
    {
        #region Fields

        private readonly bool _ignoreFilter;

        #endregion

        #region Ctor

        /// <summary>
        /// Create instance of the filter attribute
        /// </summary>
        /// <param name="ignore">Whether to ignore the execution of filter actions</param>
        public CheckAccessClosedStoreAttribute(bool ignore = false) : base(typeof(CheckAccessClosedStoreFilter))
        {
            _ignoreFilter = ignore;
            Arguments = new object[] { ignore };
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether to ignore the execution of filter actions
        /// </summary>
        public bool IgnoreFilter => _ignoreFilter;

        #endregion

        #region Nested filter

        /// <summary>
        /// Represents a filter that confirms access to closed store
        /// </summary>
        private class CheckAccessClosedStoreFilter : IActionFilter
        {
            #region Fields

            private readonly bool _ignoreFilter;
            private readonly IPermissionService _permissionService;
            private readonly IStoreContext _storeContext;
            private readonly IUrlHelperFactory _urlHelperFactory;
            private readonly StoreInformationSettings _storeInformationSettings;

            #endregion

            #region Ctor

            public CheckAccessClosedStoreFilter(bool ignoreFilter,
                IPermissionService permissionService,
                IStoreContext storeContext,
                IUrlHelperFactory urlHelperFactory,
                StoreInformationSettings storeInformationSettings)
            {
                _ignoreFilter = ignoreFilter;
                _permissionService = permissionService;
                _storeContext = storeContext;
                _urlHelperFactory = urlHelperFactory;
                _storeInformationSettings = storeInformationSettings;
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

                //check whether this filter has been overridden for the Action
                var actionFilter = context.ActionDescriptor.FilterDescriptors
                    .Where(filterDescriptor => filterDescriptor.Scope == FilterScope.Action)
                    .Select(filterDescriptor => filterDescriptor.Filter).OfType<CheckAccessClosedStoreAttribute>().FirstOrDefault();

                //ignore filter (the action is available even if a store is closed)
                if (actionFilter?.IgnoreFilter ?? _ignoreFilter)
                {
                    return;
                }

                if (!DataSettingsManager.DatabaseIsInstalled)
                {
                    return;
                }

                //store isn't closed
                if (!_storeInformationSettings.StoreClosed)
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


                //check whether current user has access to a closed store
                //if (_permissionService.Authorize(StandardPermissionProvider.AccessClosedStore))
                //    return;

                //store is closed and no access, so redirect to 'StoreClosed' page
                var storeClosedUrl = _urlHelperFactory.GetUrlHelper(context).RouteUrl("StoreClosed");
                context.Result = new RedirectResult(storeClosedUrl);
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