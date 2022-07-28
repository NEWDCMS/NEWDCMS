using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Api.Models;
using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Purchases;
using DCMS.Core.Domain.WareHouses;
using DCMS.Services.Common;
using DCMS.Services.Configuration;
using DCMS.Services.Finances;
using DCMS.Services.Logging;
using DCMS.Services.Products;
using DCMS.Services.Purchases;
using DCMS.Services.Settings;
using DCMS.Services.Users;
using DCMS.Services.WareHouses;
using DCMS.ViewModel.Models.Purchases;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using Microsoft.AspNetCore.Authorization;

namespace DCMS.Api.Controllers
{
    /// <summary>
    /// 采购单
    /// </summary>
    [Route("api/v{version:apiVersion}/dcms/purchases")]
    public class PurchaseBillController : BaseAPIController
    {
        private readonly IUserActivityService _userActivityService;
        private readonly IPurchaseBillService _purchaseBillService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IWareHouseService _wareHouseService;
        private readonly IProductService _productService;
        private readonly ISettingService _settingService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IUserService _userService;
        private readonly IAccountingService _accountingService;

        private readonly IRedLocker _locker;
        private readonly ICommonBillService _commonBillService;
        private readonly IStockService _stockService;
        private readonly IPaymentReceiptBillService _paymentReceiptBillService;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="userActivityService"></param>
        /// <param name="purchaseBillService"></param>
        /// <param name="manufacturerService"></param>
        /// <param name="wareHouseService"></param>
        /// <param name="productService"></param>
        /// <param name="settingService"></param>
        /// <param name="specificationAttributeService"></param>
        /// <param name="userService"></param>
        /// <param name="accountingService"></param>
        /// <param name="locker"></param>
        /// <param name="commonBillService"></param>
        /// <param name="stockService"></param>
        /// <param name="paymentReceiptBillService"></param>
        public PurchaseBillController(
            IUserActivityService userActivityService,
            IPurchaseBillService purchaseBillService,
            IManufacturerService manufacturerService,
            IWareHouseService wareHouseService,
            IProductService productService,
            ISettingService settingService,
            ISpecificationAttributeService specificationAttributeService,
            IUserService userService,
            IAccountingService accountingService,

            IRedLocker locker,
            ICommonBillService commonBillService,
            IStockService stockService,
            IPaymentReceiptBillService paymentReceiptBillService,
            ILogger<BaseAPIController> logger) : base(logger)
        {
            _userActivityService = userActivityService;
            _purchaseBillService = purchaseBillService;
            _manufacturerService = manufacturerService;
            _wareHouseService = wareHouseService;
            _productService = productService;
            _specificationAttributeService = specificationAttributeService;
            _settingService = settingService;
            _userService = userService;
            _accountingService = accountingService;

            _locker = locker;
            _commonBillService = commonBillService;
            _stockService = stockService;
            _paymentReceiptBillService = paymentReceiptBillService;
        }

        /// <summary>
        /// 获取采购单
        /// </summary>
        /// <param name="store"></param>
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
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet("purchases/getbills/{store}/{businessUserId}/{manufacturerId}/{wareHouseId}")]
        [SwaggerOperation("getbills")]
        [Authorize]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<PurchaseBillModel>>> AsyncList(int? store, int? makeuserId, int? businessUserId, int? manufacturerId, int? wareHouseId, string billNumber, string remark, bool? printStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? auditedStatus = null, bool? sortByAuditedTime = null, bool? showReverse = null, int pagenumber = 0, int pageSize = 50)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<PurchaseBillModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                var model = new PurchaseBillListModel();

                try
                {
                    var purchases = _purchaseBillService.GetPurchaseBillList(store ?? 0,
                        makeuserId,
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
                        false,
                        pagenumber,
                        pageSize);


                    //默认收款账户动态列
                    var defaultAcc = _accountingService.GetDefaultAccounting(store ?? 0, BillTypeEnum.PurchaseBill);

                    //所有ProductId
                    var productIds = new List<int>();
                    purchases?.ToList().ForEach(s =>
                    {
                        var pids = s?.Items?.Select(s => s.ProductId).ToList();
                        productIds.AddRange(pids);
                    });

                    var allProducts = _productService.GetProductsByIds(store ?? 0, productIds.ToArray());
                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store, allProducts.GetProductBigStrokeSmallUnitIds());
                    var allProductPrices = _productService.GetProductPricesByProductIds(store, allProducts.Select(ap => ap.Id).Distinct().ToArray());

