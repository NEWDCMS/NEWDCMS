using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Common;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Report;
using DCMS.Core.Domain.Security;
using DCMS.Core.Domain.Tasks;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Products;
using DCMS.Services.Report;
using DCMS.Services.Security;
using DCMS.Services.Users;
using DCMS.Services.WareHouses;
using DCMS.ViewModel.Models.Common;
using DCMS.ViewModel.Models.Global.Common;
using DCMS.ViewModel.Models.Users;
using DCMS.ViewModel.Models.WareHouses;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using DCMS.Core.Domain.Sales;


namespace DCMS.Web.Controllers
{
    public partial class CommonController : BasePublicController
    {

        private readonly CommonSettings _commonSettings;
        private readonly IPermissionService _permissionService;
        
        private readonly IWebHelper _webHelper;
        private readonly IStaffReportService _staffReportService;
        private readonly IUserService _userService;
        private readonly IModuleService _moduleService;
        private readonly ICategoryService _productCategoryService;
        private readonly IWareHouseService _wareHouseService;
        private readonly IMainPageReportService _mainPageReportService;
        private readonly IBrandService _brandService;
        private readonly IUserAssessmentService _userAssessmentService;
        private readonly ISaleReportService _saleReportService;

        public CommonController(
            INotificationService notificationService,
            IPermissionService permissionService,
            IStaticCacheManager cacheManager,
            CommonSettings commonSettings,
            IUserAssessmentService userAssessmentService,
            IStoreContext storeContext,
            IUserService userService,
            IModuleService moduleService,
            ICategoryService productCategoryService,
            IWareHouseService wareHouseService,
            IMainPageReportService mainPageReportService,
            IBrandService brandService,
            IStaffReportService staffReportService,
            ISaleReportService saleReportService,
            ILogger loggerService,
            IWebHelper webHelper,
            IWorkContext workContext) : base(workContext, loggerService, storeContext, notificationService)
        {
            _permissionService = permissionService;
            _userAssessmentService = userAssessmentService;
            _webHelper = webHelper;
            _commonSettings = commonSettings;
            _staffReportService = staffReportService;
            _userService = userService;
            _moduleService = moduleService;
            _productCategoryService = productCategoryService;
            _wareHouseService = wareHouseService;
            _mainPageReportService = mainPageReportService;
            _brandService = brandService;
            _saleReportService = saleReportService;
        }


        public virtual IActionResult PageNotFound()
        {
            if (_commonSettings.Log404Errors)
            {
                var statusCodeReExecuteFeature = HttpContext?.Features?.Get<IStatusCodeReExecuteFeature>();
                _loggerService.Error($"Error 404. The requested page ({statusCodeReExecuteFeature?.OriginalPath}) was not found",
                    user: _workContext.CurrentUser);
            }
            Response.StatusCode = 404;
            Response.ContentType = "text/html";
            return View();
        }

        public virtual IActionResult GenericUrl()
        {
            return InvokeHttp404();
        }

