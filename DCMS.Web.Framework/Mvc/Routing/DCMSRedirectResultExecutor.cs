using DCMS.Core.Domain.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace DCMS.Web.Framework.Mvc.Routing
{
    /// <summary>
    /// 表示自定义重写的重定向结果执行器
    /// </summary>
    public class DCMSRedirectResultExecutor : RedirectResultExecutor
    {
        #region Fields

        private readonly SecuritySettings _securitySettings;

        #endregion

        #region Ctor

        public DCMSRedirectResultExecutor(ILoggerFactory loggerFactory,
            IUrlHelperFactory urlHelperFactory,
            SecuritySettings securitySettings) : base(loggerFactory, urlHelperFactory)
        {
            _securitySettings = securitySettings;
        }

        #endregion

        #region Methods

        public override Task ExecuteAsync(ActionContext context, RedirectResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (_securitySettings.AllowNonAsciiCharactersInHeaders)
            {
                //passed redirect URL may contain non-ASCII characters, that are not allowed now (see https://github.com/aspnet/KestrelHttpServer/issues/1144)
                //so we force to encode this URL before processing
                result.Url = Uri.EscapeUriString(WebUtility.UrlDecode(result.Url));
            }

            return base.ExecuteAsync(context, result);
        }

        #endregion
    }
}