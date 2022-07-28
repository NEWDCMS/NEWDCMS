using Autofac;
using DCMS.Api.Configuration;
using DCMS.Core.Configuration;
using DCMS.Core.Infrastructure;
using DCMS.Web.Framework.Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace DCMS.Api
{
    /// <summary>
    /// 配置
    /// </summary>
    public class Startup
    {

        readonly string MyAllowSpecificOrigins = "any";
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private IEngine _engine;
        private DCMSConfig _dcmsConfig;

        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
        }


        /// <summary>
        /// 将服务添加到应用程序并配置服务提供程序
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            _webHostEnvironment.WebRootPath = _webHostEnvironment.ContentRootPath;
            (_engine, _dcmsConfig) = services.ConfigureApplicationServices(_configuration, _webHostEnvironment, true, false);

            //跨域支持
            services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins, builder =>
                 {
                       //builder.WithOrigins("http://jsdcms.com", "https://jsdcms.com", "https://www.jsdcms.com","https://www.jsdcms.com");

                       builder.AllowAnyOrigin()
                 .AllowAnyMethod()
                 .AllowAnyHeader();

             });
            });



            //添加SignalR服务
            services.AddSignalR(opt =>
            {
                opt.EnableDetailedErrors = true;
                opt.MaximumReceiveMessageSize = 10000000000;
            });

            SwaggerConfiguration.ConfigureService(services);
        }


        public void ConfigureContainer(ContainerBuilder builder)
        {
            _engine.RegisterDependencies(builder, _dcmsConfig, true);
        }


        /// <summary>
        /// 配置应用程序http请求管道
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //配置请求管道
            app.ConfigureRequestPipeline(true);

            //app.UseCors(MyAllowSpecificOrigins);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //配置和使用这个WebSocket中间件
            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(30),
                ReceiveBufferSize = 4 * 1024
            };
            app.UseWebSockets(webSocketOptions);

            //UseSignalR
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ChatHub>("/chatHub");
            });

            SwaggerConfiguration.Configure(app, MyAllowSpecificOrigins);

            //开始启动引擎
            app.StartEngine();
        }
    }
}