        //[HttpPost]
        public virtual IActionResult ClearCache(string returnUrl = "")
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.PublicStoreAllowNavigation))
            {
                return AccessDeniedView();
            }

            _userService.ClearSystemCache(curStore.Id); //清理系统缓存

            //home page
            if (string.IsNullOrEmpty(returnUrl))
            {
                //return RedirectToAction("Index", "HomePage", new { area = AreaNames.Admin });
                return RedirectToRoute("Homepage", new { store = curStore.Id });
            }

            //prevent open redirection attack
            if (!Url.IsLocalUrl(returnUrl))
            {
                return RedirectToRoute("Homepage", new { store = curStore.Id });
            }

            return Redirect(returnUrl);
        }

        [HttpPost]
        public virtual IActionResult RestartApplication(string returnUrl = "")
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMaintenance))
            {
                return AccessDeniedView();
            }

            //restart application
            _webHelper.RestartAppDomain();

            //home page
            if (string.IsNullOrEmpty(returnUrl))
            {
                return RedirectToAction("Index", "HomePage");
            }

            //prevent open redirection attack
            if (!Url.IsLocalUrl(returnUrl))
            {
                return RedirectToAction("Index", "HomePage");
            }

            return Redirect(returnUrl);
        }


        public IActionResult SignalrHub()
        {
            var model = new SignalrModel();

            if (curUser != null)
            {
                model.MobilePhone = curUser.MobileNumber;
            }

            return PartialView("_SignalrHub", model);
        }

        private class NewsGroupModel
        {
            public string Mtype { get; set; }
            public List<QueuedMessage> Data { get; set; }
        }

        /// <summary>
        /// 异步获取菜单列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> AsyncLeftSidebar()
        {
            return await Task.Run(() =>
            {
                //获取当前用户的所有模块权限记录
                var permissionIds = _userService.GetUserPermissionRecords(curUser).Select(c => c.Id).ToList();
                var userModules = _userService.GetUserModuleRecords(curStore?.Id ?? 0, curUser).Select(c => c.Id).ToList();
                var allModules = _moduleService.GetModulesByStore(0);
                var modules = GetModuleMenus(0, 0, 3, permissionIds, userModules, allModules);
                return Json(new { Data = modules, Success = true });
            });
        }

        /// <summary>
        /// 界面头部菜单
        /// </summary>
        /// <returns></returns>
        public IActionResult TopSidebar()
        {
            var model = new MenuModel();
            //获取当前用户的所有模块权限记录
            var permissionIds = _userService.GetUserPermissionRecords(curUser).Select(c => c.Id).ToList();
            var modules = _userService.GetUserModuleRecords(curStore?.Id ?? 0, curUser).Select(c => c.Id).ToList();
            //获取系统模块的所有权限集
            var allRecords = _permissionService.GetAllPermissionRecordsByStore(0);
            //获取所有模块
            var allModules = _moduleService.GetModulesByStore(0);
            model.MenuTrees = GetModuleList(0, 0, 0, permissionIds, modules, allModules, allRecords);

            return PartialView("_TopSidebar", model);
        }


        /// <summary>
        /// 异步获取模块列表
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public async Task<JsonResult> AsyncGetList(int page = 0)
        {
            return await Task.Run(() =>
            {
                if (page > 1)
                {
                    page -= 1;
                }

                var gridModel = _moduleService.GetAllModules(store: 0, isShowEnabled: true, pageIndex: page, pageSize: 500);

                return Json(new
                {
                    total = gridModel.TotalCount,
                    rows = gridModel.Select(m =>
                    {
                        return m.ToModel<ModuleModel>();

                    }).OrderBy(m => m.DisplayOrder).ToList()
                });
            });
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

                    //该模块的权限集
                    //优化取值逻辑
                    //var records = _permissionService.GetAllPermissionRecordsByModuleId(0, b.Id);
                    var records = allRecords.Where(r => r.ModuleId == b.Id).ToList();

                    //records.ToList().ForEach(r => {
                    //    //赋有权限且具有View(查看权限时菜单才会显示)
                    //    viewIsContains = permissionIds.Contains(r.Id) && r.Code == StandardPermissionProvider.View.SystemName;
                    //    if (viewIsContains)
                    //        return;
                    //});

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
                                Description = s.Description,
                                Enabled = s.Enabled,
                                CreatedOn = s.CreatedOn
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



        /// <summary>
        /// 递归获取模块树
        /// </summary>
        /// <param name="store">经销商</param>
        /// <param name="Id"></param>
        /// <returns></returns>
        [NonAction]
        private List<BaseModuleTree<BaseModule>> GetModuleMenus(int? store, int Id, int position, List<int> permissionIds, List<int> modules, List<Module> allModules)
        {
            List<BaseModuleTree<BaseModule>> trees = new List<BaseModuleTree<BaseModule>>();
            var perentList = _moduleService.GetNotPaltformModulesByParentId(allModules, store.Value, Id, position);
            if (perentList != null && perentList.Count > 0)
            {
                foreach (var b in perentList)
                {
                    List<BaseModuleTree<BaseModule>> tempList = GetModuleMenus(store.Value, b.Id, position, permissionIds, modules, allModules);
                    var model = b.ToModel<BaseModule>();
                    var node = new BaseModuleTree<BaseModule>
                    {
                        Visible = modules.Contains(b.Id),
                        Module = model,
                        Children = new List<BaseModuleTree<BaseModule>>()
                    };
                    if (tempList != null && tempList.Count > 0)
                    {
                        node.Children = tempList;
                    }
                    trees.Add(node);
                }
            }
            return trees;
        }



        /// <summary>
        /// 快速调拨
        /// </summary>
        public IActionResult AsynQuickAllocationPopup(int allocationType = 0, int wareHouseId = 0)
        {


            QuickAllocationModel model = new QuickAllocationModel
            {
                AllocationTypeId = allocationType,

                //送货员
                DeliveryUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Delivers),
                DeliveryUserId = -1,
                //商品类别
                Categories = BindCategorySelection(new Func<int?, IList<Category>>(_productCategoryService.GetAllCategoriesDisplayed), curStore),
                CategoryId = -1
            };


            string whn = _wareHouseService.GetWareHouseName(curStore.Id, wareHouseId);
            //string day = DateTime.Now.ToString("yyyy-MM-dd");
            var sl = new List<SelectListItem>();
            switch ((AllocationTypeEnum)allocationType)
            {
                //按拒收商品调拨
                case AllocationTypeEnum.ByRejection:
                    {
                        sl.Add(new SelectListItem() { Text = "加载 " + whn + $" 今天({ DateTime.Now.ToString("yyyy-MM-dd") })拒收的商品", Value = $"{QuickAllocationEnum.LoadRejectionToday}" });
                        sl.Add(new SelectListItem() { Text = "加载 " + whn + $" 昨天({ DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd") })拒收的商品", Value = $"{QuickAllocationEnum.LoadRejectionYestDay}" });
                        sl.Add(new SelectListItem() { Text = "加载 " + whn + $" 前天({ DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd") })拒收的商品", Value = $"{QuickAllocationEnum.LoadRejectionBeforeYestday}" });
                    }
                    break;
                case AllocationTypeEnum.BySaleAdd:
                    {
                        sl.Add(new SelectListItem() { Text = "加载 " + whn + " 今天销售的商品", Value = $"{QuickAllocationEnum.LoadSaleToday}" });
                        sl.Add(new SelectListItem() { Text = "加载 " + whn + " 昨天销售的商品", Value = $"{QuickAllocationEnum.LoadSaleYestDay}" });
                        sl.Add(new SelectListItem() { Text = "加载 " + whn + " 近三天销售的商品", Value = $"{QuickAllocationEnum.LoadSaleNearlyThreeDays}" });
                        sl.Add(new SelectListItem() { Text = "加载 " + whn + " 上次调拨后销售的商品", Value = $"{QuickAllocationEnum.LoadSaleLast}" });
                    }
                    break;
                case AllocationTypeEnum.ByReturn:
                    {
                        sl.Add(new SelectListItem() { Text = "加载 " + whn + " 今天退货的商品", Value = $"{QuickAllocationEnum.LoadReturnToday}" });
                        sl.Add(new SelectListItem() { Text = "加载 " + whn + " 昨天退货的商品", Value = $"{QuickAllocationEnum.LoadReturnYestDay}" });
                        sl.Add(new SelectListItem() { Text = "加载 " + whn + " 前天退货的商品", Value = $"{QuickAllocationEnum.LoadReturnBeforeYestday}" });
                    }
                    break;
                case AllocationTypeEnum.ByStock:

                    break;
                default:
                    break;
            }

            model.LoadDataId = -1;
            model.LoadDatas = new SelectList(sl);

            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("QuickAllocation", model)
            });
        }


        /// <summary>
        /// 获取今日相关数据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> GetDashboardReport()
        {
            return await Task.Run(() =>
            {
                var model = new DashboardReport();

                int[] businessUserIds = _userService.GetSubordinate(curStore.Id, curUser.Id)?.ToArray();

                model = _mainPageReportService.GetDashboardReport(curStore.Id, businessUserIds, false);

                return Json(new { Data = model, Success = true });
            });
        }


        /// <summary>
        /// 获取当日销量
        /// </summary>
        /// <param name="businessUserId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> GetBussinessVisitStoreReport(int? businessUserId = null,DateTime? start = null, DateTime? end = null)
        {
            if (start.HasValue)
            {
                start = DateTime.Parse(start.Value.ToString("yyyy-MM-dd 00:00:00"));
            }
            if (end.HasValue)
            {
                end = DateTime.Parse(end.Value.ToString("yyyy-MM-dd 23:59:59"));
            }
            return await Task.Run(() =>
            {
                int[] businessUserIds = null;
                if (businessUserId.HasValue)
                {
                    businessUserIds = new int[] { businessUserId.Value };
                }
                else 
                {
                    businessUserIds = _userService.GetSubordinate(curStore.Id, curUser.Id, "Salesmans")?.ToArray();
                }

                var models = new List<BussinessVisitStoreModel>();

                var vms = _mainPageReportService.GetBussinessVisitStoreReport(curStore.Id, start, end, businessUserIds).ToList();

                for (var i = 0; i < businessUserIds.Count(); i++)
                {
                    var currentData = vms.Where(c => c.BusinessUserId == businessUserIds[i]).FirstOrDefault();
                    var vmModel = new BussinessVisitStoreModel();
                    if (currentData != null)
                    {
                        vmModel.BussinessUserName = currentData.BussinessUserName;
                        vmModel.Data = currentData.VisitStoreAmount;
                    }
                    else
                    {
                        vmModel.BussinessUserName = _userService.GetUserName(curStore.Id, businessUserIds[i]);
                        vmModel.Data = 0;
                    }
                    models.Add(vmModel);
                }
                return Json(new { Data = models, Success = true });
            });
        }


        /// <summary>
        /// 业务分析
        /// </summary>
        /// <param name="store"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> GetBusinessAnalysis(int type, DateTime? start = null, DateTime? end = null)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var model = _saleReportService.GetBusinessAnalysis(type, curStore.Id, start, end);
                    return Json(new { Data = model, Success = true });
                }
                catch (Exception)
                {
                    return Json(new { Success = false });
                }
            });
        }


        //获取当月销量
        [HttpGet]
        public async Task<JsonResult> GetMonthSaleReport(int[] brandIds)
        {
            return await Task.Run(() =>
            {
                int[] businessUserIds = _userService.GetSubordinate(curStore.Id, curUser.Id)?.ToArray();
                var model = new List<MonthSaleReportModel>();

                var start = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01"));
                var end = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));


                var vms = _mainPageReportService.GetMonthSaleReport(curStore.Id, start, end, brandIds, businessUserIds).ToList();
                var months = new List<string>();
                for (var i = 1; i <= 12; i++)
                {
                    if (i < 10)
                    {
                        months.Add(DateTime.Now.Year.ToString() + "-0" + i);
                    }
                    else
                    {
                        months.Add(DateTime.Now.Year.ToString() + "-" + i);
                    }
                }

                model = vms.GroupBy(x => x.BrandId).Select(g =>
                {
                    if (g != null)
                    {
                        var brandName = vms.FirstOrDefault(x => x.BrandId == g.Key).BrandName;
                        var data = new List<decimal>();
                        var currentDatas = vms.Where(c => c.BrandId == g.Key).ToList();

                        for (var i = 0; i < months.Count; i++)
                        {
                            if (!currentDatas.Select(c => c.SaleDate).Contains(months[i]))
                            {
                                data.Add(0);
                            }
                            else
                            {
                                data.Add(currentDatas.FirstOrDefault(c => c.SaleDate == months[i]).SaleAmount);
                            }
                        }

                        var vmsModel = new MonthSaleReportModel
                        {
                            Name = brandName,
                            Data = data
                        };
                        return vmsModel ?? new MonthSaleReportModel();
                    }
                    else
                    {
                        return new MonthSaleReportModel();
                    }
                }).ToList();

                return Json(new { Data = model, Success = true });

            });
        }

        //获取当月销量进度
        [HttpGet]
        public async Task<JsonResult> GetBussinessSalePercentReport(int type,int year)
        {
            return await Task.Run(() =>
            {
                int[] businessUserIds = _userService.GetSubordinate(curStore.Id, curUser.Id, "Salesmans")?.ToArray();
                var models = new List<BussinessSalePercentModel>();
      
                var vms1 = _staffReportService.GetStaffReportBusinessUserAchievement(curStore?.Id ?? 0,
                null, 
                null, 
                null, 
                null, 
                DateTime.Now.AddDays(-DateTime.Now.Day + 1),
                DateTime.Now.AddMonths(1).AddDays(-DateTime.Now.Day), 
                true
                ).Select(sd =>
                {
                    return sd == null ? new StaffReportBusinessUserAchievement() : sd;

                }).AsQueryable();

                UserAssessmentModel vms2 = _userAssessmentService.GetUserAssessmentByStoreId(curStore.Id, year)?.ToModel<UserAssessmentModel>();
                if (vms2 != null)
                {
                    vms2.Items = _userAssessmentService.GetUserAssessmentItems(curStore.Id, vms2.Id, businessUserIds.ToList()).Select(item => item.ToModel<UserAssessmentItemModel>()).ToList();
                }
                else {
                    vms2 = new UserAssessmentModel();
                    vms2.Items = new List<UserAssessmentItemModel>();
                    for (var i = 0; i < businessUserIds.Count(); i++)
                    {
                        vms2.Items.Add(new UserAssessmentItemModel());
                    }
                }

                for (var i = 0; i < businessUserIds.Count(); i++)
                {
                    var currentData = vms1.Where(c => c.BusinessUserId == businessUserIds[i]).FirstOrDefault();
                    var sumData = vms2.Items.Where(c => c.UserId == businessUserIds[i]).FirstOrDefault();
                    var vmModel = new BussinessSalePercentModel();
                    var month = 0;
                    decimal amount = 0;
                    if (sumData != null)
                    {
                        switch (type)
                        {
                            case 8:
                                month = DateTime.Now.Month;
                                break;
                        }
                        switch (month)
                        {
                            case 1:
                                amount = sumData.Jan;
                                break;
                            case 2:
                                amount = sumData.Feb;
                                break;
                            case 3:
                                amount = sumData.Mar;
                                break;
                            case 4:
                                amount = sumData.Apr;
                                break;
                            case 5:
                                amount = sumData.May;
                                break;
                            case 6:
                                amount = sumData.Jun;
                                break;
                            case 7:
                                amount = sumData.Jul;
                                break;
                            case 8:
                                amount = sumData.Aug;
                                break;
                            case 9:
                                amount = sumData.Sep;
                                break;
                            case 10:
                                amount = sumData.Oct;
                                break;
                            case 11:
                                amount = sumData.Nov;
                                break;
                            case 12:
                                amount = sumData.Dec;
                                break;
                        }
                    }
                    if (currentData != null)
                    {
                        vmModel.BussinessUserName = currentData.BusinessUserName;
                        vmModel.SaleAmount = currentData.SaleAmount ?? 0;
                        vmModel.SumAmount = amount;
                    }
                    else
                    {
                        vmModel.BussinessUserName = _userService.GetUserName(curStore.Id, businessUserIds[i]);
                        vmModel.SaleAmount = 0;
                        vmModel.SumAmount = amount;

                    }
                    models.Add(vmModel);
                }
                return Json(new { Data = models, Success = true });
            });
        }

        [HttpGet]
        public async Task<JsonResult> GetBrandList()
        {
            return await Task.Run(() =>
            {
                return Json(new { Data = _brandService.GetAllBrands(curStore?.Id ?? 0), Success = true });
            });

        }

        /// <summary>
        /// 异步获取指定模组
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet]
        //[ResponseCache(Duration = 600, VaryByQueryKeys = new string[] { "Id" })]
        public async Task<JsonResult> AsyncGetModuleById(int Id)
        {
            return await Task.Run(() =>
            {


                var module = _moduleService.GetModuleById(Id).ToModel<ModuleModel>();

                return Json(new { data = module, Success = true });
            });
        }
        private class MonthSaleReportModel
        {
            public string Name { get; set; }
            public List<decimal> Data { get; set; }
        }

        private class BussinessVisitStoreModel
        {
            public string BussinessUserName { get; set; }
            public decimal Data { get; set; }
        }
        private class BussinessSalePercentModel
        {
            public int BusinessUserId { get; set; }
            public string BussinessUserName { get; set; }
            public decimal SaleAmount { get; set; }
            public decimal SumAmount { get; set; }
        }

        /// <summary>
        /// 存储本地消息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("message/save")]
        public JsonResult Storage(QueuedMessage message, int userId)
        {
            try
            {
                //string key = $"LOCALSTORAGES_{userId}";
                //var localStorages = _cacheManager.GetPersist<List<QueuedMessage>>(key);

                //if (localStorages == null)
                //    localStorages = new List<QueuedMessage>();

                ////最多保存100条，超过100条，删除最早数据
                //if (localStorages.Count > 100)
                //{
                //    localStorages.RemoveRange(0, localStorages.Count - 100);
                //}
                ////如果缓存中存在当前消息,清除当前消息
                //localStorages.RemoveAll(a => a.Id == message.Id);
                ////将当前消息添加到末尾
                //localStorages.Add(message);

                //_cacheManager.SetPersist(key, localStorages);

                return Json(new { Success = true });

            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [Route("message/get")]
        public JsonResult GetStorage(int userId)
        {
            try
            {
                string key = $"LOCALSTORAGES_{userId}";
                //var localStorages = _cacheManager.GetPersist<List<QueuedMessage>>(key)?.Distinct();
                //return Json(new { Success = true, Data = localStorages });
                return Json(new { Success = true });
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }


        [Route("message/delete")]
        public JsonResult DeleteStorage(int userId, int messageId)
        {
            try
            {
                //string key = $"LOCALSTORAGES_{userId}";
                //var localStorages = _cacheManager.GetPersist<List<QueuedMessage>>(key);
                //if (messageId == 0) //清除全部
                //{
                //    localStorages = new List<QueuedMessage>();
                //}
                //else //单笔删除
                //{
                //    //var message = localStorages.FirstOrDefault(c => c.Id == messageId);
                //    //if (message != null)
                //    //{
                //    //    localStorages.Remove(message);
                //    //}
                //    //根据messageId删除
                //    localStorages?.RemoveAll(a => a.Id == messageId);

                //}
                //_cacheManager.SetPersist(key, localStorages);

                return Json(new { Success = true });
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }


        /// <summary>
        /// 获取待处理事项统计Common/GetPendingMatters?userId=1754
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> GetPendingMatters(int? userId)
        {
            if (!userId.HasValue)
            {
                userId = curUser.Id;
            }

            return await Task.Run(() =>
            {
                var ids = _userService.GetSubordinate(curStore.Id, userId ?? 0)?.ToArray();
                var pending = new PendingCount
                {
                    //订货单
                    OrderCount = 0,
                    //退货订单
                    ReturnOrderCount = 0,
                    //待审核
                    AuditCount = _mainPageReportService.GetPendingCount(curStore.Id, ids),
                    //销售订单	
                    SaleOrderCount = _mainPageReportService.GetOrderCount(curStore.Id, ids),
                    //销售单
                    SaleCount = _mainPageReportService.GetSaleCount(curStore.Id, ids),
                    //退货单
                    ReturnCount = _mainPageReportService.GetReturnCount(curStore.Id, ids),
                    //调拨单
                    AllocationCount = _mainPageReportService.GetAllocationCount(curStore.Id, ids),
                    //收款单
                    CashReceiptCount = _mainPageReportService.GetCashReceiptCount(curStore.Id, ids),
                    //待调度
                    DispatchCount = _mainPageReportService.GetDispatchCount(curStore.Id, ids),
                    //待转单
                    ChangeCount = _mainPageReportService.GetChangeCount(curStore.Id, ids)
                };

                return Json(new { Data = pending, Success = true });
            });

        }
    }
}