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
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace DCMS.Web.Controllers
{
    /// <summary>
    /// 用于库存商品拆分管理
    /// </summary>
    public class SplitProductController : BasePublicController
    {
        private readonly IPrintTemplateService _printTemplateService;
        private readonly IUserService _userService;
        private readonly IMediaService _mediaService;
        private readonly IUserActivityService _userActivityService;
        private readonly ISettingService _settingService;
        private readonly IWareHouseService _wareHouseService;
        private readonly ISplitProductBillService _splitProductBillService;
        private readonly IProductService _productService;
        private readonly IStockService _stockService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IRedLocker _locker;
        private readonly IExportManager _exportManager;
        private readonly IPurchaseBillService _purchaseBillService;

        public SplitProductController(
            IWorkContext workContext,
            IStoreContext storeContext,
            IPrintTemplateService printTemplateService,
            IMediaService mediaService,
            IUserActivityService userActivityService,
            ISettingService settingService,
            IWareHouseService wareHouseService,
            IProductService productService,
            IStockService stockService,
            ISplitProductBillService splitProductBillService,
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
            _splitProductBillService = splitProductBillService;
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
        [AuthCode((int)AccessGranularityEnum.SplitOrderView)]
        public IActionResult List(int? wareHouseId, string billNumber = "", bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? showReverse = null, bool? sortByAuditedTime = null, string remark = "", int pagenumber = 0)
        {

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var model = new SplitProductBillListModel
            {
                WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore,null,0),
                WareHouseId = null,

                BillNumber = billNumber,
                AuditedStatus = auditedStatus,
                StartTime = (startTime == null) ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")) : startTime,
                EndTime = (endTime == null) ? DateTime.Now.AddDays(1) : endTime,
                ShowReverse = showReverse,
                SortByAuditedTime = sortByAuditedTime,
                Remark = remark
            };

            //获取分页
            var bills = _splitProductBillService.GetAllSplitProductBills(
                curStore?.Id ?? 0,
                curUser.Id,
                wareHouseId,
                billNumber,
                auditedStatus,
                model.StartTime,
                model.EndTime,
                showReverse,
                sortByAuditedTime,
                remark,
                pagenumber,
                30);
            model.PagingFilteringContext.LoadPagedList(bills);

            #region 查询需要关联其他表的数据
            var allUsers = _userService.GetUsersDictsByIds(curStore.Id, bills.Select(b => b.MakeUserId).Distinct().ToArray());
            var allProducts = _productService.GetProductsByIds(curStore.Id, bills.Select(b => b.ProductId).Distinct().ToArray());
            #endregion

            model.Items = bills.Select(b =>
            {
                var m = b.ToModel<SplitProductBillModel>();

                //业务员
                m.SalesmanName = allUsers.Where(au => au.Key == b.MakeUserId).Select(au => au.Value).FirstOrDefault();

                //商品名
                var product = allProducts.Where(ap => ap.Id == b.ProductId).FirstOrDefault();
                m.ProductName = product == null ? "" : product.Name;

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
        [AuthCode((int)AccessGranularityEnum.SplitOrderSave)]
        public IActionResult Create(int? store)
        {
            var model = new SplitProductBillModel
            {
                CreatedOnUtc = DateTime.Now,
                WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore, null, 0),
                ProductName = "",
                Quantity = null,
                ProductCost = null,
                ProductCostAmount = null
            };

            return View(model);
        }

        /// <summary>
        /// 编辑
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.SplitOrderView)]
        public IActionResult Edit(int id = 0)
        {

            var model = new SplitProductBillModel();
            var splitProductBill = _splitProductBillService.GetSplitProductBillById(curStore.Id, id, true);
            if (splitProductBill == null)
            {
                return RedirectToAction("List");
            }

            if (splitProductBill != null)
            {
                if (splitProductBill.StoreId != curStore.Id)
                {
                    return RedirectToAction("List");
                }

                model = splitProductBill.ToModel<SplitProductBillModel>();
                model.BillBarCode = _mediaService.GenerateBarCodeForBase64(splitProductBill.BillNumber, 150, 50);
                model.Items = splitProductBill.Items.Select(a => a.ToModel<SplitProductItemModel>()).ToList();
            }

            model.WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore,null,0);

            model.ProductName = _productService.GetProductName(curStore.Id, splitProductBill.ProductId);

            //制单人
            //var mu = _userService.GetUserById(curStore.Id, splitProductBill.MakeUserId);
            //model.MakeUserName = mu != null ? (mu.UserRealName + " " + splitProductBill.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss")) : "";
            var mu = string.Empty;
            if (splitProductBill.MakeUserId > 0)
            {
                mu = _userService.GetUserName(curStore.Id, splitProductBill.MakeUserId);
            }
            model.MakeUserName = mu + " " + splitProductBill.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss");

            //审核人
            //var au = _userService.GetUserById(curStore.Id, splitProductBill.AuditedUserId ?? 0);
            //model.AuditedUserName = au != null ? (au.UserRealName + " " + (splitProductBill.AuditedDate.HasValue ? splitProductBill.AuditedDate.Value.ToString("yyyy/MM/dd HH:mm:ss") : "")) : "";
            var au = string.Empty;
            if (splitProductBill.AuditedUserId != null && splitProductBill.AuditedUserId > 0)
            {
                au = _userService.GetUserName(curStore.Id, splitProductBill.AuditedUserId ?? 0);
            }
            model.AuditedUserName = au + " " + (splitProductBill.AuditedDate.HasValue ? splitProductBill.AuditedDate.Value.ToString("yyyy/MM/dd HH:mm:ss") : "");

            return View(model);
        }

        #region 单据项目
        /// <summary>
        /// 异步获取项目
        /// </summary>
        /// <param name="billId"></param>
        /// <returns></returns>
        public async Task<JsonResult> AsyncSplitProductItems(int billId)
        {
            return await Task.Run(() =>
            {
                var items = _splitProductBillService.GetSplitProductItemList(billId).Select(o =>
                  {
                      var m = o.ToModel<SplitProductItemModel>();

                      m.ProductName = _productService.GetProductName(curStore.Id, o.ProductId);
                      m.SubProductUnitName = _specificationAttributeService.GetSpecificationAttributeOptionName(0, o.SubProductUnitId ?? 0);

                      return m;

                  }).ToList();

                return Json(new
                {
                    Success = true,
                    total = items.Count,
                    rows = items
                });
            });
        }

        /// <summary>
        /// 创建/更新
        /// </summary>
        /// <param name="data"></param>
        /// <param name="billId"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.SplitOrderSave)]
        public async Task<JsonResult> CreateOrUpdate(SplitProductUpdateModel data, int? billId)
        {
            try
            {
                var bill = new SplitProductBill();

                #region 单据验证

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

                if (billId.HasValue && billId.Value != 0)
                {
                    bill = _splitProductBillService.GetSplitProductBillById(curStore.Id, billId.Value, true);

                    //公共单据验证
                    var commonBillChecking = BillChecking<SplitProductBill, SplitProductItem>(bill, BillStates.Draft);
                    if (commonBillChecking.Value != null)
                    {
                        return commonBillChecking;
                    }
                }
                #endregion

                #region 验证盘点
                if (data.Items.Any())
                {
                    if (_wareHouseService.CheckProductInventory(curStore.Id, data.WareHouseId, bill.Items?.Select(it => it.ProductId).Distinct().ToArray(), out string thisMsg))
                    {
                        return Warning(thisMsg);
                    }
                }
                #endregion

                #region 验证库存（主商品）

                IList<Product> allProducts = new List<Product>();

                //当前数据
                List<ProductStockItem> productStockItemNews = new List<ProductStockItem>();
                if (data.Items != null && data.Items.Count > 0)
                {
                    Product product = _productService.GetProductById(curStore.Id, data.ProductId);
                    if (product != null)
                    {
                        var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(0, product.GetProductBigStrokeSmallUnitIds());
                        if (product != null)
                        {
                            ProductStockItem productStockItem = productStockItemNews.Where(a => a.ProductId == data.ProductId).FirstOrDefault();
                            //商品转化量
                            var conversionQuantity = product.GetConversionQuantity(allOptions, product.SmallUnitId);
                            //库存量增量 = 单位转化量 * 数量
                            int thisQuantity = (data.Quantity ?? 0) * conversionQuantity;
                            if (productStockItem != null)
                            {
                                productStockItem.Quantity += thisQuantity;
                            }
                            else
                            {
                                productStockItem = new ProductStockItem
                                {
                                    ProductId = data.ProductId,
                                    //当期选择单位
                                    UnitId = product.SmallUnitId,
                                    //转化单位
                                    SmallUnitId = product.SmallUnitId,
                                    //转化单位
                                    BigUnitId = product.BigUnitId ?? 0,
                                    ProductName = product?.Name,
                                    ProductCode = product?.ProductCode,
                                    Quantity = thisQuantity
                                };

                                productStockItemNews.Add(productStockItem);
                            }
                        }
                    }
                }

                //验证库存
                if (!_stockService.CheckStockQty(_productService, _specificationAttributeService, curStore.Id, data.WareHouseId, productStockItemNews, out string thisMsg2))
                {
                    return Warning(thisMsg2);
                }

                #endregion

                var dataTo = data.ToEntity<SplitProductBillUpdate>();
                dataTo.Operation = (int)OperationEnum.PC;
                //主商品成本
                var mainProduct = _productService.GetProductById(curStore.Id, data.ProductId);
                if (mainProduct != null)
                {
                    dataTo.ProductCost = _purchaseBillService.GetReferenceCostPrice(curStore.Id, data.ProductId, mainProduct.SmallUnitId);
                    dataTo.ProductCostAmount = dataTo.ProductCost * dataTo.Quantity;
                }

                dataTo.Items = data.Items.Select(it =>
                {
                    //成本价（此处计算成本价防止web、api成本价未带出,web、api的controller都要单独计算（取消service计算，防止其他service都引用 pruchasebillservice））
                    var item = it.ToEntity<SplitProductItem>();
                    item.CostPrice = _purchaseBillService.GetReferenceCostPrice(curStore.Id, item.ProductId, item.UnitId);
                    item.CostAmount = item.CostPrice * item.Quantity;
                    return item;
                }).ToList();

                //RedLock
                var result = await _locker.PerformActionWithLockAsync(LockKey(data),
                  TimeSpan.FromSeconds(30),
                  TimeSpan.FromSeconds(10),
                  TimeSpan.FromSeconds(1),
                  () => _splitProductBillService.BillCreateOrUpdate(curStore.Id, curUser.Id, billId, bill, dataTo, dataTo.Items, productStockItemNews, _userService.IsAdmin(curStore.Id, curUser.Id)));
                return Json(result);

            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("CreateOrUpdate", Resources.Bill_CreateOrUpdateFailed, curUser.Id);
                _notificationService.SuccessNotification(Resources.Bill_CreateOrUpdateFailed);

                return Error(ex.Message);
            }

        }

        #endregion

        /// <summary>
        /// 审核
        /// </summary>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.SplitOrderApproved)]
        public async Task<JsonResult> Auditing(int? id)
        {
            try
            {

                var bill = new SplitProductBill();

                #region 验证

                if (!id.HasValue)
                {
                    return Warning("参数错误.");
                }
                else
                {
                    bill = _splitProductBillService.GetSplitProductBillById(curStore.Id, id.Value, true);
                    if (bill.AuditedStatus)
                    {
                        return Warning("单据已审核，请刷新页面.");
                    }
                }

                //公共单据验证
                var commonBillChecking = BillChecking<SplitProductBill, SplitProductItem>(bill, BillStates.Audited);
                if (commonBillChecking.Value != null)
                {
                    return commonBillChecking;
                }

                if (_wareHouseService.CheckProductInventory(curStore.Id, bill.WareHouseId, bill.Items?.Select(it => it.ProductId).Distinct().ToArray(), out string thisMsg))
                {
                    return Warning("仓库正在盘点中，拒绝操作.");
                }

                #region 验证库存（主商品）

                IList<Product> allProducts = new List<Product>();

                //当前数据
                List<ProductStockItem> productStockItemNews = new List<ProductStockItem>();
                if (bill.Items != null && bill.Items.Count > 0)
                {
                    Product product = _productService.GetProductById(curStore.Id, bill.ProductId);
                    if (product != null)
                    {
                        var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(bill.StoreId, product.GetProductBigStrokeSmallUnitIds());
                        if (product != null)
                        {
                            ProductStockItem productStockItem = productStockItemNews.Where(a => a.ProductId == bill.ProductId).FirstOrDefault();
                            //商品转化量
                            var conversionQuantity = product.GetConversionQuantity(allOptions, product.SmallUnitId);
                            //库存量增量 = 单位转化量 * 数量
                            int thisQuantity = (bill.Quantity ?? 0) * conversionQuantity;
                            if (productStockItem != null)
                            {
                                productStockItem.Quantity += thisQuantity;
                            }
                            else
                            {
                                productStockItem = new ProductStockItem
                                {
                                    ProductId = bill.ProductId,
                                    //当期选择单位
                                    UnitId = product.SmallUnitId,
                                    //转化单位
                                    SmallUnitId = product.SmallUnitId,
                                    //转化单位
                                    BigUnitId = product.BigUnitId ?? 0,
                                    ProductName = product?.Name,
                                    ProductCode = product?.ProductCode,
                                    Quantity = thisQuantity
                                };

                                productStockItemNews.Add(productStockItem);
                            }
                        }
                    }
                }

                //验证库存
                if (!_stockService.CheckStockQty(_productService, _specificationAttributeService, curStore.Id, bill.WareHouseId, productStockItemNews, out string thisMsg2))
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
                      () => _splitProductBillService.Auditing(curStore.Id, curUser.Id, bill));
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
        [AuthCode((int)AccessGranularityEnum.SplitOrderReverse)]
        public async Task<JsonResult> Reverse(int? id)
        {
            try
            {

                var bill = new SplitProductBill() { StoreId = curStore?.Id ?? 0 };

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
                    bill = _splitProductBillService.GetSplitProductBillById(curStore.Id, id.Value, true);
                    if (bill.ReversedStatus)
                    {
                        return Warning("单据已红冲，请刷新页面.");
                    }
                }

                //公共单据验证
                var commonBillChecking = BillChecking<SplitProductBill, SplitProductItem>(bill, BillStates.Reversed);
                if (commonBillChecking.Value != null)
                {
                    return commonBillChecking;
                }

                if (_wareHouseService.CheckProductInventory(curStore.Id, bill.WareHouseId, bill.Items?.Select(it => it.ProductId).Distinct().ToArray(), out string thisMsg))
                {
                    return Warning("仓库正在盘点中，拒绝操作.");
                }

                #region 验证库存（子商品）
                //将一个单据中 相同商品 数量 按最小单位汇总
                List<ProductStockItem> productStockItems = new List<ProductStockItem>();
                var allProducts = _productService.GetProductsByIds(curStore.Id, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(bill.StoreId, allProducts.GetProductBigStrokeSmallUnitIds());
                foreach (SplitProductItem item in bill.Items)
                {
                    var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                    ProductStockItem productStockItem = productStockItems.Where(a => a.ProductId == item.ProductId).FirstOrDefault();
                    //商品转化量
                    var conversionQuantity = product.GetConversionQuantity(allOptions, item.SubProductUnitId.Value);
                    //库存量增量 = 单位转化量 * 数量
                    int thisQuantity = (item.Quantity ?? 0) * conversionQuantity;
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
                      () => _splitProductBillService.Reverse(curUser.Id, bill));
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
        [AuthCode((int)AccessGranularityEnum.SplitOrderPrint)]
        public JsonResult PrintSetting()
        {
            var printTemplates = _printTemplateService.GetAllPrintTemplates(curStore.Id).ToList();
            var printTemplate = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.SplitProductBill).FirstOrDefault();
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
        [AuthCode((int)AccessGranularityEnum.SplitOrderPrint)]
        public JsonResult Print(int type, string selectData, int? wareHouseId, string billNumber = "", bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? showReverse = null, bool? sortByAuditedTime = null, string remark = "")
        {
            try
            {

                bool fg = true;
                string errMsg = string.Empty;

                #region 查询打印数据
                IList<SplitProductBill> splitProductBills = new List<SplitProductBill>();
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
                            SplitProductBill splitProductBill = _splitProductBillService.GetSplitProductBillById(curStore.Id, int.Parse(id), true);
                            if (splitProductBill != null)
                            {
                                splitProductBills.Add(splitProductBill);
                            }
                        }
                    }
                }
                else if (type == 2)
                {
                    splitProductBills = _splitProductBillService.GetAllSplitProductBills(
                                curStore?.Id ?? 0,
                                 curUser.Id,
                                wareHouseId,
                                billNumber,
                                auditedStatus,
                                startTime,
                                endTime,
                                showReverse,
                                sortByAuditedTime,
                                remark);
                }


                #endregion

                #region 修改数据
                if (splitProductBills != null && splitProductBills.Count > 0)
                {
                    //using (var scope = new TransactionScope())
                    //{
                    //    scope.Complete();
                    //}
                    #region 修改单据表打印数
                    foreach (var d in splitProductBills)
                    {
                        d.PrintNum = (d.PrintNum ?? 0) + 1;
                        _splitProductBillService.UpdateSplitProductBill(d);
                    }
                    #endregion
                }

                //获取打印模板
                var printTemplates = _printTemplateService.GetAllPrintTemplates(curStore.Id).ToList();
                var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.SplitProductBill).Select(a => a.Content).FirstOrDefault();

                //获取打印设置
                var pCPrintSetting = _settingService.LoadSetting<PCPrintSetting>(_storeContext.CurrentStore.Id);

                //填充打印数据
                var allProducts = _productService.GetProductsByIds(curStore.Id, splitProductBills.Select(pr => pr.ProductId).Distinct().ToArray());
                foreach (var d in splitProductBills)
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
                    }
                    User makeUser = _userService.GetUserById(curStore.Id, d.MakeUserId);
                    if (makeUser != null)
                    {
                        sb.Replace("@业务员", makeUser.UserRealName);
                        sb.Replace("@业务电话", makeUser.MobileNumber);
                    }
                    var mainProduct = allProducts.Where(ap => ap.Id == d.ProductId).FirstOrDefault();
                    if (mainProduct != null)
                    {
                        sb.Replace("@主商品", mainProduct.Name);
                    }
                    sb.Replace("@数量", d.Quantity == null ? "0" : d.Quantity.ToString());
                    sb.Replace("@单据编号", d.BillNumber);

                    #endregion

                    #region tbodyid
                    //明细
                    //获取 tbody 中的行
                    int beginTbody = sb.ToString().IndexOf("<tbody>") + 7;
                    int endTbody = sb.ToString().IndexOf("</tbody>");
                    string tbodytr = sb.ToString()[beginTbody..endTbody];

                    if (d.Items != null && d.Items.Count > 0)
                    {
                        //1.先删除明细第一行
                        sb.Remove(beginTbody, endTbody - beginTbody);
                        int i = 0;
                        var Products = _productService.GetProductsByIds(curStore.Id, d.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                        foreach (var item in d.Items)
                        {
                            int index = sb.ToString().IndexOf("</tbody>");
                            i++;
                            StringBuilder sb2 = new StringBuilder();
                            sb2.Append(tbodytr);

                            sb2.Replace("#序号", i.ToString());
                            var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();
                            if (product != null)
                            {
                                sb2.Replace("#商品名称", product.Name);
                            }
                            sb2.Replace("#子商品/主商品", "");
                            sb2.Replace("#生产日期", "");
                            sb2.Replace("#数量", item.Quantity == null ? "0" : item.Quantity.ToString());

                            sb.Insert(index, sb2);

                        }

                    }
                    #endregion

                    #region tfootid
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

        //导出
        [AuthCode((int)AccessGranularityEnum.SplitOrderExport)]
        public FileResult Export(int type, string selectData, int? wareHouseId, string billNumber = "", bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? showReverse = null, bool? sortByAuditedTime = null, string remark = "")
        {

            #region 查询导出数据
            IList<SplitProductBill> splitProductBills = new List<SplitProductBill>();
            if (type == 1)
            {
                if (!string.IsNullOrEmpty(selectData))
                {
                    List<string> ids = selectData.Split(',').ToList();
                    foreach (var id in ids)
                    {
                        SplitProductBill splitProductBill = _splitProductBillService.GetSplitProductBillById(curStore.Id, int.Parse(id), true);
                        if (splitProductBill != null)
                        {
                            splitProductBills.Add(splitProductBill);
                        }
                    }
                }
            }
            else if (type == 2)
            {
                splitProductBills = _splitProductBillService.GetAllSplitProductBills(
                            curStore?.Id ?? 0,
                             curUser.Id,
                            wareHouseId,
                            billNumber,
                            auditedStatus,
                            startTime,
                            endTime,
                            showReverse,
                            sortByAuditedTime,
                            remark);
            }

            #endregion

            #region 导出
            var ms = _exportManager.ExportSplitProductBillToXlsx(splitProductBills);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "拆分单.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "拆分单.xlsx");
            }
            #endregion
        }
    }
}