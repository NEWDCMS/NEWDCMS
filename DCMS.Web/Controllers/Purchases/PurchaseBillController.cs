using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Purchases;
using DCMS.Core.Domain.Users;
using DCMS.Core.Domain.WareHouses;
using DCMS.Services.Common;
using DCMS.Services.Configuration;
using DCMS.Services.ExportImport;
using DCMS.Services.Finances;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.OCMS;
using DCMS.Services.Products;
using DCMS.Services.Purchases;
using DCMS.Services.Settings;
using DCMS.Services.Users;
using DCMS.Services.WareHouses;
using DCMS.ViewModel.Models.Purchases;
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
using Task = System.Threading.Tasks.Task;

namespace DCMS.Web.Controllers
{
    /// <summary>
    /// 采购单
    /// </summary>
    public class PurchaseBillController : BasePublicController
    {
        private readonly IPrintTemplateService _printTemplateService;
        private readonly IUserActivityService _userActivityService;
        private readonly IPurchaseBillService _purchaseBillService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IWareHouseService _wareHouseService;
        private readonly IProductService _productService;
        private readonly IStockService _stockService;
        private readonly ISettingService _settingService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IUserService _userService;
        private readonly IAccountingService _accountingService;
        private readonly IMediaService _mediaService;
        private readonly IRedLocker _locker;
        private readonly IExportManager _exportManager;
        private readonly ICommonBillService _commonBillService;
        private readonly IPaymentReceiptBillService _paymentReceiptBillService;
        private readonly IOCMSProductsService _ocmsProductsService;
        private readonly ICharacterSettingService _characterSettingService;



        public PurchaseBillController(
            IWorkContext workContext,
            IStoreContext storeContext,
            ILogger loggerService,
            IPrintTemplateService printTemplateService,
            IUserActivityService userActivityService,
            IPurchaseBillService purchaseBillService,
            IManufacturerService manufacturerService,
            IWareHouseService wareHouseService,
            IProductService productService,
            IStockService stockService,
            ISettingService settingService,
            ISpecificationAttributeService specificationAttributeService,
            IUserService userService,
            IAccountingService accountingService,
            IMediaService mediaService,
            INotificationService notificationService,
            IRedLocker locker,
            IExportManager exportManager,
            ICommonBillService commonBillService,
            IPaymentReceiptBillService paymentReceiptBillService,
            IStaticCacheManager cacheManager,
            IOCMSProductsService ocmsProductsService,
            ICharacterSettingService characterSettingService
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            _printTemplateService = printTemplateService;
            _userActivityService = userActivityService;
            _purchaseBillService = purchaseBillService;
            _manufacturerService = manufacturerService;
            _wareHouseService = wareHouseService;
            _productService = productService;
            _stockService = stockService;
            _specificationAttributeService = specificationAttributeService;
            _settingService = settingService;
            _userService = userService;
            _accountingService = accountingService;
            _mediaService = mediaService;
            _locker = locker;
            _exportManager = exportManager;
            _commonBillService = commonBillService;
            _paymentReceiptBillService = paymentReceiptBillService;
            _ocmsProductsService = ocmsProductsService;
            _characterSettingService = characterSettingService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        [AuthCode((int)AccessGranularityEnum.PurchaseBillsView)]
        public IActionResult List(int? businessUserId, int? manufacturerId, int? wareHouseId = null, string billNumber = "", bool? printStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? auditedStatus = null, string remark = "", bool? sortByAuditedTime = null, bool? showReverse = null, int pagenumber = 0)
        {


            var model = new PurchaseBillListModel();

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }


            #region 绑定数据源

            model = PreparePurchaseBillListModel(model);

            model.BusinessUserId = (businessUserId ?? null);
            model.ManufacturerId = (manufacturerId ?? null);
            model.WareHouseId = (wareHouseId ?? null);
            model.BillNumber = billNumber;
            model.PrintStatus = printStatus;
            model.StartTime = (startTime == null) ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")) : (DateTime)startTime;
            model.EndTime = (endTime == null) ? DateTime.Now.AddDays(1) : (DateTime)endTime;
            model.AuditedStatus = auditedStatus;
            model.Remark = remark;
            model.SortByAuditedTime = sortByAuditedTime;
            model.ShowReverse = showReverse;

            #endregion

            var purchases = _purchaseBillService.GetPurchaseBillList(curStore?.Id ?? 0,
                 curUser.Id,
                businessUserId,
                manufacturerId,
                wareHouseId,
                billNumber,
                printStatus,
                //startTime,  // ?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")),
                //endTime, //?? DateTime.Now.AddDays(1),
                model.StartTime, //?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")),
                model.EndTime, //?? DateTime.Now.AddDays(1),
                auditedStatus,
                null,
                remark,
                sortByAuditedTime,
                showReverse,
                null,
                false,//未删除单据
                pagenumber,
                pageSize: 30);

            model.PagingFilteringContext.LoadPagedList(purchases);
            model.Lists = PreparePurchaseBillModel(purchases);
            return View(model);
        }

