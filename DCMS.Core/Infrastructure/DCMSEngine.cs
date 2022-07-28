using Autofac;
using Autofac.Core.Lifetime;
using AutoMapper;
using DCMS.Core.Configuration;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Core.Infrastructure.Mapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;


namespace DCMS.Core.Infrastructure
{
    /// <summary>
    /// 表示DCMS框架引擎
    /// </summary>
    public class DCMSEngine : IEngine
    {
        /// <summary>
        /// 服务提供器接口
        /// </summary>
        public virtual IServiceProvider ServiceProvider => _serviceProvider;

        private ITypeFinder _typeFinder;

        /// <summary>
        /// 容器接口
        /// </summary>
        private ContainerBuilder _container;


        /// <summary>
        /// 服务提供器接口
        /// </summary>
        private IServiceProvider _serviceProvider { get; set; }



        /// <summary>
        /// 获取服务提供器 3.1
        /// </summary>
        /// <returns>IServiceProvider</returns>
        protected IServiceProvider GetServiceProvider()
        {
            var accessor = ServiceProvider?.GetService<IHttpContextAccessor>();
            var context = accessor?.HttpContext;
            return context?.RequestServices ?? ServiceProvider;
        }

        /// <summary>
        /// 运行启动任务
        /// </summary>
        /// <param name="typeFinder">Type finder</param>
        protected virtual void RunStartupTasks(ITypeFinder typeFinder)
        {
            //查找其他程序集提供的启动任务
            var startupTasks = typeFinder.FindClassesOfType<IStartupTask>();

            //创建和排序启动任务的实例时启动这个接口，即使没有安装插件
            //否则，DbContext初始化器将无法运行，插件安装也将无法工作
            var instances = startupTasks
                .Select(startupTask => (IStartupTask)Activator.CreateInstance(startupTask))
                .OrderBy(startupTask => startupTask.Order);

            //执行任务
            foreach (var task in instances)
            {
                task.Execute();
            }
        }

        ///// <summary>
        ///// 注册依赖 2.2
        ///// </summary>
        ///// <param name="services"></param>
        ///// <param name="typeFinder"></param>
        ///// <param name="dcmsConfig"></param>
        ///// <returns></returns>
        //protected virtual IServiceProvider RegisterDependencies(IServiceCollection services, ITypeFinder typeFinder, DCMSConfig dcmsConfig)
        //{
        //    //实例化Autofac容器
        //    var containerBuilder = new ContainerBuilder();

        //    //注册引擎
        //    containerBuilder.RegisterInstance(this).As<IEngine>().SingleInstance();

        //    //注册类型查找器
        //    containerBuilder.RegisterInstance(typeFinder).As<ITypeFinder>().SingleInstance();

        //    //将collection中的服务填充到Autofac
        //    //populate Autofac container builder with the set of registered service descriptors
        //    containerBuilder.Populate(services);

        //    //查找其他程序集提供的依赖项注册器
        //    var dependencyRegistrars = typeFinder.FindClassesOfType<IDependencyRegistrar>();

        //    //创建和排序依赖关系注册器的实例
        //    var instances = dependencyRegistrars
        //        .Select(dependencyRegistrar => (IDependencyRegistrar)Activator.CreateInstance(dependencyRegistrar))
        //        .OrderBy(dependencyRegistrar => dependencyRegistrar.Order);

        //    //register all provided dependencies
        //    foreach (var dependencyRegistrar in instances)
        //    {
        //        dependencyRegistrar.Register(containerBuilder, typeFinder, dcmsConfig);
        //    }

        //    //第三方容器接管Core内置的DI容器
        //    //create service provider
        //    _container = containerBuilder.Build();

        //    _serviceProvider = new AutofacServiceProvider(_container);

        //    return _serviceProvider;
        //}

        /// <summary>
        /// 注册依赖 3.1
        /// </summary>
        /// <param name="containerBuilder"></param>
        /// <param name="dcmsConfig"></param>
        public virtual void RegisterDependencies(ContainerBuilder containerBuilder, DCMSConfig dcmsConfig, bool apiPlatform = false)
        {
            //注册引擎
            containerBuilder.RegisterInstance(this).As<IEngine>().SingleInstance();

            //注册类型查找器
            containerBuilder.RegisterInstance(_typeFinder).As<ITypeFinder>().SingleInstance();

            //查找其他程序集提供的依赖项注册器
            var dependencyRegistrars = _typeFinder.FindClassesOfType<IDependencyRegistrar>();

            //创建和排序依赖关系注册器的实例
            var instances = dependencyRegistrars
                .Select(dependencyRegistrar => (IDependencyRegistrar)Activator.CreateInstance(dependencyRegistrar))
                .OrderBy(dependencyRegistrar => dependencyRegistrar.Order);

            //注册所有提供的依赖项
            foreach (var dependencyRegistrar in instances)
            {
                dependencyRegistrar.Register(containerBuilder, _typeFinder, dcmsConfig, apiPlatform);
            }

            //DI容器
            //_container = containerBuilder.Build();
            _container = containerBuilder;
        }


