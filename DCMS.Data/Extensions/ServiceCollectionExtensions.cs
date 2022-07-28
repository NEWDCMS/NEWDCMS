using DCMS.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace DCMS.Data.Extensions
{
    public static class ServiceCollectionExtensions
    {
        ///// <summary>
        ///// 添加对数据库的支持
        ///// </summary>
        ///// <param name="services"></param>
        ///// <returns></returns>
        //public static IServiceCollection AddDatabase(this IServiceCollection services)
        //{
        //    services.TryAddScoped<IDbProvider, DbProvider>();
        //    services.TryAddSingleton<IUnitOfWorkFactory, UnitOfWorkFactory>();
        //    //services.TryAddSingleton<IRepositoryFactory, RepositoryFactory>();
        //    return services;
        //}


        /// <summary>
        /// 添加DOBOptions配置选项
        /// </summary>
        /// <param name="services"></param>
        /// <param name="dbName"></param>
        /// <param name="builderAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddDbBuilderOptions(this IServiceCollection services, string dbName, Action<DbContextOptionsBuilder> builderAction = null)
        {
            var builder = new DbContextOptionsBuilder();
            builderAction(builder);
            services.TryAddSingleton(new DOBOptions(builder, dbName));
            return services;
        }

        /// <summary>
        /// 添加DOBOptions配置选项
        /// </summary>
        /// <param name="services"></param>
        /// <param name="dbName"></param>
        /// <param name="builderAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddDbBuilderOptions<TContext>(this IServiceCollection services, string dbName, Action<DbContextOptionsBuilder<TContext>> builderAction = null)
            where TContext : DbContext
        {
            var builder = new DbContextOptionsBuilder<TContext>();
            builderAction(builder);
            services.TryAddSingleton(new DOBOptions(builder, dbName, typeof(TContext)));
            return services;
        }
    }
}
