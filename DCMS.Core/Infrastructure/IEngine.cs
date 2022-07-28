using Autofac;
using DCMS.Core.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;



namespace DCMS.Core.Infrastructure
{
    /// <summary>
    /// 引擎接口：实现该接口的类可以作为组成DCMS引擎的各种服务的入口。提供编辑功能、模块和实现通过此接口访问大多数DCMS功能。
    /// </summary>
    public interface IEngine
    {
        /// <summary>
        /// 添加配置服务
        /// </summary>
        /// <param name="services">服务描述符集合</param>
        /// <param name="configuration">配置应用程序</param>
        /// <param name="dcmsConfig">DCMS 配置参数</param>
        /// <returns>服务提供器</returns>
        void ConfigureServices(IServiceCollection services, IConfiguration configuration, DCMSConfig dcmsConfig, bool apiPlatform = false);

        /// <summary>
        /// 配置HTTP请求管道
        /// </summary>
        /// <param name="application">用于配置应用程序的请求管道的生成器</param>
        void ConfigureRequestPipeline(IApplicationBuilder application, bool apiPlatform = false);

        /// <summary>
        /// 解析依赖关系
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <returns></returns>
        T Resolve<T>() where T : class;

        /// <summary>
        /// 解析依赖关系
        /// </summary>
        /// <param name="type">服务类型</param>
        /// <returns></returns>
        object Resolve(Type type);

        /// <summary>
        /// 解析依赖关系
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <returns></returns>
        IEnumerable<T> ResolveAll<T>();

        /// <summary>
        /// 解析未注册的服务
        /// </summary>
        /// <param name="type">服务类型</param>
        /// <returns></returns>
        object ResolveUnregistered(Type type);

        /// <summary>
        /// 获取服务名称
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        T GetByName<T>(string name);


        /// <summary>
        /// 注册依赖
        /// </summary>
        /// <param name="containerBuilder">构建容器</param>
        /// <param name="dcmsConfig">DCMS配置参数</param>
        void RegisterDependencies(ContainerBuilder containerBuilder, DCMSConfig dcmsConfig, bool apiPlatform = false);
    }
}
