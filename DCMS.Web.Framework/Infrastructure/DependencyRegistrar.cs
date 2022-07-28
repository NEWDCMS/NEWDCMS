using Autofac;
using Autofac.Builder;
using Autofac.Core;
using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Configuration;
using DCMS.Core.Data;
using DCMS.Core.Domain.Stores;
using DCMS.Core.Infrastructure;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Core.Redis;
using DCMS.Data;
using DCMS.Services.Authentication;
using DCMS.Services.Caching;
using DCMS.Services.Campaigns;
using DCMS.Services.Census;
using DCMS.Services.Chat;
using DCMS.Services.Common;
using DCMS.Services.Configuration;
using DCMS.Services.CRM;
using DCMS.Services.CSMS;
using DCMS.Services.Events;
using DCMS.Services.ExportImport;
using DCMS.Services.Finances;
using DCMS.Services.Global.Common;
using DCMS.Services.Helpers;
using DCMS.Services.Installation;
using DCMS.Services.Logging;
using DCMS.Services.Media;
using DCMS.Services.Messages;
using DCMS.Services.News;
using DCMS.Services.OCMS;
using DCMS.Services.plan;
using DCMS.Services.Products;
using DCMS.Services.Purchases;
using DCMS.Services.Report;
using DCMS.Services.Sales;
using DCMS.Services.Security;
using DCMS.Services.Settings;
using DCMS.Services.Stores;
using DCMS.Services.Tasks;
using DCMS.Services.Tax;
using DCMS.Services.Terminals;
using DCMS.Services.TSS;
using DCMS.Services.Users;
using DCMS.Services.Visit;
using DCMS.Services.WareHouses;
using DCMS.Web.Framework.Mvc.Routing;
using DCMS.Web.Framework.UI;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DCMS.Web.Framework.Infrastructure
{
	/// <summary>
	/// 依赖注册
	/// </summary>
	public class DependencyRegistrar : IDependencyRegistrar
	{
		/// <summary>
		/// 注册所有服务的接口
		/// </summary>
		/// <param name="builder">ContainerBuilder -> Autofac</param>
		/// <param name="typeFinder">Type finder</param>
		/// <param name="config">DCMS 系统配置</param>
		public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, DCMSConfig config, bool apiPlatform = false)
		{
			//文件提供
			builder.RegisterType<DCMSFileProvider>().As<IDCMSFileProvider>().InstancePerLifetimeScope();

			//web辅助器
			builder.RegisterType<WebHelper>().As<IWebHelper>().InstancePerLifetimeScope();

			//用户代理辅助器
			builder.RegisterType<UserAgentHelper>().As<IUserAgentHelper>().InstancePerLifetimeScope();

			//数据层,数据提供器
			//builder.RegisterType<EfDataProviderManager>().As<IDataProviderManager>().InstancePerDependency();
			//数据提供者
			//builder.Register(context => context.Resolve<IDataProviderManager>().DataProvider).As<IDataProvider>().InstancePerDependency();
			//builder.Register(context => new DCMSObjectContext(context.Resolve<DbContextOptions<DCMSObjectContext>>())).As<IDbContext>().InstancePerLifetimeScope();
			//依赖服务获取器
			builder.RegisterType<ServiceGetter>().As<IServiceGetter>().InstancePerDependency();

			//Redis链接包装器
			if (config.RedisEnabled)
			{
				builder.RegisterType<RedisConnectionWrapper>().As<IRedLocker>().As<IRedisConnectionWrapper>().SingleInstance();
			}

			//静态缓存管理器
			if (config.RedisEnabled && config.UseRedisForCaching)
			{
				builder.RegisterType<RedisCacheManager>().As<IRedLocker>().As<IStaticCacheManager>().InstancePerLifetimeScope();
			}
			else
			{
				builder.RegisterType<MemoryCacheManager>().As<IRedLocker>().As<IStaticCacheManager>().SingleInstance();
			}

			//工作上下文
			builder.RegisterType<WebWorkContext>().As<IWorkContext>().InstancePerLifetimeScope();
			//经销商上下文
			builder.RegisterType<WebStoreContext>().As<IStoreContext>().InstancePerLifetimeScope();



			//注册服务
			//builder.RegisterType<SearchTermService>().As<ISearchTermService>().InstancePerLifetimeScope();
			builder.RegisterType<GenericAttributeService>().As<IGenericAttributeService>().InstancePerLifetimeScope();
			//builder.RegisterType<MaintenanceService>().As<IMaintenanceService>().InstancePerLifetimeScope();
			builder.RegisterType<UserAttributeFormatter>().As<IUserAttributeFormatter>().InstancePerLifetimeScope();
			builder.RegisterType<UserAttributeParser>().As<IUserAttributeParser>().InstancePerLifetimeScope();
			builder.RegisterType<UserAttributeService>().As<IUserAttributeService>().InstancePerLifetimeScope();
			builder.RegisterType<UserService>().As<IUserService>().InstancePerLifetimeScope();
			builder.RegisterType<UserRegistrationService>().As<IUserRegistrationService>().InstancePerLifetimeScope();
			//builder.RegisterType<UserReportService>().As<IUserReportService>().InstancePerLifetimeScope();
			builder.RegisterType<PermissionService>().As<IPermissionService>().InstancePerLifetimeScope();
			builder.RegisterType<StandardPermissionProvider>().As<IPermissionProvider>().InstancePerLifetimeScope();
			builder.RegisterType<AclService>().As<IAclService>().InstancePerLifetimeScope();
			builder.RegisterType<StoreService>().As<IStoreService>().InstancePerLifetimeScope();
			builder.RegisterType<StoreMappingService>().As<IStoreMappingService>().InstancePerLifetimeScope();
			//builder.RegisterType<DownloadService>().As<IDownloadService>().InstancePerLifetimeScope();

			//builder.RegisterType<MessageTemplateService>().As<IMessageTemplateService>().InstancePerLifetimeScope();
			builder.RegisterType<QueuedEmailService>().As<IQueuedEmailService>().InstancePerLifetimeScope();

			//builder.RegisterType<NewsLetterSubscriptionService>().As<INewsLetterSubscriptionService>().InstancePerLifetimeScope();
			builder.RegisterType<NotificationService>().As<INotificationService>().InstancePerLifetimeScope();
			//builder.RegisterType<PushService>().As<IPushService>().InstancePerLifetimeScope();

			builder.RegisterType<CampaignService>().As<ICampaignService>().InstancePerLifetimeScope();
			builder.RegisterType<EmailAccountService>().As<IEmailAccountService>().InstancePerLifetimeScope();
			builder.RegisterType<PartnerService>().As<IPartnerService>().InstancePerLifetimeScope();
			//builder.RegisterType<WorkflowMessageService>().As<IWorkflowMessageService>().InstancePerLifetimeScope();
			//builder.RegisterType<MessageTokenProvider>().As<IMessageTokenProvider>().InstancePerLifetimeScope();
			builder.RegisterType<Tokenizer>().As<ITokenizer>().InstancePerLifetimeScope();
			builder.RegisterType<EmailSender>().As<IEmailSender>().InstancePerLifetimeScope();
			builder.RegisterType<EncryptionService>().As<IEncryptionService>().InstancePerLifetimeScope();

			if (apiPlatform)
			{
				builder.RegisterType<JwtAuthenticationService>().As<IAuthenticationService>().InstancePerLifetimeScope();
			}
			else
			{
				builder.RegisterType<CookieAuthenticationService>().As<IAuthenticationService>().InstancePerLifetimeScope();
			}

			builder.RegisterType<DefaultLogger>().As<ILogger>().InstancePerLifetimeScope();
			builder.RegisterType<UserActivityService>().As<IUserActivityService>().InstancePerLifetimeScope();
			builder.RegisterType<DateTimeHelper>().As<IDateTimeHelper>().InstancePerLifetimeScope();
			builder.RegisterType<PageHeadBuilder>().As<IPageHeadBuilder>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleTaskService>().As<IScheduleTaskService>().InstancePerLifetimeScope();
	
			//builder.RegisterType<ExternalAuthenticationService>().As<IExternalAuthenticationService>().InstancePerLifetimeScope();

			//路由和发布服务
			builder.RegisterType<RoutePublisher>().As<IRoutePublisher>().SingleInstance();
			builder.RegisterType<EventPublisher>().As<IEventPublisher>().SingleInstance();

			//用于缓存键提供服务
			builder.RegisterType<CacheKeyService>().As<ICacheKeyService>().InstancePerLifetimeScope();

			//配置服务，允许循环依赖
			//   builder.RegisterType<SettingService>().As<ISettingService>().InstancePerLifetimeScope();
			builder.RegisterType<SettingService>().As<ISettingService>()
				.InstancePerLifetimeScope()
				.PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

			//Action上下文访问器
			builder.RegisterType<ActionContextAccessor>().As<IActionContextAccessor>().InstancePerLifetimeScope();

			//注册所有配置
			builder.RegisterSource(new SettingsSource());

			//===========================================================================
			//=============================V1.0==========================================
			//===========================================================================

			//消息发布服务
			//builder.RegisterGeneric(typeof(MessageSender<>)).As(typeof(IMessageSender<>)).InstancePerLifetimeScope();
			builder.RegisterType<MessageSender>().As<IMessageSender>().InstancePerLifetimeScope();
			builder.RegisterType<QueuedMessageService>().As<IQueuedMessageService>().InstancePerLifetimeScope();

			#region 服务
			builder.RegisterType<BillCheckService>().As<IBillCheckService>().InstancePerLifetimeScope();
			//转化
			builder.RegisterType<BillConvertService>().As<IBillConvertService>().InstancePerLifetimeScope();
			//单据公共
			builder.RegisterType<CommonBillService>().As<ICommonBillService>().InstancePerLifetimeScope();

			//库存预警
			builder.RegisterType<StockEarlyWarningService>().As<IStockEarlyWarningService>().InstancePerLifetimeScope();
			//价格体系
			builder.RegisterType<PricingStructureService>().As<IPricingStructureService>().InstancePerLifetimeScope();
			//打印模板
			builder.RegisterType<PrintTemplateService>().As<IPrintTemplateService>().InstancePerLifetimeScope();
			//备注设置
			builder.RegisterType<RemarkConfigService>().As<IRemarkConfigService>().InstancePerLifetimeScope();
			//Tax
			builder.RegisterType<TaxService>().As<ITaxService>().InstancePerLifetimeScope();

			#region 安全
			//ModuleService
			builder.RegisterType<ModuleService>().As<IModuleService>().InstancePerLifetimeScope();
			//BranchService
			builder.RegisterType<BranchService>().As<IBranchService>().InstancePerLifetimeScope();
			//IUserGroupService
			builder.RegisterType<UserGroupService>().As<IUserGroupService>().InstancePerLifetimeScope();
			#endregion

			#region 普查
			//TraditionService
			//builder.RegisterType<TraditionService>().As<ITraditionService>().InstancePerLifetimeScope();
			#endregion

			#region 销售

			//换货单
			builder.RegisterType<ExchangeBillService>().As<IExchangeBillService>().InstancePerLifetimeScope();

			//销售订单
			builder.RegisterType<SaleReservationBillService>().As<ISaleReservationBillService>().InstancePerLifetimeScope();

			//销售单
			builder.RegisterType<SaleBillService>().As<ISaleBillService>().InstancePerLifetimeScope();

			//退货订单
			builder.RegisterType<ReturnReservationBillService>().As<IReturnReservationBillService>().InstancePerLifetimeScope();

			//退货单
			builder.RegisterType<ReturnBillService>().As<IReturnBillService>().InstancePerLifetimeScope();

			//车辆对货单
			//收款对账单
			builder.RegisterType<FinanceReceiveAccountBillService>().As<IFinanceReceiveAccountBillService>().InstancePerLifetimeScope();


			//仓库分拣单
			builder.RegisterType<PickingBillService>().As<IPickingBillService>().InstancePerLifetimeScope();

			//装车调度单
			builder.RegisterType<DispatchBillService>().As<IDispatchBillService>().InstancePerLifetimeScope();

			//订单转销售单
			builder.RegisterType<ChangeReservationBillService>().As<IChangeReservationBillService>().InstancePerLifetimeScope();

			//销售报表
			builder.RegisterType<SaleReportService>().As<ISaleReportService>().InstancePerLifetimeScope();

			#endregion

			#region 采购

			//采购单
			builder.RegisterType<PurchaseBillService>().As<IPurchaseBillService>().InstancePerLifetimeScope();

			//采购退货单
			builder.RegisterType<PurchaseReturnBillService>().As<IPurchaseReturnBillService>().InstancePerLifetimeScope();

			//采购报表
			builder.RegisterType<PurchaseReportService>().As<IPurchaseReportService>().InstancePerLifetimeScope();

			#endregion

			#region 商品

			builder.RegisterType<ProductService>().As<IProductService>().InstancePerLifetimeScope();
			builder.RegisterType<CategoryService>().As<ICategoryService>().InstancePerLifetimeScope();
			builder.RegisterType<ProductAttributeFormatter>().As<IProductAttributeFormatter>().InstancePerLifetimeScope();
			builder.RegisterType<ProductAttributeParser>().As<IProductAttributeParser>().InstancePerLifetimeScope();
			builder.RegisterType<ProductAttributeService>().As<IProductAttributeService>().InstancePerLifetimeScope();
			builder.RegisterType<SpecificationAttributeService>().As<ISpecificationAttributeService>().InstancePerLifetimeScope();
			builder.RegisterType<StatisticalTypeService>().As<IStatisticalTypeService>().InstancePerLifetimeScope();
			builder.RegisterType<BrandService>().As<IBrandService>().InstancePerLifetimeScope();
			builder.RegisterType<ProductFlavorService>().As<IProductFlavorService>().InstancePerLifetimeScope();
			builder.RegisterType<ProductTierPricePlanService>().As<IProductTierPricePlanService>().InstancePerLifetimeScope();

			#endregion

			#region 仓库


			#region 仓库单据

			//仓库
			builder.RegisterType<WareHouseService>().As<IWareHouseService>().InstancePerLifetimeScope();
			builder.RegisterType<StockService>().As<IStockService>().InstancePerLifetimeScope();

			//库存商品组合单
			builder.RegisterType<CombinationProductBillService>().As<ICombinationProductBillService>().InstancePerLifetimeScope();

			//调拨单
			builder.RegisterType<AllocationBillService>().As<IAllocationBillService>().InstancePerLifetimeScope();

			//成本调价单
			builder.RegisterType<CostAdjustmentBillService>().As<ICostAdjustmentBillService>().InstancePerLifetimeScope();

			//盘点任务(整仓)
			builder.RegisterType<InventoryAllTaskBillService>().As<IInventoryAllTaskBillService>().InstancePerLifetimeScope();

			//盘点任务(部分)
			builder.RegisterType<InventoryPartTaskBillService>().As<IInventoryPartTaskBillService>().InstancePerLifetimeScope();

			//盘点盈亏单
			builder.RegisterType<InventoryProfitLossBillService>().As<IInventoryProfitLossBillService>().InstancePerLifetimeScope();

			//商品报损单
			builder.RegisterType<ScrapProductBillService>().As<IScrapProductBillService>().InstancePerLifetimeScope();

			//商品拆分单
			builder.RegisterType<SplitProductBillService>().As<ISplitProductBillService>().InstancePerLifetimeScope();

			//门店库存上报
			builder.RegisterType<InventoryReportBillService>().As<IInventoryReportBillService>().InstancePerLifetimeScope();

			#endregion

			#region 仓库报表

			builder.RegisterType<StockReportService>().As<IStockReportService>().InstancePerLifetimeScope();
			//门店库存上报
			//builder.RegisterType<SubmitFromStoreReportingService>().As<ISubmitFromStoreReportingService>().InstancePerLifetimeScope();
			#endregion


			#endregion

			#region 财务

			//计算服务
			builder.RegisterType<CalculationService>().As<ICalculationService>().InstancePerLifetimeScope();
			//会计科目
			builder.RegisterType<AccountingService>().As<IAccountingService>().InstancePerLifetimeScope();
			//收款单
			builder.RegisterType<CashReceiptBillService>().As<ICashReceiptBillService>().InstancePerLifetimeScope();
			//成本结转
			builder.RegisterType<ClosingAccountsService>().As<IClosingAccountsService>().InstancePerLifetimeScope();
			//预付款单据服务
			builder.RegisterType<AdvancePaymentBillService>().As<IAdvancePaymentBillService>().InstancePerLifetimeScope();
			//预收款单据服务
			builder.RegisterType<AdvanceReceiptBillService>().As<IAdvanceReceiptBillService>().InstancePerLifetimeScope();
			//收款单服务
			builder.RegisterType<CostContractBillService>().As<ICostContractBillService>().InstancePerLifetimeScope();
			//费用支出单据服务
			builder.RegisterType<CostExpenditureBillService>().As<ICostExpenditureBillService>().InstancePerLifetimeScope();
			//财务收入单据服务
			builder.RegisterType<FinancialIncomeBillService>().As<IFinancialIncomeBillService>().InstancePerLifetimeScope();
			//付款单据服务
			builder.RegisterType<PaymentReceiptBillService>().As<IPaymentReceiptBillService>().InstancePerLifetimeScope();
			//记账凭证
			builder.RegisterType<RecordingVoucherService>().As<IRecordingVoucherService>().InstancePerLifetimeScope();
			builder.RegisterType<LedgerDetailsService>().As<ILedgerDetailsService>().InstancePerLifetimeScope();
			builder.RegisterType<TrialBalanceService>().As<ITrialBalanceService>().InstancePerLifetimeScope();

			#endregion

			#region 往来相关
			builder.RegisterType<TerminalService>().As<ITerminalService>().InstancePerLifetimeScope();//终端信息
			builder.RegisterType<DistrictService>().As<IDistrictService>().InstancePerLifetimeScope();//片区信息
			builder.RegisterType<ChannelService>().As<IChannelService>().InstancePerLifetimeScope();//渠道
			builder.RegisterType<RankService>().As<IRankService>().InstancePerLifetimeScope();//终端等级
			builder.RegisterType<ManufacturerService>().As<IManufacturerService>().InstancePerLifetimeScope();//供应商
			builder.RegisterType<ReceivableService>().As<IReceivableService>().InstancePerLifetimeScope();//应收款
			builder.RegisterType<ReceivableDetailService>().As<IReceivableDetailService>().InstancePerLifetimeScope();//应收款明细
			#endregion

			#region 档案

			//提成方案
			builder.RegisterType<PercentagePlanService>().As<IPercentagePlanService>().InstancePerLifetimeScope();
			builder.RegisterType<PercentageService>().As<IPercentageService>().InstancePerLifetimeScope();
			//赠品
			builder.RegisterType<GiveQuotaService>().As<IGiveQuotaService>().InstancePerLifetimeScope();
			//活动
			builder.RegisterType<CampaignService>().As<ICampaignService>().InstancePerLifetimeScope();
			//线路
			builder.RegisterType<LineTierService>().As<ILineTierService>().InstancePerLifetimeScope();
			#endregion

			#region 报表

			//主页面报表
			builder.RegisterType<MainPageReportService>().As<IMainPageReportService>().InstancePerLifetimeScope();

			//资金报表
			builder.RegisterType<FundReportService>().As<IFundReportService>().InstancePerLifetimeScope();

			//员工报表
			builder.RegisterType<MarketReportService>().As<IMarketReportService>().InstancePerLifetimeScope();

			//市场报表
			builder.RegisterType<StaffReportService>().As<IStaffReportService>().InstancePerLifetimeScope();

			//IUserAssessmentService
			builder.RegisterType<UserAssessmentService>().As<IUserAssessmentService>().InstancePerLifetimeScope();

			#endregion

			#region 导入导出、消息推送
			//导出
			builder.RegisterType<ExportService>().As<IExportService>().InstancePerLifetimeScope();
			//导入
			//builder.RegisterType<ImportService>().As<IImportService>().InstancePerLifetimeScope();
			#endregion

			#region 消息管理
			//消息类别
			builder.RegisterType<NewsCategoryService>().As<INewsCategoryService>().InstancePerLifetimeScope();
			//消息管理
			builder.RegisterType<NewsService>().As<INewsService>().InstancePerLifetimeScope();
			//私信
			builder.RegisterType<PrivateMessageService>().As<IPrivateMessageService>().InstancePerLifetimeScope();
			//发送消息
			//builder.RegisterType<SendMessageService>().As<ISendMessageService>().InstancePerLifetimeScope();

			#endregion

			#region 拜访相关
			//业务员轨迹
			builder.RegisterType<TrackingService>().As<ITrackingService>().InstancePerLifetimeScope();

			//业务员拜访
			builder.RegisterType<VisitStoreService>().As<IVisitStoreService>().InstancePerLifetimeScope();
			#endregion

			#region CRM 集成

			builder.RegisterType<BpsService>().As<IBpsService>().InstancePerLifetimeScope();
			builder.RegisterType<BustatsService>().As<IBustatsService>().InstancePerLifetimeScope();
			builder.RegisterType<HeightConfsService>().As<IHeightConfsService>().InstancePerLifetimeScope();
			builder.RegisterType<OrgsService>().As<IOrgsService>().InstancePerLifetimeScope();
			builder.RegisterType<RelationsService>().As<IRelationsService>().InstancePerLifetimeScope();
			builder.RegisterType<ReturnsService>().As<IReturnsService>().InstancePerLifetimeScope();
			builder.RegisterType<Zsntm0040Service>().As<IZsntm0040Service>().InstancePerLifetimeScope();

			#endregion

			#region OCMS集成
			builder.RegisterType<OCMSProductsService>().As<IOCMSProductsService>().InstancePerLifetimeScope();
			builder.RegisterType<CharacterSettingService>().As<ICharacterSettingService>().InstancePerLifetimeScope();
			#endregion

			#region CSMS集成

			builder.RegisterType<TerminalSignReportService>().As<ITerminalSignReportService>().InstancePerLifetimeScope();

			builder.RegisterType<OrderDetailService>().As<IOrderDetailService>().InstancePerLifetimeScope();

			#endregion


			//系统安装
			builder.RegisterType<InstallationService>().As<IInstallationService>().InstancePerLifetimeScope();

			#region TSS

			builder.RegisterType<FeedbackService>().As<IFeedbackService>().InstancePerLifetimeScope();
			builder.RegisterType<MarketFeedbackService>().As<IMarketFeedbackService>().InstancePerLifetimeScope();

			//Chat
			builder.RegisterType<ConnectionService>().As<IConnectionService>().SingleInstance();
			builder.RegisterType<ChatService>().As<IChatService>().InstancePerLifetimeScope();
			builder.RegisterType<ChatUserService>().As<IChatUserService>().InstancePerLifetimeScope();

			#endregion

			#endregion
			//===========================================================================
			//===========================================================================

			//媒体图片服务
			builder.RegisterType<ExportManager>().As<IExportManager>().InstancePerLifetimeScope();
			builder.RegisterType<ImportManager>().As<IImportManager>().InstancePerLifetimeScope();
			builder.RegisterType<MediaService>().As<IMediaService>().InstancePerLifetimeScope();
			builder.RegisterType<PictureService>().As<IPictureService>().InstancePerLifetimeScope();

			//初始安装服务（用于经销商系统初始安装向导）
			if (!DataSettingsManager.DatabaseIsInstalled)
			{
				builder.RegisterType<InstallationService>().As<IInstallationService>().InstancePerLifetimeScope();
			}

			//事件消费者（用于订阅服务）
			var consumers = typeFinder.FindClassesOfType(typeof(IConsumer<>)).ToList();
			foreach (var consumer in consumers)
			{
				builder.RegisterType(consumer)
					.As(consumer.FindInterfaces((type, criteria) =>
					{
						var isMatch = type.IsGenericType && ((Type)criteria).IsAssignableFrom(type.GetGenericTypeDefinition());
						return isMatch;
					}, typeof(IConsumer<>)))
					.InstancePerLifetimeScope();
			}
		}

		/// <summary>
		/// 获取此依赖项注册器实现的排序
		/// </summary>
		public int Order => 0;
	}


	/// <summary>
	/// 配置资源
	/// </summary>
	public class SettingsSource : IRegistrationSource
	{
		private static readonly MethodInfo _buildMethod =
			typeof(SettingsSource).GetMethod("BuildRegistration", BindingFlags.Static | BindingFlags.NonPublic);


		//public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrations)
		//{
		//	var ts = service as TypedService;
		//	if (ts != null && typeof(ISettings).IsAssignableFrom(ts.ServiceType))
		//	{
		//		var buildMethod = _buildMethod.MakeGenericMethod(ts.ServiceType);
		//		yield return (IComponentRegistration)buildMethod.Invoke(null, null);
		//	}
		//}

		//private static IComponentRegistration BuildRegistration<TSettings>() where TSettings : ISettings, new()
		//{
		//    return RegistrationBuilder
		//        .ForDelegate((c, p) =>
		//        {
		//            var currentStoreId = c.Resolve<IStoreContext>().CurrentStore?.Id ?? 0;
		//            return c.Resolve<ISettingService>().LoadSetting<TSettings>(currentStoreId);
		//        })
		//        .InstancePerLifetimeScope()
		//        //.PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies)
		//        .CreateRegistration();
		//}


		private static IComponentRegistration BuildRegistration<TSettings>() where TSettings : ISettings, new()
		{
			return RegistrationBuilder
				.ForDelegate((c, p) =>
				{
					Store store;

					try
					{
						store = c.Resolve<IStoreContext>().CurrentStore;
					}
					catch
					{
						store = null;
					}

					var currentStoreId = store?.Id ?? 0;

					try
					{
						return c.Resolve<ISettingService>().LoadSetting<TSettings>(currentStoreId);
					}
					catch
					{
						if (DataSettingsManager.DatabaseIsInstalled)
							throw;
					}

					return default;
				})
				.InstancePerLifetimeScope()
				//.PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies)
				.CreateRegistration();
		}

		public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
		{
			var ts = service as TypedService;
			if (ts != null && typeof(ISettings).IsAssignableFrom(ts.ServiceType))
			{
				var buildMethod = _buildMethod.MakeGenericMethod(ts.ServiceType);
				yield return (IComponentRegistration)buildMethod.Invoke(null, null);
			}
		}

		public bool IsAdapterForIndividualComponents => false;
	}

}