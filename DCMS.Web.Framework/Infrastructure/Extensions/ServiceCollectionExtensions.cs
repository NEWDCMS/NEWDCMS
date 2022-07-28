using Autofac;
using DCMS.Core;
using DCMS.Core.Configuration;
using DCMS.Core.Data;
using DCMS.Core.Domain.Common;
using DCMS.Core.Domain.Security;
using DCMS.Core.Http;
using DCMS.Core.Infrastructure;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Core.UrlFirewall;
using DCMS.Data;
using DCMS.Services.Authentication;
using DCMS.Services.Authentication.External;
using DCMS.Services.Common;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Framework.Mvc.ModelBinding;
using DCMS.Web.Framework.Mvc.Routing;
using DCMS.Web.Framework.Security.Captcha;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StackExchange.Profiling.Storage;
using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebMarkupMin.AspNetCore6;
using WebMarkupMin.NUglify;


namespace DCMS.Web.Framework.Infrastructure.Extensions
{
	/// <summary>
	/// 服务配置扩展（核心配置）
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// 将服务添加到应用程序并配置服务提供程序
		/// </summary>
		/// <param name="services"></param>
		/// <param name="configuration"></param>
		/// <param name="hostingEnvironment"></param>
		/// <param name="apiPlatform">是否API平台</param>
		/// <param name="isManagePlatform">是否管理平台</param>
		/// <returns>配置的服务提供器</returns>
		public static (IEngine, DCMSConfig) ConfigureApplicationServices(this IServiceCollection services,
			IConfiguration configuration,
			IWebHostEnvironment hostingEnvironment,
			bool apiPlatform = false,
			bool isManagePlatform = false)
		{

			//当前大多数api提供者都需要遵循tls 1.2安全协议
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

			//添加DCMSConfig配置
			var dcmsConfig = services.ConfigureStartupConfig<DCMSConfig>(configuration.GetSection("DCMS"));
			//如果是从管理平台创建
			if (isManagePlatform)
			{
				//默认标识码为“SYSTEM”
				dcmsConfig.ManageStoreCode = "SYSTEM";
			}

			//添加主机配置参数
			services.ConfigureStartupConfig<HostingConfig>(configuration.GetSection("Hosting"));

			services.Configure<IISServerOptions>(options =>
			{
				options.MaxRequestBodySize = 52428800;
			});

			//表单最大长度限制
			services.Configure<FormOptions>(options =>
			{
				options.ValueCountLimit = 5000;
				options.ValueLengthLimit = 1024 * 1024 * 100;
				options.MultipartBodyLengthLimit = int.MaxValue;
			});

			//URL路由全小写
			services.AddRouting(options => options.LowercaseUrls = true);

			//API 注册Accessor
			if (apiPlatform)
			{
				services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
				services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

				////Chat服务
				//services.AddScoped<IUserService, UserService>();
				//services.AddScoped<IChatService, ChatService>();
				//services.AddSingleton<IConnectionService, ConnectionService>();
				//services.AddDbContext<ChatDB>(opt => opt.UseSqlite(@"Data Source=D:\Git\DCMS.Studio.Core\DCMS.Core\DCMS.Api\ChatDB.db"), ServiceLifetime.Singleton);
				//services.AddSingleton<ChatRepository>();

				services.AddHttpsRedirection(opt =>
				{
					opt.RedirectStatusCode = StatusCodes.Status301MovedPermanently;
					opt.HttpsPort = 443;
				});
			}
			else
			{
				//将访问器添加到HttpContext
				services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
				services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
		 
			}

			services.AddHsts(options =>
			{
				options.Preload = true;
				options.IncludeSubDomains = true;
				options.MaxAge = TimeSpan.FromDays(365);
			});

			//创建默认文件提供器(如：API 不提供文件操作)
			if (!string.IsNullOrEmpty(hostingEnvironment.WebRootPath) || !apiPlatform)
			{
				CommonHelper.DefaultFileProvider = new DCMSFileProvider(hostingEnvironment);
			}

			//添加AddMvcCore
			var mvcCoreBuilder = services.AddMvcCore();

			// 初始插件
			// mvcCoreBuilder.PartManager.InitializePlugins(nopConfig);

			//创建配置引擎服务
			var engine = EngineContext.Create();

			engine.ConfigureServices(services, configuration, dcmsConfig, apiPlatform);

			return (engine, dcmsConfig);
		}

