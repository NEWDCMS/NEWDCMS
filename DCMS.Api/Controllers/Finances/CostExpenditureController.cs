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
    /// 用于费用支出管理
    /// </summary>
    [Authorize]
    [Route("api/v{version:apiVersion}/dcms/finances")]
    public class CostExpenditureController : BaseAPIController
    {
        private readonly ITerminalService _terminalService;
        private readonly IUserService _userService;
        private readonly IAccountingService _accountingService;
        private readonly IUserActivityService _userActivityService;
        private readonly IRedLocker _locker;
        private readonly ICostExpenditureBillService _costExpenditureBillService;

        private readonly ICostContractBillService _costContractBillService;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="terminalService"></param>
        /// <param name="accountingService"></param>
        /// <param name="userActivityService"></param>
        /// <param name="costExpenditureBillService"></param>
        /// <param name="userService"></param>
        /// <param name="locker"></param>
        /// <param name="costContractBillService"></param>
        public CostExpenditureController(
            ITerminalService terminalService,
            IAccountingService accountingService,
            IUserActivityService userActivityService,
            ICostExpenditureBillService costExpenditureBillService,

            IUserService userService,
            IRedLocker locker,
            ICostContractBillService costContractBillService
           , ILogger<BaseAPIController> logger) : base(logger)
        {
            _terminalService = terminalService;
            _accountingService = accountingService;
            _userService = userService;
            _userActivityService = userActivityService;
            _costExpenditureBillService = costExpenditureBillService;

            _locker = locker;
            _costContractBillService = costContractBillService;
        }

        /// <summary>
        /// 获取费用支出
        /// </summary>
        /// <param name="store"></param>
        /// <param name="customerId"></param>
        /// <param name="employeeId"></param>
        /// <param name="billNumber"></param>
        /// <param name="auditedStatus"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="showReverse"></param>
        /// <param name="sortByAuditedTime"></param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet("costexpenditurebill/getbills/{store}/{customerId}/{employeeId}")]
        [SwaggerOperation("getbills")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<CostExpenditureBillModel>>> AsyncList(int? store, int? makeuserId, int? customerId, string customerName, int? employeeId, string billNumber, bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? showReverse = null, bool sortByAuditedTime = false, int? sign = null, int pagenumber = 0, int pageSize = 50)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<CostExpenditureBillModel>(Resources.ParameterError);
            return await Task.Run(() =>
            {
                try
                {
                    //默认收款账户动态列
                    var defaultAcc = _accountingService.GetDefaultAccounting(store ?? 0, BillTypeEnum.CostExpenditureBill);

                    //获取分页
                    var bills = _costExpenditureBillService.GetAllCostExpenditureBills(
                        store ?? 0,
                        makeuserId ?? 0,
                        employeeId,
                        customerId,
                        billNumber,
                        auditedStatus,
                        startTime,
                        endTime,
                        showReverse,
                        sortByAuditedTime,
                        null,
                        null,
                        sign,
                        pagenumber, 30);

                    #region 查询需要关联其他表的数据

                    var allUsers = _userService.GetUsersDictsByIds(store ?? 0, bills.Select(b => b.EmployeeId).Distinct().ToArray());

                    List<int> custorerIds = new List<int>();
                    List<int> custorerIds2 = new List<int>();
                    if (bills != null && bills.Count > 0)
                    {
                        bills.ToList().ForEach(bill =>
                        {
                            if (bill != null && bill.Items != null && bill.Items.Count > 0)
                            {
                                bill.Items.ToList().ForEach(bi =>
                                {
                                    custorerIds2.AddRange(bill.Items.Select(ba => ba.CustomerId));
                                });
                            }
                        });
                    }
                    custorerIds.AddRange(custorerIds2);
                    //去重
                    custorerIds = custorerIds.Distinct().ToList();
                    var allTerminal = _terminalService.GetTerminalsByIds(store ?? 0, custorerIds.Distinct().ToArray());

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
                    #endregion

                    var results = bills.OrderByDescending(b => b.CreatedOnUtc).Select(b =>
                    {
                        var m = b.ToModel<CostExpenditureBillModel>();

                        //员工
                        m.EmployeeName = allUsers.Where(aw => aw.Key == m.EmployeeId).Select(aw => aw.Value).FirstOrDefault();
                        m.TotalAmount = b.Items?.Sum(t => t.Amount) ?? 0;
                        m.CustomerId = b.Items?.FirstOrDefault()?.CustomerId ?? 0;
                        m.CustomerName = _terminalService.GetTerminalName(store, b.Items?.FirstOrDefault()?.CustomerId ?? 0);

                        m.CostExpenditureBillAccountings = defaultAcc?.Item4?.OrderBy(sb => sb.Key).Select(sb =>
                        {
                            var acc = b.CostExpenditureBillAccountings.Where(a => a?.AccountingOption?.ParentId == sb.Key).FirstOrDefault();
                            return new CostExpenditureBillAccountingModel()
                            {
                                Name = acc?.AccountingOption?.Name,
                                AccountingOptionName = acc?.AccountingOption?.Name,
                                AccountingOptionId = acc?.AccountingOptionId ?? 0,
                                CollectionAmount = acc?.CollectionAmount ?? 0
                            };
                        }).Where(ao => ao.AccountingOptionId > 0).ToList();

                        m.Items = b.Items.Select(sb =>
                        {
                            var it = sb.ToModel<CostExpenditureItemModel>();

                            var accountingOption2 = allAccountingOptions.Where(al => al.Id == sb.AccountingOptionId).FirstOrDefault();
                            it.AccountingOptionName = accountingOption2 == null ? "" : accountingOption2.Name;

                            var terminal = allTerminal.Where(t => t.Id == it.CustomerId).FirstOrDefault();
                            it.CustomerName = terminal == null ? "" : terminal.Name;

                            return it;
                        }).ToList();



                        return m;
                    }).ToList();
                    if (customerId.HasValue && customerId.Value > 0)
                    {
                        results = results.Where(c => c.CustomerId == customerId).ToList();
                    }

                    return this.Successful("", results);
                }
                catch (Exception ex)
                {
                    return this.Error<CostExpenditureBillModel>(ex.Message);
                }

            });
        }

        /// <summary>
        /// 获取费用支出
        /// </summary>
        /// <param name="store"></param>
        /// <param name="billId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpGet("costexpenditurebill/getCostExpenditureBill/{store}/{billId}/{user}")]
        [SwaggerOperation("getCostExpenditureBill")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<CostExpenditureBillModel>> GetCostExpenditureBill(int? store, int? billId, int user = 0)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<CostExpenditureBillModel>(false, Resources.ParameterError);
            return await Task.Run(() =>
            {
                var model = new CostExpenditureBillModel();
                try
                {
                    var bill = _costExpenditureBillService.GetCostExpenditureBillById(store ?? 0, billId ?? 0, true);

                    if (bill != null)
                    {
                        model = bill.ToModel<CostExpenditureBillModel>();
                        model.Items = bill.Items.Select(c =>
                        {
                            var m = c.ToModel<CostExpenditureItemModel>();

                            m.AccountingOptionName = _accountingService.GetAccountingOptionName(store ?? 0, c.AccountingOptionId);
                            return m;
                        }).ToList();
                    }

                    model.CustomerId = model.Items?.FirstOrDefault()?.CustomerId ?? 0;
                    model.CustomerName = _terminalService.GetTerminalName(store, model.Items?.FirstOrDefault()?.CustomerId ?? 0);

                    //获取默认收款账户
                    var defaultAcc = _accountingService.GetDefaultAccounting(store ?? 0, BillTypeEnum.CostExpenditureBill);
                    model.CostExpenditureBillAccountings = bill.CostExpenditureBillAccountings.Select(s =>
                    {
                        var m = s.ToAccountModel<CostExpenditureBillAccountingModel>();

                        m.Name = _accountingService.GetAccountingOptionName(store ?? 0, s.AccountingOptionId);
                        m.AccountingOptionName = _accountingService.GetAccountingOptionName(store ?? 0, s.AccountingOptionId);
                        return m;
                    }).ToList();

                    //员工
                    model.Employees = BindUserSelection(_userService.BindUserList, store ?? 0, DCMSDefaults.Salesmans);
                    var mu = _userService.GetUserName(store ?? 0, bill.MakeUserId);

                    model.MakeUserName = mu + " " + bill.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss");

                    var au = _userService.GetUserName(store ?? 0, bill.AuditedUserId ?? 0); ;
                    model.AuditedUserName = au + " " + (bill.AuditedDate.HasValue ? bill.AuditedDate.Value.ToString("yyyy/MM/dd HH:mm:ss") : "");

                    return this.Successful("", model);
                }
                catch (Exception ex)
                {
                    return this.Error<CostExpenditureBillModel>(false, ex.Message);
                }
            });
        }



        /// <summary>
        /// 创建/更新费用支出单
        /// </summary>
        /// <param name="data"></param>
        /// <param name="billId"></param>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost("costexpenditurebill/createorupdate/{store}/{userId}/{billId}")]
        [SwaggerOperation("createorupdate")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> CreateOrUpdate(CostExpenditureUpdateModel data, int? billId, int? store, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);
            return await Task.Run(async () =>
            {
                try
                {
                    CostExpenditureBill bill = new CostExpenditureBill();

                    if (data == null || data.Items == null)
                        return this.Warning("请录入数据.");

                    if (PeriodLocked(DateTime.Now, store ?? 0))
                        return this.Warning("锁账期间,禁止业务操作.");

                    if (PeriodClosed(DateTime.Now, store ?? 0))
                        return this.Warning("会计期间已结账,禁止业务操作.");

                    #region 单据验证

                    if (billId.HasValue && billId.Value != 0)
                    {
                        bill = _costExpenditureBillService.GetCostExpenditureBillById(store ?? 0, billId.Value, true);

                        //公共单据验证
                        var commonBillChecking = BillChecking<CostExpenditureBill, CostExpenditureItem>(bill, store ?? 0, BillStates.Draft);
                        if (commonBillChecking.Data != null)
                            return commonBillChecking;
                    }

                    //客户验证(费用合同客户必须等于当前项目明细客户)
                    if (data.Items != null && data.Items.Count > 0)
                    {
                        foreach (var it in data.Items)
                        {
                            if (it.CostContractId > 0)
                            {
                                var costContract = _costContractBillService.GetCostContractBillById(store ?? 0, it.CostContractId, false);

                                if (costContract != null)
                                {
                                    if (costContract.CustomerId != it.CustomerId)
                                    {
                                        return Warning($"费用合同{it.CostContractName}的客户不为当前客户{it.CustomerName}");
                                    }
                                }
                                else
                                {
                                    return Warning($"费用合同不存在");
                                }
                            }
                        }
                    }
                    #endregion

                    //业务逻辑
                    var accountings = _accountingService.GetAllAccountingOptions(store ?? 0, 0, true);
                    var dataTo = data.ToEntity<CostExpenditureBillUpdate>();
                    dataTo.Operation = (int)OperationEnum.APP;
                    if (data.Accounting == null)
                    {
                        return this.Warning("没有默认的付款账号");
                    }
                    dataTo.Accounting = data.Accounting.Select(ac =>
                    {
                        return ac.ToEntity<CostExpenditureBillAccounting>();
                    }).ToList();
                    dataTo.Items = data.Items.Where(it => it.AccountingOptionId > 0).Select(it =>
                    {
                        return it.ToEntity<CostExpenditureItem>();
                    }).ToList();

                    //RedLock
                    var result = await _locker.PerformActionWithLockAsync(this.LockKey(data, store ?? 0, userId ?? 0),
                        TimeSpan.FromSeconds(30),
                        TimeSpan.FromSeconds(10),
                        TimeSpan.FromSeconds(1),
                        () => _costExpenditureBillService.BillCreateOrUpdate(store ?? 0, userId ?? 0, billId, bill, dataTo.Accounting, accountings, dataTo, dataTo.Items, _userService.IsAdmin(store ?? 0, userId ?? 0)));
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
        [HttpGet("costexpenditurebill/auditing/{store}/{userId}/{id}")]
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
                    var bill = new CostExpenditureBill() { StoreId = store ?? 0 };

                    #region 验证

                    if (!id.HasValue)
                        return this.Warning("参数错误.");
                    else
                    {
                        bill = _costExpenditureBillService.GetCostExpenditureBillById(store ?? 0, id.Value, true);
                        if (bill.AuditedStatus)
                        {
                            return Warning("单据已审核，请刷新页面.");
                        }
                    }

                    //公共单据验证
                    var commonBillChecking = BillChecking<CostExpenditureBill, CostExpenditureItem>(bill, store ?? 0, BillStates.Audited);
                    if (commonBillChecking.Data != null)
                        return commonBillChecking;

                    #endregion

                    //RedLock
                    var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill, store ?? 0, userId ?? 0),
                          TimeSpan.FromSeconds(30),
                          TimeSpan.FromSeconds(10),
                          TimeSpan.FromSeconds(1),
                          () => _costExpenditureBillService.Auditing(userId ?? 0, bill));
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
        [HttpGet("costexpenditurebill/reverse/{store}/{userId}/{id}")]
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
                    var bill = new CostExpenditureBill() { StoreId = store ?? 0 };

                    #region 验证

                    if (this.PeriodClosed(DateTime.Now, store ?? 0))
                        return this.Warning("系统当月已经结转，不允许红冲");

                    if (!id.HasValue)
                        return this.Warning("参数错误.");
                    else
                    {
                        bill = _costExpenditureBillService.GetCostExpenditureBillById(store ?? 0, id.Value, true);
                        bill.Remark = remark;
                        if (bill.ReversedStatus)
                        {
                            return Warning("单据已红冲，请刷新页面.");
                        }
                    }

                    //公共单据验证
                    var commonBillChecking = BillChecking<CostExpenditureBill, CostExpenditureItem>(bill, store ?? 0, BillStates.Reversed);
                    if (commonBillChecking.Data != null)
                        return commonBillChecking;

                    #endregion

                    //RedLock
                    var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill, store ?? 0, userId ?? 0),
                          TimeSpan.FromSeconds(30),
                          TimeSpan.FromSeconds(10),
                          TimeSpan.FromSeconds(1),
                          () => _costExpenditureBillService.Reverse(userId ?? 0, bill));
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
        [HttpGet("costexpenditurebil/getinitdataasync/{store}/{userId}")]
        [SwaggerOperation("GetInitDataAsync")]
        //[AuthBaseFilter]
        public async Task<APIResult<CostExpenditureBillModel>> GetInitDataAsync(int? store, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error3<CostExpenditureBillModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var model = new CostExpenditureBillModel();

                    //默认售价（方案）


                    //默认账户设置


                    return this.Successful("", model);
                }
                catch (Exception ex)
                {
                    return this.Error3<CostExpenditureBillModel>(ex.Message);
                }
            });
        }
    }
}