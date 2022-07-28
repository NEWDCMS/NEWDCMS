using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Finances;
using DCMS.Services.Finances;
using DCMS.Services.Logging;
using DCMS.Services.Products;
using DCMS.Services.Settings;
using DCMS.Services.Users;
using DCMS.ViewModel.Models.Finances;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    /// 用于预付款管理
    /// </summary>
   [Authorize]
    [Route("api/v{version:apiVersion}/dcms/finances")]
    public class AdvancePaymentController : BaseAPIController
    {
        private readonly IUserService _userService;
        private readonly IAccountingService _accountingService;
        private readonly IUserActivityService _userActivityService;

        private readonly IAdvancePaymentBillService _advancePaymentBillService;
        
        private readonly IRedLocker _locker;
        private readonly IManufacturerService _manufacturerService;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="accountingService"></param>
        /// <param name="userActivityService"></param>
        /// <param name="advancePaymentBillService"></param>
        /// <param name="manufacturerService"></param>
        /// <param name="userService"></param>
        /// <param name="locker"></param>
        /// <param name="logger"></param>
        public AdvancePaymentController(
            IAccountingService accountingService,
            IUserActivityService userActivityService,
            IAdvancePaymentBillService advancePaymentBillService,
            IManufacturerService manufacturerService,
            IUserService userService,
            IRedLocker locker, 
            ILogger<BaseAPIController> logger) : base(logger)
        {
            _accountingService = accountingService;
            _userService = userService;
            _userActivityService = userActivityService;
            _advancePaymentBillService = advancePaymentBillService;
            
            _manufacturerService = manufacturerService;
            _locker = locker;
        }

        /// <summary>
        /// 获取预付款单
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
        /// <param name="accountingOptionId"></param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet("advancepayment/getbills/{store}/{manufacturerId}/{draweer}")]
        [SwaggerOperation("getbills")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<AdvancePaymentBillModel>>> AsyncList(int? store, int? makeuserId, int? manufacturerId, int? draweer, string billNumber = "", bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? showReverse = null, bool? sortByAuditedTime = null, int? accountingOptionId = -1, int pagenumber = 0, int pageSize = 50)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<AdvancePaymentBillModel>(Resources.ParameterError);
            return await Task.Run(() =>
            {
                try
                {
                    //默认收款账户动态列
                    var defaultAcc = _accountingService.GetDefaultAccounting(store ?? 0, BillTypeEnum.AdvancePaymentBill);

                    //获取分页
                    var bills = _advancePaymentBillService.GetAllAdvancePaymentBills(
                        store ?? 0,
                        makeuserId,
                        draweer,
                        manufacturerId,
                        billNumber,
                        auditedStatus,
                        startTime,
                        endTime,
                        showReverse,
                        sortByAuditedTime,
                        accountingOptionId,
                        pagenumber,
                        30);

                    #region 查询需要关联其他表的数据

                    var allUsers = _userService.GetUsersDictsByIds(store ?? 0, bills.Select(b => b.Draweer).Distinct().ToArray());
                    var allManufacturer = _manufacturerService.GetManufacturerDictsByIds(store ?? 0, bills.Select(b => b.ManufacturerId).Distinct().ToArray());

                    List<int> accountingOptionIds = bills.Select(b => b.AccountingOptionId ?? 0).ToList();
                    var allAccountingOptions = _accountingService.GetAccountingOptionsByIds(store, accountingOptionIds.ToArray());
                    #endregion

                    var results = bills.OrderByDescending(b => b.CreatedOnUtc).Select(b =>
                    {
                        var m = b.ToModel<AdvancePaymentBillModel>();

                        //付款人
                        m.DraweerName = allUsers.Where(aw => aw.Key == m.Draweer).Select(aw => aw.Value).FirstOrDefault();

                        //供应商
                        m.ManufacturerName = allManufacturer.Where(aw => aw.Key == m.ManufacturerId).Select(aw => aw.Value).FirstOrDefault();

                        var accountingOption = allAccountingOptions.Where(al => al.Id == m.AccountingOptionId).FirstOrDefault();
                        m.AccountingOptionName = accountingOption == null ? "" : accountingOption.Name;

                        //付款账户
                        m.Items = defaultAcc?.Item4?.OrderBy(s => s.Key).Select(s =>
                        {
                            var acc = b.Items.Where(a => a?.AccountingOption?.ParentId == s.Key).FirstOrDefault();
                            return new AdvancePaymentBillAccountingModel()
                            {
                                Name = acc?.AccountingOption?.Name,
                                AccountingOptionId = acc?.AccountingOptionId ?? 0,
                                CollectionAmount = acc?.CollectionAmount ?? 0
                            };
                        }).ToList();

                        return m;
                    }).ToList();

                    return this.Successful("", results);
                }
                catch (Exception ex)
                {
                    return this.Error<AdvancePaymentBillModel>(ex.Message);
                }

            });
        }

        /// <summary>
        /// 获取预付款单
        /// </summary>
        /// <param name="store"></param>
        /// <param name="billId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpGet("advancepaymentbill/getAdvancePaymentBill/{store}/{billId}/{user}")]
        [SwaggerOperation("getAdvancePaymentBill")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<AdvancePaymentBillModel>> GetAdvancePaymentBill(int? store, int? billId, int user = 0)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<AdvancePaymentBillModel>(false, Resources.ParameterError);
            return await Task.Run(() =>
            {
                var model = new AdvancePaymentBillModel();

                try
                {
                    var bill = _advancePaymentBillService.GetAdvancePaymentBillById(store, billId ?? 0, true);
                    if (bill != null)
                    {
                        model = bill.ToModel<AdvancePaymentBillModel>();

                        //付款科目
                        var defaultAcc = _accountingService.GetDefaultAccounting(store ?? 0, BillTypeEnum.AdvanceReceiptBill);
                        model.AccountingOptions = new SelectList(defaultAcc?.Item2.Select(a =>
                        {
                            return new SelectListItem()
                            {
                                Text = a.Name,
                                Value = a.Id.ToString()
                            };
                        }), "Value", "Text");
                        model.AccountingOptionId = bill.AccountingOptionId;

                        //付款账户
                        model.Items = bill.Items
                            .Where(s => s.AccountingOptionId != bill.AccountingOptionId)
                            .Select(r =>
                            {
                                return new AdvancePaymentBillAccountingModel()
                                {
                                    AccountingOptionId = r.AccountingOptionId,
                                    CollectionAmount = r.CollectionAmount,
                                    Name = defaultAcc?.Item3
                                    .Where(d => d.Id == r.AccountingOptionId)
                                    .Select(d => d.Name).FirstOrDefault()
                                };
                            }).ToList();

                        //经销商
                        model.ManufacturerName = _manufacturerService.GetManufacturerName(store, model.ManufacturerId);

                  
                        //付款人
                        model.Draweers = BindUserSelection(_userService.BindUserList, store, "");

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
                    return this.Error<AdvancePaymentBillModel>(false, ex.Message);
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
        [HttpPost("advancepayment/createorupdate/{store}/{userId}/{billId}")]
        [SwaggerOperation("createorupdate")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> CreateOrUpdate(AdvancePaymenUpdateModel data, int? billId, int? store, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(async () =>
            {
                try
                {
                    var bill = new AdvancePaymentBill();

                    #region 单据验证
                    if (data == null)
                        return this.Warning("请录入数据.");

                    if (PeriodLocked(store ?? 0, DateTime.Now))
                        return this.Warning("锁账期间,禁止业务操作.");

                    if (PeriodClosed(store ?? 0, DateTime.Now))
                        return this.Warning("会计期间已结账,禁止业务操作.");

                    if (billId.HasValue && billId.Value != 0)
                    {

                        bill = _advancePaymentBillService.GetAdvancePaymentBillById(store, billId.Value, true);

                        if (bill == null)
                            return this.Warning("单据信息不存在.");

                        if (bill.StoreId != store)
                            return this.Warning("非法操作.");

                        if (bill.AuditedStatus || bill.ReversedStatus)
                            return this.Warning("非法操作，单据已审核或已红冲.");

                        if (bill.Items == null || !bill.Items.Any())
                            return this.Warning("单据没有明细.");
                    }

                    #endregion


                    //业务逻辑
                    var accountings = _accountingService.GetAllAccountingOptions(store ?? 0, 0, true);
                    var dataTo = data.ToEntity<AdvancePaymenBillUpdate>();
                    dataTo.Operation = (int)OperationEnum.APP;
                    if (data.Accounting == null)
                    {
                        return this.Warning("没有默认的付款账号");
                    }
                    dataTo.Accounting = data.Accounting.Select(ac =>
                    {
                        return ac.ToEntity<AdvancePaymentBillAccounting>();
                    }).ToList();

                    //RedLock
                    var result = await _locker.PerformActionWithLockAsync(this.LockKey(data, store ?? 0, userId ?? 0),
                      TimeSpan.FromSeconds(30),
                      TimeSpan.FromSeconds(10),
                      TimeSpan.FromSeconds(1),
                      () => _advancePaymentBillService.BillCreateOrUpdate(store ?? 0, userId ?? 0, billId, bill, dataTo.Accounting, accountings, dataTo, _userService.IsAdmin(store ?? 0, userId ?? 0)));

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
        [HttpGet("advancepayment/auditing/{store}/{userId}/{billId}")]
        [SwaggerOperation("auditing")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> Auditing(int? billId, int? store, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(async () =>
            {
                try
                {
                    var bill = new AdvancePaymentBill();

                    if (this.PeriodClosed(store ?? 0, DateTime.Now))
                        return this.Warning("系统当月已经结转，不允许红冲");

                    if (!billId.HasValue)
                        return this.Warning("参数错误.");
                    else
                    { 
                        bill = _advancePaymentBillService.GetAdvancePaymentBillById(store ?? 0, billId.Value, true);
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
                        return this.Warning("非法操作，单据未审核或者重复操作.");

                    if (bill.Items == null || !bill.Items.Any())
                        return this.Warning("单据没有明细.");

                    //RedLock
                    var result = await _locker.PerformActionWithLockAsync(this.RedLockKey(bill, store ?? 0, userId ?? 0), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(1), () => _advancePaymentBillService.Auditing(store ?? 0, userId ?? 0, bill));

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
        [HttpGet("advancepayment/reverse/{store}/{userId}/{id}")]
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
                    var bill = new AdvancePaymentBill() { StoreId = store ?? 0 };

                    if (this.PeriodClosed(store ?? 0, DateTime.Now))
                        return this.Warning("系统当月已经结转，不允许红冲");

                    if (!id.HasValue)
                        return this.Warning("参数错误.");
                    else
                    {
                        bill = _advancePaymentBillService.GetAdvancePaymentBillById(store, id.Value, true);
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

                    if (bill.Items == null || !bill.Items.Any())
                        return this.Warning("单据没有明细.");

                    if (DateTime.Now.Subtract(bill.AuditedDate ?? DateTime.Now).TotalSeconds > 86400)
                        return this.Warning("已经审核的单据超过24小时，不允许红冲.");

                    //RedLock
                    var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill, store ?? 0, userId ?? 0),
                          TimeSpan.FromSeconds(30),
                          TimeSpan.FromSeconds(10),
                          TimeSpan.FromSeconds(1),
                          () => _advancePaymentBillService.Reverse(userId ?? 0, bill));

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
        [HttpGet("advancepayment/getinitdataasync/{store}/{userId}")]
        [SwaggerOperation("GetInitDataAsync")]
        //[AuthBaseFilter]
        public async Task<APIResult<AdvancePaymentBillModel>> GetInitDataAsync(int? store, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<AdvancePaymentBillModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var model = new AdvancePaymentBillModel();

                    //默认售价（方案）


                    //默认账户设置


                    return this.Successful("", model);
                }
                catch (Exception ex)
                {
                    return this.Error3<AdvancePaymentBillModel>(ex.Message);
                }
            });
        }
    }
}