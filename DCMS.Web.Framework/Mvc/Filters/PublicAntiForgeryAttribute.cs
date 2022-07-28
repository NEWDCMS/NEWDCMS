using DCMS.Core.Domain.Security;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;

namespace DCMS.Web.Framework.Mvc.Filters
{
    /// <summary>
    /// 防止 XSRF：跨站请求伪造
    /// </summary>
    public class PublicAntiForgeryAttribute : TypeFilterAttribute
    {
        #region Fields

        private readonly bool _ignoreFilter;

        #endregion

        #region Ctor

        public PublicAntiForgeryAttribute(bool ignore = false) : base(typeof(PublicAntiForgeryFilter))
        {
            _ignoreFilter = ignore;
            Arguments = new object[] { ignore };
        }

        #endregion

        #region Properties

        public bool IgnoreFilter => _ignoreFilter;

        #endregion

        #region Nested filter

        private class PublicAntiForgeryFilter : ValidateAntiforgeryTokenAuthorizationFilter
        {
            #region Fields

            private readonly bool _ignoreFilter;
            private readonly SecuritySettings _securitySettings;

            #endregion

            #region Ctor

            public PublicAntiForgeryFilter(bool ignoreFilter,
                SecuritySettings securitySettings,
                IAntiforgery antiforgery,
                ILoggerFactory loggerFactory) : base(antiforgery, loggerFactory)
            {
                _ignoreFilter = ignoreFilter;
                _securitySettings = securitySettings;
            }

            #endregion

            #region Methods


            protected override bool ShouldValidate(AuthorizationFilterContext context)
            {
                if (!base.ShouldValidate(context))
                {
                    return false;
                }

                if (context.HttpContext.Request == null)
                {
                    return false;
                }

                //ignore GET requests
                if (context.HttpContext.Request.Method.Equals(WebRequestMethods.Http.Get, StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }

                if (!_securitySettings.EnableXsrfProtectionForPublicStore)
                {
                    return false;
                }

                //check whether this filter has been overridden for the Action
                var actionFilter = context.ActionDescriptor.FilterDescriptors
                    .Where(filterDescriptor => filterDescriptor.Scope == FilterScope.Action)
                    .Select(filterDescriptor => filterDescriptor.Filter).OfType<PublicAntiForgeryAttribute>().FirstOrDefault();

                //ignore this filter
                if (actionFilter?.IgnoreFilter ?? _ignoreFilter)
                {
                    return false;
                }

                return true;
            }

            #endregion
        }

        #endregion
    }
}