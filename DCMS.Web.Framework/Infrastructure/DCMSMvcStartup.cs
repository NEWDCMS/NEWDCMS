using DCMS.Core.Infrastructure;
using DCMS.Web.Framework.Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DCMS.Web.Framework.Infrastructure
{
    /// <summary>
    /// 表示在应用程序启动时配置MVC的对象
    /// </summary>
    public class DCMSMvcStartup : IDCMSStartup
    {
        /// <summary>
        /// 添加和配置任何中间件
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration, bool apiPlatform = false)
        {
            ////添加白名单过滤
            //services.AddBlackList(configuration);

            //添加 MiniProfiler 服务
            services.AddDCMSMiniProfiler();

            //添加 WebMarkupMin 服务
            services.AddDCMSWebMarkupMin();

            //添加配置 MVC 功能
            services.AddDCMSMvc();

            //添加自定义重定向结果执行器
            services.AddDCMSRedirectResultExecutor();
        }

        /// <summary>
        /// 配置使用添加的中间件
        /// </summary>
        /// <param name="application">用于配置应用程序的请求管道的生成器</param>
        /// 3.1
        public void Configure(IApplicationBuilder application, bool apiPlatform = false)
        {
            //使用 MiniProfiler
            application.UseMiniProfiler();

            //使用 WebMarkupMin
            application.UseDCMSWebMarkupMin();

            //使用 Endpoints routing
            application.UseDCMSEndpoints();
        }


        /// <summary>
        /// 启动优先级别，MVC应该最后加载
        /// </summary>
        public int Order => 1000;

    }
}