		/// <summary>
		/// 从指定的配置参数，创建、绑定注册服务
		/// </summary>
		/// <typeparam name="TConfig">配置参数</typeparam>
		/// <param name="services">Collection of service descriptors</param>
		/// <param name="configuration">Set of key/value application configuration properties</param>
		/// <param name="services"></param>
		public static TConfig ConfigureStartupConfig<TConfig>(this IServiceCollection services, IConfiguration configuration) where TConfig : class, new()
		{
			if (services == null)
			{
				throw new ArgumentNullException(nameof(services));
			}

			if (configuration == null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}

			//创建配置实例
			var config = new TConfig();

			//将其绑定到配置的适当部分
			configuration.Bind(config);

			//配置为单例服务
			services.AddSingleton(config);

			return config;
		}

		/// <summary>
		/// 注册HTTP 上下文访问器
		/// </summary>
		/// <param name="services"></param>
		public static void AddHttpContextAccessor(this IServiceCollection services)
		{
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
		}

		/// <summary>
		/// 添加防伪支持所需的服务
		/// </summary>
		/// <param name="services"></param>
		public static void AddAntiForgery(this IServiceCollection services)
		{
			//override cookie name
			services.AddAntiforgery(options =>
			{
				options.Cookie.Name = $"{DCMSCookieDefaults.Prefix}{DCMSCookieDefaults.AntiforgeryCookie}";
				//whether to allow the use of anti-forgery cookies from SSL protected page on the other store pages which are not
				options.Cookie.SecurePolicy = DataSettingsManager.DatabaseIsInstalled && EngineContext.Current.Resolve<SecuritySettings>().ForceSslForAllPages
					? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.None;
			});
		}

		/// <summary>
		/// 添加服务请求会话状态
		/// </summary>
		/// <param name="services"></param>
		public static void AddHttpSession(this IServiceCollection services)
		{
			services.AddSession(options =>
			{
				options.Cookie.Name = $"{DCMSCookieDefaults.Prefix}{DCMSCookieDefaults.SessionCookie}";
				options.Cookie.HttpOnly = true;

				//whether to allow the use of session values from SSL protected page on the other store pages which are not
				options.Cookie.SecurePolicy = DataSettingsManager.DatabaseIsInstalled && EngineContext.Current.Resolve<SecuritySettings>().ForceSslForAllPages
					? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.None;
			});
		}


