
using DCMS.Core.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DCMS.Data
{
    public static class SqlServerServiceCollectionExtensions
    {
        /// <summary>
        /// 添加DCMS框架对Sql Server数据库的支持
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddDCMSSqlServer(this IServiceCollection services)
        {
            services.TryAddSingleton<IDbContextOptionsBuilderUser, SqlServerDbContextOptionsBuilderUser>();
            return services;
        }
    }
}
