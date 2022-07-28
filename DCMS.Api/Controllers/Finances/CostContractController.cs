using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Finances;
using DCMS.Services.Finances;
using DCMS.Services.Logging;
using DCMS.Services.Products;
using DCMS.Services.Settings;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
using DCMS.ViewModel.Models.Configuration;
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
    /// 用于费用合同管理
    /// </summary>
    [Authorize]
    [Route("api/v{version:apiVersion}/dcms/finances")]
    public class CostContractController : BaseAPIController
    {
        private readonly ITerminalService _terminalService;
        private readonly IUserService _userService;
        private readonly IAccountingService _accountingService;
        private readonly IUserActivityService _userActivityService;
        private readonly ICostContractBillService _costContractBillService;


        private readonly IRedLocker _locker;

        private readonly IGiveQuotaService _giveQuotaService;
        private readonly IProductService _productService;
        private readonly ISpecificationAttributeService _specificationAttributeService;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="terminalService"></param>
        /// <param name="accountingService"></param>
        /// <param name="userActivityService"></param>
        /// <param name="costContractBillService"></param>
        /// <param name="userService"></param>
        public CostContractController(
            ITerminalService terminalService,
            IAccountingService accountingService,
            IUserActivityService userActivityService,
            ICostContractBillService costContractBillService,

            IUserService userService,
            IRedLocker locker,
            IGiveQuotaService giveQuotaService,
            IProductService productService,
            ISpecificationAttributeService specificationAttributeService
           , ILogger<BaseAPIController> logger) : base(logger)
        {
            _terminalService = terminalService;
            _accountingService = accountingService;
            _userService = userService;
            _userActivityService = userActivityService;
            _costContractBillService = costContractBillService;

            _locker = locker;

            _giveQuotaService = giveQuotaService;
            _productService = productService;
            _specificationAttributeService = specificationAttributeService;
        }

        /// <summary>
        /// 获取费用合同
        /// </summary>
        /// <param name="store"></param>
        /// <param name="customerId"></param>
        /// <param name="employeeId"></param>
        /// <param name="billNumber"></param>
        /// <param name="remark"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pagenumber"></param>
        /// <returns></returns>
        [HttpGet("costContract/getbills/{store}/{customerId}/{employeeId}")]
        [SwaggerOperation("getbills")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<CostContractBillModel>>> AsyncList(int? store, int? makeuserId, int? customerId, string customerName, int? employeeId, string billNumber, string remark, DateTime? startTime = null, DateTime? endTime = null, bool? auditedStatus = null, bool? showReverse = null, int pagenumber = 0, int pageSize = 50)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<CostContractBillModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    //获取分页
                    var bills = _costContractBillService.GetAllCostContractBills(store, makeuserId, customerId, customerName, employeeId, billNumber, remark, startTime, endTime, false, null, null, null, auditedStatus, showReverse, pagenumber, pageSize);
                    #region 查询需要关联其他表的数据

                    var allUsers = _userService.GetUsersDictsByIds(store ?? 0, bills.Select(b => b.EmployeeId).Distinct().ToArray());
                    var allTerminals = _terminalService.GetTerminalsDictsByIds(store ?? 0, bills.Select(b => b.CustomerId).Distinct().ToArray());
                    var alllAccounting = _accountingService.GetAccountingOptionsByIds(store ?? 0, bills.Select(b => b.AccountingOptionId).Distinct().ToArray());

                    #endregion

                    var result = bills.OrderByDescending(b => b.CreatedOnUtc).Select(b =>
                     {
                         var m = b.ToModel<CostContractBillModel>();

                         var account = alllAccounting.Where(a => a.Id == m.AccountingOptionId).FirstOrDefault();

                         m.AccountingOptionName = account != null ? account.Name : "";
                         m.EmployeeName = _userService.GetUserName(store, b.EmployeeId);
                         m.CustomerName = allTerminals.Where(u => u.Key == m.CustomerId).Select(u => u.Value).FirstOrDefault();
                         m.AuditedStatusName = m.AuditedStatus ? "已审核" : "未审核";

                         if (b.Items != null && b.Items.Count > 0)
                         {
                             m.Items = b.Items.Select(i =>
                              {
                                  var s = i.ToModel<CostContractItemModel>();

                                  if (i.CType == 0)
                                  {
                                      s.ProductName = _productService.GetProductById(store ?? 0, i.ProductId)?.Name;
                                  }
                                  else
                                  {
                                      s.ProductName = "现金";
                                  }

                                  s.TotalQuantity = i.Total == null ? 0 : decimal.ToInt32(i.Total ?? 0);
                                  return s;
                              }).ToList();
                         }

                         return m;
                     }).ToList();
                    return this.Successful(Resources.Successful, result);
                }
                catch (Exception ex)
                {
                    return this.Error<CostContractBillModel>(ex.Message);
                }
            });
        }

        /// <summary>
        /// 根据Id获取费用合同
        /// </summary>
        /// <param name="store"></param>
        /// <param name="billId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpGet("costContract/getCostContractBill/{store}/{billId}/{user}")]
        [SwaggerOperation("getCostContractBill")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<CostContractBillModel>> GetCostContractBill(int? store, int? billId, int user = 0)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<CostContractBillModel>(false, Resources.ParameterError);
            return await Task.Run(() =>
            {
                var model = new CostContractBillModel();
                try
                {
                    var bill = _costContractBillService.GetCostContractBillById(store ?? 0, billId ?? 0, true);
                    if (bill != null)
                    {
                        model = bill.ToModel<CostContractBillModel>();

                        model.Items = bill.Items.Select(c =>
                        {
                            var m = c.ToModel<CostContractItemModel>();

                            if (c.CType == 0)
                            {
                                m.ProductName = _productService.GetProductName(store, m.ProductId);
                            }
                            else
                            {
                                m.ProductName = "现金";
                            }

                            m.TotalQuantity = m.Total == null ? 0 : decimal.ToInt32(m.Total ?? 0);

                            return m;
                        }).ToList();

                        model.AccountingOptionName = _accountingService.GetAccountingOptionName(store ?? 0, bill.AccountingOptionId);

                        model.LeaderName = _userService.GetUserName(store ?? 0, bill.LeaderId ?? 0);
                        //主管
                        model.Leaders = BindUserSelection(_userService.BindUserList, store ?? 0, DCMSDefaults.Administrators);

                        //员工
                        model.Employees = BindUserSelection(_userService.BindUserList, store ?? 0, DCMSDefaults.Salesmans);

                        //客户
                        model.CustomerName = _terminalService.GetTerminalName(store ?? 0, bill.CustomerId);

                        //制单人
                        model.MakeUserName = _userService.GetUserName(store ?? 0, bill.MakeUserId) + " " + bill.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss");

                        //审核人
                        model.AuditedUserName = _userService.GetUserName(store ?? 0, bill.AuditedUserId ?? 0) + " " + (bill.AuditedDate.HasValue ? bill.AuditedDate.Value.ToString("yyyy/MM/dd HH:mm:ss") : "");

                        var accountungs = _accountingService.GetSubAccountingOptionsByAccountCodeTypeIds(store ?? 0, new[] { (int)AccountingCodeEnum.SaleFees, (int)AccountingCodeEnum.Preferential, (int)AccountingCodeEnum.CardFees, (int)AccountingCodeEnum.DisplayFees }, true);
                        if (accountungs != null)
                        {
                            model.AccountingOptionSelects = accountungs.Select(s =>
                            {
                                var m = s.ToModel<AccountingOptionModel>();
                                m.ParentName = _accountingService.GetAccountingOptionName(store ?? 0, s.ParentId ?? 0);
                                return m;
                            }).ToList();
                        }
                    }
                    return this.Successful(Resources.Successful, model);
                }
                catch (Exception ex)
                {
                    return this.Error<CostContractBillModel>(false, ex.Message);
                }

            });
        }


        /// <summary>
        /// 创建/更新单据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <param name="billId"></param>
        /// <returns></returns>
        [HttpPost("costContract/createorupdate/{store}/{userId}/{billId}")]
        [SwaggerOperation("createorupdate")]
        //[ValidateActionParameters]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> CreateOrUpdate(CostContractUpdateModel data, int? store, int? userId, int? billId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(async () =>
            {
                try
                {
                    if (data == null || data.Items == null)
                        return this.Warning("请录入数据.");

                    if (PeriodLocked(DateTime.Now, store ?? 0))
                        return this.Warning("锁账期间,禁止业务操作.");

                    if (PeriodClosed(DateTime.Now, store ?? 0))
                        return this.Warning("会计期间已结账,禁止业务操作.");

                    //验证从主管赠品扣除
                    if (data.ContractType == (int)ContractTypeEnum.ManageGift)
                    {
                        if (data.Items != null && data.Items.Count > 0)
                        {
                            var allGiveQuotaOption = _giveQuotaService.GetGiveQuotaBalances(store, data.Year, data.LeaderId); //获取主管的赠品及赠品额度余额(余额已根据最小单位转换计算)
                            var allProducts = _productService.GetProductsByIds(store ?? 0, data.Items.Select(di => di.ProductId).Distinct().ToArray());
                            var allOptions = _specificationAttributeService.GetSpecificationAttributeOptionByIds(store ?? 0, allProducts.GetProductBigStrokeSmallUnitIds());

                            foreach (var item in data.Items)
                            {
                                if (item.ProductId > 0)
                                {
                                    if (allGiveQuotaOption.Count(c => c.ProductId == item.ProductId) == 0)
                                        return this.Warning("主管赠品中查无此商品!");

                                    //转换成相同单位
                                    var giveQuotaOption = allGiveQuotaOption.FirstOrDefault(ag => ag.ProductId == item.ProductId);

                                    var product = allProducts.Where(ap => ap.Id == item.ProductId).FirstOrDefault();

                                    if (giveQuotaOption != null)
                                    {
                                        //添加主管额度主表 Id
                                        item.GiveQuotaId = giveQuotaOption.GiveQuotaId;
                                        item.GiveQuotaOptionId = giveQuotaOption.Id;

                                        int quantity = 0;
                                        //具体到月
                                        switch (data.Month)
                                        {
                                            case 1:
                                                quantity = giveQuotaOption.Jan_Balance == null ? 0 : (int)giveQuotaOption.Jan_Balance;
                                                break;
                                            case 2:
                                                quantity = giveQuotaOption.Feb_Balance == null ? 0 : (int)giveQuotaOption.Feb_Balance;
                                                break;
                                            case 3:
                                                quantity = giveQuotaOption.Mar_Balance == null ? 0 : (int)giveQuotaOption.Mar_Balance;
                                                break;
                                            case 4:
                                                quantity = giveQuotaOption.Apr_Balance == null ? 0 : (int)giveQuotaOption.Apr_Balance;
                                                break;
                                            case 5:
                                                quantity = giveQuotaOption.May_Balance == null ? 0 : (int)giveQuotaOption.May_Balance;
                                                break;
                                            case 6:
                                                quantity = giveQuotaOption.Jun_Balance == null ? 0 : (int)giveQuotaOption.Jun_Balance;
                                                break;
                                            case 7:
                                                quantity = giveQuotaOption.Jul_Balance == null ? 0 : (int)giveQuotaOption.Jul_Balance;
                                                break;
                                            case 8:
                                                quantity = giveQuotaOption.Aug_Balance == null ? 0 : (int)giveQuotaOption.Aug_Balance;
                                                break;
                                            case 9:
                                                quantity = giveQuotaOption.Sep_Balance == null ? 0 : (int)giveQuotaOption.Sep_Balance;
                                                break;
                                            case 10:
                                                quantity = giveQuotaOption.Oct_Balance == null ? 0 : (int)giveQuotaOption.Oct_Balance;
                                                break;
                                            case 11:
                                                quantity = giveQuotaOption.Nov_Balance == null ? 0 : (int)giveQuotaOption.Nov_Balance;
                                                break;
                                            case 12:
                                                quantity = giveQuotaOption.Dec_Balance == null ? 0 : (int)giveQuotaOption.Dec_Balance;
                                                break;
                                            default:
                                                break;
                                        }

                                        //当前用户输入数量：注意防止用户复制明细，所以根据 GiveQuotaOptionId汇总
                                        List<CostContractItemModel> itemModels = data.Items.Where(di => di.ProductId == item.ProductId).ToList();
                                        if (itemModels != null && itemModels.Count > 0)
                                        {
                                            int thisQuantity = 0;
                                            foreach (var item2 in itemModels)
                                            {
                                                //商品转化量
                                                var conversionQuantity2 = product.GetConversionQuantity(allOptions, item.UnitId);
                                                int thisTotal = 0;
                                                thisTotal = item2.Total == null ? 0 : (int)item2.Total;
                                                //最小单位数量
                                                thisQuantity += thisTotal * conversionQuantity2;
                                            }

                                            if (thisQuantity > quantity)
                                            {
                                                return this.Warning("主管赠品余额不足!");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            return this.Warning("费用合同没有明细");
                        }
                    }

                    //按月兑付时，判断当月是否已有合同
                    if (data.ContractType == (int)ContractTypeEnum.ByMonth)
                    {
                        if (!_costContractBillService.CheckContract(store ?? 0, data.Year, data.CustomerId, data.AccountingOptionId, data.Items.Select(it =>
                          {
                              return it.ToEntity<CostContractItem>();
                          }).ToList(), out string errMsg))
                            return this.Warning(errMsg);
                    }

                    //业务逻辑
                    var accountings = _accountingService.GetAllAccountingOptions(store ?? 0, 0, true);
                    var dataTo = data.ToEntity<CostContractBillUpdate>();
                    dataTo.Operation = (int)OperationEnum.APP;
                    dataTo.Items = data.Items.Select(it =>
                    {
                        return it.ToEntity<CostContractItem>();
                    }).ToList();

                    //RedLock
                    var result = await _locker.PerformActionWithLockAsync(this.LockKey(data, store ?? 0, store ?? 0),
                       TimeSpan.FromSeconds(30),
                       TimeSpan.FromSeconds(10),
                       TimeSpan.FromSeconds(1),
                       () => _costContractBillService.BillCreateOrUpdate(store ?? 0, userId ?? 0, billId, accountings, dataTo, dataTo.Items, _userService.IsAdmin(store ?? 0, userId ?? 0)));
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
        [HttpGet("costContract/auditing/{store}/{userId}/{id}")]
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
                    CostContractBill bill = new CostContractBill();
                    #region 验证

                    if (!id.HasValue)
                        return this.Warning("参数错误.");
                    else
                    {
                        bill = _costContractBillService.GetCostContractBillById(store ?? 0, id.Value, true);
                        if (bill.AuditedStatus)
                        {
                            return Warning("单据已审核，请刷新页面.");
                        }
                    }

                    //公共单据验证
                    var commonBillChecking = this.BillChecking<CostContractBill, CostContractItem>(bill, store ?? 0, BillStates.Audited);
                    if (commonBillChecking.Data != null)
                        return commonBillChecking;

                    #endregion

                    //RedLock
                    var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill, store ?? 0, userId ?? 0),
                          TimeSpan.FromSeconds(30),
                          TimeSpan.FromSeconds(10),
                          TimeSpan.FromSeconds(1),
                          () => _costContractBillService.Auditing(store ?? 0, userId ?? 0, bill));
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
        /// 驳回
        /// </summary>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("costContract/rejected/{store}/{userId}/{id}")]
        [SwaggerOperation("rejected")]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> Rejected(int? store, int? userId, int? id)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(async () =>
            {
                try
                {

                    var bill = new CostContractBill();
                    #region 验证

                    if (!id.HasValue)
                        return this.Warning("参数错误.");
                    else
                    {
                        bill = _costContractBillService.GetCostContractBillById(store ?? 0, id.Value, true);
                        if (bill.RejectedStatus)
                        {
                            return Warning("单据已驳回，请刷新页面.");
                        }
                    }

                    //公共单据验证
                    var commonBillChecking = this.BillChecking<CostContractBill, CostContractItem>(bill, store ?? 0, BillStates.Audited);
                    if (commonBillChecking.Data != null)
                        return commonBillChecking;

                    #endregion

                    //RedLock
                    var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill, store ?? 0, userId ?? 0),
                          TimeSpan.FromSeconds(30),
                          TimeSpan.FromSeconds(10),
                          TimeSpan.FromSeconds(1),
                          () => _costContractBillService.Rejected(store ?? 0, userId ?? 0, bill));
                    return result.To(result);
                }
                catch (Exception ex)
                {
                    _userActivityService.InsertActivity("Reverse", "单据驳回失败", userId);
                    return this.Error(ex.Message);
                }
            });
        }

        /// <summary>
        /// 终止
        /// </summary>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("costContract/abandoned/{store}/{userId}/{id}")]
        [SwaggerOperation("abandoned")]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> Abandoned(int? store, int? userId, int? id)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(async () =>
            {
                try
                {

                    var bill = new CostContractBill();
                    #region 验证
                    if (!id.HasValue)
                        return this.Warning("参数错误.");
                    else
                        bill = _costContractBillService.GetCostContractBillById(store ?? 0, id.Value, true);

                    //公共单据验证
                    var commonBillChecking = this.BillChecking<CostContractBill, CostContractItem>(bill, store ?? 0, BillStates.Reversed);
                    if (commonBillChecking.Data != null)
                        return commonBillChecking;

                    if (bill.AuditedStatus != true || bill.RejectedStatus != true || bill.AbandonedStatus != false)
                        return this.Warning("重复操作.");

                    #endregion

                    //RedLock
                    var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill, store ?? 0, userId ?? 0),
                          TimeSpan.FromSeconds(30),
                          TimeSpan.FromSeconds(10),
                          TimeSpan.FromSeconds(1),
                          () => _costContractBillService.Auditing(store ?? 0, userId ?? 0, bill));
                    return result.To(result);
                }
                catch (Exception ex)
                {
                    _userActivityService.InsertActivity("Reverse", "单据驳回失败", userId);
                    return this.Error(ex.Message);
                }

            });
        }



        /// <summary>
        /// 根据科目类别获取费用合同
        /// </summary>
        /// <param name="store"></param>
        /// <param name="accountCodeTypeId"></param>
        /// <param name="customerId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("costContract/getCostContractsByAccountingOptionId/{store}/{customerId}/{accountCodeTypeId}")]
        [SwaggerOperation("getCostContractsByAccountingOptionId")]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<CostContractBindingModel>>> GetCostContractsByAccountingOptionId(int? store, int? userId, int? customerId, int? accountOptionId, int? accountCodeTypeId, int year, int month, int? contractType = 0, bool? auditedStatus = null, bool? showReverse = null, int pageIndex = 0, int pageSize = 50)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<CostContractBindingModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    var result = new List<CostContractBindingModel>();
                    var bills = _costContractBillService.GetAllCostContractBills(store, userId, customerId, accountOptionId, accountCodeTypeId, year, month, contractType, auditedStatus, showReverse, pageIndex, pageSize);

                    var accountName = _accountingService.GetAccountingOptionNameByCodeType(store, accountCodeTypeId ?? 0);

                    foreach (var bill in bills)
                    {
                        var items = _costContractBillService.CalcCostContractBalances(store ?? 0, bill.CustomerId, bill).Where(c => c.CType == 1).ToList();

                        //客户名称
                        string customerName = _terminalService.GetTerminalName(store ?? 0, bill.CustomerId);

                        if (bill.ContractType == 0)
                        {
                            items.ForEach(i =>
                            {
                                if (i.Jan >= 0)
                                {
                                    var m = PrepareCostContractBindModel(bill, i, bill.Year, (int)i.Parse("Jan"), i.Jan, i.Jan_Balance);

                                    m.AccountingOptionName = accountName;
                                    m.CustomerName = customerName;

                                    result.Add(m);
                                }
                                //2月
                                if (i.Feb >= 0)
                                {
                                    var m = PrepareCostContractBindModel(bill, i, bill.Year, (int)i.Parse("Feb"), i.Feb, i.Feb_Balance);

                                    m.AccountingOptionName = accountName;
                                    m.CustomerName = customerName;

                                    result.Add(m);
                                }
                                //3月
                                if (i.Mar >= 0)
                                {
                                    var m = PrepareCostContractBindModel(bill, i, bill.Year, (int)i.Parse("Mar"), i.Mar, i.Mar_Balance);

                                    m.AccountingOptionName = accountName;
                                    m.CustomerName = customerName;

                                    result.Add(m);
                                }
                                //4月
                                if (i.Apr >= 0)
                                {
                                    var m = PrepareCostContractBindModel(bill, i, bill.Year, (int)i.Parse("Apr"), i.Apr, i.Apr_Balance);

                                    m.AccountingOptionName = accountName;
                                    m.CustomerName = customerName;

                                    result.Add(m);
                                }
                                //5月
                                if (i.May >= 0)
                                {
                                    var m = PrepareCostContractBindModel(bill, i, bill.Year, (int)i.Parse("May"), i.May, i.May_Balance);

                                    m.AccountingOptionName = accountName;
                                    m.CustomerName = customerName;

                                    result.Add(m);
                                }
                                //6月
                                if (i.Jun >= 0)
                                {
                                    var m = PrepareCostContractBindModel(bill, i, bill.Year, (int)i.Parse("Jun"), i.Jun, i.Jun_Balance);

                                    m.AccountingOptionName = accountName;
                                    m.CustomerName = customerName;

                                    result.Add(m);
                                }
                                //7月
                                if (i.Jul >= 0)
                                {
                                    var m = PrepareCostContractBindModel(bill, i, year, (int)i.Parse("Jul"), i.Jul, i.Jul_Balance);

                                    m.AccountingOptionName = accountName;
                                    m.CustomerName = customerName;

                                    result.Add(m);
                                }
                                //8月
                                if (i.Aug >= 0)
                                {
                                    var m = PrepareCostContractBindModel(bill, i, bill.Year, (int)i.Parse("Aug"), i.Aug, i.Aug_Balance);

                                    m.AccountingOptionName = accountName;
                                    m.CustomerId = bill.CustomerId;
                                    m.CustomerName = customerName;

                                    result.Add(m);
                                }
                                //9月
                                if (i.Sep >= 0)
                                {
                                    var m = PrepareCostContractBindModel(bill, i, bill.Year, (int)i.Parse("Sep"), i.Sep, i.Sep_Balance);

                                    m.AccountingOptionName = accountName;
                                    m.CustomerName = customerName;

                                    result.Add(m);
                                }
                                //10月
                                if (i.Oct >= 0)
                                {

                                    var m = PrepareCostContractBindModel(bill, i, bill.Year, (int)i.Parse("Oct"), i.Oct, i.Oct_Balance);

                                    m.AccountingOptionName = accountName;
                                    m.CustomerName = customerName;

                                    result.Add(m);
                                }
                                //11月
                                if (i.Nov >= 0)
                                {

                                    var m = PrepareCostContractBindModel(bill, i, bill.Year, (int)i.Parse("Nov"), i.Nov, i.Nov_Balance);

                                    m.AccountingOptionName = accountName;
                                    m.CustomerId = bill.CustomerId;
                                    m.CustomerName = customerName;

                                    result.Add(m);
                                }
                                //12月
                                if (i.Dec >= 0)
                                {
                                    var m = PrepareCostContractBindModel(bill, i, year, (int)i.Parse("Dec"), i.Dec, i.Dec_Balance);

                                    m.AccountingOptionName = accountName;
                                    m.CustomerName = customerName;

                                    result.Add(m);
                                }
                            });
                        }
                        else
                        {
                            items.ForEach(i =>
                            {
                                if (i.Total > 0)
                                {
                                    var m = PrepareCostContractBindModel(bill, i, year, null, i.Total, i.Total_Balance);

                                    m.AccountingOptionName = accountName;
                                    m.CustomerName = customerName;

                                    result.Add(m);
                                }
                            });
                        }

                    }

                    //排序
                    result = result.OrderBy(o => o.BillNumber).ThenByDescending(s => s.Month).ToList();

                    return this.Successful(Resources.Successful, result);
                }
                catch (Exception ex)
                {
                    return this.Error<CostContractBindingModel>(ex.Message);
                }
            });
        }

        [NonAction]
        private CostContractBindingModel PrepareCostContractBindModel(CostContractBill bill, CostContractItem item, int? year, int? month, decimal? monthAmount, decimal? monthBalance)
        {

            return new CostContractBindingModel
            {
                Id = bill.Id,
                StoreId = bill.StoreId,
                CustomerId = bill.CustomerId,
                BillNumber = bill.BillNumber,
                Year = year ?? 0,
                Month = month.HasValue ? month ?? 0 : 0,
                MonthName = month.HasValue ? $"{month ?? 0}月" : "",
                Amount = monthAmount,
                Balance = monthBalance,
                AccountingOptionId = bill.AccountingOptionId,
                AccountCodeTypeId = bill.AccountCodeTypeId
            };
        }
    }
}