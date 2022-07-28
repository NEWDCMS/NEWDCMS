using DCMS.Core;
using DCMS.Core.Configuration;
using DCMS.Core.Data;
using DCMS.Core.Domain.Common;
using DCMS.Core.Infrastructure;
using DCMS.Services.Authentication;
using DCMS.Services.Logging;
using DCMS.Web.Framework.Mvc.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using WebMarkupMin.AspNetCore6;
using DCMS.Services.Chat;
using System;

namespace DCMS.Web.Framework.Infrastructure.Extensions
{
    /// <summary>
    /// IApplicationBuilder 扩展方法
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// 配置应用程序http请求管道
        /// </summary>
        /// <param name="application"></param>
        public static void ConfigureRequestPipeline(this IApplicationBuilder application, bool apiPlatform = false)
        {
            EngineContext.Current.ConfigureRequestPipeline(application, apiPlatform);
        }

        /// <summary>
        /// 启动引擎 3.0 转移到此
        /// </summary>
        /// <param name="application"></param>
        public static void StartEngine(this IApplicationBuilder application)
        {
            //var engine = EngineContext.Current;

            ////仅当安装了数据库时才执行进一步的操作
            //if (DataSettingsManager.DatabaseIsInstalled)
            //{
            //    //初始计划任务（此功能已经转移到DadaSync）
            //    //Services.Tasks.TaskManager.Instance.Initialize();
            //    //Services.Tasks.TaskManager.Instance.Start();

            //    //记录应用程序启动
            //    //engine.Resolve<ILogger>().Information("Application started");

            //    //初始化插件(DCMS 2期建设时才提供支持)
            //    //var pluginService = engine.Resolve<IPluginService>();
            //    //安装插件
            //    //pluginService.InstallPlugins();
            //    //更新插件
            //    //pluginService.UpdatePlugins();
            //}
        }


        /// <summary>
        /// 添加DCMS异常处理
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public static void UseDCMSExceptionHandler(this IApplicationBuilder application)
        {
            var dcmsConfig = EngineContext.Current.Resolve<DCMSConfig>();
            var webHostEnvironment = EngineContext.Current.Resolve<IWebHostEnvironment>();
            var useDetailedExceptionPage = dcmsConfig.DisplayFullErrorStack || webHostEnvironment.IsDevelopment();
            if (useDetailedExceptionPage)
            {
                //get detailed exceptions for developing and testing purposes
                application.UseDeveloperExceptionPage();
            }
            else
            {
                //or use special exception handler
                application.UseExceptionHandler("/Exception");
            }

            //log errors
            application.UseExceptionHandler(handler =>
            {
                handler.Run(context =>
                {
                    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                    if (exception == null)
                    {
                        return Task.CompletedTask;
                    }

                    try
                    {
                        //check whether database is installed
                        if (DataSettingsManager.DatabaseIsInstalled)
                        {
                            //get current user
                            var currentUser = EngineContext.Current.Resolve<IWorkContext>().CurrentUser;

                            //log error
                            EngineContext.Current.Resolve<ILogger>().Error(exception.Message, exception, currentUser);
                        }
                    }
                    finally
                    {
                        ExceptionDispatchInfo.Throw(exception);

                    }

                    return Task.CompletedTask;
                });
            });
        }

