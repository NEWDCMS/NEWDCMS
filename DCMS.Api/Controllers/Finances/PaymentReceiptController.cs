using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Finances;
using DCMS.Services.Finances;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Products;
using DCMS.Services.Settings;
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
    /// 用于付款单管理
    /// </summary>
    [Authorize]
    [Route("api/v{version:apiVersion}/dcms/finances")]
    public class PaymentReceiptController : BaseAPIController
    {
        private readonly IUserService _userService;
        private readonly IAccountingService _accountingService;
        private readonly IUserActivityService _userActivityService;
        private readonly IPaymentReceiptBillService _paymentReceiptBillService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IRedLocker _locker;

        //  private readonly static object _MyLock = new object();

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="accountingService"></param>
        /// <param name="userActivityService"></param>
        /// <param name="paymentReceiptBillService"></param>
        /// <param name="manufacturerService"></param>
        /// <param name="userService"></param>
        /// <param name="locker"></param>
        /// <param name="notificationService"></param>
        /// <param name="logger"></param>
        public PaymentReceiptController(
            IAccountingService accountingService,
            IUserActivityService userActivityService,
            IPaymentReceiptBillService paymentReceiptBillService,
            IManufacturerService manufacturerService,
            IUserService userService,
            IRedLocker locker,
            INotificationService notificationService,
           ILogger<BaseAPIController> logger) : base(logger)
        {
            _accountingService = accountingService;
            _userService = userService;
            _userActivityService = userActivityService;
            _paymentReceiptBillService = paymentReceiptBillService;
            _manufacturerService = manufacturerService;
            _locker = locker;
        }

        /// <summary>
        /// 获取付款单列表
        /// </summary>
        /// <param name="store"></param>
        /// <param name="manufacturerId"></param>
        /// <param name="draweer"></param>
        /// <param name="billNumber"></param>
        /// <param name="auditedStatus"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="showReverse"></param>
        /// <param name="sortByAuditedTime"></param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet("paymentreceipt/getbills/{store}/{manufacturerId}/{draweer}")]
        [SwaggerOperation("getbills")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<PaymentReceiptBillModel>>> AsyncList(int? store, int? makeuserId, int? manufacturerId, int? draweer, string billNumber, bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? showReverse = null, bool? sortByAuditedTime = null, int pagenumber = 0, int pageSize = 50)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<PaymentReceiptBillModel>(Resources.ParameterError);
            return await Task.Run(() =>
            {
                var model = new PaymentReceiptBillModel();
                try
                {
                    //默认收款账户动态列
                    var defaultAcc = _accountingService.GetDefaultAccounting(store ?? 0, BillTypeEnum.PaymentReceiptBill);

                    //获取分页
                    var bills = _paymentReceiptBillService.GetAllPaymentReceiptBills(
                        store ?? 0,
                        makeuserId ?? 0,
                        draweer,
                        manufacturerId,
                        billNumber,
                        auditedStatus,
                        startTime,
                        endTime,
                        showReverse,
                        sortByAuditedTime,
                        pagenumber, 30);

                    #region 查询需要关联其他表的数据

                    var allUsers = _userService.GetUsersDictsByIds(store ?? 0, bills.Select(b => b.Draweer).Distinct().ToArray());

                    var allManufacturer = _manufacturerService.GetManufacturersByIds(store ?? 0, bills.Select(b => b.ManufacturerId).Distinct().ToArray());
                    #endregion

                    var result = bills.OrderByDescending(b => b.CreatedOnUtc).Select(b =>
                    {
                        var m = b.ToModel<PaymentReceiptBillModel>();

                        //付款人
                        m.DraweerName = allUsers.Where(aw => aw.Key == m.Draweer).Select(aw => aw.Value).FirstOrDefault();

                        //供应商
                        var manufacturer = allManufacturer.Where(am => am.Id == m.ManufacturerId).FirstOrDefault();
                        m.ManufacturerName = manufacturer == null ? "" : manufacturer.Name;

                        //付款账户
                        m.PaymentReceiptBillAccountings = defaultAcc?.Item4?.OrderBy(s => s.Key).Select(s =>
                        {
                            var acc = b.PaymentReceiptBillAccountings.Where(a => a?.AccountingOption?.ParentId == s.Key).FirstOrDefault();
                            return new PaymentReceiptBillAccountingModel()
                            {
                                AccountingOptionId = acc?.AccountingOptionId ?? 0,
                                CollectionAmount = acc?.CollectionAmount ?? 0
                            };
                        }).ToList();

                        //总优惠金额(本次优惠金额总和)
                        m.DiscountAmount = b.Items.Sum(sb => sb.DiscountAmountOnce);

                        //付款前欠款金额
                        m.BeforArrearsAmount = b.Items.Sum(sb => sb.ArrearsAmount);

                        //剩余金额(付款后尚欠金额总和)
                        m.AmountOwedAfterReceipt = b.Items.Sum(sb => sb.AmountOwedAfterReceipt);

                        return m;
                    }).ToList();
                    return this.Successful("", result);
                }
                catch (Exception ex)
                {
                    return this.Error<PaymentReceiptBillModel>(ex.Message);
                }
            });
        }

        /// <summary>
        /// 创建/编辑
        /// </summary>
        /// <param name="data"></param>
        /// <param name="billId"></param>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost("paymentreceipt/createorupdate/{store}/{userId}/{billId}")]
        [SwaggerOperation("createorupdate")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> CreateOrUpdate(PaymentReceiptUpdateModel data, int? billId, int? store, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(async () =>
            {
                try
                {
                    PaymentReceiptBill bill = new PaymentReceiptBill();

                    if (data == null || data.Items == null)
                        return this.Warning("请录入数据.");

                    if (PeriodLocked(DateTime.Now, store ?? 0))
                        return this.Warning("锁账期间,禁止业务操作.");

                    if (PeriodClosed(DateTime.Now, store ?? 0))
                        return this.Warning("会计期间已结账,禁止业务操作.");


                    #region 单据验证
                    if (billId.HasValue && billId.Value != 0)
                    {
                        bill = _paymentReceiptBillService.GetPaymentReceiptBillById(store ?? 0, billId.Value, true);

                        //公共单据验证
                        var commonBillChecking = BillChecking<PaymentReceiptBill, PaymentReceiptItem>(bill, store ?? 0, BillStates.Draft);
                        if (commonBillChecking.Data != null)
                            return commonBillChecking;

                    }

                    //判断指定单据是否尚有欠款(是否已经收完款)
                    var isDebt = true;
                    data.Items.ForEach(s =>
                    {
                        isDebt = _paymentReceiptBillService.ThereAnyDebt(store ?? 0, s.BillTypeId, s.BillId);
                        if (!isDebt)
                        {
                            return;
                        }
                    });
                    if (!isDebt)
                    {
                        return Warning("非法操作，单据存在已经结清款项.");
                    }

                    #endregion

                    #region 验证预付款
                    if (data.Accounting != null)
                    {
                        if (data.AdvanceAmount > 0 && data.AdvanceAmount > data.AdvanceAmountBalance)
                        {
                            return this.Warning("预付款余额不足!");
                        }
                    }
                    #endregion

                    //业务逻辑
                    var accountings = _accountingService.GetAllAccountingOptions(store ?? 0, 0, true);
                    var dataTo = data.ToEntity<PaymentReceiptBillUpdate>();
                    dataTo.Operation = (int)OperationEnum.APP;
                    if (data.Accounting == null)
                    {
                        return this.Warning("没有默认的付款账号");
                    }
                    dataTo.Accounting = data.Accounting.Select(ac =>
                    {
                        return ac.ToAccountEntity<PaymentReceiptBillAccounting>();
                    }).ToList();
                    dataTo.Items = data.Items.Select(it =>
                    {
                        return it.ToEntity<PaymentReceiptItem>();
                    }).ToList();

                    //RedLock
                    var result = await _locker.PerformActionWithLockAsync(this.LockKey(data, store ?? 0, userId ?? 0),
                          TimeSpan.FromSeconds(30),
                          TimeSpan.FromSeconds(10),
                          TimeSpan.FromSeconds(1),
                          () => _paymentReceiptBillService.BillCreateOrUpdate(store ?? 0, userId ?? 0, billId, bill, dataTo.Accounting, accountings, dataTo, dataTo.Items, _userService.IsAdmin(store ?? 0, userId ?? 0)));
                    return result.To(result);

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
        [HttpGet("paymentreceipt/auditing/{store}/{userId}/{id}")]
        [SwaggerOperation("auditing")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> Auditing(int? id, int? store, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(async () =>
            {
                try
                {
                    var bill = new PaymentReceiptBill();
                    var recordingVoucher = new RecordingVoucher();

                    #region 验证
                    if (!id.HasValue)
                        return this.Warning("参数错误.");
                    else
                    {
                        bill = _paymentReceiptBillService.GetPaymentReceiptBillById(store ?? 0, id.Value, true);
                        if (bill.AuditedStatus)
                        {
                            return Warning("单据已审核，请刷新页面.");
                        }
                    }

                    //公共单据验证
                    var commonBillChecking = BillChecking<PaymentReceiptBill, PaymentReceiptItem>(bill, store ?? 0, BillStates.Audited);
                    if (commonBillChecking.Data != null)
                        return commonBillChecking;

                    #endregion

                    //RedLock
                    var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill, store ?? 0, userId ?? 0),
                          TimeSpan.FromSeconds(30),
                          TimeSpan.FromSeconds(10),
                          TimeSpan.FromSeconds(1),
                          () => _paymentReceiptBillService.Auditing(store ?? 0, userId ?? 0, bill));
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
        [HttpGet("paymentreceipt/reverse/{store}/{userId}/{id}")]
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
                    var bill = new PaymentReceiptBill() { StoreId = store ?? 0 };

                    #region 验证
                    if (this.PeriodClosed(DateTime.Now, store ?? 0))
                        return this.Warning("系统当月已经结转，不允许红冲");
                    if (!id.HasValue)
                        return this.Warning("参数错误.");
                    else
                    {
                        bill = _paymentReceiptBillService.GetPaymentReceiptBillById(store ?? 0, id.Value, true);
                        bill.Remark = remark;
                        if (bill.ReversedStatus)
                        {
                            return Warning("单据已红冲，请刷新页面.");
                        }
                    }

                    //公共单据验证
                    var commonBillChecking = BillChecking<PaymentReceiptBill, PaymentReceiptItem>(bill, store ?? 0, BillStates.Reversed);
                    if (commonBillChecking.Data != null)
                        return commonBillChecking;

                    #endregion

                    //RedLock
                    var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill, store ?? 0, userId ?? 0),
                          TimeSpan.FromSeconds(30),
                          TimeSpan.FromSeconds(10),
                          TimeSpan.FromSeconds(1),
                          () => _paymentReceiptBillService.Reverse(userId ?? 0, bill));
                    return result.To(result);
                }
                catch (Exception ex)
                {
                    _userActivityService.InsertActivity("Reverse", "单据红冲失败", userId ?? 0);
                    return this.Error(ex.Message);
                }
            });
        }
    }
}