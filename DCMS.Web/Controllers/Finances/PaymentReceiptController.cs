using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Finances;
using DCMS.Core.Domain.Products;
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
    /// 用于付款单管理
    /// </summary>
    public class PaymentReceiptController : BasePublicController
    {
        private readonly IPrintTemplateService _printTemplateService;
        private readonly IUserService _userService;
        private readonly IMediaService _mediaService;
        private readonly IAccountingService _accountingService;
        private readonly IUserActivityService _userActivityService;
        private readonly ISettingService _settingService;
        private readonly IPaymentReceiptBillService _paymentReceiptBillService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IBillConvertService _billConvertService;
        private readonly IRedLocker _locker;
        private readonly IExportManager _exportManager;
        private readonly ICommonBillService _commonBillService;

        public PaymentReceiptController(
            IWorkContext workContext,
            IStoreContext storeContext,
            ILogger loggerService,
            IPrintTemplateService printTemplateService,
            IMediaService mediaService,
            IAccountingService accountingService,
            IUserActivityService userActivityService,
            ISettingService settingService,
            IPaymentReceiptBillService paymentReceiptBillService,
            IManufacturerService manufacturerService,
            IBillConvertService billConvertService,
            INotificationService notificationService,
            IUserService userService,
            IRedLocker locker,
            ICommonBillService commonBillService,
            IExportManager exportManager
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            _printTemplateService = printTemplateService;
            _mediaService = mediaService;
            _accountingService = accountingService;
            _userService = userService;
            _userActivityService = userActivityService;
            _settingService = settingService;
            _paymentReceiptBillService = paymentReceiptBillService;
            _billConvertService = billConvertService;
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
        /// 付款单列表
        /// </summary>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.PaymentBillsView)]
        public IActionResult List(int? manufacturerId, int? draweer, string billNumber = "", bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? showReverse = null, bool? sortByAuditedTime = null, int pagenumber = 0)
        {

            if (pagenumber > 0)
            {
                pagenumber -= 1;
            }

            var model = new PaymentReceiptBillListModel
            {
                //付款人
                Draweers = BindUserSelection(_userService.BindUserList, curStore, "")
            };
            model.Draweer = draweer ?? null;

            //供应商
            model.Manufacturers = BindManufacturerSelection(_manufacturerService.BindManufacturerList, curStore);
            model.ManufacturerId = manufacturerId ?? null;

            //默认收款账户动态列
            var defaultAcc = _accountingService.GetDefaultAccounting(curStore?.Id ?? 0, BillTypeEnum.PaymentReceiptBill);
            model.DynamicColumns = defaultAcc?.Item4?.OrderBy(s => s.Key).Select(s => s.Value).ToList();

            model.BillNumber = billNumber;
            model.AuditedStatus = auditedStatus;
            model.StartTime = (startTime == null) ? DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")) : startTime;
            model.EndTime = (endTime == null) ? DateTime.Now.AddDays(1) : endTime;
            model.ShowReverse = showReverse;
            model.SortByAuditedTime = sortByAuditedTime;

            //获取分页
            var bills = _paymentReceiptBillService.GetAllPaymentReceiptBills(
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
                pagenumber, 30);
            model.PagingFilteringContext.LoadPagedList(bills);

            #region 查询需要关联其他表的数据

            var allUsers = _userService.GetUsersDictsByIds(curStore.Id, bills.Select(b => b.Draweer).Distinct().ToArray());

            var allManufacturer = _manufacturerService.GetManufacturersByIds(curStore.Id, bills.Select(b => b.ManufacturerId).Distinct().ToArray());
            #endregion

            model.Items = bills.OrderByDescending(b => b.CreatedOnUtc).Select(b =>
              {
                  var m = b.ToModel<PaymentReceiptBillModel>();

                  //付款人
                  m.DraweerName = allUsers.Where(aw => aw.Key == m.Draweer).Select(aw => aw.Value).FirstOrDefault();

                  //供应商
                  var manufacturer = allManufacturer.Where(am => am.Id == m.ManufacturerId).FirstOrDefault();
                  m.ManufacturerName = manufacturer == null ? "" : manufacturer.Name;

                  //付款账户
                  m.PaymentReceiptBillAccountings = defaultAcc?.Item4?.OrderBy(s => s.Key).Select(s =>
                  {
                      var acc = b.PaymentReceiptBillAccountings.Where(a => a?.AccountingOption?.ParentId == s.Key).FirstOrDefault();
                      return new PaymentReceiptBillAccountingModel()
                      {
                          AccountingOptionId = acc?.AccountingOptionId ?? 0,
                          CollectionAmount = acc?.CollectionAmount ?? 0
                      };
                  }).ToList();

                  //总优惠金额(本次优惠金额总和)
                  m.DiscountAmount = b.Items.Sum(sb => sb.DiscountAmountOnce);

                  //付款前欠款金额
                  m.BeforArrearsAmount = b.Items.Sum(sb => sb.ArrearsAmount);

                  //剩余金额(付款后尚欠金额总和)
                  m.AmountOwedAfterReceipt = b.Items.Sum(sb => sb.AmountOwedAfterReceipt);

                  return m;
              }).ToList();

            return View(model);
        }

        /// <summary>
        /// 添加付款单
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        [HttpGet]
        [AuthCode((int)AccessGranularityEnum.PaymentBillsSave)]
        public IActionResult Create(int? store)
        {
            var model = new PaymentReceiptBillModel
            {
                BillTypeEnumId = (int)BillTypeEnum.PaymentReceiptBill,
                CreatedOnUtc = DateTime.Now,
                //付款人
                Draweers = BindUserSelection(_userService.BindUserList, curStore, "")
            };
            model.Draweer = model.Draweer = -1;

            //获取默认付款账户
            var defaultAcc = _accountingService.GetDefaultAccounting(curStore?.Id ?? 0, BillTypeEnum.PaymentReceiptBill);
            model.CollectionAccount = defaultAcc?.Item1?.Id ?? 0;
            model.PaymentReceiptBillAccountings.Add(new PaymentReceiptBillAccountingModel()
            {
                AccountingOptionId = defaultAcc?.Item1?.Id ?? 0,
                CollectionAmount = 0,
                Name = defaultAcc?.Item1?.Name,
                AccountCodeTypeId = defaultAcc?.Item1?.AccountCodeTypeId ?? 0
            });


            return View(model);
        }

        /// <summary>
        /// 编辑付款单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.PaymentBillsView)]
        public IActionResult Edit(int id = 0)
        {
            var model = new PaymentReceiptBillModel
            {
                BillTypeEnumId = (int)BillTypeEnum.PaymentReceiptBill
            };

            var bill = _paymentReceiptBillService.GetPaymentReceiptBillById(curStore.Id, id, true);
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

                model = bill.ToModel<PaymentReceiptBillModel>();
                model.BillBarCode = _mediaService.GenerateBarCodeForBase64(bill.BillNumber, 150, 50);
            }

            //获取默认付款账户
            var defaultAcc = _accountingService.GetDefaultAccounting(curStore?.Id ?? 0, BillTypeEnum.PaymentReceiptBill);
            model.CollectionAccount = defaultAcc?.Item1?.Id ?? 0;
            model.CollectionAmount = bill.PaymentReceiptBillAccountings.Where(sa => sa.AccountingOptionId == defaultAcc?.Item1?.Id).Sum(sa => sa.CollectionAmount);
            //付款账户
            model.PaymentReceiptBillAccountings = bill.PaymentReceiptBillAccountings.Select(s =>
            {
                var m = s.ToAccountModel<PaymentReceiptBillAccountingModel>();
                //m.Name = _accountingService.GetAccountingOptionById(s.AccountingOptionId).Name;
                m.Name = _accountingService.GetAccountingOptionName(curStore.Id, s.AccountingOptionId);
                return m;
            }).ToList();

            //经销商
            model.ManufacturerName = _manufacturerService.GetManufacturerName(curStore.Id, model.ManufacturerId);

            //付款人
            model.Draweers = BindUserSelection(_userService.BindUserList, curStore, "");

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
        /// 审核付款单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.PaymentBillsApproved)]
        public async Task<JsonResult> Auditing(int? id)
        {
            try
            {
                var bill = new PaymentReceiptBill();
                var recordingVoucher = new RecordingVoucher();

                #region 验证
                if (!id.HasValue)
                {
                    return Warning("参数错误.");
                }
                else
                {
                    bill = _paymentReceiptBillService.GetPaymentReceiptBillById(curStore.Id, id.Value, true);
                }

                //公共单据验证
                var commonBillChecking = BillChecking<PaymentReceiptBill, PaymentReceiptItem>(bill, BillStates.Audited);
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
                      () => _paymentReceiptBillService.Auditing(curStore.Id, curUser.Id, bill));
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
        [AuthCode((int)AccessGranularityEnum.PaymentBillsReverse)]
        public async Task<JsonResult> Reverse(int? id)
        {
            try
            {
                var bill = new PaymentReceiptBill() { StoreId = curStore?.Id ?? 0 };

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
                    bill = _paymentReceiptBillService.GetPaymentReceiptBillById(curStore.Id, id.Value, true);
                }

                //公共单据验证
                var commonBillChecking = BillChecking<PaymentReceiptBill, PaymentReceiptItem>(bill, BillStates.Reversed);
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
                      () => _paymentReceiptBillService.Reverse(curUser.Id, bill));
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
        /// 异步获取付款单项目
        /// </summary>
        /// <param name="paymentReceiptBillId"></param>
        /// <returns></returns>
        public async Task<JsonResult> AsyncPaymentReceiptItems(int? paymentReceiptBillId)
        {
            return await Task.Run(() =>
            {
                var items = _paymentReceiptBillService.GetPaymentReceiptItemList(paymentReceiptBillId ?? 0)
                .Select(o =>
                     {
                         var m = o.ToModel<PaymentReceiptItemModel>();
                         m.BillTypeName = CommonHelper.GetEnumDescription(o.BillTypeEnum);
                         m.BillLink = _billConvertService.GenerateBillUrl(m.BillTypeId, m.Id);
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
        /// 创建/更新付款单
        /// </summary>
        /// <param name="data"></param>
        /// <param name="billId"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthCode((int)AccessGranularityEnum.PaymentBillsSave)]
        public async Task<JsonResult> CreateOrUpdate(PaymentReceiptUpdateModel data, int? billId,bool doAudit = true)
        {
            try
            {
                PaymentReceiptBill paymentReceiptBill = new PaymentReceiptBill();

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
                    paymentReceiptBill = _paymentReceiptBillService.GetPaymentReceiptBillById(curStore.Id, billId.Value, true);

                    //公共单据验证
                    var commonBillChecking = BillChecking<PaymentReceiptBill, PaymentReceiptItem>(paymentReceiptBill, BillStates.Draft);
                    if (commonBillChecking.Value != null)
                    {
                        return commonBillChecking;
                    }
                }

                //判断指定单据是否尚有欠款(是否已经收完款)
                var isDebt = true;
                data.Items.ForEach(s =>
                {
                    isDebt = _paymentReceiptBillService.ThereAnyDebt(curStore.Id, s.BillTypeId, s.BillId);
                    if (!isDebt)
                    {
                        return;
                    }
                });
                if (!isDebt)
                {
                    return Warning("非法操作，单据存在已经结清款项.");
                }

                #endregion

                #region 验证预付款
                if (data.Accounting != null)
                {
                    if (data.AdvanceAmount > 0 && data.AdvanceAmount > data.AdvanceAmountBalance)
                    {
                        return Warning("预付款余额不足!");
                    }
                }
                #endregion

                //业务逻辑
                var accountings = _accountingService.GetAllAccountingOptions(curStore.Id, 0, true);
                var dataTo = data.ToEntity<PaymentReceiptBillUpdate>();
                dataTo.Operation = (int)OperationEnum.PC;
                if (data.Accounting == null)
                {
                    return Warning("没有默认的付款账号");
                }
                dataTo.Accounting = data.Accounting.Select(ac =>
                {
                    return ac.ToAccountEntity<PaymentReceiptBillAccounting>();
                }).ToList();
                dataTo.Items = data.Items.Select(it =>
                {
                    return it.ToEntity<PaymentReceiptItem>();
                }).ToList();

                //RedLock
                var result = await _locker.PerformActionWithLockAsync(LockKey(data),
                      TimeSpan.FromSeconds(30),
                      TimeSpan.FromSeconds(10),
                      TimeSpan.FromSeconds(1),
                      () => _paymentReceiptBillService.BillCreateOrUpdate(curStore.Id, curUser.Id, billId, paymentReceiptBill, dataTo.Accounting, accountings, dataTo, dataTo.Items, _userService.IsAdmin(curStore.Id, curUser.Id), doAudit));
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


        #region  付款单据类型汇总查询

        public JsonResult AsyncLoadOwecashBillsPopup(int manufacturerId = 0)
        {
            var model = new PaymentReceiptBillModel
            {
                StartTime = DateTime.Now.AddDays(-30),
                EndTime = DateTime.Now,
                //客户
                ManufacturerId = manufacturerId,
                //业务员
                Draweers = BindUserSelection(_userService.BindUserList, curStore, ""),
                //单据类型
                BillTypes = BindBillTypeSelection<BillTypeEnum>(ids: new BillTypeEnum[] {
                    BillTypeEnum.PurchaseBill,
                    BillTypeEnum.PurchaseReturnBill,
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
        /// <param name="manufacturerId"></param>
        /// <param name="billTypeId"></param>
        /// <param name="billNumber"></param>
        /// <param name="remark"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<JsonResult> AsyncLoadOwecashBills(int? payeer,
            int? manufacturerId,
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

                //获取指定单据初始欠款
                var bills = _paymentReceiptBillService.GetBillPaymentReceiptSummary(curStore?.Id ?? 0,
                    null,
                    manufacturerId, billTypeId, billNumber, remark, startTime, endTime);

                //重写计算： 优惠金额	 已付金额  尚欠金额
                foreach (var bill in bills)
                {
                    //采购单
                    if (bill.BillTypeId == (int)BillTypeEnum.PurchaseBill)
                    {
                        //单据金额
                        decimal calc_billAmount = Convert.ToDecimal(bill.Amount ?? 0) + Convert.ToDecimal(bill.DiscountAmount ?? 0);
                        //优惠金额 
                        decimal calc_discountAmount = 0;
                        //已付金额
                        decimal calc_paymentedAmount = 0;
                        //尚欠金额
                        decimal calc_arrearsAmount = 0;

                        #region 计算如下

                        //单已经付款部分的本次优惠合计
                        decimal discountAmountOnce = _commonBillService.GetPayBillDiscountAmountOnce(curStore?.Id ?? 0, bill.BillId);

                        //优惠金额 =  单据优惠金额  + （单已经付款部分的本次优惠合计）
                        calc_discountAmount = Convert.ToDecimal(bill.DiscountAmount ?? 0) + discountAmountOnce;

                        //单据付款金额（付款账户）3800.0000
                        decimal collectionAmount = _commonBillService.GetPayBillCollectionAmount(curStore?.Id ?? 0, bill.BillId, BillTypeEnum.PurchaseBill);

                        //单已经付款部分的本次付款合计60
                        decimal receivableAmountOnce = _commonBillService.GetPayBillReceivableAmountOnce(curStore?.Id ?? 0, bill.BillId);

                        //已付金额 = 单据付款金额（付款账户） + （单已经付款部分的本次付款合计）3860
                        calc_paymentedAmount = collectionAmount + receivableAmountOnce;

                        //尚欠金额 
                        //calc_billAmount = 3861.0000 
                        //calc_discountAmount = 0.0000
                        //calc_paymentedAmount = 3860.0000
                        calc_arrearsAmount = Convert.ToDecimal(Convert.ToDouble(calc_billAmount) - Convert.ToDouble(calc_discountAmount) - Convert.ToDouble(calc_paymentedAmount));

                        #endregion

                        //重新赋值
                        bill.Amount = calc_billAmount;
                        bill.DiscountAmount = calc_discountAmount;
                        bill.PaymentedAmount = calc_paymentedAmount;
                        bill.ArrearsAmount = calc_arrearsAmount;

                    }
                    //采购退货单
                    else if (bill.BillTypeId == (int)BillTypeEnum.PurchaseReturnBill)
                    {
                        //单据金额
                        decimal calc_billAmount = bill.Amount ?? 0;
                        //优惠金额 
                        decimal calc_discountAmount = 0;
                        //已付金额
                        decimal calc_paymentedAmount = 0;
                        //尚欠金额
                        decimal calc_arrearsAmount = 0;

                        #region 计算如下

                        //单已经付款部分的本次优惠合计
                        decimal discountAmountOnce = _commonBillService.GetPayBillDiscountAmountOnce(curStore?.Id ?? 0, bill.BillId);

                        //优惠金额 =  单据优惠金额  + （单已经付款部分的本次优惠合计）
                        calc_discountAmount = Convert.ToDecimal(bill.DiscountAmount ?? 0) + discountAmountOnce;

                        //单据付款金额（付款账户）
                        decimal collectionAmount = _commonBillService.GetPayBillCollectionAmount(curStore?.Id ?? 0, bill.BillId, BillTypeEnum.PurchaseReturnBill);

                        //单已经付款部分的本次付款合计
                        decimal receivableAmountOnce = _commonBillService.GetPayBillReceivableAmountOnce(curStore?.Id ?? 0, bill.BillId);

                        //已付金额 = 单据付款金额（付款账户） + （单已经付款部分的本次付款合计）
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
                        //已付金额
                        decimal calc_paymentedAmount = 0;
                        //尚欠金额
                        decimal calc_arrearsAmount = 0;

                        #region 计算如下

                        //单已经付款部分的本次优惠合计
                        decimal discountAmountOnce = _commonBillService.GetPayBillDiscountAmountOnce(curStore?.Id ?? 0, bill.BillId);

                        //优惠金额 =  单据优惠金额  + （单已经付款部分的本次优惠合计）
                        calc_discountAmount = Convert.ToDecimal(bill.DiscountAmount ?? 0) + discountAmountOnce;

                        //单据付款金额（付款账户）
                        decimal collectionAmount = _commonBillService.GetPayBillCollectionAmount(curStore?.Id ?? 0, bill.BillId, BillTypeEnum.FinancialIncomeBill);

                        //单已经付款部分的本次付款合计
                        decimal receivableAmountOnce = _commonBillService.GetPayBillReceivableAmountOnce(curStore?.Id ?? 0, bill.BillId);

                        //已付金额 = 单据付款金额（付款账户） + （单已经付款部分的本次付款合计）
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
                }


                billSummaries = bills.Select(s =>
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
        [AuthCode((int)AccessGranularityEnum.PaymentBillsPrint)]
        public JsonResult PrintSetting()
        {
            var printTemplates = _printTemplateService.GetAllPrintTemplates(curStore.Id).ToList();
            var printTemplate = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.PaymentReceiptBill).FirstOrDefault();
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
        [AuthCode((int)AccessGranularityEnum.PaymentBillsPrint)]
        public JsonResult Print(int type, string selectData, int? manufacturerId, int? draweer, string billNumber = "", bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? showReverse = null, bool? sortByAuditedTime = null)
        {
            try
            {

                bool fg = true;
                string errMsg = string.Empty;

                #region 查询打印数据

                IList<PaymentReceiptBill> paymentReceiptBills = new List<PaymentReceiptBill>();
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
                            PaymentReceiptBill paymentReceiptBill = _paymentReceiptBillService.GetPaymentReceiptBillById(curStore.Id, int.Parse(id), true);
                            if (paymentReceiptBill != null)
                            {
                                paymentReceiptBills.Add(paymentReceiptBill);
                            }
                        }
                    }
                }
                else if (type == 2)
                {
                    paymentReceiptBills = _paymentReceiptBillService.GetAllPaymentReceiptBills(
                                curStore?.Id ?? 0,
                                 curUser.Id,
                                draweer,
                                manufacturerId,
                                billNumber,
                                auditedStatus,
                                startTime,
                                endTime,
                                showReverse,
                                sortByAuditedTime);
                }

                #endregion

                #region 修改数据
                if (paymentReceiptBills != null && paymentReceiptBills.Count > 0)
                {
                    //using (var scope = new TransactionScope())
                    //{

                    //    scope.Complete();
                    //}
                    #region 修改单据表打印数
                    foreach (var d in paymentReceiptBills)
                    {
                        d.PrintNum = (d.PrintNum ?? 0) + 1;
                        _paymentReceiptBillService.UpdatePaymentReceiptBill(d);
                    }
                    #endregion
                }

                //获取打印模板
                var printTemplates = _printTemplateService.GetAllPrintTemplates(curStore.Id).ToList();
                var content = printTemplates.Where(a => a.BillType == (int)BillTypeEnum.PaymentReceiptBill).Select(a => a.Content).FirstOrDefault();

                //获取打印设置
                var pCPrintSetting = _settingService.LoadSetting<PCPrintSetting>(curStore.Id);

                //填充打印数据
                foreach (var d in paymentReceiptBills)
                {

                    StringBuilder sb = new StringBuilder();
                    sb.Append(content);

                    #region theadid
                    //sb.Replace("@商铺名称", curStore.Name);
                    if (pCPrintSetting != null)
                    {
                        sb.Replace("@商铺名称", string.IsNullOrWhiteSpace(pCPrintSetting.StoreName) ? "&nbsp;" : pCPrintSetting.StoreName);
                    }

                    Manufacturer manufacturer = _manufacturerService.GetManufacturerById(curStore.Id, d.ManufacturerId);
                    if (manufacturer != null)
                    {
                        sb.Replace("@供应商名称", manufacturer.Name);
                    }

                    User businessUser = _userService.GetUserById(curStore.Id, d.MakeUserId);
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
                            sb2.Replace("#已付金额", item.PaymentedAmount == null ? "0.00" : item.PaymentedAmount?.ToString("0.00"));
                            sb2.Replace("#尚欠金额", item.ArrearsAmount == null ? "0.00" : item.ArrearsAmount?.ToString("0.00"));
                            sb2.Replace("#本次付款金额", item.ReceivableAmountOnce == null ? "0.00" : item.ReceivableAmountOnce?.ToString("0.00"));

                            sb.Insert(index, sb2);

                        }

                        sb.Replace("单据金额:###", d.Items.Sum(s => s.Amount ?? 0).ToString("0.00"));
                        sb.Replace("已付金额:###", d.Items.Sum(s => s.PaymentedAmount ?? 0).ToString("0.00"));
                        sb.Replace("尚欠金额:###", d.Items.Sum(s => s.ArrearsAmount ?? 0).ToString("0.00"));
                        sb.Replace("本次付款金额:###", d.Items.Sum(s => s.ReceivableAmountOnce ?? 0).ToString("0.00"));
                    }
                    #endregion

                    #region tfootid
                    User makeUser = _userService.GetUserById(curStore.Id, d.MakeUserId);
                    if (makeUser != null)
                    {
                        sb.Replace("@制单", makeUser.UserRealName);
                    }
                    sb.Replace("@日期", d.CreatedOnUtc.ToString("yyyy/MM/dd HH:mm:ss"));
                    sb.Replace("@打印日期", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
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

                    sb.Replace("@备注", d.Remark);
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
        [AuthCode((int)AccessGranularityEnum.PaymentBillsExport)]
        public FileResult Export(int type, string selectData, int? manufacturerId, int? draweer, string billNumber = "", bool? auditedStatus = null, DateTime? startTime = null, DateTime? endTime = null, bool? showReverse = null, bool? sortByAuditedTime = null)
        {

            #region 查询导出数据
            IList<PaymentReceiptBill> paymentReceiptBills = new List<PaymentReceiptBill>();

            if (type == 1)
            {
                if (!string.IsNullOrEmpty(selectData))
                {
                    List<string> ids = selectData.Split(',').ToList();
                    foreach (var id in ids)
                    {
                        PaymentReceiptBill paymentReceiptBill = _paymentReceiptBillService.GetPaymentReceiptBillById(curStore.Id, int.Parse(id), true);
                        if (paymentReceiptBill != null)
                        {
                            paymentReceiptBills.Add(paymentReceiptBill);
                        }
                    }
                }
            }
            else if (type == 2)
            {
                paymentReceiptBills = _paymentReceiptBillService.GetAllPaymentReceiptBills(
                            curStore?.Id ?? 0,
                             curUser.Id,
                            draweer,
                            manufacturerId,
                            billNumber,
                            auditedStatus,
                            startTime,
                            endTime,
                            showReverse,
                            sortByAuditedTime);
            }

            #endregion

            #region 导出
            var ms = _exportManager.ExportPaymentReceiptBillToXlsx(paymentReceiptBills, curStore.Id);
            if (ms != null)
            {
                return File(ms, "application/vnd.ms-excel", "付款单.xlsx");
            }
            else
            {
                return File(new MemoryStream(), "application/vnd.ms-excel", "付款单.xlsx");
            }
            #endregion

        }
    }
}