        /// <summary>
        /// 获取采购单列表
        /// </summary>
        /// <param name="businessUserId"></param>
        /// <param name="manufacturerId"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="billNumber"></param>
        /// <param name="printStatus"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="auditedStatus"></param>
        /// <param name="remark"></param>
        /// <param name="sortByAuditedTime"></param>
        /// <param name="showReverse"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.PurchaseBillsView)]
        public async Task<JsonResult> AsyncList(int? businessUserId, int? manufacturerId, int? wareHouseId = null, string billNumber = "", bool? printStatus = false, DateTime? startTime = null, DateTime? endTime = null, bool? auditedStatus = null, string remark = "", bool? sortByAuditedTime = null, bool? showReverse = null, int pageIndex = 0, int pageSize = 20)
        {
            return await Task.Run(() =>
            {
                var model = new PurchaseBillListModel();

                var purchases = _purchaseBillService.GetPurchaseBillList(curStore?.Id ?? 0,
                     curUser.Id,
                businessUserId,
                manufacturerId,
                wareHouseId,
                billNumber,
                printStatus,
                startTime,
                endTime,
                auditedStatus,
                null,
                remark,
                sortByAuditedTime,
                showReverse,
                null,
                false,//未删除单据
                pageIndex,
                pageSize);

                model.Lists = PreparePurchaseBillModel(purchases);

                return Json(new
                {
                    Success = true,
                    total = purchases.TotalCount,
                    rows = model.Lists
                });

            });
        }