        /// <summary>
        /// 注册配置AutoMapper
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="typeFinder">Type finder</param>
        protected virtual void AddAutoMapper(IServiceCollection services, ITypeFinder typeFinder)
        {
            //查找其他程序集提供的映射器配置
            var mapperConfigurations = typeFinder.FindClassesOfType<IOrderedMapperProfile>();

            //创建映射器配置的实例并对其排序
            var instances = mapperConfigurations
                .Select(mapperConfiguration => (IOrderedMapperProfile)Activator.CreateInstance(mapperConfiguration))
                .OrderBy(mapperConfiguration => mapperConfiguration.Order);

            //创建配置
            var config = new MapperConfiguration(cfg =>
            {
                foreach (var instance in instances)
                {
                    cfg.AddProfile(instance.GetType());
                }
            });

            //注册
            AutoMapperConfiguration.Init(config);
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            //check for assembly already loaded
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
            if (assembly != null)
            {
                return assembly;
            }

            //get assembly from TypeFinder
            var tf = Resolve<ITypeFinder>();
            assembly = tf?.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
            return assembly;
        }



        #region Methods

        ///// <summary>
        ///// 添加并配置服务 2.2
        ///// </summary>
        ///// <param name="services"></param>
        ///// <param name="configuration"></param>
        ///// <param name="dcmsConfig"></param>
        ///// <returns></returns>
        //public IServiceProvider ConfigureServices(IServiceCollection services, IConfiguration configuration, DCMSConfig dcmsConfig)
        //{
        //    //查找其他程序集提供的启动配置
        //    var typeFinder = new WebAppTypeFinder();
        //    var startupConfigurations = typeFinder.FindClassesOfType<IDCMSStartup>();

        //    //按优先级执行
        //    //create and sort instances of startup configurations
        //    var instances = startupConfigurations
        //        .Select(startup => (IDCMSStartup)Activator.CreateInstance(startup))
        //        .OrderBy(startup => startup.Order);

        //    //配置服务
        //    /*
        //     * 注意：这里的执行优先级是：
        //     * ErrorHandlerStartup(0) > DCMSDbStartup(>=10) > DCMSCommonStartup(>=100) > AuthenticationStartup(>=500) > DCMSMvcStartup(>=1000)
        //     */
        //    foreach (var instance in instances)
        //    {
        //        instance.ConfigureServices(services, configuration);
        //    }

        //     //注册AutoMapper映射配置
        //    AddAutoMapper(services, typeFinder);

        //     //依赖服务
        //    RegisterDependencies(services, typeFinder, dcmsConfig);

        //    //启动计划任务
        //    RunStartupTasks(typeFinder);

        //    //在此处解析程序集。否则，插件在呈现视图时会抛出异常
        //    AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

        //    return _serviceProvider;
        //}

        /// <summary>
        /// 添加并配置服务 3.1
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="nopConfig"></param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration, DCMSConfig nopConfig, bool apiPlatform = false)
        {
            //查找其他程序集提供的启动配置
            _typeFinder = new WebAppTypeFinder();
            var startupConfigurations = _typeFinder.FindClassesOfType<IDCMSStartup>();

            //按优先级执行
            var instances = startupConfigurations
                .Select(startup => (IDCMSStartup)Activator.CreateInstance(startup))
                .OrderBy(startup => startup.Order);

            //配置服务
            foreach (var instance in instances)
            {
                instance.ConfigureServices(services, configuration, apiPlatform);
            }

            //注册AutoMapper映射配置
            AddAutoMapper(services, _typeFinder);

            //启动计划任务
            //RunStartupTasks(_typeFinder);

            //在此处解析程序集。否则，插件在呈现视图时会抛出异常
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }


        /// <summary>
        /// 配置HTTP请求管道
        /// </summary>
        /// <param name="application"></param>
        public void ConfigureRequestPipeline(IApplicationBuilder application, bool apiPlatform = false)
        {
            //转移：3.1 _serviceProvider 提供
            _serviceProvider = application.ApplicationServices;

            //查找其他程序集提供的启动配置
            var typeFinder = Resolve<ITypeFinder>();
            var startupConfigurations = typeFinder.FindClassesOfType<IDCMSStartup>();

            //创建和排序启动配置的实例
            var instances = startupConfigurations
                .Select(startup => (IDCMSStartup)Activator.CreateInstance(startup))
                .OrderBy(startup => startup.Order);

            //配置请求管道
            foreach (var instance in instances)
            {
                instance.Configure(application, apiPlatform);
            }
        }


