using Autofac;
using Microsoft.EntityFrameworkCore;
using System;


namespace DCMS.Web.Framework.Infrastructure.Extensions
{
    /// <summary>
    /// Autofac ContainerBuilder 扩展
    /// </summary>
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// 为插件注册数据上下文
        /// </summary>
        /// <typeparam name="TContext">DB Context type</typeparam>
        /// <param name="builder">Builder</param>
        /// <param name="contextName">Context name</param>
        public static void RegisterPluginDataContext<TContext>(this ContainerBuilder builder, string contextName) where TContext : DbContext
        {
            //register named context
            builder.Register(context => (DbContext)Activator.CreateInstance(typeof(TContext), new[] { context.Resolve<DbContextOptions<TContext>>() }))
                .Named<DbContext>(contextName).InstancePerLifetimeScope();
        }
    }
}