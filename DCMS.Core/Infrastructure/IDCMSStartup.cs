using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DCMS.Core.Infrastructure
{
    /// <summary>
    /// 表示应用程序启动时配置服务和中间件的对象
    /// </summary>
    public interface IDCMSStartup
    {
        void ConfigureServices(IServiceCollection services, IConfiguration configuration, bool apiPlatform = false);
        void Configure(IApplicationBuilder application , bool apiPlatform = false);
        int Order { get; }
        //bool ApiPlatform { get; set; }

    }
}
