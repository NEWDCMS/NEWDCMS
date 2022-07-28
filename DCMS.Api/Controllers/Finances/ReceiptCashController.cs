using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Finances;
using DCMS.Services.Common;
using DCMS.Services.Configuration;
using DCMS.Services.Finances;
using DCMS.Services.Logging;
using DCMS.Services.Settings;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
using DCMS.ViewModel.Models.Finances;
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
    /// 用于收款单管理
    /// </summary>
    [Authorize]
    [Route("api/v{version:apiVersion}/dcms/finances")]
    public class ReceiptCashController : BaseAPIController
    {
        private readonly ITerminalService _terminalService;
        private readonly IUserService _userService;
        private readonly ICashReceiptBillService _cashReceiptBillService;
        private readonly IAccountingService _accountingService;
        private readonly IUserActivityService _userActivityService;
        private readonly ISettingService _settingService;
        private readonly IRedLocker _locker;
        private readonly IBillConvertService _billConvertService;
        private readonly ICommonBillService _commonBillService;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="terminalService"></param>
        /// <param name="cashReceiptBillService"></param>
        /// <param name="accountingService"></param>
        /// <param name="userActivityService"></param>
        /// <param name="settingService"></param>
        /// <param name="billConvertService"></param>
        /// <param name="userService"></param>
        /// <param name="locker"></param>
        /// <param name="commonBillService"></param>
        /// <param name="logger"></param>
        public ReceiptCashController(
            ITerminalService terminalService,
            ICashReceiptBillService cashReceiptBillService,
            IAccountingService accountingService,
            IUserActivityService userActivityService,
            ISettingService settingService,
            IBillConvertService billConvertService,

            IUserService userService,
            IRedLocker locker,
            ICommonBillService commonBillService,
            ILogger<BaseAPIController> logger) : base(logger)
        {
            _terminalService = terminalService;
            _cashReceiptBillService = cashReceiptBillService;
            _accountingService = accountingService;
            _userService = userService;
            _userActivityService = userActivityService;
            _settingService = settingService;

            _billConvertService = billConvertService;
            _locker = locker;
            _commonBillService = commonBillService;
        }

        /// <summary>
        /// 收款单列表
        /// </summary>
        /// <param name="store"></param>
        /// <param name="customerId"></param>
        /// <param name="customerName"></param>
        /// <param name="payeer"></param>
        /// <param name="billNumber"></param>
        /// <param name="auditedStatus"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="showReverse"></param>
        /// <param name="sortByAuditedTime"></param>
        /// <param name="remark"></param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet("receiptcashbill/getbills/{store}/{customerId}/{payeer}")]
        [SwaggerOperation("getbills")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<CashReceiptBillModel>>> AsyncList(int? store, int? makeuserId, int? customerId, string customerName, int? payeer, string billNumber, string remark, bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? showReverse = null, bool? sortByAuditedTime = null, bool? handleStatus = null, int pagenumber = 0, int pageSize = 50)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<CashReceiptBillModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    //默认收款账户动态列
                    var defaultAcc = _accountingService.GetDefaultAccounting(store ?? 0, BillTypeEnum.CashReceiptBill);

                    //获取分页
                    var bills = _cashReceiptBillService.GetAllCashReceiptBills(
                        store ?? 0,
                        makeuserId ?? 0,
                        customerId, payeer,
                        billNumber,
                        auditedStatus,
                        startTime,
                        endTime,
                        showReverse,
                        sortByAuditedTime,
                        remark,
                        null,
                        null,
                        pagenumber, 30);

                    #region 查询需要关联其他表的数据

                    var allUsers = _userService.GetUsersDictsByIds(store ?? 0, bills.Select(b => b.Payeer).Distinct().ToArray());
                    var allTerminal = _terminalService.GetTerminalsByIds(store ?? 0, bills.Select(b => b.CustomerId).Distinct().ToArray());
                    #endregion

                    var result = bills.OrderByDescending(b => b.CreatedOnUtc).Select(b =>
                    {
                        var m = b.ToModel<CashReceiptBillModel>();

                        //收款人
                        m.PayeerName = allUsers.Where(aw => aw.Key == m.Payeer).Select(aw => aw.Value).FirstOrDefault();

                        //客户名称
                        var terminal = allTerminal.Where(at => at.Id == m.CustomerId).FirstOrDefault();
                        m.CustomerName = terminal == null ? "" : terminal.Name;

                        //收款账户
                        m.CashReceiptBillAccountings = defaultAcc?.Item4?.OrderBy(s => s.Key).Select(s =>
                        {
                            var acc = b.CashReceiptBillAccountings.Where(a => a?.AccountingOption?.ParentId == s.Key).FirstOrDefault();
                            return new CashReceiptBillAccountingModel()
                            {
                                AccountingOptionId = acc?.AccountingOptionId ?? 0,
                                CollectionAmount = acc?.CollectionAmount ?? 0,
                                Name = acc?.AccountingOption?.Name,
                                AccountingOptionName = acc?.AccountingOption?.Name
                            };
                        }).Where(ao => ao.AccountingOptionId > 0).ToList();

                        //总优惠金额(本次优惠金额总和)
                        m.TotalDiscountAmountOnce = b.Items.Sum(sb => sb.DiscountAmountOnce);

                        //收款前欠款金额
                        m.BeforArrearsAmount = b.Items.Sum(sb => sb.ArrearsAmount);

                        //剩余金额(收款后尚欠金额总和)
                        m.TotalSubAmountOwedAfterReceipt = b.Items.Sum(sb => sb.AmountOwedAfterReceipt);

                        m.Items = b.Items.Select(sb =>
                        {
                            var it = sb.ToModel<CashReceiptItemModel>();
                            return it;

                        }).ToList();

                        return m;
                    }).ToList();

                    return this.Successful("", result);
                }
                catch (Exception ex)
                {
                    return this.Error<CashReceiptBillModel>(ex.Message);
                }
            });
        }

        /// <summary>
        /// 收款单
        /// </summary>
        /// <param name="store"></param>
        /// <param name="billId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpGet("receiptcashbill/getCashReceiptBill/{store}/{billId}/{user}")]
        [SwaggerOperation("getCashReceiptBill")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<CashReceiptBillModel>> GetCashReceiptBill(int? store, int? billId, int user = 0)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<CashReceiptBillModel>(false, Resources.ParameterError);

            return await Task.Run(() =>
            {
                var model = new CashReceiptBillModel();
                try
                {
                    var bill = _cashReceiptBillService.GetCashReceiptBillById(store ?? 0, billId ?? 0, true);
                    var companySetting = _settingService.LoadSetting<CompanySetting>(store ?? 0);

                    model = bill.ToModel<CashReceiptBillModel>();

                    model.Items = bill.Items.Select(b => b.ToModel<CashReceiptItemModel>()).ToList();

                    //收款账户
                    if (bill != null && bill.CashReceiptBillAccountings != null)
                    {
                        model.CashReceiptBillAccountings = bill.CashReceiptBillAccountings.Select(s =>
                        {
                            var m = s.ToAccountModel<CashReceiptBillAccountingModel>();
                            m.Name = _accountingService.GetAccountingOptionName(store ?? 0, s.AccountingOptionId);
                            m.AccountingOptionName = _accountingService.GetAccountingOptionName(store ?? 0, s.AccountingOptionId);
                            return m;
                        }).ToList();
                    }

                    //客户名称
                    var t = _terminalService.GetTerminalById(store ?? 0, bill.CustomerId);
                    if (t != null)
                    {
                        model.CustomerName = t.Name;
                    }

                    //收款人
                    model.Payeers = BindUserSelection(_userService.BindUserList, store ?? 0, DCMSDefaults.Employees);
                    model.PayeerName = _userService.GetUserName(store, model.Payeer ?? 0);

                    //制单人
                    var mu = _userService.GetUserName(store ?? 0, bill.MakeUserId);
                    model.MakeUserName = mu + " " + bill.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss");

                    //审核人
                    var au = _userService.GetUserName(store ?? 0, bill.AuditedUserId ?? 0);
                    model.AuditedUserName = au + " " +
                    (bill.AuditedDate.HasValue ? bill.AuditedDate.Value.ToString("yyyy/MM/dd HH:mm:ss") : "");

                    //允许预收款支付成负数
                    model.AllowAdvancePaymentsNegative = companySetting.AllowAdvancePaymentsNegative;

                    return this.Successful("", model);
                }
                catch (Exception ex)
                {
                    return this.Error<CashReceiptBillModel>(false, ex.Message);
                }


            });
        }


        /// <summary>
        /// 创建/更新收款单
        /// </summary>
        /// <param name="data"></param>
        /// <param name="billId"></param>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost("receiptcashbill/createorupdate/{store}/{userId}/{billId}")]
        [SwaggerOperation("createorupdate")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> CreateOrUpdate(CashReceiptUpdateModel data, int? billId, int? store, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(async () =>
            {
                try
                {
                    var bill = new CashReceiptBill();

                    if (data == null || data.Items == null)
                        return this.Warning("请录入数据.");

                    if (PeriodLocked(DateTime.Now, store ?? 0))
                        return this.Warning("锁账期间,禁止业务操作.");

                    if (PeriodClosed(DateTime.Now, store ?? 0))
                        return this.Warning("会计期间已结账,禁止业务操作.");

                    #region 单据验证
                    if (billId.HasValue && billId.Value != 0)
                    {
                        bill = _cashReceiptBillService.GetCashReceiptBillById(store ?? 0, billId.Value, true);

                        //公共单据验证
                        var commonBillChecking = BillChecking<CashReceiptBill, CashReceiptItem>(bill, store ?? 0, BillStates.Draft);
                        if (commonBillChecking.Data != null)
                            return commonBillChecking;

                    }
                    //判断指定单据是否尚有欠款(是否已经收完款)
                    var isDebt = true;
                    var warningMsg = string.Empty;
                    foreach (var item in data.Items)
                    {
                        isDebt = _cashReceiptBillService.ThereAnyDebt(store??0, item.BillTypeId, item.BillId);
                        if (!isDebt) //如果已经结清款项，则返回提示;若没有，则继续循环
                        {
                            warningMsg = $"非法操作，单据{item.BillNumber}已结清款项.";
                            break;
                        }
                        //判断单据是否存在未审核的收款单
                        isDebt = _cashReceiptBillService.ExistsUnAuditedByBillNumber(store??0, item.BillNumber, billId ?? 0);
                        if (!isDebt) //存在为甚的收款单
                        {
                            warningMsg = $"单据{item.BillNumber}存在未审核的收款单.";
                            break;
                        }
                    }
                    if (!isDebt)
                    {
                        return Warning(warningMsg);
                    }
                    #endregion

                    #region 预收款 验证
                    if (data.Accounting != null)
                    {
                        if (data.AdvanceAmount > 0 && data.AdvanceAmount > data.AdvanceAmountBalance)
                        {
                            return this.Warning("预收款余额不足!");
                        }
                    }
                    #endregion

                    //业务逻辑
                    var accountings = _accountingService.GetAllAccountingOptions(store ?? 0, 0, true);
                    var dataTo = data.ToEntity<CashReceiptBillUpdate>();
                    dataTo.Operation = (int)OperationEnum.APP;

                    if (data.Accounting == null)
                    {
                        return this.Warning("没有默认的付款账号");
                    }

                    dataTo.Accounting = data.Accounting.Select(ac => ac.ToEntity<CashReceiptBillAccounting>()).ToList();
                    dataTo.Items = data.Items.Select(it => it.ToEntity<CashReceiptItem>()).ToList();

                    //RedLock
                    var result = await _locker.PerformActionWithLockAsync(this.LockKey(data, store ?? 0, userId ?? 0),
                          TimeSpan.FromSeconds(30),
                          TimeSpan.FromSeconds(10),
                          TimeSpan.FromSeconds(1),
                          () => _cashReceiptBillService.BillCreateOrUpdate(store ?? 0, userId ?? 0, billId, bill, dataTo.Accounting, accountings, dataTo, dataTo.Items, _userService.IsAdmin(store ?? 0, userId ?? 0)));

                    return result.To(result);
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
        [HttpGet("receiptcashbill/auditing/{store}/{userId}/{id}")]
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
                    var bill = new CashReceiptBill();

                    #region 验证
                    if (!id.HasValue)
                        return this.Warning("参数错误.");
                    else
                    {
                        bill = _cashReceiptBillService.GetCashReceiptBillById(store ?? 0, id.Value, true);
                        if (bill.AuditedStatus)
                        {
                            return Warning("单据已审核，请刷新页面.");
                        }
                    }

                    //公共单据验证
                    var commonBillChecking = BillChecking<CashReceiptBill, CashReceiptItem>(bill, store ?? 0, BillStates.Audited);
                    if (commonBillChecking.Data != null)
                        return commonBillChecking;

                    #endregion

                    //RedLock
                    var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill, store ?? 0, userId ?? 0),
                          TimeSpan.FromSeconds(30),
                          TimeSpan.FromSeconds(10),
                          TimeSpan.FromSeconds(1),
                          () => _cashReceiptBillService.Auditing(store ?? 0, userId ?? 0, bill));
                    return result.To(result);
                }
                catch (Exception ex)
                {
                    _userActivityService.InsertActivity("Auditing", ex.Message, userId);
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
        [HttpGet("receiptcashbill/reverse/{store}/{userId}/{id}")]
        [SwaggerOperation("reverse")]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> Reverse(int? store, int? userId, int? id, string remark = "")
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(async () =>
            {
                try
                {

                    var bill = new CashReceiptBill() { StoreId = store ?? 0 };

                    #region 验证

                    if (this.PeriodClosed(DateTime.Now, store ?? 0))
                        return this.Warning("系统当月已经结转，不允许红冲");

                    if (!id.HasValue)
                        return this.Warning("参数错误.");
                    else
                    {
                        bill = _cashReceiptBillService.GetCashReceiptBillById(store ?? 0, id.Value, true);
                        bill.Remark = remark;
                        if (bill.ReversedStatus)
                        {
                            return Warning("单据已红冲，请刷新页面.");
                        }
                    }

                    //公共单据验证
                    var commonBillChecking = BillChecking<CashReceiptBill, CashReceiptItem>(bill, store ?? 0, BillStates.Reversed);
                    if (commonBillChecking.Data != null)
                        return commonBillChecking;

                    #endregion

                    //RedLock
                    var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill, store ?? 0, userId ?? 0),
                          TimeSpan.FromSeconds(30),
                          TimeSpan.FromSeconds(10),
                          TimeSpan.FromSeconds(1),
                          () => _cashReceiptBillService.Reverse(userId ?? 0, bill));
                    return result.To(result);
                }
                catch (Exception ex)
                {
                    //活动日志
                    _userActivityService.InsertActivity("Reverse", "单据红冲失败", userId ?? 0);
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
        [HttpGet("receiptcashbill/getinitdataasync/{store}/{userId}")]
        [SwaggerOperation("GetInitDataAsync")]
        //[AuthBaseFilter]
        public async Task<APIResult<CashReceiptBillModel>> GetInitDataAsync(int? store, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<CashReceiptBillModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var companySetting = _settingService.LoadSetting<CompanySetting>(store ?? 0);
                    var model = new CashReceiptBillModel
                    {
                        BillTypeEnumId = (int)BillTypeEnum.CashReceiptBill,
                    };

                    //默认收款
                    var defaultAcc = _accountingService.GetDefaultAccounting(store ?? 0, BillTypeEnum.CashReceiptBill);
                    model.CashReceiptBillAccountings.Add(new CashReceiptBillAccountingModel()
                    {
                        Name = defaultAcc?.Item1?.Name,
                        CollectionAmount = 0,
                        AccountingOptionId = defaultAcc?.Item1?.Id ?? 0,
                        AccountCodeTypeId = defaultAcc?.Item1?.AccountCodeTypeId ?? 0
                    });


                    //允许预收款支付成负数
                    model.AllowAdvancePaymentsNegative = companySetting.AllowAdvancePaymentsNegative;


                    return this.Successful("", model);
                }
                catch (Exception ex)
                {
                    return this.Error3<CashReceiptBillModel>(ex.Message);
                }
            });
        }


        /// <summary>
        /// 获取欠款单据
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="terminalId"></param>
        /// <param name="billTypeId"></param>
        /// <param name="billNumber"></param>
        /// <param name="remark"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("receiptcashbill/getowecashbills/{store}/{userId}")]
        [SwaggerOperation("GetOwecashBills")]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<BillSummaryModel>>> GetOwecashBills(int? store,
            int? userId,
            int? terminalId,
            int? billTypeId,
            string billNumber = "",
            string remark = "",
            DateTime? startTime = null,
            DateTime? endTime = null,
            int pageIndex = 0,
            int pageSize = 20)
        {

            if (!store.HasValue || store.Value == 0)
                return this.Error3<IList<BillSummaryModel>>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var billSummaries = new List<BillSummaryModel>();

                    var bills = _cashReceiptBillService.GetBillCashReceiptSummary(store ?? 0,
                        userId,
                        terminalId,
                        billTypeId,
                        billNumber,
                        remark,
                        startTime,
                        endTime,
                        pageIndex,
                        pageSize);

                    //重写计算： 优惠金额	 已收金额  尚欠金额
                    foreach (var bill in bills.Where(s => s.BillTypeId != (int)BillTypeEnum.FinancialIncomeBill))
                    {
                        //销售单
                        if (bill.BillTypeId == (int)BillTypeEnum.SaleBill)
                        {
                            //单据金额
                            decimal calc_billAmount = bill.Amount ?? 0;
                            if (bill.Remark != null && bill.Remark.IndexOf("应收款销售单") != -1)
                            {
                                calc_billAmount = bill.ArrearsAmount ?? 0;
                            }

                            //优惠金额 
                            decimal calc_discountAmount = 0;
                            //已收金额
                            decimal calc_paymentedAmount = 0;
                            //尚欠金额
                            decimal calc_arrearsAmount = 0;

                            #region 计算如下

                            //单已经收款部分的本次优惠合计
                            var discountAmountOnce = _commonBillService.GetBillDiscountAmountOnce(store ?? 0, bill.BillId);

                            //优惠金额 =  单据优惠金额  + （单已经收款部分的本次优惠合计）
                            calc_discountAmount = Convert.ToDecimal(Convert.ToDouble(bill.DiscountAmount ?? 0) + Convert.ToDouble(discountAmountOnce));

                            //单据收款金额（收款账户）
                            var collectionAmount = _commonBillService.GetBillCollectionAmount(store ?? 0, bill.BillId, BillTypeEnum.SaleBill);

                            //单已经收款部分的本次收款合计
                            var receivableAmountOnce = _commonBillService.GetBillReceivableAmountOnce(store ?? 0, bill.BillId);

                            //已收金额 = 单据收款金额（收款账户） + （单已经收款部分的本次收款合计）
                            calc_paymentedAmount = collectionAmount + receivableAmountOnce;

                            //尚欠金额
                            //Convert.ToDouble(bill.ArrearsAmount ?? 0) + 
                            calc_arrearsAmount = Convert.ToDecimal(Convert.ToDouble(calc_billAmount) - Convert.ToDouble(calc_discountAmount) - Convert.ToDouble(calc_paymentedAmount));

                            #endregion

                            //重新赋值
                            bill.Amount = calc_billAmount;
                            bill.DiscountAmount = calc_discountAmount;
                            bill.PaymentedAmount = calc_paymentedAmount;
                            bill.ArrearsAmount = calc_arrearsAmount;

                        }
                        //退货单
                        else if (bill.BillTypeId == (int)BillTypeEnum.ReturnBill)
                        {
                            //单据金额
                            decimal calc_billAmount = bill.Amount ?? 0;
                            //优惠金额 
                            decimal calc_discountAmount = 0;
                            //已收金额
                            decimal calc_paymentedAmount = 0;
                            //尚欠金额
                            decimal calc_arrearsAmount = 0;

                            #region 计算如下

                            //单已经收款部分的本次优惠合计
                            var discountAmountOnce = _commonBillService.GetBillDiscountAmountOnce(store ?? 0, bill.BillId);

                            //优惠金额 =  单据优惠金额  + （单已经收款部分的本次优惠合计）
                            calc_discountAmount = bill.DiscountAmount ?? 0 + discountAmountOnce;

                            //单据收款金额（收款账户）
                            var collectionAmount = _commonBillService.GetBillCollectionAmount(store ?? 0, bill.BillId, BillTypeEnum.ReturnBill);

                            //单已经收款部分的本次收款合计
                            var receivableAmountOnce = _commonBillService.GetBillReceivableAmountOnce(store ?? 0, bill.BillId);

                            //已收金额 = 单据收款金额（收款账户） + （单已经收款部分的本次收款合计）
                            calc_paymentedAmount = collectionAmount + receivableAmountOnce;

                            //尚欠金额
                            calc_arrearsAmount = Convert.ToDecimal(Convert.ToDouble(calc_billAmount) - Convert.ToDouble(calc_discountAmount) - Math.Abs(Convert.ToDouble(calc_paymentedAmount)));

                            #endregion

                            //重新赋值
                            bill.Amount = -calc_billAmount;
                            bill.DiscountAmount = -calc_discountAmount;
                            bill.PaymentedAmount = calc_paymentedAmount;
                            bill.ArrearsAmount = -calc_arrearsAmount;

                        }
                        //预收款单
                        else if (bill.BillTypeId == (int)BillTypeEnum.AdvanceReceiptBill)
                        {

                            //单据金额
                            decimal calc_billAmount = bill.Amount ?? 0;
                            //优惠金额 
                            decimal calc_discountAmount = 0;
                            //已收金额
                            decimal calc_paymentedAmount = 0;
                            //尚欠金额
                            decimal calc_arrearsAmount = 0;

                            #region 计算如下

                            //单已经收款部分的本次优惠合计
                            var discountAmountOnce = _commonBillService.GetBillDiscountAmountOnce(store ?? 0, bill.BillId);

                            //优惠金额 =  单据优惠金额  + （已经收款部分的本次优惠合计）
                            calc_discountAmount = bill.DiscountAmount ?? 0 + discountAmountOnce;

                            //单据收款金额（收款账户）
                            var collectionAmount = _commonBillService.GetBillCollectionAmount(store ?? 0, bill.BillId, BillTypeEnum.AdvanceReceiptBill);

                            //单已经收款部分的本次收款合计
                            var receivableAmountOnce = _commonBillService.GetBillReceivableAmountOnce(store ?? 0, bill.BillId);

                            //已收金额 = 单据收款金额（收款账户） + （已经收款部分的本次收款合计）
                            calc_paymentedAmount = collectionAmount + receivableAmountOnce;

                            calc_arrearsAmount = Convert.ToDecimal(Convert.ToDouble(calc_billAmount) - Convert.ToDouble(calc_discountAmount) - Convert.ToDouble(calc_paymentedAmount));

                            #endregion

                            //重新赋值
                            bill.Amount = calc_billAmount;
                            bill.DiscountAmount = calc_discountAmount;
                            bill.PaymentedAmount = calc_paymentedAmount;
                            bill.ArrearsAmount = calc_arrearsAmount;
                        }
                        //费用支出
                        else if (bill.BillTypeId == (int)BillTypeEnum.CostExpenditureBill)
                        {
                            //单据金额
                            decimal calc_billAmount = bill.Amount ?? 0;
                            //优惠金额 
                            decimal calc_discountAmount = 0;
                            //已收金额
                            decimal calc_paymentedAmount = 0;
                            //尚欠金额
                            decimal calc_arrearsAmount = 0;

                            #region 计算如下

                            //单已经收款部分的本次优惠合计
                            var discountAmountOnce = _commonBillService.GetBillDiscountAmountOnce(store ?? 0, bill.BillId);

                            //优惠金额 =  单据优惠金额  + （单已经收款部分的本次优惠合计）
                            calc_discountAmount = bill.DiscountAmount ?? 0 + discountAmountOnce;

                            //单据收款金额（收款账户）
                            var collectionAmount = _commonBillService.GetBillCollectionAmount(store ?? 0, bill.BillId, BillTypeEnum.CostExpenditureBill);

                            //单已经收款部分的本次收款合计
                            var receivableAmountOnce = _commonBillService.GetBillReceivableAmountOnce(store ?? 0, bill.BillId);

                            //已收金额 = 单据收款金额（收款账户） + （单已经收款部分的本次收款合计）
                            calc_paymentedAmount = collectionAmount + receivableAmountOnce;

                            //尚欠金额 
                            calc_arrearsAmount = Convert.ToDecimal(Convert.ToDouble(calc_billAmount) - Convert.ToDouble(calc_discountAmount) - Math.Abs(Convert.ToDouble(calc_paymentedAmount)));

                            #endregion

                            //重新赋值
                            bill.Amount = -Math.Abs(calc_billAmount);
                            bill.DiscountAmount = -Math.Abs(calc_discountAmount);
                            bill.PaymentedAmount = -Math.Abs(calc_paymentedAmount);
                            bill.ArrearsAmount = -Math.Abs(calc_arrearsAmount);
                        }
                        //其它收入
                        else if (bill.BillTypeId == (int)BillTypeEnum.FinancialIncomeBill)
                        {
                            //单据金额
                            decimal calc_billAmount = bill.Amount ?? 0;
                            //优惠金额 
                            decimal calc_discountAmount = 0;
                            //已收金额
                            decimal calc_paymentedAmount = 0;
                            //尚欠金额
                            decimal calc_arrearsAmount = 0;

                            #region 计算如下

                            //单已经收款部分的本次优惠合计
                            var discountAmountOnce = _commonBillService.GetBillDiscountAmountOnce(store ?? 0, bill.BillId);

                            //优惠金额 =  单据优惠金额  + （单已经收款部分的本次优惠合计）
                            calc_discountAmount = bill.DiscountAmount ?? 0 + discountAmountOnce;

                            //单据收款金额（收款账户）
                            var collectionAmount = _commonBillService.GetBillCollectionAmount(store ?? 0, bill.BillId, BillTypeEnum.FinancialIncomeBill);

                            //单已经收款部分的本次收款合计
                            var receivableAmountOnce = _commonBillService.GetBillReceivableAmountOnce(store ?? 0, bill.BillId);

                            //已收金额 = 单据收款金额（收款账户） + （单已经收款部分的本次收款合计）
                            calc_paymentedAmount = collectionAmount + receivableAmountOnce;

                            //尚欠金额 
                            calc_arrearsAmount = Convert.ToDecimal(Convert.ToDouble(calc_billAmount) - Convert.ToDouble(calc_discountAmount) - Convert.ToDouble(calc_paymentedAmount));

                            #endregion

                            //重新赋值
                            bill.Amount = calc_billAmount;
                            bill.DiscountAmount = calc_discountAmount;
                            bill.PaymentedAmount = calc_paymentedAmount;
                            bill.ArrearsAmount = calc_arrearsAmount;

                        }
                    }

                    billSummaries = bills.Where(s => Math.Abs(s.ArrearsAmount ?? 0) > 0).Select(s =>
                    {
                        var bill = s.ToModel<BillSummaryModel>();

                        bill.CustomerName = _terminalService.GetTerminalName(store ?? 0, s.CustomerId);
                        bill.BillLink = _billConvertService.GenerateBillUrl(bill.BillTypeId, bill.BillId);

                        return bill;
                    }).ToList();

                    return this.Successful("", billSummaries);
                }
                catch (Exception ex)
                {
                    return this.Error3<IList<BillSummaryModel>>(ex.Message);
                }

            });
        }

        /// <summary>
        /// 获取欠款单据
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="terminalId"></param>
        /// <param name="billTypeId"></param>
        /// <param name="billNumber"></param>
        /// <param name="remark"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("receiptcashbill/getowecashbillslist/{store}/{userId}")]
        [SwaggerOperation("GetOwecashBillsList")]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<BillSummaryModel>>> GetOwecashBillsList(int? store,
            int? userId,
            int? terminalId,
            int? billTypeId,
            string billNumber = "",
            string remark = "",
            DateTime? startTime = null,
            DateTime? endTime = null,
            int pageIndex = 0,
            int pageSize = 20)
        {

            if (!store.HasValue || store.Value == 0)
                return this.Error3<IList<BillSummaryModel>>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var billSummaries = new List<BillSummaryModel>();
                    var userIdList = new List<int>();
                    var user = _userService.GetUserById(userId??0);
                    //判断用户是否是系统管理员
                    if (!user.IsSystemAccount) 
                    {
                        userIdList.Add(user.Id);
                        var lst = _userService.GetAllSubordinateByUserId(user.Id, store??0);
                        userIdList.AddRange(lst);
                    }
                    var bills = _cashReceiptBillService.GetBillCashReceiptList(store ?? 0,
                        userIdList,
                        terminalId,
                        billTypeId,
                        billNumber,
                        remark,
                        startTime,
                        endTime,
                        pageIndex,
                        pageSize);

                    //重写计算： 优惠金额	 已收金额  尚欠金额
                    foreach (var bill in bills.Where(s => s.BillTypeId != (int)BillTypeEnum.FinancialIncomeBill))
                    {
                        //销售单
                        if (bill.BillTypeId == (int)BillTypeEnum.SaleBill)
                        {
                            //单据金额
                            decimal calc_billAmount = bill.Amount ?? 0;
                            if (bill.Remark != null && bill.Remark.IndexOf("应收款销售单") != -1)
                            {
                                calc_billAmount = bill.ArrearsAmount ?? 0;
                            }

                            //优惠金额 
                            decimal calc_discountAmount = 0;
                            //已收金额
                            decimal calc_paymentedAmount = 0;
                            //尚欠金额
                            decimal calc_arrearsAmount = 0;

                            #region 计算如下

                            //单已经收款部分的本次优惠合计
                            var discountAmountOnce = _commonBillService.GetBillDiscountAmountOnce(store ?? 0, bill.BillId);

                            //优惠金额 =  单据优惠金额  + （单已经收款部分的本次优惠合计）
                            calc_discountAmount = Convert.ToDecimal(Convert.ToDouble(bill.DiscountAmount ?? 0) + Convert.ToDouble(discountAmountOnce));

                            //单据收款金额（收款账户）
                            var collectionAmount = _commonBillService.GetBillCollectionAmount(store ?? 0, bill.BillId, BillTypeEnum.SaleBill);

                            //单已经收款部分的本次收款合计
                            var receivableAmountOnce = _commonBillService.GetBillReceivableAmountOnce(store ?? 0, bill.BillId);

                            //已收金额 = 单据收款金额（收款账户） + （单已经收款部分的本次收款合计）
                            calc_paymentedAmount = collectionAmount + receivableAmountOnce;

                            //尚欠金额
                            //Convert.ToDouble(bill.ArrearsAmount ?? 0) + 
                            calc_arrearsAmount = Convert.ToDecimal(Convert.ToDouble(calc_billAmount) - Convert.ToDouble(calc_discountAmount) - Convert.ToDouble(calc_paymentedAmount));

                            #endregion

                            //重新赋值
                            bill.Amount = calc_billAmount;
                            bill.DiscountAmount = calc_discountAmount;
                            bill.PaymentedAmount = calc_paymentedAmount;
                            bill.ArrearsAmount = calc_arrearsAmount;

                        }
                        //退货单
                        else if (bill.BillTypeId == (int)BillTypeEnum.ReturnBill)
                        {
                            //单据金额
                            decimal calc_billAmount = bill.Amount ?? 0;
                            //优惠金额 
                            decimal calc_discountAmount = 0;
                            //已收金额
                            decimal calc_paymentedAmount = 0;
                            //尚欠金额
                            decimal calc_arrearsAmount = 0;

                            #region 计算如下

                            //单已经收款部分的本次优惠合计
                            var discountAmountOnce = _commonBillService.GetBillDiscountAmountOnce(store ?? 0, bill.BillId);

                            //优惠金额 =  单据优惠金额  + （单已经收款部分的本次优惠合计）
                            calc_discountAmount = bill.DiscountAmount ?? 0 + discountAmountOnce;

                            //单据收款金额（收款账户）
                            var collectionAmount = _commonBillService.GetBillCollectionAmount(store ?? 0, bill.BillId, BillTypeEnum.ReturnBill);

                            //单已经收款部分的本次收款合计
                            var receivableAmountOnce = _commonBillService.GetBillReceivableAmountOnce(store ?? 0, bill.BillId);

                            //已收金额 = 单据收款金额（收款账户） + （单已经收款部分的本次收款合计）
                            calc_paymentedAmount = collectionAmount + receivableAmountOnce;

                            //尚欠金额
                            calc_arrearsAmount = Convert.ToDecimal(Convert.ToDouble(calc_billAmount) - Convert.ToDouble(calc_discountAmount) - Math.Abs(Convert.ToDouble(calc_paymentedAmount)));

                            #endregion

                            //重新赋值
                            bill.Amount = -calc_billAmount;
                            bill.DiscountAmount = -calc_discountAmount;
                            bill.PaymentedAmount = calc_paymentedAmount;
                            bill.ArrearsAmount = -calc_arrearsAmount;

                        }
                        //预收款单
                        else if (bill.BillTypeId == (int)BillTypeEnum.AdvanceReceiptBill)
                        {

                            //单据金额
                            decimal calc_billAmount = bill.Amount ?? 0;
                            //优惠金额 
                            decimal calc_discountAmount = 0;
                            //已收金额
                            decimal calc_paymentedAmount = 0;
                            //尚欠金额
                            decimal calc_arrearsAmount = 0;

                            #region 计算如下

                            //单已经收款部分的本次优惠合计
                            var discountAmountOnce = _commonBillService.GetBillDiscountAmountOnce(store ?? 0, bill.BillId);

                            //优惠金额 =  单据优惠金额  + （已经收款部分的本次优惠合计）
                            calc_discountAmount = bill.DiscountAmount ?? 0 + discountAmountOnce;

                            //单据收款金额（收款账户）
                            var collectionAmount = _commonBillService.GetBillCollectionAmount(store ?? 0, bill.BillId, BillTypeEnum.AdvanceReceiptBill);

                            //单已经收款部分的本次收款合计
                            var receivableAmountOnce = _commonBillService.GetBillReceivableAmountOnce(store ?? 0, bill.BillId);

                            //已收金额 = 单据收款金额（收款账户） + （已经收款部分的本次收款合计）
                            calc_paymentedAmount = collectionAmount + receivableAmountOnce;

                            calc_arrearsAmount = Convert.ToDecimal(Convert.ToDouble(calc_billAmount) - Convert.ToDouble(calc_discountAmount) - Convert.ToDouble(calc_paymentedAmount));

                            #endregion

                            //重新赋值
                            bill.Amount = calc_billAmount;
                            bill.DiscountAmount = calc_discountAmount;
                            bill.PaymentedAmount = calc_paymentedAmount;
                            bill.ArrearsAmount = calc_arrearsAmount;
                        }
                        //费用支出
                        else if (bill.BillTypeId == (int)BillTypeEnum.CostExpenditureBill)
                        {
                            //单据金额
                            decimal calc_billAmount = bill.Amount ?? 0;
                            //优惠金额 
                            decimal calc_discountAmount = 0;
                            //已收金额
                            decimal calc_paymentedAmount = 0;
                            //尚欠金额
                            decimal calc_arrearsAmount = 0;

                            #region 计算如下

                            //单已经收款部分的本次优惠合计
                            var discountAmountOnce = _commonBillService.GetBillDiscountAmountOnce(store ?? 0, bill.BillId);

                            //优惠金额 =  单据优惠金额  + （单已经收款部分的本次优惠合计）
                            calc_discountAmount = bill.DiscountAmount ?? 0 + discountAmountOnce;

                            //单据收款金额（收款账户）
                            var collectionAmount = _commonBillService.GetBillCollectionAmount(store ?? 0, bill.BillId, BillTypeEnum.CostExpenditureBill);

                            //单已经收款部分的本次收款合计
                            var receivableAmountOnce = _commonBillService.GetBillReceivableAmountOnce(store ?? 0, bill.BillId);

                            //已收金额 = 单据收款金额（收款账户） + （单已经收款部分的本次收款合计）
                            calc_paymentedAmount = collectionAmount + receivableAmountOnce;

                            //尚欠金额 
                            calc_arrearsAmount = Convert.ToDecimal(Convert.ToDouble(calc_billAmount) - Convert.ToDouble(calc_discountAmount) - Math.Abs(Convert.ToDouble(calc_paymentedAmount)));

                            #endregion

                            //重新赋值
                            bill.Amount = -Math.Abs(calc_billAmount);
                            bill.DiscountAmount = -Math.Abs(calc_discountAmount);
                            bill.PaymentedAmount = -Math.Abs(calc_paymentedAmount);
                            bill.ArrearsAmount = -Math.Abs(calc_arrearsAmount);
                        }
                        //其它收入
                        else if (bill.BillTypeId == (int)BillTypeEnum.FinancialIncomeBill)
                        {
                            //单据金额
                            decimal calc_billAmount = bill.Amount ?? 0;
                            //优惠金额 
                            decimal calc_discountAmount = 0;
                            //已收金额
                            decimal calc_paymentedAmount = 0;
                            //尚欠金额
                            decimal calc_arrearsAmount = 0;

                            #region 计算如下

                            //单已经收款部分的本次优惠合计
                            var discountAmountOnce = _commonBillService.GetBillDiscountAmountOnce(store ?? 0, bill.BillId);

                            //优惠金额 =  单据优惠金额  + （单已经收款部分的本次优惠合计）
                            calc_discountAmount = bill.DiscountAmount ?? 0 + discountAmountOnce;

                            //单据收款金额（收款账户）
                            var collectionAmount = _commonBillService.GetBillCollectionAmount(store ?? 0, bill.BillId, BillTypeEnum.FinancialIncomeBill);

                            //单已经收款部分的本次收款合计
                            var receivableAmountOnce = _commonBillService.GetBillReceivableAmountOnce(store ?? 0, bill.BillId);

                            //已收金额 = 单据收款金额（收款账户） + （单已经收款部分的本次收款合计）
                            calc_paymentedAmount = collectionAmount + receivableAmountOnce;

                            //尚欠金额 
                            calc_arrearsAmount = Convert.ToDecimal(Convert.ToDouble(calc_billAmount) - Convert.ToDouble(calc_discountAmount) - Convert.ToDouble(calc_paymentedAmount));

                            #endregion

                            //重新赋值
                            bill.Amount = calc_billAmount;
                            bill.DiscountAmount = calc_discountAmount;
                            bill.PaymentedAmount = calc_paymentedAmount;
                            bill.ArrearsAmount = calc_arrearsAmount;

                        }
                    }

                    billSummaries = bills.Where(s => Math.Abs(s.ArrearsAmount ?? 0) > 0).Select(s =>
                    {
                        var bill = s.ToModel<BillSummaryModel>();

                        bill.CustomerName = _terminalService.GetTerminalName(store ?? 0, s.CustomerId);
                        bill.BillLink = _billConvertService.GenerateBillUrl(bill.BillTypeId, bill.BillId);

                        return bill;
                    }).ToList();

                    return this.Successful("", billSummaries);
                }
                catch (Exception ex)
                {
                    return this.Error3<IList<BillSummaryModel>>(ex.Message);
                }

            });
        }
    }
}