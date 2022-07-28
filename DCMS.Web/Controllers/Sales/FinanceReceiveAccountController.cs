using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Services.Common;
using DCMS.Services.Configuration;
using DCMS.Services.Finances;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Sales;
using DCMS.Services.Settings;
using DCMS.Services.Users;
using DCMS.ViewModel.Models.Sales;
using DCMS.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using DCMS.Core.Domain.Sales;

namespace DCMS.Web.Controllers
{

    /// <summary>
    /// 收款对账单
    /// </summary>
    public class FinanceReceiveAccountController : BasePublicController
    {
        private readonly IUserService _userService;
        private readonly IUserActivityService _userActivityService;
        private readonly ISettingService _settingService;
        private readonly IFinanceReceiveAccountBillService _financeReceiveAccountBillService;
        //销售单
        private readonly ISaleBillService _saleService;
        //退货单
        private readonly IReturnBillService _returnBillService;
        //收款单
        private readonly ICashReceiptBillService _cashReceiptBillService;
        //收预收款
        private readonly IAdvanceReceiptBillService _advanceReceiptBillService;
        //费用支出
        private readonly ICostExpenditureBillService _costExpenditureBillService;
        private readonly IRedLocker _locker;
        private readonly IAccountingService _accountingService;
        private readonly IBillConvertService _billConvertService;


        public FinanceReceiveAccountController(
            IWorkContext workContext,
            IStoreContext storeContext,
            ILogger loggerService,
            IUserService userService,
            IUserActivityService userActivityService,
            IFinanceReceiveAccountBillService financeReceiveAccountBillService,
            ISaleBillService saleService,
            IReturnBillService returnBillService,
            ICashReceiptBillService cashReceiptBillService,
            IAdvanceReceiptBillService advanceReceiptBillService,
            ICostExpenditureBillService costExpenditureBillService,
            ISettingService settingService,
            INotificationService notificationService,
            IAccountingService accountingService,
            IBillConvertService billConvertService,
            IRedLocker locker
            ) : base(workContext, loggerService, storeContext, notificationService)
        {
            _userService = userService;
            _userActivityService = userActivityService;
            _settingService = settingService;
            _financeReceiveAccountBillService = financeReceiveAccountBillService;
            _saleService = saleService;
            _returnBillService = returnBillService;
            _cashReceiptBillService = cashReceiptBillService;
            _advanceReceiptBillService = advanceReceiptBillService;
            _costExpenditureBillService = costExpenditureBillService;
            _accountingService = accountingService;
            _locker = locker;
            _billConvertService = billConvertService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }



