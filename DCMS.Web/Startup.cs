using Autofac;
using DCMS.Core.Configuration;
using DCMS.Core.Infrastructure;
using DCMS.Web.Framework.Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace DCMS.Web
{
	public class Startup
	{
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
			(_engine, _dcmsConfig) = services.ConfigureApplicationServices(_configuration, _webHostEnvironment, false, false);
		}

		public void ConfigureContainer(ContainerBuilder builder)
		{
			_engine.RegisterDependencies(builder, _dcmsConfig);
		}

		/// <summary>
		/// 配置应用程序http请求管道
		/// </summary>
		/// <param name="application"></param>
		public void Configure(IApplicationBuilder application)
		{

			application.UseWebSocket();

			//配置请求管道
			application.ConfigureRequestPipeline();

			//开始启动引擎
			application.StartEngine();

		}

	}
}