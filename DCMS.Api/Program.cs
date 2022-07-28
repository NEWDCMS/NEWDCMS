using Autofac.Extensions.DependencyInjection;
using DCMS.ViewModel.Models.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;



namespace DCMS.Api
{
	public class Program
	{
		public static List<ClientInfo> ClientInfoList = new List<ClientInfo>();
		public static ConcurrentDictionary<string, ClientInfo> OnlineClients { get; }


		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args)
		{
#if DEBUG
			return Host.CreateDefaultBuilder(args)
			   .UseServiceProviderFactory(new AutofacServiceProviderFactory())
			   .ConfigureWebHostDefaults(webBuilder =>
			   {
				   webBuilder
				  .UseStartup<Startup>()
				  .UseKestrel(SetHost);
			   });
#else
		  return Host.CreateDefaultBuilder(args)
			   .UseServiceProviderFactory(new AutofacServiceProviderFactory())
			   //.ConfigureAppConfiguration(builder =>
			   //{
			   //    builder.SetBasePath(GetBasePath());
			   //    builder.AddCommandLine(args);
			   //    builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
			   // })
			   .ConfigureWebHostDefaults(webBuilder =>
			   {
				   webBuilder
				  .UseStartup<Startup>()
				  .UseIISIntegration();
			   });

#endif
		}



		//private static string GetBasePath()
		//{
		//    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
		//    var isDevelopment = environment == Environments.Development;
		//    if (isDevelopment)
		//    {
		//        return Directory.GetCurrentDirectory();
		//    }
		//    using var processModule = Process.GetCurrentProcess().MainModule;
		//    return Path.GetDirectoryName(processModule?.FileName);
		//}


		/// <summary>
		/// 配置Kestrel
		/// </summary>
		/// <param name="options"></param>
		private static void SetHost(KestrelServerOptions options)
		{
			var configuration = (IConfiguration)options.ApplicationServices.GetService(typeof(IConfiguration));
			var host = configuration.GetSection("RafHost").Get<KHost>();//依据Host类反序列化appsettings.json中指定节点
			foreach (var endpointKvp in host.Endpoints)
			{
				var endpointName = endpointKvp.Key;
				var endpoint = endpointKvp.Value;//获取appsettings.json的相关配置信息
				if (!endpoint.IsEnabled)
				{
					continue;
				}

				var address = System.Net.IPAddress.Parse(endpoint.Address);
				options.Listen(address, endpoint.Port, opt =>
				{
					if (endpoint.Certificate != null)//证书不为空使用UserHttps
							{
						switch (endpoint.Certificate.Source)
						{
							case "File":
							opt.Protocols = HttpProtocols.Http1;
							opt.UseHttps(endpoint.Certificate.Path, endpoint.Certificate.Password);
							break;
							default:
							throw new NotImplementedException($"文件 {endpoint.Certificate.Source}还没有实现");
						}
					}
				});

				options.UseSystemd();
			}
		}
	}


	/// <summary>
	/// 待反序列化节点
	/// </summary>
	public class KHost
	{
		/// <summary>
		/// appsettings.json字典
		/// </summary>
		public Dictionary<string, Endpoint> Endpoints { get; set; }
	}

	/// <summary>
	/// 终结点
	/// </summary>
	public class Endpoint
	{
		/// <summary>
		/// 是否启用
		/// </summary>
		public bool IsEnabled { get; set; }

		/// <summary>
		/// ip地址
		/// </summary>
		public string Address { get; set; }

		/// <summary>
		/// 端口号
		/// </summary>
		public int Port { get; set; }

		/// <summary>
		/// 证书
		/// </summary>
		public Certificate Certificate { get; set; }
	}

	/// <summary>
	/// 证书类
	/// </summary>
	public class Certificate
	{
		/// <summary>
		/// 源
		/// </summary>
		public string Source { get; set; }

		/// <summary>
		/// 证书路径()
		/// </summary>
		public string Path { get; set; }

		/// <summary>
		/// 证书密钥
		/// </summary>
		public string Password { get; set; }
	}
}
