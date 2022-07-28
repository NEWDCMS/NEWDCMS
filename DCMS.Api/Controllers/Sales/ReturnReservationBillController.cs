using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Api.Models;
using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Domain.WareHouses;
using DCMS.Services.Configuration;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Products;
using DCMS.Services.Purchases;
using DCMS.Services.Sales;
using DCMS.Services.Settings;
using DCMS.Services.Stores;
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
    /// 退货订单
    /// </summary>
   [Authorize]
    [Route("api/v{version:apiVersion}/dcms/sales")]
    public class ReturnReservationBillController : BaseAPIController
    {

        private readonly IReturnReservationBillService _returnReservationBillService;
        private readonly IReturnBillService _returnBillService;
        private readonly IWareHouseService _wareHouseService;
        private readonly IProductService _productService;
        private readonly ISettingService _settingService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IProductTierPricePlanService _productTierPricePlanService;
        private readonly IUserService _userService;
        private readonly ITerminalService _terminalService;
        private readonly IAccountingService _accountingService;
        
        private readonly IRedLocker _locker;
        private readonly IUserActivityService _userActivityService;
        private readonly IPurchaseBillService _purchaseBillService;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="returnReservationBillService"></param>
        /// <param name="returnBillService"></param>
        /// <param name="wareHouseService"></param>
        /// <param name="productService"></param>
        /// <param name="settingService"></param>
        /// <param name="specificationAttributeService"></param>
        /// <param name="productTierPricePlanService"></param>
        /// <param name="userService"></param>
        /// <param name="terminalService"></param>
        /// <param name="accountingService"></param>
        /// <param name="locker"></param>
        /// <param name="storeService"></param>
        /// <param name="userActivityService"></param>
        /// <param name="notificationService"></param>
        /// <param name="purchaseBillService"></param>
        /// <param name="logger"></param>
        public ReturnReservationBillController(
            IReturnReservationBillService returnReservationBillService,
            IReturnBillService returnBillService,
            IWareHouseService wareHouseService,
            IProductService productService,
            ISettingService settingService,
            ISpecificationAttributeService specificationAttributeService,
            IProductTierPricePlanService productTierPricePlanService,
            IUserService userService,
            ITerminalService terminalService,
            IAccountingService accountingService,
           
            IRedLocker locker,
            IStoreService storeService,
            IUserActivityService userActivityService,
            INotificationService notificationService,
            IPurchaseBillService purchaseBillService, 
            ILogger<BaseAPIController> logger) : base(logger)
        {
            _returnReservationBillService = returnReservationBillService;
            _returnBillService = returnBillService;
            _wareHouseService = wareHouseService;
            _productService = productService;
            _specificationAttributeService = specificationAttributeService;
            _productTierPricePlanService = productTierPricePlanService;
            _settingService = settingService;
            _userService = userService;
            _terminalService = terminalService;
            _accountingService = accountingService;
            
            _locker = locker;
            _userActivityService = userActivityService;
            _purchaseBillService = purchaseBillService;
        }

        /// <summary>
        /// 获取退货订单
        /// </summary>
        /// <param name="store"></param>
        /// <param name="terminalId"></param>
        /// <param name="terminalName"></param>
        /// <param name="businessUserId"></param>
        /// <param name="deliveryUserId"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="districtId"></param>
        /// <param name="billNumber"></param>
        /// <param name="remark"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="auditedStatus"></param>
        /// <param name="sortByAuditedTime"></param>
        /// <param name="showReverse"></param>
        /// <param name="showReturn"></param>
        /// <param name="alreadyChange"></param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet("returnreservationbill/getbills/{store}/{terminalId}/{businessUserId}/{wareHouseId}")]
        [SwaggerOperation("getbills")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<ReturnReservationBillModel>>> AsyncList(int? store, int? makeuserId, int? terminalId, string terminalName, int? businessUserId, int? deliveryUserId, int? wareHouseId, int? districtId, string billNumber, string remark, DateTime? startTime = null, DateTime? endTime = null, bool? auditedStatus = null, bool? sortByAuditedTime = null, bool? showReverse = null, bool? showReturn = null, bool? alreadyChange = false, int pagenumber = 0, int pageSize = 50)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<ReturnReservationBillModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {

                    var returns = _returnReservationBillService.GetReturnReservationBillList(store ?? 0,
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
                      alreadyChange,
                      false,//未删除单据
                      pagenumber,
                      pageSize: 30);

                    //默认账户
                    var defaultAcc = _accountingService.GetDefaultAccounting(store ?? 0, BillTypeEnum.ReturnReservationBill);

                    #region 查询需要关联其他表的数据

                    var allUsers = _userService.GetUsersDictsByIds(store ?? 0, returns.Select(b => b.BusinessUserId).Distinct().ToArray());
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
                        var m = s.ToModel<ReturnReservationBillModel>();

                        //业务员名称
                        m.BusinessUserName = allUsers.Where(aw => aw.Key == m.BusinessUserId).Select(aw => aw.Value).FirstOrDefault();

                        //送货员名称
                        m.DeliveryUserName = allUsers.Where(aw => aw.Key == m.DeliveryUserId).Select(aw => aw.Value).FirstOrDefault();

                        //客户名称
                        var terminal = allTerminal.Where(at => at.Id == m.TerminalId).FirstOrDefault();
                        m.TerminalName = terminal?.Name;
                        m.TerminalPointCode = terminal?.Code;

                        //仓库名称
                        var warehouse = allWareHouses.Where(aw => aw.Id == m.WareHouseId).FirstOrDefault();
                        m.WareHouseName = warehouse?.Name;

                        //应收金额	
                        m.ReceivableAmount = (s.ReceivableAmount == 0) ? ((s.OweCash != 0) ? s.OweCash : s.ReturnReservationBillAccountings.Sum(sa => sa.CollectionAmount)) : s.ReceivableAmount;

                        //收款账户
                        m.ReturnReservationBillAccountings = defaultAcc?.Item4?.OrderBy(sb => sb.Key).Select(sb =>
                            {
                                var acc = s.ReturnReservationBillAccountings.Where(a => a?.AccountingOption?.ParentId == sb.Key).FirstOrDefault();
                                return new ReturnReservationBillAccountingModel()
                                {
                                    Name = acc?.AccountingOption?.Name,
                                    AccountingOptionName = acc?.AccountingOption?.Name,
                                    AccountingOptionId = acc?.AccountingOptionId ?? 0,
                                    CollectionAmount = acc?.CollectionAmount ?? 0
                                };
                            }).Where(ao=>ao.AccountingOptionId>0).ToList();


                        //查询退货订单 关联的退货单
                        ReturnBill returnBill = _returnBillService.GetReturnBillByReturnReservationBillId(store ?? 0, m.Id);
                        if (returnBill != null)
                        {
                            m.ReturnBillId = returnBill.Id;
                            m.ReturnBillNumber = returnBill.BillNumber;
                        }

                        m.Items?.ToList()?.ForEach(item =>
                        {
                            var p = allProducts.Where(s => s.Id == item.ProductId).FirstOrDefault();
                            item.ProductName = p?.Name;
                            item.UnitName = _specificationAttributeService.GetSpecificationAttributeOptionName(store, item.UnitId);
                        });

                        return m;
                    }).ToList();

                    return this.Successful(Resources.Successful, results);
                }
                catch (Exception ex)
                {
                    return this.Error<ReturnReservationBillModel>(ex.Message);
                }
            });
        }

        /// <summary>
        /// 获取退货订单
        /// </summary>
        /// <param name="store"></param>
        /// <param name="billId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("returnreservationbill/getReturnreservationbill/{store}/{billId}/{userId}")]
        [SwaggerOperation("getReturnreservationbill")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<ReturnReservationBillModel>> GetReturnreservationbill(int? store, int? billId, int? userId = 0)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<ReturnReservationBillModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                var model = new ReturnReservationBillModel();
                try
                {
                    var companySetting = _settingService.LoadSetting<CompanySetting>(store ?? 0);

                    var bill = _returnReservationBillService.GetReturnReservationBillById(store ?? 0, billId.Value, true);
                    model = bill.ToModel<ReturnReservationBillModel>();

                    //默认收款账户金额
                    var defaultAcc = _accountingService.GetDefaultAccounting(store ?? 0, BillTypeEnum.ReturnReservationBill);
                    model.CollectionAccount = defaultAcc?.Item1?.Id ?? 0;
                    model.CollectionAmount = bill.ReturnReservationBillAccountings.Where(sa => sa.AccountingOptionId == defaultAcc?.Item1?.Id).Sum(sa => sa.CollectionAmount);
                    model.ReturnReservationBillAccountings = bill.ReturnReservationBillAccountings.Select(s =>
                    {
                        var m = s.ToModel<ReturnReservationBillAccountingModel>();

                        m.AccountingOptionName = _accountingService.GetAccountingOptionName(store, s.AccountingOptionId);
                        m.Name = _accountingService.GetAccountingOptionName(store ?? 0, s.AccountingOptionId);
                        return m;
                    }).ToList();

                    //取单据项目
                    //var gridModel = _returnReservationBillService.GetReturnReservationItemList(billId.Value);
                    var allProductPrices = _productService.GetProductPricesByProductIds(store, bill.Items.Select(p => p.ProductId).ToArray());
                    var allProducts = _productService.GetProductsByIds(store ?? 0, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());
                    var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store, allProducts.GetProductBigStrokeSmallUnitIds());
                    var allProductTierPrices = _productService.GetProductTierPriceByProductIds(store, bill.Items.Select(pr => pr.ProductId).Distinct().ToArray());

                    model.Items = bill.Items.Select(o =>
                    {
                        var m = o.ToModel<ReturnReservationItemModel>();
                        var product = allProducts.Where(ap => ap.Id == m.ProductId).FirstOrDefault();
                        if (product != null)
                        {
                            //这里替换成高级用法
                            m = product.InitBaseModel<ReturnReservationItemModel>(m, o.ReturnReservationBill.WareHouseId, allOptions, allProductPrices, allProductTierPrices, _productService);

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

                    model.BillTypeEnumId = (int)BillTypeEnum.ReturnReservationBill;

                    //业务员
                    model.BusinessUsers = BindUserSelection(_userService.BindUserList, store ?? 0, DCMSDefaults.Salesmans, userId ?? 0, true, _userService.IsAdmin(store ?? 0, userId ?? 0));

                    //当前用户为业务员时默认绑定
                    model.BusinessUserName = _userService.GetUserName(store, model.BusinessUserId??0);


                    //仓库
                    model.WareHouses = BindWareHouseSelection(_wareHouseService.BindWareHouseList, store ?? 0, WHAEnum.ReturnReservationBill, userId ?? 0);
                    model.WareHouseName = _wareHouseService.GetWareHouseName(store, model.WareHouseId); 

                    model.PayTypeId = model.PayTypeId;

                    //送货员
                    model.DeliveryUsers = BindUserSelection(_userService.BindUserList, store ?? 0, DCMSDefaults.Delivers);
                    model.DeliveryUserName = _userService.GetUserName(store, model.DeliveryUserId ?? 0);

                    //默认售价类型
                    model.ReturnReservationDefaultAmounts = BindPricePlanSelection(_productTierPricePlanService.GetAllPricePlan, store ?? 0);
                    //默认
                    model.DefaultAmountId = "0_5";

                    if (companySetting != null)
                    {
                        if (!string.IsNullOrEmpty(companySetting.DefaultPricePlan))
                        {
                            //分析配置 格式："{PricesPlanId}_{PriceTypeId}"
                            var settingDefault = model.ReturnReservationDefaultAmounts?.Where(s => s.Value.EndsWith(companySetting.DefaultPricePlan.ToString())).FirstOrDefault();
                            //这里取默认（不选择时），否启手动下拉选择
                            model.DefaultAmountId = settingDefault?.Value; //如：0_0
                        }
                    }

                    #endregion

                    //制单人
                    var mu = string.Empty;
                    if (bill.MakeUserId > 0)
                    {
                        mu = _userService.GetUserName(store ?? 0, bill.MakeUserId);
                    }
                    model.MakeUserName = mu + " " + bill.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss");

                    //审核人
                    var au = string.Empty;
                    if (bill.AuditedUserId != null && bill.AuditedUserId > 0)
                    {
                        au = _userService.GetUserName(store ?? 0, bill.AuditedUserId ?? 0);
                    }
                    model.AuditedUserName = au + " " + (bill.AuditedDate.HasValue ? bill.AuditedDate.Value.ToString("yyyy/MM/dd HH:mm:ss") : "");

                    model.IsShowCreateDate = companySetting.OpenBillMakeDate == 0 ? false : true;

                    //优惠后金额
                    model.PreferentialEndAmount = model.SumAmount - model.PreferentialAmount;

                    model.PayTypeId = model.PayTypeId;
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

                    return this.Successful(Resources.Successful, model);
                }
                catch (Exception ex)
                {
                    return this.Error3<ReturnReservationBillModel>(ex.Message);
                }

            });
        }


        /// <summary>
        /// 退货订单创建/更新
        /// </summary>
        /// <param name="data"></param>
        /// <param name="billId"></param>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost("returnreservationbill/createorupdate/{store}/{userId}/{billId}")]
        [SwaggerOperation("createorupdate")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> CreateOrUpdate(ReturnReservationBillUpdateModel data, int? billId, int? store, int? userId, bool doAudit = true)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(async () =>
            {
                try
                {
                    var bill = new ReturnReservationBill();

                    var dataTo = data.ToEntity<ReturnReservationBillUpdate>();
                    dataTo.Operation = (int)OperationEnum.APP;

                    #region 单据验证

                    if (data == null || data.Items == null)
                        return this.Warning("请录入数据.");

                    if (PeriodLocked(DateTime.Now, store ?? 0))
                        return this.Warning("锁账期间,禁止业务操作.");

                    if (PeriodClosed(DateTime.Now, store ?? 0))
                        return this.Warning("会计期间已结账,禁止业务操作.");


                    if (billId.HasValue && billId.Value != 0)
                    {
                        bill = _returnReservationBillService.GetReturnReservationBillById(store ?? 0, billId.Value, true);

                        //公共单据验证
                        var commonBillChecking = BillChecking<ReturnReservationBill, ReturnReservationItem>(bill, store ?? 0, BillStates.Draft);
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
                        return ac.ToAccountEntity<ReturnReservationBillAccounting>();
                    }).ToList();
                    dataTo.Items = data.Items.Select(it =>
                    {
                      //成本价（此处计算成本价防止web、api成本价未带出,web、api的controller都要单独计算（取消service计算，防止其他service都引用 pruchasebillservice））
                      var item = it.ToEntity<ReturnReservationItem>();
                        item.CostPrice = _purchaseBillService.GetReferenceCostPrice(store ?? 0, item.ProductId, item.UnitId);
                        item.CostAmount = item.CostPrice * item.Quantity;
                        return item;
                    }).ToList();

                    //RedLock
                    var result = await _locker.PerformActionWithLockAsync(this.LockKey(data, store ?? 0, userId ?? 0),
                        TimeSpan.FromSeconds(30),
                        TimeSpan.FromSeconds(10),
                        TimeSpan.FromSeconds(1),
                        () => _returnReservationBillService.BillCreateOrUpdate(store ?? 0, userId ?? 0, billId, bill, dataTo.Accounting, accountings, dataTo, dataTo.Items, new List<ProductStockItem>(), _userService.IsAdmin(store ?? 0, userId ?? 0),doAudit));

                    return this.Successful(Resources.Successful, result);
                }
                catch (Exception ex)
                {
                    //活动日志
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
        [HttpGet("returnreservationbill/auditing/{store}/{userId}/{id}")]
        [SwaggerOperation("auditing")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> Auditing(int? store, int? userId, int? id)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(async () =>
            {
                try
                {
                    var bill = new ReturnReservationBill();

                    #region 验证

                    if (!id.HasValue)
                        return this.Warning("参数错误.");
                    else
                    { 
                        bill = _returnReservationBillService.GetReturnReservationBillById(store ?? 0, id.Value, true);
                        if (bill.AuditedStatus)
                        {
                            return Warning("单据已审核，请刷新页面.");
                        }
                    }

                    //公共单据验证
                    var commonBillChecking = BillChecking<ReturnReservationBill, ReturnReservationItem>(bill, store ?? 0, BillStates.Audited);
                    if (commonBillChecking.Data != null)
                        return commonBillChecking;

                    if (_wareHouseService.CheckProductInventory(store ?? 0, bill.WareHouseId, bill.Items?.Select(it => it.ProductId).Distinct().ToArray(), out string thisMsg))
                        return this.Warning("仓库正在盘点中，拒绝操作.");
                    #endregion

                    //RedLock
                    var results = await _locker.PerformActionWithLockAsync(RedLockKey(bill, store ?? 0, userId ?? 0),
                          TimeSpan.FromSeconds(30),
                          TimeSpan.FromSeconds(10),
                          TimeSpan.FromSeconds(1),
                          () => _returnReservationBillService.Auditing(store ?? 0, userId ?? 0, bill));

                    return this.Successful("审核成功", results);
                }
                catch (Exception ex)
                {
                    _userActivityService.InsertActivity("Auditing", "单据审核失败", userId);
                    return this.Error(ex.Message);
                }

            });
        }

        [HttpGet("returnreservationbill/reverse/{store}/{userId}/{id}")]
        [SwaggerOperation("reverse")]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> Reverse(int? store, int? userId, int? id, string remark = "")
        {
            if (!store.HasValue || store.Value == 0)
            {
                return null;
            }

            return await Task.Run(async () =>
            {

                try
                {

                    var bill = new ReturnReservationBill() { StoreId = store ?? 0 };

                    if (this.PeriodClosed(DateTime.Now, store ?? 0))
                        return this.Warning("系统当月已经结转，不允许红冲");

                    #region 验证

                    if (this.PeriodClosed(DateTime.Now, store ?? 0))
                        return this.Warning("系统当月已经结转，不允许红冲");

                    if (!id.HasValue)
                        return this.Warning("参数错误.");
                    else
                    {
                        bill = _returnReservationBillService.GetReturnReservationBillById(store ?? 0, id.Value, true);
                        bill.Remark = remark;
                        if (bill.ReversedStatus)
                        {
                            return Warning("单据已红冲，请刷新页面.");
                        }
                    }

                    //公共单据验证
                    var commonBillChecking = BillChecking<ReturnReservationBill, ReturnReservationItem>(bill, store ?? 0, BillStates.Reversed);
                    if (commonBillChecking.Data != null)
                        return commonBillChecking;

                    if (_wareHouseService.CheckProductInventory(store ?? 0, bill.WareHouseId, bill.Items?.Select(it => it.ProductId).Distinct().ToArray(), out string thisMsg))
                        return this.Warning("仓库正在盘点中，拒绝操作.");

                    #endregion

                    //RedLock
                    var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill, store ?? 0, userId ?? 0),
                          TimeSpan.FromSeconds(30),
                          TimeSpan.FromSeconds(10),
                          TimeSpan.FromSeconds(1),
                          () => _returnReservationBillService.Reverse(userId ?? 0, bill));

                    return this.Successful("红冲成功", result);
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
        [HttpGet("returnreservationbill/getinitdataasync/{store}/{userId}")]
        [SwaggerOperation("GetInitDataAsync")]
        //[AuthBaseFilter]
        public async Task<APIResult<ReturnReservationBillModel>> GetInitDataAsync(int? store, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<ReturnReservationBillModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var model = new ReturnReservationBillModel
                    {
                        //默认售价（方案）
                        ReturnReservationDefaultAmounts = BindPricePlanSelection(_productTierPricePlanService.GetAllPricePlan, store ?? 0),
                        DefaultAmountId = "0_5"
                    };

                    var companySetting = _settingService.LoadSetting<CompanySetting>(store ?? 0);
                    if (companySetting != null)
                    {
                        if (!string.IsNullOrEmpty(companySetting.DefaultPricePlan))
                        {
                            //分析配置 格式："{PricesPlanId}_{PriceTypeId}"
                            var settingDefault = model.ReturnReservationDefaultAmounts?.Where(s => s.Value.EndsWith(companySetting.DefaultPricePlan.ToString())).FirstOrDefault();
                            //这里取默认（不选择时），否启手动下拉选择
                            model.DefaultAmountId = settingDefault?.Value; //如：0_0
                        }
                    }

                    //默认账户设置
                    var defaultAcc = _accountingService.GetDefaultAccounting(store ?? 0, BillTypeEnum.ReturnReservationBill);
                    model.ReturnReservationBillAccountings.Add(new  ReturnReservationBillAccountingModel()
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
                    return this.Error3<ReturnReservationBillModel>(ex.Message);
                }
            });
        }

    }
}