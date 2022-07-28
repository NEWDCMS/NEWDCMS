
using DCMS.Core.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DCMS.Data
{
    public static class MySqlServiceCollectionExtensions
    {
        /// <summary>
        /// 添加DCMS框架对MySql数据库的支持
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddDCMSMySql(this IServiceCollection services)
        {
            services.TryAddSingleton<IDbContextOptionsBuilderUser, MySqlDbContextOptionsBuilderUser>();
            return services;
        }
    }
}