        [AuthCode((int)AccessGranularityEnum.AccountReceivableView)]
        public async Task<IActionResult> List(int? employeeId, int? payeerId, int? paymentId, string billNumber = "", DateTime? startTime = null, DateTime? endTime = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var model = new FinanceReceiveAccountBillListModel
            {
                StartTime = startTime ?? DateTime.Parse(DateTime.Now.AddMonths(-1).ToString("yyyy-MM-01")),
                EndTime = endTime ?? DateTime.Now.AddDays(1),

                //员工
                EmployeeId = employeeId,
                Employees = BindUserSelection(_userService.BindUserList, curStore, "", 0),

                //收款人
                PayeerId = payeerId,
                Payeers = BindUserSelection(_userService.BindUserList, curStore, "", 0),

                BillNumber = billNumber
            };

            //支付方式
            var alls = _accountingService.GetAllAccounts(curStore?.Id ?? 0);
            var accounts = _accountingService.GetReceiptAccounting(curStore?.Id ?? 0, BillTypeEnum.FinanceReceiveAccount, 0, alls?.ToList());
            model.PaymentId = paymentId;
            model.Payments = BindAccountSelection(accounts.Item1.OrderBy(s => s.Id).ToList(), 0, 0);

            //支付方式动态列
            model.DynamicColumns = accounts.Item1.Select(s => s.Name).ToList();

            //待上交
            var lists = await Task.Run(() =>
             {
                 var bills = new List<FinanceReceiveAccountBillModel>();
                 var summeries = _financeReceiveAccountBillService.GetFinanceReceiveAccounts(curStore?.Id ?? 0,
                 model.StartTime,
                 model.EndTime,
                 model.EmployeeId,
                 model.PayeerId,
                 model.PaymentId,
                 model.BillNumber,
                 pageIndex, pageSize);

                 if (summeries != null && summeries.Any())
                 {
                     //重算金额
                     foreach (var item in summeries)
                     {
                         var sales = _financeReceiveAccountBillService.GetRankProducts(curStore?.Id ?? 0, false, item.UserId, (int)BillTypeEnum.SaleBill, model.StartTime, model.EndTime, new int[] { item.BillId })?.ToList();
                         var gifts = _financeReceiveAccountBillService.GetRankProducts(curStore?.Id ?? 0, true, item.UserId, (int)item.BillType, model.StartTime, model.EndTime, new int[] { item.BillId })?.ToList();
                         var returns = _financeReceiveAccountBillService.GetRankProducts(curStore?.Id ?? 0, false, item.UserId, (int)BillTypeEnum.ReturnBill, model.StartTime, model.EndTime, new int[] { item.BillId })?.ToList();

                         var bill = new FinanceReceiveAccountBillModel
                         {
                             TerminalId = item.TerminalId,
                             TerminalName = item.TerminalName,

                             // 单据编号
                             BillNumber = item.BillNumber,
                             BillType = item.BillType,
                             BillLink = _billConvertService.GenerateBillUrl(item.BillType, item.BillId),

                             /// 业务员
                             UserId = item.UserId,
                             UserName = _userService.GetUserName(curStore?.Id ?? 0, item.UserId),

                             /// 上交状态
                             HandInStatus = item.HandInStatus,

                             /// 待交金额
                             PaidAmount = item.PaidAmount,
                             /// 电子支付金额
                             EPaymentAmount = item.EPaymentAmount,

                             // 销售收款 = 销售金额-预收款-欠款
                             SaleAmountSum = (item.SaleAmount - item.SaleAdvanceReceiptAmount - item.SaleOweCashAmount),
                             SaleAmount = item.SaleAmount,
                             SaleAdvanceReceiptAmount = item.SaleAdvanceReceiptAmount,
                             SaleOweCashAmount = item.SaleOweCashAmount,


                             // 退货款 =退款金额-预收款-欠款
                             ReturnAmountSum = - (item.ReturnAmount - item.ReturnAdvanceReceiptAmount - item.ReturnOweCashAmount),
                             ReturnAmount = - item.ReturnAmount,
                             ReturnAdvanceReceiptAmount = - item.ReturnAdvanceReceiptAmount,
                             ReturnOweCashAmount = - item.ReturnOweCashAmount,


                             // 收欠款 =应收金额-预收款
                             ReceiptCashOweCashAmountSum = (item.ReceiptCashReceivableAmount - item.ReceiptCashAdvanceReceiptAmount),
                             ReceiptCashReceivableAmount = item.ReceiptCashReceivableAmount,
                             ReceiptCashAdvanceReceiptAmount = item.ReceiptCashAdvanceReceiptAmount,

                             //收预收款 =预收金额-欠款
                             AdvanceReceiptSum = (item.AdvanceReceiptAmount - item.AdvanceReceiptOweCashAmount),
                             AdvanceReceiptAmount = item.AdvanceReceiptAmount,
                             AdvanceReceiptOweCashAmount = item.AdvanceReceiptOweCashAmount,

                             //费用支出 = 支出金额-欠款
                             CostExpenditureSum = (item.CostExpenditureAmount - item.CostExpenditureOweCashAmount) == 0 ? 0 : -(item.CostExpenditureAmount - item.CostExpenditureOweCashAmount),
                             CostExpenditureAmount = - item.CostExpenditureAmount == 0 ? 0 : -item.CostExpenditureAmount,
                             CostExpenditureOweCashAmount = - item.CostExpenditureOweCashAmount,

                             // 单据ID
                             BillId = item.BillId,

                             // 优惠金额
                             PreferentialAmount = item.PreferentialAmount,

                             // 销售商品
                             SaleProductCount = sales?.GroupBy(s => s.CategoryId).Count() ?? 0,
                             SaleProducts = sales,

                             // 赠送商品
                             GiftProductCount = gifts?.GroupBy(s => s.CategoryId).Count() ?? 0,
                             GiftProducts = gifts,

                             // 退货商品
                             ReturnProductCount = returns?.GroupBy(s => s.CategoryId).Count() ?? 0,
                             ReturnProducts = returns,

                             // 创建时间
                             CreatedOnUtc = item.CreatedOnUtc
                         };
                         int ratio = bill.BillType switch
                         {
                             (int)BillTypeEnum.ReturnReservationBill =>  -1,
                             (int)BillTypeEnum.ReturnBill => -1,
                             (int)BillTypeEnum.PurchaseBill => -1,
                             (int)BillTypeEnum.PaymentReceiptBill => -1,
                             (int)BillTypeEnum.AdvancePaymentBill => -1,
                             (int)BillTypeEnum.CostExpenditureBill => -1,
                             _ => 1,
                         };
                         var accs = accounts.Item1.Select(s =>
                         {
                             var curAcc = item?.Accounts?.Where(a => a.AccountingOptionId == s.Id).FirstOrDefault();
                             return new FinanceReceiveAccountBillAccounting()
                             {
                                 AccountingOptionId = (curAcc?.Id ?? 0) == 0 ? s.Id : (curAcc?.Id ?? 0),
                                 CollectionAmount = (curAcc?.CollectionAmount ?? 0) * ratio,
                                 AccountingOptionName = string.IsNullOrEmpty(curAcc?.AccountingOption?.Name) ? s.Name : curAcc?.AccountingOption?.Name
                             };
                         }).ToList();

                         bill.Accounts = accs;

                         bills.Add(bill);
                     }
                 }

                 return bills;
             });

            //已上交
            var submitlists = await Task.Run(() =>
            {

                var submitBills = new List<FinanceReceiveAccountBillModel>();

                var submitSummeries = _financeReceiveAccountBillService.GetSubmittedBills(curStore?.Id ?? 0,
                model.StartTime,
                model.EndTime,
                model.EmployeeId,
                0,
                model.BillNumber,
                pageIndex, pageSize);

                if (submitSummeries != null && submitSummeries.Any())
                {
                    //重算金额
                    foreach (var item in submitSummeries)
                    {
                        var sales = _financeReceiveAccountBillService.GetRankProducts(curStore?.Id ?? 0, false, item.UserId, (int)BillTypeEnum.SaleBill, model.StartTime, model.EndTime, new int[] { item.BillId })?.ToList();
                        var gifts = _financeReceiveAccountBillService.GetRankProducts(curStore?.Id ?? 0, true, item.UserId, (int)item.BillType, model.StartTime, model.EndTime, new int[] { item.BillId })?.ToList();
                        var returns = _financeReceiveAccountBillService.GetRankProducts(curStore?.Id ?? 0, false, item.UserId, (int)BillTypeEnum.ReturnBill, model.StartTime, model.EndTime, new int[] { item.BillId })?.ToList();
                        item.Accounts = _financeReceiveAccountBillService.GetFinanceReceiveAccountBillAccountings(curStore?.Id ?? 0, item.Id)?.ToList();

                        var bill = new FinanceReceiveAccountBillModel
                        {
                            Id = item.Id,
                            TerminalId = 0,
                            TerminalName = "",

                            // 单据编号
                            BillNumber = item.BillNumber,
                            BillType = (int)item.BillType,
                            BillLink = _billConvertService.GenerateBillUrl((int)item.BillType, item.BillId),

                            /// 业务员
                            UserId = item.UserId,
                            UserName = _userService.GetUserName(curStore?.Id ?? 0, item.UserId),

                            /// 上交状态
                            HandInStatus = item.HandInStatus,

                            /// 待交金额
                            PaidAmount = item.PaidAmount,
                            /// 电子支付金额
                            EPaymentAmount = item.EPaymentAmount,

                            // 销售收款 = 销售金额-预收款-欠款
                            SaleAmountSum = (item.SaleAmount - item.SaleAdvanceReceiptAmount - item.SaleOweCashAmount),
                            SaleAmount = item.SaleAmount,
                            SaleAdvanceReceiptAmount = item.SaleAdvanceReceiptAmount,
                            SaleOweCashAmount = item.SaleOweCashAmount,


                            // 退货款 =退款金额-预收款-欠款
                            ReturnAmountSum = - (item.ReturnAmount - item.ReturnAdvanceReceiptAmount - item.ReturnOweCashAmount),
                            ReturnAmount = - item.ReturnAmount,
                            ReturnAdvanceReceiptAmount = - item.ReturnAdvanceReceiptAmount,
                            ReturnOweCashAmount = - item.ReturnOweCashAmount,


                            // 收欠款 =应收金额-预收款
                            ReceiptCashOweCashAmountSum = (item.ReceiptCashReceivableAmount - item.ReceiptCashAdvanceReceiptAmount),
                            ReceiptCashReceivableAmount = item.ReceiptCashReceivableAmount,
                            ReceiptCashAdvanceReceiptAmount = item.ReceiptCashAdvanceReceiptAmount,

                            //收预收款 =预收金额-欠款
                            AdvanceReceiptSum = (item.AdvanceReceiptAmount - item.AdvanceReceiptOweCashAmount),
                            AdvanceReceiptAmount = item.AdvanceReceiptAmount,
                            AdvanceReceiptOweCashAmount = item.AdvanceReceiptOweCashAmount,

                            //费用支出 = 支出金额-欠款
                            CostExpenditureSum = (item.CostExpenditureAmount - item.CostExpenditureOweCashAmount) == 0 ? 0 : -(item.CostExpenditureAmount - item.CostExpenditureOweCashAmount),
                            CostExpenditureAmount = item.CostExpenditureAmount == 0 ? 0 : -item.CostExpenditureAmount,
                            CostExpenditureOweCashAmount = - item.CostExpenditureOweCashAmount,

                            // 单据ID
                            BillId = item.BillId,

                            // 优惠金额
                            PreferentialAmount = item.PreferentialAmount,

                            // 销售商品
                            SaleProductCount = sales?.GroupBy(s => s.CategoryId).Count() ?? 0,
                            SaleProducts = sales,

                            // 赠送商品
                            GiftProductCount = gifts?.GroupBy(s => s.CategoryId).Count() ?? 0,
                            GiftProducts = gifts,

                            // 退货商品
                            ReturnProductCount = returns?.GroupBy(s => s.CategoryId).Count() ?? 0,
                            ReturnProducts = returns,

                            // 创建时间
                            CreatedOnUtc = item.CreatedOnUtc
                        };

                        var accs = accounts.Item1.Select(s =>
                        {
                            var curAcc = item?.Accounts?.Where(a => a.AccountingOptionId == s.Id).FirstOrDefault();
                            return new FinanceReceiveAccountBillAccounting()
                            {
                                Id = curAcc?.Id ?? 0,
                                AccountingOptionId = (curAcc?.AccountingOptionId ?? 0) == 0 ? s.Id : (curAcc?.AccountingOptionId ?? 0),
                                CollectionAmount = curAcc?.CollectionAmount ?? 0,
                                AccountingOptionName = string.IsNullOrEmpty(curAcc?.AccountingOption?.Name) ? s.Name : curAcc?.AccountingOption?.Name
                            };
                        }).ToList();

                        bill.Accounts = accs;

                        submitBills.Add(bill);
                    }
                }

                return submitBills;
            });
            model.Lists = lists;
            model.SubmitLists = submitlists;

            return View(model);
        }

