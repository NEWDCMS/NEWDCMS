using DCMS.Core;
using DCMS.Core.Data;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace DCMS.Services.Common
{
    /// <summary>
    /// Represents middleware that checks whether request is for keep alive
    /// </summary>
    public class KeepAliveMiddleware
    {
        #region Fields

        private readonly RequestDelegate _next;

        #endregion

        #region Ctor

        public KeepAliveMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        #endregion

        #region Methods

        /// <summary>
        /// 执行中间件 actions
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <param name="webHelper">Web helper</param>
        /// <returns>Task</returns>
        public async Task Invoke(HttpContext context, IWebHelper webHelper)
        {
            if (DataSettingsManager.DatabaseIsInstalled)
            {
                //keep alive page requested (we ignore it to prevent creating a guest user records)
                var keepAliveUrl = $"{webHelper.GetStoreLocation()}{DCMSCommonDefaults.KeepAlivePath}";
                if (webHelper.GetThisPageUrl(false).StartsWith(keepAliveUrl, StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }
            }

            //调用请求管道中的下一个中间件
            await _next(context);
        }

        #endregion
    }
}