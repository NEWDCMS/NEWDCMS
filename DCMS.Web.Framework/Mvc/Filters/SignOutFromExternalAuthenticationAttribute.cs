using DCMS.Services.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace DCMS.Web.Framework.Mvc.Filters
{
    /// <summary>
    /// 表示从外部身份验证方案注销的筛选器属性
    /// </summary>
    public class SignOutFromExternalAuthenticationAttribute : TypeFilterAttribute
    {
        #region Ctor

        /// <summary>
        /// Create instance of the filter attribute
        /// </summary>
        public SignOutFromExternalAuthenticationAttribute() : base(typeof(SignOutFromExternalAuthenticationFilter))
        {
        }

        #endregion

        #region Nested filter

        /// <summary>
        /// Represents a filter that sign out from the external authentication scheme
        /// </summary>
        private class SignOutFromExternalAuthenticationFilter : IAuthorizationFilter
        {
            #region Methods

            /// <summary>
            /// Called early in the filter pipeline to confirm request is authorized
            /// </summary>
            /// <param name="filterContext">Authorization filter context</param>
            public async void OnAuthorization(AuthorizationFilterContext filterContext)
            {
                if (filterContext == null)
                {
                    throw new ArgumentNullException(nameof(filterContext));
                }

                //sign out from the external authentication scheme
                var authenticateResult = await filterContext.HttpContext.AuthenticateAsync(DCMSAuthenticationDefaults.ExternalAuthenticationScheme);
                if (authenticateResult.Succeeded)
                {
                    await filterContext.HttpContext.SignOutAsync(DCMSAuthenticationDefaults.ExternalAuthenticationScheme);
                }
            }

            #endregion
        }

        #endregion
    }
}