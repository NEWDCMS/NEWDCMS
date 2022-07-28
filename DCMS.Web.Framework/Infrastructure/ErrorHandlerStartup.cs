using DCMS.Core.Infrastructure;
using DCMS.Web.Framework.Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DCMS.Web.Framework.Infrastructure
{
    /// <summary>
    /// 表示用于在应用程序启动时配置异常和错误处理的对象
    /// </summary>
    public class ErrorHandlerStartup : IDCMSStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration, bool apiPlatform = false)
        {
        }

        /// <summary>
        /// 配置使用添加的中间件
        /// </summary>
        /// <param name="application">用于配置应用程序的请求管道的生成器</param>
        public void Configure(IApplicationBuilder application, bool apiPlatform = false)
        {
            //异常处理
            application.UseDCMSExceptionHandler();

            //400 错误 (bad request)
            application.UseBadRequestResult();

            //404 错误 (not found)
            application.UsePageNotFound();
        }

        /// <summary>
        /// 启动优先级别，启动时应首先加载错误处理程序
        /// </summary>
        public int Order => 0;
    }
}