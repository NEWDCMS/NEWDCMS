using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Finances;
using DCMS.Core.Domain.Users;
using DCMS.Services.Common;
using DCMS.Services.Configuration;
using DCMS.Services.ExportImport;
using DCMS.Services.Finances;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Products;
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
    /// 用于财务收入管理（其它收入）
    /// </summary>
    public class FinancialIncomeController : BasePublicController
    {
        private readonly IPrintTemplateService _printTemplateService;
        private readonly ITerminalService _terminalService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IUserService _userService;
        private readonly IMediaService _mediaService;
        private readonly IAccountingService _accountingService;
        private readonly IUserActivityService _userActivityService;
        private readonly ISettingService _settingService;
        private readonly IFinancialIncomeBillService _financialIncomeBillService;
        private readonly IRedLocker _locker;
        private readonly IExportManager _exportManager;
        private readonly ICommonBillService _commonBillService;


        public FinancialIncomeController(
            IWorkContext workContext,
            IStoreContext storeContext,
            ILogger loggerService,
            IPrintTemplateService printTemplateService,
            ITerminalService terminalService,
            IManufacturerService manufacturerService,
            IUserService userService,
            IMediaService mediaService,
            IAccountingService accountingService,
            IUserActivityService userActivityService,
            ISettingService settingService,
            IFinancialIncomeBillService financialIncomeBillService,
            INotificationService notificationService,
            IRedLocker locker,
            IExportManager exportManager,
            ICommonBillService commonBillService
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            _printTemplateService = printTemplateService;
            _terminalService = terminalService;
            _manufacturerService = manufacturerService;
            _userService = userService;
            _mediaService = mediaService;
            _accountingService = accountingService;
            _userActivityService = userActivityService;
            _settingService = settingService;
            _financialIncomeBillService = financialIncomeBillService;
            _locker = locker;
            _exportManager = exportManager;
            _commonBillService = commonBillService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        /// <summary>
        /// 财务收入单列表
        /// </summary>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.OtherIncomeView)]
        public IActionResult List(int? salesmanId, int? customerId, string customerName, int? manufacturerId, string manufacturerName, string billNumber = "", bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? showReverse = null, bool? sortByAuditedTime = null, string remark = "", int pagenumber = 0)
        {

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var model = new FinancialIncomeBillListModel
            {
                CustomerId = customerId ?? 0,
                CustomerName = customerName,
                ManufacturerId = manufacturerId ?? 0,
                ManufacturerName = manufacturerName,

                //业务员
                Salesmans = BindUserSelection(_userService.BindUserList, curStore, ""),
                SalesmanId = salesmanId ?? null
            };

            //默认收款账户动态列
            var defaultAcc = _accountingService.GetDefaultAccounting(curStore?.Id ?? 0, BillTypeEnum.FinancialIncomeBill);
            model.DynamicColumns = defaultAcc?.Item4?.OrderBy(s => s.Key).Select(s => s.Value).ToList();

            model.BillNumber = billNumber;
            model.AuditedStatus = auditedStatus;
            model.StartTime = (startTime == null) ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")) : startTime;
            model.EndTime = (endTime == null) ? DateTime.Now.AddDays(1) : endTime;
            model.ShowReverse = showReverse;
            model.SortByAuditedTime = sortByAuditedTime;
            model.Remark = remark;

            //获取分页
            var bills = _financialIncomeBillService.GetAllFinancialIncomeBills(
               curStore?.Id ?? 0,
                curUser.Id,
               salesmanId,
               customerId,
               manufacturerId,
               billNumber,
               auditedStatus,
               model.StartTime, //?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")),
               model.EndTime, //?? DateTime.Now.AddDays(1),
               showReverse,
               sortByAuditedTime,
               remark,
               pagenumber, 30);
            model.PagingFilteringContext.LoadPagedList(bills);

            #region 查询需要关联其他表的数据

            var allUsers = _userService.GetUsersDictsByIds(curStore.Id, bills.Select(b => b.SalesmanId).Distinct().ToArray());

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
            var allTerminals = _terminalService.GetTerminalsByIds(curStore.Id, terminalIds.Distinct().ToArray());

            List<int> manufacturerIds = new List<int>();
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
            var allManufacturers = _manufacturerService.GetManufacturersByIds(curStore.Id, manufacturerIds.Distinct().ToArray());
            #endregion

            model.Items = bills.OrderByDescending(b => b.CreatedOnUtc).Select(b =>
            {
                var m = b.ToModel<FinancialIncomeBillModel>();

                //业务员
                m.SalesmanName = allUsers?.Where(aw => aw.Key == m.SalesmanId).Select(aw => aw.Value).FirstOrDefault();

                if (b.TerminalId > 0)
                {
                    m.TerminalName = allTerminals?.Where(am => am.Id == b.TerminalId).FirstOrDefault()?.Name;
                }

                if (b.ManufacturerId > 0)
                {
                    m.ManufacturerName = allManufacturers?.Where(am => am.Id == b.ManufacturerId).FirstOrDefault()?.Name;
                }

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

                m.Items = b.Items.Select(sb =>
                {
                    var it = sb.ToModel<FinancialIncomeItemModel>();
                    //var accountingOption2 = allAccountingOptions.Where(al => al.Id == sb.AccountingOptionId).FirstOrDefault();
                    //it.AccountingOptionName = accountingOption2 == null ? "" : accountingOption2.Name;
                    return it;
                }).ToList();

                return m;
            }).ToList();


            return View(model);
        }

        /// <summary>
        /// 添加财务收入单
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.OtherIncomeSave)]
        public IActionResult Create(int? store)
        {
            var model = new FinancialIncomeBillModel
            {
                CreatedOnUtc = DateTime.Now
            };

            model.BillTypeEnumId = (int)BillTypeEnum.FinancialIncomeBill;

            //业务员
            model.Salesmans = BindUserSelection(_userService.BindUserList, curStore, "");

            //当前用户为业务员时默认绑定
            if (curUser.IsSalesman())
            {
                model.SalesmanId = curUser.Id;
            }

            //默认账户设置
            var defaultAcc = _accountingService.GetDefaultAccounting(curStore?.Id ?? 0, BillTypeEnum.FinancialIncomeBill);
            model.FinancialIncomeBillAccountings.Add(new FinancialIncomeBillAccountingModel()
            {
                AccountingOptionId = defaultAcc?.Item1?.Id ?? 0,
                CollectionAmount = 0,
                Name = defaultAcc.Item1?.Name,
                AccountCodeTypeId = defaultAcc?.Item1?.AccountCodeTypeId ?? 0
            });

            return View(model);
        }

        /// <summary>
        /// 编辑财务收入单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.OtherIncomeView)]
        public IActionResult Edit(int id = 0)
        {
            var model = new FinancialIncomeBillModel
            {
                BillTypeEnumId = (int)BillTypeEnum.FinancialIncomeBill
            };

            var financialIncomeBill = _financialIncomeBillService.GetFinancialIncomeBillById(curStore.Id, id, true);
            if (financialIncomeBill == null)
            {
                return RedirectToAction("List");
            }

            if (financialIncomeBill != null)
            {
                //只能操作当前经销商数据
                if (financialIncomeBill.StoreId != curStore.Id)
                {
                    return RedirectToAction("List");
                }

                model = financialIncomeBill.ToModel<FinancialIncomeBillModel>();
                model.BillBarCode = _mediaService.GenerateBarCodeForBase64(financialIncomeBill.BillNumber, 150, 50);
                model.Items = financialIncomeBill.Items.Select(c => c.ToModel<FinancialIncomeItemModel>()).ToList();
            }

            //获取默认收款账户
            var defaultAcc = _accountingService.GetDefaultAccounting(curStore?.Id ?? 0, BillTypeEnum.FinancialIncomeBill);
            model.FinancialIncomeBillAccountings = financialIncomeBill.FinancialIncomeBillAccountings.Select(s =>
            {
                var m = s.ToAccountModel<FinancialIncomeBillAccountingModel>();
                m.Name = _accountingService.GetAccountingOptionName(curStore.Id, s.AccountingOptionId);
                return m;
            }).ToList();

            //业务员
            model.Salesmans = BindUserSelection(_userService.BindUserList, curStore, "");

            //制单人

            var mu = string.Empty;
            if (financialIncomeBill.MakeUserId > 0)
            {
                mu = _userService.GetUserName(curStore.Id, financialIncomeBill.MakeUserId);
            }
            model.MakeUserName = mu + " " + financialIncomeBill.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss");

            //审核人
            var au = string.Empty;
            if (financialIncomeBill.AuditedUserId != null && financialIncomeBill.AuditedUserId > 0)
            {
                au = _userService.GetUserName(curStore.Id, financialIncomeBill.AuditedUserId ?? 0);
            }
            model.AuditedUserName = au + " " + (financialIncomeBill.AuditedDate.HasValue ? financialIncomeBill.AuditedDate.Value.ToString("yyyy/MM/dd HH:mm:ss") : "");

            return View(model);
        }

        #region 单据项目

        /// <summary>
        /// 异步获取财务收入单项目
        /// </summary>
        /// <param name="billId"></param>
        /// <returns></returns>
        public async Task<JsonResult> AsyncFinancialIncomeItems(int billId)
        {
            return await Task.Run(() =>
            {
                var items = _financialIncomeBillService.GetFinancialIncomeItemList(billId).Select(o =>
                  {
                      var m = o.ToModel<FinancialIncomeItemModel>();
                      m.AccountingOptionName = _accountingService.GetAccountingOptionName(curStore.Id, m.AccountingOptionId);
                      switch (m.CustomerOrManufacturerType)
                      {
                          case (int)TerminalDataType.Manufacturer:
                              m.ManufacturerName = _manufacturerService.GetManufacturerName(curStore.Id, m.CustomerOrManufacturerId);
                              break;
                          case (int)TerminalDataType.Terminal:
                              m.CustomerName = _terminalService.GetTerminalName(curStore.Id, m.CustomerOrManufacturerId);
                              break;
                          default:
                              break;
                      }
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
        /// 创建/更新财务收入单
        /// </summary>
        /// <param name="data"></param>
        /// <param name="billId"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.OtherIncomeSave)]
        public async Task<JsonResult> CreateOrUpdate(FinancialIncomeUpdateModel data, int? billId,bool doAudit = true)
        {
            try
            {
                var bill = new FinancialIncomeBill();

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
                    bill = _financialIncomeBillService.GetFinancialIncomeBillById(curStore.Id, billId.Value, true);

                    //公共单据验证
                    var commonBillChecking = BillChecking<FinancialIncomeBill, FinancialIncomeItem>(bill, BillStates.Draft);
                    if (commonBillChecking.Value != null)
                    {
                        return commonBillChecking;
                    }
                }
                #endregion

                #region 验证预付款
                if (data.Accounting != null)
                {
                    var useAdvancePaymentAmount = _commonBillService.CalcManufacturerBalance(curStore.Id, data.ManufacturerId);
                    if (data.AdvancedPaymentsAmount > 0 && data.AdvancedPaymentsAmount > useAdvancePaymentAmount.AdvanceAmountBalance)
                    {
                        return Warning("预付款余额不足!");
                    }
                }
                #endregion

                #region 预收款 验证
                if (data.Accounting != null)
                {
                    //剩余预收款金额(预收账款科目下的所有子类科目)：
                    var advanceAmountBalance = _commonBillService.CalcTerminalBalance(curStore.Id, data.TerminalId);
                    if (data.AdvanceAmount > 0 && data.AdvanceAmount > advanceAmountBalance.AdvanceAmountBalance)
                    {
                        return Warning("预收款余额不足!");
                    }
                }
                #endregion


                //业务逻辑
                var accountings = _accountingService.GetAllAccountingOptions(curStore.Id, 0, true);
                var dataTo = data.ToEntity<FinancialIncomeBillUpdate>();
                if (data.Accounting == null)
                {
                    return Warning("没有默认的付款账号");
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
                var result = await _locker.PerformActionWithLockAsync(LockKey(data),
                      TimeSpan.FromSeconds(30),
                      TimeSpan.FromSeconds(10),
                      TimeSpan.FromSeconds(1),
                      () => _financialIncomeBillService.BillCreateOrUpdate(curStore.Id, curUser.Id, billId, bill, dataTo.Accounting, accountings, dataTo, dataTo.Items, _userService.IsAdmin(curStore.Id, curUser.Id), doAudit));

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
        /// 财务收入支出
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.OtherIncomeApproved)]
        public async Task<JsonResult> Auditing(int? id)
        {
            try
            {

                var bill = new FinancialIncomeBill();

                #region 验证

                if (!id.HasValue)
                {
                    return Warning("参数错误.");
                }
                else
                {
                    bill = _financialIncomeBillService.GetFinancialIncomeBillById(curStore.Id, id.Value, true);
                    if (bill.AuditedStatus)
                    {
                        return Warning("单据已审核，请刷新页面.");
                    }
                }

                //公共单据验证
                var commonBillChecking = BillChecking<FinancialIncomeBill, FinancialIncomeItem>(bill, BillStates.Audited);
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
                      () => _financialIncomeBillService.Auditing(curStore.Id, curUser.Id, bill));
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
        /// 财务收入支出 红冲
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.OtherIncomeReverse)]
        public async Task<JsonResult> Reverse(int? id)
        {
            try
            {
                var bill = new FinancialIncomeBill();

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
                    bill = _financialIncomeBillService.GetFinancialIncomeBillById(curStore.Id, id.Value, true);
                    if (bill.ReversedStatus)
                    {
                        return Warning("单据已红冲，请刷新页面.");
                    }
                }

                //公共单据验证
                var commonBillChecking = BillChecking<FinancialIncomeBill, FinancialIncomeItem>(bill, BillStates.Reversed);
                if (commonBillChecking.Value != null)
                {
                    return commonBillChecking;
                }


                //RedLock
                var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(1), () => _financialIncomeBillService.Reverse(curUser.Id, bill));
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
        [AuthCode((int)AccessGranularityEnum.OtherIncomePrint)]
        public JsonResult PrintSetting()
        {
            var printTemplates = _printTemplateService.GetAllPrintTemplates(curStore.Id).ToList();
            var printTemplate = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.FinancialIncomeBill).FirstOrDefault();
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
        [AuthCode((int)AccessGranularityEnum.OtherIncomePrint)]
        public JsonResult Print(int type, string selectData, int? salesmanId, int? customerOrManufacturerType, int? customerId, string customerName, string billNumber = "", bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? showReverse = null, bool? sortByAuditedTime = null, string remark = "")
        {
            try
            {
                bool fg = true;
                string errMsg = string.Empty;

                #region 查询打印数据

                IList<FinancialIncomeBill> financialIncomeBills = new List<FinancialIncomeBill>();
                var datas = new List<string>();
                //默认选择
                type = type == 0 ? 1 : type;
                if (type == 1)
                {
                    if (!string.IsNullOrEmpty(selectData))
                    {
                        List<string> ids = selectData.Split(',').ToList();
                        foreach (var id in ids)
                        {
                            FinancialIncomeBill financialIncomeBill = _financialIncomeBillService.GetFinancialIncomeBillById(curStore.Id, int.Parse(id), true);
                            if (financialIncomeBill != null)
                            {
                                financialIncomeBills.Add(financialIncomeBill);
                            }
                        }
                    }
                }
                else if (type == 2)
                {
                    financialIncomeBills = _financialIncomeBillService.GetAllFinancialIncomeBills(
                                curStore?.Id ?? 0,
                                 curUser.Id,
                                salesmanId,
                                customerId,
                                0,
                                billNumber,
                                auditedStatus,
                                startTime,
                                endTime,
                                showReverse,
                                sortByAuditedTime,
                                remark);
                }

                #endregion

                #region 修改数据
                if (financialIncomeBills != null && financialIncomeBills.Count > 0)
                {
                    //using (var scope = new TransactionScope())
                    //{
                    //    scope.Complete();
                    //}
                    #region 修改单据表打印数
                    foreach (var d in financialIncomeBills)
                    {
                        d.PrintNum = (d.PrintNum ?? 0) + 1;
                        _financialIncomeBillService.UpdateFinancialIncomeBill(d);
                    }
                    #endregion
                }

                //获取打印模板
                var printTemplates = _printTemplateService.GetAllPrintTemplates(curStore.Id).ToList();
                var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.FinancialIncomeBill).Select(a => a.Content).FirstOrDefault();

                //获取打印设置
                var pCPrintSetting = _settingService.LoadSetting<PCPrintSetting>(curStore.Id);

                //填充打印数据
                foreach (var d in financialIncomeBills)
                {

                    StringBuilder sb = new StringBuilder();
                    sb.Append(content);

                    #region theadid

                    User businessUser = _userService.GetUserById(curStore.Id, d.SalesmanId);
                    if (businessUser != null)
                    {
                        sb.Replace("@业务员", businessUser.UserRealName);
                        //sb.Replace("@业务电话", businessUser.MobileNumber);
                    }
                    sb.Replace("@打印时间", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                    sb.Replace("@单据编号", d.BillNumber);

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
                                sb2.Replace("#收入类别", accountingOption.Name);
                            }
                            sb2.Replace("#金额", item.Amount.ToString());

                            //string customerName = string.Empty;
                            //string manufacturerName = string.Empty;
                            //Terminal terminal = _terminalService.GetTerminalById(item.CustomerOrManufacturerId);
                            //if (terminal != null)
                            //{
                            //    customerName = terminal.Name;
                            //}
                            //Manufacturer manufacturer = _manufacturerService.GetManufacturerById(item.CustomerOrManufacturerId);
                            //if (manufacturer != null)
                            //{
                            //    manufacturerName = manufacturer.Name;
                            //}
                            string customerNameManufacturerName = string.Empty;
                            switch (item.CustomerOrManufacturerType)
                            {
                                case (int)TerminalDataType.Manufacturer:
                                    customerNameManufacturerName = _manufacturerService.GetManufacturerName(curStore.Id, item.CustomerOrManufacturerId);
                                    break;
                                case (int)TerminalDataType.Terminal:
                                    customerNameManufacturerName = _terminalService.GetTerminalName(curStore.Id, item.CustomerOrManufacturerId);
                                    break;
                                default:
                                    break;
                            }

                            sb.Replace("#客户/供应商", customerNameManufacturerName);
                            sb2.Replace("#备注", item.Remark);

                            sb.Insert(index, sb2);

                        }

                        sb.Replace("@合计", d.Items.Sum(s => s.Amount ?? 0).ToString());
                    }
                    #endregion

                    #region tfootid
                    sb.Replace("@付款日期", "");
                    sb.Replace("@付款方式", "");
                    sb.Replace("@收入金额", "");
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

                    datas.Add(sb.ToString());
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
        [AuthCode((int)AccessGranularityEnum.OtherIncomeExport)]
        public FileResult Export(int type, string selectData, int? salesmanId, int? customerOrManufacturerType, int? customerId, string customerName, string billNumber = "", bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? showReverse = null, bool? sortByAuditedTime = null, string remark = "")
        {

            #region 查询导出数据
            IList<FinancialIncomeBill> financialIncomeBills = new List<FinancialIncomeBill>();

            if (type == 1)
            {
                if (!string.IsNullOrEmpty(selectData))
                {
                    List<string> ids = selectData.Split(',').ToList();
                    foreach (var id in ids)
                    {
                        FinancialIncomeBill financialIncomeBill = _financialIncomeBillService.GetFinancialIncomeBillById(curStore.Id, int.Parse(id), true);
                        if (financialIncomeBill != null)
                        {
                            financialIncomeBills.Add(financialIncomeBill);
                        }
                    }
                }
            }
            else if (type == 2)
            {
                financialIncomeBills = _financialIncomeBillService.GetAllFinancialIncomeBills(
                            curStore?.Id ?? 0,
                             curUser.Id,
                            salesmanId,
                            customerId,
                            0,
                            billNumber,
                            auditedStatus,
                            startTime,
                            endTime,
                            showReverse,
                            sortByAuditedTime,
                            remark);
            }

            #endregion

            #region 导出
            var ms = _exportManager.ExportFinancialIncomeBillToXlsx(financialIncomeBills, curStore.Id);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "财务收入单.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "财务收入单.xlsx");
            }
            #endregion

        }
    }
}