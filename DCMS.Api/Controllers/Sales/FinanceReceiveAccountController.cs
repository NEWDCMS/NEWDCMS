using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Services.Common;
using DCMS.Services.Sales;
using DCMS.Services.Settings;
using DCMS.Services.Users;
using DCMS.ViewModel.Models.Sales;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using DCMS.Api.Infrastructure.Mapper.Extensions;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Domain.Tasks;
using DCMS.Services.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace DCMS.Api.Controllers
{

    /// <summary>
    /// 收款对账单
    /// </summary>
   [Authorize]
    [Route("api/v{version:apiVersion}/dcms/sales")]
    public class FinanceReceiveAccountController : BaseAPIController
    {
        private readonly IUserService _userService;
        private readonly IFinanceReceiveAccountBillService _financeReceiveAccountBillService;
        private readonly IAccountingService _accountingService;

        private readonly IRedLocker _locker;
        private readonly IBillConvertService _billConvertService;
        private readonly IQueuedMessageService _queuedMessageService;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="userService"></param>
        /// <param name="financeReceiveAccountBillService"></param>
        /// <param name="accountingService"></param>
        /// <param name="billConvertService"></param>
        /// <param name="locker"></param>
        /// <param name="logger"></param>
        public FinanceReceiveAccountController(
            IUserService userService,
            IFinanceReceiveAccountBillService financeReceiveAccountBillService,
            IAccountingService accountingService,
            IBillConvertService billConvertService,
            IRedLocker locker,
            ILogger<BaseAPIController> logger) : base(logger)
        {
            _userService = userService;
            _financeReceiveAccountBillService = financeReceiveAccountBillService;
            _accountingService = accountingService;
            _billConvertService = billConvertService;
            _locker = locker;
        }

        /// <summary>
        ///  获取已上交对账单据
        /// </summary>
        /// <param name="store"></param>
        /// <param name="businessUserId"></param>
        /// <param name="billTypeId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="billNumber"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("financereceiveaccount/getBills/{store}/{businessUserId}")]
        [SwaggerOperation("getBills")]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<FinanceReceiveAccountBillModel>>> GetSubmittedBills(int? store, int? businessUserId, int? billTypeId, DateTime? startTime = null, DateTime? endTime = null, string billNumber = "", int pageIndex = 0, int pageSize = 20)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<FinanceReceiveAccountBillModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    //支付方式
                    var alls = _accountingService.GetAllAccounts(store ?? 0);
                    var accounts = _accountingService.GetReceiptAccounting(store ?? 0, BillTypeEnum.FinanceReceiveAccount, 0, alls?.ToList());

                    var bills = new List<FinanceReceiveAccountBillModel>();
                    var summeries = _financeReceiveAccountBillService.GetSubmittedBills(store ?? 0,
                        startTime,
                        endTime,
                        businessUserId,
                        billTypeId,
                        billNumber,
                        pageIndex,
                        pageSize);

                    return this.Successful<FinanceReceiveAccountBillModel>("", summeries.Select(s => s.ToModel<FinanceReceiveAccountBillModel>()).ToList());

                }
                catch (Exception ex)
                {
                    return this.Error<FinanceReceiveAccountBillModel>(ex.Message);
                }

            });
        }


        /// <summary>
        /// 获取待上交对账单汇总
        /// </summary>
        /// <param name="store"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="businessUserId"></param>
        /// <param name="payeer"></param>
        /// <param name="accountingOptionId"></param>
        /// <param name="billNumber"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("financereceiveaccount/getfinancereceiveaccounts/{store}/{businessUserId}")]
        [SwaggerOperation("getfinancereceiveaccounts")]
        //[AuthBaseFilter]
        public async Task<APIResult<IList<FinanceReceiveAccountBillModel>>> GetFinanceReceiveAccounts(int? store, DateTime? start, DateTime? end, int? businessUserId, int? payeer, int? accountingOptionId, string billNumber = "", int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error<FinanceReceiveAccountBillModel>(Resources.ParameterError);

            return await Task.Run(() =>
            {
                try
                {
                    //支付方式
                    var alls = _accountingService.GetAllAccounts(store ?? 0);
                    var accounts = _accountingService.GetReceiptAccounting(store ?? 0, BillTypeEnum.FinanceReceiveAccount, 0, alls?.ToList());

                    var bills = new List<FinanceReceiveAccountBillModel>();
                    var summeries = _financeReceiveAccountBillService.GetFinanceReceiveAccounts(store ?? 0,
                        start,
                        end,
                        businessUserId,
                        payeer,
                        accountingOptionId,
                        billNumber,
                        pageIndex, 
                        100);

                    if (summeries != null && summeries.Any())
                    {
                        //重算金额
                        foreach (var item in summeries)
                        {
                            var sales = _financeReceiveAccountBillService.GetRankProducts(store ?? 0, false, businessUserId, 12, start, end, new int[] { item.BillId })?.ToList();

                            var gifts = _financeReceiveAccountBillService.GetRankProducts(store ?? 0, true, businessUserId, 12, start, end, new int[] { item.BillId })?.ToList();

                            var returns = _financeReceiveAccountBillService.GetRankProducts(store ?? 0, false, businessUserId, 14, start, end, new int[] { item.BillId })?.ToList();

                            var bill = new FinanceReceiveAccountBillModel
                            {
                                //客户
                                TerminalId = item.TerminalId,
                                TerminalName = item.TerminalName,

                                // 单据编号
                                BillNumber = item.BillNumber,
                                BillType = item.BillType,
                                BillLink = _billConvertService.GenerateBillUrl(item.BillType, item.BillId),

                                // 业务员
                                UserId = item.UserId,
                                UserName = _userService.GetUserName(store ?? 0, item.UserId),

                                // 上交状态
                                HandInStatus = item.HandInStatus,

                                // 待交金额
                                PaidAmount = item.PaidAmount,
                                // 电子支付金额
                                EPaymentAmount = item.EPaymentAmount,

                                // 销售收款 = 销售金额-预收款-欠款
                                SaleAmountSum = (item.SaleAmount - item.SaleAdvanceReceiptAmount - item.SaleOweCashAmount),
                                SaleAmount = item.SaleAmount,
                                SaleAdvanceReceiptAmount = item.SaleAdvanceReceiptAmount,
                                SaleOweCashAmount = item.SaleOweCashAmount,


                                // 退货款 =退款金额-预收款-欠款
                                ReturnAmountSum = (item.ReturnAmount - item.ReturnAdvanceReceiptAmount - item.ReturnOweCashAmount),
                                ReturnAmount = item.ReturnAmount,
                                ReturnAdvanceReceiptAmount = item.ReturnAdvanceReceiptAmount,
                                ReturnOweCashAmount = item.ReturnOweCashAmount,


                                // 收欠款 =应收金额-预收款
                                ReceiptCashOweCashAmountSum = (item.ReceiptCashReceivableAmount - item.ReceiptCashAdvanceReceiptAmount),
                                ReceiptCashReceivableAmount = item.ReceiptCashReceivableAmount,
                                ReceiptCashAdvanceReceiptAmount = item.ReceiptCashAdvanceReceiptAmount,

                                //收预收款 =预收金额-欠款
                                AdvanceReceiptSum = (item.AdvanceReceiptAmount - item.AdvanceReceiptOweCashAmount),
                                AdvanceReceiptAmount = item.AdvanceReceiptAmount,
                                AdvanceReceiptOweCashAmount = item.AdvanceReceiptOweCashAmount,

                                //费用支出 = 支出金额-欠款
                                CostExpenditureSum = (item.CostExpenditureAmount - item.CostExpenditureOweCashAmount),
                                CostExpenditureAmount = item.CostExpenditureAmount,
                                CostExpenditureOweCashAmount = item.CostExpenditureOweCashAmount,

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
                                    AccountingOptionId = (curAcc?.Id ?? 0) == 0 ? s.Id : (curAcc?.Id ?? 0),
                                    CollectionAmount = curAcc?.CollectionAmount ?? 0,
                                    AccountingOptionName = string.IsNullOrEmpty(curAcc?.AccountingOption?.Name) ? s.Name : curAcc?.AccountingOption?.Name
                                };
                            }).OrderBy(s => s.AccountingOptionId).ToList();

                            bill.Accounts = accs;

                            bills.Add(bill);
                        }
                    }

                    return this.Successful("", bills);

                }
                catch (Exception ex)
                {
                    return this.Error<FinanceReceiveAccountBillModel>(ex.StackTrace);
                }

            });
        }



        /// <summary>
        /// 上交对账单
        /// </summary>
        /// <param name="data"></param>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost("financereceiveaccount/submitaccountstatement/{store}/{userId}")]
        [SwaggerOperation("submitaccountstatement")]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> SubmitAccountStatement(FinanceReceiveAccountBillModel data, int? store, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(async () =>
            {
                try
                {
                    if (!userId.HasValue || userId.Value == 0)
                        return this.Warning("开单人为指定.");

                    if (data == null)
                        return this.Warning("请录入数据.");

                    if (PeriodLocked(DateTime.Now, store ?? 0))
                        return this.Warning("锁账期间,禁止业务操作.");

                    if (PeriodClosed(DateTime.Now, store ?? 0))
                        return this.Warning("会计期间已结账,禁止业务操作.");

                    //Map
                    var bill = data.ToEntity<FinanceReceiveAccountBill>();
                    if (bill == null)
                        return this.Warning("数据转换失败.");

                    bill.StoreId = store ?? 0;
                    bill.BillNumber = data.BillNumber;
                    bill.Remark = data.BillNumber;
                    bill.Operation = (int)OperationEnum.APP;
                    bill.UserId = userId ?? 0;
                    bill.BillId = data.BillId;

                    //RedLock
                    var results = await _locker.PerformActionWithLockAsync(this.LockKey(data, store ?? 0, userId ?? 0),
                        TimeSpan.FromSeconds(30),
                        TimeSpan.FromSeconds(10),
                        TimeSpan.FromSeconds(1),
                        () => _financeReceiveAccountBillService.SubmitAccountStatement(store ?? 0, userId ?? 0, bill));


                    //更新单据交账状态   
                    if (results.Success)
                    {
                        _billConvertService.UpdataBillAccountStatement(store ?? 0, data.BillId, (BillTypeEnum)data.BillType);
                        try
                        {
                            decimal AmountTotal = 0;
                            switch ((BillTypeEnum)data.BillType)
                            {
                                //销售单
                                case BillTypeEnum.SaleBill:
                                    AmountTotal = data.SaleAmount - data.SaleAdvanceReceiptAmount - data.SaleOweCashAmount;
                                    break;
                                //退货单
                                case BillTypeEnum.ReturnBill:
                                    AmountTotal = data.ReturnAmount - data.ReturnAdvanceReceiptAmount - data.ReturnOweCashAmount;
                                    break;
                                //收款单
                                case BillTypeEnum.CashReceiptBill:
                                    AmountTotal = data.ReceiptCashReceivableAmount - data.ReceiptCashAdvanceReceiptAmount;
                                    break;
                                //预收款单
                                case BillTypeEnum.AdvanceReceiptBill:
                                    AmountTotal = data.AdvanceReceiptAmount - data.AdvanceReceiptOweCashAmount;
                                    break;
                                //费用支出
                                case BillTypeEnum.CostExpenditureBill:
                                    AmountTotal = data.CostExpenditureAmount - data.CostExpenditureOweCashAmount;
                                    break;
                            }
                            #region 发送通知 管理员 
                            //制单人、管理员
                            var userNumbers = _userService.GetAllUserMobileNumbersByUserIds(new List<int> { data.UserId }).ToList();
                            QueuedMessage queuedMessage = new QueuedMessage()
                            {
                                StoreId = data.StoreId,
                                MType = MTypeEnum.TransferCompleted,
                                Title = CommonHelper.GetEnumDescription(MTypeEnum.TransferCompleted),
                                Date = bill.CreatedOnUtc,
                                BillType = BillTypeEnum.FinanceReceiveAccount,
                                BillNumbers = bill.BillNumber,
                                BillNumber = bill.BillNumber,
                                BillId = bill.Id,
                                CreatedOnUtc = DateTime.Now,
                                Amount= AmountTotal
                            };
                            _queuedMessageService.InsertQueuedMessage(userNumbers,queuedMessage);
                        }
                        catch (Exception ex)
                        {
                            _queuedMessageService.WriteLogs(ex.Message);
                        }

                        #endregion

                    }

                    return this.Successful(Resources.Successful, results);
                }
                catch (Exception ex)
                {
                    return this.Error(ex.Message);
                }

            });
        }


        /// <summary>
        /// 批量上交对账单
        /// </summary>
        /// <param name="data"></param>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost("financereceiveaccount/batchsubmitaccountstatements/{store}/{userId}")]
        [SwaggerOperation("batchsubmitaccountstatements")]
        //[AuthBaseFilter]
        public async Task<APIResult<dynamic>> BatchSubmitAccountStatements(FinanceReceiveAccountBillSubmitModel data, int? store, int? userId)
        {
            if (!store.HasValue || store.Value == 0)
                return this.Error(Resources.ParameterError);

            return await Task.Run(async () =>
            {
                try
                {

                    var datas = data?.Items;

                    if (!userId.HasValue || userId.Value == 0)
                        return this.Warning("开单人为指定.");

                    if (datas == null || !datas.Any())
                        return this.Warning("请录入数据.");

                    if (PeriodLocked(DateTime.Now, store ?? 0))
                        return this.Warning("锁账期间,禁止业务操作.");

                    if (PeriodClosed(DateTime.Now, store ?? 0))
                        return this.Warning("会计期间已结账,禁止业务操作.");

                    //Map
                    var bills = datas.Select(data =>
                    {
                        var bill = data.ToEntity<FinanceReceiveAccountBill>();
                        bill.StoreId = store ?? 0;
                        bill.BillNumber = data.BillNumber;
                        bill.Remark = data.BillNumber;
                        bill.Operation = (int)OperationEnum.APP;
                        bill.UserId = userId ?? 0;
                        bill.BillId = data.BillId;
                        return bill;

                    }).ToList();

                    if (bills == null || !bills.Any())
                        return this.Warning("数据转换失败.");

                    //RedLock
                    var results = await _locker.PerformActionWithLockAsync(this.LockKey(bills, store ?? 0, userId ?? 0),
                        TimeSpan.FromSeconds(30),
                        TimeSpan.FromSeconds(10),
                        TimeSpan.FromSeconds(1),
                        () => _financeReceiveAccountBillService.BatchSubmitAccountStatements(store ?? 0, userId ?? 0, bills));


                    //更新单据交账状态   
                    if (results.Success)
                    {
                        bills.ForEach(data =>
                        {
                            _billConvertService.UpdataBillAccountStatement(store ?? 0, data.BillId, (BillTypeEnum)data.BillType);
                        });
                    }

                    return this.Successful(Resources.Successful, results);
                }
                catch (Exception ex)
                {
                    return this.Error(ex.Message);
                }

            });
        }

    }
}