        /// <summary>
        /// 添加采购单
        /// </summary>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.PurchaseBillsSave)]
        public IActionResult Create()
        {

            PurchaseBillModel model = new PurchaseBillModel();
            #region 绑定数据源
            var companySetting = _settingService.LoadSetting<CompanySetting>(curStore.Id);
            model = PreparePurchaseBillModel(model);

            #endregion

            model.PreferentialAmount = 0;
            model.PreferentialEndAmount = 0;
            model.OweCash = 0;
            model.TransactionDate = DateTime.Now;

            //单号
            model.BillNumber = CommonHelper.GetBillNumber(CommonHelper.GetEnumDescription(BillTypeEnum.PurchaseBill).Split(',')[1], curStore.Id);
            model.BillBarCode = _mediaService.GenerateBarCodeForBase64(model.BillNumber, 150, 50);
            //制单人
            var mu = _userService.GetUserById(curStore.Id, curUser.Id);
            model.MakeUserName = mu != null ? (mu.UserRealName + " " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")) : "";

            //默认账户设置
            var defaultAcc = _accountingService.GetDefaultAccounting(curStore?.Id ?? 0, BillTypeEnum.PurchaseBill);
            model.PurchaseBillAccountings.Add(new PurchaseBillAccountingModel()
            {
                AccountingOptionId = defaultAcc?.Item1?.Id ?? 0,
                CollectionAmount = 0,
                Name = defaultAcc.Item1?.Name,
                AccountCodeTypeId = defaultAcc?.Item1?.AccountCodeTypeId ?? 0
            });

            model.IsShowCreateDate = companySetting.OpenBillMakeDate == 2;
            //默认进价
            model.DefaultPurchasePrice = companySetting.DefaultPurchasePrice;
            //单据合计精度
            model.AccuracyRounding = companySetting.AccuracyRounding;
            //显示订单占用库存
            model.APPShowOrderStock = companySetting.APPShowOrderStock;
            //启用税务功能
            model.EnableTaxRate = companySetting.EnableTaxRate;
            model.TaxRate = companySetting.TaxRate;
            return View(model);
        }

        /// <summary>
        /// 编辑采购单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.PurchaseBillsView)]
        public IActionResult Edit(int? id)
        {

            //没有值跳转到列表
            if (id == null)
            {
                return RedirectToAction("List");
            }
            var companySetting = _settingService.LoadSetting<CompanySetting>(curStore.Id);
            var model = new PurchaseBillModel();

            PurchaseBill purchase = _purchaseBillService.GetPurchaseBillById(curStore.Id, id.Value, true);

            //没有值跳转到列表
            if (purchase == null)
            {
                return RedirectToAction("List");
            }
            //只能操作当前经销商数据
            if (purchase.StoreId != curStore.Id)
            {
                return RedirectToAction("List");
            }

            model = purchase.ToModel<PurchaseBillModel>();
            model.BillBarCode = _mediaService.GenerateBarCodeForBase64(purchase.BillNumber, 150, 50);

            //获取默认收款账户
            var defaultAcc = _accountingService.GetDefaultAccounting(curStore?.Id ?? 0, BillTypeEnum.SaleReservationBill);
            model.CollectionAccount = defaultAcc?.Item1?.Id ?? 0;
            model.CollectionAmount = purchase.PurchaseBillAccountings.Where(sa => sa.AccountingOptionId == defaultAcc?.Item1?.Id).Sum(sa => sa.CollectionAmount);
            model.PurchaseBillAccountings = purchase.PurchaseBillAccountings.Select(s =>
            {
                var m = s.ToAccountModel<PurchaseBillAccountingModel>();
                m.Name = _accountingService.GetAccountingOptionName(curStore.Id, s.AccountingOptionId);
                return m;
            }).ToList();

            //取单据项目
            model.Items =purchase.Items.Select(s => s.ToModel<PurchaseItemModel>()).ToList();

            #region 绑定数据源

            model = PreparePurchaseBillModel(model);

            #endregion

            //制单人
            //var mu = _userService.GetUserById(curStore.Id, purchase.MakeUserId);
            //model.MakeUserName = mu != null ? (mu.UserRealName + " " + purchase.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss")) : "";
            var mu = string.Empty;
            if (purchase.MakeUserId > 0)
            {
                mu = _userService.GetUserName(curStore.Id, purchase.MakeUserId);
            }
            model.MakeUserName = mu + " " + purchase.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss");

            //审核人
            //var au = _userService.GetUserById(curStore.Id, purchase.AuditedUserId ?? 0);
            //model.AuditedUserName = au != null ? (au.UserRealName + " " + (purchase.AuditedDate.HasValue ? purchase.AuditedDate.Value.ToString("yyyy/MM/dd HH:mm:ss") : "")) : "";
            var au = string.Empty;
            if (purchase.AuditedUserId != null && purchase.AuditedUserId > 0)
            {
                au = _userService.GetUserName(curStore.Id, purchase.AuditedUserId ?? 0);
            }
            model.AuditedUserName = au + " " + (purchase.AuditedDate.HasValue ? purchase.AuditedDate.Value.ToString("yyyy/MM/dd HH:mm:ss") : "");

            //优惠后金额
            model.PreferentialEndAmount = model.ReceivableAmount;
            model.IsShowCreateDate = companySetting.OpenBillMakeDate == 2;
            //单据合计精度
            model.AccuracyRounding = companySetting.AccuracyRounding;
            //显示订单占用库存
            model.APPShowOrderStock = companySetting.APPShowOrderStock;
            //启用税务功能
            model.EnableTaxRate = companySetting.EnableTaxRate;
            model.TaxRate = companySetting.TaxRate;
            return View(model);
        }

        #region 单据项目

        /// <summary>
        /// 异步获取采购单项目
        /// </summary>
        /// <param name="purchaseId"></param>
        /// <returns></returns>
        public JsonResult AsyncPurchaseItems(int purchaseBillId)
        {

            var gridModel = _purchaseBillService.GetPurchaseItemList(purchaseBillId);

            var allProducts = _productService.GetProductsByIds(curStore.Id, gridModel.Select(pr => pr.ProductId).Distinct().ToArray());
            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());
            var allProductPrices = _productService.GetProductPricesByProductIds(curStore.Id, gridModel.Select(gm => gm.ProductId).Distinct().ToArray());
            var allProductTierPrices = _productService.GetProductTierPriceByProductIds(curStore.Id, gridModel.Select(p => p.ProductId).Distinct().ToArray());

            var details = gridModel.Select(o =>
            {
                var m = o.ToModel<PurchaseItemModel>();
                var product = allProducts.Where(ap => ap.Id == o.ProductId).FirstOrDefault();
                if (product != null)
                {
                    //这里替换成高级用法
                    m = product.InitBaseModel<PurchaseItemModel>(m, o.PurchaseBill.WareHouseId, allOptions, allProductPrices, allProductTierPrices, _productService);

                    //商品信息
                    m.BigUnitId = product.BigUnitId;
                    m.StrokeUnitId = product.StrokeUnitId;
                    m.SmallUnitId = product.SmallUnitId;
                    m.IsManufactureDete = product.IsManufactureDete;

                    //税价总计
                    m.TaxPriceAmount = m.Amount;

                    if (o.PurchaseBill.TaxAmount > 0 && m.TaxRate > 0)
                    {
                        //税额
                        m.TaxPrice = m.Amount - m.Amount / (1 + m.TaxRate / 100);

                        //含税价格
                        m.ContainTaxPrice = m.Price;

                        m.Price /= (1 + m.TaxRate / 100);
                        m.Amount /= (1 + m.TaxRate / 100);
                    }
                }
                return m;

            }).ToList();

            return Json(new
            {
                total = details.Count,
                hidden = details.Count(c => c.TaxRate > 0) > 0,
                rows = details
            });
        }