        /// <summary>
        /// 添加一个特殊处理程序，该处理程序检查具有404状态代码且没有正文的响应
        /// </summary>
        /// <param name="application"></param>
        public static void UsePageNotFound(this IApplicationBuilder application)
        {
            application.UseStatusCodePages(async context =>
            {
                //handle 404 Not Found
                if (context.HttpContext.Response.StatusCode == StatusCodes.Status404NotFound)
                {
                    var webHelper = EngineContext.Current.Resolve<IWebHelper>();
                    if (!webHelper.IsStaticResource())
                    {
                        //get original path and query
                        var originalPath = context.HttpContext.Request.Path;
                        var originalQueryString = context.HttpContext.Request.QueryString;

                        //store the original paths in special feature, so we can use it later
                        context.HttpContext.Features.Set<IStatusCodeReExecuteFeature>(new StatusCodeReExecuteFeature
                        {
                            OriginalPathBase = context.HttpContext.Request.PathBase.Value,
                            OriginalPath = originalPath.Value,
                            OriginalQueryString = originalQueryString.HasValue ? originalQueryString.Value : null
                        });

                        //get new path
                        context.HttpContext.Request.Path = "/page-not-found";
                        context.HttpContext.Request.QueryString = QueryString.Empty;

                        try
                        {
                            //re-execute request with new path
                            await context.Next(context.HttpContext);
                        }
                        finally
                        {
                            //return original path to request
                            context.HttpContext.Request.QueryString = originalQueryString;
                            context.HttpContext.Request.Path = originalPath;
                            context.HttpContext.Features.Set<IStatusCodeReExecuteFeature>(null);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// 添加一个特殊处理程序，用于检查具有400状态代码的响应（错误请求）
        /// </summary>
        /// <param name="application"></param>
        public static void UseBadRequestResult(this IApplicationBuilder application)
        {
            application.UseStatusCodePages(context =>
            {
                //handle 404 (Bad request)
                if (context.HttpContext.Response.StatusCode == StatusCodes.Status400BadRequest)
                {
                    var logger = EngineContext.Current.Resolve<ILogger>();
                    var workContext = EngineContext.Current.Resolve<IWorkContext>();
                    logger.Error("Error 400. Bad request", null, user: workContext.CurrentUser);
                }

                return Task.CompletedTask;
            });
        }

        /// <summary>
        /// 使用 HTTP 动态响应压缩
        /// </summary>
        /// <param name="application"></param>
        public static void UseDCMSResponseCompression(this IApplicationBuilder application)
        {
            //whether to use compression (gzip by default)
            if (DataSettingsManager.DatabaseIsInstalled && EngineContext.Current.Resolve<CommonSettings>().UseResponseCompression)
                application.UseResponseCompression();
        }

        /// <summary>
        /// 配置静态文件服务
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public static void UseDCMSStaticFiles(this IApplicationBuilder application)
        {
            void staticFileResponse(StaticFileResponseContext context)
            {
                if (!DataSettingsManager.DatabaseIsInstalled)
                {
                    return;
                }

                var commonSettings = EngineContext.Current.Resolve<CommonSettings>();
                if (!string.IsNullOrEmpty(commonSettings.StaticFilesCacheControl))
                {
                    context.Context.Response.Headers.Append(HeaderNames.CacheControl, commonSettings.StaticFilesCacheControl);
                }
            }

            var fileProvider = EngineContext.Current.Resolve<IDCMSFileProvider>();

            //common static files
            application.UseStaticFiles(new StaticFileOptions { OnPrepareResponse = staticFileResponse });

            //themes static files
            //application.UseStaticFiles(new StaticFileOptions
            //{
            //    FileProvider = new PhysicalFileProvider(fileProvider.MapPath(@"Themes")),
            //    RequestPath = new PathString("/Themes"),
            //    OnPrepareResponse = staticFileResponse
            //});

            //plugins static files
            //var staticFileOptions = new StaticFileOptions
            //{
            //    FileProvider = new PhysicalFileProvider(fileProvider.MapPath(@"Plugins")),
            //    RequestPath = new PathString("/Plugins"),
            //    OnPrepareResponse = staticFileResponse
            //};

            //if (DataSettingsManager.DatabaseIsInstalled)
            //{
            //    var securitySettings = EngineContext.Current.Resolve<SecuritySettings>();
            //    //if (!string.IsNullOrEmpty(securitySettings.PluginStaticFileExtensionsBlacklist))
            //    //{
            //    //    var fileExtensionContentTypeProvider = new FileExtensionContentTypeProvider();

            //    //    foreach (var ext in securitySettings.PluginStaticFileExtensionsBlacklist
            //    //        .Split(';', ',')
            //    //        .Select(e => e.Trim().ToLower())
            //    //        .Select(e => $"{(e.StartsWith(".") ? string.Empty : ".")}{e}")
            //    //        .Where(fileExtensionContentTypeProvider.Mappings.ContainsKey))
            //    //    {
            //    //        fileExtensionContentTypeProvider.Mappings.Remove(ext);
            //    //    }

            //    //    staticFileOptions.ContentTypeProvider = fileExtensionContentTypeProvider;
            //    //}
            //}

            //application.UseStaticFiles(staticFileOptions);

            //add support for backups
            var provider = new FileExtensionContentTypeProvider
            {
                Mappings = { [".bak"] = MimeTypes.ApplicationOctetStream }
            };

            //application.UseStaticFiles(new StaticFileOptions
            //{
            //    FileProvider = new PhysicalFileProvider(fileProvider.GetAbsolutePath("db_backups")),
            //    RequestPath = new PathString("/db_backups"),
            //    ContentTypeProvider = provider
            //});

            //add support for webmanifest files
            provider.Mappings[".webmanifest"] = MimeTypes.ApplicationManifestJson;

            //application.UseStaticFiles(new StaticFileOptions
            //{
            //    FileProvider = new PhysicalFileProvider(fileProvider.GetAbsolutePath("icons")),
            //    RequestPath = "/icons",
            //    ContentTypeProvider = provider
            //});

            //if (DataSettingsManager.DatabaseIsInstalled)
            //{
            //    application.UseStaticFiles(new StaticFileOptions
            //    {
            //        FileProvider = new RoxyFilemanProvider(fileProvider.GetAbsolutePath(DCMSRoxyFilemanDefaults.DefaultRootDirectory.TrimStart('/').Split('/'))),
            //        RequestPath = new PathString(DCMSRoxyFilemanDefaults.DefaultRootDirectory),
            //        OnPrepareResponse = staticFileResponse
            //    });
            //}
        }

        ///// <summary>
        ///// 配置中间件检查请求的页是否保持活动页
        ///// </summary>
        ///// <param name="application"></param>
        //public static void UseKeepAlive(this IApplicationBuilder application)
        //{
        //    //application.UseMiddleware<KeepAliveMiddleware>();
        //}

        ///// <summary>
        ///// 配置中间件检查是否安装了数据库
        ///// </summary>
        ///// <param name="application"></param>
        //public static void UseInstallUrl(this IApplicationBuilder application)
        //{
        //    //application.UseMiddleware<InstallUrlMiddleware>();
        //}

        /// <summary>
        /// 加身份验证中间件，以启用身份验证功能。
        /// </summary>
        /// <param name="application"></param>
        public static void UseDCMSAuthentication(this IApplicationBuilder application, bool apiPlatform = false)
        {
            if (!DataSettingsManager.DatabaseIsInstalled)
                return;

            //如果是API平台创建则使用
            if (apiPlatform)
            {
                application.UseMiddleware<JwtMiddleware>();
            }
            else
            {
                application.UseMiddleware<AuthenticationMiddleware>();
            }
        }


        ///// <summary>
        ///// 自定义配置MVC路由
        ///// </summary>
        ///// <param name="application"></param>
        //public static void UseDCMSMvc(this IApplicationBuilder application)
        //{
        //    //跨域允许
        //    application.UseCors(options =>
        //    {
        //        options.AllowAnyHeader();
        //        options.AllowAnyMethod();
        //        options.AllowAnyOrigin();
        //        options.AllowCredentials();
        //    });

        //    //application.UseMvc(routeBuilder =>
        //    //{
        //    //    //register all routes
        //    //    EngineContext.Current.Resolve<IRoutePublisher>().RegisterRoutes(routeBuilder);
        //    //});
        //}

        ///// <summary>
        ///// 使用终结点路由机制
        ///// </summary>
        ///// <param name="app"></param>
        ///// <param name="configureRoutes"></param>
        ///// <returns></returns>
        //public static IApplicationBuilder UseDCMSMvcEndpointRoute(this IApplicationBuilder app, Action<IRouteBuilder> configureRoutes)
        //{
        //    //此处各种验证，略。。
        //    var options = app.ApplicationServices.GetRequiredService<IOptions<MvcOptions>>();
        //    if (options.Value.EnableEndpointRouting)
        //    {
        //        var mvcEndpointDataSource = app.ApplicationServices
        //            .GetRequiredService<IEnumerable<EndpointDataSource>>()
        //            .OfType<MvcEndpointDataSource>()
        //            .First();
        //        var parameterPolicyFactory = app.ApplicationServices
        //            .GetRequiredService<ParameterPolicyFactory>();

        //        var endpointRouteBuilder = new EndpointRouteBuilder(app);

        //        configureRoutes(endpointRouteBuilder);

        //        foreach (var router in endpointRouteBuilder.Routes)
        //        {
        //            // Only accept Microsoft.AspNetCore.Routing.Route when converting to endpoint
        //            // Sub-types could have additional customization that we can't knowingly convert
        //            if (router is Route route && router.GetType() == typeof(Route))
        //            {
        //                var endpointInfo = new MvcEndpointInfo(
        //                    route.Name,
        //                    route.RouteTemplate,
        //                    route.Defaults,
        //                    route.Constraints.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value),
        //                    route.DataTokens,
        //                    parameterPolicyFactory);
        //                mvcEndpointDataSource.ConventionalEndpointInfos.Add(endpointInfo);
        //            }
        //            else
        //            {
        //                throw new InvalidOperationException($"Cannot use '{router.GetType().FullName}' with Endpoint Routing.");
        //            }
        //        }
        //        if (!app.Properties.TryGetValue(EndpointRoutingRegisteredKey, out _))
        //        {
        //            // Matching middleware has not been registered yet
        //            // For back-compat register middleware so an endpoint is matched and then immediately used
        //            app.UseEndpointRouting();
        //        }
        //        return app.UseEndpoint();
        //    }
        //    else
        //    {
        //        //旧版路由方案
        //        services.AddMvc(option => option.EnableEndpointRouting = false).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        //    }
        //}


        /// <summary>
        /// 使用终结点路由机制
        /// </summary>
        /// <param name="application"></param>
        public static void UseDCMSEndpoints(this IApplicationBuilder application)
        {
            application.UseRouting();
            application.UseEndpoints(endpoints =>
            {
                EngineContext.Current.Resolve<IRoutePublisher>().RegisterRoutes(endpoints);
            });
        }


        /// <summary>
        /// 配置WebMarkupmin
        /// </summary>
        /// <param name="application"></param>
        public static void UseDCMSWebMarkupMin(this IApplicationBuilder application)
        {
            if (!DataSettingsManager.DatabaseIsInstalled)
                return;

            application.UseWebMarkupMin();
        }


        public static void UseWebSocket(this IApplicationBuilder application)
        {
            //WebSocket中间件
            var webSocketOptions = new WebSocketOptions()
            {
                //KeepAliveInterval - 向客户端发送“ping”帧的频率，以确保代理保持连接处于打开状态。 默认值为 2 分钟。
                KeepAliveInterval = TimeSpan.FromSeconds(10),
                //ReceiveBufferSize - 用于接收数据的缓冲区的大小。 高级用户可能需要对其进行更改，以便根据数据大小调整性能。 默认值为 4 KB。
                //ReceiveBufferSize = 4 * 1024
            };

            application.UseWebSockets(webSocketOptions);
            application.UseMiddleware<WebSocketMiddleware>();
        }
    }
}
