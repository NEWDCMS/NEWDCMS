using DCMS.Core.Infrastructure;
using DCMS.Web.Framework.Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DCMS.Data;


namespace DCMS.Web.Framework.Infrastructure
{
    /// <summary>
    /// 表示启动时配置数据库上下文对象
    /// </summary>
    public class DCMSDbStartup : IDCMSStartup
    {
        /// <summary>
        /// 添加并配置中间件
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration, bool apiPlatform = false)
        {
            //添加对象上下文
            services.AddDCMSObjectContext<DbContextBase>();

            //添加读写支持
            //services.AddDCMSDatabase();

            //添加EF服务
            //services.AddEntityFrameworkSqlServer();
            services.AddEntityFrameworkProxies();
        }


        /// <summary>
        /// 配置使用添加的中间件
        /// </summary>
        /// <param name="application">用于配置应用程序的请求管道的生成器</param>
        public void Configure(IApplicationBuilder application, bool apiPlatform = false)
        {
        }

        /// <summary>
        /// 启动优先级
        /// </summary>
        public int Order => 10;
    }
}