                    #region 查询需要关联其他表的数据

                    var allUsers = _userService.GetUsersDictsByIds(store ?? 0, purchases.Select(b => b.BusinessUserId).Distinct().ToArray());
                    var allManufacturer = _manufacturerService.GetManufacturersByIds(store, purchases.Select(b => b.ManufacturerId).Distinct().ToArray());
                    var allWareHouses = _wareHouseService.GetWareHouseByIds(store, purchases.Select(b => b.WareHouseId).Distinct().ToArray(), true);
                    #endregion

                    var results = purchases.Select(s =>
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
                                  AccountingOptionName = acc?.AccountingOption?.Name,
                                  AccountingOptionId = acc?.AccountingOptionId ?? 0,
                                  CollectionAmount = acc?.CollectionAmount ?? 0
                              };
                          }).Where(ao => ao.AccountingOptionId > 0).ToList();

                         m.Items?.ToList()?.ForEach(item =>
                         {
                             var p = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault();
                             item.ProductName = p?.Name;
                             item.Subtotal = item.Amount;
                             item.UnitName = _specificationAttributeService.GetSpecificationAttributeOptionName(store, item.UnitId);
                         });

                         return m;
                     }).ToList();

                    return this.Successful(Resources.Successful, results);
                }
                catch (Exception ex)
                {
                    return this.Error<PurchaseBillModel>(ex.Message);
                }
            });
        }

        /// <summary>
        /// 获取采购单
        /// </summary>
        /// <param name="store"></param>
        /// <param name="billId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpGet("purchases/getPurchaseBill/{store}/{billId}/{user}")]
        [SwaggerOperation("getPurchaseBill")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        [Authorize]
        public async Task<APIResult<PurchaseBillModel>> GetPurchaseBill(int? store, int? billId, int user = 0)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Successful(Resources.ParameterError, new PurchaseBillModel());

            return await Task.Run(() =>
            {
                var bill = _purchaseBillService.GetPurchaseBillById(store, billId.Value, true);

                if (bill != null)
                {
                    var companySetting = _settingService.LoadSetting<CompanySetting>(store ?? 0);

                    var model = bill.ToModel<PurchaseBillModel>();

                    //获取默认收款账户
                    var defaultAcc = _accountingService.GetDefaultAccounting(store ?? 0, BillTypeEnum.SaleReservationBill);
                    model.CollectionAccount = defaultAcc?.Item1?.Id ?? 0;
                    model.CollectionAmount = bill.PurchaseBillAccountings.Where(sa => sa.AccountingOptionId == defaultAcc?.Item1?.Id).Sum(sa => sa.CollectionAmount);
                    model.PurchaseBillAccountings = bill.PurchaseBillAccountings?.Select(s =>
                    {
                        var m = s.ToModel<PurchaseBillAccountingModel>();
                        m.AccountingOptionName = _accountingService.GetAccountingOptionName(store, s.AccountingOptionId);
                        m.Name = _accountingService.GetAccountingOptionName(store, s.AccountingOptionId);
                        return m;
                    }).ToList();

                    //取单据项目
                    var allProductPrices = _productService.GetProductPricesByProductIds(store, bill.Items?.Select(p => p.ProductId).ToArray());
                    var allProducts = _productService.GetProductsByIds(store ?? 0, bill.Items?.Select(pr => pr.ProductId).Distinct().ToArray());
                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store, allProducts.GetProductBigStrokeSmallUnitIds());
                    var allProductTierPrices = _productService.GetProductTierPriceByProductIds(store, bill.Items?.Select(pr => pr.ProductId).Distinct().ToArray());

                    model.Items = bill.Items.Select(o =>
                    {
                        var m = o.ToModel<PurchaseItemModel>();
                        var product = allProducts.Where(ap => ap.Id == m.ProductId).FirstOrDefault();
                        if (product != null)
                        {
                            //这里替换成高级用法
                            m = product.InitBaseModel<PurchaseItemModel>(m, o.PurchaseBill.WareHouseId, allOptions, allProductPrices, allProductTierPrices, _productService);

                            m.UnitId = o.UnitId;
                            m.UnitName = o.UnitId == 0 ? m.Units.Keys.Select(k => k).ToArray()[2] : m.Units.Where(q => q.Value == o.UnitId).Select(q => q.Key)?.FirstOrDefault();
                            m.Subtotal = o.Amount;
                        }
                        return m;
                    }).ToList();

                    #region 绑定数据源

                    model.BillTypeEnumId = (int)BillTypeEnum.PurchaseBill;


  
                    //业务员
                    model.BusinessUsers = BindUserSelection(_userService.BindUserList, store ?? 0, DCMSDefaults.Salesmans, user, true, _userService.IsAdmin(store ?? 0, user));

                    model.BusinessUserName = _userService.GetUserName(store, model.BusinessUserId ?? 0);

                    //仓库
                    model.WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, store ?? 0, WHAEnum.PurchaseBill, user);
                    model.WareHouseName = _wareHouseService.GetWareHouseName(store, model.WareHouseId);

                    //供应商
                    model.ManufacturerName = _manufacturerService.GetManufacturerName(store, model.ManufacturerId);


                    //可用预付款金额
                    decimal prepaidAmount = 0;
                    //增加条件，避免不必要查询
                    if (store > 0 && model.ManufacturerId > 0)
                    {
                        prepaidAmount = _commonBillService.CalcManufacturerBalance(store ?? 0, model.ManufacturerId).AdvanceAmountBalance;
                    }
                    model.PrepaidAmount = prepaidAmount;

                    //总欠款金额（历史欠款）
                    model.OweCashTotal = _commonBillService.CalcManufacturerBalance(store ?? 0, model.ManufacturerId).TotalOweCash;

                    #endregion

                    //制单人
                    var mu = _userService.GetUserName(store, bill.MakeUserId);
                    model.MakeUserName = mu + " " + bill.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss");

                    //审核人
                    var au = _userService.GetUserName(store, bill.AuditedUserId ?? 0);
                    model.AuditedUserName = au + " " + (bill.AuditedDate.HasValue ? bill.AuditedDate.Value.ToString("yyyy/MM/dd HH:mm:ss") : "");

                    //优惠后金额
                    model.PreferentialEndAmount = model.ReceivableAmount;
                    model.IsShowCreateDate = companySetting.OpenBillMakeDate == 2 ? true : false;
                    //单据合计精度
                    model.AccuracyRounding = companySetting.AccuracyRounding;
                    //显示订单占用库存
                    model.APPShowOrderStock = companySetting.APPShowOrderStock;
                    //启用税务功能
                    model.EnableTaxRate = companySetting.EnableTaxRate;
                    model.TaxRate = companySetting.TaxRate;

                    return this.Successful<PurchaseBillModel>(Resources.Successful, model);
                }
                else
                {
                    return this.Successful<PurchaseBillModel>(Resources.Failed, new PurchaseBillModel());
                }

            });
        }

        /// <summary>
        /// 创建或者更新采购单
        /// </summary>
        /// <param name="data"></param>
        /// <param name="billId"></param>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost("purchases/createorupdate/{store}/{userId}/{billId}")]
        [SwaggerOperation("createorupdate")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> CreateOrUpdate(PurchaseBillUpdateModel data, int? billId, int? store, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);


            try
            {
                var bill = new PurchaseBill();

                #region 单据验证

                if (!userId.HasValue)
                    return this.Warning("用户未指定.");

                if (data == null || data.Items == null)
                    return this.Warning("请录入数据.");

                if (PeriodLocked(DateTime.Now, store ?? 0))
                    return this.Warning("锁账期间,禁止业务操作.");

                if (PeriodClosed(DateTime.Now, store ?? 0))
                    return this.Warning("会计期间已结账,禁止业务操作.");

                if (billId.HasValue && billId.Value != 0)
                {
                    bill = _purchaseBillService.GetPurchaseBillById(store, billId.Value, true);

                    //公共单据验证
                    var commonBillChecking = this.BillChecking<PurchaseBill, PurchaseItem>(bill, store ?? 0, BillStates.Draft);
                    if (commonBillChecking.Data != null)
                        return commonBillChecking;
                }
                #endregion

                #region 预付款 验证
                if (data.Accounting != null)
                {
                    if (data.AdvanceAmount > 0 && data.AdvanceAmount > data.AdvanceAmountBalance)
                    {
                        return this.Warning("预付款余额不足!");
                    }

                    #region 弃用
                    ////1.获取当前经销商 预付款科目Id
                    //int accountingOptionId = 0;
                    //AccountingOption accountingOption = _accountingService.GetAccountingOptionByAccountCodeTypeId(store, (int)AccountingCodeEnum.Imprest);
                    //if (accountingOption != null)
                    //{
                    //    accountingOptionId = (accountingOption == null) ? 0 : accountingOption.Id;
                    //}
                    ////获取用户输入 预付款金额
                    //var advancePaymentAmount = data.Accounting.Where(ac => ac.AccountingOptionId == accountingOptionId).Sum(ac => ac.CollectionAmount);
                    //if (data.Accounting.Where(ac => ac.AccountingOptionId == accountingOptionId).ToList().Count > 0)
                    //{
                    //    //用户可用 预付款金额
                    //    decimal useAdvancePaymentAmount = _commonBillService.GetUseAdvancePaymentAmount(store, data.ManufacturerId);
                    //    //如果输入预付款大于用户可用预付款
                    //    if (advancePaymentAmount > useAdvancePaymentAmount)
                    //    {
                    //        return this.Warning("用户输入预付款金额：" + advancePaymentAmount + ",大于用户可用预付款金额：" + useAdvancePaymentAmount);
                    //    }
                    //}
                    #endregion
                }
                #endregion

                //业务逻辑
                var accountings = _accountingService.GetAllAccountingOptions(store, 0, true);
                var dataTo = data.ToEntity<PurchaseBillUpdate>();
                dataTo.Operation = (int)OperationEnum.APP;
                if (data.Accounting == null)
                {
                    return this.Warning("没有默认的付款账号");
                }
                dataTo.Accounting = data.Accounting.Select(ac => ac.ToEntity<PurchaseBillAccounting>()).ToList();
                dataTo.Items = data.Items.OrderBy(it => it.SortIndex).Select(it => it.ToEntity<PurchaseItem>()).ToList();

                //RedLock
                var results = await _locker.PerformActionWithLockAsync(this.LockKey(data, store ?? 0, userId ?? 0),
                      TimeSpan.FromSeconds(30),
                      TimeSpan.FromSeconds(10),
                      TimeSpan.FromSeconds(1),
                      () => _purchaseBillService.BillCreateOrUpdate(store ?? 0, userId ?? 0, billId, bill, dataTo.Accounting, accountings, dataTo, dataTo.Items, new List<ProductStockItem>(), _userService.IsAdmin(store ?? 0, userId ?? 0)));

                return results.To(results);

            }
            catch (Exception ex)
            {
                //活动日志

                _userActivityService.InsertActivity("CreateOrUpdate", Resources.Bill_CreateOrUpdateFailed, userId ?? 0);
                return this.Error(ex.Message);
            }

        }


        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="id"></param>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("purchases/auditing/{store}/{userId}/{id}")]
        [SwaggerOperation("auditing")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        [Authorize]
        public async Task<APIResult<dynamic>> Auditing(int? store, int? userId, int? id)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);


            return await Task.Run(async () =>
            {
                try
                {
                    var bill = new PurchaseBill();

                    #region 验证

                    if (!id.HasValue)
                        return this.Warning("参数错误.");
                    else
                    {
                        bill = _purchaseBillService.GetPurchaseBillById(store, id.Value, true);
                        if (bill.AuditedStatus)
                        {
                            return Warning("单据已审核，请刷新页面.");
                        }
                    }


                    //公共单据验证
                    var commonBillChecking = BillChecking<PurchaseBill, PurchaseItem>(bill, store ?? 0, BillStates.Audited);
                    if (commonBillChecking.Data != null)
                        return commonBillChecking;

                    if (_wareHouseService.CheckProductInventory(store ?? 0, bill.WareHouseId, bill.Items?.Select(it => it.ProductId).Distinct().ToArray(), out string thisMsg))
                        return this.Warning("仓库正在盘点中，拒绝操作");

                    #region 预付款 验证
                    if (bill.PurchaseBillAccountings != null)
                    {
                        //1.获取当前经销商 预付款科目Id
                        int accountingOptionId = 0;
                        AccountingOption accountingOption = _accountingService.GetAccountingOptionByAccountCodeTypeId(store, (int)AccountingCodeEnum.Imprest);
                        if (accountingOption != null)
                        {
                            accountingOptionId = (accountingOption == null) ? 0 : accountingOption.Id;
                        }

                        //获取用户输入预付款金额
                        var advancePaymentAmount = bill.PurchaseBillAccountings.Where(ac => ac.AccountingOptionId == accountingOptionId).Sum(ac => ac.CollectionAmount);
                        if (bill.PurchaseBillAccountings.Where(ac => ac.AccountingOptionId == accountingOptionId).ToList().Count > 0)
                        {
                            //用户可用预付款金额
                            decimal useAdvancePaymentAmount = _commonBillService.CalcManufacturerBalance(store ?? 0, bill.ManufacturerId).AdvanceAmountBalance;
                            //如果输入预付款大于用户可用预付款
                            if (advancePaymentAmount > 0 && advancePaymentAmount > useAdvancePaymentAmount)
                            {
                                return this.Warning("预付款余额不足!");
                            }
                        }
                    }
                    #endregion

                    #endregion

                    //RedLock
                    var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill, store ?? 0, userId ?? 0),
                          TimeSpan.FromSeconds(30),
                          TimeSpan.FromSeconds(10),
                          TimeSpan.FromSeconds(1),
                          () => _purchaseBillService.Auditing(store ?? 0, userId ?? 0, bill));

                    return this.Successful("审核成功", result);
                }
                catch (Exception ex)
                {
                    _userActivityService.InsertActivity("Auditing", "单据审核失败", userId);
                    return this.Error("单据审核失败", ex.Message);
                }

            });
        }

        /// <summary>
        /// 红冲
        /// </summary>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("purchases/reverse/{store}/{userId}/{id}")]
        [SwaggerOperation("reverse")]
        //[AuthBaseFilter]
        [Authorize]
        public async Task<APIResult<dynamic>> Reverse(int? store, int? userId, int? id, string remark = "")
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);


            return await Task.Run(async () =>
            {
                try
                {
                    var bill = new PurchaseBill() { StoreId = store ?? 0 };

                    #region 验证

                    if (this.PeriodClosed(DateTime.Now, store ?? 0))
                        return this.Warning("系统当月已经结转，不允许红冲");

                    if (!id.HasValue)
                        return this.Warning("参数错误.");
                    else
                    {
                        bill = _purchaseBillService.GetPurchaseBillById(store, id.Value, true);
                        bill.Remark = remark;
                        if (bill.ReversedStatus)
                        {
                            return Warning("单据已红冲，请刷新页面.");
                        }
                    }

                    //公共单据验证
                    var commonBillChecking = BillChecking<PurchaseBill, PurchaseItem>(bill, store ?? 0, BillStates.Reversed);
                    if (commonBillChecking.Data != null)
                        return commonBillChecking;


                    if (_wareHouseService.CheckProductInventory(store ?? 0, bill.WareHouseId, bill.Items?.Select(it => it.ProductId).Distinct().ToArray(), out string thisMsg))
                        return this.Warning("仓库正在盘点中，拒绝操作");

                    #region 验证是否付款
                    var paymentReceipt = _paymentReceiptBillService.CheckBillPaymentReceipt(store ?? 0, (int)BillTypeEnum.PurchaseBill, bill.BillNumber);
                    if (paymentReceipt.Item1)
                    {
                        return this.Warning($"单据在付款单:{paymentReceipt.Item2}中已经付款.");
                    }
                    #endregion

                    #region 验证库存
                    IList<Product> allProducts = new List<Product>();

                    //当前数据
                    List<ProductStockItem> productStockItemNews = new List<ProductStockItem>();
                    if (bill.Items != null && bill.Items.Count > 0)
                    {
                        allProducts = _productService.GetProductsByIds(store ?? 0, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                        var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store, allProducts.GetProductBigStrokeSmallUnitIds());

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
                    if (!_stockService.CheckStockQty(_productService, _specificationAttributeService, store ?? 0, bill.WareHouseId, productStockItemNews, out string errMsg))
                    {
                        return this.Warning(errMsg);
                    }

                    #endregion
                    #endregion

                    //RedLock
                    var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill, store ?? 0, userId ?? 0),
                          TimeSpan.FromSeconds(30),
                          TimeSpan.FromSeconds(10),
                          TimeSpan.FromSeconds(1),
                          () => _purchaseBillService.Reverse(userId ?? 0, bill));

                    return Successful("单据红冲成功", result);

                }
                catch (Exception ex)
                {
                    _userActivityService.InsertActivity("Reverse", "单据红冲失败", userId);
                    return this.Error("单据红冲失败", ex.Message);
                }
            });
        }

        /// <summary>
        /// 获取上次采购价格
        /// </summary>
        /// <param name="store"></param>
        /// <param name="productId"></param>
        /// <param name="beforeTax"></param>
        /// <returns></returns>
        [HttpGet("purchases/lastpurchaseprice/{store}/{productId}")]
        [Authorize]
        public async Task<APIResult<IList<PurchaseItemModel>>> AsyncPurchaseItemByProductId(int? store, int productId, bool beforeTax = false)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<PurchaseItemModel>(Resources.ParameterError);
            return await Task.Run(() =>
            {

                try
                {
                    var model = new List<PurchaseItemModel>();

                    Product product = _productService.GetProductById(store, productId);
                    var details = _purchaseBillService.GetPurchaseItemByProduct(product, store ?? 0, productId, beforeTax);

                    if (details != null && details.Item1 != null)
                        model.Add(details.Item1.ToModel<PurchaseItemModel>());

                    if (details != null && details.Item2 != null)
                        model.Add(details.Item2.ToModel<PurchaseItemModel>());

                    if (details != null && details.Item3 != null)
                        model.Add(details.Item3.ToModel<PurchaseItemModel>());

                    return this.Successful(Resources.Successful, model);
                }
                catch (Exception ex)
                {
                    return this.Error<PurchaseItemModel>(ex.Message);
                }
            });
        }


        /// <summary>
        /// 获取初始绑定
        /// </summary>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("purchases/getinitdataasync/{store}/{userId}")]
        [SwaggerOperation("GetInitDataAsync")]
        //[AuthBaseFilter]
        [Authorize]
        public async Task<APIResult<PurchaseBillModel>> GetInitDataAsync(int? store, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<PurchaseBillModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var model = new PurchaseBillModel();

                    //默认账户设置
                    var defaultAcc = _accountingService.GetDefaultAccounting(store ?? 0, BillTypeEnum.PurchaseBill);
                    model.PurchaseBillAccountings.Add(new PurchaseBillAccountingModel()
                    {
                        AccountingOptionId = defaultAcc?.Item1?.Id ?? 0,
                        CollectionAmount = 0,
                        Name = defaultAcc.Item1?.Name,
                        AccountCodeTypeId = defaultAcc?.Item1?.AccountCodeTypeId ?? 0
                    });

                    return this.Successful("", model);
                }
                catch (Exception ex)
                {
                    return this.Error3<PurchaseBillModel>(ex.Message);
                }
            });
        }

    }
}