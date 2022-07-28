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
using DCMS.Web.Framework.Extensions;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DCMS.Web.Controllers
{
    /// <summary>
    /// 用于预收款管理
    /// </summary>
    public class AdvanceReceiptController : BasePublicController
    {

        private readonly IPrintTemplateService _printTemplateService;
        private readonly ITerminalService _terminalService;
        private readonly IUserService _userService;
        private readonly IMediaService _mediaService;
        private readonly IAccountingService _accountingService;
        private readonly IUserActivityService _userActivityService;
        private readonly ISettingService _settingService;
        private readonly IAdvanceReceiptBillService _advanceReceiptBillService;
        private readonly IRedLocker _locker;
        private readonly IExportManager _exportManager;
        private readonly ICommonBillService _commonBillService;

        public AdvanceReceiptController(
            IWorkContext workContext,
            IStoreContext storeContext,
            ILogger loggerService,
            IPrintTemplateService printTemplateService,
            ITerminalService terminalService,
            IMediaService mediaService,
            IAccountingService accountingService,
            IUserActivityService userActivityService,
            ISettingService settingService,
            IAdvanceReceiptBillService advanceReceiptBillService,
            IUserService userService,
            INotificationService notificationService,
            IRedLocker locker,
            IExportManager exportManager,
            ICommonBillService commonBillService
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            _printTemplateService = printTemplateService;
            _terminalService = terminalService;
            _mediaService = mediaService;
            _accountingService = accountingService;
            _userService = userService;
            _userActivityService = userActivityService;
            _settingService = settingService;
            _advanceReceiptBillService = advanceReceiptBillService;
            _locker = locker;
            _exportManager = exportManager;
            _commonBillService = commonBillService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        /// <summary>
        /// 预收款单列表
        /// </summary>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.ReceivablesBillsView)]
        public IActionResult List(int? customerId, int? makeuserId, string customerName, int? payeer, string billNumber = "", bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? showReverse = null, bool? sortByAuditedTime = null, int? accountingOptionId = null, int pagenumber = 0)
        {

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var model = new AdvanceReceiptBillListModel();

            //默认收款账户动态列
            var defaultAcc = _accountingService.GetDefaultAccounting(curStore?.Id ?? 0, BillTypeEnum.AdvanceReceiptBill);
            model.DynamicColumns = defaultAcc?.Item4?.OrderBy(s => s.Key).Select(s => s.Value).ToList();

            //预收款账户
            model.AccountingOptions = new SelectList(defaultAcc.Item2.Select(a =>
            {
                return new SelectListItem()
                {
                    Text = a.Name,
                    Value = a.Id.ToString()
                };
            }), "Value", "Text");

            model.AccountingOptionId = accountingOptionId == 0 ? null : accountingOptionId;


            //客户
            model.CustomerId = customerId ?? 0;
            model.CustomerName = customerName;

            //收款人
            model.Payeers = BindUserSelection(_userService.BindUserList, curStore, "");
            model.Payeer = payeer ?? null;





            model.BillNumber = billNumber;
            model.AuditedStatus = auditedStatus;
            model.StartTime = (startTime == null) ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")) : startTime;
            model.EndTime = (endTime == null) ? DateTime.Now.AddDays(1) : endTime;
            model.ShowReverse = showReverse;
            model.SortByAuditedTime = sortByAuditedTime;

            //获取分页
            var bills = _advanceReceiptBillService.GetAllAdvanceReceiptBills(
                curStore?.Id ?? 0,
                makeuserId,
                customerId,
                customerName,
                payeer,
                billNumber,
                auditedStatus,
                model.StartTime, //?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")),
                model.EndTime, //?? DateTime.Now.AddDays(1),
                showReverse,
                sortByAuditedTime,
                accountingOptionId,
                null,
                null,
                pagenumber,
                30);
            model.PagingFilteringContext.LoadPagedList(bills);

            #region 查询需要关联其他表的数据

            var allUsers = _userService.GetUsersDictsByIds(curStore.Id, bills.Select(b => b.Payeer).Distinct().ToArray());
            var allTerminals = _terminalService.GetTerminalsDictsByIds(curStore.Id, bills.Select(b => b.CustomerId).Distinct().ToArray());

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
            var allAccountingOptions = _accountingService.GetAccountingOptionsByIds(curStore.Id, accountingOptionIds.ToArray());
            #endregion

            model.Items = bills.OrderByDescending(b => b.CreatedOnUtc).Select(b =>
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
                m.Items = defaultAcc?.Item4?.OrderBy(s => s.Key).Select(s =>
                {
                    var acc = b.Items.Where(a => a?.AccountingOption?.ParentId == s.Key).FirstOrDefault();
                    return new AdvanceReceiptBillAccountingModel()
                    {
                        Name = acc?.AccountingOption?.Name,
                        AccountingOptionId = acc?.AccountingOptionId ?? 0,
                        CollectionAmount = acc?.CollectionAmount ?? 0
                    };
                }).ToList();

                return m;
            }).ToList();

            return View(model);
        }

        /// <summary>
        /// 添加预收款单
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.ReceivablesBillsSave)]
        public IActionResult Create(int? store)
        {

            var model = new AdvanceReceiptBillModel
            {
                CreatedOnUtc = DateTime.Now,
                AdvanceAmount = 0,
                DiscountAmount = 0,
                //收款人
                Payeers = BindUserSelection(_userService.BindUserList, curStore, "")
            };

            model.BillTypeEnumId = (int)BillTypeEnum.AdvanceReceiptBill;

            model.Payeer = (model.Payeer ?? -1);

            //默认收款
            var defaultAcc = _accountingService.GetDefaultAccounting(curStore?.Id ?? 0, BillTypeEnum.AdvanceReceiptBill);
            model.Items.Add(new AdvanceReceiptBillAccountingModel()
            {
                Name = defaultAcc?.Item1?.Name,
                CollectionAmount = 0,
                AccountingOptionId = defaultAcc?.Item1?.Id ?? 0
            });

            //收款账户
            model.AccountingOptions = new SelectList(defaultAcc.Item2.Select(a =>
            {
                return new SelectListItem()
                {
                    Text = a.Name,
                    Value = a.Id.ToString()
                };
            }), "Value", "Text");

            model.AccountingOptionId = defaultAcc?.Item1?.Id ?? 0;

            return View(model);
        }

        /// <summary>
        /// 编辑预收款单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.ReceivablesBillsView)]
        public IActionResult Edit(int id = 0)
        {
            var model = new AdvanceReceiptBillModel();
            //获取公司配置
            var companySetting = _settingService.LoadSetting<CompanySetting>(curStore.Id);

            var bill = _advanceReceiptBillService.GetAdvanceReceiptBillById(curStore.Id, id, true);
            if (bill == null)
            {
                return RedirectToAction("List");
            }

            if (bill != null)
            {
                //只能操作当前经销商数据
                if (bill.StoreId != curStore.Id)
                {
                    return RedirectToAction("List");
                }
                model = bill.ToModel<AdvanceReceiptBillModel>();
                model.BillBarCode = _mediaService.GenerateBarCodeForBase64(bill.BillNumber, 150, 50);
            }

            //预收款账户AccountingOptions
            var defaultAcc = _accountingService.GetDefaultAccounting(curStore?.Id ?? 0, BillTypeEnum.AdvanceReceiptBill);
            model.AccountingOptions = new SelectList(defaultAcc?.Item2.Select(a =>
            {
                return new SelectListItem()
                {
                    Text = a.Name,
                    Value = a.Id.ToString()
                };
            }), "Value", "Text");
            model.AccountingOptionId = bill.AccountingOptionId;

            //收款账户
            model.Items = bill.Items
                .Where(s => s.AccountingOptionId != bill.AccountingOptionId)
                .Select(r =>
                {
                    return new AdvanceReceiptBillAccountingModel()
                    {
                        AccountingOptionId = r.AccountingOptionId,
                        CollectionAmount = r.CollectionAmount,
                        Name = defaultAcc?.Item3
                        .Where(d => d.Id == r.AccountingOptionId)
                        .Select(d => d.Name).FirstOrDefault()
                    };
                }).ToList();

            //客户名称
            model.CustomerName = _terminalService.GetTerminalName(curStore.Id, bill.CustomerId);

            //收款人
            model.Payeers = BindUserSelection(_userService.BindUserList, curStore, "");
            model.Payeer = (model.Payeer ?? -1);

            //制单人
            var mu = string.Empty;
            if (bill.MakeUserId > 0)
            {
                mu = _userService.GetUserName(curStore.Id, bill.MakeUserId);
            }
            model.MakeUserName = mu + " " + bill.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss");

            //审核人
            var au = string.Empty;
            if (bill.AuditedUserId != null && bill.AuditedUserId > 0)
            {
                au = _userService.GetUserName(curStore.Id, bill.AuditedUserId ?? 0);
            }
            model.AuditedUserName = au + " " + (bill.AuditedDate.HasValue ? bill.AuditedDate.Value.ToString("yyyy/MM/dd HH:mm:ss") : "");

            return View(model);
        }

        /// <summary>
        /// 创建/更新预收款单
        /// </summary>
        /// <param name="data"></param>
        /// <param name="billId"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.ReceivablesBillsSave)]
        public async Task<JsonResult> CreateOrUpdate(AdvanceReceiptUpdateModel data, int? billId, bool doAudit = true)
        {

            try
            {
                AdvanceReceiptBill advanceReceiptBill = new AdvanceReceiptBill();

                if (data == null)
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
                    advanceReceiptBill = _advanceReceiptBillService.GetAdvanceReceiptBillById(curStore.Id, billId.Value, false);

                    //单据不存在
                    if (advanceReceiptBill == null)
                    {
                        return Warning("单据信息不存在.");
                    }

                    //单开经销商不等于当前经销商
                    if (advanceReceiptBill.StoreId != curStore.Id)
                    {
                        return Warning("非法操作.");
                    }

                    //单据已经审核或已经红冲
                    if (advanceReceiptBill.AuditedStatus || advanceReceiptBill.ReversedStatus)
                    {
                        return Warning("非法操作，单据已审核或已红冲.");
                    }
                }

                #endregion

                //业务逻辑
                var accountings = _accountingService.GetAllAccountingOptions(curStore.Id, 0, true);
                var dataTo = data.ToEntity<AdvanceReceiptBillUpdate>();
                dataTo.Operation = (int)OperationEnum.PC;
                if (data.Accounting == null)
                {
                    return Warning("没有默认的付款账号");
                }
                dataTo.Accounting = data.Accounting.Select(ac =>
                {
                    return ac.ToAccountEntity<AdvanceReceiptBillAccounting>();
                }).ToList();

                //RedLock);
                var result = await _locker.PerformActionWithLockAsync(LockKey(data),
                   TimeSpan.FromSeconds(30),
                   TimeSpan.FromSeconds(10),
                   TimeSpan.FromSeconds(1),
                   () => _advanceReceiptBillService.BillCreateOrUpdate(curStore.Id, curUser.Id, billId, advanceReceiptBill, dataTo.Accounting, accountings, dataTo, _userService.IsAdmin(curStore.Id, curUser.Id), doAudit));
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

        /// <summary>
        /// 审核预收款单
        /// </summary>
        /// <param name="billId"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.ReceivablesBillsApproved)]
        public async Task<JsonResult> AsyncAudited(int? billId)
        {
            try
            {
                string errMsg = string.Empty;

                AdvanceReceiptBill advanceReceiptBill = new AdvanceReceiptBill();
                #region 验证
                if (!billId.HasValue)
                {
                    return Warning("参数错误.");
                }
                else
                {
                    advanceReceiptBill = _advanceReceiptBillService.GetAdvanceReceiptBillById(curStore.Id, billId.Value, true);
                    if (advanceReceiptBill.AuditedStatus)
                    {
                        return Warning("单据已审核，请刷新页面.");
                    }
                }

                if (!curUser.IsAdmin())
                    return Warning("权限不足.");

                if (advanceReceiptBill == null)
                {
                    return Warning("单据信息不存在.");
                }

                if (advanceReceiptBill.StoreId != curStore.Id)
                {
                    return Warning("非法操作.");
                }

                if (advanceReceiptBill.AuditedStatus || advanceReceiptBill.ReversedStatus)
                {
                    return Warning("重复操作.");
                }

                #endregion

                //RedLock
                string lockKey = string.Format(DCMSCachingDefaults.RedisDataReSubmitKey, Request.GetUrl(), curStore.Id, curUser.Id, CommonHelper.MD5(JsonConvert.SerializeObject(billId)));
                var result = await _locker.PerformActionWithLockAsync(lockKey,
                      TimeSpan.FromSeconds(30),
                      TimeSpan.FromSeconds(10),
                      TimeSpan.FromSeconds(1),
                      () => _advanceReceiptBillService.Auditing(curStore.Id, curUser.Id, advanceReceiptBill));
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
        [AuthCode((int)AccessGranularityEnum.ReceivablesBillsReverse)]
        public async Task<JsonResult> Reverse(int? id)
        {
            try
            {

                var bill = new AdvanceReceiptBill() { StoreId = curStore?.Id ?? 0 };

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
                    bill = _advanceReceiptBillService.GetAdvanceReceiptBillById(curStore.Id, id.Value, true);
                    if (bill.ReversedStatus)
                    {
                        return Warning("单据已红冲，请刷新页面.");
                    }
                }

                if (bill == null)
                {
                    return Warning("单据信息不存在.");
                }

                if (bill.StoreId != curStore.Id)
                {
                    return Warning("非法操作.");
                }

                if (!bill.AuditedStatus || bill.ReversedStatus)
                {
                    return Warning("非法操作，单据未审核或者重复操作.");
                }

                if (bill.Deleted)
                {
                    return Warning("单据已作废.");
                }

                if (bill.Items == null || !bill.Items.Any())
                {
                    return Warning("单据没有明细.");
                }

                if (DateTime.Now.Subtract(bill.AuditedDate ?? DateTime.Now).TotalSeconds > 86400)
                {
                    return Warning("已经审核的单据超过24小时，不允许红冲.");
                }


                #region 预收款 验证

                //if (bill.Items != null)
                //{
                //    //1.获取当前经销商 预收款科目Id
                //    int accountingOptionId = 0;
                //    IList<AccountingOption> accountingOptions = _accountingService.GetDefaultAccounts(curStore.Id);
                //    if (accountingOptions != null && accountingOptions.Count > 0)
                //    {
                //        AccountingOption accountingOption = accountingOptions.Where(a => a.AccountCodeTypeId == (int)AccountingCodeEnum.AdvanceReceipt).FirstOrDefault();
                //        accountingOptionId = (accountingOption == null) ? 0 : accountingOption.Id;
                //    }
                //    //获取用户输入 预收款金额
                //    //注意：这里不加会计科目条件，所有金额都是 预收款
                //    var advancePaymentReceipt = bill.Items.Sum(ac => ac.CollectionAmount);

                //    //用户可用 预收款金额
                //    decimal useAdvanceReceiptAmount = _commonBillService.GetUseAdvanceReceiptAmount(curStore.Id, bill.CustomerId);
                //    //如果输入预收款大于用户可用预收款
                //    if (advancePaymentReceipt > useAdvanceReceiptAmount)
                //    {
                //        return this.Warning("用户输入预收款金额：" + advancePaymentReceipt + ",大于用户可用预收款金额：" + useAdvanceReceiptAmount);
                //    }
                //}

                #endregion

                //RedLock

                var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill),
                      TimeSpan.FromSeconds(30),
                      TimeSpan.FromSeconds(10),
                      TimeSpan.FromSeconds(1),
                      () => _advanceReceiptBillService.Reverse(curUser.Id, bill));
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
        [AuthCode((int)AccessGranularityEnum.ReceivablesBillsPrint)]
        public JsonResult PrintSetting()
        {
            var printTemplates = _printTemplateService.GetAllPrintTemplates(curStore.Id).ToList();
            var printTemplate = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.AdvanceReceiptBill).FirstOrDefault();
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
        [AuthCode((int)AccessGranularityEnum.ReceivablesBillsPrint)]
        public JsonResult Print(int type, string selectData, int? customerId, string customerName, int? payeer, string billNumber = "", bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? showReverse = null, bool? sortByAuditedTime = null)
        {
            try
            {

                bool fg = true;
                string errMsg = string.Empty;

                #region 查询打印数据

                IList<AdvanceReceiptBill> advanceReceiptBills = new List<AdvanceReceiptBill>();
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
                            AdvanceReceiptBill advanceReceiptBill = _advanceReceiptBillService.GetAdvanceReceiptBillById(curStore.Id, int.Parse(id), true);
                            if (advanceReceiptBill != null)
                            {
                                advanceReceiptBills.Add(advanceReceiptBill);
                            }
                        }
                    }
                }
                else if (type == 2)
                {
                    advanceReceiptBills = _advanceReceiptBillService.GetAllAdvanceReceiptBills(
                                        curStore?.Id ?? 0,
                                         curUser.Id,
                                        customerId,
                                        customerName,
                                        payeer,
                                        billNumber,
                                        auditedStatus,
                                        startTime,
                                        endTime,
                                        showReverse,
                                        sortByAuditedTime, null);
                }


                #endregion

                #region 修改数据
                if (advanceReceiptBills != null && advanceReceiptBills.Count > 0)
                {
                    //using (var scope = new TransactionScope())
                    //{
                    //    scope.Complete();
                    //}
                    #region 修改单据表打印数
                    foreach (var d in advanceReceiptBills)
                    {
                        d.PrintNum = (d.PrintNum ?? 0) + 1;
                        _advanceReceiptBillService.UpdateAdvanceReceiptBill(d);
                    }
                    #endregion
                }

                //获取打印模板
                var printTemplates = _printTemplateService.GetAllPrintTemplates(curStore.Id).ToList();
                var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.AdvanceReceiptBill).Select(a => a.Content).FirstOrDefault();

                //获取打印设置
                var pCPrintSetting = _settingService.LoadSetting<PCPrintSetting>(curStore.Id);

                //填充打印数据
                foreach (var d in advanceReceiptBills)
                {

                    StringBuilder sb = new StringBuilder();
                    sb.Append(content);

                    #region theadid
                    //sb.Replace("@商铺名称", curStore.Name);
                    if (pCPrintSetting != null)
                    {
                        sb.Replace("@商铺名称", string.IsNullOrWhiteSpace(pCPrintSetting.StoreName) ? "&nbsp;" : pCPrintSetting.StoreName);
                    }

                    #endregion

                    #region tbodyid
                    //明细
                    //获取 tbody 中的行
                    int beginTbody = sb.ToString().IndexOf(@"<tbody id=""tbody"">") + @"<tbody id=""tbody"">".Length;
                    if (beginTbody == 17)
                    {
                        beginTbody = sb.ToString().IndexOf(@"<tbody id='tbody'>") + @"<tbody id='tbody'>".Length;
                    }
                    int endTbody = sb.ToString().IndexOf("</tbody>", beginTbody);
                    string tbodytr = sb.ToString()[beginTbody..endTbody];

                    sb.Remove(beginTbody, endTbody - beginTbody);
                    int index = sb.ToString().IndexOf("</tbody>", beginTbody);
                    StringBuilder sb2 = new StringBuilder();
                    sb2.Append(tbodytr);
                    sb2.Replace("#单据编号", d.BillNumber);
                    Terminal terminal = _terminalService.GetTerminalById(curStore.Id, d.CustomerId);
                    sb2.Replace("#客户名称", terminal.Name);
                    AccountingOption acc = _accountingService.GetAccountingOptionById(d.AccountingOptionId ?? 0);
                    sb2.Replace("#预收款账户", acc.Name);
                    sb2.Replace("#预收款金额", d.AdvanceAmount == null ? "0.00" : d.AdvanceAmount?.ToString("0.00"));
                    sb2.Replace("#优惠金额", d.DiscountAmount == null ? "0.00" : d.DiscountAmount?.ToString("0.00"));
                    sb2.Replace("#收款日期", d?.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss"));
                    sb2.Replace("#备注", d.Remark);
                    sb.Insert(index, sb2);

                    #endregion

                    #region tfootid
                    User makeUser = _userService.GetUserById(curStore.Id, d.MakeUserId);
                    if (makeUser != null)
                    {
                        sb.Replace("@制单", makeUser.UserRealName);
                    }
                    User businessUser = _userService.GetUserById(curStore.Id, d.Payeer);
                    if (businessUser != null)
                    {
                        sb.Replace("@业务员", businessUser.UserRealName);
                        sb.Replace("@业务电话", businessUser.MobileNumber);
                    }
                    sb.Replace("@打印日期", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                    sb.Replace("@支付信息", "");
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
        [AuthCode((int)AccessGranularityEnum.ReceivablesBillsExport)]
        public FileResult Export(int type, string selectData, int? customerId, string customerName, int? payeer, string billNumber = "", bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? showReverse = null, bool? sortByAuditedTime = null)
        {

            #region 查询导出数据
            IList<AdvanceReceiptBill> advanceReceiptBills = new List<AdvanceReceiptBill>();

            if (type == 1)
            {
                if (!string.IsNullOrEmpty(selectData))
                {
                    List<string> ids = selectData.Split(',').ToList();
                    foreach (var id in ids)
                    {
                        AdvanceReceiptBill advanceReceiptBill = _advanceReceiptBillService.GetAdvanceReceiptBillById(curStore.Id, int.Parse(id), true);
                        if (advanceReceiptBill != null)
                        {
                            advanceReceiptBills.Add(advanceReceiptBill);
                        }
                    }
                }
            }
            else if (type == 2)
            {
                advanceReceiptBills = _advanceReceiptBillService.GetAllAdvanceReceiptBills(
                                    curStore?.Id ?? 0,
                                     curUser.Id,
                                    customerId,
                                    customerName,
                                    payeer,
                                    billNumber,
                                    auditedStatus,
                                    startTime,
                                    endTime,
                                    showReverse,
                                    sortByAuditedTime, null);
            }

            #endregion

            #region 导出
            var ms = _exportManager.ExportAdvanceReceiptBillToXlsx(advanceReceiptBills, curStore.Id);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "预收款单.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "预收款单.xlsx");
            }
            #endregion

        }
    }
}