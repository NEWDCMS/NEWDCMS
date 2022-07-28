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
using DCMS.Services.Purchases;
using DCMS.Services.Users;
using DCMS.Services.WareHouses;
using DCMS.ViewModel.Models.WareHouses;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using DCMS.Web.Models;
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
    /// 用于盘点盈亏管理
    /// </summary>
    public class InventoryProfitLossController : BasePublicController
    {
        private readonly IPrintTemplateService _printTemplateService;
        private readonly IUserService _userService;
        private readonly IMediaService _mediaService;
        private readonly IUserActivityService _userActivityService;
        private readonly ISettingService _settingService;
        private readonly IWareHouseService _wareHouseService;
        private readonly IInventoryProfitLossBillService _inventoryProfitLossBillService;
        private readonly IProductService _productService;
        private readonly IStockService _stockService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IRedLocker _locker;
        private readonly IExportManager _exportManager;
        private readonly IPurchaseBillService _purchaseBillService;

        public InventoryProfitLossController(
            IWorkContext workContext,
            IStoreContext storeContext,
            IPrintTemplateService printTemplateService,
            IMediaService mediaService,
            IUserActivityService userActivityService,
            ISettingService settingService,
            IWareHouseService wareHouseService,
            IInventoryProfitLossBillService inventoryProfitLossBillService,
            IProductService productService,
            IStockService stockService,
            ISpecificationAttributeService specificationAttributeService,
            IUserService userService,
            ILogger loggerService,
            INotificationService notificationService,
            IRedLocker locker,
            IExportManager exportManager,
            IPurchaseBillService purchaseBillService
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            _printTemplateService = printTemplateService;
            _mediaService = mediaService;
            _userService = userService;
            _userActivityService = userActivityService;
            _settingService = settingService;
            _wareHouseService = wareHouseService;
            _inventoryProfitLossBillService = inventoryProfitLossBillService;
            _productService = productService;
            _stockService = stockService;
            _specificationAttributeService = specificationAttributeService;
            _locker = locker;
            _exportManager = exportManager;
            _purchaseBillService = purchaseBillService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        /// <summary>
        /// 列表
        /// </summary>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.InventoryProfitlossView)]
        public IActionResult List(int? wareHouseId, int? chargePerson, string billNumber = "", bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? showReverse = null, bool? sortByAuditedTime = null, string remark = "", int pagenumber = 0)
        {
            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var model = new InventoryProfitLossBillListModel
            {

                //仓库
                WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore,null,0),
                WareHouseId = null,

                BillNumber = billNumber,
                AuditedStatus = auditedStatus,
                StartTime = (startTime == null) ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")) : startTime,
                EndTime = (endTime == null) ? DateTime.Now.AddDays(1) : endTime,
                ShowReverse = showReverse,
                SortByAuditedTime = sortByAuditedTime,

                //操作员
                ChargePersons = BindUserSelection(_userService.BindUserList, curStore, ""),
                ChargePerson = null
            };

            //获取分页
            var bills = _inventoryProfitLossBillService.GetAllInventoryProfitLossBills(
                curStore?.Id ?? 0,
                 curUser.Id,
                chargePerson,
                wareHouseId,
                billNumber,
                auditedStatus,
                model.StartTime,
                model.EndTime,
                showReverse,
                sortByAuditedTime,
                false,  //过滤删除
                remark,
                pagenumber,
                30);
            model.PagingFilteringContext.LoadPagedList(bills);

            #region 查询需要关联其他表的数据
            var allUsers = _userService.GetUsersDictsByIds(curStore.Id, bills.Select(b => b.ChargePerson).Distinct().ToArray());
            var allWarehouses = _wareHouseService.GetWareHouseByIds(curStore.Id, bills.Select(b => b.WareHouseId).Distinct().ToArray());
            #endregion

            model.Items = bills.OrderByDescending(b => b.CreatedOnUtc).Select(b =>
            {
                var m = b.ToModel<InventoryProfitLossBillModel>();

                //操作员
                m.ChargePersonName = allUsers.Where(au => au.Key == b.ChargePerson).Select(au => au.Value).FirstOrDefault();
                //仓库
                var warehouse = allWarehouses.Where(aw => aw.Id == b.WareHouseId).FirstOrDefault();
                m.WareHouseName = warehouse == null ? "" : warehouse.Name;

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
        [AuthCode((int)AccessGranularityEnum.InventoryProfitlossSave)]
        public IActionResult Create(int? store)
        {

            //获取公司配置
            var companySetting = _settingService.LoadSetting<CompanySetting>(curStore.Id);

            var model = new InventoryProfitLossBillModel
            {
                CreatedOnUtc = DateTime.Now,
                InventoryDate = DateTime.Now,

                WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore, null, 0),
                WareHouseId = -1,

                //操作员
                ChargePersons = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Salesmans, curUser.Id, true, _userService.IsAdmin(curStore.Id, curUser.Id)),
                ChargePerson = -1
            };

            model.IsShowCreateDate = companySetting.OpenBillMakeDate == 2 ? true : false;

            return View(model);
        }

        /// <summary>
        /// 编辑
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.InventoryProfitlossView)]
        public IActionResult Edit(int id = 0)
        {

            //获取公司配置
            var companySetting = _settingService.LoadSetting<CompanySetting>(curStore.Id);

            var model = new InventoryProfitLossBillModel();
            var inventoryProfitLossBill = _inventoryProfitLossBillService.GetInventoryProfitLossBillById(curStore.Id, id, true);
            if (inventoryProfitLossBill == null)
            {
                return RedirectToAction("List");
            }

            if (inventoryProfitLossBill != null)
            {
                if (inventoryProfitLossBill.StoreId != curStore.Id)
                {
                    return RedirectToAction("List");
                }

                model = inventoryProfitLossBill.ToModel<InventoryProfitLossBillModel>();
                model.BillBarCode = _mediaService.GenerateBarCodeForBase64(inventoryProfitLossBill.BillNumber, 150, 50);
                model.Items = inventoryProfitLossBill.Items.Select(a => a.ToModel<InventoryProfitLossItemModel>()).ToList();
            }

            model.WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore, null, 0);

            //操作员
            model.ChargePersons = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Salesmans, curUser.Id, true, _userService.IsAdmin(curStore.Id, curUser.Id));

            var mu = string.Empty;
            if (inventoryProfitLossBill.MakeUserId > 0)
            {
                mu = _userService.GetUserName(curStore.Id, inventoryProfitLossBill.MakeUserId);
            }
            model.MakeUserName = mu + " " + inventoryProfitLossBill.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss");

            var au = string.Empty;
            if (inventoryProfitLossBill.AuditedUserId != null && inventoryProfitLossBill.AuditedUserId > 0)
            {
                au = _userService.GetUserName(curStore.Id, inventoryProfitLossBill.AuditedUserId ?? 0);
            }
            model.AuditedUserName = au + " " + (inventoryProfitLossBill.AuditedDate.HasValue ? inventoryProfitLossBill.AuditedDate.Value.ToString("yyyy/MM/dd HH:mm:ss") : "");

            model.IsShowCreateDate = companySetting.OpenBillMakeDate == 2 ? true : false;

            return View(model);
        }

        #region 单据项目

        /// <summary>
        /// 异步获取项目
        /// </summary>
        /// <param name="billId"></param>
        /// <returns></returns>
        public JsonResult AsyncInventoryProfitLossItems(int billId)
        {

            var gridModel = _inventoryProfitLossBillService.GetInventoryProfitLossItemList(billId);

            var allProducts = _productService.GetProductsByIds(curStore.Id, gridModel.Select(pr => pr.ProductId).Distinct().ToArray());
            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());
            var allProductPrices = _productService.GetProductPricesByProductIds(curStore.Id, gridModel.Select(pr => pr.ProductId).Distinct().ToArray());
            var allProductTierPrices = _productService.GetProductTierPriceByProductIds(curStore.Id, gridModel.Select(pr => pr.ProductId).Distinct().ToArray());

            var items = gridModel.Select(o =>
            {
                var m = o.ToModel<InventoryProfitLossItemModel>();

                var product = allProducts.Where(ap => ap.Id == m.ProductId).FirstOrDefault();
                if (product != null)
                {
                    //这里替换成高级用法
                    m = product.InitBaseModel<InventoryProfitLossItemModel>(m, 0, allOptions, allProductPrices, allProductTierPrices, _productService);

                    //商品信息
                    m.BigUnitId = product.BigUnitId;
                    m.StrokeUnitId = product.StrokeUnitId;
                    m.SmallUnitId = product.SmallUnitId;
                    m.IsManufactureDete = product.IsManufactureDete;//商品是否开启生产日期功能
                }

                return m;

            }).ToList();


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
        [AuthCode((int)AccessGranularityEnum.InventoryProfitlossSave)]
        public async Task<JsonResult> CreateOrUpdate(InventoryProfitLossUpdateModel data, int? billId)
        {

            try
            {

                var bill = new InventoryProfitLossBill();

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
                    bill = _inventoryProfitLossBillService.GetInventoryProfitLossBillById(curStore.Id, billId.Value, true);

                    //公共单据验证
                    var commonBillChecking = BillChecking<InventoryProfitLossBill, InventoryProfitLossItem>(bill, BillStates.Draft);
                    if (commonBillChecking.Value != null)
                    {
                        return commonBillChecking;
                    }
                }
                #endregion

                #region 验证盘点
                if (data != null && data.Items != null && data.Items.Count > 0)
                {
                    if (_wareHouseService.CheckProductInventory(curStore.Id, data.WareHouseId, bill.Items?.Select(it => it.ProductId).Distinct().ToArray(), out string thisMsg))
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
                    foreach (InventoryProfitLossItemModel item in data.Items)
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

                //当前预占(数量小于0才未预占)
                productStockItemNews = productStockItemNews.Where(pn => pn.Quantity < 0).ToList();
                //将负数变成正数
                productStockItemNews.ForEach(pn =>
                {
                    pn.Quantity = (-1) * pn.Quantity;
                });

                //验证库存
                string errMsg = string.Empty;
                var fg = _stockService.CheckStockQty(_productService, _specificationAttributeService, curStore.Id, data.WareHouseId, productStockItemNews, out errMsg);
                if (fg == false)
                {
                    return Warning(errMsg);
                }

                #endregion

                //业务逻辑
                var dataTo = data.ToEntity<InventoryProfitLossBillUpdate>();
                dataTo.Operation = (int)OperationEnum.PC;
                dataTo.Items = data.Items.Select(it =>
                {
                    //成本价（此处计算成本价防止web、api成本价未带出,web、api的controller都要单独计算（取消service计算，防止其他service都引用 pruchasebillservice））
                    var item = it.ToEntity<InventoryProfitLossItem>();
                    item.CostPrice = _purchaseBillService.GetReferenceCostPrice(curStore.Id, item.ProductId, item.UnitId);
                    item.CostAmount = item.CostPrice * item.Quantity;
                    return item;
                }).ToList();

                //RedLock
                var result = await _locker.PerformActionWithLockAsync(LockKey(data),
                   TimeSpan.FromSeconds(30),
                   TimeSpan.FromSeconds(10),
                   TimeSpan.FromSeconds(1),
                   () => _inventoryProfitLossBillService.BillCreateOrUpdate(curStore.Id, curUser.Id, billId, bill, dataTo, dataTo.Items, productStockItemNews, _userService.IsAdmin(curStore.Id, curUser.Id)));
                return Json(result);
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("CreateOrUpdate", Resources.Bill_CreateOrUpdateFailed, curUser.Id);

                _notificationService.ErrorNotification(Resources.Bill_CreateOrUpdateFailed);
                return Error(ex.Message);
            }

        }

        #endregion

        /// <summary>
        /// 审核
        /// </summary>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.InventoryProfitlossApproved)]
        public async Task<JsonResult> Auditing(int? id)
        {
            try
            {
                var bill = new InventoryProfitLossBill();

                #region 验证

                if (!id.HasValue)
                {
                    return Warning("参数错误.");
                }
                else
                {
                    bill = _inventoryProfitLossBillService.GetInventoryProfitLossBillById(curStore.Id, id.Value, true);
                    if (bill.AuditedStatus)
                    {
                        return Warning("单据已审核，请刷新页面.");
                    }
                }

                //公共单据验证
                var commonBillChecking = BillChecking<InventoryProfitLossBill, InventoryProfitLossItem>(bill, BillStates.Audited);
                if (commonBillChecking.Value != null)
                {
                    return commonBillChecking;
                }

                if (_wareHouseService.CheckProductInventory(curStore.Id, bill.WareHouseId, bill.Items?.Select(it => it.ProductId).Distinct().ToArray(), out string thisMsg))
                {
                    return Warning("仓库正在盘点中，拒绝操作.");
                }


                #region 验证库存
                IList<Product> allProducts = new List<Product>();

                //当前数据
                List<ProductStockItem> productStockItemNews = new List<ProductStockItem>();
                if (bill.Items != null && bill.Items.Count > 0)
                {
                    allProducts = _productService.GetProductsByIds(curStore.Id, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());
                    foreach (InventoryProfitLossItem item in bill.Items)
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

                //数量小于0需要验证
                productStockItemNews = productStockItemNews.Where(pn => pn.Quantity < 0).ToList();
                //将负数变成正数
                productStockItemNews.ForEach(pn =>
                {
                    pn.Quantity = (-1) * pn.Quantity;
                });

                //验证库存
                string errMsg = string.Empty;
                var fg = _stockService.CheckStockQty(_productService, _specificationAttributeService, curStore.Id, bill.WareHouseId, productStockItemNews, out errMsg);
                if (fg == false)
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
                      () => _inventoryProfitLossBillService.Auditing(curStore.Id, curUser.Id, bill));
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
        [AuthCode((int)AccessGranularityEnum.InventoryProfitlossReverse)]
        public async Task<JsonResult> Reverse(int? id)
        {
            try
            {

                var bill = new InventoryProfitLossBill() { StoreId = curStore?.Id ?? 0 };

                #region 验证

                if (PeriodClosed(DateTime.Now))
                {
                    return Warning("系统当月已经结转，不允许红冲.");
                }

                if (!id.HasValue)
                {
                    return Warning("参数错误.");
                }
                else
                {
                    bill = _inventoryProfitLossBillService.GetInventoryProfitLossBillById(curStore.Id, id.Value, true);
                    if (bill.ReversedStatus)
                    {
                        return Warning("单据已红冲，请刷新页面.");
                    }
                }

                //公共单据验证
                var commonBillChecking = BillChecking<InventoryProfitLossBill, InventoryProfitLossItem>(bill, BillStates.Reversed);
                if (commonBillChecking.Value != null)
                {
                    return commonBillChecking;
                }

                if (_wareHouseService.CheckProductInventory(curStore.Id, bill.WareHouseId, bill.Items?.Select(it => it.ProductId).Distinct().ToArray(), out string thisMsg))
                {
                    return Warning("仓库正在盘点中，拒绝操作.");
                }

                #region 验证库存
                //将一个单据中 相同商品 数量 按最小单位汇总
                List<ProductStockItem> productStockItems = new List<ProductStockItem>();

                var allProducts = _productService.GetProductsByIds(curStore.Id, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());
                foreach (InventoryProfitLossItem item in bill.Items)
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
                //数量大于0需要验证
                productStockItems = productStockItems.Where(pn => pn.Quantity > 0).ToList();

                //验证当前商品库存

                if (!_stockService.CheckStockQty(_productService, _specificationAttributeService, bill.StoreId, bill.WareHouseId, productStockItems, out string thisMsg2))
                {
                    return Warning(thisMsg2);
                }
                #endregion
                #endregion

                //RedLock
                var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill),
                      TimeSpan.FromSeconds(30),
                      TimeSpan.FromSeconds(10),
                      TimeSpan.FromSeconds(1),
                      () => _inventoryProfitLossBillService.Reverse(curUser.Id, bill));
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
        [AuthCode((int)AccessGranularityEnum.InventoryProfitlossPrint)]
        public JsonResult PrintSetting()
        {
            var printTemplates = _printTemplateService.GetAllPrintTemplates(curStore.Id).ToList();
            var printTemplate = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.InventoryProfitLossBill).FirstOrDefault();
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
        [AuthCode((int)AccessGranularityEnum.InventoryProfitlossPrint)]
        public JsonResult Print(int type, string selectData, int? wareHouseId, int? chargePerson, string billNumber = "", bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? showReverse = null, bool? sortByAuditedTime = null)
        {
            try
            {

                bool fg = true;
                string errMsg = string.Empty;

                #region 查询打印数据

                IList<InventoryProfitLossBill> inventoryProfitLossBills = new List<InventoryProfitLossBill>();
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
                            InventoryProfitLossBill inventoryProfitLossBill = _inventoryProfitLossBillService.GetInventoryProfitLossBillById(curStore.Id, int.Parse(id), true);
                            if (inventoryProfitLossBill != null)
                            {
                                inventoryProfitLossBills.Add(inventoryProfitLossBill);
                            }
                        }
                    }
                }
                else if (type == 2)
                {
                    inventoryProfitLossBills = _inventoryProfitLossBillService.GetAllInventoryProfitLossBills(
                               curStore?.Id ?? 0,
                                curUser.Id,
                               chargePerson,
                               wareHouseId,
                               billNumber,
                               auditedStatus,
                               startTime,
                               endTime,
                               showReverse,
                               sortByAuditedTime);
                }


                #endregion

                #region 修改数据
                if (inventoryProfitLossBills != null && inventoryProfitLossBills.Count > 0)
                {
                    //using (var scope = new TransactionScope())
                    //{
                    //    scope.Complete();
                    //}

                    #region 修改单据表打印数
                    foreach (var d in inventoryProfitLossBills)
                    {
                        d.PrintNum = (d.PrintNum ?? 0) + 1;
                        _inventoryProfitLossBillService.UpdateInventoryProfitLossBill(d);
                    }
                    #endregion
                }

                //获取打印模板
                var printTemplates = _printTemplateService.GetAllPrintTemplates(curStore.Id).ToList();
                var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.InventoryProfitLossBill).Select(a => a.Content).FirstOrDefault();

                //获取打印设置
                var pCPrintSetting = _settingService.LoadSetting<PCPrintSetting>(_storeContext.CurrentStore.Id);

                //填充打印数据
                foreach (var d in inventoryProfitLossBills)
                {

                    StringBuilder sb = new StringBuilder();
                    sb.Append(content);

                    #region theadid
                    //sb.Replace("@商铺名称", curStore.Name);
                    if (pCPrintSetting != null)
                    {
                        sb.Replace("@商铺名称", string.IsNullOrWhiteSpace(pCPrintSetting.StoreName) ? "&nbsp;" : pCPrintSetting.StoreName);
                    }

                    WareHouse wareHouse = _wareHouseService.GetWareHouseById(curStore.Id, d.WareHouseId);
                    if (wareHouse != null)
                    {
                        sb.Replace("@仓库", wareHouse.Name);
                        sb.Replace("@库管员", "***");
                    }
                    User makeUser = _userService.GetUserById(curStore.Id, d.MakeUserId);
                    if (makeUser != null)
                    {
                        sb.Replace("@经办人", makeUser.UserRealName);
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
                            sb2.Replace("#成本价", item.CostPrice == null ? "0.00" : item.CostPrice?.ToString("0.00"));
                            sb2.Replace("#成本金额", item.CostAmount == null ? "0.00" : item.CostAmount?.ToString("0.00"));
                            sb2.Replace("#备注", "");
                            sb.Insert(index, sb2);

                        }
                        sb.Replace("辅助数量:###", sumItem1 + "大" + sumItem2 + "中" + sumItem3 + "小");
                        sb.Replace("数量:###", d.Items.Sum(s => s.Quantity).ToString());
                        sb.Replace("金额:###", d.Items.Sum(s => s.CostAmount ?? 0).ToString("0.00"));
                    }
                    #endregion

                    #region tfootid
                    if (makeUser != null)
                    {
                        sb.Replace("@制单", makeUser.UserRealName);
                    }
                    sb.Replace("@日期", d.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss"));
                    sb.Replace("@打印日期", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
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
        /// 导出
        /// </summary>
        /// <param name="wareHouseId"></param>
        /// <param name="chargePerson"></param>
        /// <param name="billNumber"></param>
        /// <param name="auditedStatus"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="showReverse"></param>
        /// <param name="sortByAuditedTime"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.InventoryProfitlossExport)]
        public FileResult Export(int type, string selectData, int? wareHouseId, int? chargePerson, string billNumber = "", bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? showReverse = null, bool? sortByAuditedTime = null)
        {

            #region 查询导出数据

            IList<InventoryProfitLossBill> inventoryProfitLossBills = new List<InventoryProfitLossBill>();
            if (type == 1)
            {
                if (!string.IsNullOrEmpty(selectData))
                {
                    List<string> ids = selectData.Split(',').ToList();
                    foreach (var id in ids)
                    {
                        InventoryProfitLossBill inventoryProfitLossBill = _inventoryProfitLossBillService.GetInventoryProfitLossBillById(curStore.Id, int.Parse(id), true);
                        if (inventoryProfitLossBill != null)
                        {
                            inventoryProfitLossBills.Add(inventoryProfitLossBill);
                        }
                    }
                }
            }
            else if (type == 2)
            {
                inventoryProfitLossBills = _inventoryProfitLossBillService.GetAllInventoryProfitLossBills(
                       curStore?.Id ?? 0,
                        curUser.Id,
                       chargePerson,
                       wareHouseId,
                       billNumber,
                       auditedStatus,
                       startTime,
                       endTime,
                       showReverse,
                       sortByAuditedTime);
            }
            #endregion

            #region 导出
            var ms = _exportManager.ExportInventoryProfitLossBillToXlsx(inventoryProfitLossBills);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "盘点盈亏单.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "盘点盈亏单.xlsx");
            }
            #endregion
        }

        /// <summary>
        /// 作废  2021-09-03 mu 添加
        /// </summary>
        /// <param name="id">单据ID</param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.InventoryProfitlossDelete)]
        public JsonResult Delete(int? id)
        {
            try
            {
                var bill = new InventoryProfitLossBill();

                #region 验证
                if (!id.HasValue)
                {
                    return Warning("参数错误.");
                }
                else
                {
                    bill = _inventoryProfitLossBillService.GetInventoryProfitLossBillById(curStore.Id, id.Value, true);
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
                        var rs = _inventoryProfitLossBillService.Delete(curUser.Id, bill);
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