        /// <summary>
        /// 解析依赖
        /// </summary>
        /// <typeparam name="T">Type of resolved service</typeparam>
        /// <returns>Resolved service</returns>
        public T Resolve<T>() where T : class
        {
            return (T)Resolve(typeof(T));
        }

        /// <summary>
        /// 解析依赖
        /// </summary>
        /// <param name="type">Type of resolved service</param>
        /// <returns>Resolved service</returns>
        public object Resolve(Type type)
        {
            return GetServiceProvider()?.GetService(type);
        }

        /// <summary>
        /// 解析依赖
        /// </summary>
        /// <typeparam name="T">Type of resolved services</typeparam>
        /// <returns>Collection of resolved services</returns>
        public virtual IEnumerable<T> ResolveAll<T>()
        {
            return (IEnumerable<T>)GetServiceProvider()?.GetServices(typeof(T));
        }

        /// <summary>
        /// 解析未注册的服务
        /// </summary>
        /// <param name="type">Type of service</param>
        /// <returns>Resolved service</returns>
        public virtual object ResolveUnregistered(Type type)
        {
            Exception innerException = null;
            foreach (var constructor in type.GetConstructors())
            {
                try
                {
                    //try to resolve constructor parameters
                    var parameters = constructor.GetParameters().Select(parameter =>
                    {
                        var service = Resolve(parameter.ParameterType);
                        if (service == null)
                        {
                            throw new DCMSException("Unknown dependency");
                        }

                        return service;
                    });

                    //all is ok, so create instance
                    return Activator.CreateInstance(type, parameters.ToArray());
                }
                catch (Exception ex)
                {
                    innerException = ex;
                }
            }

            throw new DCMSException("No constructor was found that had all the dependencies satisfied.", innerException);
        }



        public T GetByName<T>(string name)
        {
            return Scope().ResolveNamed<T>(name);
            ////ContainerBuilder builder = new ContainerBuilder();
            ////builder.RegisterType<TodayWriter>().Named<IDateWriter>(typeof(TodayWriter).Name);
            ////var container = builder.Build();
            ////IDateWriter today = container.ResolveNamed<T>(typeof(TodayWriter).Name)
            //return Container.ResolveNamed<T>(name);
        }



        //public ILifetimeScope Scope()
        //{
        //    try
        //    {
        //        //AutofacDependencyResolver.Current.RequestLifetimeScope.ResolveNamed<T>(name)
        //        return AutofacRequestLifetimeHttpModule.GetLifetimeScope(Container, null);
        //    }
        //    catch
        //    {
        //        return Container;
        //    }
        //}


        public ILifetimeScope Scope()
        {
            try
            {
                //AutofacDependencyResolver.Current.RequestLifetimeScope.ResolveNamed<T>(name)
                //when such lifetime scope is returned, you should be sure that it'll be disposed once used (e.g. in schedule tasks)
                return _container?.Build().BeginLifetimeScope();
            }
            catch (Exception)
            {
                //we can get an exception here if RequestLifetimeScope is already disposed
                //for example, requested in or after "Application_EndRequest" handler
                //but note that usually it should never happen

                //when such lifetime scope is returned, you should be sure that it'll be disposed once used (e.g. in schedule tasks)
                return _container?.Build().BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);
            }
        }

        #endregion

    }


    /// <summary>
    ///  ss = DeepClon<Student, StudentSecond>.Trans(s);
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    public static class DeepClon<TIn, TOut>
    {

        private static readonly Func<TIn, TOut> cache = GetFunc();
        private static Func<TIn, TOut> GetFunc()
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(TIn), "p");
            List<MemberBinding> memberBindingList = new List<MemberBinding>();

            foreach (var item in typeof(TOut).GetProperties())
            {
                if (!item.CanWrite)
                {
                    continue;
                }

                MemberExpression property = Expression.Property(parameterExpression, typeof(TIn).GetProperty(item.Name));
                MemberBinding memberBinding = Expression.Bind(item, property);
                memberBindingList.Add(memberBinding);
            }

            MemberInitExpression memberInitExpression = Expression.MemberInit(Expression.New(typeof(TOut)), memberBindingList.ToArray());
            Expression<Func<TIn, TOut>> lambda = Expression.Lambda<Func<TIn, TOut>>(memberInitExpression, new ParameterExpression[] { parameterExpression });

            return lambda.Compile();
        }

        public static TOut Trans(TIn tIn)
        {
            return cache(tIn);
        }

    }
}

