using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Users;
using DCMS.Core.Domain.WareHouses;
using DCMS.Services.Common;
using DCMS.Services.Configuration;
using DCMS.Services.ExportImport;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Products;
using DCMS.Services.Users;
using DCMS.Services.WareHouses;
using DCMS.ViewModel.Models.Products;
using DCMS.ViewModel.Models.WareHouses;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using DCMS.Web.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCMS.Web.Controllers
{
    /// <summary>
    /// 用于调拨单管理
    /// </summary>
    public class AllocationController : BasePublicController
    {
        
        private readonly ICategoryService _categoryService;
        private readonly IBrandService _brandService;
        private readonly IPrintTemplateService _printTemplateService;
        private readonly IUserService _userService;
        private readonly IUserActivityService _userActivityService;
        private readonly ISettingService _settingService;
        private readonly IWareHouseService _wareHouseService;
        private readonly IAllocationBillService _allocationBillService;
        private readonly IProductService _productService;
        private readonly IStockService _stockService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IMediaService _mediaService;
        private readonly IRedLocker _locker;
        private readonly IExportManager _exportManager;

        public AllocationController(
            IWorkContext workContext,
            IStaticCacheManager cacheManager,
            ICategoryService categoryService,
            IBrandService brandService,
            IStoreContext storeContext,
            IPrintTemplateService printTemplateService,
            IUserActivityService userActivityService,
            ISettingService settingService,
            IWareHouseService wareHouseService,
            IAllocationBillService allocationBillService,
            IMediaService mediaService,
            IProductService productService,
            IStockService stockService,
            ISpecificationAttributeService specificationAttributeService,
            IUserService userService,
            INotificationService notificationService,
            ILogger loggerService,
            IRedLocker locker,
            IExportManager exportManager
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            
            _categoryService = categoryService;
            _brandService = brandService;
            _printTemplateService = printTemplateService;
            _userService = userService;
            _userActivityService = userActivityService;
            _settingService = settingService;
            _wareHouseService = wareHouseService;
            _allocationBillService = allocationBillService;
            _mediaService = mediaService;
            _productService = productService;
            _stockService = stockService;
            _specificationAttributeService = specificationAttributeService;
            _locker = locker;
            _exportManager = exportManager;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="shipmentWareHouseId">出货仓库Id</param>
        /// <param name="incomeWareHouseId">入货仓库Id</param>
        /// <param name="billNumber">单据号</param>
        /// <param name="auditedStatus">状态</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="showReverse">显示红冲数据</param>
        /// <param name="sortByAuditedTime">按审核时间</param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.AllocationFormView)]
        public IActionResult List(int? shipmentWareHouseId, int? incomeWareHouseId, string billNumber = "", bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? showReverse = null, bool? sortByAuditedTime = null, string remark = "", int? productId = 0, string productName = "", int pagenumber = 0)
        {

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var model = new AllocationBillListModel();

            #region 绑定数据源

            var ws = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore, WHAEnum.AllocationBill, curUser.Id);
            model.ShipmentWareHouses = ws;
            model.ShipmentWareHouseId = (shipmentWareHouseId ?? null);

            model.IncomeWareHouses = ws;
            model.IncomeWareHouseId = incomeWareHouseId ?? null;

            model.BillNumber = billNumber;

            model.AuditedStatus = auditedStatus;

            model.StartTime = (startTime == null) ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")) : (DateTime)startTime;
            model.EndTime = (endTime == null) ? DateTime.Now.AddDays(1) : (DateTime)endTime;

            model.ShowReverse = showReverse;

            model.SortByAuditedTime = sortByAuditedTime;
            model.ProductId = productId ?? 0;
            model.ProductName = productName;

            #endregion

            //获取分页
            var bills = _allocationBillService.GetAllAllocationBills(curStore?.Id ?? 0,
                curUser.Id,
                null,
                shipmentWareHouseId,
                incomeWareHouseId,
                billNumber,
                auditedStatus,
                model.StartTime,
                model.EndTime,
                showReverse,
                false,
                productId,
                pagenumber,
                30);

            model.PagingFilteringContext.LoadPagedList(bills);

            #region 查询需要关联其他表的数据

            List<int> warehouseIds = new List<int>();
            warehouseIds.AddRange(bills.Select(b => b.ShipmentWareHouseId).Distinct().ToArray());
            warehouseIds.AddRange(bills.Select(b => b.IncomeWareHouseId).Distinct().ToArray());
            var allWarehouses = _wareHouseService.GetWareHouseByIds(curStore.Id, warehouseIds.Distinct().ToArray());

            #endregion

            List<int> userIds = new List<int>();
            userIds.AddRange(bills.Select(b => b.MakeUserId).Distinct().ToArray());
            var allUsers = _userService.GetUsersDictsByIds(curStore.Id, userIds.Distinct().ToArray());

            model.Lists = bills.OrderByDescending(b => b.CreatedOnUtc).Select(b =>
            {
                var m = b.ToModel<AllocationBillModel>();

                //出货仓库
                var warehouse = allWarehouses.Where(aw => aw.Id == b.ShipmentWareHouseId).FirstOrDefault();
                m.ShipmentWareHouseName = warehouse == null ? "" : warehouse.Name;

                //入货仓库
                var warehouse2 = allWarehouses.Where(aw => aw.Id == b.IncomeWareHouseId).FirstOrDefault();
                m.IncomeWareHouseName = warehouse2 == null ? "" : warehouse2.Name;

               //开单人
                m.MakeUserName = allUsers.Where(au => au.Key == b.MakeUserId).Select(au => au.Value).FirstOrDefault();

                return m;
            }).ToList();

            return View(model);
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.AllocationFormSave)]
        public IActionResult Create(int? store, int? ModelType = 0, string data = "")
        {
            //获取配置
            var companySetting = _settingService.LoadSetting<CompanySetting>(curStore.Id);

            var ws = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore, WHAEnum.AllocationBill, curUser.Id);
            var model = new AllocationBillModel
            {
                CreatedOnUtc = DateTime.Now,

                ShipmentWareHouses = ws,
                ShipmentWareHouseId = -1,

                IncomeWareHouses = ws,
                IncomeWareHouseId = -1
            };

            //单号
            model.BillNumber = CommonHelper.GetBillNumber(CommonHelper.GetEnumDescription(BillTypeEnum.AllocationBill).Split(',')[1], curStore.Id);
            model.BillBarCode = _mediaService.GenerateBarCodeForBase64(model.BillNumber, 150, 50);
            //制单人
            var mu = _userService.GetUserById(curStore.Id, curUser.Id);
            model.MakeUserName = mu != null ? (mu.UserRealName + " " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")) : "";

            //加载其他页面传输过来的数据
            if (!string.IsNullOrEmpty(data))
            {
                List<string> lst = data.Split(',').ToList();
                if (lst != null && lst.Count > 0)
                {
                    foreach (string it in lst)
                    {
                        model.Items.Add(new AllocationItemModel() { ProductId = int.Parse(it.Split('|')[0]), Quantity = int.Parse(it.Split('|')[1]) });
                    }
                }
                model.ModelLoadData = data;
            }
            //显示订单占用库存
            model.APPShowOrderStock = companySetting.APPShowOrderStock;

            return View(model);
        }

        /// <summary>
        /// 编辑
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.AllocationFormView)]
        public IActionResult Edit(int id = 0)
        {
            var model = new AllocationBillModel();
            //获取配置
            var companySetting = _settingService.LoadSetting<CompanySetting>(curStore.Id);

            var allocationBill = _allocationBillService.GetAllocationBillById(curStore.Id, id, true);
            if (allocationBill == null)
            {
                return RedirectToAction("List");
            }

            if (allocationBill != null)
            {
                if (allocationBill.StoreId != curStore.Id)
                {
                    return RedirectToAction("List");
                }

                model = allocationBill.ToModel<AllocationBillModel>();
                model.BillBarCode = _mediaService.GenerateBarCodeForBase64(allocationBill.BillNumber, 150, 50);
                //model.Items = _allocationBillService.GetAllocationItemsByAllocationBillId(model.Id, curStore.Id, 0, 0, 30).Select(a => a.ToModel<AllocationItemModel>()).ToList();
                model.Items = allocationBill.Items.Select(a => a.ToModel<AllocationItemModel>()).ToList();
            }

            var ws = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore, WHAEnum.AllocationBill, curUser.Id);
            model.ShipmentWareHouses = ws;
            model.IncomeWareHouses = ws;

            //制单人
            //var mu = _userService.GetUserById(curStore.Id, allocationBill.MakeUserId);
            //model.MakeUserName = mu != null ? (mu.UserRealName + " " + allocationBill.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss")) : "";
            var mu = string.Empty;
            if (allocationBill.MakeUserId > 0)
            {
                mu = _userService.GetUserName(curStore.Id, allocationBill.MakeUserId);
            }
            model.MakeUserName = mu + " " + allocationBill.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss");

            //审核人
            //var au = _userService.GetUserById(curStore.Id, allocationBill.AuditedUserId ?? 0);
            //model.AuditedUserName = au != null ? (au.UserRealName + " " + (allocationBill.AuditedDate.HasValue ? allocationBill.AuditedDate.Value.ToString("yyyy/MM/dd HH:mm:ss") : "")) : "";
            var au = string.Empty;
            if (allocationBill.AuditedUserId != null && allocationBill.AuditedUserId > 0)
            {
                au = _userService.GetUserName(curStore.Id, allocationBill.AuditedUserId ?? 0);
            }
            model.AuditedUserName = au + " " + (allocationBill.AuditedDate.HasValue ? allocationBill.AuditedDate.Value.ToString("yyyy/MM/dd HH:mm:ss") : "");

            //显示订单占用库存
            model.APPShowOrderStock = companySetting.APPShowOrderStock;

            return View(model);
        }

        #region 单据项目

        /// <summary>
        /// 异步获取项目
        /// </summary>
        /// <param name="billId"></param>
        /// <returns></returns>
        public JsonResult AsyncAllocationItems(int billId, string data)
        {

            var gridModel = _allocationBillService.GetAllocationItemList(billId);

            //所有涉及商品
            List<int> productIds = new List<int>();
            productIds.AddRange(gridModel.Select(p => p.ProductId));
            if (!string.IsNullOrEmpty(data))
            {
                List<string> lst = data.Split(',').ToList();
                if (lst != null && lst.Count > 0)
                {
                    foreach (string it in lst)
                    {
                        productIds.Add(int.Parse(it.Split('|')[0]));
                    }
                }
            }
            var allProducts = _productService.GetProductsByIds(curStore.Id, productIds.Distinct().ToArray());
            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());
            var allProductPrices = _productService.GetProductPricesByProductIds(curStore.Id, productIds.Distinct().ToArray());
            var allProductTierPrices = _productService.GetProductTierPriceByProductIds(curStore.Id, productIds.Distinct().ToArray());

            var items = gridModel.Select(o =>
            {
                var m = o.ToModel<AllocationItemModel>();

                var product = allProducts.Where(ap => ap.Id == m.ProductId).FirstOrDefault();
                if (product != null)
                {
                    //这里替换成高级用法
                    m = product.InitBaseModel<AllocationItemModel>(m, o.AllocationBill.ShipmentWareHouseId, allOptions, allProductPrices, allProductTierPrices, _productService);

                    //商品信息
                    m.BigUnitId = product.BigUnitId;
                    m.StrokeUnitId = product.StrokeUnitId;
                    m.SmallUnitId = product.SmallUnitId;
                }

                m.TradePrice ??= 0;
                m.WholesaleAmount ??= 0;

                return m;

            }).ToList();

            //加载其他页面传过来的数量
            if (!string.IsNullOrEmpty(data))
            {
                List<string> lst = data.Split(',').ToList();
                if (lst != null && lst.Count > 0)
                {
                    foreach (string it in lst)
                    {
                        var m = new AllocationItemModel
                        {
                            ProductId = int.Parse(it.Split('|')[0]),
                            Quantity = int.Parse(it.Split('|')[1])
                        };

                        var product = _productService.GetProductById(curStore.Id, int.Parse(it.Split('|')[0]));
                        if (product != null)
                        {
                            //items.Add(new AllocationItemModel() { ProductId = product.Id, ProductName = product.Name, UnitId = product.SmallUnitId, Quantity = int.Parse(it.Split('|')[1]) });
                            //这里替换成高级用法
                            m = product.InitBaseModel<AllocationItemModel>(m, 0, allOptions, allProductPrices, allProductTierPrices, _productService);
                            m.UnitId = product.SmallUnitId;
                            m.UnitName = allOptions.Where(s => s.Id == m.UnitId).Select(s => s.Name).FirstOrDefault();
                            m.TradePrice = 0;
                            m.WholesaleAmount = 0;
                            items.Add(m);
                        }
                    }
                }
            }

            return Json(new
            {
                Success = true,
                total = items.Count,
                rows = items
            });
        }

        /// <summary>
        /// 创建/更新
        /// </summary>
        /// <param name="data"></param>
        /// <param name="billId"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.AllocationFormSave)]
        public async Task<JsonResult> CreateOrUpdate(AllocationUpdateModel data, int? billId,bool doAudit = true)
        {
            try
            {
                if (data != null)
                {
                    var bill = new AllocationBill();

                    if (data == null || data.Items == null)
                    {
                        return Warning("请录入数据.");
                    }

                    if (PeriodLocked(DateTime.Now))
                    {
                        return Warning("锁账期间,禁止业务操作.");
                    }

                    if (PeriodClosed(DateTime.Now))
                    {
                        return Warning("会计期间已结账,禁止业务操作.");
                    }

                    #region 单据验证
                    if (billId.HasValue && billId.Value != 0)
                    {
                        bill = _allocationBillService.GetAllocationBillById(curStore.Id, billId.Value, true);

                        //公共单据验证
                        var commonBillChecking = BillChecking<AllocationBill, AllocationItem>(bill, BillStates.Draft);
                        if (commonBillChecking.Value != null)
                        {
                            return commonBillChecking;
                        }
                    }
                    #endregion

                    #region 验证盘点

                    if (data != null && data.Items != null && data.Items.Count > 0)
                    {
                        if (_wareHouseService.CheckProductInventory(curStore.Id, data.IncomeWareHouseId, bill.Items?.Select(it => it.ProductId).Distinct().ToArray(), out string thisMsg))
                        {
                            return Warning(thisMsg);
                        }
                    }

                    #endregion

                    #region 验证库存
                    IList<Product> allProducts = new List<Product>();

                    //当前数据
                    List<ProductStockItem> productStockItemNews = new List<ProductStockItem>();
                    if (data.Items != null && data.Items.Count > 0)
                    {
                        allProducts = _productService.GetProductsByIds(curStore.Id, data.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                        var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());
                        foreach (AllocationItemModel item in data.Items)
                        {
                            if (item.ProductId != 0)
                            {
                                var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                                if (product != null)
                                {
                                    ProductStockItem productStockItem = productStockItemNews.Where(a => a.ProductId == item.ProductId).FirstOrDefault();
                                    //商品转化量
                                    var conversionQuantity = product.GetConversionQuantity(allOptions, item.UnitId);
                                    //库存量增量 = 单位转化量 * 数量
                                    int thisQuantity = item.Quantity * conversionQuantity;
                                    if (productStockItem != null)
                                    {
                                        productStockItem.Quantity += thisQuantity;
                                    }
                                    else
                                    {
                                        productStockItem = new ProductStockItem
                                        {
                                            ProductId = item.ProductId,
                                            //当期选择单位
                                            UnitId = product.SmallUnitId,
                                            //转化单位
                                            SmallUnitId = product.SmallUnitId,
                                            //转化单位
                                            BigUnitId = product.BigUnitId ?? 0,
                                            ProductName = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault()?.Name,
                                            ProductCode = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault()?.ProductCode,
                                            Quantity = thisQuantity
                                        };

                                        productStockItemNews.Add(productStockItem);
                                    }
                                }
                            }

                        }
                    }

                    //验证库存
                    string thisMsg2 = string.Empty;
                    var fg = _stockService.CheckStockQty(_productService, _specificationAttributeService, curStore.Id, data.ShipmentWareHouseId, productStockItemNews, out thisMsg2);
                    if (fg == false)
                    {
                        return Warning(thisMsg2);
                    }

                    #endregion

                    //业务逻辑
                    var dataTo = data.ToEntity<AllocationBillUpdate>();
                    dataTo.Operation = (int)OperationEnum.PC;
                    dataTo.Items = data.Items.Select(it =>
                    {
                        return it.ToEntity<AllocationItem>();
                    }).ToList();

                    //RedLock
                    var result = await _locker.PerformActionWithLockAsync(LockKey(data),
                         TimeSpan.FromSeconds(30),
                         TimeSpan.FromSeconds(10),
                         TimeSpan.FromSeconds(1),
                         () => _allocationBillService.BillCreateOrUpdate(curStore.Id, curUser.Id, billId, bill, dataTo, dataTo.Items, productStockItemNews, _userService.IsAdmin(curStore.Id, curUser.Id), doAudit));

                    return Json(result);
                }
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("CreateOrUpdate", Resources.Bill_CreateOrUpdateFailed, curUser.Id);
                _notificationService.SuccessNotification(Resources.Bill_CreateOrUpdateFailed);
                return Error(ex.Message);
            }

            _userActivityService.InsertActivity("CreateOrUpdate", Resources.CreateOrUpdate, curUser.Id);
            _notificationService.SuccessNotification(Resources.Bill_CreateOrUpdateSuccessful);
            return Json(new { Success = true, Message = Resources.Bill_CreateOrUpdateSuccessful });

        }

        #endregion

        /// <summary>
        /// 审核
        /// </summary>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.AllocationFormApproved)]
        public async Task<JsonResult> Auditing(int? id)
        {
            try
            {

                var bill = new AllocationBill();

                #region 验证

                if (!id.HasValue)
                {
                    return Warning("参数错误.");
                }
                else
                {
                    bill = _allocationBillService.GetAllocationBillById(curStore.Id, id.Value, true);
                    if (bill.AuditedStatus)
                    {
                        return Warning("单据已审核，请刷新页面.");
                    }
                }

                //公共单据验证
                var commonBillChecking = BillChecking<AllocationBill, AllocationItem>(bill, BillStates.Audited);
                if (commonBillChecking.Value != null)
                {
                    return commonBillChecking;
                }

                #region 验证库存
                IList<Product> allProducts = new List<Product>();

                //当前数据
                List<ProductStockItem> productStockItemNews = new List<ProductStockItem>();
                if (bill.Items != null && bill.Items.Count > 0)
                {
                    allProducts = _productService.GetProductsByIds(curStore.Id, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());

                    foreach (AllocationItem item in bill.Items)
                    {
                        if (item.ProductId != 0)
                        {
                            var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                            if (product != null)
                            {
                                ProductStockItem productStockItem = productStockItemNews.Where(a => a.ProductId == item.ProductId).FirstOrDefault();
                                //商品转化量
                                var conversionQuantity = product.GetConversionQuantity(allOptions, item.UnitId);
                                //库存量增量 = 单位转化量 * 数量
                                int thisQuantity = item.Quantity * conversionQuantity;
                                if (productStockItem != null)
                                {
                                    productStockItem.Quantity += thisQuantity;
                                }
                                else
                                {
                                    productStockItem = new ProductStockItem
                                    {
                                        ProductId = item.ProductId,
                                        //当期选择单位
                                        UnitId = product.SmallUnitId,
                                        //转化单位
                                        SmallUnitId = product.SmallUnitId,
                                        //转化单位
                                        BigUnitId = product.BigUnitId ?? 0,
                                        ProductName = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault()?.Name,
                                        ProductCode = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault()?.ProductCode,
                                        Quantity = thisQuantity
                                    };

                                    productStockItemNews.Add(productStockItem);
                                }
                            }
                        }

                    }
                }

                //验证库存
                if (!_stockService.CheckStockQty(_productService, _specificationAttributeService, curStore.Id, bill.ShipmentWareHouseId, productStockItemNews, out string errMsg))
                {
                    return Warning(errMsg);
                }

                #endregion

                #endregion

                //RedLock
                var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill),
                      TimeSpan.FromSeconds(30),
                      TimeSpan.FromSeconds(10),
                      TimeSpan.FromSeconds(1),
                      () => _allocationBillService.Auditing(curStore.Id, curUser.Id, bill));
                return Json(result);

            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("Auditing", "单据审核失败", curUser.Id);
                _notificationService.SuccessNotification("单据审核失败");
                return Error(ex.Message);
            }

        }

        /// <summary>
        /// 红冲
        /// </summary>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.AllocationFormReverse)]
        public async Task<JsonResult> Reverse(int? id)
        {
            try
            {

                var bill = new AllocationBill() { StoreId = curStore?.Id ?? 0 };


                #region 验证

                if (PeriodClosed(DateTime.Now))
                {
                    return Warning("系统当月已经结转，不允许红冲");
                }

                if (!id.HasValue)
                {
                    return Warning("参数错误.");
                }
                else
                {
                    bill = _allocationBillService.GetAllocationBillById(curStore.Id, id.Value, true);
                    if (bill.ReversedStatus)
                    {
                        return Warning("单据已红冲，请刷新页面.");
                    }
                }

                //公共单据验证
                var commonBillChecking = BillChecking<AllocationBill, AllocationItem>(bill, BillStates.Reversed);
                if (commonBillChecking.Value != null)
                {
                    return commonBillChecking;
                }

                #region 验证库存
                //将一个单据中 相同商品 数量 按最小单位汇总
                List<ProductStockItem> productStockItems = new List<ProductStockItem>();

                var allProducts = _productService.GetProductsByIds(curStore.Id, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());
                foreach (AllocationItem item in bill.Items)
                {
                    var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                    ProductStockItem productStockItem = productStockItems.Where(a => a.ProductId == item.ProductId).FirstOrDefault();
                    //商品转化量
                    var conversionQuantity = product.GetConversionQuantity(allOptions, item.UnitId);
                    //库存量增量 = 单位转化量 * 数量
                    int thisQuantity = item.Quantity * conversionQuantity;
                    if (productStockItem != null)
                    {
                        productStockItem.Quantity += thisQuantity;
                    }
                    else
                    {
                        productStockItem = new ProductStockItem
                        {
                            ProductId = item.ProductId,
                            //当期选择单位
                            UnitId = product.SmallUnitId,
                            //转化单位
                            SmallUnitId = product.SmallUnitId,
                            //转化单位
                            BigUnitId = product.BigUnitId ?? 0,
                            ProductName = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault()?.Name,
                            ProductCode = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault()?.ProductCode,
                            Quantity = thisQuantity
                        };

                        productStockItems.Add(productStockItem);
                    }
                }
                //验证当前商品库存
                if (!_stockService.CheckStockQty(_productService, _specificationAttributeService, bill.StoreId, bill.IncomeWareHouseId, productStockItems, out string thisMsg2))
                {
                    return Warning(thisMsg2);
                }
                #endregion


                #region 验证流程下个节点是否已红冲或不存在
                if (!CheckNextNodeReversed(curStore, bill.Id, BillTypeEnum.AllocationBill))
                {
                    return Warning("已送货签收转单，相关销售单未红冲，本调拨单不能红冲.");
                }
                #endregion

                #endregion

                //RedLock
                var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill),
                      TimeSpan.FromSeconds(30),
                      TimeSpan.FromSeconds(10),
                      TimeSpan.FromSeconds(1),
                      () => _allocationBillService.Reverse(curUser.Id, bill));
                return Json(result);

            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("Reverse", "单据红冲失败", curUser.Id);
                _notificationService.SuccessNotification("单据红冲失败");
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="shipmentWareHouseId">出货仓库Id</param>
        /// <param name="incomeWareHouseId">入货仓库Id</param>
        /// <param name="billNumber">单据号</param>
        /// <param name="auditedStatus">状态</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="showReverse">显示红冲数据</param>
        /// <param name="sortByAuditedTime">按审核时间</param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.AllocationFormExport)]
        public FileResult Export(int type, string selectData, int? shipmentWareHouseId, int? incomeWareHouseId, string billNumber = "", bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? showReverse = null, bool? sortByAuditedTime = null, string remark = "")
        {

            #region 查询导出数据

            IList<AllocationBill> allocationBills = new List<AllocationBill>();
            if (type == 1)
            {
                if (!string.IsNullOrEmpty(selectData))
                {
                    List<string> ids = selectData.Split(',').ToList();
                    foreach (var id in ids)
                    {
                        AllocationBill allocationBill = _allocationBillService.GetAllocationBillById(curStore.Id, int.Parse(id), true);
                        if (allocationBill != null)
                        {
                            allocationBills.Add(allocationBill);
                        }
                    }
                }
            }
            else if (type == 2)
            {
                allocationBills = _allocationBillService.GetAllAllocationBills(curStore?.Id ?? 0, curUser.Id, null, shipmentWareHouseId, incomeWareHouseId, billNumber, auditedStatus, startTime, endTime, showReverse, null, 0);
            }

            #endregion

            #region 导出
            var ms = _exportManager.ExportAllocationBillToXlsx(allocationBills);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "调拨单.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "调拨单.xlsx");
            }
            #endregion

        }
        [AuthCode((int)AccessGranularityEnum.AllocationFormPrint)]
        public JsonResult PrintSetting()
        {
            var printTemplates = _printTemplateService.GetAllPrintTemplates(curStore.Id).ToList();
            var printTemplate = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.AllocationBill).FirstOrDefault();
            //获取打印设置
            var pCPrintSetting = _settingService.LoadSetting<PCPrintSetting>(_storeContext.CurrentStore.Id);
            var settings = new object();
            if (pCPrintSetting != null)
            {
                settings = new
                {
                    PaperWidth = (printTemplate?.PaperWidth == 0 || printTemplate?.PaperHeight == 0) ? pCPrintSetting.PaperWidth : printTemplate.PaperWidth,
                    PaperHeight = (printTemplate?.PaperWidth == 0 || printTemplate?.PaperHeight == 0) ? pCPrintSetting.PaperHeight : printTemplate.PaperHeight,
                    BorderType = pCPrintSetting.BorderType,
                    MarginTop = pCPrintSetting.MarginTop,
                    MarginBottom = pCPrintSetting.MarginBottom,
                    MarginLeft = pCPrintSetting.MarginLeft,
                    MarginRight = pCPrintSetting.MarginRight,
                    IsPrintPageNumber = pCPrintSetting.IsPrintPageNumber,
                    PrintHeader = pCPrintSetting.PrintHeader,
                    PrintFooter = pCPrintSetting.PrintFooter,
                    FixedRowNumber = pCPrintSetting.FixedRowNumber,
                    PrintSubtotal = pCPrintSetting.PrintSubtotal,
                    PrintPort = pCPrintSetting.PrintPort
                };
                return Successful("", settings);
            }
            return Successful("", null);

        }
        /// <summary>
        /// 打印
        /// </summary>
        /// <param name="selectData"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.AllocationFormPrint)]
        public JsonResult Print(int type, string selectData, int? shipmentWareHouseId, int? incomeWareHouseId, string billNumber = "", bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? showReverse = null, bool? sortByAuditedTime = null, string remark = "")
        {
            try
            {

                bool fg = true;
                string errMsg = string.Empty;

                #region 查询打印数据

                IList<AllocationBill> allocationBills = new List<AllocationBill>();
                var datas = new List<string>();
                //默认选择
                type = type == 0 ? 1 : type;
                if (type == 1)
                {
                    if (!string.IsNullOrEmpty(selectData))
                    {
                        List<string> ids = selectData.Split(',').ToList();
                        foreach (var id in ids)
                        {
                            AllocationBill allocationBill = _allocationBillService.GetAllocationBillById(curStore.Id, int.Parse(id), true);
                            if (allocationBill != null)
                            {
                                allocationBills.Add(allocationBill);
                            }
                        }
                    }
                }
                else if (type == 2)
                {
                    allocationBills = _allocationBillService.GetAllAllocationBills(curStore?.Id ?? 0, null, curUser.Id, shipmentWareHouseId, incomeWareHouseId, billNumber, auditedStatus, startTime, endTime, showReverse, null, 0);
                }

                #endregion

                #region 修改数据
                if (allocationBills != null && allocationBills.Count > 0)
                {
                    //using (var scope = new TransactionScope())
                    //{
                    //    scope.Complete();
                    //}

                    #region 修改单据表打印数
                    foreach (var d in allocationBills)
                    {
                        d.PrintNum = (d.PrintNum ?? 0) + 1;
                        _allocationBillService.UpdateAllocationBill(d);
                    }
                    #endregion
                }

                //获取打印模板
                var printTemplates = _printTemplateService.GetAllPrintTemplates(curStore.Id).ToList();
                var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.AllocationBill).Select(a => a.Content).FirstOrDefault();

                //获取打印设置
                var pCPrintSetting = _settingService.LoadSetting<PCPrintSetting>(_storeContext.CurrentStore.Id);

                //填充打印数据
                foreach (var d in allocationBills)
                {

                    StringBuilder sb = new StringBuilder();
                    sb.Append(content);

                    #region theadid
                    //sb.Replace("@商铺名称", curStore.Name);
                    if (pCPrintSetting != null)
                    {
                        //sb.Replace("@商铺名称", pCPrintSetting.StoreName);
                        sb.Replace("@商铺名称", string.IsNullOrWhiteSpace(pCPrintSetting.StoreName) ? "&nbsp;" : pCPrintSetting.StoreName);
                    }

                    WareHouse shipmentWareHouse = _wareHouseService.GetWareHouseById(curStore.Id, d.ShipmentWareHouseId);
                    if (shipmentWareHouse != null)
                    {
                        sb.Replace("@出货仓库", shipmentWareHouse.Name);
                    }
                    WareHouse incomeWareHouse = _wareHouseService.GetWareHouseById(curStore.Id, d.IncomeWareHouseId);
                    if (incomeWareHouse != null)
                    {
                        sb.Replace("@入货仓库", incomeWareHouse.Name);
                    }
                    sb.Replace("@调拨日期", d.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss"));
                    sb.Replace("@打印日期", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                    User businessUser = _userService.GetUserById(curStore.Id, d.MakeUserId);
                    if (businessUser != null)
                    {
                        sb.Replace("@业务员", businessUser.UserRealName);
                        sb.Replace("@业务电话", businessUser.MobileNumber);
                    }
                    sb.Replace("@单据编号", d.BillNumber);

                    #endregion

                    #region tbodyid
                    //明细
                    //获取 tbody 中的行
                    int beginTbody = sb.ToString().IndexOf(@"<tbody id=""tbody"">") + @"<tbody id=""tbody"">".Length;
                    if (beginTbody == 17)
                    {
                        beginTbody = sb.ToString().IndexOf(@"<tbody id='tbody'>") + @"<tbody id='tbody'>".Length;
                    }
                    int endTbody = sb.ToString().IndexOf("</tbody>", beginTbody);
                    string tbodytr = sb.ToString()[beginTbody..endTbody];
                    var sumItem1 = 0;
                    var sumItem2 = 0;
                    var sumItem3 = 0;
                    if (d.Items != null && d.Items.Count > 0)
                    {
                        //1.先删除明细第一行
                        sb.Remove(beginTbody, endTbody - beginTbody);
                        int i = 0;
                        var allProducts = _productService.GetProductsByIds(curStore.Id, d.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                        var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());
                        foreach (var item in d.Items)
                        {
                            int index = sb.ToString().IndexOf("</tbody>", beginTbody);
                            i++;
                            StringBuilder sb2 = new StringBuilder();
                            sb2.Append(tbodytr);

                            sb2.Replace("#序号", i.ToString());
                            var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                            if (product != null)
                            {
                                sb2.Replace("#商品名称", product.Name);
                                if (product.IsManufactureDete)
                                {
                                    sb2.Replace("#生产日期", item.ManufactureDete?.ToString("yyyy/MM/dd HH:mm:ss"));
                                }
                                else
                                {
                                    sb2.Replace("#生产日期", "");
                                }
                                ProductUnitOption productUnitOption = product.GetProductUnit(_specificationAttributeService, _productService);
                                if (item.UnitId == product.SmallUnitId)
                                {
                                    sb2.Replace("#条形码", product.SmallBarCode);
                                    if (productUnitOption != null && productUnitOption.smallOption != null)
                                    {
                                        sb2.Replace("#商品单位", productUnitOption.smallOption.Name);
                                    }

                                }
                                else if (item.UnitId == product.StrokeUnitId)
                                {
                                    sb2.Replace("#条形码", product.StrokeBarCode);
                                    if (productUnitOption != null && productUnitOption.strokOption != null)
                                    {
                                        sb2.Replace("#商品单位", productUnitOption.strokOption.Name);
                                    }
                                }
                                else if (item.UnitId == product.BigUnitId)
                                {
                                    sb2.Replace("#条形码", product.BigBarCode);
                                    if (productUnitOption != null && productUnitOption.bigOption != null)
                                    {
                                        sb2.Replace("#商品单位", productUnitOption.bigOption.Name);
                                    }
                                }

                                sb2.Replace("#单位换算", product.GetProductUnitConversion(allOptions));
                                sb2.Replace("#保质期", (product.ExpirationDays ?? 0).ToString());
                                int conversionQuantity = product.GetConversionQuantity(item.UnitId, _specificationAttributeService, _productService);
                                var qty = Pexts.StockQuantityFormat(conversionQuantity * item.Quantity, product.StrokeQuantity ?? 0, product.BigQuantity ?? 0);
                                sb2.Replace("#辅助数量", qty.Item1 + "大" + qty.Item2 + "中" + qty.Item3 + "小");
                                sumItem1 += qty.Item1;
                                sumItem2 += qty.Item2;
                                sumItem3 += qty.Item3;
                            }
                            sb2.Replace("#数量", item.Quantity.ToString());
                            sb2.Replace("#备注", item.Remark);

                            sb.Insert(index, sb2);

                        }
                        sb.Replace("辅助数量:###", sumItem1 + "大" + sumItem2 + "中" + sumItem3 + "小");
                        sb.Replace("数量:###", d.Items.Sum(s => s.Quantity).ToString());
                    }
                    #endregion

                    #region tfootid
                    User makeUser = _userService.GetUserById(curStore.Id, d.MakeUserId);
                    if (makeUser != null)
                    {
                        sb.Replace("@制单", makeUser.UserRealName);
                    }
                    sb.Replace("@日期", d.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss"));
                    //sb.Replace("@公司地址", "");
                    if (pCPrintSetting != null)
                    {
                        sb.Replace("@公司地址", pCPrintSetting.Address);
                    }

                    //sb.Replace("@订货电话", "");
                    if (pCPrintSetting != null)
                    {
                        sb.Replace("@订货电话", pCPrintSetting.PlaceOrderTelphone);
                    }

                    sb.Replace("@备注", d.Remark);
                    #endregion

                    datas.Add(sb.ToString());
                }

                if (fg)
                {
                    return Successful("打印成功", datas);
                }
                else
                {
                    return Warning(errMsg);

                }
                #endregion

            }
            catch (Exception ex)
            {
                return Warning(ex.ToString());
            }
        }



        /// <summary>
        /// 快速调拨加载数据
        /// </summary>
        /// <param name="allocationType">调拨类型</param>
        /// <param name="wareHouseId">出货仓库</param>
        /// <param name="deliveryUserId">送货员</param>
        /// <param name="categoryIds">商品类别（为空默认所有类别商品）</param>
        /// <param name="loadDataNameIds">加载范围</param>
        /// <returns></returns>
        [HttpPost]
        //[ValidateInput(false)]
        public JsonResult QuickAllocation(int allocationType = 0, int wareHouseId = 0, int deliveryUserId = 0, string categoryIds = "", string loadDataNameIds = "")
        {

            //当前节点所有子孙节点
            string categoryIdsNew = string.Empty;
            var categorys = new List<Category>();
            var allCategories = _categoryService.GetAllCategories(curStore.Id);
            if (!string.IsNullOrEmpty(categoryIds) && allCategories != null && allCategories.Count > 0)
            {
                List<string> lids = categoryIds.Split(',').ToList();
                lids.ForEach(cid =>
                {
                    if (int.TryParse(cid, out int categoryId))
                    {
                        categorys.AddRange(allCategories.SortCategoriesForTree(categoryId));
                    }
                });
            }

            if (categorys != null && categorys.Count > 0)
            {
                categoryIdsNew = string.Join(",", categorys.Select(c => c.Id).ToList());
            }

            var items = _allocationBillService.GetQuickAllocation(curStore.Id, allocationType, wareHouseId, deliveryUserId, categoryIdsNew, loadDataNameIds);

            var gridModel = new List<Product>();
            if (items != null && items.Count > 0)
            {
                var allProducts = _productService.GetProductsByIds(curStore.Id, items.Select(pr => pr.ProductId).Distinct().ToArray());

                items.ToList().ForEach(i =>
                {
                    var product = allProducts.Where(ap => ap.Id == i.ProductId).FirstOrDefault();
                    if (product != null)
                    {
                        gridModel.Add(product);
                    }
                });
            }

            var allCatagories = _categoryService.GetCategoriesByCategoryIds(curStore?.Id ?? 0, gridModel.Select(p => p.CategoryId).Distinct().ToArray());
            var allBrands = _brandService.GetBrandsByBrandIds(curStore?.Id ?? 0, gridModel.Select(p => p.BrandId).Distinct().ToArray());
            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, gridModel.GetProductBigStrokeSmallUnitIds());
            var allProductPrices = _productService.GetProductPricesByProductIds(curStore.Id, gridModel.Select(gm => gm.Id).Distinct().ToArray());
            var allProductTierPrices = _productService.GetProductTierPriceByProductIds(curStore.Id, gridModel.Select(gm => gm.Id).Distinct().ToArray());

            var model = gridModel.Select(m =>
             {
                 var cat = allCatagories.Where(ca => ca.Id == m.CategoryId).FirstOrDefault();
                 var brand = allBrands.Where(br => br.Id == m.BrandId).FirstOrDefault();
                 var p = m.ToModel<ProductModel>();

                 p.CategoryName = cat != null ? cat.Name : "";
                 p.BrandName = brand != null ? brand.Name : "";

                 //这里替换成高级用法
                 p = m.InitBaseModel<ProductModel>(p, 0, allOptions, allProductPrices, allProductTierPrices, _productService);

                 //默认调拨数量
                 p.Quantity = items.Where(t => t.ProductId == m.Id).Select(t => t.Quantity).FirstOrDefault();

                 return p;
             }).ToList();

            return Json(new
            {
                Success = true,
                total = model.Count,
                rows = model
            });
        }
        /// <summary>
        /// 作废
        /// </summary>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.AllocationFormDelete)]
        public JsonResult Delete(int? id)
        {
            try
            {
                var bill = new AllocationBill() { StoreId = curStore?.Id ?? 0 };

                #region 验证
                if (!id.HasValue)
                {
                    return Warning("参数错误.");
                }
                else
                {
                    bill = _allocationBillService.GetAllocationBillById(curStore.Id, id.Value, true);
                    if (bill.AuditedStatus || bill.ReversedStatus)
                    {
                        return Warning("单据已审核或红冲，不能作废.");
                    }
                    if (bill.Deleted)
                    {
                        return Warning("单据已作废，请刷新页面.");
                    }
                    if (bill != null)
                    {
                        var rs = _allocationBillService.Delete(curUser.Id, bill);
                        if (!rs.Success)
                        {
                            return Error(rs.Message);
                        }
                    }
                }
                #endregion
                return Successful("作废成功");

            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("Delete", "单据作废失败", curUser.Id);
                _notificationService.SuccessNotification("单据作废失败");
                return Error(ex.Message);
            }
        }
    }
}