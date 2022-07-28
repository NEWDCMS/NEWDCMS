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
using DCMS.Services.Users;
using DCMS.ViewModel.Models.Finances;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCMS.Web.Controllers
{
    /// <summary>
    /// 用于预付款管理
    /// </summary>
    public class AdvancePaymentController : BasePublicController
    {
        private readonly IPrintTemplateService _printTemplateService;
        private readonly IUserService _userService;
        private readonly IMediaService _mediaService;
        private readonly IAccountingService _accountingService;
        private readonly IUserActivityService _userActivityService;
        private readonly ISettingService _settingService;
        private readonly IAdvancePaymentBillService _advancePaymentBillService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IRedLocker _locker;
        private readonly IExportManager _exportManager;
        private readonly ICommonBillService _commonBillService;

        public AdvancePaymentController(
            IWorkContext workContext,
            IStoreContext storeContext,
            ILogger loggerService,
            IPrintTemplateService printTemplateService,
            IMediaService mediaService,
            IAccountingService accountingService,
            IUserActivityService userActivityService,
            ISettingService settingService,
            IAdvancePaymentBillService advancePaymentBillService,
            IManufacturerService manufacturerService,
            IUserService userService,
            INotificationService notificationService,
            IRedLocker locker,
            IExportManager exportManager,
            ICommonBillService commonBillService
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            _printTemplateService = printTemplateService;
            _mediaService = mediaService;
            _accountingService = accountingService;
            _userService = userService;
            _userActivityService = userActivityService;
            _settingService = settingService;
            _advancePaymentBillService = advancePaymentBillService;
            _manufacturerService = manufacturerService;
            _locker = locker;
            _exportManager = exportManager;
            _commonBillService = commonBillService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        /// <summary>
        /// 预付款单列表
        /// </summary>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.PrepaidBillsView)]
        public IActionResult List(int? manufacturerId, int? draweer, string billNumber = "", bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? showReverse = null, bool? sortByAuditedTime = null, int? accountingOptionId = null, int pagenumber = 0)
        {
            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var model = new AdvancePaymentBillListModel();

            //默认收款账户动态列
            var defaultAcc = _accountingService.GetDefaultAccounting(curStore?.Id ?? 0, BillTypeEnum.AdvancePaymentBill);
            model.DynamicColumns = defaultAcc?.Item4?.OrderBy(s => s.Key).Select(s => s.Value).ToList();

            //预付单预付款科目
            model.AccountingOptions = new SelectList(defaultAcc.Item2.Select(a =>
            {
                return new SelectListItem()
                {
                    Text = a.Name,
                    Value = a.Id.ToString()
                };
            }), "Value", "Text");

            model.AccountingOptionId = accountingOptionId == 0 ? null : accountingOptionId;


            //付款人
            model.Draweers = BindUserSelection(_userService.BindUserList, curStore, "");
            model.Draweer = draweer ?? null;

            //供应商
            model.Manufacturers = BindManufacturerSelection(_manufacturerService.BindManufacturerList, curStore);
            model.ManufacturerId = manufacturerId ?? null;


            model.BillNumber = billNumber;
            model.AuditedStatus = auditedStatus;
            model.StartTime = (startTime == null) ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")) : startTime;
            model.EndTime = (endTime == null) ? DateTime.Now.AddDays(1) : endTime;
            model.ShowReverse = showReverse;
            model.SortByAuditedTime = sortByAuditedTime;

            //获取分页
            var bills = _advancePaymentBillService.GetAllAdvancePaymentBills(
                curStore?.Id ?? 0,
                curUser.Id,
                draweer,
                manufacturerId,
                billNumber,
                auditedStatus,
                model.StartTime, //?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")),
                model.EndTime, //?? DateTime.Now.AddDays(1),
                showReverse,
                sortByAuditedTime,
                accountingOptionId,
                pagenumber,
                30);
            model.PagingFilteringContext.LoadPagedList(bills);

            #region 查询需要关联其他表的数据

            var allUsers = _userService.GetUsersDictsByIds(curStore.Id, bills.Select(b => b.Draweer).Distinct().ToArray());
            var allManufacturer = _manufacturerService.GetManufacturerDictsByIds(curStore.Id, bills.Select(b => b.ManufacturerId).Distinct().ToArray());

            List<int> accountingOptionIds = bills.Select(b => b.AccountingOptionId ?? 0).ToList();
            var allAccountingOptions = _accountingService.GetAccountingOptionsByIds(curStore.Id, accountingOptionIds.ToArray());
            #endregion

            model.Items = bills.OrderByDescending(b => b.CreatedOnUtc).Select(b =>
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

            return View(model);
        }

        /// <summary>
        /// 添加预付款单
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.PrepaidBillsSave)]
        public IActionResult Create(int? store)
        {
            var model = new AdvancePaymentBillModel
            {
                CreatedOnUtc = DateTime.Now,
                AdvanceAmount = 0
            };

            model.BillTypeEnumId = (int)BillTypeEnum.AdvancePaymentBill;

            //付款科目
            var defaultAcc = _accountingService.GetDefaultAccounting(curStore?.Id ?? 0, BillTypeEnum.AdvancePaymentBill);
            model.Items.Add(new AdvancePaymentBillAccountingModel()
            {
                Name = defaultAcc?.Item1?.Name,
                CollectionAmount = 0,
                AccountingOptionId = defaultAcc?.Item1?.Id ?? 0
            });


            //付款账户
            model.AccountingOptions = new SelectList(defaultAcc.Item2.Select(a =>
            {
                return new SelectListItem()
                {
                    Text = a.Name,
                    Value = a.Id.ToString()
                };
            }), "Value", "Text");

            model.AccountingOptionId = defaultAcc?.Item1?.Id ?? 0;

            //付款人
            model.Draweers = BindUserSelection(_userService.BindUserList, curStore, "");
            model.Draweer = model.Draweer = -1;

            return View(model);
        }

        /// <summary>
        /// 编辑预付款单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.PrepaidBillsView)]
        public IActionResult Edit(int id = 0)
        {
            var model = new AdvancePaymentBillModel
            {
                BillTypeEnumId = (int)BillTypeEnum.AdvancePaymentBill
            };

            var bill = _advancePaymentBillService.GetAdvancePaymentBillById(curStore.Id, id, true);
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

                model = bill.ToModel<AdvancePaymentBillModel>();
                model.BillBarCode = _mediaService.GenerateBarCodeForBase64(bill.BillNumber, 150, 50);
            }

            //付款科目
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
            model.ManufacturerName = _manufacturerService.GetManufacturerName(curStore.Id, model.ManufacturerId);

            //付款人
            model.Draweers = BindUserSelection(_userService.BindUserList, curStore, "");

            //制单人
            var mu = _userService.GetUserName(curStore.Id, bill.MakeUserId);
            model.MakeUserName = mu + " " + bill.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss");

            //审核人
            var au = _userService.GetUserName(curStore.Id, bill.AuditedUserId ?? 0);
            model.AuditedUserName = au + " " + (bill.AuditedDate.HasValue ? bill.AuditedDate.Value.ToString("yyyy/MM/dd HH:mm:ss") : "");

            return View(model);
        }

        /// <summary>
        /// 创建/更新预付款单
        /// </summary>
        /// <param name="data"></param>
        /// <param name="billId"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.PrepaidBillsSave)]
        public async Task<JsonResult> CreateOrUpdate(AdvancePaymenUpdateModel data, int? billId,bool doAudit = true)
        {
            try
            {
                var bill = new AdvancePaymentBill();

                #region 单据验证
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

                if (billId.HasValue && billId.Value != 0)
                {

                    bill = _advancePaymentBillService.GetAdvancePaymentBillById(curStore.Id, billId.Value, true);

                    if (bill == null)
                    {
                        return Warning("单据信息不存在.");
                    }

                    if (bill.StoreId != curStore.Id)
                    {
                        return Warning("非法操作.");
                    }

                    if (bill.AuditedStatus || bill.ReversedStatus)
                    {
                        return Warning("非法操作，单据已审核或已红冲.");
                    }

                    if (bill.Items == null || !bill.Items.Any())
                    {
                        return Warning("单据没有明细.");
                    }
                }

                #endregion


                //业务逻辑
                var accountings = _accountingService.GetAllAccountingOptions(curStore.Id, 0, true);
                var dataTo = data.ToEntity<AdvancePaymenBillUpdate>();
                dataTo.Operation = (int)OperationEnum.PC;
                if (data.Accounting == null)
                {
                    return Warning("没有默认的付款账号");
                }
                dataTo.Accounting = data.Accounting.Select(ac =>
                {
                    return ac.ToAccountEntity<AdvancePaymentBillAccounting>();
                }).ToList();

                //RedLock
                var result = await _locker.PerformActionWithLockAsync(LockKey(data),
                  TimeSpan.FromSeconds(30),
                  TimeSpan.FromSeconds(10),
                  TimeSpan.FromSeconds(1),
                  () => _advancePaymentBillService.BillCreateOrUpdate(curStore.Id, curUser.Id, billId, bill, dataTo.Accounting, accountings, dataTo, _userService.IsAdmin(curStore.Id, curUser.Id), doAudit));
                return Json(result);


            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("CreateOrUpdate", Resources.Bill_CreateOrUpdateFailed, curUser.Id);
                _notificationService.ErrorNotification(Resources.Bill_CreateOrUpdateFailed);
                return Error(ex.Message);
            }

        }

        /// <summary>
        /// 审核预付款单
        /// </summary>
        /// <param name="billId"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.PrepaidBillsApproved)]
        public async Task<JsonResult> AsyncAudited(int? id)
        {
            try
            {
                var bill = new AdvancePaymentBill();

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
                    bill = _advancePaymentBillService.GetAdvancePaymentBillById(curStore.Id, id.Value, true);
                    if (bill.AuditedStatus)
                    {
                        return Warning("单据已审核，请刷新页面.");
                    }
                }

                if (!curUser.IsAdmin())
                    return Warning("权限不足.");

                if (bill == null)
                {
                    return Warning("单据信息不存在.");
                }

                if (bill.StoreId != curStore.Id)
                {
                    return Warning("非法操作.");
                }

                if (bill.AuditedStatus || bill.ReversedStatus)
                {
                    return Warning("非法操作，单据未审核或者重复操作.");
                }

                if (bill.Items == null || !bill.Items.Any())
                {
                    return Warning("单据没有明细.");
                }

                //RedLock

                var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(1), () => _advancePaymentBillService.Auditing(curStore.Id, curUser.Id, bill));
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
        [AuthCode((int)AccessGranularityEnum.PrepaidBillsReverse)]
        public async Task<JsonResult> Reverse(int? id)
        {
            try
            {
                var bill = new AdvancePaymentBill() { StoreId = curStore?.Id ?? 0 };

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
                    bill = _advancePaymentBillService.GetAdvancePaymentBillById(curStore.Id, id.Value, true);
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

                if (bill.Items == null || !bill.Items.Any())
                {
                    return Warning("单据没有明细.");
                }

                if (DateTime.Now.Subtract(bill.AuditedDate ?? DateTime.Now).TotalSeconds > 86400)
                {
                    return Warning("已经审核的单据超过24小时，不允许红冲.");
                }


                #region 预付款 验证

                //if (bill.Items != null)
                //{
                //    //1.获取当前经销商 预付款科目Id
                //    int accountingOptionId = 0;
                //    IList<AccountingOption> accountingOptions = _accountingService.GetDefaultAccounts(curStore.Id);
                //    if (accountingOptions != null && accountingOptions.Count > 0)
                //    {
                //        AccountingOption accountingOption = accountingOptions.Where(a => a.AccountCodeTypeId == (int)AccountingCodeEnum.AdvancePayment).FirstOrDefault();
                //        accountingOptionId = (accountingOption == null) ? 0 : accountingOption.Id;
                //    }
                //    //获取用户输入预付款金额 
                //    //注意：这里不加会计科目条件，所有金额都是 预付款
                //    var advancePaymentAmount = bill.Items.Sum(ac => ac.CollectionAmount);

                //    //用户可用预付款金额
                //    decimal useAdvancePaymentAmount = _commonBillService.GetUseAdvancePaymentAmount(curStore.Id, bill.ManufacturerId);
                //    //如果输入预付款大于用户可用预付款
                //    if (advancePaymentAmount > useAdvancePaymentAmount)
                //    {

                //        return this.Warning("用户输入预付款金额：" + advancePaymentAmount + ",大于用户可用预付款金额：" + useAdvancePaymentAmount);
                //    }

                //}

                #endregion


                //RedLock

                var result = await _locker.PerformActionWithLockAsync(RedLockKey(bill),
                      TimeSpan.FromSeconds(30),
                      TimeSpan.FromSeconds(10),
                      TimeSpan.FromSeconds(1),
                      () => _advancePaymentBillService.Reverse(curUser.Id, bill));
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
        [AuthCode((int)AccessGranularityEnum.PrepaidBillsPrint)]
        public JsonResult PrintSetting()
        {
            var printTemplates = _printTemplateService.GetAllPrintTemplates(curStore.Id).ToList();
            var printTemplate = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.AdvancePaymentBill).FirstOrDefault();
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
        [AuthCode((int)AccessGranularityEnum.PrepaidBillsPrint)]
        public JsonResult Print(int type, string selectData, int? manufacturerId, int? draweer, string billNumber = "", bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? showReverse = null, bool? sortByAuditedTime = null, int? accountingOptionId = -1)
        {
            try
            {
                bool fg = true;
                string errMsg = string.Empty;

                #region 查询打印数据

                IList<AdvancePaymentBill> advancePaymentBills = new List<AdvancePaymentBill>();
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
                            AdvancePaymentBill advancePaymentBill = _advancePaymentBillService.GetAdvancePaymentBillById(curStore.Id, int.Parse(id), true);
                            if (advancePaymentBill != null)
                            {
                                advancePaymentBills.Add(advancePaymentBill);
                            }
                        }
                    }
                }
                else if (type == 2)
                {
                    advancePaymentBills = _advancePaymentBillService.GetAllAdvancePaymentBills(
                                curStore?.Id ?? 0,
                                 curUser.Id,
                                draweer,
                                manufacturerId,
                                billNumber,
                                auditedStatus,
                                startTime,
                                endTime,
                                showReverse,
                                sortByAuditedTime,
                                accountingOptionId);
                }


                #endregion

                #region 修改数据
                if (advancePaymentBills != null && advancePaymentBills.Count > 0)
                {
                    //using (var scope = new TransactionScope())
                    //{

                    //    scope.Complete();
                    //}
                    #region 修改单据表打印数
                    foreach (var d in advancePaymentBills)
                    {
                        d.PrintNum = (d.PrintNum ?? 0) + 1;
                        _advancePaymentBillService.UpdateAdvancePaymentBill(d);
                    }
                    #endregion
                }

                //获取打印模板
                var printTemplates = _printTemplateService.GetAllPrintTemplates(curStore.Id).ToList();
                var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.AdvancePaymentBill).Select(a => a.Content).FirstOrDefault();

                //获取打印设置
                var pCPrintSetting = _settingService.LoadSetting<PCPrintSetting>(curStore.Id);

                //填充打印数据
                foreach (var d in advancePaymentBills)
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
                    var ManufacturerName = _manufacturerService.GetManufacturerName(curStore.Id, d.ManufacturerId);
                    sb2.Replace("#供应商", ManufacturerName);
                    AccountingOption acc = _accountingService.GetAccountingOptionById(d.AccountingOptionId ?? 0);
                    sb2.Replace("#预付款账户", acc.Name);
                    sb2.Replace("#预付款金额", d.AdvanceAmount == null ? "0.00" : d.AdvanceAmount?.ToString("0.00"));
                    sb2.Replace("#付款日期", d?.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss"));
                    sb2.Replace("#备注", d.Remark);
                    sb.Insert(index, sb2);
                    #endregion

                    #region tfootid
                    User makeUser = _userService.GetUserById(curStore.Id, d.MakeUserId);
                    if (makeUser != null)
                    {
                        sb.Replace("@制单", makeUser.UserRealName);
                    }
                    User draweerUser = _userService.GetUserById(curStore.Id, d.Draweer);
                    if (draweerUser != null)
                    {
                        sb.Replace("@业务员", draweerUser.UserRealName);
                        sb.Replace("@业务电话", draweerUser.MobileNumber);
                    }
                    else
                    {
                        sb.Replace("@业务员", "");
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
        [AuthCode((int)AccessGranularityEnum.PrepaidBillsExport)]
        public FileResult Export(int type, string selectData, int? manufacturerId, int? draweer, string billNumber = "", bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? showReverse = null, bool? sortByAuditedTime = null, int? accountingOptionId = -1)
        {

            #region 查询导出数据
            IList<AdvancePaymentBill> advancePaymentBills = new List<AdvancePaymentBill>();

            if (type == 1)
            {
                if (!string.IsNullOrEmpty(selectData))
                {
                    List<string> ids = selectData.Split(',').ToList();
                    foreach (var id in ids)
                    {
                        AdvancePaymentBill advancePaymentBill = _advancePaymentBillService.GetAdvancePaymentBillById(curStore.Id, int.Parse(id), true);
                        if (advancePaymentBill != null)
                        {
                            advancePaymentBills.Add(advancePaymentBill);
                        }
                    }
                }
            }
            else if (type == 2)
            {
                advancePaymentBills = _advancePaymentBillService.GetAllAdvancePaymentBills(
                            curStore?.Id ?? 0,
                             curUser.Id,
                            draweer,
                            manufacturerId,
                            billNumber,
                            auditedStatus,
                            startTime,
                            endTime,
                            showReverse,
                            sortByAuditedTime,
                            accountingOptionId);
            }

            #endregion

            #region 导出
            var ms = _exportManager.ExportAdvancePaymentBillToXlsx(advancePaymentBills, curStore.Id);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "预付款单.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "预付款单.xlsx");
            }
            #endregion

        }


    }
}