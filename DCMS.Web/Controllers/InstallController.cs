using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Data;
using DCMS.Core.Infrastructure;
using DCMS.Services.Installation;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Stores;
using DCMS.Services.Security;
using DCMS.Services.Users;
using DCMS.ViewModel.Models.Install;
using DCMS.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System;
using DCMS.Services.Terminals;

namespace DCMS.Web.Controllers
{
    /// <summary>
    /// 用于初始安装向导
    /// </summary>
    [CheckAccessPublicStore]
    public class InstallController : BasePublicController
    {
        private readonly IUserService _userService;
        private readonly IDCMSFileProvider _fileProvider;
        private readonly IPermissionService _permissionService;
        private readonly IStoreService  _storeService;
        private readonly IDistrictService _districtService;

        public InstallController(
          INotificationService notificationService,
          IStoreContext storeContext,
          ILogger loggerService,
          IUserService userService,
          IDCMSFileProvider fileProvider,
          IStoreService storeService,
          IPermissionService permissionService,
          IDistrictService districtService,
          IWorkContext workContext) : base(workContext, loggerService, storeContext, notificationService)
        {
            _userService = userService;
            _fileProvider = fileProvider;
            _permissionService = permissionService;
            _storeService = storeService;
            _districtService = districtService;
        }

        [CheckAccessPublicStore(true)]
        public IActionResult Index(string returnUrl)
        {
            if (!_permissionService.ManageAuthorize())
            {
                return AccessDeniedView();
            }

            var model = new InstallModel
            {

                StoreName = "",
                StoreEmail = "",
                AdminUserName = "",
                StoreMobileNumber = ""
            };

            return View(model);
        }

        /// <summary>
        /// 简化版
        /// </summary>
        /// <param name="order"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        [HttpPost]
        [CheckAccessPublicStore(true)]
        public virtual JsonResult Install(int order)
        {
            try
            {
                int storeId = curStore?.Id ?? 0;

                ProcessStatus result;
                string message = "";
                bool success = true;
                if (storeId == 0 && order != 1)
                {
                    return Json(new { Success = false, Message = "糟糕！第" + order + "步安装失败啦", Data = storeId });
                }

                var installationService = EngineContext.Current.Resolve<IInstallationService>();

                switch (order)
                {
                    case 1:
                        result = installationService.InstallAccounting(storeId);
                        success = result.Result;
                        if (result.Result)
                        {
                            message = "会计科目初始完成";
                        }
                        else
                        {
                            message = "失败，回滚会计科目";
                            installationService.RollBackInstall(storeId);
                        }

                        break;
                    case 2:
                        result = installationService.InstallCategories(storeId);
                        success = result.Result;
                        if (result.Result)
                        {
                            message = "商品类别初始完成";
                        }
                        else
                        {
                            message = "失败，回滚商品类别初";
                            installationService.RollBackInstall(storeId);
                        }

                        break;
                    case 3:
                        result = installationService.InstallBrands(storeId);
                        success = result.Result;
                        if (result.Result)
                        {
                            message = "品牌初始完成";
                        }
                        else
                        {
                            message = "失败，回滚品牌初始";
                            installationService.RollBackInstall(storeId);
                        }
                        break;
                    case 4:
                        result = installationService.InstallManufacturers(storeId);
                        success = result.Result;
                        if (result.Result)
                        {
                            message = "供应商初始完成";
                        }
                        else
                        {
                            message = "失败，回滚供应商初始";
                            installationService.RollBackInstall(storeId);
                        }
                        break;
                    case 5:
                        result = installationService.InstallRemarkConfigs(storeId, "");
                        success = result.Result;
                        if (result.Result)
                        {
                            message = "备注初始完成";
                        }
                        else
                        {
                            message = "失败，回滚备注初始";
                            installationService.RollBackInstall(storeId);
                        }
                        break;
                    case 6:
                        result = installationService.InstallProductUnit(storeId);
                        success = result.Result;
                        if (result.Result)
                        {
                            message = "商品单位初始完成";
                        }
                        else
                        {
                            message = "失败，回滚商品单位初始";
                            installationService.RollBackInstall(storeId);
                        }

                        break;
                    case 7:
                        result = installationService.InstallWarehouses(storeId);
                        success = result.Result;
                        if (result.Result)
                        {
                            message = "仓库初始完成";
                        }
                        else
                        {
                            message = "失败，回滚仓库初始";
                            installationService.RollBackInstall(storeId);
                        }

                        break;
                    case 8:
                        break;
                    case 9:
                        result = installationService.InstallPrintTemplate(storeId);
                        success = result.Result;
                        if (result.Result)
                        {
                            message = "安装打印模板完成";
                        }
                        else
                        {
                            message = "失败，回滚安装打印模板初始";
                            installationService.RollBackInstall(storeId);
                        }
                        break;
                    case 10:
                        if (!_districtService.HadInstall(storeId))
                        {
                            success = _districtService.InstallDistrict(storeId);
                            if (success)
                            {
                                message = "片区初始完成";
                            }
                            else
                            {
                                message = "失败，回滚片区初始";
                                installationService.RollBackInstall(storeId);
                            }
                        }
                        else
                        {
                            success = true;
                            message = "片区初始完成";
                        }
                        break;
                }

                return Json(new { Success = success, Message = message, Data = order });
            }
            catch (Exception)
            {
                //清除缓存
                var cacheManager = EngineContext.Current.Resolve<IStaticCacheManager>();
                cacheManager.Clear();
                //添加错误验证提示
                return Json(new { Success = false, Message = "糟糕！第" + order + "步安装失败啦", Data = 0 });
            }
        }
        [HttpPost]
        [CheckAccessPublicStore(true)]
        public virtual JsonResult ConfirmCompletion()
        {
            try
            {
                int storeId = curStore?.Id ?? 0;
                var store = _storeService.GetStoreById(storeId);
                if (store != null)
                {
                    store.Setuped = true;
                    store.Actived = true;
                    _storeService.UpdateStore(store);
                }
                return Json(new { Success = true, Message ="确认完成" });
            }
            catch (Exception ex)
            {
                //清除缓存
                var cacheManager = EngineContext.Current.Resolve<IStaticCacheManager>();
                cacheManager.Clear();
                //添加错误验证提示
                return Json(new { Success = false, Message = ex.Message });
            }
        }

    }
}