using DCMS.Core.Infrastructure;
using DCMS.Web.Framework.Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DCMS.Web.Framework.Infrastructure
{
    /// <summary>
    /// 表示在应用程序启动时配置公共功能和中间件的对象
    /// </summary>
    public class DCMSCommonStartup : IDCMSStartup
    {
        /// <summary>
        /// 添加和配置中间件
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration, bool apiPlatform = false)
        {
            //添加响应压缩
            services.AddResponseCompression();

            //添加项目配置功能
            services.AddOptions();

            //添加分布式内存缓存
            services.AddDistributedMemoryCache();

            //添加 HTTP sesion 状态功能
            services.AddHttpSession();

            //添加默认 HTTP clients
            services.AddDCMSHttpClients();

            //添加 anti-forgery
            services.AddAntiForgery();

            //添加本地化
            services.AddLocalization();

            //添加主题支持
            //services.AddThemes();
        }

        /// <summary>
        /// 配置使用中间件
        /// </summary>
        /// <param name="application"></param>
        public void Configure(IApplicationBuilder application, bool apiPlatform = false)
        {
            application.UseDCMSResponseCompression();
            application.UseDCMSStaticFiles();
            application.UseSession();
        }

        public int Order => 100;
    }
}