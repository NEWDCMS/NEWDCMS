using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Finances;
using DCMS.Services.Configuration;
using DCMS.Services.Finances;
using DCMS.Services.Logging;
using DCMS.Services.Products;
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
    /// 用于财务收入管理
    /// </summary>
    [Authorize]
    [Route("api/v{version:apiVersion}/dcms/finances")]
    public class FinancialIncomeController : BaseAPIController
    {
        private readonly ITerminalService _terminalService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IUserService _userService;
        private readonly IAccountingService _accountingService;
        private readonly IUserActivityService _userActivityService;
        private readonly IRedLocker _locker;
        private readonly IFinancialIncomeBillService _financialIncomeBillService;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="terminalService"></param>
        /// <param name="manufacturerService"></param>
        /// <param name="userService"></param>
        /// <param name="accountingService"></param>
        /// <param name="userActivityService"></param>
        /// <param name="settingService"></param>
        /// <param name="financialIncomeBillService"></param>
        /// <param name="locker"></param>
        /// <param name="logger"></param>
        public FinancialIncomeController(
            ITerminalService terminalService,
            IManufacturerService manufacturerService,
            IUserService userService,
            IAccountingService accountingService,
            IUserActivityService userActivityService,
            ISettingService settingService,
            IFinancialIncomeBillService financialIncomeBillService,
            IRedLocker locker,
            ILogger<BaseAPIController> logger) : base(logger)
        {
            _terminalService = terminalService;
            _manufacturerService = manufacturerService;
            _userService = userService;
            _accountingService = accountingService;
            _userActivityService = userActivityService;
            _financialIncomeBillService = financialIncomeBillService;
            _locker = locker;
        }

        /// <summary>
        /// 获取财务收入单据
        /// </summary>
        /// <param name="store"></param>
        /// <param name="salesmanId"></param>
        /// <param name="customerId"></param>
        /// <param name="customerName"></param>
        /// <param name="manufacturerId"></param>
        /// <param name="manufacturerName"></param>
        /// <param name="billNumber"></param>
        /// <param name="auditedStatus"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="showReverse"></param>
        /// <param name="sortByAuditedTime"></param>
        /// <param name="remark"></param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet("financialincome/getbills/{store}/{salesmanId}/{customerId}")]
        [SwaggerOperation("getbills")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<FinancialIncomeBillModel>>> AsyncList(int? store, int? makeuserId, int? salesmanId, int? customerId, string customerName, int? manufacturerId, string manufacturerName, string billNumber, string remark, bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? showReverse = null, bool? sortByAuditedTime = null, int pagenumber = 0, int pageSize = 50)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<FinancialIncomeBillModel>(Resources.ParameterError);
            return await Task.Run(() =>
            {
                var model = new List<FinancialIncomeBillModel>();

                try
                {
                    //默认收款账户动态列
                    var defaultAcc = _accountingService.GetDefaultAccounting(store ?? 0, BillTypeEnum.FinancialIncomeBill);
                    //获取分页
                    var bills = _financialIncomeBillService.GetAllFinancialIncomeBills(
                       store ?? 0,
                       makeuserId ?? 0,
                       salesmanId,
                       customerId,
                       manufacturerId,
                       billNumber,
                       auditedStatus,
                       startTime,
                       endTime,
                       showReverse,
                       sortByAuditedTime,
                       remark,
                       pagenumber, 30);
                    #region 查询需要关联其他表的数据

                    var allUsers = _userService.GetUsersDictsByIds(store ?? 0, bills.Select(b => b.SalesmanId).Distinct().ToArray());

                    List<int> accountingOptionIds = new List<int>();
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
                    var allAccountingOptions = _accountingService.GetAccountingOptionsByIds(store ?? 0, accountingOptionIds.Distinct().ToArray());

                    List<int> terminalIds = new List<int>();
                    if (bills != null && bills.Count > 0)
                    {
                        bills.ToList().ForEach(bill =>
                        {
                            if (bill != null && bill.Items != null && bill.Items.Count > 0)
                            {
                                terminalIds.AddRange(bill.Items.Where(fi => fi.CustomerOrManufacturerType == (int)TerminalDataType.Terminal).Select(ba => ba.CustomerOrManufacturerId));
                            }
                        });
                    }
                    var allTerminals = _terminalService.GetTerminalsByIds(store ?? 0, terminalIds.Distinct().ToArray());

                    var manufacturerIds = new List<int>();
                    if (bills != null && bills.Count > 0)
                    {
                        bills.ToList().ForEach(bill =>
                        {
                            if (bill != null && bill.Items != null && bill.Items.Count > 0)
                            {
                                manufacturerIds.AddRange(bill.Items.Where(fi => fi.CustomerOrManufacturerType == (int)TerminalDataType.Manufacturer).Select(ba => ba.CustomerOrManufacturerId));
                            }
                        });
                    }
                    var allManufacturers = _manufacturerService.GetManufacturersByIds(store ?? 0, manufacturerIds.Distinct().ToArray());
                    #endregion

                    var result = bills.OrderByDescending(b => b.CreatedOnUtc).Select(b =>
                    {
                        var m = b.ToModel<FinancialIncomeBillModel>();

                        //业务员
                        m.SalesmanName = allUsers?.Where(aw => aw.Key == m.SalesmanId).Select(aw => aw.Value).FirstOrDefault();

                        if (b.TerminalId > 0)
                            m.TerminalName = allTerminals?.Where(am => am.Id == b.TerminalId).FirstOrDefault()?.Name;

                        if (b.ManufacturerId > 0)
                            m.ManufacturerName = allManufacturers?.Where(am => am.Id == b.ManufacturerId).FirstOrDefault()?.Name;

                        m.FinancialIncomeBillAccountings = defaultAcc?.Item4?.OrderBy(s => s.Key).Select(s =>
                        {
                            var acc = b.FinancialIncomeBillAccountings.Where(a => a?.AccountingOption?.ParentId == s.Key).FirstOrDefault();
                            return new FinancialIncomeBillAccountingModel()
                            {
                                AccountingOptionId = acc?.AccountingOptionId ?? 0,
                                CollectionAmount = acc?.CollectionAmount ?? 0
                            };
                        }).ToList();

                        int first = b.Items.Select(s => s.AccountingOptionId).FirstOrDefault();
                        m.AccountingOptionName = allAccountingOptions.Where(al => al.Id == first).FirstOrDefault()?.Name;

                        m.Items = b.Items.Select(sb => sb.ToModel<FinancialIncomeItemModel>()).ToList();

                        return m;
                    }).ToList();

                    return this.Successful("", result);
                }
                catch (Exception ex)
                {
                    return this.Error<FinancialIncomeBillModel>(ex.Message);
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
        [HttpPost("financialincome/createorupdate/{store}/{userId}/{billId}")]
        [SwaggerOperation("createorupdate")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> CreateOrUpdate(FinancialIncomeUpdateModel data, int? billId, int? store, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(async () =>
            {
                try
                {
                    var bill = new FinancialIncomeBill();

                    if (data == null || data.Items == null)
                        return this.Warning("请录入数据.");

                    if (PeriodLocked(DateTime.Now, store ?? 0))
                        return this.Warning("锁账期间,禁止业务操作.");

                    if (PeriodClosed(DateTime.Now, store ?? 0))
                        return this.Warning("会计期间已结账,禁止业务操作.");

                    #region 单据验证
                    if (billId.HasValue && billId.Value != 0)
                    {
                        bill = _financialIncomeBillService.GetFinancialIncomeBillById(store ?? 0, billId.Value, true);

                        //公共单据验证
                        var commonBillChecking = BillChecking<FinancialIncomeBill, FinancialIncomeItem>(bill, store ?? 0, BillStates.Draft);
                        if (commonBillChecking.Data != null)
                            return commonBillChecking;
                    }

                    #endregion

                    #region 验证预付款
                    if (data.Accounting != null)
                    {
                        if (data.AdvancedPaymentsAmount > 0 && data.AdvancedPaymentsAmount > data.AdvanceAmountBalance)
                        {
                            return this.Warning("预付款余额不足!");
                        }
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
                    var dataTo = data.ToEntity<FinancialIncomeBillUpdate>();
                    if (data.Accounting == null)
                    {
                        return this.Warning("没有默认的付款账号");
                    }
                    dataTo.Accounting = data.Accounting.Select(ac =>
                    {
                        return ac.ToAccountEntity<FinancialIncomeBillAccounting>();
                    }).ToList();
                    dataTo.Items = data.Items.Select(it =>
                    {
                        return it.ToEntity<FinancialIncomeItem>();
                    }).ToList();

                    //RedLock
                    var result = await _locker.PerformActionWithLockAsync(this.LockKey(data, store ?? 0, userId ?? 0),
                          TimeSpan.FromSeconds(30),
                          TimeSpan.FromSeconds(10),
                          TimeSpan.FromSeconds(1),
                          () => _financialIncomeBillService.BillCreateOrUpdate(store ?? 0, userId ?? 0, billId, bill, dataTo.Accounting, accountings, dataTo, dataTo.Items, _userService.IsAdmin(store ?? 0, userId ?? 0)));

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
        [HttpGet("financialincome/auditing/{store}/{userId}/{id}")]
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
                    var bill = new FinancialIncomeBill();
                    #region 验证

                    if (!id.HasValue)
                        return this.Warning("参数错误.");
                    else
                    {
                        bill = _financialIncomeBillService.GetFinancialIncomeBillById(store ?? 0, id.Value, true);
                        if (bill.AuditedStatus)
                        {
                            return Warning("单据已审核，请刷新页面.");
                        }
                    }

                    //公共单据验证
                    var commonBillChecking = BillChecking<FinancialIncomeBill, FinancialIncomeItem>(bill, store ?? 0, BillStates.Audited);
                    if (commonBillChecking.Data != null)
                        return commonBillChecking;

                    #endregion

                    //RedLock
                    var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill, store ?? 0, userId ?? 0),
                          TimeSpan.FromSeconds(30),
                          TimeSpan.FromSeconds(10),
                          TimeSpan.FromSeconds(1),
                          () => _financialIncomeBillService.Auditing(store ?? 0, userId ?? 0, bill));
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
        [HttpGet("financialincome/reverse/{store}/{userId}/{id}")]
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
                    var bill = new FinancialIncomeBill();

                    if (this.PeriodClosed(DateTime.Now, store ?? 0))
                        return this.Warning("系统当月已经结转，不允许红冲");

                    if (!id.HasValue)
                        return this.Warning("参数错误.");
                    else
                    {
                        bill = _financialIncomeBillService.GetFinancialIncomeBillById(store ?? 0, id.Value, true);
                        bill.Remark = remark;
                        if (bill.ReversedStatus)
                        {
                            return Warning("单据已红冲，请刷新页面.");
                        }
                    }

                    //公共单据验证
                    var commonBillChecking = BillChecking<FinancialIncomeBill, FinancialIncomeItem>(bill, store ?? 0, BillStates.Reversed);
                    if (commonBillChecking.Data != null)
                        return commonBillChecking;

                    //RedLock
                    var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill, store ?? 0, userId ?? 0), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(1), () => _financialIncomeBillService.Reverse(userId ?? 0, bill));
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
    }
}