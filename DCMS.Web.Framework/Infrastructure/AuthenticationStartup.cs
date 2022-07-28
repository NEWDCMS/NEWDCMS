using DCMS.Core.Infrastructure;
using DCMS.Web.Framework.Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DCMS.Web.Framework.Infrastructure
{
    /// <summary>
    /// 表示在应用程序启动时配置身份验证中间件的对象
    /// </summary>
    public class AuthenticationStartup : IDCMSStartup
    {
        /// <summary>
        /// 添加和配置中间件
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration, bool apiPlatform = false)
        {
            //添加数据提供
            //services.AddDCMSDataProtection();
            //添加认证
            services.AddDCMSAuthentication(apiPlatform);
        }

        /// <summary>
        /// 配置使用中间件
        /// </summary>
        /// <param name="application">构建请求管道</param>
        public void Configure(IApplicationBuilder application, bool apiPlatform = false)
        {
            application.UseDCMSAuthentication(apiPlatform);
        }

        /// <summary>
        /// 优先级 应在MVC之前加载身份验证
        /// </summary>
        public int Order => 500;
    }
}