        /// <summary>
        /// 更新/编辑项目
        /// </summary>
        /// <param name="data"></param>
        /// <param name="purchaseId"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.PurchaseBillsSave)]
        public async Task<JsonResult> CreateOrUpdate(PurchaseBillUpdateModel data, int? billId, bool doAudit = true)
        {
            try
            {
                var bill = new PurchaseBill();

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
                    bill = _purchaseBillService.GetPurchaseBillById(curStore.Id, billId.Value, true);

                    //公共单据验证
                    var commonBillChecking = BillChecking<PurchaseBill, PurchaseItem>(bill, BillStates.Draft,((int)AccessGranularityEnum.PurchaseBillsSave).ToString());
                    if (commonBillChecking.Value != null)
                    {
                        return commonBillChecking;
                    }
                }
                #endregion

                #region 预付款 验证

                //预付款 验证
                if (data.Accounting != null)
                {
                    if (data.AdvanceAmount > 0 && data.AdvanceAmount > data.AdvanceAmountBalance)
                    {
                        return Warning("预付款余额不足!");
                    }

                    #region 弃用
                    ////1.获取当前经销商 预付款科目Id
                    //int accountingOptionId = 0;
                    //AccountingOption accountingOption = _accountingService.GetAccountingOptionByAccountCodeTypeId(curStore.Id, (int)AccountingCodeEnum.Imprest);
                    //if (accountingOption != null)
                    //{
                    //    accountingOptionId = (accountingOption == null) ? 0 : accountingOption.Id;
                    //}
                    ////获取用户输入 预付款金额
                    //var advancePaymentAmount = data.Accounting.Where(ac => ac.AccountingOptionId == accountingOptionId).Sum(ac => ac.CollectionAmount);
                    //if (data.Accounting.Where(ac => ac.AccountingOptionId == accountingOptionId).ToList().Count > 0)
                    //{
                    //    //用户可用 预付款金额
                    //    decimal useAdvancePaymentAmount = _commonBillService.GetUseAdvancePaymentAmount(curStore.Id, data.ManufacturerId);
                    //    //如果输入预付款大于用户可用预付款
                    //    if (advancePaymentAmount > useAdvancePaymentAmount)
                    //    {
                    //        return this.Warning("用户输入预付款金额：" + advancePaymentAmount + ",大于用户可用预付款金额：" + useAdvancePaymentAmount);
                    //    }
                    //}
                    #endregion
                }
                #endregion

                #region 验证0元开单备注
                string errMsg = "";
                if (data != null && data.Items != null && data.Items.Count > 0)
                {
                    var items = data.Items.Where(it => it.ProductId > 0);
                    if (items != null && items.Count() > 0)
                    {
                        foreach (var item in items)
                        {
                            var product = _productService.GetProductById(curStore.Id, item.ProductId);
                            if (billId== null || billId == 0)
                            {
                                var rst = ChechProductSslEnable(product.ProductCode);
                                if (curStore.SslEnabled && rst)
                                {
                                    errMsg += $"商品：{product.Name},不允许手动采购.";
                                    break;
                                }
                            }
                            if (item.Price == 0 && string.IsNullOrEmpty(item.Remark))
                            {
                                errMsg += $"商品：{product.Name},0元开单，必须选择备注.";
                                break;
                            }
                        }
                    }
                }
                if (!string.IsNullOrEmpty(errMsg))
                {
                    return Warning(errMsg);
                }
                #endregion

                //业务逻辑
                //var tonketId = "";
                //
                var accountings = _accountingService.GetAllAccountingOptions(curStore.Id, 0, true);
                var dataTo = data.ToEntity<PurchaseBillUpdate>();
                dataTo.Operation = (int)OperationEnum.PC;
                if (data.Accounting == null)
                {
                    return Warning("没有默认的付款账号");
                }
                dataTo.Accounting = data.Accounting.Select(ac =>
                {
                    return ac.ToAccountEntity<PurchaseBillAccounting>();
                }).ToList();
                dataTo.Items = data.Items.Select(it =>
                {
                    var item = it.ToEntity<PurchaseItem>();
                    return item;
                }).ToList();

                //RedLock
                var result = await _locker.PerformActionWithLockAsync(LockKey(data),
                      TimeSpan.FromSeconds(30),
                      TimeSpan.FromSeconds(10),
                      TimeSpan.FromSeconds(1),
                      () => _purchaseBillService.BillCreateOrUpdate(curStore.Id, curUser.Id, billId, bill, dataTo.Accounting, accountings, dataTo, dataTo.Items, new List<ProductStockItem>(), _userService.IsAdmin(curStore.Id, curUser.Id), doAudit));
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

        //异步获取上次采购价格
        [HttpGet]
        public async Task<JsonResult> AsyncPurchaseItemByProductId(int productId, bool beforeTax = false)
        {
            return await Task.Run(() =>
            {
                Product product = _productService.GetProductById(curStore.Id, productId);
                var details = _purchaseBillService.GetPurchaseItemByProduct(product, curStore.Id, productId, beforeTax);

                var datas = Json(new
                {
                    SmallUnitId = (details != null && details.Item1 != null) ? details.Item1.UnitId : 0,
                    SmallPrise = (details != null && details.Item1 != null) ? details.Item1.Price : 0,
                    StrokeUnitId = (details != null && details.Item2 != null) ? details.Item2.UnitId : 0,
                    StrokePrise = (details != null && details.Item2 != null) ? details.Item2.Price : 0,
                    BigUnitId = (details != null && details.Item3 != null) ? details.Item3.UnitId : 0,
                    BigPrise = (details != null && details.Item3 != null) ? details.Item3.Price : 0,
                });

                return Json(new { Data = datas, Success = true });

            });
        }
        #endregion

        /// <summary>
        /// 审核
        /// </summary>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.PurchaseBillsApproved)]
        public async Task<JsonResult> Auditing(int? id)
        {
            try
            {
                var bill = new PurchaseBill();

                #region 验证

                if (!id.HasValue)
                {
                    return Warning("参数错误.");
                }
                else
                {
                    bill = _purchaseBillService.GetPurchaseBillById(curStore.Id, id.Value, true);
                    if (bill.AuditedStatus)
                    {
                        return Warning("单据已审核，请刷新页面.");
                    }
                }

                //公共单据验证
                var commonBillChecking = BillChecking<PurchaseBill, PurchaseItem>(bill, BillStates.Audited, ((int)AccessGranularityEnum.PurchaseBillsApproved).ToString());
                if (commonBillChecking.Value != null)
                {
                    return commonBillChecking;
                }

                if (_wareHouseService.CheckProductInventory(curStore.Id, bill.WareHouseId, bill.Items?.Select(it => it.ProductId).Distinct().ToArray(), out string thisMsg))
                {
                    return Warning("仓库正在盘点中，拒绝操作");
                }


                #region 预付款 验证
                //预付款 验证
                if (bill.PurchaseBillAccountings != null)
                {
                    //1.获取当前经销商 预付款科目Id
                    int accountingOptionId = 0;
                    AccountingOption accountingOption = _accountingService.GetAccountingOptionByAccountCodeTypeId(curStore.Id, (int)AccountingCodeEnum.Imprest);
                    if (accountingOption != null)
                    {
                        accountingOptionId = (accountingOption == null) ? 0 : accountingOption.Id;
                    }

                    //获取用户输入预付款金额
                    var advancePaymentAmount = bill.PurchaseBillAccountings.Where(ac => ac.AccountingOptionId == accountingOptionId).Sum(ac => ac.CollectionAmount);
                    if (bill.PurchaseBillAccountings.Where(ac => ac.AccountingOptionId == accountingOptionId).ToList().Count > 0)
                    {
                        //用户可用预付款金额
                        var useAdvancePaymentAmount = _commonBillService.CalcManufacturerBalance(curStore.Id, bill.ManufacturerId);
                        //如果输入预付款大于用户可用预付款
                        if (advancePaymentAmount > 0 && advancePaymentAmount > useAdvancePaymentAmount.AdvanceAmountBalance)
                        {
                            return Warning("用户输入预付款金额大于用户可用预付款余额");
                        }
                    }
                }
                #endregion

                #endregion

                //RedLock
                var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill),
                      TimeSpan.FromSeconds(30),
                      TimeSpan.FromSeconds(10),
                      TimeSpan.FromSeconds(1),
                      () => _purchaseBillService.Auditing(curStore.Id, curUser.Id, bill));

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
        [AuthCode((int)AccessGranularityEnum.PurchaseBillsReverse)]
        public async Task<JsonResult> Reverse(int? id)
        {
            try
            {
                var bill = new PurchaseBill() { StoreId = curStore?.Id ?? 0 };

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
                    bill = _purchaseBillService.GetPurchaseBillById(curStore.Id, id.Value, true);
                    if (bill.ReversedStatus)
                    {
                        return Warning("单据已红冲，请刷新页面.");
                    }
                }

                //公共单据验证
                var commonBillChecking = BillChecking<PurchaseBill, PurchaseItem>(bill, BillStates.Reversed, ((int)AccessGranularityEnum.PurchaseBillsReverse).ToString());
                if (commonBillChecking.Value != null)
                {
                    return commonBillChecking;
                }

                if (_wareHouseService.CheckProductInventory(curStore.Id, bill.WareHouseId, bill.Items?.Select(it => it.ProductId).Distinct().ToArray(), out string thisMsg))
                {
                    return Warning("仓库正在盘点中，拒绝操作");
                }

                //验证是否付款
                var paymentReceipt = _paymentReceiptBillService.CheckBillPaymentReceipt(curStore.Id, (int)BillTypeEnum.PurchaseBill, bill.BillNumber);
                if (paymentReceipt.Item1)
                {
                    return Warning($"单据在付款单:{paymentReceipt.Item2}中已经付款.");
                }

                #region 验证库存
                IList<Product> allProducts = new List<Product>();

                //当前数据
                List<ProductStockItem> productStockItemNews = new List<ProductStockItem>();
                if (bill.Items != null && bill.Items.Count > 0)
                {
                    allProducts = _productService.GetProductsByIds(curStore.Id, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());

                    foreach (PurchaseItem item in bill.Items)
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
                if (!_stockService.CheckStockQty(_productService, _specificationAttributeService, curStore.Id, bill.WareHouseId, productStockItemNews, out string errMsg))
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
                      () => _purchaseBillService.Reverse(curUser.Id, bill));

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
        /// 作废
        /// </summary>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.PurchaseBillsDelete)]
        public JsonResult Delete(int? id)
        {
            try
            {
                var bill = new PurchaseBill() { StoreId = curStore?.Id ?? 0 };

                #region 验证
                if (!id.HasValue)
                {
                    return Warning("参数错误.");
                }
                else
                {
                    bill = _purchaseBillService.GetPurchaseBillById(curStore.Id, id.Value, true);
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
                        var rs = _purchaseBillService.Delete(curUser.Id, bill);
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

        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="TerminalId"></param>
        /// <param name="BusinessUserId"></param>
        /// <param name="BillNumber"></param>
        /// <param name="WareHouseId"></param>
        /// <param name="Remark"></param>
        /// <param name="StartTime"></param>
        /// <param name="EndTime"></param>
        /// <param name="DistrictId"></param>
        /// <param name="AuditedStatus"></param>
        /// <param name="SortByAuditedTime"></param>
        /// <param name="ShowReverse"></param>
        /// <param name="ShowReturn"></param>
        /// <param name="PaymentMethodType"></param>
        /// <param name="BillSourceType"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.PurchaseBillsExport)]
        public FileResult Export(int type, string selectData, int? businessUserId, int? manufacturerId, int? wareHouseId = null, string billNumber = "", bool? printStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? auditedStatus = null, string remark = "", bool? sortByAuditedTime = null, bool? showReverse = null)
        {

            #region 查询导出数据

            IList<PurchaseBill> purchaseBills = new List<PurchaseBill>();
            if (type == 1)
            {
                if (!string.IsNullOrEmpty(selectData))
                {
                    List<string> ids = selectData.Split(',').ToList();
                    foreach (var id in ids)
                    {
                        PurchaseBill purchaseBill = _purchaseBillService.GetPurchaseBillById(curStore.Id, int.Parse(id), true);
                        if (purchaseBill != null)
                        {
                            purchaseBills.Add(purchaseBill);
                        }
                    }
                }
            }
            else if (type == 2)
            {
                purchaseBills = _purchaseBillService.GetPurchaseBillList(curStore?.Id ?? 0,
                     curUser.Id,
                        businessUserId,
                        manufacturerId,
                        wareHouseId,
                        billNumber,
                        printStatus,
                        startTime,
                        endTime,
                        auditedStatus,
                        null,
                        remark,
                        sortByAuditedTime,
                        showReverse,
                        null,
                        false,//未删除单据
                        0
                        );
            }

            #endregion

            #region 导出
            var ms = _exportManager.ExportPurchaseBillToXlsx(purchaseBills, curStore.Id);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "采购单.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "采购单.xlsx");
            }
            #endregion

        }
        [AuthCode((int)AccessGranularityEnum.PurchaseBillsPrint)]
        public JsonResult PrintSetting()
        {
            var printTemplates = _printTemplateService.GetAllPrintTemplates(curStore.Id).ToList();
            var printTemplate = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.PurchaseBill).FirstOrDefault();
            //获取打印设置
            var pCPrintSetting = _settingService.LoadSetting<PCPrintSetting>(_storeContext.CurrentStore.Id);
            var settings = new object();
            if (pCPrintSetting != null)
            {
                settings = new
                {
                    PaperWidth = (printTemplate?.PaperWidth == 0 || printTemplate?.PaperHeight == 0) ? pCPrintSetting.PaperWidth : printTemplate.PaperWidth,
                    PaperHeight = (printTemplate?.PaperWidth == 0 || printTemplate?.PaperHeight == 0) ? pCPrintSetting.PaperHeight : printTemplate.PaperHeight,
                    pCPrintSetting.BorderType,
                    pCPrintSetting.MarginTop,
                    pCPrintSetting.MarginBottom,
                    pCPrintSetting.MarginLeft,
                    pCPrintSetting.MarginRight,
                    pCPrintSetting.IsPrintPageNumber,
                    pCPrintSetting.PrintHeader,
                    pCPrintSetting.PrintFooter,
                    pCPrintSetting.FixedRowNumber,
                    pCPrintSetting.PrintSubtotal,
                    pCPrintSetting.PrintPort
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
        [AuthCode((int)AccessGranularityEnum.PurchaseBillsPrint)]
        public IActionResult Print(int type, string selectData, int? businessUserId, int? manufacturerId, int? wareHouseId = null, string billNumber = "", bool? printStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? auditedStatus = null, string remark = "", bool? sortByAuditedTime = null, bool? showReverse = null)
        {
            try
            {

                bool fg = true;
                string errMsg = string.Empty;

                #region 查询打印数据

                IList<PurchaseBill> purchaseBills = new List<PurchaseBill>();
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
                            PurchaseBill purchaseBill = _purchaseBillService.GetPurchaseBillById(curStore.Id, int.Parse(id), true);
                            if (purchaseBill != null)
                            {
                                purchaseBills.Add(purchaseBill);
                            }
                        }
                    }
                }
                else if (type == 2)
                {
                    purchaseBills = _purchaseBillService.GetPurchaseBillList(curStore?.Id ?? 0,
                         curUser.Id,
                       businessUserId,
                       manufacturerId,
                       wareHouseId,
                       billNumber,
                       printStatus,
                       startTime,
                       endTime,
                       auditedStatus,
                       null,
                       remark,
                       sortByAuditedTime,
                       showReverse,
                       null,
                       false,//未删除单据
                       0
                       );
                }



                #endregion

                #region 修改数据
                if (purchaseBills != null && purchaseBills.Count > 0)
                {
                    //using (var scope = new TransactionScope())
                    //{

                    //    scope.Complete();
                    //}
                    #region 修改单据表打印数
                    foreach (var d in purchaseBills)
                    {
                        d.PrintNum += 1;
                        _purchaseBillService.UpdatePurchaseBill(d);
                    }
                    #endregion
                }

                //获取打印模板
                var printTemplates = _printTemplateService.GetAllPrintTemplates(curStore.Id).ToList();
                var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.PurchaseBill).Select(a => a.Content).FirstOrDefault();

                //获取打印设置
                var pCPrintSetting = _settingService.LoadSetting<PCPrintSetting>(curStore.Id);

                //填充打印数据
                foreach (var d in purchaseBills)
                {

                    StringBuilder sb = new StringBuilder();
                    sb.Append(content);

                    #region theadid
                    //sb.Replace("@商铺名称", curStore.Name);
                    if (pCPrintSetting != null)
                    {
                        sb.Replace("@商铺名称", string.IsNullOrWhiteSpace(pCPrintSetting.StoreName) ? "&nbsp;" : pCPrintSetting.StoreName);
                    }

                    Manufacturer manufacturer = _manufacturerService.GetManufacturerById(curStore.Id, d.ManufacturerId);
                    if (manufacturer != null)
                    {
                        sb.Replace("@供应商", manufacturer.Name);
                    }
                    User businessUser = _userService.GetUserById(curStore.Id, d.BusinessUserId);
                    if (businessUser != null)
                    {
                        sb.Replace("@业务员", businessUser.UserRealName);
                        sb.Replace("@业务电话", businessUser.MobileNumber);
                    }
                    sb.Replace("@单据编号", d.BillNumber);
                    sb.Replace("@交易日期", d.TransactionDate == null ? "" : ((DateTime)d.TransactionDate).ToString("yyyy/MM/dd HH:mm:ss"));
                    sb.Replace("@打印日期", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                    WareHouse wareHouse = _wareHouseService.GetWareHouseById(curStore.Id, d.WareHouseId);
                    if (wareHouse != null)
                    {
                        sb.Replace("@仓库", wareHouse.Name);
                        sb.Replace("@库管员", "***");
                    }

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
                        var allProducts = _productService.GetProductsByIds(curStore.Id, d.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                        var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());
                        //1.先删除明细第一行
                        sb.Remove(beginTbody, endTbody - beginTbody);
                        int i = 0;
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
                            sb2.Replace("#价格", item.Price.ToString("0.00"));
                            sb2.Replace("#金额", item.Amount.ToString("0.00"));
                            sb2.Replace("#备注", item.Remark);

                            sb.Insert(index, sb2);

                        }
                        sb.Replace("辅助数量:###", sumItem1 + "大" + sumItem2 + "中" + sumItem3 + "小");
                        sb.Replace("数量:###", d.Items.Sum(s => s.Quantity).ToString());
                        sb.Replace("金额:###", d.Items.Sum(s => s.Amount).ToString("0.00"));
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

        public IActionResult CheckSslEnable(int productId) 
        {
            try
            {
                var product = _productService.GetProductById(curStore.Id, productId);
                var rst = ChechProductSslEnable(product.ProductCode);
                if (curStore.SslEnabled && rst)
                {
                    return Successful("");
                }
                return Warning("");
            }
            catch (Exception ex)
            {
                return Warning(ex.ToString(), new { suc = false });
            }
        }

        [NonAction]
        private IList<PurchaseBillModel> PreparePurchaseBillModel(IPagedList<PurchaseBill> purchases)
        {

            //默认收款账户动态列
            var defaultAcc = _accountingService.GetDefaultAccounting(curStore?.Id ?? 0, BillTypeEnum.PurchaseBill);

            //所有ProductId
            List<int> productIds = new List<int>();
            if (purchases != null && purchases.Count > 0)
            {
                purchases.ToList().ForEach(s =>
                {
                    if (s.Items != null && s.Items.Count > 0)
                    {
                        s.Items.ToList().ForEach(i =>
                        {
                            if (!productIds.Contains(i.ProductId))
                            {
                                productIds.Add(i.ProductId);
                            }
                        });
                    }
                });
            }
            var allProducts = _productService.GetProductsByIds(curStore.Id, productIds.ToArray());
            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(curStore.Id, allProducts.GetProductBigStrokeSmallUnitIds());
            var allProductPrices = _productService.GetProductPricesByProductIds(curStore.Id, allProducts.Select(ap => ap.Id).Distinct().ToArray());

            #region 查询需要关联其他表的数据

            var allUsers = _userService.GetUsersDictsByIds(curStore.Id, purchases.Select(b => b.BusinessUserId).Distinct().ToArray());
            var allManufacturer = _manufacturerService.GetManufacturersByIds(curStore.Id, purchases.Select(b => b.ManufacturerId).Distinct().ToArray());
            var allWareHouses = _wareHouseService.GetWareHouseByIds(curStore.Id, purchases.Select(b => b.WareHouseId).Distinct().ToArray(), true);
            #endregion

            return purchases.Select(s =>
            {
                var m = s.ToModel<PurchaseBillModel>();

                //业务员名称
                m.BusinessUserName = allUsers.Where(aw => aw.Key == m.BusinessUserId).Select(aw => aw.Value).FirstOrDefault();

                //供应商名称
                var manufacturer = allManufacturer.Where(am => am.Id == m.ManufacturerId).FirstOrDefault();
                m.ManufacturerName = manufacturer == null ? "" : manufacturer.Name;

                //仓库名称
                var warehouse = allWareHouses.Where(aw => aw.Id == m.WareHouseId).FirstOrDefault();
                m.WareHouseName = warehouse == null ? "" : warehouse.Name;

                //应收金额	
                //优化获取方式:为减少数据库请求
                m.ReceivableAmount = (s.ReceivableAmount == 0) ? ((s.OweCash != 0) ? s.OweCash : s.PurchaseBillAccountings.Sum(sa => sa.CollectionAmount)) : s.ReceivableAmount;


                //优惠金额
                m.PreferentialAmount = s.PreferentialAmount;

                //付款账户
                m.PurchaseBillAccountings = defaultAcc?.Item4?.OrderBy(sb => sb.Key).Select(sb =>
                {
                    var acc = s.PurchaseBillAccountings.Where(a => a?.AccountingOption?.ParentId == sb.Key).FirstOrDefault();
                    return new PurchaseBillAccountingModel()
                    {
                        Name = acc?.AccountingOption?.Name,
                        AccountingOptionId = acc?.AccountingOptionId ?? 0,
                        CollectionAmount = acc?.CollectionAmount ?? 0
                    };
                }).ToList();


                return m;
            }).ToList();

        }

        [NonAction]
        private PurchaseBillListModel PreparePurchaseBillListModel(PurchaseBillListModel model)
        {
            //默认收款账户动态列
            var defaultAcc = _accountingService.GetDefaultAccounting(curStore?.Id ?? 0, BillTypeEnum.PurchaseBill);
            model.DynamicColumns = defaultAcc?.Item4?.OrderBy(s => s.Key).Select(s => s.Value).ToList();

            //业务员
            model.BusinessUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Salesmans,curUser.Id,true, _userService.IsAdmin(curStore.Id, curUser.Id));

            //仓库 
            model.WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore, WHAEnum.PurchaseBill, curUser.Id);

            //供应商
            model.Manufacturers = BindManufacturerSelection(_manufacturerService.BindManufacturerList, curStore);

            return model;
        }


        [NonAction]
        private PurchaseBillModel PreparePurchaseBillModel(PurchaseBillModel model)
        {
            model.BillTypeEnumId = (int)BillTypeEnum.PurchaseBill;

            //业务员
            model.BusinessUsers = BindUserSelection(_userService.BindUserList, curStore, DCMSDefaults.Salesmans,curUser.Id,true, _userService.IsAdmin(curStore.Id, curUser.Id));

            //当前用户为业务员时默认绑定
            if (curUser.IsSalesman() && !(model.Id > 0))
            {
                model.BusinessUserId = curUser.Id;
            }
            else
            {
                model.BusinessUserId = model.BusinessUserId == 0 ? null : model.BusinessUserId;
            }

            //仓库
            model.WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, curStore, WHAEnum.PurchaseBill, curUser.Id);

            //供应商
            model.ManufacturerId = model.ManufacturerId;
            model.ManufacturerName = _manufacturerService.GetManufacturerName(curStore.Id, model.ManufacturerId);


            ////可用预付款金额
            //decimal prepaidAmount = 0;
            ////增加条件，避免不必要查询
            //if (curStore.Id > 0 && model.ManufacturerId > 0)
            //{
            //    prepaidAmount = _commonBillService.CalcManufacturerBalance(curStore.Id, model.ManufacturerId).AdvanceAmountBalance;
            //}
            //model.PrepaidAmount = prepaidAmount;

            ////总欠款金额（历史欠款）
            //model.OweCashTotal = _commonBillService.CalcManufacturerBalance(curStore.Id, model.ManufacturerId).TotalOweCash;

            model.MBalance = _commonBillService.CalcManufacturerBalance(curStore.Id, model.ManufacturerId);

            return model;
        }
        [NonAction]
        private bool ChechProductSslEnable(string productCode) 
        {
            try
            {
                var rst = false;
                var entity = _ocmsProductsService.FindByCode(productCode);
                if (entity != null) 
                {
                    rst = _characterSettingService.Exists(int.Parse(curStore.Code).ToString(), entity.OCMS_PRODUCTID);
                }
                return rst;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}