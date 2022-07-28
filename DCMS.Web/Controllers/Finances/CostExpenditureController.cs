using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Finances;
using DCMS.Core.Domain.Terminals;
using DCMS.Core.Domain.Users;
using DCMS.Services.Common;
using DCMS.Services.Configuration;
using DCMS.Services.ExportImport;
using DCMS.Services.Finances;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Settings;
using DCMS.Services.Terminals;
using DCMS.Services.Users;
using DCMS.ViewModel.Models.Finances;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace DCMS.Web.Controllers
{
    /// <summary>
    /// 用于费用支出管理
    /// </summary>
    public class CostExpenditureController : BasePublicController
    {
        private readonly IPrintTemplateService _printTemplateService;
        private readonly ITerminalService _terminalService;
        private readonly IUserService _userService;
        private readonly IMediaService _mediaService;
        private readonly IAccountingService _accountingService;
        private readonly IUserActivityService _userActivityService;
        private readonly ISettingService _settingService;
        private readonly ICostExpenditureBillService _costExpenditureBillService;
        private readonly ICostContractBillService _costContractBillService;
        private readonly ICommonBillService _commonBillService;
        private readonly IRedLocker _locker;
        private readonly IExportManager _exportManager;

        public CostExpenditureController(
            IWorkContext workContext,
            IStoreContext storeContext,
            ILogger loggerService,
            IPrintTemplateService printTemplateService,
            ITerminalService terminalService,
            IMediaService mediaService,
            IAccountingService accountingService,
            IUserActivityService userActivityService,
            ISettingService settingService,
            ICostExpenditureBillService costExpenditureBillService,
            ICostContractBillService costContractBillService,
            IUserService userService,
            INotificationService notificationService,
            ICommonBillService commonBillService,
            IRedLocker locker,
            IExportManager exportManager
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            _printTemplateService = printTemplateService;
            _terminalService = terminalService;
            _mediaService = mediaService;
            _accountingService = accountingService;
            _userService = userService;
            _userActivityService = userActivityService;
            _settingService = settingService;
            _costExpenditureBillService = costExpenditureBillService;
            _costContractBillService = costContractBillService;
            _commonBillService = commonBillService;
            _locker = locker;
            _exportManager = exportManager;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        /// <summary>
        /// 费用支出单列表
        /// </summary>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.ExpenseExpenditureView)]
        public IActionResult List(int? customerId, string customerName, int? employeeId, string billNumber = "", bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? showReverse = null, bool sortByAuditedTime = false, int pagenumber = 0)
        {

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var model = new CostExpenditureBillListModel
            {
                CustomerId = customerId ?? 0,
                CustomerName = customerName,

                //员工
                Employees = BindUserSelection(_userService.BindUserList, curStore, ""),
                EmployeeId = employeeId ?? null
            };

            //默认收款账户动态列
            var defaultAcc = _accountingService.GetDefaultAccounting(curStore?.Id ?? 0, BillTypeEnum.CostExpenditureBill);
            model.DynamicColumns = defaultAcc?.Item4?.OrderBy(s => s.Key).Select(s => s.Value).ToList();

            model.BillNumber = billNumber;
            model.AuditedStatus = auditedStatus;
            model.StartTime = (startTime == null) ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")) : startTime;
            model.EndTime = (endTime == null) ? DateTime.Now.AddDays(1) : endTime;
            model.ShowReverse = showReverse;
            model.SortByAuditedTime = sortByAuditedTime;

            //获取分页
            var bills = _costExpenditureBillService.GetAllCostExpenditureBills(
                curStore?.Id ?? 0,
                 curUser.Id,
                employeeId,
                customerId,
                billNumber,
                auditedStatus,
                model.StartTime, //?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")),
                model.EndTime, //?? DateTime.Now.AddDays(1),
                showReverse,
                sortByAuditedTime,
                null,
                null,
                null,
                pagenumber, 30);

            model.PagingFilteringContext.LoadPagedList(bills);

            #region 查询需要关联其他表的数据

            var allUsers = _userService.GetUsersDictsByIds(curStore.Id, bills.Select(b => b.EmployeeId).Distinct().ToArray());

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
            var allTerminal = _terminalService.GetTerminalsByIds(curStore.Id, custorerIds.Distinct().ToArray());

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
            var allAccountingOptions = _accountingService.GetAccountingOptionsByIds(curStore.Id, accountingOptionIds.Distinct().ToArray());
            #endregion

            model.Lists = bills.OrderByDescending(b => b.CreatedOnUtc).Select(b =>
              {
                  var m = b.ToModel<CostExpenditureBillModel>();

                  //员工
                  m.EmployeeName = allUsers.Where(aw => aw.Key == m.EmployeeId).Select(aw => aw.Value).FirstOrDefault();

                  //客户名称
                  //var terminal = allTerminal.Where(tm => tm.Id == m.CustomerId).FirstOrDefault();
                  //m.CustomerName = terminal == null ? "" : terminal.Name;

                  m.CostExpenditureBillAccountings = defaultAcc?.Item4?.OrderBy(sb => sb.Key).Select(sb =>
                  {
                      var acc = b.CostExpenditureBillAccountings.Where(a => a?.AccountingOption?.ParentId == sb.Key).FirstOrDefault();
                      return new CostExpenditureBillAccountingModel()
                      {
                          Name = acc?.AccountingOption?.Name,
                          AccountingOptionId = acc?.AccountingOptionId ?? 0,
                          CollectionAmount = acc?.CollectionAmount ?? 0
                      };
                  }).ToList();

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

            if (customerId.HasValue)
            {
                model.Lists = model.Lists.Where(c => c.CustomerId == customerId).ToList();
            }

            return View(model);
        }

        /// <summary>
        /// 添加费用支出单
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.ExpenseExpenditureSave)]
        public IActionResult Create(int? store)
        {
            var model = new CostExpenditureBillModel
            {
                CreatedOnUtc = DateTime.Now
            };

            model.BillTypeEnumId = (int)BillTypeEnum.CostExpenditureBill;

            //默认账户设置
            var defaultAcc = _accountingService.GetDefaultAccounting(curStore?.Id ?? 0, BillTypeEnum.CostExpenditureBill);
            model.CostExpenditureBillAccountings.Add(new CostExpenditureBillAccountingModel()
            {
                AccountingOptionId = defaultAcc?.Item1?.Id ?? 0,
                CollectionAmount = 0,
                Name = defaultAcc.Item1?.Name,
                AccountCodeTypeId = defaultAcc?.Item1?.AccountCodeTypeId ?? 0
            });


            //员工
            model.Employees = BindUserSelection(_userService.BindUserList, curStore, "");
            model.EmployeeId = -1;

            return View(model);
        }

        /// <summary>
        /// 编辑费用支出单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.ExpenseExpenditureView)]
        public IActionResult Edit(int id = 0)
        {

            var model = new CostExpenditureBillModel
            {
                BillTypeEnumId = (int)BillTypeEnum.CostExpenditureBill
            };


            var cashReceiptBill = _costExpenditureBillService.GetCostExpenditureBillById(curStore.Id, id, true);
            if (cashReceiptBill == null)
            {
                return RedirectToAction("List");
            }

            if (cashReceiptBill != null)
            {
                if (cashReceiptBill.StoreId != curStore.Id)
                {
                    return RedirectToAction("List");
                }

                model = cashReceiptBill.ToModel<CostExpenditureBillModel>();
                model.BillBarCode = _mediaService.GenerateBarCodeForBase64(cashReceiptBill.BillNumber, 150, 50);
                model.Items = cashReceiptBill.Items.Select(c => c.ToModel<CostExpenditureItemModel>()).ToList();
                var cust = _terminalService.GetTerminalById(curStore.Id, cashReceiptBill.TerminalId);
                model.CustomerId = cashReceiptBill.TerminalId;
                model.CustomerName = cust != null ? cust.Name : "";
                model.CustomerCode = cust != null ? cust.Code : "";
            }
            var defaultAcc = _accountingService.GetDefaultAccounting(curStore?.Id ?? 0, BillTypeEnum.CostExpenditureBill);
            model.CostExpenditureBillAccountings = cashReceiptBill.CostExpenditureBillAccountings.Select(s =>
            {
                var m = s.ToAccountModel<CostExpenditureBillAccountingModel>();
                m.Name = _accountingService.GetAccountingOptionName(curStore.Id, s.AccountingOptionId);
                return m;
            }).ToList();

            //员工
            model.Employees = BindUserSelection(_userService.BindUserList, curStore, "");
            //制单人
            //var mu = _userService.GetUserById(curStore.Id, cashReceiptBill.MakeUserId);
            //model.MakeUserName = mu != null ? (mu.UserRealName + " " + cashReceiptBill.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss")) : "";
            var mu = string.Empty;
            if (cashReceiptBill.MakeUserId > 0)
            {
                mu = _userService.GetUserName(curStore.Id, cashReceiptBill.MakeUserId);
            }
            model.MakeUserName = mu + " " + cashReceiptBill.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss");

            //审核人
            //var au = _userService.GetUserById(curStore.Id, cashReceiptBill.AuditedUserId ?? 0);
            //model.AuditedUserName = au != null ? (au.UserRealName + " " + (cashReceiptBill.AuditedDate.HasValue ? cashReceiptBill.AuditedDate.Value.ToString("yyyy/MM/dd HH:mm:ss") : "")) : "";
            var au = string.Empty;
            if (cashReceiptBill.AuditedUserId != null && cashReceiptBill.AuditedUserId > 0)
            {
                au = _userService.GetUserName(curStore.Id, cashReceiptBill.AuditedUserId ?? 0);
            }
            model.AuditedUserName = au + " " + (cashReceiptBill.AuditedDate.HasValue ? cashReceiptBill.AuditedDate.Value.ToString("yyyy/MM/dd HH:mm:ss") : "");

            return View(model);
        }

        #region 单据项目

        /// <summary>
        /// 异步获取费用支出单项目
        /// </summary>
        /// <param name="billId"></param>
        /// <returns></returns>
        public async Task<JsonResult> AsyncCostExpenditureItems(int billId)
        {
            return await Task.Run(() =>
            {
                var items = _costExpenditureBillService.GetCostExpenditureItemList(billId).Select(o =>
                  {
                      var m = o.ToModel<CostExpenditureItemModel>();
                      //var u = _userService.GetUserById(curStore.Id, m.CustomerId);
                      var u = _terminalService.GetTerminalById(curStore.Id, m.CustomerId);
                      var c = _costContractBillService.GetCostContractBillById(curStore.Id, m.CostContractId, false);

                      m.AccountingOptionName = _accountingService.GetAccountingOptionName(curStore.Id, m.AccountingOptionId);
                      m.CustomerName = u != null ? u.Name : "";
                      m.CostContractName = c != null ? c.BillNumber : "";

                      return m;

                  }).ToList();

                return Json(new
                {
                    Success = true,
                    total = items.Count,
                    rows = items
                });
            });
        }

        /// <summary>
        /// 创建/更新费用支出单
        /// </summary>
        /// <param name="data"></param>
        /// <param name="billId"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.ExpenseExpenditureSave)]

        public async Task<JsonResult> CreateOrUpdate(CostExpenditureUpdateModel data, int? billId, bool doAudit = true)
        {

            try
            {
                CostExpenditureBill costExpenditureBill = new CostExpenditureBill();

                if (data == null || data.Items == null)
                {
                    return Warning("请录入数据.");
                }

                if (PeriodLocked(DateTime.Now))
                {
                    return Warning("锁账期间,禁止业务操作.");
                }

                if (PeriodClosed(DateTime.Now))
                {
                    return Warning("会计期间已结账,禁止业务操作.");
                }

                #region 单据验证
                if (billId.HasValue && billId.Value != 0)
                {
                    costExpenditureBill = _costExpenditureBillService.GetCostExpenditureBillById(curStore.Id, billId.Value, true);

                    //公共单据验证
                    var commonBillChecking = BillChecking<CostExpenditureBill, CostExpenditureItem>(costExpenditureBill, BillStates.Draft);
                    if (commonBillChecking.Value != null)
                    {
                        return commonBillChecking;
                    }
                }


                //客户验证(费用合同客户必须等于当前项目明细客户)
                if (data.Items != null && data.Items.Count > 0)
                {
                    foreach (var it in data.Items)
                    {
                        if (it.CostContractId > 0)
                        {
                            var costContract = _costContractBillService.GetCostContractBillById(curStore.Id, it.CostContractId, false);

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
                var accountings = _accountingService.GetAllAccountingOptions(curStore.Id, 0, true);
                var dataTo = data.ToEntity<CostExpenditureBillUpdate>();
                dataTo.Operation = (int)OperationEnum.PC;
                if (data.Accounting == null)
                {
                    return Warning("没有默认的付款账号");
                }

                dataTo.Accounting = data.Accounting.Select(ac =>
                {
                    return ac.ToAccountEntity<CostExpenditureBillAccounting>();
                }).ToList();

                dataTo.Items = data.Items.Where(it => it.AccountingOptionId > 0)
                    .Select(it =>
                    {
                        return it.ToEntity<CostExpenditureItem>();
                    }).ToList();

                //RedLock
                var result = await _locker.PerformActionWithLockAsync(LockKey(data),
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(1),
                    () => _costExpenditureBillService.BillCreateOrUpdate(curStore.Id, curUser.Id, billId, costExpenditureBill, dataTo.Accounting, accountings, dataTo, dataTo.Items, _userService.IsAdmin(curStore.Id, curUser.Id), doAudit));
                return Json(result);

            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("CreateOrUpdate", Resources.Bill_CreateOrUpdateFailed, curUser.Id);
                _notificationService.SuccessNotification(Resources.Bill_CreateOrUpdateFailed);
                return Error(ex.Message);
            }
        }

        #endregion

        /// <summary>
        /// 审核费用支出
        /// </summary>
        /// <param name="billId"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.ExpenseExpenditureApproved)]
        public async Task<JsonResult> Auditing(int? id)
        {
            try
            {
                var bill = new CostExpenditureBill() { StoreId = curStore?.Id ?? 0 };

                #region 验证

                if (!id.HasValue)
                {
                    return Warning("参数错误.");
                }
                else
                {
                    bill = _costExpenditureBillService.GetCostExpenditureBillById(curStore.Id, id.Value, true);
                    if (bill.AuditedStatus)
                    {
                        return Warning("单据已审核，请刷新页面.");
                    }
                }

                //公共单据验证
                var commonBillChecking = BillChecking<CostExpenditureBill, CostExpenditureItem>(bill, BillStates.Audited);
                if (commonBillChecking.Value != null)
                {
                    return commonBillChecking;
                }

                #endregion

                //RedLock
                var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill),
                      TimeSpan.FromSeconds(30),
                      TimeSpan.FromSeconds(10),
                      TimeSpan.FromSeconds(1),
                      () => _costExpenditureBillService.Auditing(curUser.Id, bill));
                return Json(result);

            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("Auditing", "单据审核失败", curUser.Id);
                _notificationService.SuccessNotification("单据审核失败");
                return Error(ex.Message);
            }
        }

        /// <summary>
        /// 红冲
        /// </summary>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.ExpenseExpenditureReverse)]
        public async Task<JsonResult> Reverse(int? id)
        {
            try
            {
                var bill = new CostExpenditureBill() { StoreId = curStore?.Id ?? 0 };

                #region 验证

                if (PeriodClosed(DateTime.Now))
                {
                    return Warning("系统当月已经结转，不允许红冲");
                }

                if (!id.HasValue)
                {
                    return Warning("参数错误.");
                }
                else
                {
                    bill = _costExpenditureBillService.GetCostExpenditureBillById(curStore.Id, id.Value, true);
                    if (bill.ReversedStatus)
                    {
                        return Warning("单据已红冲，请刷新页面.");
                    }
                }

                //公共单据验证
                var commonBillChecking = BillChecking<CostExpenditureBill, CostExpenditureItem>(bill, BillStates.Reversed);
                if (commonBillChecking.Value != null)
                {
                    return commonBillChecking;
                }

                #endregion

                //RedLock
                var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill),
                      TimeSpan.FromSeconds(30),
                      TimeSpan.FromSeconds(10),
                      TimeSpan.FromSeconds(1),
                      () => _costExpenditureBillService.Reverse(curUser.Id, bill));
                return Json(result);

            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("Reverse", "单据红冲失败", curUser.Id);
                _notificationService.SuccessNotification("单据红冲失败");
                return Error(ex.Message);
            }
        }
        [AuthCode((int)AccessGranularityEnum.ExpenseExpenditurePrint)]
        public JsonResult PrintSetting()
        {
            var printTemplates = _printTemplateService.GetAllPrintTemplates(curStore.Id).ToList();
            var printTemplate = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.CostExpenditureBill).FirstOrDefault();
            //获取打印设置
            var pCPrintSetting = _settingService.LoadSetting<PCPrintSetting>(_storeContext.CurrentStore.Id);
            var settings = new object();
            if (pCPrintSetting != null)
            {
                settings = new
                {
                    PaperWidth = (printTemplate?.PaperWidth == 0 || printTemplate?.PaperHeight == 0) ? pCPrintSetting.PaperWidth : printTemplate.PaperWidth,
                    PaperHeight = (printTemplate?.PaperWidth == 0 || printTemplate?.PaperHeight == 0) ? pCPrintSetting.PaperHeight : printTemplate.PaperHeight,
                    BorderType = pCPrintSetting.BorderType,
                    MarginTop = pCPrintSetting.MarginTop,
                    MarginBottom = pCPrintSetting.MarginBottom,
                    MarginLeft = pCPrintSetting.MarginLeft,
                    MarginRight = pCPrintSetting.MarginRight,
                    IsPrintPageNumber = pCPrintSetting.IsPrintPageNumber,
                    PrintHeader = pCPrintSetting.PrintHeader,
                    PrintFooter = pCPrintSetting.PrintFooter,
                    FixedRowNumber = pCPrintSetting.FixedRowNumber,
                    PrintSubtotal = pCPrintSetting.PrintSubtotal,
                    PrintPort = pCPrintSetting.PrintPort
                };
                return Successful("", settings);
            }
            return Successful("", null);

        }
        /// <summary>
        /// 打印
        /// </summary>
        /// <param name="selectData"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.ExpenseExpenditurePrint)]
        public JsonResult Print(int type, string selectData, int? customerId, string customerName, int? employeeId, string billNumber = "", bool? auditedStatus = false, DateTime? startTime = null, DateTime? endTime = null, bool? showReverse = false, bool? sortByAuditedTime = false)
        {
            try
            {

                bool fg = true;
                string errMsg = string.Empty;

                #region 查询打印数据

                IList<CostExpenditureBill> costExpenditureBills = new List<CostExpenditureBill>();
                string datas = string.Empty;
                //默认选择
                type = type == 0 ? 1 : type;
                if (type == 1)
                {
                    if (!string.IsNullOrEmpty(selectData))
                    {
                        List<string> ids = selectData.Split(',').ToList();
                        foreach (var id in ids)
                        {
                            CostExpenditureBill costExpenditureBill = _costExpenditureBillService.GetCostExpenditureBillById(curStore.Id, int.Parse(id), true);
                            if (costExpenditureBill != null)
                            {
                                costExpenditureBills.Add(costExpenditureBill);
                            }
                        }
                    }
                }
                else if (type == 2)
                {
                    costExpenditureBills = _costExpenditureBillService.GetAllCostExpenditureBills(
                                curStore?.Id ?? 0,
                                 curUser.Id,
                                employeeId,
                                customerId,
                                billNumber,
                                auditedStatus,
                                startTime,
                                endTime,
                                showReverse,
                                sortByAuditedTime, null, null, null);
                }


                #endregion

                #region 修改数据
                if (costExpenditureBills != null && costExpenditureBills.Count > 0)
                {
                    //using (var scope = new TransactionScope())
                    //{

                    //    scope.Complete();
                    //}
                    #region 修改单据表打印数
                    foreach (var d in costExpenditureBills)
                    {
                        d.PrintNum = (d.PrintNum ?? 0) + 1;
                        _costExpenditureBillService.UpdateCostExpenditureBill(d);
                    }
                    #endregion
                }

                //获取打印模板
                var printTemplates = _printTemplateService.GetAllPrintTemplates(curStore.Id).ToList();
                var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.CostExpenditureBill).Select(a => a.Content).FirstOrDefault();

                //获取打印设置
                var pCPrintSetting = _settingService.LoadSetting<PCPrintSetting>(curStore.Id);

                //填充打印数据
                foreach (var d in costExpenditureBills)
                {

                    StringBuilder sb = new StringBuilder();
                    sb.Append(content);

                    #region theadid
                    sb.Replace("@业务员", _userService.GetUserName(curStore.Id, d.EmployeeId));
                    sb.Replace("@打印时间", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));

                    #endregion

                    #region tbodyid
                    //明细
                    //获取 tbody 中的行
                    int beginTbody = sb.ToString().IndexOf("<tbody>") + 7;
                    int endTbody = sb.ToString().IndexOf("</tbody>");
                    string tbodytr = sb.ToString()[beginTbody..endTbody];

                    if (d.Items != null && d.Items.Count > 0)
                    {
                        //1.先删除明细第一行
                        sb.Remove(beginTbody, endTbody - beginTbody);
                        int i = 0;
                        foreach (var item in d.Items)
                        {
                            int index = sb.ToString().IndexOf("</tbody>");
                            i++;
                            StringBuilder sb2 = new StringBuilder();
                            sb2.Append(tbodytr);

                            sb2.Replace("#序号", i.ToString());

                            AccountingOption accountingOption = _accountingService.GetAccountingOptionById(item.AccountingOptionId);
                            if (accountingOption != null)
                            {
                                sb2.Replace("#费用类别", accountingOption.Name);
                            }
                            sb2.Replace("#金额", item.Amount.ToString());

                            Terminal terminal = _terminalService.GetTerminalById(curStore.Id, item.CustomerId);
                            if (terminal != null)
                            {
                                sb.Replace("#客户", terminal.Name);
                            }
                            sb2.Replace("#备注", item.Remark);

                            sb.Insert(index, sb2);

                        }

                        sb.Replace("@合计", d.Items.Sum(s => s.Amount ?? 0).ToString());

                    }
                    #endregion

                    #region tfootid

                    sb.Replace("@付款日期", d.PayDate == null ? "" : ((DateTime)d.PayDate).ToString("yyyy/MM/dd HH:mm:ss"));
                    sb.Replace("@付款方式", "");
                    sb.Replace("@支出金额", "");
                    sb.Replace("@日期", d.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss"));

                    User makeUser = _userService.GetUserById(curStore.Id, d.MakeUserId);
                    if (makeUser != null)
                    {
                        sb.Replace("@制单", makeUser.UserRealName);
                    }
                    sb.Replace("@备注", d.Remark);

                    User auditedUser = _userService.GetUserById(curStore.Id, d.AuditedUserId ?? 0);
                    if (auditedUser != null)
                    {
                        sb.Replace("@审核人", auditedUser.UserRealName);
                    }

                    #endregion

                    datas += sb;
                }

                if (fg)
                {
                    return Successful("打印成功", datas);
                }
                else
                {
                    return Warning(errMsg);
                }
                #endregion

            }
            catch (Exception ex)
            {
                return Warning(ex.ToString());
            }
        }

        //导出
        [AuthCode((int)AccessGranularityEnum.ExpenseExpenditureExport)]
        public FileResult Export(int type, string selectData, int? customerId, string customerName, int? employeeId, string billNumber = "", bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? showReverse = null, bool? sortByAuditedTime = null)
        {

            #region 查询导出数据
            IList<CostExpenditureBill> costExpenditureBills = new List<CostExpenditureBill>();

            if (type == 1)
            {
                if (!string.IsNullOrEmpty(selectData))
                {
                    List<string> ids = selectData.Split(',').ToList();
                    foreach (var id in ids)
                    {
                        CostExpenditureBill costExpenditureBill = _costExpenditureBillService.GetCostExpenditureBillById(curStore.Id, int.Parse(id), true);
                        if (costExpenditureBill != null)
                        {
                            costExpenditureBills.Add(costExpenditureBill);
                        }
                    }
                }
            }
            else if (type == 2)
            {
                costExpenditureBills = _costExpenditureBillService.GetAllCostExpenditureBills(
                            curStore?.Id ?? 0,
                             curUser.Id,
                            employeeId,
                            customerId,
                            billNumber,
                            auditedStatus,
                            startTime,
                            endTime,
                            showReverse,
                            sortByAuditedTime,
                            null, null, null);
            }

            #endregion

            #region 导出
            var ms = _exportManager.ExportCostExpenditureBillToXlsx(costExpenditureBills, curStore.Id);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "费用支出单.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "费用支出单.xlsx");
            }
            #endregion

        }
    }
}