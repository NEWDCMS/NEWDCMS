using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Finances;
using DCMS.Services.Finances;
using DCMS.Services.Logging;
using DCMS.Services.Settings;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
using DCMS.ViewModel.Models.Finances;
using DCMS.Web.Framework.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
    /// 用于预收款管理
    /// </summary>
   [Authorize]
   [Route("api/v{version:apiVersion}/dcms/finances")]
    public class AdvanceReceiptController : BaseAPIController
    {
        private readonly ITerminalService _terminalService;
        private readonly IUserService _userService;
        private readonly IAccountingService _accountingService;
        private readonly IUserActivityService _userActivityService;
        private readonly IAdvanceReceiptBillService _advanceReceiptBillService;

        private readonly IRedLocker _locker;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="terminalService"></param>
        /// <param name="accountingService"></param>
        /// <param name="advanceReceiptBillService"></param>
        /// <param name="userService"></param>
        /// <param name="locker"></param>
        /// <param name="userActivityService"></param>
        /// <param name="logger"></param>
        public AdvanceReceiptController(
            ITerminalService terminalService,
            IAccountingService accountingService,
            IAdvanceReceiptBillService advanceReceiptBillService,
            IUserService userService,
            IRedLocker locker,
            IUserActivityService userActivityService,
            ILogger<BaseAPIController> logger) : base(logger)
        {
            _terminalService = terminalService;
            _accountingService = accountingService;
            _userService = userService;
            _advanceReceiptBillService = advanceReceiptBillService;
            _locker = locker;
            _userActivityService = userActivityService;
        }

        /// <summary>
        /// 获取预收款
        /// </summary>
        /// <param name="store"></param>
        /// <param name="customerId"></param>
        /// <param name="payeer"></param>
        /// <param name="billNumber"></param>
        /// <param name="auditedStatus"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="showReverse"></param>
        /// <param name="sortByAuditedTime"></param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet("advanceReceipt/getbills/{store}/{customerId}/{payeer}")]
        [SwaggerOperation("getbills")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<AdvanceReceiptBillModel>>> AsyncList(int? store, int? customerId, int? makeuserId, string customerName, int? payeer, string billNumber = "", bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? showReverse = null, bool? sortByAuditedTime = null, int? accountingOptionId = null, int pagenumber = 0, int pageSize = 50)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<AdvanceReceiptBillModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    //默认收款账户动态列
                    var defaultAcc = _accountingService.GetDefaultAccounting(store ?? 0, BillTypeEnum.AdvanceReceiptBill);

                    //获取分页
                    var bills = _advanceReceiptBillService.GetAllAdvanceReceiptBills(
                        store ?? 0,
                        makeuserId,
                        customerId,
                        customerName,
                        payeer,
                        billNumber,
                        auditedStatus,
                        startTime,
                        endTime,
                        showReverse,
                        sortByAuditedTime,
                        accountingOptionId,
                        null,
                        null,
                        pagenumber,
                        30);
                    #region 查询需要关联其他表的数据

                    var allUsers = _userService.GetUsersDictsByIds(store ?? 0, bills.Select(b => b.Payeer).Distinct().ToArray());
                    var allTerminals = _terminalService.GetTerminalsDictsByIds(store ?? 0, bills.Select(b => b.CustomerId).Distinct().ToArray());

                    List<int> accountingOptionIds = bills.Select(b => b.AccountingOptionId ?? 0).ToList();
                    List<int> accountingOptionIds2 = new List<int>();
                    if (bills != null && bills.Count > 0)
                    {
                        bills.ToList().ForEach(bill =>
                        {
                            if (bill != null && bill.Items != null && bill.Items.Count > 0)
                            {
                                accountingOptionIds2.AddRange(bill.Items.Select(ba => ba.AccountingOptionId));
                            }
                        });
                    }
                    accountingOptionIds.AddRange(accountingOptionIds2);
                    //去重
                    accountingOptionIds = accountingOptionIds.Distinct().ToList();
                    var allAccountingOptions = _accountingService.GetAccountingOptionsByIds(store ?? 0, accountingOptionIds.ToArray());

                    #endregion

                    var results = bills.OrderByDescending(b => b.CreatedOnUtc).Select(b =>
                   {
                       var m = b.ToModel<AdvanceReceiptBillModel>();

                       //收款人
                       m.PayeerName = allUsers.Where(u => u.Key == m.Payeer).Select(u => u.Value).FirstOrDefault();
                       //客户名称
                       m.CustomerName = allTerminals.Where(t => t.Key == m.CustomerId).Select(u => u.Value).FirstOrDefault();

                       var accountingOption1 = allAccountingOptions.Where(al => al.Id == m.AccountingOptionId).FirstOrDefault();
                       m.AccountingOptionName = accountingOption1 == null ? "" : accountingOption1.Name;

                       m.OutstandingPayment = m.OweCash;

                       //收款账户
                       m.Items = b.Items.Where(hh=>hh.Copy==false).Select(s =>
                      {
                          return new AdvanceReceiptBillAccountingModel()
                          {
                              Name = _accountingService.GetAccountingOptionName(store, s.AccountingOptionId), 
                              AccountingOptionName = _accountingService.GetAccountingOptionName(store, s.AccountingOptionId),
                              AccountingOptionId = s?.AccountingOptionId ?? 0,
                              CollectionAmount = s?.CollectionAmount ?? 0
                          };
                      }).ToList();

                       return m;
                   }).ToList();

                    return this.Successful("", results);
                }
                catch (Exception ex)
                {
                    return this.Error<AdvanceReceiptBillModel>(ex.Message);
                }
            });
        }

        /// <summary>
        /// 获取预收款
        /// </summary>
        /// <param name="store"></param>
        /// <param name="billId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpGet("advanceReceipt/getAdvanceReceiptBill/{store}/{billId}/{user}")]
        [SwaggerOperation("getAdvanceReceiptBill")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<AdvanceReceiptBillModel>> GetAdvanceReceiptBill(int? store, int? billId, int user = 0)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<AdvanceReceiptBillModel>(false, Resources.ParameterError);

            return await Task.Run(() =>
            {
                var model = new AdvanceReceiptBillModel();

                try
                {
                    var bill = _advanceReceiptBillService.GetAdvanceReceiptBillById(store, billId ?? 0, true);
                    if (bill != null)
                    {
                        model = bill.ToModel<AdvanceReceiptBillModel>();

                        //预收款账户AccountingOptions
                        var defaultAcc = _accountingService.GetDefaultAccounting(store ?? 0, BillTypeEnum.AdvanceReceiptBill);
                        model.AccountingOptionId = bill.AccountingOptionId;
                        model.AccountingOptionName = _accountingService.GetAccountingOptionName(store, model.AccountingOptionId??0);

                        //收款账户
                        model.Items = bill.Items
                            .Where(s => s.AccountingOptionId != bill.AccountingOptionId)
                            .Select(r =>
                            {
                                return new AdvanceReceiptBillAccountingModel()
                                {
                                    AccountingOptionId = r.AccountingOptionId,
                                    AccountingOptionName = defaultAcc?.Item3.Where(d => d.Id == r.AccountingOptionId)
                                    .Select(d => d.Name).FirstOrDefault(),
                                    CollectionAmount = r.CollectionAmount,
                                    Name = defaultAcc?.Item3
                                    .Where(d => d.Id == r.AccountingOptionId)
                                    .Select(d => d.Name).FirstOrDefault()
                                };
                            }).ToList();

                        //客户名称
                        model.CustomerName = _terminalService.GetTerminalName(store, bill.CustomerId);

                        //收款人
                        model.Payeers = BindUserSelection(_userService.BindUserList, store, "");

                        //制单人
                        var mu = _userService.GetUserName(store, bill.MakeUserId);

                        model.MakeUserName = mu + " " + bill.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss");

                        //审核人
                        var au = _userService.GetUserName(store, bill.AuditedUserId ?? 0);

                        model.AuditedUserName = au + " " + (bill.AuditedDate.HasValue ? bill.AuditedDate.Value.ToString("yyyy/MM/dd HH:mm:ss") : "");

                    }

                    return this.Successful("", model);
                }
                catch (Exception ex)
                {
                    return this.Error<AdvanceReceiptBillModel>(false, ex.Message);
                }

            });
        }

        /// <summary>
        /// 创建修改预收款
        /// </summary>
        /// <param name="data"></param>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <param name="billId"></param>
        /// <returns></returns>
        [HttpPost("advanceReceipt/CreateOrUpdate/{store}/{userId}/{billId}")]
        [SwaggerOperation("CreateOrUpdate")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> CreateOrUpdate(AdvanceReceiptUpdateModel data, int? store, int? userId, int? billId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(async () =>
            {
                try
                {

                    AdvanceReceiptBill bill = new AdvanceReceiptBill();
                    if (data == null)
                        return this.Warning("请录入数据.");

                    if (PeriodLocked(store ?? 0, DateTime.Now))
                        return this.Warning("锁账期间,禁止业务操作.");

                    if (PeriodClosed(store ?? 0, DateTime.Now))
                        return this.Warning("会计期间已结账,禁止业务操作.");

                    #region 单据验证
                    if (billId.HasValue && billId.Value != 0)
                    {
                        bill = _advanceReceiptBillService.GetAdvanceReceiptBillById(store ?? 0, billId.Value, false);

                        //单据不存在
                        if (bill == null)
                        {
                            return this.Warning("单据信息不存在.");
                        }

                        //单开经销商不等于当前经销商
                        if (bill.StoreId != store)
                        {
                            return this.Warning("非法操作.");
                        }

                        //单据已经审核或已经红冲
                        if (bill.AuditedStatus || bill.ReversedStatus)
                        {
                            return this.Warning("非法操作，单据已审核或已红冲.");
                        }
                    }

                    #endregion

                    //业务逻辑
                    var accountings = _accountingService.GetAllAccountingOptions(store ?? 0, 0, true);
                    var dataTo = data.ToEntity<AdvanceReceiptBillUpdate>();
                    dataTo.Operation = (int)OperationEnum.APP;
                    if (data.Accounting == null)
                    {
                        return this.Warning("没有默认的付款账号");
                    }
                    dataTo.Accounting = data.Accounting.Select(ac =>
                    {
                        return ac.ToAccountEntity<AdvanceReceiptBillAccounting>();
                    }).ToList();

                    //RedLock);
                    var result = await _locker.PerformActionWithLockAsync(this.LockKey(data, store ?? 0, userId ?? 0),
                       TimeSpan.FromSeconds(30),
                       TimeSpan.FromSeconds(10),
                       TimeSpan.FromSeconds(1),
                       () => _advanceReceiptBillService.BillCreateOrUpdate(store ?? 0, userId ?? 0, billId, bill
                       , dataTo.Accounting, accountings, dataTo, _userService.IsAdmin(store ?? 0, userId ?? 0)));

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
        /// <param name="billId"></param>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("advanceReceipt/auditing/{store}/{userId}/{billId}")]
        [SwaggerOperation("auditing")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> Auditing(int? store, int? userId, int? billId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(async () =>
            {
                try
                {
                    AdvanceReceiptBill bill = new AdvanceReceiptBill();
                    #region 验证
                    if (!billId.HasValue)
                        return this.Warning("参数错误.");
                    else
                    { 
                        bill = _advanceReceiptBillService.GetAdvanceReceiptBillById(store ?? 0, billId.Value, true);
                        if (bill.AuditedStatus)
                        {
                            return Warning("单据已审核，请刷新页面.");
                        }
                    }

                    if (bill == null)
                        return this.Warning("单据信息不存在.");

                    if (bill.StoreId != store)
                        return this.Warning("非法操作.");

                    if (bill.AuditedStatus || bill.ReversedStatus)
                        return this.Warning("重复操作.");

                    #endregion

                    //RedLock
                    string lockKey = string.Format(DCMSCachingDefaults.RedisDataReSubmitKey, Request.GetUrl(), store ?? 0, userId ?? 0, CommonHelper.MD5(JsonConvert.SerializeObject(billId)));
                    var result = await _locker.PerformActionWithLockAsync(lockKey,
                          TimeSpan.FromSeconds(30),
                          TimeSpan.FromSeconds(10),
                          TimeSpan.FromSeconds(1),
                          () => _advanceReceiptBillService.Auditing(store ?? 0, userId ?? 0, bill));
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
        [HttpGet("advanceReceipt/reverse/{store}/{userId}/{id}")]
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
                    var bill = new AdvanceReceiptBill() { StoreId = store ?? 0 };

                    if (this.PeriodClosed(store ?? 0, DateTime.Now))
                        return this.Warning("系统当月已经结转，不允许红冲");

                    if (!id.HasValue)
                        return this.Warning("参数错误.");
                    else
                    { 
                        bill = _advanceReceiptBillService.GetAdvanceReceiptBillById(store ?? 0, id.Value, true);
                        bill.Remark = remark;
                        if (bill.ReversedStatus)
                        {
                            return Warning("单据已红冲，请刷新页面.");
                        }
                    }

                    if (bill == null)
                        return this.Warning("单据信息不存在.");

                    if (bill.StoreId != store)
                        return this.Warning("非法操作.");

                    if (!bill.AuditedStatus || bill.ReversedStatus)
                        return this.Warning("非法操作，单据未审核或者重复操作.");

                    if (bill.Deleted)
                        return this.Warning("单据已作废.");

                    if (bill.Items == null || !bill.Items.Any())
                        return this.Warning("单据没有明细.");

                    if (DateTime.Now.Subtract(bill.AuditedDate ?? DateTime.Now).TotalSeconds > 86400)
                        return this.Warning("已经审核的单据超过24小时，不允许红冲.");
                    //RedLock

                    var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill, store ?? 0, userId ?? 0),
                          TimeSpan.FromSeconds(30),
                          TimeSpan.FromSeconds(10),
                          TimeSpan.FromSeconds(1),
                          () => _advanceReceiptBillService.Reverse(userId ?? 0, bill));
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
        [HttpGet("advanceReceipt/getinitdataasync/{store}/{userId}")]
        [SwaggerOperation("GetInitDataAsync")]
        //[AuthBaseFilter]
        public async Task<APIResult<AdvanceReceiptBillModel>> GetInitDataAsync(int? store, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<AdvanceReceiptBillModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var model = new AdvanceReceiptBillModel();

                    //默认收款
                    var defaultAcc = _accountingService.GetDefaultAccounting(store ?? 0, BillTypeEnum.AdvanceReceiptBill);
                    model.Items.Add(new AdvanceReceiptBillAccountingModel()
                    {
                        Name = defaultAcc?.Item1?.Name,
                        CollectionAmount = 0,
                        AccountingOptionId = defaultAcc?.Item1?.Id ?? 0
                    });
                    return this.Successful("", model);
                }
                catch (Exception ex)
                {
                    return this.Error3<AdvanceReceiptBillModel>(ex.Message);
                }
            });
        }
    }
}