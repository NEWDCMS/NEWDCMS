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
    /// 用于收款单管理
    /// </summary>
    public class ReceiptCashController : BasePublicController
    {
        private readonly IPrintTemplateService _printTemplateService;
        private readonly ITerminalService _terminalService;
        private readonly IUserService _userService;
        private readonly ICashReceiptBillService _cashReceiptBillService;
        private readonly IMediaService _mediaService;
        private readonly IAccountingService _accountingService;
        private readonly IUserActivityService _userActivityService;
        private readonly ISettingService _settingService;
        private readonly IBillConvertService _billConvertService;
        private readonly IRedLocker _locker;
        private readonly IExportManager _exportManager;
        private readonly ICommonBillService _commonBillService;

        public ReceiptCashController(
            IWorkContext workContext,
            IStoreContext storeContext,
            ILogger loggerService,
            IPrintTemplateService printTemplateService,
            ITerminalService terminalService,
            ICashReceiptBillService cashReceiptBillService,
            IMediaService mediaService,
            IAccountingService accountingService,
            IUserActivityService userActivityService,
            INotificationService notificationService,
            ISettingService settingService,
            IBillConvertService billConvertService,
            IUserService userService,
            IRedLocker locker,
            ICommonBillService commonBillService,
            IExportManager exportManager
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            _printTemplateService = printTemplateService;
            _terminalService = terminalService;
            _cashReceiptBillService = cashReceiptBillService;
            _mediaService = mediaService;
            _accountingService = accountingService;
            _userService = userService;
            _userActivityService = userActivityService;
            _settingService = settingService;
            _billConvertService = billConvertService;
            _locker = locker;
            _exportManager = exportManager;
            _commonBillService = commonBillService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        /// <summary>
        /// 收款单列表
        /// </summary>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.ReceiptBillsView)]
        public IActionResult List(int? customerId, string customerName, int? payeer, string billNumber = "", bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? showReverse = null, bool? sortByAuditedTime = null, string remark = "", int pagenumber = 0)
        {

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var model = new CashReceiptBillListModel
            {
                CustomerId = customerId ?? 0,
                CustomerName = customerName,
                //收款人
                Payeers = BindUserSelection(_userService.BindUserList, curStore, "")
            };
            model.Payeer = payeer ?? null;

            //默认收款账户动态列
            var defaultAcc = _accountingService.GetDefaultAccounting(curStore?.Id ?? 0, BillTypeEnum.CashReceiptBill);
            model.DynamicColumns = defaultAcc?.Item4?.OrderBy(s => s.Key).Select(s => s.Value).ToList();

            model.BillNumber = billNumber;
            model.AuditedStatus = auditedStatus;
            model.StartTime = (startTime == null) ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")) : startTime;
            model.EndTime = (endTime == null) ? DateTime.Now.AddDays(1) : endTime;
            model.ShowReverse = showReverse;
            model.SortByAuditedTime = sortByAuditedTime;

            //获取分页
            var bills = _cashReceiptBillService.GetAllCashReceiptBills(
                curStore?.Id ?? 0,
                 curUser.Id,
                customerId, payeer,
                billNumber,
                auditedStatus,
                model.StartTime, //?? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd")),
                model.EndTime, //?? DateTime.Now.AddDays(1),
                showReverse,
                sortByAuditedTime,
                remark,
                null,
                null,
                pagenumber, 30);
            model.PagingFilteringContext.LoadPagedList(bills);

            #region 查询需要关联其他表的数据

            var allUsers = _userService.GetUsersDictsByIds(curStore.Id, bills.Select(b => b.Payeer).Distinct().ToArray());
            var allTerminal = _terminalService.GetTerminalsByIds(curStore.Id, bills.Select(b => b.CustomerId).Distinct().ToArray());
            #endregion

            model.Lists = bills.OrderByDescending(b => b.CreatedOnUtc).Select(b =>
              {
                  var m = b.ToModel<CashReceiptBillModel>();

                  //收款人
                  m.PayeerName = allUsers.Where(aw => aw.Key == m.Payeer).Select(aw => aw.Value).FirstOrDefault();

                  //客户名称
                  var terminal = allTerminal.Where(at => at.Id == m.CustomerId).FirstOrDefault();
                  m.CustomerName = terminal == null ? "" : terminal.Name;

                  //收款账户
                  m.CashReceiptBillAccountings = defaultAcc?.Item4?.OrderBy(s => s.Key).Select(s =>
                    {
                        var acc = b.CashReceiptBillAccountings.Where(a => a?.AccountingOption?.ParentId == s.Key).FirstOrDefault();
                        return new CashReceiptBillAccountingModel()
                        {
                            AccountingOptionId = acc?.AccountingOptionId ?? 0,
                            CollectionAmount = acc?.CollectionAmount ?? 0
                        };
                    }).ToList();

                  //总优惠金额(本次优惠金额总和)
                  m.TotalDiscountAmountOnce = b.Items.Sum(sb => sb.DiscountAmountOnce);

                  //收款前欠款金额
                  m.BeforArrearsAmount = b.Items.Sum(sb => sb.ArrearsAmount);

                  //剩余金额(收款后尚欠金额总和)
                  m.TotalSubAmountOwedAfterReceipt = b.Items.Sum(sb => sb.AmountOwedAfterReceipt);

                  m.Items = b.Items.Select(sb =>
                  {
                      var it = sb.ToModel<CashReceiptItemModel>();
                      return it;

                  }).ToList();

                  return m;
              }).ToList();

            return View(model);
        }

        /// <summary>
        /// 添加收款单
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.ReceiptBillsSave)]
        public IActionResult Create(int? store)
        {

            var companySetting = _settingService.LoadSetting<CompanySetting>(curStore.Id);
            var model = new CashReceiptBillModel
            {
                BillTypeEnumId = (int)BillTypeEnum.CashReceiptBill,
                CreatedOnUtc = DateTime.Now,
                //收款人
                Payeers = BindUserSelection(_userService.BindUserList, curStore, "")
            };
            model.Payeer = (model.Payeer ?? -1);

            //默认收款
            var defaultAcc = _accountingService.GetDefaultAccounting(curStore?.Id ?? 0, BillTypeEnum.CashReceiptBill);
            model.CashReceiptBillAccountings.Add(new CashReceiptBillAccountingModel()
            {
                Name = defaultAcc?.Item1?.Name,
                CollectionAmount = 0,
                AccountingOptionId = defaultAcc?.Item1?.Id ?? 0,
                AccountCodeTypeId = defaultAcc?.Item1?.AccountCodeTypeId ?? 0
            });


            //允许预收款支付成负数
            model.AllowAdvancePaymentsNegative = companySetting.AllowAdvancePaymentsNegative;

            return View(model);
        }

        /// <summary>
        /// 编辑收款单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.ReceiptBillsView)]
        public IActionResult Edit(int id = 0)
        {

            var companySetting = _settingService.LoadSetting<CompanySetting>(curStore.Id);
            var model = new CashReceiptBillModel
            {
                BillTypeEnumId = (int)BillTypeEnum.CashReceiptBill
            };

            var cashReceiptBill = _cashReceiptBillService.GetCashReceiptBillById(curStore.Id, id, true);
            if (cashReceiptBill == null)
            {
                return RedirectToAction("List");
            }

            if (cashReceiptBill != null)
            {
                //只能操作当前经销商数据
                if (cashReceiptBill.StoreId != curStore.Id)
                {
                    return RedirectToAction("List");
                }

                model = cashReceiptBill.ToModel<CashReceiptBillModel>();
                model.BillBarCode = _mediaService.GenerateBarCodeForBase64(cashReceiptBill.BillNumber, 150, 50);
            }


            //收款账户
            if (cashReceiptBill != null && cashReceiptBill.CashReceiptBillAccountings != null)
            {
                model.CashReceiptBillAccountings = cashReceiptBill.CashReceiptBillAccountings.Select(s =>
                {
                    var m = s.ToAccountModel<CashReceiptBillAccountingModel>();
                    //m.Name = _accountingService.GetAccountingOptionById(s.AccountingOptionId).Name;
                    m.Name = _accountingService.GetAccountingOptionName(curStore.Id, s.AccountingOptionId);
                    return m;
                }).ToList();
            }

            //客户名称
            var t = _terminalService.GetTerminalById(curStore.Id, cashReceiptBill.CustomerId);
            if (t != null)
            {
                model.CustomerName = t.Name;
            }

            //收款人
            model.Payeers = BindUserSelection(_userService.BindUserList, curStore, "");
            model.Payeer = (model.Payeer ?? -1);

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

            //允许预收款支付成负数
            model.AllowAdvancePaymentsNegative = companySetting.AllowAdvancePaymentsNegative;

            return View(model);
        }

        /// <summary>
        /// 审核收款单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.ReceiptBillsApproved)]
        public async Task<JsonResult> Auditing(int? id)
        {
            try
            {
                var bill = new CashReceiptBill();
                var recordingVoucher = new RecordingVoucher();
                #region 验证
                if (!id.HasValue)
                {
                    return Warning("参数错误.");
                }
                else
                {
                    bill = _cashReceiptBillService.GetCashReceiptBillById(curStore.Id, id.Value, true);
                }

                //公共单据验证
                var commonBillChecking = BillChecking<CashReceiptBill, CashReceiptItem>(bill, BillStates.Audited, ((int)AccessGranularityEnum.ReceiptBillsApproved).ToString());
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
                      () => _cashReceiptBillService.Auditing(curStore.Id, curUser.Id, bill));
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
        [AuthCode((int)AccessGranularityEnum.ReceiptBillsReverse)]
        public async Task<JsonResult> Reverse(int? id)
        {
            try
            {
                var bill = new CashReceiptBill() { StoreId = curStore?.Id ?? 0 };

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
                    bill = _cashReceiptBillService.GetCashReceiptBillById(curStore.Id, id.Value, true);
                }

                //公共单据验证
                var commonBillChecking = BillChecking<CashReceiptBill, CashReceiptItem>(bill, BillStates.Reversed);
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
                      () => _cashReceiptBillService.Reverse(curUser.Id, bill));
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

        #region 单据项目

        /// <summary>
        /// 异步获取收款单项目
        /// </summary>
        /// <param name="cashReceiptBillId"></param>
        /// <returns></returns>
        public async Task<JsonResult> AsyncCashReceiptItems(int cashReceiptBillId)
        {
            return await Task.Run(() =>
            {
                var items = _cashReceiptBillService.GetCashReceiptItemList(cashReceiptBillId)
                .Select(o =>
                  {
                      var m = o.ToModel<CashReceiptItemModel>();
                      m.BillTypeName = CommonHelper.GetEnumDescription(o.BillTypeEnum);
                      m.BillLink = _billConvertService.GenerateBillUrl(m.BillTypeId, m.BillId);
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
        /// 创建/更新收款单
        /// </summary>
        /// <param name="data"></param>
        /// <param name="billId"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.ReceiptBillsSave)]
        public async Task<JsonResult> CreateOrUpdate(CashReceiptUpdateModel data, int? billId, bool doAudit = true)
        {

            try
            {
                var cashReceiptBill = new CashReceiptBill();

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
                    cashReceiptBill = _cashReceiptBillService.GetCashReceiptBillById(curStore.Id, billId.Value, true);

                    //公共单据验证
                    var commonBillChecking = BillChecking<CashReceiptBill, CashReceiptItem>(cashReceiptBill, BillStates.Draft);
                    if (commonBillChecking.Value != null)
                    {
                        return commonBillChecking;
                    }
                }
                //判断指定单据是否尚有欠款(是否已经收完款)
                var isDebt = true;
                var warningMsg = string.Empty;
                foreach (var item in data.Items)
                {
                    isDebt = _cashReceiptBillService.ThereAnyDebt(curStore.Id, item.BillTypeId, item.BillId);
                    if (!isDebt) //如果已经结清款项，则返回提示;若没有，则继续循环
                    {
                        warningMsg = $"非法操作，单据{item.BillNumber}已结清款项.";
                        break;
                    }
                    //判断单据是否存在未审核的收款单
                    isDebt = _cashReceiptBillService.ExistsUnAuditedByBillNumber(curStore.Id, item.BillNumber, billId ?? 0);
                    if (!isDebt) //存在为甚的收款单
                    {
                        warningMsg = $"单据{item.BillNumber}存在未审核的收款单.";
                        break;
                    }
                }
                if (!isDebt)
                {
                    return Warning(warningMsg);
                }
                #endregion

                #region 预收款 验证
                if (data.Accounting != null)
                {
                    //剩余预收款金额(预收账款科目下的所有子类科目)：
                    //var advanceAmountBalance = _commonBillService.CalcTerminalBalance(curStore.Id, data.CustomerId);
                    if (data.AdvanceAmount > 0 && data.AdvanceAmount > data.AdvanceAmountBalance)
                    {
                        return Warning("预收款余额不足!");
                    }
                }
                #endregion

                //业务逻辑
                var accountings = _accountingService.GetAllAccountingOptions(curStore.Id, 0, true);
                var dataTo = data.ToEntity<CashReceiptBillUpdate>();
                dataTo.Operation = (int)OperationEnum.PC;

                if (data.Accounting == null)
                {
                    return Warning("没有默认的付款账号");
                }

                dataTo.Accounting = data.Accounting.Select(ac => ac.ToAccountEntity<CashReceiptBillAccounting>()).ToList();
                dataTo.Items = data.Items.Select(it => it.ToEntity<CashReceiptItem>()).ToList();

                //RedLock
                var result = await _locker.PerformActionWithLockAsync(LockKey(data),
                      TimeSpan.FromSeconds(30),
                      TimeSpan.FromSeconds(10),
                      TimeSpan.FromSeconds(1),
                      () => _cashReceiptBillService.BillCreateOrUpdate(curStore.Id, curUser.Id, billId, cashReceiptBill, dataTo.Accounting, accountings, dataTo, dataTo.Items, _userService.IsAdmin(curStore.Id, curUser.Id), doAudit));
                return Json(result);
            }
            catch (Exception ex)
            {
                //活动日志
                _userActivityService.InsertActivity("CreateOrUpdate", Resources.Bill_CreateOrUpdateFailed, curUser.Id);
                //_notificationService.SuccessNotification(Resources.Bill_CreateOrUpdateFailed);
                return Error(ex.Message);
            }
        }

        #endregion


        #region  收款单据类型汇总查询

        public JsonResult AsyncLoadOwecashBillsPopup(int terminalId = 0)
        {
            var model = new CashReceiptBillModel
            {
                StartTime = DateTime.Now.AddDays(-30),
                EndTime = DateTime.Now,
                //客户
                CustomerId = terminalId,
                //业务员
                Payeers = BindUserSelection(_userService.BindUserList, curStore, ""),
                //单据类型
                BillTypes = BindBillTypeSelection<BillTypeEnum>(ids: new BillTypeEnum[] {
                    BillTypeEnum.SaleBill,
                    BillTypeEnum.ReturnBill,
                    BillTypeEnum.AdvanceReceiptBill,
                    BillTypeEnum.CostExpenditureBill,
                    BillTypeEnum.FinancialIncomeBill }),
                BillTypeId = null
            };

            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("_AsyncLoadOwecashBillsPopup", model)
            });
        }

        /// <summary>
        /// 获取欠款单据
        /// </summary>
        /// <param name="payeer"></param>
        /// <param name="terminalId"></param>
        /// <param name="billTypeId"></param>
        /// <param name="billNumber"></param>
        /// <param name="remark"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<JsonResult> AsyncLoadOwecashBills(int? payeer,
            int? terminalId,
            int? billTypeId,
            string billNumber = "",
            string remark = "",
            DateTime? startTime = null,
            DateTime? endTime = null,
            int pageIndex = 0,
            int pageSize = 20)
        {
            return await Task.Run(() =>
            {
                var billSummaries = new List<BillSummaryModel>();

                var bills = _cashReceiptBillService.GetBillCashReceiptSummary(curStore?.Id ?? 0,
                    null,
                    terminalId, 
                    billTypeId, 
                    billNumber, 
                    remark, 
                    startTime, 
                    endTime);

                //重写计算： 优惠金额	 已收金额  尚欠金额
                foreach (var bill in bills)
                {
                    //销售单
                    if (bill.BillTypeId == (int)BillTypeEnum.SaleBill)
                    {
                        //单据金额
                        decimal calc_billAmount = bill.Amount ?? 0;
                        if (bill.Remark != null && bill.Remark.IndexOf("应收款销售单") != -1)
                        {
                            calc_billAmount = bill.ArrearsAmount ?? 0;
                        }

                        //优惠金额 
                        decimal calc_discountAmount = 0;
                        //已收金额
                        decimal calc_paymentedAmount = 0;
                        //尚欠金额
                        decimal calc_arrearsAmount = 0;

                        #region 计算如下

                        //单已经收款部分的本次优惠合计
                        var discountAmountOnce = _commonBillService.GetBillDiscountAmountOnce(curStore?.Id ?? 0, bill.BillId);

                        //优惠金额 =  单据优惠金额  + （单已经收款部分的本次优惠合计）
                        calc_discountAmount = Convert.ToDecimal(Convert.ToDouble(bill.DiscountAmount ?? 0) + Convert.ToDouble(discountAmountOnce));

                        //单据收款金额（收款账户）
                        var collectionAmount = _commonBillService.GetBillCollectionAmount(curStore?.Id ?? 0, bill.BillId, BillTypeEnum.SaleBill);

                        //单已经收款部分的本次收款合计
                        var receivableAmountOnce = _commonBillService.GetBillReceivableAmountOnce(curStore?.Id ?? 0, bill.BillId);

                        //已收金额 = 单据收款金额（收款账户） + （单已经收款部分的本次收款合计）
                        calc_paymentedAmount = collectionAmount + receivableAmountOnce;

                        //尚欠金额
                        //Convert.ToDouble(bill.ArrearsAmount ?? 0) + 
                        calc_arrearsAmount = Convert.ToDecimal(Convert.ToDouble(calc_billAmount) - Convert.ToDouble(calc_discountAmount) - Convert.ToDouble(calc_paymentedAmount));

                        #endregion

                        //重新赋值
                        bill.Amount = calc_billAmount;
                        bill.DiscountAmount = calc_discountAmount;
                        bill.PaymentedAmount = calc_paymentedAmount;
                        bill.ArrearsAmount = calc_arrearsAmount;

                    }
                    //退货单
                    else if (bill.BillTypeId == (int)BillTypeEnum.ReturnBill)
                    {
                        //单据金额
                        decimal calc_billAmount = bill.Amount ?? 0;
                        //优惠金额 
                        decimal calc_discountAmount = 0;
                        //已收金额
                        decimal calc_paymentedAmount = 0;
                        //尚欠金额
                        decimal calc_arrearsAmount = 0;

                        #region 计算如下

                        //单已经收款部分的本次优惠合计
                        var discountAmountOnce = _commonBillService.GetBillDiscountAmountOnce(curStore?.Id ?? 0, bill.BillId);

                        //优惠金额 =  单据优惠金额  + （单已经收款部分的本次优惠合计）
                        calc_discountAmount = bill.DiscountAmount ?? 0 + discountAmountOnce;

                        //单据收款金额（收款账户）
                        var collectionAmount = _commonBillService.GetBillCollectionAmount(curStore?.Id ?? 0, bill.BillId, BillTypeEnum.ReturnBill);

                        //单已经收款部分的本次收款合计
                        var receivableAmountOnce = _commonBillService.GetBillReceivableAmountOnce(curStore?.Id ?? 0, bill.BillId);

                        //已收金额 = 单据收款金额（收款账户） + （单已经收款部分的本次收款合计）
                        calc_paymentedAmount = collectionAmount + receivableAmountOnce;

                        //尚欠金额
                        calc_arrearsAmount = Convert.ToDecimal(Convert.ToDouble(calc_billAmount) - Convert.ToDouble(calc_discountAmount) - Math.Abs(Convert.ToDouble(calc_paymentedAmount)));

                        #endregion

                        //重新赋值
                        bill.Amount = -calc_billAmount;
                        bill.DiscountAmount = -calc_discountAmount;
                        bill.PaymentedAmount = -calc_paymentedAmount;
                        bill.ArrearsAmount = -calc_arrearsAmount;

                    }
                    //预收款单
                    else if (bill.BillTypeId == (int)BillTypeEnum.AdvanceReceiptBill)
                    {

                        //单据金额
                        decimal calc_billAmount = bill.Amount ?? 0;
                        //优惠金额 
                        decimal calc_discountAmount = 0;
                        //已收金额
                        decimal calc_paymentedAmount = 0;
                        //尚欠金额
                        decimal calc_arrearsAmount = 0;

                        #region 计算如下

                        //单已经收款部分的本次优惠合计
                        var discountAmountOnce = _commonBillService.GetBillDiscountAmountOnce(curStore?.Id ?? 0, bill.BillId);

                        //优惠金额 =  单据优惠金额  + （已经收款部分的本次优惠合计）
                        calc_discountAmount = bill.DiscountAmount ?? 0 + discountAmountOnce;

                        //单据收款金额（收款账户）
                        var collectionAmount = _commonBillService.GetBillCollectionAmount(curStore?.Id ?? 0, bill.BillId, BillTypeEnum.AdvanceReceiptBill);

                        //单已经收款部分的本次收款合计
                        var receivableAmountOnce = _commonBillService.GetBillReceivableAmountOnce(curStore?.Id ?? 0, bill.BillId);

                        //已收金额 = 单据收款金额（收款账户） + （已经收款部分的本次收款合计）
                        calc_paymentedAmount = collectionAmount + receivableAmountOnce;

                        calc_arrearsAmount = Convert.ToDecimal(Convert.ToDouble(calc_billAmount) - Convert.ToDouble(calc_discountAmount) - Convert.ToDouble(calc_paymentedAmount));

                        #endregion

                        //重新赋值
                        bill.Amount = calc_billAmount;
                        bill.DiscountAmount = calc_discountAmount;
                        bill.PaymentedAmount = calc_paymentedAmount;
                        bill.ArrearsAmount = calc_arrearsAmount;
                    }
                    //费用支出
                    else if (bill.BillTypeId == (int)BillTypeEnum.CostExpenditureBill)
                    {
                        //单据金额
                        decimal calc_billAmount = bill.Amount ?? 0;
                        //优惠金额 
                        decimal calc_discountAmount = 0;
                        //已收金额
                        decimal calc_paymentedAmount = 0;
                        //尚欠金额
                        decimal calc_arrearsAmount = 0;

                        #region 计算如下

                        //单已经收款部分的本次优惠合计
                        var discountAmountOnce = _commonBillService.GetBillDiscountAmountOnce(curStore?.Id ?? 0, bill.BillId);

                        //优惠金额 =  单据优惠金额  + （单已经收款部分的本次优惠合计）
                        calc_discountAmount = bill.DiscountAmount ?? 0 + discountAmountOnce;

                        //单据收款金额（收款账户）
                        var collectionAmount = _commonBillService.GetBillCollectionAmount(curStore?.Id ?? 0, bill.BillId, BillTypeEnum.CostExpenditureBill);

                        //单已经收款部分的本次收款合计
                        var receivableAmountOnce = _commonBillService.GetBillReceivableAmountOnce(curStore?.Id ?? 0, bill.BillId);

                        //已收金额 = 单据收款金额（收款账户） + （单已经收款部分的本次收款合计）
                        calc_paymentedAmount = collectionAmount + receivableAmountOnce;

                        //尚欠金额 
                        calc_arrearsAmount = Convert.ToDecimal(Convert.ToDouble(calc_billAmount) - Convert.ToDouble(calc_discountAmount) - Math.Abs(Convert.ToDouble(calc_paymentedAmount)));

                        #endregion

                        //重新赋值
                        bill.Amount = -Math.Abs(calc_billAmount);
                        bill.DiscountAmount = -Math.Abs(calc_discountAmount);
                        bill.PaymentedAmount = -Math.Abs(calc_paymentedAmount);
                        bill.ArrearsAmount = -Math.Abs(calc_arrearsAmount);
                    }
                    //其它收入
                    else if (bill.BillTypeId == (int)BillTypeEnum.FinancialIncomeBill)
                    {
                        //单据金额
                        decimal calc_billAmount = bill.Amount ?? 0;
                        //优惠金额 
                        decimal calc_discountAmount = 0;
                        //已收金额
                        decimal calc_paymentedAmount = 0;
                        //尚欠金额
                        decimal calc_arrearsAmount = 0;

                        #region 计算如下

                        //单已经收款部分的本次优惠合计
                        var discountAmountOnce = _commonBillService.GetBillDiscountAmountOnce(curStore?.Id ?? 0, bill.BillId);

                        //优惠金额 =  单据优惠金额  + （单已经收款部分的本次优惠合计）
                        calc_discountAmount = bill.DiscountAmount ?? 0 + discountAmountOnce;

                        //单据收款金额（收款账户）
                        var collectionAmount = _commonBillService.GetBillCollectionAmount(curStore?.Id ?? 0, bill.BillId, BillTypeEnum.FinancialIncomeBill);

                        //单已经收款部分的本次收款合计
                        var receivableAmountOnce = _commonBillService.GetBillReceivableAmountOnce(curStore?.Id ?? 0, bill.BillId);

                        //已收金额 = 单据收款金额（收款账户） + （单已经收款部分的本次收款合计）
                        calc_paymentedAmount = collectionAmount + receivableAmountOnce;

                        //尚欠金额 
                        calc_arrearsAmount = Convert.ToDecimal(Convert.ToDouble(calc_billAmount) - Convert.ToDouble(calc_discountAmount) - Convert.ToDouble(calc_paymentedAmount));

                        #endregion

                        //重新赋值
                        bill.Amount = calc_billAmount;
                        bill.DiscountAmount = calc_discountAmount;
                        bill.PaymentedAmount = calc_paymentedAmount;
                        bill.ArrearsAmount = calc_arrearsAmount;

                    }
                }


                billSummaries = bills.Where(s => Math.Abs(s.ArrearsAmount ?? 0) > 0).Select(s =>
                  {
                      var bill = s.ToModel<BillSummaryModel>();

                    bill.BillLink = _billConvertService.GenerateBillUrl(bill.BillTypeId, bill.BillId);

                    return bill;

                }).ToList();

                return Json(new
                {
                    Success = true,
                    total = billSummaries.Count,
                    rows = billSummaries
                });

            });
        }


        #endregion
        [AuthCode((int)AccessGranularityEnum.ReceiptBillsPrint)]
        public JsonResult PrintSetting()
        {
            var printTemplates = _printTemplateService.GetAllPrintTemplates(curStore.Id).ToList();
            var printTemplate = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.CashReceiptBill).FirstOrDefault();
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
        [AuthCode((int)AccessGranularityEnum.ReceiptBillsPrint)]
        public JsonResult Print(int type, string selectData, int? customerId, string customerName, int? payeer, string billNumber = "", bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? showReverse = null, bool? sortByAuditedTime = null, string remark = "")
        {
            try
            {
                bool fg = true;
                string errMsg = string.Empty;

                #region 查询打印数据

                IList<CashReceiptBill> cashReceiptBills = new List<CashReceiptBill>();
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
                            CashReceiptBill cashReceiptBill = _cashReceiptBillService.GetCashReceiptBillById(curStore.Id, int.Parse(id), true);
                            if (cashReceiptBill != null)
                            {
                                cashReceiptBills.Add(cashReceiptBill);
                            }
                        }
                    }
                }
                else if (type == 2)
                {
                    cashReceiptBills = _cashReceiptBillService.GetAllCashReceiptBills(
                                curStore?.Id ?? 0,
                                 curUser.Id,
                                customerId, payeer,
                                billNumber,
                                auditedStatus,
                                startTime,
                                endTime,
                                showReverse,
                                sortByAuditedTime,
                                remark, null);
                }

                #endregion

                #region 修改数据
                if (cashReceiptBills != null && cashReceiptBills.Count > 0)
                {
                    //using (var scope = new TransactionScope())
                    //{
                    //    scope.Complete();
                    //}
                    #region 修改单据表打印数
                    foreach (var d in cashReceiptBills)
                    {
                        d.PrintNum = (d.PrintNum ?? 0) + 1;
                        _cashReceiptBillService.UpdateCashReceiptBill(d);
                    }
                    #endregion
                }

                //获取打印模板
                var printTemplates = _printTemplateService.GetAllPrintTemplates(curStore.Id).ToList();
                var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.CashReceiptBill).Select(a => a.Content).FirstOrDefault();

                //获取打印设置
                var pCPrintSetting = _settingService.LoadSetting<PCPrintSetting>(curStore.Id);

                //填充打印数据
                foreach (var d in cashReceiptBills)
                {
                    Terminal terminal = _terminalService.GetTerminalById(curStore.Id, d.CustomerId);

                    StringBuilder sb = new StringBuilder();
                    sb.Append(content);

                    #region theadid
                    //sb.Replace("@商铺名称", curStore.Name);
                    if (pCPrintSetting != null)
                    {
                        sb.Replace("@商铺名称", string.IsNullOrWhiteSpace(pCPrintSetting.StoreName) ? "&nbsp;" : pCPrintSetting.StoreName);
                    }

                    if (terminal != null)
                    {
                        sb.Replace("@客户名称", terminal.Name);
                        sb.Replace("@老板姓名", terminal.BossName);
                    }
                    User businessUser = _userService.GetUserById(curStore.Id, d.Payeer);
                    if (businessUser != null)
                    {
                        sb.Replace("@业务员", businessUser.UserRealName);
                        sb.Replace("@业务电话", businessUser.MobileNumber);
                    }
                    sb.Replace("@单据编号", d.BillNumber);
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

                    if (d.Items != null && d.Items.Count > 0)
                    {
                        //1.先删除明细第一行
                        sb.Remove(beginTbody, endTbody - beginTbody);
                        int i = 0;
                        foreach (var item in d.Items)
                        {
                            int index = sb.ToString().IndexOf("</tbody>", beginTbody);
                            i++;
                            StringBuilder sb2 = new StringBuilder();
                            sb2.Append(tbodytr);

                            sb2.Replace("#序号", i.ToString());
                            sb2.Replace("#单据编号", item.BillNumber);
                            sb2.Replace("#单据类型", CommonHelper.GetEnumDescription<BillTypeEnum>(item.BillTypeId));
                            sb2.Replace("#开单时间", item.CreatedOnUtc.ToString());
                            sb2.Replace("#单据金额", item.Amount == null ? "0.00" : item.Amount?.ToString("0.00"));
                            sb2.Replace("#已收金额", item.PaymentedAmount == null ? "0.00" : item.PaymentedAmount?.ToString("0.00"));
                            sb2.Replace("#尚欠金额", item.ArrearsAmount == null ? "0.00" : item.ArrearsAmount?.ToString("0.00"));
                            sb2.Replace("#本次收款金额", item.ReceivableAmountOnce == null ? "0.00" : item.ReceivableAmountOnce?.ToString("0.00"));
                            sb2.Replace("#备注", "");
                            sb.Insert(index, sb2);

                        }

                        sb.Replace("单据金额:###", d.Items.Sum(s => s.Amount ?? 0).ToString("0.00"));
                        sb.Replace("已收金额:###", d.Items.Sum(s => s.PaymentedAmount ?? 0).ToString("0.00"));
                        sb.Replace("尚欠金额:###", d.Items.Sum(s => s.ArrearsAmount ?? 0).ToString("0.00"));
                        sb.Replace("本次收款金额:###", d.Items.Sum(s => s.ReceivableAmountOnce ?? 0).ToString("0.00"));

                    }
                    #endregion

                    #region tfootid
                    User makeUser = _userService.GetUserById(curStore.Id, d.MakeUserId);
                    if (makeUser != null)
                    {
                        sb.Replace("@制单", makeUser.UserRealName);
                    }
                    sb.Replace("@日期", d.CreatedOnUtc.ToString());
                    sb.Replace("@打印日期", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                    sb.Replace("@备注", d.Remark);
                    //sb.Replace("@公司地址", "");
                    if (pCPrintSetting != null)
                    {
                        sb.Replace("@公司地址", pCPrintSetting.Address);
                    }

                    //sb.Replace("@订货电话", "");
                    if (pCPrintSetting != null)
                    {
                        sb.Replace("@订货电话", pCPrintSetting.PlaceOrderTelphone);
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
        [AuthCode((int)AccessGranularityEnum.ReceiptBillsExport)]
        public FileResult Export(int type, string selectData, int? customerId, string customerName, int? payeer, string billNumber = "", bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? showReverse = null, bool? sortByAuditedTime = null, string remark = "")
        {

            #region 查询导出数据
            IList<CashReceiptBill> cashReceiptBills = new List<CashReceiptBill>();

            if (type == 1)
            {
                if (!string.IsNullOrEmpty(selectData))
                {
                    List<string> ids = selectData.Split(',').ToList();
                    foreach (var id in ids)
                    {
                        CashReceiptBill cashReceiptBill = _cashReceiptBillService.GetCashReceiptBillById(curStore.Id, int.Parse(id), true);
                        if (cashReceiptBill != null)
                        {
                            cashReceiptBills.Add(cashReceiptBill);
                        }
                    }
                }
            }
            else if (type == 2)
            {
                cashReceiptBills = _cashReceiptBillService.GetAllCashReceiptBills(
                            curStore?.Id ?? 0,
                             curUser.Id,
                            customerId, payeer,
                            billNumber,
                            auditedStatus,
                            startTime,
                            endTime,
                            showReverse,
                            sortByAuditedTime,
                            remark, null);
            }

            #endregion

            #region 导出
            var ms = _exportManager.ExportReceiptCashBillToXlsx(cashReceiptBills, curStore.Id);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "收款单.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "收款单.xlsx");
            }
            #endregion

        }
    }
}