		/// <summary>
		/// 添加身份认证服务
		/// </summary>
		/// <param name="services"></param>
		public static void AddDCMSAuthentication(this IServiceCollection services, bool apiPlatform = false)
		{
			var _configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
			//使用JWT身份验证
			if (apiPlatform)
			{

				//创建基于策略的授权
				services.AddAuthorization(options =>
				{
					options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy =>
					{
						policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
						policy.RequireClaim(ClaimTypes.NameIdentifier);
					});
				});


				var authenticationBuilder = services.AddAuthentication(options =>
				{
					options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
					options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
					options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				});

				authenticationBuilder.AddJwtBearer(cfg =>
				{
					cfg.SaveToken = true;
					cfg.RequireHttpsMetadata = false;
					cfg.TokenValidationParameters = new TokenValidationParameters
					{
						ValidIssuer = _configuration["DCMS:JWT:Issuer"],
						ValidAudience = _configuration["DCMS:JWT:Issuer"],
						ValidateLifetime = true,
						RequireExpirationTime = true,
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["DCMS:JWT:Secret"])),
						ClockSkew = TimeSpan.Zero
					};

					cfg.Events = new JwtBearerEvents()
					{
						// 当令牌验证因验证参数而失败时调用
						OnAuthenticationFailed = (context) =>
						{
							int x = context.Response.StatusCode;
							return Task.CompletedTask;
						},

						//请求到达时调用
						OnMessageReceived =  (context) =>
						{
							int x = context.Response.StatusCode;
							return Task.CompletedTask;
						},

						// 当响应为401未授权时调用
						OnChallenge =  (context) =>
						{
							int x = context.Response.StatusCode;
							return Task.CompletedTask;
						},

						// 验证令牌后调用
						OnTokenValidated =  (context) =>
						{
							int x = context.Response.StatusCode;
							return Task.CompletedTask;
						},
					};
				});


				//配置外部身份验证插件
				var typeFinder = new WebAppTypeFinder();
				var externalAuthConfigurations = typeFinder.FindClassesOfType<IExternalAuthenticationRegistrar>();
				var externalAuthInstances = externalAuthConfigurations
					.Select(x => (IExternalAuthenticationRegistrar)Activator.CreateInstance(x));

				foreach (var instance in externalAuthInstances)
				{
					instance.Configure(authenticationBuilder);
				}
			}
			//使用cookie身份验证
			else
			{
				//设置默认身份验证方案
				var authenticationBuilder = services.AddAuthentication(options =>
				{
					options.DefaultChallengeScheme = DCMSAuthenticationDefaults.AuthenticationScheme;
					options.DefaultScheme = DCMSAuthenticationDefaults.AuthenticationScheme;
					options.DefaultSignInScheme = DCMSAuthenticationDefaults.ExternalAuthenticationScheme;
				});

				//添加主cookie身份验证
				authenticationBuilder.AddCookie(DCMSAuthenticationDefaults.AuthenticationScheme, options =>
				{
					options.Cookie.Name = $"{DCMSCookieDefaults.Prefix}{DCMSCookieDefaults.AuthenticationCookie}";
					options.Cookie.HttpOnly = true;
					//登录页
					options.LoginPath = DCMSAuthenticationDefaults.LoginPath;
					//拒绝授权页
					options.AccessDeniedPath = DCMSAuthenticationDefaults.AccessDeniedPath;
					//是否允许使用其他存储页上受SSL保护的页中的身份验证Cookie
					options.Cookie.SecurePolicy = DataSettingsManager.DatabaseIsInstalled && EngineContext.Current.Resolve<SecuritySettings>().ForceSslForAllPages ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.None;
				});

				//添加外部身份验证
				authenticationBuilder.AddCookie(DCMSAuthenticationDefaults.ExternalAuthenticationScheme, options =>
				{
					options.Cookie.Name = $"{DCMSCookieDefaults.Prefix}{DCMSCookieDefaults.ExternalAuthenticationCookie}";
					options.Cookie.HttpOnly = true;
				//登录页
				options.LoginPath = DCMSAuthenticationDefaults.LoginPath;
				//拒绝授权页
				options.AccessDeniedPath = DCMSAuthenticationDefaults.AccessDeniedPath;

				//是否允许使用其他存储页上受SSL保护的页中的身份验证Cookie
				options.Cookie.SecurePolicy = DataSettingsManager.DatabaseIsInstalled && EngineContext.Current.Resolve<SecuritySettings>().ForceSslForAllPages ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.None;
				});


				//配置外部身份验证插件
				var typeFinder = new WebAppTypeFinder();
				var externalAuthConfigurations = typeFinder.FindClassesOfType<IExternalAuthenticationRegistrar>();
				var externalAuthInstances = externalAuthConfigurations
					.Select(x => (IExternalAuthenticationRegistrar)Activator.CreateInstance(x));

				foreach (var instance in externalAuthInstances)
				{
					instance.Configure(authenticationBuilder);
				}
			}
		}

		/// <summary>
		/// 添加和配置mvc
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IMvcBuilder AddDCMSMvc(this IServiceCollection services)
		{
			//添加基本MVC功能
			var mvcBuilder = services.AddControllersWithViews();

			//配置Microsoft.Extensions.DependencyInjection.IMvcBuilder以支持Razor视图和Razor页面的运行时编译
			mvcBuilder.AddRazorRuntimeCompilation();

			//AllowRecompilingViewsOnFileChange 
			//services.AddRazorPages()
			//    .AddRazorRuntimeCompilation();


			//启用终结点路由，可以提升系统RPS
			//services.AddMvc(option => option.EnableEndpointRouting = true).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

			//该值确定路由是否应该在内部使用端点，或者是否应该使用旧版路由逻辑。
			//端点路由用于将HTTP请求与MVC操作进行匹配，并使用IUrlHelper生成URL 
			mvcBuilder.AddMvcOptions(options =>
			{
				options.EnableEndpointRouting = false;
				//添加全局异常处理机制
				options.Filters.Add<GlobalExceptionFilter>();
			});


			//appsettings 配置
			var dcmsConfig = services.BuildServiceProvider().GetRequiredService<DCMSConfig>();
			if (dcmsConfig.UseSessionStateTempDataProvider)
			{
				//使用基于cookie的临时数据提供器
				mvcBuilder.AddSessionStateTempDataProvider();
			}
			else
			{

				//使用基于cookie的临时数据提供器
				mvcBuilder.AddCookieTempDataProvider(options =>
				{
					options.Cookie.Name = $"{DCMSCookieDefaults.Prefix}{DCMSCookieDefaults.TempDataCookie}";

					//是否允许使用其他存储页上受SSL保护的页中的身份验证Cookie
					options.Cookie.SecurePolicy = DataSettingsManager.DatabaseIsInstalled && EngineContext.Current.Resolve<SecuritySettings>().ForceSslForAllPages ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.None;
				});
			}

			//使用RazorPages
			services.AddRazorPages();

			//添加JSON 序列化解析支持
			//3.1
			mvcBuilder.AddNewtonsoftJson(options =>
			{
				//忽略空值
				options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
				//忽略列循环引用
				options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
				//不使用驼峰样式的key
				options.SerializerSettings.ContractResolver = new DefaultContractResolver();
				//设置时间格式
				//options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
			});


			if (dcmsConfig.CustomModelBindEnabled)
			{
				//添加自定义元数据提供器
				mvcBuilder.AddMvcOptions(options => options.ModelMetadataDetailsProviders.Add(new DCMSMetadataProvider()));
				//添加自定义模型绑定器
				mvcBuilder.AddMvcOptions(options => options.ModelBinderProviders.Insert(0, new DCMSModelBinderProvider()));
			}


			//添加 FluentValidation 验证
			mvcBuilder.AddFluentValidation(configuration =>
			{
				//从DCMS程序集中注册所有可用的验证程序
				var assemblies = mvcBuilder.PartManager.ApplicationParts
					.OfType<AssemblyPart>()
					.Where(part => part.Name.StartsWith("DCMS", StringComparison.InvariantCultureIgnoreCase))
					.Select(part => part.Assembly);
				configuration.RegisterValidatorsFromAssemblies(assemblies);

				//隐式自动验证子属性
				configuration.ImplicitlyValidateChildProperties = true;
			});

			//将控制器注册为服务，将允许重写它们
			mvcBuilder.AddControllersAsServices();

			return mvcBuilder;
		}

		/// <summary>
		/// 注册自定义重定向结果执行器
		/// </summary>
		/// <param name="services"></param>
		public static void AddDCMSRedirectResultExecutor(this IServiceCollection services)
		{
			//允许在重定向url中使用非ascii字符
			services.AddSingleton<IActionResultExecutor<RedirectResult>, DCMSRedirectResultExecutor>();
		}


		///====================================================
		///


		/// <summary>
		/// 注册对象上下文
		/// </summary>
		/// <param name="services"></param>
		public static void AddDCMSObjectContext<T>(this IServiceCollection services) where T : DbContextBase
		{
			//services.AddDbContextPool<DCMSObjectContext>(optionsBuilder =>
			//{
			//    //延迟加载数据库
			//    //optionsBuilder.UseSqlServerWithLazyLoading(services);
			//    optionsBuilder.UseMySqlServerWithLazyLoading(services);
			//});

			//DataSettingsManager 已弃用
			//var dcmsConfig = services.BuildServiceProvider().GetRequiredService<DCMSConfig>();
			//var dataSettings = DataSettingsManager.LoadSettings();
			//if (dataSettings.DbConnections.Count == 0)
			//    return;

			//注册配置信息
			var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
			services.AddSingleton(config).AddSingleton<IConfiguration>(config);

			//注册配置服务
			services.AddOptions();
			services.TryAddSingleton<IConfigureOptions<DCMSOptions>, DCMSOptionsSetup>();
			services.TryAddScoped<IDbProvider, DbProvider>();
			services.TryAddSingleton<IUnitOfWorkFactory, UnitOfWorkFactory>();
			services.TryAddScoped<IRepositoryFactory, RepositoryFactory>();

			//使用Mysql使用者
			services.TryAddSingleton<IDbContextOptionsBuilderUser, MySqlDbContextOptionsBuilderUser>();

			//添加库,注册上下文
			var dbs = new string[] { "AUTH", "DCMS", "Census", "SKD", "TSS", "CRM", "OCMS" ,"CSMS"};
			foreach (var db in dbs)
			{
				services.AddSingleton(new DOBOptions(new DbContextOptionsBuilder<T>(), $"{db}_RO", typeof(T)));
				services.AddSingleton(new DOBOptions(new DbContextOptionsBuilder<T>(), $"{db}_RW", typeof(T)));
			}

			services.TryAddSingleton<IServiceGetter, ServiceGetter>();

		}

		///// <summary>
		///// 添加DCMS 配置（方案2）
		///// </summary>
		///// <param name="serviceCollection"></param>
		///// <returns></returns>
		//public static IServiceCollection AddDCMS(this IServiceCollection services)
		//{
		//    services.AddOptions();
		//    services.TryAddSingleton<IConfigureOptions<DCMSOptions>, DCMSOptionsSetup>();
		//    return services;
		//}


		///// <summary>
		///// 添加框架对数据库的支持（方案2）
		///// </summary>
		///// <param name="services"></param>
		///// <returns></returns>
		//public static IServiceCollection AddDCMSDatabase(this IServiceCollection services)
		//{
		//    services.TryAddScoped<IDBProvider, DBProvider>();
		//    services.TryAddSingleton<IUnitOfWorkFactory, UnitOfWorkFactory>();
		//    services.TryAddSingleton<IRepositoryFactory, RepositoryFactory>();
		//    return services;
		//}


		///// <summary>
		///// 添加EF DOBOptions配置选项
		///// </summary>
		///// <param name="services"></param>
		///// <param name="dbName"></param>
		///// <param name="builderAction"></param>
		///// <returns></returns>
		//public static IServiceCollection AddDbBuilderOptions(this IServiceCollection services, string dbName, Action<DbContextOptionsBuilder> builderAction = null)
		//{
		//    var builder = new DbContextOptionsBuilder();
		//    builderAction(builder);
		//    services.TryAddSingleton<DOBOptions>(new DOBOptions(builder, dbName));
		//    return services;
		//}

		/// <summary>
		/// 添加EF DOBOptions 配置选项
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
			services.TryAddSingleton<DOBOptions>(new DOBOptions(builder, dbName, typeof(TContext)));
			return services;
		}



		///======================================================



		/// <summary>
		/// 添加和配置MiniProfiler服务
		/// </summary>
		/// <param name="services"></param>
		public static void AddDCMSMiniProfiler(this IServiceCollection services)
		{
			services.AddMiniProfiler(miniProfilerOptions =>
			{
				//使用内存缓存提供程序存储每个结果
				((MemoryCacheStorage)miniProfilerOptions.Storage).CacheDuration = TimeSpan.FromMinutes(60);

				//是否应显示MiniProfiler
				miniProfilerOptions.ShouldProfile = request => true;

				//确定谁可以访问MiniProfiler结果
				miniProfilerOptions.ResultsAuthorize = request => true;
			}).AddEntityFramework();
		}

		/// <summary>
		///  添加和配置WebMarkupMin服务
		///  Web标记简化程序（缩写为WebMarkupMin）是一个.NET库，
		///  其中包含一组标记简化符。该项目的目的是通过减少HTML，XHTML和XML代码的大小来提高Web应用程序的性能。
		/// </summary>
		/// <param name="services"></param>
		public static void AddDCMSWebMarkupMin(this IServiceCollection services)
		{
			//是否已安装数据库
			if (!DataSettingsManager.DatabaseIsInstalled)
			{
				return;
			}

			services.AddWebMarkupMin(options =>
				{
					options.AllowMinificationInDevelopmentEnvironment = true;
					options.AllowCompressionInDevelopmentEnvironment = true;
					options.DisableMinification = !EngineContext.Current.Resolve<CommonSettings>().EnableHtmlMinification;
					options.DisableCompression = true;
					options.DisablePoweredByHttpHeaders = true;
				})
				.AddHtmlMinification(options =>
				{
					var settings = options.MinificationSettings;

					options.CssMinifierFactory = new NUglifyCssMinifierFactory();
					options.JsMinifierFactory = new NUglifyJsMinifierFactory();
				})
				.AddXmlMinification(options =>
				{
					var settings = options.MinificationSettings;
					settings.RenderEmptyTagsWithSpace = true;
					settings.CollapseTagsWithoutContent = true;
				});
		}



		/// <summary>
		/// 添加和配置默认http客户端
		/// </summary>
		/// <param name="services"></param>
		public static void AddDCMSHttpClients(this IServiceCollection services)
		{
			//默认客户端
			services.AddHttpClient(DCMSHttpDefaults.DefaultHttpClient).WithProxy();

			//经销商
			services.AddHttpClient<StoreHttpClient>();

			//DCMS官方
			services.AddHttpClient<DCMSHttpClient>().WithProxy();

			//验证码请求
			services.AddHttpClient<CaptchaHttpClient>().WithProxy();
		}

		/// <summary>
		/// 添加白名单过滤
		/// </summary>
		/// <param name="services"></param>
		public static void AddBlackList(this IServiceCollection services, IConfiguration configuration)
		{
			//var dcmsConfig = services.BuildServiceProvider().GetRequiredService<DCMSConfig>();
			services.AddUrlFirewall(options =>
			{
				options.RuleType = UrlFirewallRuleType.Black;
				//options.SetRuleList(configuration.GetSection("UrlBlackList"));
				options.SetIpList(configuration.GetSection("IpBlackList"));
				options.StatusCode = HttpStatusCode.NotFound;
			});

		}

	}
}