        /// <summary>
        /// 商品汇总POPup
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="billType"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public JsonResult AsyncShowProductsPopup(int? userId, int billType, bool gift, DateTime? start, DateTime? end)
        {
            var model = new FinanceReceiveAccountBillListModel
            {

                EmployeeId = userId,
                StartTime = start ?? DateTime.Now,
                EndTime = start ?? DateTime.Now,
                BillType = billType,
                Gift = gift
            };

            return Json(new
            {
                Success = true,
                RenderHtml = RenderPartialViewToString("AsyncShowProducts", model)
            });
        }

        /// <summary>
        /// 获取商品汇总
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="userId"></param>
        /// <param name="billType"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<JsonResult> AsyncShowProducts(int? storeId, int? userId, bool gift, int billType, DateTime? start, DateTime? end)
        {
            if (!storeId.HasValue)
            {
                storeId = curStore?.Id ?? 0;
            }

            return await Task.Run(() =>
            {

                var rows = _financeReceiveAccountBillService.GetRankProducts(storeId.Value, gift, userId, billType, start, end);
                return Json(new
                {
                    total = rows.TotalCount,
                    rows
                });
            });
        }

        /// <summary>
        /// 撤销上交
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.AccountReceivableRevocation)]
        public IActionResult Delete(int? id)
        {
            try
            {
                var bill = _financeReceiveAccountBillService.GetFinanceReceiveAccountBillById(curStore.Id, id ?? 0, true);

                if (bill != null)
                {
                    //收款对账科目映射
                    if (bill.FinanceReceiveAccountBillAccountings.Any())
                    {
                        bill.FinanceReceiveAccountBillAccountings.ToList().ForEach(acc =>
                        {
                            _financeReceiveAccountBillService.DeleteFinanceReceiveAccountBillAccounting(acc);
                        });
                    }

                    //删除收款对账单
                    _financeReceiveAccountBillService.DeleteFinanceReceiveAccountBill(bill);

                    //关联单据修改
                    switch (bill.BillTypeId)
                    {
                        case (int)BillTypeEnum.SaleBill:
                            {
                                var sbill = _saleService.GetSaleBillById(curStore.Id, bill.BillId, false);
                                if (sbill != null)
                                {
                                    sbill.HandInStatus = false;
                                    sbill.HandInDate = null;
                                    _saleService.UpdateSaleBill(sbill);
                                }
                            }
                            break;
                        case (int)BillTypeEnum.ReturnBill:
                            {
                                var rbill = _returnBillService.GetReturnBillById(curStore.Id, bill.BillId, false);
                                if (rbill != null)
                                {
                                    rbill.HandInStatus = false;
                                    rbill.HandInDate = null;
                                    _returnBillService.UpdateReturnBill(rbill);
                                }
                            }
                            break;
                        case (int)BillTypeEnum.CashReceiptBill:
                            {
                                var cbill = _cashReceiptBillService.GetCashReceiptBillById(curStore.Id, bill.BillId, false);
                                if (cbill != null)
                                {
                                    cbill.HandInStatus = false;
                                    cbill.HandInDate = null;
                                    _cashReceiptBillService.UpdateCashReceiptBill(cbill);
                                }
                            }
                            break;
                        case (int)BillTypeEnum.AdvanceReceiptBill:
                            {
                                var abill = _advanceReceiptBillService.GetAdvanceReceiptBillById(curStore.Id, bill.BillId, false);
                                if (abill != null)
                                {
                                    abill.HandInStatus = false;
                                    abill.HandInDate = null;
                                    _advanceReceiptBillService.UpdateAdvanceReceiptBill(abill);
                                }
                            }
                            break;
                        case (int)BillTypeEnum.CostExpenditureBill:
                            {
                                var ebill = _costExpenditureBillService.GetCostExpenditureBillById(curStore.Id, bill.BillId, false);
                                if (ebill != null)
                                {
                                    ebill.HandInStatus = false;
                                    ebill.HandInDate = null;
                                    _costExpenditureBillService.UpdateCostExpenditureBill(ebill);
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception){}

            return RedirectToAction("List");
        }

    }
}