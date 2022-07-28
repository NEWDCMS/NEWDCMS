using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Configuration;
using DCMS.Core.Domain.Common;
using DCMS.Core.Domain.Security;
using DCMS.Core.Domain.Stores;
using DCMS.Core.Domain.Users;
using DCMS.Core.Infrastructure;
using DCMS.Services.Common;
using DCMS.Services.Helpers;
using DCMS.Services.Security;
using DCMS.Services.Stores;
using DCMS.Services.Users;
using DCMS.ViewModel.Models.Common;
using DCMS.ViewModel.Models.Users;
using DCMS.Web.Framework.Security;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace DCMS.Web.Factories
{

	public partial class CommonModelFactory : ICommonModelFactory
	{
		private readonly IWorkContext _workContext;
		private readonly IStoreContext _storeContext;
		private readonly CommonSettings _commonSettings;
		private readonly AdminAreaSettings _adminAreaSettings;
		private readonly IActionContextAccessor _actionContextAccessor;
		private readonly IUserService _userService;
		private readonly IDateTimeHelper _dateTimeHelper;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IDCMSFileProvider _fileProvider;
		
		private readonly IStoreService _storeService;
		private readonly IUrlHelperFactory _urlHelperFactory;
		private readonly IWebHelper _webHelper;
		private readonly DCMSConfig _dcmsConfig;
		private readonly DCMSHttpClient _dcmsHttpClient;
		private readonly ProxySettings _proxySettings;
		private readonly IPermissionService _permissionService;
		private readonly IModuleService _moduleService;
		protected readonly IStaticCacheManager _cacheManager;
		public Store curStore => _storeContext.CurrentStore;
		public User curUser => _workContext.CurrentUser;

		public CommonModelFactory(AdminAreaSettings adminAreaSettings,
			IActionContextAccessor actionContextAccessor,
			IUserService userService,
			IDateTimeHelper dateTimeHelper,
			IDCMSFileProvider fileProvider,
			IHttpContextAccessor httpContextAccessor,
			IStaticCacheManager cacheManager,
			IStoreContext storeContext,
			IStoreService storeService,
			IUrlHelperFactory urlHelperFactory,
			IPermissionService permissionService,
			IWebHelper webHelper,
			IWorkContext workContext,
			DCMSConfig dcmsConfig,
			DCMSHttpClient dcmsHttpClient,
			CommonSettings commonSettings,
			IModuleService moduleService,
			ProxySettings proxySettings)
		{
			_adminAreaSettings = adminAreaSettings;
			_actionContextAccessor = actionContextAccessor;
			_userService = userService;
			_dateTimeHelper = dateTimeHelper;
			_httpContextAccessor = httpContextAccessor;
			//_maintenanceService = maintenanceService;
			_fileProvider = fileProvider;
			//_searchTermService = searchTermService;
			_cacheManager = cacheManager;
			_storeContext = storeContext;
			_storeService = storeService;
			_urlHelperFactory = urlHelperFactory;
			//_urlRecordService = urlRecordService;
			_webHelper = webHelper;
			_workContext = workContext;
			_dcmsConfig = dcmsConfig;
			_dcmsHttpClient = dcmsHttpClient;
			_proxySettings = proxySettings;
			_commonSettings = commonSettings;
			_permissionService = permissionService;
			_moduleService = moduleService;
		}

		protected virtual void PrepareStoreUrlWarningModel(IList<SystemWarningModel> models)
		{
			if (models == null)
			{
				throw new ArgumentNullException(nameof(models));
			}

			//check whether current store URL matches the store configured URL
			var currentStoreUrl = _storeContext.CurrentStore.Url;
			if (!string.IsNullOrEmpty(currentStoreUrl) &&
				(currentStoreUrl.Equals(_webHelper.GetStoreLocation(false), StringComparison.InvariantCultureIgnoreCase) ||
				currentStoreUrl.Equals(_webHelper.GetStoreLocation(true), StringComparison.InvariantCultureIgnoreCase)))
			{
				models.Add(new SystemWarningModel
				{
					Level = SystemWarningLevel.Pass,
					Text = "匹配通过！"
				});
				return;
			}

			models.Add(new SystemWarningModel
			{
				Level = SystemWarningLevel.Fail,
				Text = "匹配失败！"
			});
		}

		protected virtual void PrepareRemovalKeyWarningModel(IList<SystemWarningModel> models)
		{
			if (models == null)
			{
				throw new ArgumentNullException(nameof(models));
			}

			if (!_adminAreaSettings.CheckCopyrightRemovalKey)
			{
				return;
			}

			//try to get a warning
			var warning = string.Empty;
			try
			{
				warning = _dcmsHttpClient.GetCopyrightWarningAsync().Result;
			}
			catch { }
			if (string.IsNullOrEmpty(warning))
			{
				return;
			}

			models.Add(new SystemWarningModel
			{
				Level = SystemWarningLevel.CopyrightRemovalKey,
				Text = warning,
				DontEncode = true //this text could contain links, so don't encode it
			});
		}

		protected virtual void PrepareFilePermissionsWarningModel(IList<SystemWarningModel> models)
		{
			if (models == null)
			{
				throw new ArgumentNullException(nameof(models));
			}

			var dirPermissionsOk = true;
			var dirsToCheck = FilePermissionHelper.GetDirectoriesWrite();
			foreach (var dir in dirsToCheck)
			{
				if (FilePermissionHelper.CheckPermissions(dir, false, true, true, false))
				{
					continue;
				}

				models.Add(new SystemWarningModel
				{
					Level = SystemWarningLevel.Warning,
					Text = string.Format("Admin.System.Warnings.DirectoryPermission.Wrong：{0} / {1}", CurrentOSUser.FullName, dir)
				});
				dirPermissionsOk = false;
			}

			if (dirPermissionsOk)
			{
				models.Add(new SystemWarningModel
				{
					Level = SystemWarningLevel.Pass,
					Text = "Admin.System.Warnings.DirectoryPermission.OK"
				});
			}

			var filePermissionsOk = true;
			var filesToCheck = FilePermissionHelper.GetFilesWrite();
			foreach (var file in filesToCheck)
			{
				if (FilePermissionHelper.CheckPermissions(file, false, true, true, true))
				{
					continue;
				}

				models.Add(new SystemWarningModel
				{
					Level = SystemWarningLevel.Warning,
					Text = string.Format("Admin.System.Warnings.FilePermission.Wrong：{0} / {1}", CurrentOSUser.FullName, file)
				});
				filePermissionsOk = false;
			}

			if (filePermissionsOk)
			{
				models.Add(new SystemWarningModel
				{
					Level = SystemWarningLevel.Pass,
					Text = "Admin.System.Warnings.FilePermission.OK"
				});
			}
		}

		protected virtual BackupFileSearchModel PrepareBackupFileSearchModel(BackupFileSearchModel searchModel)
		{
			if (searchModel == null)
			{
				throw new ArgumentNullException(nameof(searchModel));
			}

			//prepare page parameters
			searchModel.SetGridPageSize();

			return searchModel;
		}

		public virtual SystemInfoModel PrepareSystemInfoModel(SystemInfoModel model)
		{
			if (model == null)
			{
				throw new ArgumentNullException(nameof(model));
			}

			model.NopVersion = DCMSVersion.CurrentVersion;
			model.ServerTimeZone = TimeZoneInfo.Local.StandardName;
			model.ServerLocalTime = DateTime.Now;
			model.UtcTime = DateTime.UtcNow;
			model.CurrentUserTime = _dateTimeHelper.ConvertToUserTime(DateTime.Now);
			model.HttpHost = _httpContextAccessor.HttpContext.Request.Headers[HeaderNames.Host];

			//ensure no exception is thrown
			try
			{
				model.OperatingSystem = Environment.OSVersion.VersionString;
				model.AspNetInfo = RuntimeEnvironment.GetSystemVersion();
				model.IsFullTrust = AppDomain.CurrentDomain.IsFullyTrusted.ToString();
			}
			catch { }

			foreach (var header in _httpContextAccessor.HttpContext.Request.Headers)
			{
				model.Headers.Add(new SystemInfoModel.HeaderModel
				{
					Name = header.Key,
					Value = header.Value
				});
			}

			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				var loadedAssemblyModel = new SystemInfoModel.LoadedAssembly
				{
					FullName = assembly.FullName
				};

				//ensure no exception is thrown
				try
				{
					loadedAssemblyModel.Location = assembly.IsDynamic ? null : assembly.Location;
					loadedAssemblyModel.IsDebug = assembly.GetCustomAttributes(typeof(DebuggableAttribute), false)
						.FirstOrDefault() is DebuggableAttribute attribute && attribute.IsJITOptimizerDisabled;

			
					loadedAssemblyModel.BuildDate = assembly.IsDynamic ? null : (DateTime?)TimeZoneInfo.ConvertTimeFromUtc(_fileProvider.GetLastWriteTimeUtc(assembly.Location), TimeZoneInfo.Local);

				}
				catch { }
				model.LoadedAssemblies.Add(loadedAssemblyModel);
			}

			model.CurrentStaticCacheManager = _cacheManager.GetType().Name;

			//model.RedisEnabled = _dcmsConfig.RedisEnabled;
			////model.UseRedisToStoreDataProtectionKeys = _dcmsConfig.UseRedisToStoreDataProtectionKeys;
			//model.UseRedisForCaching = _dcmsConfig.UseRedisForCaching;
			//model.UseRedisToStorePluginsInfo = _dcmsConfig.UseRedisToStorePluginsInfo;

	 

			return model;
		}

		protected virtual void PrepareProxyConnectionWarningModel(IList<SystemWarningModel> models)
		{
			if (models == null)
			{
				throw new ArgumentNullException(nameof(models));
			}

			//whether proxy is enabled
			if (!_proxySettings.Enabled)
			{
				return;
			}

			try
			{
				_dcmsHttpClient.PingAsync().Wait();

				//connection is OK
				models.Add(new SystemWarningModel
				{
					Level = SystemWarningLevel.Pass,
					Text = "Admin.System.Warnings.ProxyConnection.OK"
				});
			}
			catch
			{
				//connection failed
				models.Add(new SystemWarningModel
				{
					Level = SystemWarningLevel.Fail,
					Text = "Admin.System.Warnings.ProxyConnection.Failed"
				});
			}
		}

		public virtual IList<SystemWarningModel> PrepareSystemWarningModels()
		{
			var models = new List<SystemWarningModel>();

			//store URL
			PrepareStoreUrlWarningModel(models);

			//removal key
			PrepareRemovalKeyWarningModel(models);

			//validate write permissions (the same procedure like during installation)
			PrepareFilePermissionsWarningModel(models);

			//proxy connection
			PrepareProxyConnectionWarningModel(models);

			return models;
		}


		public virtual UrlRecordSearchModel PrepareUrlRecordSearchModel(UrlRecordSearchModel searchModel)
		{
			if (searchModel == null)
			{
				throw new ArgumentNullException(nameof(searchModel));
			}

			//prepare page parameters
			searchModel.SetGridPageSize();

			return searchModel;
		}

		public virtual UrlRecordListModel PrepareUrlRecordListModel(UrlRecordSearchModel searchModel)
		{
			if (searchModel == null)
			{
				throw new ArgumentNullException(nameof(searchModel));
			}

			var model = new UrlRecordListModel();
			////get URL records
			//var urlRecords = _urlRecordService.GetAllUrlRecords(slug: searchModel.SeName,
			//    pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

			////get URL helper
			//var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

			////prepare list model
			//var model = new UrlRecordListModel().PrepareToGrid(searchModel, urlRecords, () =>
			//{
			//    return urlRecords.Select(urlRecord =>
			//    {
			//        //fill in model values from the entity
			//        var urlRecordModel = urlRecord.ToModel<UrlRecordModel>();

			//        //fill in additional values (not existing in the entity)
			//        urlRecordModel.Name = urlRecord.Slug;

			//        //details URL
			//        var detailsUrl = string.Empty;
			//        var entityName = urlRecord.EntityName?.ToLowerInvariant() ?? string.Empty;
			//        switch (entityName)
			//        {
			//            case "category":
			//                detailsUrl = urlHelper.Action("Edit", "Category", new { id = urlRecord.EntityId });
			//                break;
			//            case "manufacturer":
			//                detailsUrl = urlHelper.Action("Edit", "Manufacturer", new { id = urlRecord.EntityId });
			//                break;
			//            case "product":
			//                detailsUrl = urlHelper.Action("Edit", "Product", new { id = urlRecord.EntityId });
			//                break;
			//        }

			//        urlRecordModel.DetailsUrl = detailsUrl;

			//        return urlRecordModel;
			//    });
			//});


			return model;
		}

		public virtual CommonStatisticsModel PrepareCommonStatisticsModel()
		{
			var model = new CommonStatisticsModel
			{
				NumberOfOrders = 0
			};

			//var customerRoleIds = new[] { _customerService.GetCustomerRoleBySystemName(NopCustomerDefaults.RegisteredRoleName).Id };
			//model.NumberOfCustomers = _customerService.GetAllCustomers(customerRoleIds: customerRoleIds,
			//    pageIndex: 0, pageSize: 1, getOnlyTotalCount: true).TotalCount;

			//var returnRequestStatus = ReturnRequestStatus.Pending;
			//model.NumberOfPendingReturnRequests = _returnRequestService.SearchReturnRequests(rs: returnRequestStatus,
			//    pageIndex: 0, pageSize: 1, getOnlyTotalCount: true).TotalCount;

			//model.NumberOfLowStockProducts =
			//    _productService.GetLowStockProducts(getOnlyTotalCount: true).TotalCount +
			//    _productService.GetLowStockProductCombinations(getOnlyTotalCount: true).TotalCount;

			return model;
		}

		public virtual FaviconAndAppIconsModel PrepareFaviconAndAppIconsModel()
		{
			var model = new FaviconAndAppIconsModel
			{
				HeadCode = _commonSettings.FaviconAndAppIconsHeadCode
			};

			return model;
		}


		/// <summary>
		/// 用户登录状态
		/// </summary>
		/// <returns></returns>
		public virtual UserModel LoginStates()
		{
			var model = new UserModel
			{
				Active = false
			};

			if (curUser != null)
			{
				var u = curUser.UserRoles.Select(r => r?.Name).ToList().FirstOrDefault();
				model = new UserModel()
				{
					Active = true,
					UserRealName = curUser.UserRealName,
					UserRoleNames = string.Join(",", string.IsNullOrEmpty(u) ? "" : u)
				};
			}
			return model;
		}


		public virtual MenuModel LeftSidebar()
		{

			if (_storeContext.CurrentStore == null || _workContext.CurrentUser == null)
			{
				return new MenuModel();
			}

			try
			{

				var model = new MenuModel();

				//当前用户角色
				var userRoles = _userService.GetUserRolesByUser(curUser.StoreId, curUser.Id);
				userRoles?.ToList()?.ForEach(x =>
				{
					curUser.AddUserRole(x);
				});


				//获取当前用户的所有模块权限记录
				var permissionIds = _userService.GetUserPermissionRecords(curUser).Select(c => c.Id).ToList();

				//获取用户角色模块
				var modules = _userService.GetUserModuleRecordsByUser(curUser).Select(c => c.Value ?? 0).ToList();

				//获取系统模块的所有权限集
				var allRecords = _permissionService.GetAllPermissionRecordsByStore(0);

				//获取所有模块
				var allModules = _moduleService.GetModulesByStore(0);

				model.MenuTrees = GetModuleList(0, 0, 3, permissionIds, modules, allModules, allRecords);


				return model;
			}
			catch (Exception ex)
			{
				var error = ex.Message;
				return null;
			}

		}


		/// <summary>
		/// 递归获取模块树
		/// </summary>
		/// <param name="store">经销商</param>
		/// <param name="Id"></param>
		/// <returns></returns>
		[NonAction]
		private List<ModuleTree<ModuleModel>> GetModuleList(int? store, int Id, int position, List<int> permissionIds, List<int> modules, List<Module> allModules, IList<PermissionRecord> allRecords)
		{
			List<ModuleTree<ModuleModel>> trees = new List<ModuleTree<ModuleModel>>();
			var perentList = _moduleService.GetNotPaltformModulesByParentId(allModules, store.Value, Id, position);

			if (perentList != null && perentList.Count > 0)
			{
				foreach (var b in perentList)
				{
					List<ModuleTree<ModuleModel>> tempList = GetModuleList(store.Value, b.Id, position, permissionIds, modules, allModules, allRecords);

					var records = allRecords.Where(r => r.ModuleId == b.Id).ToList();

					var model = b.ToModel<ModuleModel>();
					model.Selected = modules != null ? modules.Contains(b.Id) : false;

					var node = new ModuleTree<ModuleModel>
					{
						Visible = modules.Contains(b.Id),
						Module = model,
						Children = new List<ModuleTree<ModuleModel>>(),
						PermissionRecords = records.Select(s =>
						{
							return new PermissionRecordModel()
							{
								Id = s.Id,
								Name = s.Name,
								Code = s.Code,
								SystemName = s.SystemName,
								StoreId = s.StoreId,
								ModuleId = s.ModuleId,
								ModuleName = b == null ? "" : b.Name,
								Enabled = s.Enabled
							};

						}).ToList()
					};

					if (model.ParentId == 0 && tempList != null && tempList.Count(t => t.Visible == true) > 0)
					{
						node.Visible = true;
					}

					if (tempList != null && tempList.Count > 0)
					{
						node.Children = tempList;
					}

					trees.Add(node);

				}
			}
			return trees;
		}


	}
}