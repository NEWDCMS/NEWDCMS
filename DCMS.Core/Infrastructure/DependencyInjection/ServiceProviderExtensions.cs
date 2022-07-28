using DCMS.Core.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace DCMS.Core.Infrastructure.DependencyInjection
{
    public static class ServiceProviderExtensions
    {
        /// <summary>
        /// 获取配置信息
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IConfiguration GetConfiguration(this IServiceCollection services)
        {
            return (IConfiguration)services.FirstOrDefault(d => d.ServiceType == typeof(IConfiguration))?.ImplementationInstance;
        }

        /// <summary>
        /// 获取DCMSOptions
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static DCMSOptions DCMSOptions(this IServiceProvider provider)
        {
            return provider.GetRequiredService<IOptions<DCMSOptions>>()?.Value;
        }

        /// <summary>
        /// 获取指定类型的日志对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static ILogger<T> GetLogger<T>(this IServiceProvider provider)
        {
            ILoggerFactory factory = provider.GetRequiredService<ILoggerFactory>();
            return factory.CreateLogger<T>();
        }

        /// <summary>
        /// 获取指定类型的日志对象
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ILogger GetLogger(this IServiceProvider provider, Type type)
        {
            ILoggerFactory factory = provider.GetRequiredService<ILoggerFactory>();
            return factory.CreateLogger(type);
        }

        /// <summary>
        /// 获取指定名称的日志对象
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ILogger GetLogger(this IServiceProvider provider, string name)
        {
            ILoggerFactory factory = provider.GetService<ILoggerFactory>();
            return factory.CreateLogger(name);
        }
    }
}
