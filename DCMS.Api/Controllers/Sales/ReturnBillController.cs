using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Api.Models;
using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Domain.WareHouses;
using DCMS.Services.Configuration;
using DCMS.Services.Finances;
using DCMS.Services.Logging;
using DCMS.Services.Products;
using DCMS.Services.Purchases;
using DCMS.Services.Sales;
using DCMS.Services.Settings;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
using DCMS.Services.WareHouses;
using DCMS.ViewModel.Models.Sales;
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
    /// 退货单
    /// </summary>
   [Authorize]
    [Route("api/v{version:apiVersion}/dcms/sales")]
    public class ReturnBillController : BaseAPIController
    {

        private readonly IReturnBillService _returnBillService;
        private readonly IReturnReservationBillService _returnReservationBillService;
        private readonly IWareHouseService _wareHouseService;
        private readonly IProductService _productService;
        private readonly ISettingService _settingService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IProductTierPricePlanService _productTierPricePlanService;
        private readonly IUserService _userService;
        private readonly ITerminalService _terminalService;
        private readonly IAccountingService _accountingService;
        private readonly IDispatchBillService _dispatchBillService;
        private readonly IRedLocker _locker;
        private readonly IUserActivityService _userActivityService;
        private readonly IStockService _stockService;
        private readonly IPurchaseBillService _purchaseBillService;
        private readonly ICashReceiptBillService _cashReceiptBillService;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="returnBillService"></param>
        /// <param name="returnReservationBillService"></param>
        /// <param name="wareHouseService"></param>
        /// <param name="productService"></param>
        /// <param name="settingService"></param>
        /// <param name="specificationAttributeService"></param>
        /// <param name="productTierPricePlanService"></param>
        /// <param name="userService"></param>
        /// <param name="terminalService"></param>
        /// <param name="accountingService"></param>
        /// <param name="dispatchBillService"></param>
        /// <param name="locker"></param>
        /// <param name="userActivityService"></param>
        /// <param name="stockService"></param>
        /// <param name="purchaseBillService"></param>
        /// <param name="cashReceiptBillService"></param>
        /// <param name="logger"></param>
        public ReturnBillController(
            IReturnBillService returnBillService,
            IReturnReservationBillService returnReservationBillService,
            IWareHouseService wareHouseService,
            IProductService productService,
            ISettingService settingService,
            ISpecificationAttributeService specificationAttributeService,
            IProductTierPricePlanService productTierPricePlanService,
            IUserService userService,
            ITerminalService terminalService,
            IAccountingService accountingService,
           IDispatchBillService dispatchBillService,
            IRedLocker locker,
            IUserActivityService userActivityService,
            IStockService stockService,
            IPurchaseBillService purchaseBillService,
            ICashReceiptBillService cashReceiptBillService, 
            ILogger<BaseAPIController> logger) : base(logger)
        {
            _returnBillService = returnBillService;
            _returnReservationBillService = returnReservationBillService;
            _wareHouseService = wareHouseService;
            _productService = productService;
            _specificationAttributeService = specificationAttributeService;
            _productTierPricePlanService = productTierPricePlanService;
            _settingService = settingService;
            _userService = userService;
            _terminalService = terminalService;
            _accountingService = accountingService;
            _dispatchBillService = dispatchBillService;
            _locker = locker;
            _userActivityService = userActivityService;
            _stockService = stockService;
            _purchaseBillService = purchaseBillService;
            _cashReceiptBillService = cashReceiptBillService;
        }

        /// <summary>
        /// 获取退货单
        /// </summary>
        /// <param name="store"></param>
        /// <param name="terminalId"></param>
        /// <param name="terminalName"></param>
        /// <param name="businessUserId"></param>
        /// <param name="deliveryUserId"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="districtId"></param>
        /// <param name="remark"></param>
        /// <param name="billNumber"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="auditedStatus"></param>
        /// <param name="sortByAuditedTime"></param>
        /// <param name="showReverse"></param>
        /// <param name="showReturn"></param>
        /// <param name="paymentMethodType"></param>
        /// <param name="billSourceType"></param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet("returnbill/getbills/{store}/{terminalId}/{businessUserId}/{wareHouseId}")]
        [SwaggerOperation("getbills")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<ReturnBillModel>>> AsyncList(int? store, int? makeuserId, int? terminalId, string terminalName, int? businessUserId, int? deliveryUserId, int? wareHouseId, int? districtId, string remark, string billNumber, DateTime? startTime = null, DateTime? endTime = null, bool? auditedStatus = null, bool? sortByAuditedTime = null, bool? showReverse = null, bool? showReturn = null, int? paymentMethodType = null, int? billSourceType = null, bool? handleStatus = null, int pagenumber = 0, int pageSize = 50)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<ReturnBillModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var returns = _returnBillService.GetReturnBillList(store ?? 0,
                        makeuserId ?? 0,
                        terminalId,
                        terminalName,
                        businessUserId,
                        deliveryUserId,
                        billNumber,
                        wareHouseId,
                        remark,
                        startTime,
                        endTime,
                        districtId,
                        auditedStatus,
                        sortByAuditedTime,
                        showReverse,
                        paymentMethodType,
                        billSourceType,
                        null,
                        false,//未删除单据
                        null,
                        0,
                        pagenumber,
                        pageSize);

                    //默认收款账户动态列
                    var defaultAcc = _accountingService.GetDefaultAccounting(store ?? 0, BillTypeEnum.ReturnBill);

                    #region 查询需要关联其他表的数据

                    List<int> userIds = new List<int>();
                    userIds.AddRange(returns.Select(b => b.BusinessUserId).Distinct().ToArray());
                    userIds.AddRange(returns.Select(b => b.DeliveryUserId).Distinct().ToArray());
                    var allUsers = _userService.GetUsersDictsByIds(store ?? 0, userIds.Distinct().ToArray());
                    var allTerminal = _terminalService.GetTerminalsByIds(store ?? 0, returns.Select(b => b.TerminalId).Distinct().ToArray());
                    var allWareHouses = _wareHouseService.GetWareHouseByIds(store ?? 0, returns.Select(b => b.WareHouseId).Distinct().ToArray());
                    #endregion

                    var productIds = new List<int>();
                    returns?.ToList().ForEach(s =>
                    {
                        var pids = s?.Items?.Select(s => s.ProductId).ToList();
                        productIds.AddRange(pids);
                    });
                    var allProducts = _productService.GetProductsByIds(store ?? 0, productIds.Distinct().ToArray());

                    var results = returns.Select(s =>
                    {
                        var m = s.ToModel<ReturnBillModel>();

                        //业务员名称
                        m.BusinessUserName = allUsers.Where(aw => aw.Key == m.BusinessUserId).Select(aw => aw.Value).FirstOrDefault();
                        //送货员名称
                        m.DeliveryUserName = allUsers.Where(aw => aw.Key == m.DeliveryUserId).Select(aw => aw.Value).FirstOrDefault();

                        //客户名称
                        var terminal = allTerminal.Where(at => at.Id == m.TerminalId).FirstOrDefault();
                        m.TerminalName = terminal == null ? "" : terminal.Name;
                        m.TerminalPointCode = terminal == null ? "" : terminal.Code;
                        //仓库名称
                        var warehouse = allWareHouses.Where(aw => aw.Id == m.WareHouseId).FirstOrDefault();
                        m.WareHouseName = warehouse == null ? "" : warehouse.Name;

                        //应收金额	
                        m.ReceivableAmount = (s.ReceivableAmount == 0) ? ((s.OweCash != 0) ? s.OweCash : s.ReturnBillAccountings.Sum(sa => sa.CollectionAmount)) : s.ReceivableAmount;

                        //收款账户
                        m.ReturnBillAccountings = defaultAcc?.Item4?.OrderBy(sb => sb.Key).Select(sb =>
                        {
                            var acc = s.ReturnBillAccountings.Where(a => a?.AccountingOption?.ParentId == sb.Key).FirstOrDefault();
                            return new ReturnBillAccountingModel()
                            {
                                Name = acc?.AccountingOption?.Name,
                                AccountingOptionName = acc?.AccountingOption?.Name,
                                AccountingOptionId = acc?.AccountingOptionId ?? 0,
                                CollectionAmount = acc?.CollectionAmount ?? 0
                            };
                        }).Where(ao=>ao.AccountingOptionId>0).ToList();


                        //查询退货单 关联的退货订单
                        var returnReservationBill = _returnReservationBillService.GetReturnReservationBillById(store ?? 0, m.ReturnReservationBillId);
                        if (returnReservationBill != null)
                        {
                            m.ReturnReservationBillId = returnReservationBill.Id;
                            m.ReturnReservationBillNumber = returnReservationBill.BillNumber;
                        }

                        m.Items?.ToList()?.ForEach(item =>
                        {
                            var p = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault();
                            item.ProductName = p?.Name;
                            item.UnitName = _specificationAttributeService.GetSpecificationAttributeOptionName(store, item.UnitId);
                        });

                        return m;
                    }).ToList();

                    return this.Successful("", results);

                }
                catch (Exception ex)
                {
                    return this.Error<ReturnBillModel>(ex.Message);
                }

            });
        }


        /// <summary>
        /// 获取退货订单
        /// </summary>
        /// <param name="store"></param>
        /// <param name="billId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpGet("returnbill/getReturnBill/{store}/{billId}/{user}")]
        [SwaggerOperation("getReturnBill")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<ReturnBillModel>> GetReturnBill(int? store, int? billId, int user = 0)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<ReturnBillModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                var model = new ReturnBillModel();
                try
                {
                    var companySetting = _settingService.LoadSetting<CompanySetting>(store ?? 0);

                    var bill = _returnBillService.GetReturnBillById(store ?? 0, billId.Value, true);

                    model = bill.ToModel<ReturnBillModel>();

                    //获取默认付款账户
                    var defaultAcc = _accountingService.GetDefaultAccounting(store ?? 0, BillTypeEnum.ReturnBill);
                    model.CollectionAccount = defaultAcc?.Item1?.Id ?? 0;
                    model.CollectionAmount = bill.ReturnBillAccountings.Where(sa => sa.AccountingOptionId == defaultAcc?.Item1?.Id).Sum(sa => sa.CollectionAmount);
                    model.ReturnBillAccountings = bill.ReturnBillAccountings.Select(s =>
                    {
                        var m = s.ToModel<ReturnBillAccountingModel>();

                        m.AccountingOptionName = _accountingService.GetAccountingOptionName(store, s.AccountingOptionId);
                        m.Name = _accountingService.GetAccountingOptionName(store ?? 0, s.AccountingOptionId);
                        return m;
                    }).ToList();

                    //取单据项目
                    //var gridModel = _returnBillService.GetReturnItemList(billId.Value);
                    var allProductPrices = _productService.GetProductPricesByProductIds(store, bill.Items.Select(p => p.ProductId).ToArray());
                    var allProducts = _productService.GetProductsByIds(store ?? 0, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store, allProducts.GetProductBigStrokeSmallUnitIds());
                    var allProductTierPrices = _productService.GetProductTierPriceByProductIds(store, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());

                    model.Items = bill.Items.Select(o =>
                    {
                        var m = o.ToModel<ReturnItemModel>();
                        var product = allProducts.Where(ap => ap.Id == m.ProductId).FirstOrDefault();
                        if (product != null)
                        {
                            //这里替换成高级用法
                            m = product.InitBaseModel<ReturnItemModel>(m, o.ReturnBill.WareHouseId, allOptions, allProductPrices, allProductTierPrices, _productService);

                            m.UnitId = o.UnitId;
                            m.UnitName = o.UnitId == 0 ? m.Units.Keys.Select(k => k).ToArray()[2] : m.Units.Where(q => q.Value == o.UnitId).Select(q => q.Key)?.FirstOrDefault();
                        }
                        return m;
                    }).ToList(); 

                    //获取客户名称
                    var terminal = _terminalService.GetTerminalById(store ?? 0, model.TerminalId);
                    model.TerminalName = terminal == null ? "" : terminal.Name;
                    model.TerminalPointCode = terminal == null ? "" : terminal.Code;

                    #region 绑定数据源

                    model.BillTypeEnumId = (int)BillTypeEnum.ReturnBill;

                    //业务员
                    model.BusinessUsers = BindUserSelection(_userService.BindUserList, store ?? 0, DCMSDefaults.Salesmans, user, true, _userService.IsAdmin(store ?? 0, user));
                    model.BusinessUserName = _userService.GetUserName(store, model.BusinessUserId??0); 

                    //仓库
                    model.WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, store, WHAEnum.ReturnBill, user);
                    model.WareHouseName = _wareHouseService.GetWareHouseName(store, model.WareHouseId); 

                    model.PayTypeId = model.PayTypeId;

                    //送货员
                    model.DeliveryUsers = BindUserSelection(_userService.BindUserList, store ?? 0, DCMSDefaults.Delivers);
                    model.DeliveryUserName = _userService.GetUserName(store, model.DeliveryUserId ?? 0); 

                    //默认售价类型
                    model.ReturnDefaultAmounts = BindPricePlanSelection(_productTierPricePlanService.GetAllPricePlan, store ?? 0);

                    //默认
                    model.DefaultAmountId = "0_5";

                    if (companySetting != null)
                    {
                        if (!string.IsNullOrEmpty(companySetting.DefaultPricePlan))
                        {
                            //分析配置 格式："{PricesPlanId}_{PriceTypeId}"
                            var settingDefault = model.ReturnDefaultAmounts?.Where(s => s.Value.EndsWith(companySetting.DefaultPricePlan.ToString())).FirstOrDefault();
                            //这里取默认（不选择时），否启手动下拉选择
                            model.DefaultAmountId = settingDefault?.Value; //如：0_0
                        }
                    }

                    #endregion

                    //制单人
                    var mu = _userService.GetUserName(store ?? 0, bill.MakeUserId);
                    model.MakeUserName = mu + " " + bill.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss");

                    //审核人
                    var au = _userService.GetUserName(store ?? 0, bill.AuditedUserId ?? 0);
                    model.AuditedUserName = au + " " + (bill.AuditedDate.HasValue ? bill.AuditedDate.Value.ToString("yyyy/MM/dd HH:mm:ss") : "");

                    //优惠后金额
                    model.PreferentialEndAmount = model.ReceivableAmount - model.PreferentialAmount;
                    model.IsShowCreateDate = companySetting.OpenBillMakeDate == 0 ? false : true;
                    //商品变价参考
                    model.VariablePriceCommodity = companySetting.VariablePriceCommodity;
                    //单据合计精度
                    model.AccuracyRounding = companySetting.AccuracyRounding;
                    //交易日期可选范围
                    model.AllowSelectionDateRange = companySetting.AllowSelectionDateRange;
                    //允许预收款支付成负数
                    model.AllowAdvancePaymentsNegative = companySetting.AllowAdvancePaymentsNegative;
                    //启用税务功能
                    model.EnableTaxRate = companySetting.EnableTaxRate;
                    model.TaxRate = companySetting.TaxRate;

                    return this.Successful("", model);
                }
                catch (Exception ex)
                {
                    return this.Error3<ReturnBillModel>(ex.Message);
                }
            });
        }


        /// <summary>
        /// 创建退货单/更新退货单
        /// </summary>
        /// <param name="data"></param>
        /// <param name="billId"></param>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost("returnbill/createorupdate/{store}/{userId}/{billId}")]
        [SwaggerOperation("createorupdate")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> CreateOrUpdate(ReturnBillUpdateModel data, int? billId, int? store, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(async () =>
            {

                try
                {
                    var bill = new ReturnBill();
                    var dataTo = data.ToEntity<ReturnBillUpdate>();
                    dataTo.Operation = (int)OperationEnum.APP;

                    #region APP转单签收时（关联转入订单）逻辑

                    if (data.OrderId > 0)
                    {
                        bill.ReturnReservationBillId = data.OrderId;
                        bill.Operation = (int)OperationEnum.APP;

                        //获取调度项目
                        var dispatchItem = _dispatchBillService.GetDispatchItemsById(store, data.DispatchItemId);
                        //获取调度单
                        var dispatchBill = _dispatchBillService.GetDispatchBillById(store, dispatchItem?.DispatchBillId ?? 0);
                        //调度信息
                        dataTo.dispatchBill = dispatchBill;
                        dataTo.dispatchItem = dispatchItem;
                        //验证
                        if (dispatchItem == null || dispatchBill == null)
                        {
                            return this.Warning("不能签收，调度信息不存在");
                        }
                        if (dispatchItem.SignStatus == 1)
                        {
                            return this.Warning("单据已签收");
                        }
                    }

                    #endregion

                    #region 单据验证

                    if (data == null || data.Items == null)
                        return this.Warning("请录入数据.");

                    if (PeriodLocked(DateTime.Now, store ?? 0))
                        return this.Warning("锁账期间,禁止业务操作.");

                    if (PeriodClosed(DateTime.Now, store ?? 0))
                        return this.Warning("会计期间已结账,禁止业务操作.");

                    if (billId.HasValue && billId.Value != 0)
                    {
                        bill = _returnBillService.GetReturnBillById(store ?? 0, billId.Value, true);

                        //公共单据验证
                        var commonBillChecking = BillChecking<ReturnBill, ReturnItem>(bill, store ?? 0, BillStates.Draft);
                        if (commonBillChecking.Data != null)
                            return commonBillChecking;
                    }

                    #endregion

                    //业务逻辑
                    var accountings = _accountingService.GetAllAccountingOptions(store ?? 0, 0, true);
          
                    if (data.Accounting == null)
                    {
                        return this.Warning("没有默认的付款账号");
                    }
                   dataTo.Accounting = data.Accounting.Select(ac =>
                    {
                        return ac.ToAccountEntity<ReturnBillAccounting>();
                    }).ToList();
                    dataTo.Items = data.Items.OrderBy(it => it.SortIndex).Select(it =>
                    {
                        //成本价（此处计算成本价防止web、api成本价未带出,web、api的controller都要单独计算（取消service计算，防止其他service都引用 pruchasebillservice））
                        var item = it.ToEntity<ReturnItem>();
                        item.CostPrice = _purchaseBillService.GetReferenceCostPrice(store ?? 0, item.ProductId, item.UnitId);
                        item.CostAmount = item.CostPrice * item.Quantity;
                        return item;

                    }).ToList();

                    //RedLock
                    var results = await _locker.PerformActionWithLockAsync(this.LockKey(data, store ?? 0, userId ?? 0),
                        TimeSpan.FromSeconds(30),
                        TimeSpan.FromSeconds(10),
                        TimeSpan.FromSeconds(1),
                        () => _returnBillService.BillCreateOrUpdate(store ?? 0, userId ?? 0, billId, bill, dataTo.Accounting, accountings, dataTo, dataTo.Items, new List<ProductStockItem>(), _userService.IsAdmin(store ?? 0, userId ?? 0)));


                    if (results.Success && data.OrderId > 0)
                    {
                        _returnReservationBillService.ChangedBill(data.OrderId, userId ?? 0);
                    }

                    return results.To(results);

                }
                catch (Exception ex)
                {
                    _userActivityService.InsertActivity("CreateOrUpdate", Resources.Bill_CreateOrUpdateFailed, userId ?? 0);
                    return this.Error(ex.Message);
                }
            });
        }

        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="id"></param>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("returnbill/auditing/{store}/{userId}/{id}")]
        [SwaggerOperation("auditing")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> Auditing(int? store, int? userId, int? id)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Warning("参数错误.");

            return await Task.Run(async () =>
            {
                var result = new APIResult<dynamic>();
                try
                {
                    var bill = new ReturnBill();

                    #region 验证
                    if (!id.HasValue)
                        return this.Warning("参数错误.");
                    else
                    { 
                        bill = _returnBillService.GetReturnBillById(store ?? 0, id.Value, true);
                        if (bill.AuditedStatus)
                        {
                            return Warning("单据已审核，请刷新页面.");
                        }
                    }

                    //公共单据验证
                    var commonBillChecking = BillChecking<ReturnBill, ReturnItem>(bill, store ?? 0, BillStates.Audited);
                    if (commonBillChecking.Data != null)
                        return commonBillChecking;

                    if (_wareHouseService.CheckProductInventory(store ?? 0, bill.WareHouseId, bill.Items?.Select(it => it.ProductId).Distinct().ToArray(), out string thisMsg))
                        return this.Warning("仓库正在盘点中，拒绝操作");
                    #endregion

                    //RedLock
                    var results = await _locker.PerformActionWithLockAsync(RedLockKey(bill, store ?? 0, userId ?? 0),
                          TimeSpan.FromSeconds(30),
                          TimeSpan.FromSeconds(10),
                          TimeSpan.FromSeconds(1),
                          () => _returnBillService.Auditing(store ?? 0, userId ?? 0, bill));

                    return this.Successful("审核成功", results);

                }
                catch (Exception ex)
                {
                    _userActivityService.InsertActivity("Auditing", "审核失败", userId);
                    return this.Error(ex.Message);
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
        [HttpGet("returnbill/reverse/{store}/{userId}/{id}")]
        [SwaggerOperation("reverse")]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> Reverse(int? store, int? userId, int? id, string remark = "")
        {
            if (!store.HasValue || store.Value == 0)
                return this.Warning("参数错误.");

            return await Task.Run(async () =>
            {
                try
                {
                    var bill = new ReturnBill() { StoreId = store ?? 0 };

                    #region 验证

                    if (this.PeriodClosed(DateTime.Now, store ?? 0))
                        return this.Warning("系统当月已经结转，不允许红冲.");

                    if (!id.HasValue)
                        return this.Warning("参数错误.");
                    else
                    { 
                        bill = _returnBillService.GetReturnBillById(store ?? 0, id.Value, true);
                        bill.Remark = remark;
                        if (bill.ReversedStatus)
                        {
                            return Warning("单据已红冲，请刷新页面.");
                        }
                    }

                    //公共单据验证
                    var commonBillChecking = BillChecking<ReturnBill, ReturnItem>(bill, store ?? 0, BillStates.Reversed);
                    if (commonBillChecking.Data != null)
                        return commonBillChecking;

                    if (_wareHouseService.CheckProductInventory(store ?? 0, bill.WareHouseId, bill.Items?.Select(it => it.ProductId).Distinct().ToArray(), out string thisMsg))
                        return this.Warning("仓库正在盘点中，拒绝操作.");

                    //验证是否收款
                    var cashReceipt = _cashReceiptBillService.CheckBillCashReceipt(store ?? 0, (int)BillTypeEnum.ReturnBill, bill.BillNumber);
                    if (cashReceipt.Item1)
                    {
                        return this.Warning($"单据在收款单:{cashReceipt.Item2}中已经收款.");
                    }

                    #region 验证库存
                    //将一个单据中 相同商品 数量 按最小单位汇总
                    List<ProductStockItem> productStockItems = new List<ProductStockItem>();
                    var allProducts = _productService.GetProductsByIds(store ?? 0, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store ?? 0, allProducts.GetProductBigStrokeSmallUnitIds());

                    foreach (ReturnItem item in bill.Items)
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
                    if (!_stockService.CheckStockQty(_productService, _specificationAttributeService, bill.StoreId, bill.WareHouseId, productStockItems, out string thisMsg2))
                    {
                        return this.Warning(thisMsg2);
                    }

                    #endregion
                    #endregion

                    //RedLock
                    var results = await _locker.PerformActionWithLockAsync(RedLockKey(bill, store ?? 0, userId ?? 0),
                          TimeSpan.FromSeconds(30),
                          TimeSpan.FromSeconds(10),
                          TimeSpan.FromSeconds(1),
                          () => _returnBillService.Reverse(userId ?? 0, bill));

                    return this.Successful("红冲成功", results);
                }
                catch (Exception ex)
                {
                    _userActivityService.InsertActivity("Reverse", "单据红冲失败", userId);
                    return this.Error(ex.Message);
                }
            });
        }



        /// <summary>
        /// 获取初始绑定
        /// </summary>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("returnbill/getinitdataasync/{store}/{userId}")]
        [SwaggerOperation("GetInitDataAsync")]
        //[AuthBaseFilter]
        public async Task<APIResult<ReturnBillModel>> GetInitDataAsync(int? store, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<ReturnBillModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var model = new ReturnBillModel
                    {
                        //默认售价（方案）
                        ReturnDefaultAmounts = BindPricePlanSelection(_productTierPricePlanService.GetAllPricePlan, store ?? 0),
                        DefaultAmountId = "0_5"
                    };

                    var companySetting = _settingService.LoadSetting<CompanySetting>(store ?? 0);
                    if (companySetting != null)
                    {
                        if (!string.IsNullOrEmpty(companySetting.DefaultPricePlan))
                        {
                            //分析配置 格式："{PricesPlanId}_{PriceTypeId}"
                            var settingDefault = model.ReturnDefaultAmounts?.Where(s => s.Value.EndsWith(companySetting.DefaultPricePlan.ToString())).FirstOrDefault();
                            //这里取默认（不选择时），否启手动下拉选择
                            model.DefaultAmountId = settingDefault?.Value; //如：0_0
                        }
                    }

                    //默认账户设置
                    var defaultAcc = _accountingService.GetDefaultAccounting(store ?? 0, BillTypeEnum.ReturnBill);
                    model.ReturnBillAccountings.Add(new  ReturnBillAccountingModel()
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
                    return this.Error3<ReturnBillModel>(ex.Message);
                }
            });
        }
    }
}