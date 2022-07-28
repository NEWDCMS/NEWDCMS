using DCMS.Core.Http.Extensions;
using DCMS.Core.Infrastructure;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace DCMS.Services.Authentication.External
{
    /// <summary>
    /// External authorizer helper
    /// </summary>
    public static partial class ExternalAuthorizerHelper
    {
        #region Methods

        /// <summary>
        /// Add error
        /// </summary>
        /// <param name="error">Error</param>
        public static void AddErrorsToDisplay(string error)
        {
            var session = EngineContext.Current.Resolve<IHttpContextAccessor>().HttpContext.Session;
            var errors = session.Get<IList<string>>(DCMSAuthenticationDefaults.ExternalAuthenticationErrorsSessionKey) ?? new List<string>();
            errors.Add(error);
            session.Set(DCMSAuthenticationDefaults.ExternalAuthenticationErrorsSessionKey, errors);
        }

        /// <summary>
        /// Retrieve errors to display
        /// </summary>
        /// <returns>Errors</returns>
        public static IList<string> RetrieveErrorsToDisplay()
        {
            var session = EngineContext.Current.Resolve<IHttpContextAccessor>().HttpContext.Session;
            var errors = session.Get<IList<string>>(DCMSAuthenticationDefaults.ExternalAuthenticationErrorsSessionKey);

            if (errors != null)
            {
                session.Remove(DCMSAuthenticationDefaults.ExternalAuthenticationErrorsSessionKey);
            }

            return errors;
        }

        #endregion
    }
}