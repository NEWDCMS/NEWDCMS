using DCMS.Core;
using DCMS.Core.Domain.Finances;
using DCMS.Core.Domain.WareHouses;
using DCMS.Services.Common;
using DCMS.Services.Finances;
using DCMS.Services.Logging;
using DCMS.Services.Messages;
using DCMS.Services.Settings;
using DCMS.Services.WareHouses;
using DCMS.ViewModel.Models.Finances;
using DCMS.Web.Framework.Mvc.Filters;
using DCMS.Web.Infrastructure.Mapper.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DCMS.Web.Controllers
{
    /// <summary>
    /// 用于期末结账
    /// </summary>
    [RequestFormLimits(ValueCountLimit = 5000)]
    public class ClosingAccountsController : BasePublicController
    {
        private readonly IClosingAccountsService _closingAccountsService;
        private readonly IBillCheckService _billCheckService;
        private readonly IStockService _stockService;
        private readonly IRecordingVoucherService _recordingVoucherService;
        private readonly IAccountingService _accountingService;
        private readonly ITrialBalanceService _trialBalanceService;
        private readonly IBillConvertService _billConvertService;
        private readonly ILedgerDetailsService _ledgerDetailsService;

        public ClosingAccountsController(ILogger loggerService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IClosingAccountsService closingAccountsService,
            IBillCheckService billCheckService,
            IStockService stockService,
            IRecordingVoucherService recordingVoucherService,
            IAccountingService accountingService,
            ITrialBalanceService trialBalanceService,
            IBillConvertService billConvertService,
            ILedgerDetailsService ledgerDetailsService,
            INotificationService notificationService) : base(workContext, loggerService, storeContext, notificationService)
        {
            _closingAccountsService = closingAccountsService;
            _billCheckService = billCheckService;
            _stockService = stockService;
            _recordingVoucherService = recordingVoucherService;
            _accountingService = accountingService;
            _trialBalanceService = trialBalanceService;
            _billConvertService = billConvertService;
            _ledgerDetailsService = ledgerDetailsService;

        }

        public IActionResult Index()
        {
            return RedirectToAction("Settle");
        }


        /// <summary>
        /// 期末结账
        /// </summary>
        /// <returns></returns>
        [AuthCode((int)AccessGranularityEnum.FinancialCheck)]
        public IActionResult Settle()
        {
            var model = new CheckAccountModel();

            var checks = _closingAccountsService.GetAll(curStore?.Id);
            if (checks != null && checks.Count > 0)
            {
                var lists = checks.OrderByDescending(s => s.ClosingAccountDate).Take(12).ToList();
                var first = lists.First();
                if (first.CheckStatus)
                {
                    lists.Add(new ClosingAccounts { ClosingAccountDate = first.ClosingAccountDate.AddMonths(1), LockStatus = false, CheckStatus = false });
                }

                //HasPrecursor
                foreach (var month in lists)
                {
                    var last = month.ClosingAccountDate.AddDays(1 - month.ClosingAccountDate.Day).AddMonths(1).AddDays(-1);
                    month.ClosingAccountDate = last;
                    model.ClosingAccounts.Add(month.ToModel<ClosingAccountsModel>());
                }

                if (!model.ClosingAccounts.Select(s => s.ClosingAccountDate).Contains(DateTime.Parse("2020-02-29")))
                {
                    model.ClosingAccounts.Add(new ClosingAccountsModel { ClosingAccountDate = DateTime.Parse("2020-02-29") });
                }
            }
            else
            {
                //初始时间
                model.ClosingAccounts.Add(new ClosingAccountsModel { ClosingAccountDate = DateTime.Parse("2020-02-29") });
            }

            return View(model);
        }


        /// <summary>
        /// 期末检查(Step.1)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Checkout(ClosingAccountsModel model)
        {
            try
            {
                if (model.ClosingAccountDate == null)
                {
                    return Warning("参数错误");
                }

                var first = model.ClosingAccountDate.AddDays(1 - model.ClosingAccountDate.Day);
                var last = model.ClosingAccountDate.AddDays(1 - model.ClosingAccountDate.Day).AddMonths(1).AddDays(-1);

                //检查当月是否有未审核单据，有则拒绝流程，提示未审核单据信息
                var noCompletes = _billCheckService.CheckAllBills(curStore.Id, new Tuple<DateTime, DateTime>(first, last));
                if (noCompletes.Any())
                {
                    return Warning($"当月有{noCompletes.Count()}项未审核单据,请检查发生业务。");
                }

                //检查当期是否已经结转，如果已经结账则拒绝流程
                var closed = _closingAccountsService.CheckClosingAccounts(curStore?.Id, model.ClosingAccountDate);
                if (closed)
                {
                    return Warning($"本会计期间({model.ClosingAccountDate.ToString("yyyyMM")})已结账，禁止操作。");
                }

                //检查上一个会计期间是否存在，如果存在则检查结账是否完成，如果存在锁帐和已经结账，则拒绝流程
                var lastMonth = model.ClosingAccountDate.AddMonths(-1);
                var last_period = _closingAccountsService.GetClosingAccountsByPeriod(curStore.Id, lastMonth);
                if (last_period != null && model.ClosingAccountDate.ToString("yyyy-MM") != "2020-02")
                {
                    if (!last_period.CheckStatus || !last_period.LockStatus)
                    {
                        return Warning($"上一个会计期间{lastMonth.ToString("MM")}未结账");
                    }
                }

                //检查当期凭证是否审核完，如果有未审核完成的凭证则拒绝流程
                var cur_vouchers = _recordingVoucherService.GetRecordingVoucherFromPeriod(curStore.Id, model.ClosingAccountDate);
                if (cur_vouchers != null)
                {
                    var draft_voucher_count = cur_vouchers.Where(s => s.AuditedStatus == false).Count();
                    if (draft_voucher_count > 0)
                    {
                        return Warning($"该期间有{draft_voucher_count}张凭证未确认,请先审核凭证。");
                    }
                }

                var period = _closingAccountsService.GetClosingAccounts(curStore?.Id, model.ClosingAccountDate).FirstOrDefault();
                if (period == null)
                {
                    period = new ClosingAccounts()
                    {

                        ClosingAccountDate = model.ClosingAccountDate.AddDays(1 - model.ClosingAccountDate.Day).AddMonths(1).AddDays(-1),
                        StoreId = curStore.Id,
                        CheckUserId = curUser?.Id,
                        CheckDate = DateTime.Now,
                        CheckStatus = false,
                        LockStatus = true,
                    };
                    _closingAccountsService.InsertClosingAccounts(period);
                }
                else
                {
                    period.LockStatus = true;
                    period.ClosingAccountDate = model.ClosingAccountDate.AddDays(1 - model.ClosingAccountDate.Day).AddMonths(1).AddDays(-1);
                    _closingAccountsService.UpdateClosingAccounts(period);
                }

                return Successful("允许结转");
            }
            catch (Exception exc)
            {
                return Error(exc.Message);
            }
        }


        #region 成本结转(Step.2)

        /// <summary>
        /// 初始并校验
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual IActionResult CostCarryOver_Task1([FromBody]CheckOutModel model)
        {
            try
            {
                model.Period.StoreId = curStore.Id;
                model.Period.CheckUserId = curUser.Id;

                var billTypes = new[] { 12, 14, 22, 24, 32, 33, 34, 37, 38 };
                //获取当期发生成本汇总
                var monthProductCosts = GetMonthProductCostSummery(model.Period, billTypes);
                //获取商品成本汇总
                var monthProductCostDicts = GetMonthProductCostDicts(model.Period, monthProductCosts);

                #region //重新计算成本价格,并替换商品成本价

                decimal allBalancePrice = 0;

                var billDicts = new Dictionary<string, Tuple<int, int, int, decimal>>();

                //计算当期库存商品科目（所有商品类别涉及的科目）贷方金额合计，发出时已结转的实际成本
                var vouchers_InventoryGoods = _recordingVoucherService.GetVoucherItemsByAccountCodeTypeIdFromPeriod(curStore.Id, (int)AccountingCodeEnum.InventoryGoods, model.Period.ClosingAccountDate);
                //贷方金额合计
                var realOutCost = vouchers_InventoryGoods?.Sum(s => s.DebitAmount ?? 0);

                foreach (var costProduct in monthProductCostDicts)
                {
                    //当前商品销售成本金额（加权平均计算而来）
                    var currentPeriodOutCost = ComputeBalancePrice(costProduct, CostMethodEnum.AVERAGE);

                    //结存单价价 = 加权平均价
                    costProduct.EndAmount = currentPeriodOutCost.Item2;

                    //两者之差 = 当前商品发出成本(当期加权平均成本) - 发出时已结转的实际成本
                    var diffCost = currentPeriodOutCost.Item1 - realOutCost;

                    //获取当前商品对应的单据
                    foreach (var p in costProduct.Records)
                    {
                        string key = $"{p.ProductId}_{p.BillId}_{p.BillTypeId}";
                        if (!billDicts.ContainsKey(key))
                        {
                            billDicts.Add(key, new Tuple<int, int, int, decimal>(p.ProductId, p.BillId, p.BillTypeId, currentPeriodOutCost.Item2));
                        }
                    }

                    allBalancePrice += diffCost ?? 0;
                }

                //替换成结转后的全月平均价
                //结转成本后，已审核业务单据中的成本价，将会被替换成结转后的全月平均价!
                foreach (var bill in billDicts)
                {
                    var item = bill.Value;
                    _billConvertService.AdjustBillCost(curStore.Id, item.Item3, item.Item2, item.Item1, item.Item4);
                }

                #endregion

                model.CostPriceSummeries = monthProductCostDicts;

            }
            catch (Exception)
            {
                return Error($"{model.Task.Name}失败");
            }

            return Successful($"{model.Task.Name}完成", model);
        }

        /// <summary>
        /// 结转销售类
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual IActionResult CostCarryOver_Task2([FromBody]CheckOutModel model)
        {
            try
            {
                //单据： 销售单  12 退货单 14 
                var billTypes = new[] { 12, 14 };
                //获取当期发生成本汇总
                var monthProductCosts = GetMonthProductCostSummery(model.Period, billTypes);
                //获取商品成本汇总
                var monthProductCostDicts = GetMonthProductCostDicts(model.Period, monthProductCosts);
                //计算成本
                var costAmount = RecalculateCostPrice(model.Period, monthProductCostDicts);

                model.CostOfSales = costAmount;
            }
            catch (Exception)
            {
                return Error($"{model.Task.Name}失败");
            }
            return Successful($"{model.Task.Name}完成", model);
        }

        /// <summary>
        /// 结转库存损失类
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual IActionResult CostCarryOver_Task3([FromBody]CheckOutModel model)
        {
            try
            {
                //报损单 34
                var billTypes = new[] { 34 };
                //获取当期发生成本汇总
                var monthProductCosts = GetMonthProductCostSummery(model.Period, billTypes);
                //获取商品成本汇总
                var monthProductCostDicts = GetMonthProductCostDicts(model.Period, monthProductCosts);
                //计算成本
                var costAmount = RecalculateCostPrice(model.Period, monthProductCostDicts);

                model.CostOfStockLoss = costAmount;
            }
            catch (Exception)
            {
                return Error($"{model.Task.Name}失败");
            }
            return Successful($"{model.Task.Name}完成", model);
        }

        /// <summary>
        /// 结转库存调整类
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual IActionResult CostCarryOver_Task4([FromBody]CheckOutModel model)
        {
            try
            {
                //单据： 盘点盈亏单 32  
                var billTypes = new[] { 32 };
                //获取当期发生成本汇总
                var monthProductCosts = GetMonthProductCostSummery(model.Period, billTypes);
                //获取商品成本汇总
                var monthProductCostDicts = GetMonthProductCostDicts(model.Period, monthProductCosts);
                //计算成本
                var costAmount = RecalculateCostPrice(model.Period, monthProductCostDicts);

                model.CostOfStockAdjust = costAmount;
            }
            catch (Exception)
            {
                return Error($"{model.Task.Name}失败");
            }
            return Successful($"{model.Task.Name}完成", model);
        }

        /// <summary>
        /// 结转拆装组合类
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual IActionResult CostCarryOver_Task5([FromBody]CheckOutModel model)
        {
            try
            {
                //单据： 组合单 37  拆分单 38
                var billTypes = new[] { 37, 38 };
                //获取当期发生成本汇总
                var monthProductCosts = GetMonthProductCostSummery(model.Period, billTypes);
                //获取商品成本汇总
                var monthProductCostDicts = GetMonthProductCostDicts(model.Period, monthProductCosts);
                //计算成本
                var costAmount = RecalculateCostPrice(model.Period, monthProductCostDicts);

                model.CostOfJointGoods = costAmount;
            }
            catch (Exception)
            {
                return Error($"{model.Task.Name}失败");
            }
            return Successful($"{model.Task.Name}完成", model);
        }

        /// <summary>
        /// 结转采购退货类
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual IActionResult CostCarryOver_Task6([FromBody]CheckOutModel model)
        {
            try
            {
                //单据： 采购单  22 采购退货单  24  
                var billTypes = new[] { 22, 24 };
                //获取当期发生成本汇总
                var monthProductCosts = GetMonthProductCostSummery(model.Period, billTypes);
                //获取商品成本汇总
                var monthProductCostDicts = GetMonthProductCostDicts(model.Period, monthProductCosts);
                //计算成本
                var costAmount = RecalculateCostPrice(model.Period, monthProductCostDicts);

                model.CostOfPurchaseReject = costAmount;
            }
            catch (Exception)
            {
                return Error($"{model.Task.Name}失败");
            }
            return Successful($"{model.Task.Name}完成", model);
        }

        /// <summary>
        /// 结转价格调整类
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual IActionResult CostCarryOver_Task7([FromBody]CheckOutModel model)
        {
            try
            {
                //单据： 成本调价单 33
                var billTypes = new[] { 33 };
                //获取当期发生成本汇总
                var monthProductCosts = GetMonthProductCostSummery(model.Period, billTypes);
                //获取商品成本汇总
                var monthProductCostDicts = GetMonthProductCostDicts(model.Period, monthProductCosts);
                //计算成本
                var costAmount = RecalculateCostPrice(model.Period, monthProductCostDicts);

                model.CostOfPriceAdjust = costAmount;
            }
            catch (Exception)
            {
                return Error($"{model.Task.Name}失败");
            }
            return Successful($"{model.Task.Name}完成", model);
        }

        /// <summary>
        /// 生成结账凭证行
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual IActionResult CostCarryOver_Task8([FromBody]CheckOutModel model)
        {
            try
            {
                #region 生成结账凭证行

                //decimal allBalancePrice = 0;
                var period = model.Period.ToEntity<ClosingAccounts>();

                //按照单据中商品的成本价生成相应的成本凭证
                //销售单  12 退货单 14  采购单  22 采购退货单  24  盘点盈亏单 32  成本调价单 33 报损单 34 组合单 37  拆分单 38

                //（销售类）CostOfSales:
                //（库存损失类）CostOfStockLoss
                //（库存调整类）CostOfStockAdjust:
                //（拆装组合类）CostOfJointGoods:
                //（采购退货类）CostOfPurchaseReject:
                //（价格调整类）CostOfPriceAdjust:

                #region //（销售类）CostOfSales:
                /*
                 主营业务成本: 借方
                 库存商品: 贷方
                 */
                //var vouchers_MainCost = _recordingVoucherService.GetVoucherItemsByAccountCodeTypeIdFromPeriod(curStore.Id, (int)AccountingCodeEnum.MainCost, period.ClosingAccountDate);
                _recordingVoucherService.CreateCostVoucher(CostOfSettleEnum.CostOfSales,
                    curStore.Id,
                    curUser.Id,
                    AccountingCodeEnum.MainCost,
                    AccountingCodeEnum.InventoryGoods,
                    model.CostOfSales,
                    model.CostOfSales,
                    period);
                #endregion

                #region//（库存损失类）CostOfStockLoss:  报损单
                /*
                 盘点亏损: 借方
                 库存商品: 贷方
                 */
                //var vouchers_InventoryLoss = _recordingVoucherService.GetVoucherItemsByAccountCodeTypeIdFromPeriod(curStore.Id, (int)AccountingCodeEnum.InventoryLoss, period.ClosingAccountDate);
                //var debitAmount_InventoryLoss = vouchers_InventoryLoss?.Sum(s => s.DebitAmount ?? 0);
                _recordingVoucherService.CreateCostVoucher(CostOfSettleEnum.CostOfStockLoss,
                    curStore.Id,
                    curUser.Id,
                    AccountingCodeEnum.InventoryLoss,
                    AccountingCodeEnum.InventoryGoods,
                    model.CostOfStockLoss,
                    model.CostOfStockLoss,
                    period);
                #endregion

                #region//（库存调整类）CostOfStockAdjust:
                /*


                 盘点亏损: 借方
                 库存商品: 贷方

                 库存商品: 借方
                 盘点报溢收入 : 贷方
                 */
                //var vouchers_StockAdjust = _recordingVoucherService.GetVoucherItemsByAccountCodeTypeIdFromPeriod(curStore.Id, (int)AccountingCodeEnum.InventoryLoss, period.ClosingAccountDate);
                //var debitAmount_StockAdjust = vouchers_StockAdjust?.Sum(s => s.DebitAmount ?? 0);
                //var vouchers_InventoryGoods = _recordingVoucherService.GetVoucherItemsByAccountCodeTypeIdFromPeriod(curStore.Id, (int)AccountingCodeEnum.TakeStockIncome, period.ClosingAccountDate);
                //var creditAmount_InventoryGoods = vouchers_InventoryGoods?.Sum(s => s.CreditAmount ?? 0);
                if (model.CostOfPriceAdjust > 0)
                {
                    _recordingVoucherService.CreateCostVoucher(CostOfSettleEnum.CostOfStockAdjust,
                          curStore.Id,
                          curUser.Id,
                          AccountingCodeEnum.InventoryLoss,
                          AccountingCodeEnum.InventoryGoods,
                          model.CostOfPriceAdjust,
                          model.CostOfPriceAdjust,
                          period);
                }
                else
                {
                    _recordingVoucherService.CreateCostVoucher(CostOfSettleEnum.CostOfStockAdjust,
                      curStore.Id,
                      curUser.Id,
                      AccountingCodeEnum.InventoryGoods,
                      AccountingCodeEnum.TakeStockIncome,
                      model.CostOfPriceAdjust,
                      model.CostOfPriceAdjust,
                      period);
                }

                #endregion

                #region//（拆装组合类）CostOfJointGoods:
                /*
                 商品拆装收入:借方
                 库存商品: 贷方
                 */
                //var vouchers_GoodsIncome = _recordingVoucherService.GetVoucherItemsByAccountCodeTypeIdFromPeriod(curStore.Id, (int)AccountingCodeEnum.GoodsIncome, period.ClosingAccountDate);
                //var debitAmount_GoodsIncome = vouchers_GoodsIncome?.Sum(s => s.DebitAmount ?? 0);
                _recordingVoucherService.CreateCostVoucher(CostOfSettleEnum.CostOfJointGoods,
                    curStore.Id,
                    curUser.Id,
                    AccountingCodeEnum.GoodsIncome,
                    AccountingCodeEnum.InventoryGoods,
                    model.CostOfJointGoods,
                    model.CostOfJointGoods,
                    period);
                #endregion

                #region // (采购退货类)CostOfPurchaseReject:
                /*
                 采购退货损失:借方
                 库存商品: 贷方
                 */
                //var vouchersPurchaseLoss = _recordingVoucherService.GetVoucherItemsByAccountCodeTypeIdFromPeriod(curStore.Id, (int)AccountingCodeEnum.PurchaseLoss, period.ClosingAccountDate);
                //var debitAmount_PurchaseLoss = vouchersPurchaseLoss?.Sum(s => s.DebitAmount ?? 0);
                _recordingVoucherService.CreateCostVoucher(CostOfSettleEnum.CostOfPurchaseReject,
                    curStore.Id,
                    curUser.Id,
                    AccountingCodeEnum.PurchaseLoss,
                    AccountingCodeEnum.InventoryGoods,
                    model.CostOfPurchaseReject,
                    model.CostOfPurchaseReject,
                    period);
                #endregion

                #region// (价格调整类)CostOfPriceAdjust:
                /*
                 成本调价损失:借方
                 库存商品: 贷方

                 库存商品: 借方
                 成本调价收入:贷方
                 */
                //var vouchers_CostLoss = _recordingVoucherService.GetVoucherItemsByAccountCodeTypeIdFromPeriod(curStore.Id, (int)AccountingCodeEnum.CostLoss, period.ClosingAccountDate);
                //var debitAmount_CostLoss = vouchers_CostLoss?.Sum(s => s.DebitAmount ?? 0);
                //var vouchers_CostIncome = _recordingVoucherService.GetVoucherItemsByAccountCodeTypeIdFromPeriod(curStore.Id, (int)AccountingCodeEnum.CostIncome, period.ClosingAccountDate);
                //var creditAmount_CostIncome = vouchers_CostIncome?.Sum(s => s.CreditAmount ?? 0);

                if (model.CostOfPriceAdjust > 0)
                {
                    _recordingVoucherService.CreateCostVoucher(CostOfSettleEnum.CostOfPriceAdjust,
                        curStore.Id,
                        curUser.Id,
                        AccountingCodeEnum.InventoryGoods,
                        AccountingCodeEnum.CostIncome,
                        model.CostOfPriceAdjust,
                        model.CostOfPriceAdjust,
                        period);
                }
                else
                {
                    _recordingVoucherService.CreateCostVoucher(CostOfSettleEnum.CostOfPriceAdjust,
                            curStore.Id,
                            curUser.Id,
                            AccountingCodeEnum.CostLoss,
                            AccountingCodeEnum.InventoryGoods,
                            model.CostOfPriceAdjust,
                            model.CostOfPriceAdjust,
                            period);
                }

                #endregion

                #endregion

                #region 生成成本汇总表

                var itemRecords = new List<CostPriceChangeRecords>();
                foreach (var cps in model.CostPriceSummeries)
                {
                    cps.Date = model.Period.ClosingAccountDate.AddDays(1 - model.Period.ClosingAccountDate.Day).AddMonths(1).AddDays(-1);
                    var curSummery = _closingAccountsService.GetCostPriceSummeriesByProductId(cps.StoreId, cps.Date, cps.ProductId, cps.UnitId);
                    if (curSummery == null)
                    {
                        //写入当期成本汇总
                        _closingAccountsService.InsertCostPriceSummery(cps);

                        if (cps != null && cps.Id > 0)
                        {
                            foreach (var item in cps.Records)
                            {
                                item.CostPriceSummeryId = cps.Id;
                                itemRecords.Add(item);
                            }
                        }
                    }
                    else
                    {
                        cps.Id = curSummery.Id;
                        curSummery = cps;
                        _closingAccountsService.UpdateCostPriceSummery(curSummery);

                        foreach (var item in curSummery.Records)
                        {
                            item.CostPriceSummeryId = cps.Id;
                            itemRecords.Add(item);
                        }
                    }
                }

                //批量写入当期成本变化明细
                if (itemRecords.Any())
                {
                    foreach (var item in itemRecords)
                    {
                        var record = _closingAccountsService.CostPriceChangeRecords(item.StoreId, item.CreatedOnUtc, item.BillTypeId, item.BillId, item.ProductId, item.UnitId);
                        if (record != null)
                        {
                            _closingAccountsService.UpdateCostPriceChangeRecords(item);
                        }
                        else
                        {
                            _closingAccountsService.InsertCostPriceChangeRecords(item);
                        }
                    }
                }

                #endregion
            }
            catch (Exception)
            {
                return Error($"任务{model.Task.Name}失败");
            }

            return Successful($"{model.Task.Name}完成", model);
        }

        #endregion


        #region 损益结转(Step.3)


        /// <summary> 
        /// 初始并校验
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual IActionResult ProfitAndLossCarryOver_Task1([FromBody]CheckOutModel model)
        {
            try
            {
                #region
                /*
                说明：

                确认凭证个数
                该期间有%s张凭证未确认

                //revenue_account_ids 收入类科目
                //expense_account_ids 费用类科目
                //revenue_total = 0  # 收入类科目合计
                //expense_total = 0  # 费用（支出）类科目合计

                # 贷方冲借方

                # 借方冲贷方


                # 本年利润  利润结余

                if (收入类科目合计 - 费用（支出）类科目合计) > 0
                {
                credit(借方) = 收入类科目合计 - 费用（支出）类科目合计
                debit (贷方) =0
                }

                if (收入类科目合计 - 费用（支出）类科目合计) < 0
                {
                debit (贷方) =费用（支出）类科目合计 - 收入类科目合计
                credit(借方) = 0
                }

                # 生成凭证


                # 年度结余
                if( month(结转当期) = 12 )
                {
                记账 年度结余 科目

                'Name': u'年度结余',
                'AccountingOptionId': （未分配利润科目）remain_account.id,
                'DebitAmount (贷方)': 0,
                'CreditAmount(借方)': year_total,


                'Name': u'年度结余',
                'AccountingOptionId': （本年利润科目）year_profit_account.id,
                'DebitAmount (贷方)': year_total,
                'CreditAmount(借方)': 0,


                # 创建结转凭证
                # 凭证确认

                }
                */
                #endregion

                var period = model.Period.ClosingAccountDate;

                model.Period.StoreId = curStore.Id;

                // 收入类科目合计
                model.RevenueTotal = 0;

                // 费用类科目合计
                model.ExpenseTotal = 0;

                #region 科目余额处理

                /*
                1、资产类科目：期末借方余额=期初借方余额+本期借方发生额-本期贷方发生额；
                2、负债及所有者权益类科目：期末贷方余额=期初贷方余额+本期贷方发生额-本期借方发生额。
                3、月期末余额=下月期初余额
                4、年初余额=本年1月的数据
                5、资产=负债+所有者权益
                6、本年累计借方余额=年初借方余额+本年借方累计发生额-本年贷方累计发生额
                7、本年累计贷方余额=年初贷方余额-本年借方累计发生额+本年贷方累计发生额
                8、期末余额=年初金额+本年累计额借方-本年累计贷方
                9、期末余额=年初金额+本年累计借方-本年累计贷方
                10、货币资金=现金+银行存款+其他货币资金。
                */


                /*
              //用于插入
              var inserts = new List<TrialBalance>();

              //用于更新
              var updates = new List<TrialBalance>();


              //上一期间
              var lastMonth = period.AddMonths(-1);

              //获取上期余额,不存在则构建初始期间科目余额（包含新进科目）
              var lastTrialbalances = _trialBalanceService.TryGetTrialBalances(curStore.Id, lastMonth);

              if (lastTrialbalances == null || !lastTrialbalances.Any())
              {
                  return this.Error($"任务{model.Task.Name}失败,会计科目未指定.");
              }

              //通过上期余额来构建本期发生额
              var currentOccurrences = new List<TrialBalance>();
              foreach (var last in lastTrialbalances)
              {
                  var curr = new TrialBalance() { StoreId = curStore.Id };
                  //获取本期科目凭证明细项目
                  var currVoucherItems = _recordingVoucherService.GetVoucherItemsByAccountingOptionIdFromPeriod(curStore.Id, last.AccountingOptionId, period);

                  //本期期初为上一期期末
                  curr.InitialBalanceDebit = last.EndBalanceDebit;
                  curr.InitialBalanceCredit = last.EndBalanceCredit;

                  //本期借 =  本期凭证明细借项目 合计
                  curr.PeriodBalanceDebit = currVoucherItems?.Sum(s => s.DebitAmount ?? 0);
                  //本期贷 =  本期凭证明细贷项目 合计
                  curr.PeriodBalanceCredit = currVoucherItems?.Sum(s => s.CreditAmount ?? 0);

                  //本期末
                  //判断方向（根据科目的类型，判断余额方向是借方或者贷方）
                  //资产类科目：期末借方余额=期初借方余额+本期借方发生额-本期贷方发生额；
                  //负债及所有者权益类科目：期末贷方余额 = 期初贷方余额 + 本期贷方发生额 - 本期借方发生额。
                  if (last.DirectionsType == DirectionsTypeEnum.IN)
                      curr.EndBalanceDebit = curr.InitialBalanceDebit + curr.PeriodBalanceDebit - curr.PeriodBalanceCredit;
                  else
                      curr.EndBalanceDebit = curr.InitialBalanceCredit + curr.PeriodBalanceCredit - curr.PeriodBalanceDebit;

                  //指定为本期(从上期取的数据在这里重写指定期间)
                  curr.PeriodDate = period;

                  curr.AccountingOptionId = last.AccountingOptionId;
                  curr.AccountingOptionName = last.AccountingOptionName;
                  curr.AccountingTypeId = last.AccountingTypeId;
                  curr.AccountingOption = last.AccountingOption;

                  curr.CreatedOnUtc = DateTime.Now;
                  currentOccurrences.Add(curr);
              }

              //进行本期计算
              if (currentOccurrences.Any())
              {
                  //这里 trialBalances 已经是本期发生后的额度记录
                  foreach (var current_occurrence in currentOccurrences)
                  {
                      //存在则更新(反结转后，不会删除余额表，如果再次结转时会更新余额)
                      if (current_occurrence.Id > 0)
                      {
                          //更新 科目余额表， 将新出现的 科目加入到 科目余额表
                          updates.Add(current_occurrence);
                      }
                      //(包含新进科目)
                      else
                      {
                          inserts.Add(current_occurrence);
                      }
                  }
              }



              //对 科目余额表 上下级 数据重新计算
              updates.ForEach(b =>
              {
                  //判断当前科目下是否有子集科目，如果包含自己科目，则对自己进行数据合计
                  if (_trialBalanceService.HasChilds(b.AccountingOptionId))
                  {
                      //期初余额
                      b.InitialBalanceDebit += updates.Where(s => s.AccountingOption.ParentId == b.AccountingOptionId).Sum(s => s.InitialBalanceDebit ?? 0);
                      b.InitialBalanceCredit += updates.Where(s => s.AccountingOption.ParentId == b.AccountingOptionId).Sum(s => s.InitialBalanceCredit ?? 0);
                      //本期发生额
                      b.PeriodBalanceDebit += updates.Where(s => s.AccountingOption.ParentId == b.AccountingOptionId).Sum(s => s.PeriodBalanceDebit ?? 0);
                      b.PeriodBalanceCredit += updates.Where(s => s.AccountingOption.ParentId == b.AccountingOptionId).Sum(s => s.PeriodBalanceCredit ?? 0);
                      //期末余额
                      b.EndBalanceDebit += updates.Where(s => s.AccountingOption.ParentId == b.AccountingOptionId).Sum(s => s.EndBalanceDebit ?? 0);
                      b.EndBalanceCredit += updates.Where(s => s.AccountingOption.ParentId == b.AccountingOptionId).Sum(s => s.EndBalanceCredit ?? 0);
                  }
              });

              //对新进科目 数据重新计算
              inserts.ForEach(b =>
              {
                  if (_trialBalanceService.HasChilds(b.AccountingOptionId))
                  {
                      //期初余额
                      b.InitialBalanceDebit += inserts.Where(s => s.AccountingOption.ParentId == b.AccountingOptionId).Sum(s => s.InitialBalanceDebit ?? 0);
                      b.InitialBalanceCredit += inserts.Where(s => s.AccountingOption.ParentId == b.AccountingOptionId).Sum(s => s.InitialBalanceCredit ?? 0);
                      //本期发生额
                      b.PeriodBalanceDebit += inserts.Where(s => s.AccountingOption.ParentId == b.AccountingOptionId).Sum(s => s.PeriodBalanceDebit ?? 0);
                      b.PeriodBalanceCredit += inserts.Where(s => s.AccountingOption.ParentId == b.AccountingOptionId).Sum(s => s.PeriodBalanceCredit ?? 0);
                      //期末余额
                      b.EndBalanceDebit += inserts.Where(s => s.AccountingOption.ParentId == b.AccountingOptionId).Sum(s => s.EndBalanceDebit ?? 0);
                      b.EndBalanceCredit += inserts.Where(s => s.AccountingOption.ParentId == b.AccountingOptionId).Sum(s => s.EndBalanceCredit ?? 0);
                  }
              });

              //追加新进
              updates.AddRange(inserts);
              if (updates.Any())
              {
                  updates.ForEach(s =>
                  {
                      s.StoreId = s.StoreId == 0 ? curStore.Id : s.StoreId;
                      s.PeriodDate = model.Period.ClosingAccountDate.AddDays(1 - model.Period.ClosingAccountDate.Day).AddMonths(1).AddDays(-1);
                  });

                  model.TrialBalances = updates;
              }
              */

                //本期发生额(会计科目树最大允许3级，这里不使用递归)
                var currentOccurrences = new List<TrialBalance>();
                var trees = GetTrialBalanceTree(curStore.Id, period, 0, null);
                foreach (var node in trees)
                {
                    if (node.Children.Any())
                    {
                        foreach (var node1 in node.Children)
                        {
                            if (node1.Children.Any())
                            {
                                foreach (var node2 in node1.Children)
                                {
                                    currentOccurrences.Add(node2.TrialBalance);
                                }
                            }
                            currentOccurrences.Add(node1.TrialBalance);
                        }
                    }
                    currentOccurrences.Add(node.TrialBalance);
                }
                model.TrialBalances = currentOccurrences;

                #endregion
            }
            catch (Exception)
            {
                return Error($"任务{model.Task.Name}失败");
            }
            return Successful($"{model.Task.Name}完成", model);
        }

        /// <summary>
        /// 结转损益类（收入）
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual IActionResult ProfitAndLossCarryOver_Task2([FromBody]CheckOutModel model)
        {
            try
            {
                model.Period.StoreId = curStore.Id;

                //记账凭证(settleyyyyMM)
                var incomeVoucher = new RecordingVoucher()
                {
                    BillId = 0,
                    StoreId = curStore.Id,
                    //单据编号
                    BillNumber = $"settle{model.Period.ClosingAccountDate.ToString("yyyyMM")}",
                    //单据类型
                    BillTypeId = 0,
                    //系统生成
                    GenerateMode = (int)GenerateMode.Auto,
                    RecordTime = model.Period.ClosingAccountDate,
                    //审核时间
                    AuditedDate = DateTime.Now,
                    //自动审核
                    AuditedStatus = true
                };

                //收入科目
                var incomeAccounts = _accountingService.GetAccountingTree(curStore.Id, 0, (int)AccountingEnum.Income);
                foreach (var acc in incomeAccounts)
                {
                    //贷方总计
                    //decimal credit_total = 0;

                    //如果含有子集科目时（父级不记账）
                    if (acc.Children.Any())
                    {
                        foreach (var chid in acc.Children)
                        {
                            //获取当期科目凭证汇总
                            var vouchers = _recordingVoucherService.GetVoucherItemsByAccountingOptionIdFromPeriod(curStore.Id, chid.Id, model.Period.ClosingAccountDate);
                            //借
                            var debitAmountTotal = vouchers.Sum(s => s.DebitAmount ?? 0);
                            //贷
                            var creditAmountTotal = vouchers.Sum(s => s.CreditAmount ?? 0);

                            //credit_total += creditAmountTotal - debitAmountTotal;
                            model.RevenueTotal += creditAmountTotal + debitAmountTotal;

                            //贷方冲借方
                            //if (credit_total != 0)
                            //{
                            //当期科目
                            incomeVoucher.Items.Add(new VoucherItem()
                            {
                                StoreId = curStore.Id,
                                Direction = 0,
                                RecordTime = model.Period.ClosingAccountDate,
                                Summary = $"第{model.Period.ClosingAccountDate.ToString("MM")}期结转损益",
                                AccountingOptionName = $"{chid?.Name}:{chid?.Code}",
                                AccountingOptionId = chid?.Id ?? 0,
                                DebitAmount = debitAmountTotal,//credit_total,
                                CreditAmount = creditAmountTotal //0 //贷方清0
                            });
                            //}
                        }
                    }
                    else
                    {
                        //获取当期科目凭证汇总
                        var vouchers = _recordingVoucherService.GetVoucherItemsByAccountingOptionIdFromPeriod(curStore.Id, acc.Id, model.Period.ClosingAccountDate);
                        //借
                        var debitAmountTotal = vouchers.Sum(s => s.DebitAmount ?? 0);
                        //贷
                        var creditAmountTotal = vouchers.Sum(s => s.CreditAmount ?? 0);

                        //credit_total += creditAmountTotal - debitAmountTotal;
                        model.RevenueTotal += creditAmountTotal + debitAmountTotal;

                        //贷方冲借方
                        //if (credit_total != 0)
                        //{
                        //当期科目
                        incomeVoucher.Items.Add(new VoucherItem()
                        {
                            StoreId = curStore.Id,
                            Direction = 0,
                            RecordTime = model.Period.ClosingAccountDate,
                            Summary = $"第{model.Period.ClosingAccountDate.ToString("MM")}期结转损益",
                            AccountingOptionName = $"{acc?.Name}:{acc?.Code}",
                            AccountingOptionId = acc?.Id ?? 0,
                            DebitAmount = debitAmountTotal,//credit_total,
                            CreditAmount = creditAmountTotal //0 //贷方清0
                        });
                        //}
                    }
                }

                //收入凭证
                model.IncomeVoucher = incomeVoucher;

            }
            catch (Exception)
            {
                return Error($"任务{model.Task.Name}失败");
            }

            return Successful($"{model.Task.Name}完成", model);
        }

        /// <summary>
        /// 结转损益类（支出）
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual IActionResult ProfitAndLossCarryOver_Task3([FromBody]CheckOutModel model)
        {
            try
            {
                model.Period.StoreId = curStore.Id;
                //记账凭证
                var expenseVoucher = new RecordingVoucher()
                {
                    BillId = 0,
                    StoreId = curStore.Id,
                    //单据编号
                    BillNumber = $"settle{model.Period.ClosingAccountDate.ToString("yyyyMM")}",
                    //单据类型
                    BillTypeId = 0,
                    //系统生成
                    GenerateMode = (int)GenerateMode.Auto,
                    RecordTime = model.Period.ClosingAccountDate,
                    //审核时间
                    AuditedDate = DateTime.Now,
                    //自动审核
                    AuditedStatus = true
                };

                //支出科目
                var expenseAccounts = _accountingService.GetAccountingTree(curStore.Id, 0, (int)AccountingEnum.Expense);
                foreach (var acc in expenseAccounts)
                {
                    //借方总计
                    decimal debit_total = 0;

                    //如果含有子集科目时（父级不记账）
                    if (acc.Children.Any())
                    {
                        foreach (var chid in acc.Children)
                        {
                            //获取当期科目凭证汇总
                            var vouchers = _recordingVoucherService.GetVoucherItemsByAccountingOptionIdFromPeriod(curStore.Id, chid.Id, model.Period.ClosingAccountDate);
                            //借
                            var debitAmountTotal = vouchers.Sum(s => s.DebitAmount ?? 0);
                            //贷
                            var creditAmountTotal = vouchers.Sum(s => s.CreditAmount ?? 0);

                            //debit_total += debitAmountTotal - creditAmountTotal;
                            model.ExpenseTotal += debitAmountTotal + creditAmountTotal;

                            //当期科目
                            expenseVoucher.Items.Add(new VoucherItem()
                            {
                                StoreId = curStore.Id,
                                Direction = 1,
                                RecordTime = model.Period.ClosingAccountDate,
                                Summary = $"第{model.Period.ClosingAccountDate.ToString("MM")}期结转损益",
                                AccountingOptionName = $"{chid?.Name}:{chid?.Code}",
                                AccountingOptionId = chid?.Id ?? 0,
                                CreditAmount = creditAmountTotal,//debit_total,
                                DebitAmount = debitAmountTotal //0 //借方清0
                            });

                        }
                    }
                    else
                    {
                        //获取当期科目凭证汇总
                        var vouchers = _recordingVoucherService.GetVoucherItemsByAccountingOptionIdFromPeriod(curStore.Id, acc.Id, model.Period.ClosingAccountDate);
                        //借
                        var debitAmountTotal = vouchers.Sum(s => s.DebitAmount ?? 0);
                        //贷
                        var creditAmountTotal = vouchers.Sum(s => s.CreditAmount ?? 0);

                        debit_total += debitAmountTotal - creditAmountTotal;
                        model.ExpenseTotal += debit_total;

                        //当期科目
                        expenseVoucher.Items.Add(new VoucherItem()
                        {
                            StoreId = curStore.Id,
                            Direction = 1,
                            RecordTime = model.Period.ClosingAccountDate,
                            Summary = $"第{model.Period.ClosingAccountDate.ToString("MM")}期结转损益",
                            AccountingOptionName = $"{acc?.Name}:{acc?.Code}",
                            AccountingOptionId = acc?.Id ?? 0,
                            CreditAmount = creditAmountTotal,//debit_total,
                            DebitAmount = debitAmountTotal //0 //借方清0
                        });

                    }
                }

                //支出凭证
                model.ExpenseVoucher = expenseVoucher;
            }
            catch (Exception)
            {
                return Error($"任务{model.Task.Name}失败");
            }

            return Successful($"{model.Task.Name}完成", model);
        }

        /// <summary>
        /// 年末处理
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual IActionResult ProfitAndLossCarryOver_Task4([FromBody]CheckOutModel model)
        {

            #region 判断年末
            try
            {
                model.YearTotal = 0;
                if (model.Period.ClosingAccountDate.Month == 12)
                {
                    //记账凭证(unAllotProfiryyyyMM)
                    var unAllotProfirVoucher = new RecordingVoucher()
                    {
                        StoreId = curStore.Id,
                        //单据编号
                        BillNumber = $"unAllotProfir{model.Period.ClosingAccountDate.ToString("yyyyMM")}",
                        //单据类型
                        BillTypeId = 0,
                        //系统生成
                        GenerateMode = (int)GenerateMode.Auto,
                        //审核时间
                        AuditedDate = DateTime.Now,
                        RecordTime = model.Period.ClosingAccountDate,
                        //自动审核
                        AuditedStatus = true
                    };

                    //未分配利润科目
                    var undistributedProfit = _accountingService.Parse(curStore.Id, AccountingCodeEnum.UndistributedProfit);
                    if (undistributedProfit == null)
                    {
                        return Warning($"公司未分配利润科目未配置。");
                    }

                    //本年利润
                    var thisYearProfits = _accountingService.Parse(curStore.Id, AccountingCodeEnum.ThisYearProfits);
                    if (thisYearProfits == null)
                    {
                        return Warning($"公司本年利润科目未配置。");
                    }

                    //年末未分配利润，就是年末时未分配利润的余额，包括年初数据及本年数据。
                    //未分配利润 : 未分配利润是本年利润的余额从反方向结转过来的。
                    //本年利润 : 本年利润科目是归集了所有成本费用收入和期间费用类科目得到的金额
                    //未分配利润 ＝ 年初数 + 本年未分配利润 ＝ 年初数 + 本年净利润－本年计提的公积
                    //年末未分配利润不是计算出来的，当收入类结转到本年利润贷方，成本类结转到本年利润借方，期间费用结转到本年利润的借方之后，如果收入大于成本费用，那么本月就是盈利的。1-12月都这样做了之后，在12月底，将本年利润科目的余额，如果是贷方表示盈利，从借方转出，转入利润分配的贷方。如果是借方余额表示亏损，从贷方转入本年利润的借方。这样，本年利润数据就出来了。

                    //获取本年利润科目余额
                    //var thisYearProfit = model.TrialBalances.Where(s => s.AccountingOptionId == thisYearProfits.Id).FirstOrDefault();
                    //if (thisYearProfit != null)
                    //{
                    //    var typCredit = thisYearProfit.EndBalanceCredit;
                    //    //贷方表示盈利
                    //    if (typCredit > 0)
                    //    { 

                    //    }

                    //    var typDebit = thisYearProfit.EndBalanceDebit;
                    //}


                    unAllotProfirVoucher.Items.Add(new VoucherItem()
                    {
                        StoreId = curStore.Id,
                        RecordTime = model.Period.ClosingAccountDate,
                        Summary = $"第{model.Period.ClosingAccountDate.ToString("MM")}期结转损益",
                        AccountingOptionName = "未分配利润",
                        AccountingOptionId = undistributedProfit?.Id ?? 0,
                        DebitAmount = 0,
                        CreditAmount = model.YearTotal
                    });

                    unAllotProfirVoucher.Items.Add(new VoucherItem()
                    {
                        StoreId = curStore.Id,
                        RecordTime = model.Period.ClosingAccountDate,
                        Summary = $"第{model.Period.ClosingAccountDate.ToString("MM")}期结转损益",
                        AccountingOptionName = "本年利润",
                        AccountingOptionId = thisYearProfits?.Id ?? 0,
                        DebitAmount = model.YearTotal,
                        CreditAmount = 0
                    });

                    model.UnAllotProfirVoucher = unAllotProfirVoucher;
                }
            }
            catch (Exception)
            {
                return Error($"任务{model.Task.Name}失败");
            }

            #endregion

            return Successful($"{model.Task.Name}完成", model);
        }

        /// <summary>
        /// 生成结账凭证行
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual IActionResult ProfitAndLossCarryOver_Task5([FromBody]CheckOutModel model)
        {
            try
            {

                if (model.IncomeVoucher.Items == null || !model.IncomeVoucher.Items.Any())
                {
                    return Warning($"损益类（收入）凭证未声明");
                }

                if (model.ExpenseVoucher.Items == null || !model.IncomeVoucher.Items.Any())
                {
                    return Warning($"损益类（支出）凭证未声明");
                }


                //本年利润
                var thisYearProfits = _accountingService.Parse(curStore.Id, AccountingCodeEnum.ThisYearProfits);
                if (thisYearProfits == null)
                {
                    return Warning($"公司本年利润科目未配置.");
                }



                #region //生成 损益类（收入）科目 结账凭证行




                //// 收入类科目合计 -  费用类科目合计
                //if ((revenue_total - expense_total) > 0)
                //{
                //    //本年利润
                //    //debit=0
                //    //credit = revenue_total - expense_total
                //}

                //if ((revenue_total - expense_total) < 0)
                //{
                //    //本年利润
                //    //debit = expense_total - revenue_total
                //    //credit = 0
                //}

                model.IncomeVoucher.Items.Add(new VoucherItem()
                {
                    StoreId = curStore.Id,
                    Direction = 1, //计入贷方
                    RecordTime = model.Period.ClosingAccountDate,
                    Summary = $"第{model.Period.ClosingAccountDate.ToString("MM")}期结转损益",
                    AccountingOptionName = $"{thisYearProfits?.Name}:{thisYearProfits?.Code}",//本年利润
                    AccountingOptionId = thisYearProfits?.Id ?? 0,
                    CreditAmount = model.RevenueTotal, //计入贷方
                    DebitAmount = 0 //借方清0
                });

                //凭证确认
                _recordingVoucherService.CreateRecordingVoucher(curStore.Id, curUser.Id, model.IncomeVoucher);



                #endregion


                #region //生成 损益类（支出）科目 结账凭证行

                //// 收入类科目合计 -  费用类科目合计
                //if ((revenue_total - expense_total) > 0)
                //{
                //    //本年利润
                //    //debit=0
                //    //credit = revenue_total - expense_total
                //}

                //if ((revenue_total - expense_total) < 0)
                //{
                //    //本年利润
                //    //debit = expense_total - revenue_total
                //    //credit = 0
                //}

                model.ExpenseVoucher.Items.Add(new VoucherItem()
                {
                    StoreId = curStore.Id,
                    Direction = 0, //计入贷方
                    RecordTime = model.Period.ClosingAccountDate,
                    Summary = $"第{model.Period.ClosingAccountDate.ToString("MM")}期结转损益",
                    AccountingOptionName = $"{thisYearProfits?.Name}:{thisYearProfits?.Code}",//本年利润
                    AccountingOptionId = thisYearProfits?.Id ?? 0,
                    DebitAmount = model.ExpenseTotal, //计入借方
                    CreditAmount = 0 //贷方清0
                });


                //凭证确认
                _recordingVoucherService.CreateRecordingVoucher(curStore.Id, curUser.Id, model.ExpenseVoucher);


                #endregion


                //未分配利润 凭证确认
                if (model.Period.ClosingAccountDate.Month == 12)
                {
                    _recordingVoucherService.CreateRecordingVoucher(curStore.Id, curUser.Id, model.UnAllotProfirVoucher);
                }


                //生成科目余额表
                if (model.TrialBalances.Any())
                {
                    model.TrialBalances.ForEach(s =>
                    {
                        s.PeriodDate = model.Period.ClosingAccountDate.AddDays(1 - model.Period.ClosingAccountDate.Day).AddMonths(1).AddDays(-1);
                        s.CreatedOnUtc = DateTime.Now;

                        var tb = _trialBalanceService.FindTrialBalance(curStore.Id, s.AccountingTypeId, s.AccountingOptionId, s.PeriodDate);
                        if (tb == null)
                        {
                            _trialBalanceService.InsertTrialBalance(s);
                        }
                        else
                        {
                            s.Id = tb.Id;
                            _trialBalanceService.UpdateTrialBalance(s);
                        }

                    });
                }

            }
            catch (Exception)
            {
                return Error($"{model.Task.Name}失败");
            }

            return Successful($"{model.Task.Name}完成", model);
        }

        #endregion


        /// <summary>
        /// 结转确认(Step.4)
        /// </summary>
        /// <param name="confirmModel"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual IActionResult CarryForward(ClosingAccountsModel model)
        {
            try
            {
                var period = model.ClosingAccountDate;

                #region 生成资产负债表（BalanceSheet）

                var balanceSheets = new List<BalanceSheet>();
                int[] bsTypes = new[] { (int)AccountingEnum.Assets, (int)AccountingEnum.Liability, (int)AccountingEnum.Rights };
                var bsTrees = GetBalanceSheetTree(curStore.Id, period, bsTypes, 0, null);
                foreach (var node in bsTrees)
                {
                    if (node.Children.Any())
                    {
                        foreach (var node1 in node.Children)
                        {
                            if (node1.Children.Any())
                            {
                                foreach (var node2 in node1.Children)
                                {
                                    balanceSheets.Add(node2.BalanceSheet);
                                }
                            }
                            balanceSheets.Add(node1.BalanceSheet);
                        }
                    }
                    balanceSheets.Add(node.BalanceSheet);
                }
                if (balanceSheets.Any())
                {
                    balanceSheets.ForEach(s =>
                    {
                        s.PeriodDate = period.AddDays(1 - period.Day).AddMonths(1).AddDays(-1);
                        s.CreatedOnUtc = DateTime.Now;

                        var bs = _ledgerDetailsService.FindBalanceSheet(curStore.Id, s.AccountingTypeId, s.AccountingOptionId, s.PeriodDate);
                        if (bs == null)
                        {
                            _ledgerDetailsService.InsertBalanceSheet(s);
                        }
                        else
                        {
                            s.Id = bs.Id;
                            _ledgerDetailsService.UpdateBalanceSheet(s);
                        }

                    });
                }

                #endregion

                #region 生成利润表（ProfitSheet）

                //创建利润表
                var profitSheets = new List<ProfitSheet>();
                int[] psTypes = new[] { (int)AccountingEnum.Income, (int)AccountingEnum.Expense };
                var psTrees = GetProfitSheetTree(curStore.Id, period, psTypes, 0, null);
                foreach (var node in psTrees)
                {
                    if (node.Children.Any())
                    {
                        foreach (var node1 in node.Children)
                        {
                            if (node1.Children.Any())
                            {
                                foreach (var node2 in node1.Children)
                                {
                                    profitSheets.Add(node2.ProfitSheet);
                                }
                            }
                            profitSheets.Add(node1.ProfitSheet);
                        }
                    }
                    profitSheets.Add(node.ProfitSheet);
                }
                if (profitSheets.Any())
                {
                    profitSheets.ForEach(s =>
                    {
                        s.PeriodDate = period.AddDays(1 - period.Day).AddMonths(1).AddDays(-1);
                        s.CreatedOnUtc = DateTime.Now;

                        var ps = _ledgerDetailsService.FindProfitSheet(curStore.Id, s.AccountingTypeId, s.AccountingOptionId, s.PeriodDate);
                        if (ps == null)
                        {
                            _ledgerDetailsService.InsertProfitSheet(s);
                        }
                        else
                        {
                            s.Id = ps.Id;
                            _ledgerDetailsService.UpdateProfitSheet(s);
                        }

                    });
                }

                #endregion

                #region 生成结账会计期间

                var exist = _closingAccountsService.GetClosingAccounts(curStore?.Id, model.ClosingAccountDate).Count() > 0;
                if (!exist)
                {
                    var last = model.ClosingAccountDate.AddDays(1 - model.ClosingAccountDate.Day).AddMonths(1).AddDays(-1);
                    var closingAccounts = new ClosingAccountsModel()
                    {
                        ClosingAccountDate = last,
                        StoreId = curStore.Id,
                        CheckUserId = curUser?.Id,
                        CheckDate = DateTime.Now,
                        CheckStatus = true,
                        LockStatus = true,
                    };
                    _closingAccountsService.InsertClosingAccounts(closingAccounts.ToEntity<ClosingAccounts>());
                }
                else
                {
                    var last = DateTime.Now.AddDays(1 - DateTime.Now.Day).AddMonths(1).AddDays(-1);
                    var closing = _closingAccountsService.GetClosingAccounts(curStore?.Id, model.ClosingAccountDate).FirstOrDefault();
                    closing.ClosingAccountDate = model.ClosingAccountDate;
                    closing.StoreId = curStore.Id;
                    closing.CheckUserId = curUser?.Id;
                    closing.CheckDate = last;
                    closing.CheckStatus = true;
                    closing.LockStatus = true;
                    _closingAccountsService.UpdateClosingAccounts(closing);
                }

                #endregion

                return Successful("结转成功");
            }
            catch (Exception exc)
            {
                return Error(exc.Message);
            }
        }

        /// <summary>
        /// 解锁帐
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual IActionResult LockAccount(ClosingAccountsModel model)
        {
            try
            {
                var last = model.ClosingAccountDate.AddDays(1 - model.ClosingAccountDate.Day).AddMonths(1).AddDays(-1);
                var period = _closingAccountsService.GetClosingAccounts(curStore?.Id, model.ClosingAccountDate).FirstOrDefault();
                if (period == null)
                {
                    period = new ClosingAccounts()
                    {
                        ClosingAccountDate = last,
                        StoreId = curStore.Id,
                        CheckUserId = curUser?.Id,
                        CheckDate = DateTime.Now,
                        CheckStatus = false,
                        LockStatus = true,
                    };
                    _closingAccountsService.InsertClosingAccounts(period);
                    return Successful(period.LockStatus, "成功");
                }
                else
                {
                    period.LockStatus = !period.LockStatus;
                    _closingAccountsService.UpdateClosingAccounts(period);
                    return Successful(period.LockStatus, "成功");
                }
            }
            catch (Exception exc)
            {
                return Error(exc.Message);
            }
        }

        /// <summary>
        /// 反结转（基于某些特殊原因，例如结账后发现上一期间存在单据错漏，需要修改已经结账期间单据，那么可以通过选择反结账功能来实现。）
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public virtual IActionResult CounterCheckout(int ClosingAccountId)
        {
            try
            {
                //重新打开会计期间
                var closing = _closingAccountsService.GetClosingAccountsById(ClosingAccountId);
                if (closing != null)
                {
                    //1.删除损益结转凭证和明细
                    _recordingVoucherService.DeleteRecordingVoucherFromPeriod(curStore.Id, closing.ClosingAccountDate, $"settle{closing.ClosingAccountDate.ToString("yyyyMM")}");

                    //2.重置科目余额表（余额清零）
                    var trialBalances = _trialBalanceService.TryGetTrialBalances(curStore.Id, closing.ClosingAccountDate);
                    trialBalances.ToList().ForEach(t =>
                    {
                        t.InitialBalanceCredit = 0;
                        t.InitialBalanceDebit = 0;
                        t.PeriodBalanceDebit = 0;
                        t.PeriodBalanceCredit = 0;
                        t.EndBalanceCredit = 0;
                        t.EndBalanceDebit = 0;
                    });
                    if (trialBalances?.Any() ?? false)
                    {
                        _trialBalanceService.UpdateTrialBalances(trialBalances);
                    }

                    //3.反向记账分录月结
                    var costOfRVS = _recordingVoucherService.GetLikeRecordingVoucherFromPeriod(curStore.Id, closing.ClosingAccountDate, "costOf");
                    if (costOfRVS != null && costOfRVS.Any())
                    {
                        foreach (var rvs in costOfRVS)
                        {
                            var debit = _recordingVoucherService.GetVoucherItemsByRecordingVoucherId(curStore.Id, rvs.Id).Where(s => s.Direction == 0).FirstOrDefault();

                            var credit = _recordingVoucherService.GetVoucherItemsByRecordingVoucherId(curStore.Id, rvs.Id).Where(s => s.Direction == 1).FirstOrDefault();

                            if (debit == null || credit == null)
                            {
                                break;
                            }

                            switch (rvs.BillTypeId)
                            {
                                case (int)CostOfSettleEnum.CostOfPriceAdjust:
                                    {
                                        _recordingVoucherService.CreateCostVoucher(CostOfSettleEnum.CostOfPriceAdjust,
                                         curStore.Id,
                                         curUser.Id,
                                         _accountingService.ReserveParse(curStore.Id, credit?.AccountingOptionId ?? 0),
                                         _accountingService.ReserveParse(curStore.Id, debit?.AccountingOptionId ?? 0),
                                         credit?.CreditAmount ?? 0,
                                         debit?.DebitAmount ?? 0,
                                         closing,
                                         true);
                                    }
                                    break;
                                case (int)CostOfSettleEnum.CostOfPurchaseReject:
                                    {
                                        _recordingVoucherService.CreateCostVoucher(CostOfSettleEnum.CostOfPurchaseReject,
                                        curStore.Id,
                                        curUser.Id,
                                        _accountingService.ReserveParse(curStore.Id, credit?.AccountingOptionId ?? 0),
                                        _accountingService.ReserveParse(curStore.Id, debit?.AccountingOptionId ?? 0),
                                        credit?.CreditAmount ?? 0,
                                        debit?.DebitAmount ?? 0,
                                        closing,
                                        true);
                                    }
                                    break;
                                case (int)CostOfSettleEnum.CostOfJointGoods:
                                    {
                                        _recordingVoucherService.CreateCostVoucher(CostOfSettleEnum.CostOfJointGoods,
                                        curStore.Id,
                                        curUser.Id,
                                        _accountingService.ReserveParse(curStore.Id, credit?.AccountingOptionId ?? 0),
                                        _accountingService.ReserveParse(curStore.Id, debit?.AccountingOptionId ?? 0),
                                        credit?.CreditAmount ?? 0,
                                        debit?.DebitAmount ?? 0,
                                        closing,
                                        true);
                                    }
                                    break;
                                case (int)CostOfSettleEnum.CostOfStockAdjust:
                                    {
                                        _recordingVoucherService.CreateCostVoucher(CostOfSettleEnum.CostOfStockAdjust,
                                        curStore.Id,
                                        curUser.Id,
                                        _accountingService.ReserveParse(curStore.Id, credit?.AccountingOptionId ?? 0),
                                        _accountingService.ReserveParse(curStore.Id, debit?.AccountingOptionId ?? 0),
                                        credit?.CreditAmount ?? 0,
                                        debit?.DebitAmount ?? 0,
                                        closing,
                                        true);
                                    }
                                    break;
                                case (int)CostOfSettleEnum.CostOfStockLoss:
                                    {
                                        _recordingVoucherService.CreateCostVoucher(CostOfSettleEnum.CostOfStockLoss,
                                        curStore.Id,
                                        curUser.Id,
                                        _accountingService.ReserveParse(curStore.Id, credit?.AccountingOptionId ?? 0),
                                        _accountingService.ReserveParse(curStore.Id, debit?.AccountingOptionId ?? 0),
                                        credit?.CreditAmount ?? 0,
                                        debit?.DebitAmount ?? 0,
                                        closing,
                                        true);
                                    }
                                    break;
                                //销售类
                                case (int)CostOfSettleEnum.CostOfSales:
                                    {
                                        _recordingVoucherService.CreateCostVoucher(CostOfSettleEnum.CostOfSales,
                                         curStore.Id,
                                         curUser.Id,
                                        _accountingService.ReserveParse(curStore.Id, credit?.AccountingOptionId ?? 0),
                                         _accountingService.ReserveParse(curStore.Id, debit?.AccountingOptionId ?? 0),
                                         credit?.CreditAmount ?? 0,
                                         debit?.DebitAmount ?? 0,
                                         closing,
                                         true);
                                    }
                                    break;
                            }
                        }
                    }
                    //4.删除成本汇总表和汇总明细
                    _closingAccountsService.DeleteCostPriceSummery(curStore.Id, closing.ClosingAccountDate);

                    //5.删除会计期间
                    _closingAccountsService.DeleteClosingAccounts(closing);

                    return Successful("成功");
                }
                else
                {
                    return Warning("失败");
                }
            }
            catch (Exception exc)
            {
                return Error(exc.Message);
            }
        }


        #region NonAction

        [NonAction]
        private List<TrialBalanceTree> GetTrialBalanceTree(int store, DateTime period, int parentId, List<int> options)
        {
            List<TrialBalanceTree> trees = new List<TrialBalanceTree>();
            var perentList = _accountingService.GetAccountingOptionsByParentId(store, parentId);
            if (perentList != null && perentList.Count > 0)
            {
                foreach (var b in perentList)
                {
                    var tempList = GetTrialBalanceTree(store, period, b.Id, options);
                    b.Selected = options != null ? options.Contains(b.Id) : false;

                    //获取科目上一期间余额
                    var lastMonth = period.AddMonths(-1);
                    var last = _trialBalanceService.GetTrialBalance(store, b.AccountingTypeId, b.Id, lastMonth);

                    //获取本期科目凭证明细项目
                    var currVoucherItems = _recordingVoucherService.GetVoucherItemsByAccountingOptionIdFromPeriod(curStore.Id, b.Id, period);

                    var curr = new TrialBalance()
                    {
                        Id = 0,
                        StoreId = store,
                        //关联科目
                        AccountingOption = b,
                        DirectionsType = CommonHelper.GetAccountingDirections(b.AccountingTypeId),
                        AccountingTypeId = b.AccountingTypeId,
                        AccountingOptionId = b.Id,
                        AccountingOptionName = b.Name,
                        PeriodDate = period,

                        //本期期初为上一期期末
                        InitialBalanceDebit = last?.EndBalanceDebit ?? 0,
                        InitialBalanceCredit = last?.EndBalanceCredit ?? 0,

                        //本期借
                        PeriodBalanceDebit = currVoucherItems?.Sum(s => s.DebitAmount ?? 0),
                        //本期贷
                        PeriodBalanceCredit = currVoucherItems?.Sum(s => s.CreditAmount ?? 0),

                        EndBalanceDebit = 0,
                        EndBalanceCredit = 0,

                        CreatedOnUtc = DateTime.Now
                    };

                    //本期末
                    //判断方向（根据科目的类型，判断余额方向是借方或者贷方）
                    //资产类科目：期末借方余额=期初借方余额+本期借方发生额-本期贷方发生额；
                    //负债及所有者权益类科目：期末贷方余额 = 期初贷方余额 + 本期贷方发生额 - 本期借方发生额。
                    if (curr.DirectionsType == DirectionsTypeEnum.IN)
                    {
                        curr.EndBalanceDebit = curr.InitialBalanceDebit + curr.PeriodBalanceDebit - curr.PeriodBalanceCredit;
                    }
                    else
                    {
                        curr.EndBalanceDebit = curr.InitialBalanceCredit + curr.PeriodBalanceCredit - curr.PeriodBalanceDebit;
                    }

                    var node = new TrialBalanceTree
                    {
                        AccountingOption = b,
                        TrialBalance = curr,
                        Children = new List<TrialBalanceTree>()
                    };

                    if (tempList != null && tempList.Count > 0)
                    {
                        //期初余额
                        node.TrialBalance.InitialBalanceDebit += tempList.Sum(s => s.TrialBalance?.InitialBalanceDebit ?? 0);
                        node.TrialBalance.InitialBalanceCredit += tempList.Sum(s => s.TrialBalance?.InitialBalanceCredit ?? 0);

                        //本期发生额
                        node.TrialBalance.PeriodBalanceDebit += tempList.Sum(s => s.TrialBalance?.PeriodBalanceDebit ?? 0);
                        node.TrialBalance.PeriodBalanceCredit += tempList.Sum(s => s.TrialBalance?.PeriodBalanceCredit ?? 0);
                        //期末余额
                        node.TrialBalance.EndBalanceDebit += tempList.Sum(s => s.TrialBalance?.EndBalanceDebit ?? 0);
                        node.TrialBalance.EndBalanceCredit += tempList.Sum(s => s.TrialBalance?.EndBalanceCredit ?? 0);

                        node.Children = tempList;
                    }

                    trees.Add(node);
                }
            }
            return trees;
        }

        /// <summary>
        /// 获取当期商品成本汇总
        /// </summary>
        /// <returns></returns>
        [NonAction]
        private List<StockQtySummery> GetMonthProductCostSummery(ClosingAccountsModel model, int[] billTypes)
        {
            //当期
            var period = model.ToEntity<ClosingAccounts>();
            //所在月第一天
            var first = period.ClosingAccountDate.AddDays(1 - period.ClosingAccountDate.Day);
            //所在月最后一天
            var last = period.ClosingAccountDate.AddDays(1 - period.ClosingAccountDate.Day).AddMonths(1).AddDays(-1);

            //获取所有库存商品
            var stockProducts = _stockService.GetAllStockProducts(curStore.Id);

            /*
            将本月发生的出入库类单据（按照单据中商品的成本价）进行成本结转
            出入库类单据： 销售单  12 退货单 14  采购单  22 采购退货单  24  盘点盈亏单 32  成本调价单 33 报损单 34 组合单 37  拆分单 38
            每月发出成本，获取以下单出入库记录
            */
            var monthProductCosts = new List<StockQtySummery>();

            try
            {
                //获取当期商品出入库量和成本金额
                var stockQTYs = _stockService.GetStockQTY(curStore.Id, billTypes, first, last);
                //每个产品最多取两行，一个本月出库，一个本月入库 如果一个产品在本月没有出入库记录，则不生成发出成本记录(这里，单位采用最小单位)
                var stockQTYGroups = stockQTYs.GroupBy(m => new StockQtySummery
                {
                    //按ProductId 和 Direction 分组
                    ProductId = m.ProductId,
                    Direction = m.Direction,

                    ProductName = m.ProductName,
                    ProductCode = m.ProductCode,

                    UnitId = m.UnitId,
                    SmallUnitId = m.SmallUnitId,
                    BigUnitId = m.BigUnitId,

                    UnitName = m.UnitName,
                    SmallUnitName = m.SmallUnitName,
                    BigUnitName = m.BigUnitName

                });


                //填充monthProductCosts
                foreach (var stockQTY in stockQTYGroups)
                {
                    var summery = stockQTY.Key;
                    summery.BillLists = stockQTY.Select(s => s.BillId).ToList();
                    summery.BillNumberLists = stockQTY.Select(s => s.BillCode).ToList();
                    summery.Products = stockQTY.ToList();
                    monthProductCosts.Add(summery);
                }


                foreach (var m in stockProducts)
                {
                    if (monthProductCosts.Where(s => s.ProductId == m.ProductId).Count() == 0)
                    {
                        monthProductCosts.Add(new StockQtySummery()
                        {
                            ProductId = m.ProductId,
                            Direction = 0,

                            ProductName = m.ProductName,
                            ProductCode = m.ProductCode,

                            UnitId = m.UnitId,
                            SmallUnitId = m.SmallUnitId,
                            BigUnitId = m.BigUnitId,

                            UnitName = m.UnitName,
                            SmallUnitName = m.SmallUnitName,
                            BigUnitName = m.BigUnitName,
                            Products = new List<StockQty>() { m }
                        });
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            return monthProductCosts;
        }

        /// <summary>
        /// 匹配并填充商品成本汇总字典
        /// </summary>
        /// <returns></returns>
        [NonAction]
        private List<CostPriceSummery> GetMonthProductCostDicts(ClosingAccountsModel model, List<StockQtySummery> monthProductCosts)
        {
            //获取当期上一个期间成本汇总
            var lastMonthCostSummerires = _closingAccountsService.GetLastCostPriceSummeries(curStore.Id, model.ClosingAccountDate);

            var monthProductCostDicts = new List<CostPriceSummery>();

            foreach (var product in monthProductCosts)
            {
                int sumQTY = 0;
                decimal sumCost = 0;
                decimal sumPrice = 0;
                int count = product.Products.Count;

                foreach (var sub in product.Products)
                {
                    //开单单位为小单位时，数量就等于开单量
                    if (sub.UnitId == sub.SmallUnitId)
                    {
                        sumQTY += sub.Quantity;
                        sumPrice += sub.Price;
                        sumCost += sub.Quantity * sub.Price;
                    }
                    //开单单位为大单位时，数量就等于开单量 * 大单位转化量
                    else if (sub.UnitId == sub.BigUnitId)
                    {
                        sumQTY += sub.Quantity * sub.BigQuantity;
                        sumPrice += sub.Price;
                        sumCost += (sub.Quantity * sub.BigQuantity) * sub.Price;
                    }
                }

                //获取上一期商品对应的的 剩余数量，和剩余数量成本
                //注意：这里由于汇总表和成本变化明细表分开存储
                var last = lastMonthCostSummerires.Where(s => s.ProductId == product.ProductId).FirstOrDefault();

                var mp = monthProductCostDicts.Where(s => s.ProductId == product.ProductId).FirstOrDefault();
                if (mp == null)
                {
                    mp = new CostPriceSummery()
                    {
                        StoreId = curStore.Id,
                        Date = model.ClosingAccountDate,
                        ProductId = product.ProductId,
                        ProductCode = string.IsNullOrEmpty(product.ProductCode) ? "" : product.ProductCode,
                        ProductName = string.IsNullOrEmpty(product.ProductName) ? "" : product.ProductName,

                        //全部转小单位
                        UnitId = product.SmallUnitId,
                        UnitName = product.SmallUnitName
                    };

                    //期初 = 上期末
                    mp.InitQuantity = last?.EndQuantity ?? 0;
                    mp.InitAmount = last?.EndAmount ?? 0;
                    mp.InitPrice = last?.EndPrice ?? 0;


                    //期入
                    if (product.Direction == 1)
                    {
                        mp.InQuantity = sumQTY;
                        mp.InAmount = sumCost;
                        //平均成本单价
                        mp.InPrice = sumPrice / count;

                    }
                    //期出
                    else if (product.Direction == 2)
                    {
                        mp.OutQuantity = sumQTY;
                        mp.OutAmount = sumCost;
                        //平均成本单价
                        mp.OutPrice = sumPrice / count;
                    }

                    //算出 本月的剩余的数量和成本
                    //期末
                    mp.EndQuantity = mp.InitQuantity - mp.OutQuantity + mp.InQuantity;// 期初数量 - 本期出数量 + 本期入数量
                    mp.EndAmount = mp.InitAmount - mp.OutAmount + mp.InAmount;  // 期初成本 - 本期出成本 + 本期入成本
                    mp.EndPrice = 0;//成本单价需要重新计算


                    mp.Records = product.Products.Select(s =>
                    {
                        decimal sumCostAmount = 0;
                        int sumCostQTY = 0;

                        //开单单位为小单位时，数量就等于开单量
                        if (s.UnitId == s.SmallUnitId)
                        {
                            sumCostQTY = s.Quantity;
                            sumCostAmount = s.Quantity * s.Price;
                        }
                        //开单单位为大单位时，数量就等于开单量 * 大单位转化量
                        else if (s.UnitId == s.BigUnitId)
                        {
                            sumCostQTY = s.Quantity * s.BigQuantity;
                            sumCostAmount = (s.Quantity * s.BigQuantity) * s.Price;
                        }

                        return new CostPriceChangeRecords()
                        {
                            StoreId = curStore.Id,
                            Date = model.ClosingAccountDate,
                            CreatedOnUtc = s.CreatedOnUtc,
                            BillId = s.BillId,
                            BillTypeId = s.BillType,
                            BillNumber = s.BillCode,

                            //全部转小单位
                            UnitId = s.SmallUnitId,
                            UnitName = s.SmallUnitName,

                            ProductId = s.ProductId,
                            ProductCode = string.IsNullOrEmpty(s.ProductCode) ? "" : s.ProductCode,
                            ProductName = string.IsNullOrEmpty(s.ProductName) ? "" : s.ProductName,
                            BillTypeName = CommonHelper.GetEnumDescription<BillTypeEnum>(s.BillType),

                            //本期入库
                            InPrice = s.Direction == 1 ? s.Price : 0,
                            InAmount = s.Direction == 1 ? sumCostAmount : 0,
                            InQuantity = s.Direction == 1 ? sumCostQTY : 0,

                            //本期出库
                            OutPrice = s.Direction == 2 ? s.Price : 0,
                            OutAmount = s.Direction == 2 ? sumCostAmount : 0,
                            OutQuantity = s.Direction == 2 ? sumCostQTY : 0,

                            // 期末 = 期初数量 - 本期出数量 + 本期入数量
                            EndPrice = mp.EndPrice,
                            EndAmount = mp.InAmount - (s.Direction == 2 ? sumCostAmount : 0) + (s.Direction == 1 ? sumCostAmount : 0),
                            EndQuantity = mp.InitQuantity - (s.Direction == 2 ? sumCostQTY : 0) + (s.Direction == 1 ? sumCostQTY : 0)

                        };
                    }).ToList();

                    monthProductCostDicts.Add(mp);

                }
                else
                {
                    //期初 = 上期末
                    mp.InitQuantity = last?.EndQuantity ?? 0;
                    mp.InitAmount = last?.EndAmount ?? 0;
                    mp.InitPrice = last?.EndPrice ?? 0;

                    //期入
                    if (product.Direction == 1)
                    {
                        mp.InQuantity = sumQTY;
                        mp.InAmount = sumCost;
                        mp.InPrice = sumPrice / count;
                        //平均成本单价
                    }
                    //期出
                    else if (product.Direction == 2)
                    {
                        mp.OutQuantity = sumQTY;
                        mp.OutAmount = sumCost;
                        //平均成本单价
                        mp.OutPrice = sumPrice / count;
                    }

                    //算出 本月的剩余的数量和成本
                    //期末
                    mp.EndQuantity = mp.InitQuantity - mp.OutQuantity + mp.InQuantity;// 期初数量 - 本期出数量 + 本期入数量
                    mp.EndAmount = mp.InitAmount - mp.OutAmount + mp.InAmount;  // 期初成本 - 本期出成本 + 本期入成本
                    mp.EndPrice = 0;//成本单价需要重新计算

                    product.Products.ToList().ForEach(s =>
                    {
                        decimal sumCostAmount = 0;
                        int sumCostQTY = 0;

                        //开单单位为小单位时，数量就等于开单量
                        if (s.UnitId == s.SmallUnitId)
                        {
                            sumCostQTY = s.Quantity;
                            sumCostAmount = s.Quantity * s.Price;
                        }
                        //开单单位为大单位时，数量就等于开单量 * 大单位转化量
                        else if (s.UnitId == s.BigUnitId)
                        {
                            sumCostQTY = s.Quantity * s.BigQuantity;
                            sumCostAmount = (s.Quantity * s.BigQuantity) * s.Price;
                        }

                        var cpcr = new CostPriceChangeRecords()
                        {
                            Date = model.ClosingAccountDate,
                            CreatedOnUtc = s.CreatedOnUtc,
                            BillId = s.BillId,
                            BillTypeId = s.BillType,
                            BillNumber = s.BillCode,

                            //全部转小单位
                            UnitId = s.SmallUnitId,
                            UnitName = s.SmallUnitName,

                            ProductId = s.ProductId,
                            ProductCode = string.IsNullOrEmpty(s.ProductCode) ? "" : s.ProductCode,
                            ProductName = string.IsNullOrEmpty(s.ProductName) ? "" : s.ProductName,
                            BillTypeName = CommonHelper.GetEnumDescription<BillTypeEnum>(s.BillType),

                            //本期入库
                            InPrice = s.Direction == 1 ? s.Price : 0,
                            InAmount = s.Direction == 1 ? sumCostAmount : 0,
                            InQuantity = s.Direction == 1 ? sumCostQTY : 0,

                            //本期出库
                            OutPrice = s.Direction == 2 ? s.Price : 0,
                            OutAmount = s.Direction == 2 ? sumCostAmount : 0,
                            OutQuantity = s.Direction == 2 ? sumCostQTY : 0,

                            // 期末 = 期初数量 - 本期出数量 + 本期入数量
                            EndPrice = mp.EndPrice,
                            EndAmount = mp.InAmount - (s.Direction == 2 ? sumCostAmount : 0) + (s.Direction == 1 ? sumCostAmount : 0),
                            EndQuantity = mp.InitQuantity - (s.Direction == 2 ? sumCostQTY : 0) + (s.Direction == 1 ? sumCostQTY : 0)

                        };

                        mp.Records.Add(cpcr);

                    });
                }
            }

            return monthProductCostDicts;
        }

        /// <summary>
        /// 重新计算成本价格,并替换商品成本价
        /// </summary>
        /// <returns></returns>
        private decimal RecalculateCostPrice(ClosingAccountsModel period, List<CostPriceSummery> monthProductCostDicts)
        {
            decimal allBalancePrice = 0;
            //计算当期库存商品科目（所有商品类别涉及的科目）贷方金额合计，发出时已结转的实际成本
            var vouchers_InventoryGoods = _recordingVoucherService.GetVoucherItemsByAccountCodeTypeIdFromPeriod(curStore.Id, (int)AccountingCodeEnum.InventoryGoods, period.ClosingAccountDate);
            //贷方金额合计
            var realOutCost = vouchers_InventoryGoods?.Sum(s => s.DebitAmount ?? 0);
            foreach (var costProduct in monthProductCostDicts)
            {
                //当前商品销售成本金额（加权平均计算而来）
                var currentPeriodOutCost = ComputeBalancePrice(costProduct, CostMethodEnum.AVERAGE);
                //两者之差 = 当前商品发出成本(当期加权平均成本) - 发出时已结转的实际成本
                var diffCost = currentPeriodOutCost.Item1 - realOutCost;
                allBalancePrice += diffCost ?? 0;
            }
            return allBalancePrice;
        }

        /// <summary>
        /// 计算发出成本
        /// </summary>
        /// <param name="product">成本商品</param>
        /// <param name="costMethodEnum">成本计算方法</param>
        /// <returns></returns>
        [NonAction]
        private Tuple<decimal, decimal> ComputeBalancePrice(CostPriceSummery product, CostMethodEnum costMethodEnum)
        {
            var cm = CostMethodEnum.AVERAGE;
            switch (cm)
            {
                case CostMethodEnum.AVERAGE:
                    {
                        //借：主营业务成本={（期初产品成本+本期入库产品成本）/期初产品数量+本期入库产品)}**产品销售数量

                        //加权平均单价＝（期初结存商品金额+本期收入商品金额-本期非销售发出商品金额）÷（期初结存商品数量+本期收入商品数量-本期非销售发出商品数量） 
                        //本期商品销售成本＝本期商品销售数量×加权平均单价

                        //结存单价(本月该商品的结存单价) = (期初成本(上月该商品的成本余额) + 本期入库成本(本月入库成本)) / (期初数量(上月数量余额) + 本期入库量(本月入库数量))

                        var divisor = (product.InitQuantity + product.InQuantity - product.OutNoSaleQuantity);
                        decimal balance_price = 0;

                        if (divisor != 0)
                        {
                            balance_price = (product.InitAmount + product.InAmount - product.OutNoSaleAmount) / divisor;
                        }

                        //发出成本(全月一次加权平均价) = 结存单价(本月该商品的结存单价) * (发出数量)本期出库量
                        var month_cost = balance_price * product.OutQuantity;
                        return new Tuple<decimal, decimal>(month_cost, balance_price);

                    }
                case CostMethodEnum.FIFO:
                    {
                        //实际成本  = 本期出库成本
                        return new Tuple<decimal, decimal>(product.OutAmount, product.OutPrice);
                    }
                case CostMethodEnum.STD:
                    {
                        //定额成本  = 商品价格 * (发出数量)本期出库量
                        return new Tuple<decimal, decimal>(product.OutPrice * product.OutAmount, product.OutPrice);
                    }
                default:
                    return new Tuple<decimal, decimal>(0, 0);
            }
        }


        /// <summary>
        /// 获取资产负债树
        /// </summary>
        /// <param name="store"></param>
        /// <param name="period"></param>
        /// <param name="types">
        /// (int)AccountingEnum.Assets  资产
        /// (int)AccountingEnum.Liability 负债
        /// (int)AccountingEnum.Rights 权益
        /// </param> 
        /// <param name="parentId"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        [NonAction]
        private List<BalanceSheetTree> GetBalanceSheetTree(int store, DateTime period, int[] types, int parentId, List<int> options)
        {
            List<BalanceSheetTree> trees = new List<BalanceSheetTree>();
            var perentList = _accountingService.GetAccountingOptionsByParentId(store, parentId, types);
            if (perentList != null && perentList.Count > 0)
            {
                foreach (var b in perentList)
                {
                    var tempList = GetBalanceSheetTree(store, period, types, b.Id, options);
                    b.Selected = options != null ? options.Contains(b.Id) : false;

                    //获取本期期末余额
                    var currEnd = _trialBalanceService.GetTrialBalance(store, b.AccountingTypeId, b.Id, period);
                    //获取本期年初余额
                    var yearBegin = _trialBalanceService.GetYearBegainTrialBalance(store, b.Id, period);

                    decimal currEndAmount = 0;
                    decimal currYearBegint = 0;

                    //判别科目方向
                    switch (b.AccountingTypeId)
                    {
                        //资产类 月末余额方向在“借”方
                        case (int)AccountingEnum.Assets:
                            {
                                currEndAmount = currEnd?.EndBalanceDebit ?? 0 - currEnd?.EndBalanceCredit ?? 0;
                                currYearBegint = yearBegin?.EndBalanceDebit ?? 0 - yearBegin?.EndBalanceCredit ?? 0;
                            }
                            break;
                        //负债类 月末余额方向在“贷”方
                        case (int)AccountingEnum.Liability:
                        //所有者权益类 月末余额方向在“贷”方
                        case (int)AccountingEnum.Rights:
                            {
                                currEndAmount = currEnd?.EndBalanceCredit ?? 0 - currEnd?.EndBalanceDebit ?? 0;
                                currYearBegint = yearBegin?.EndBalanceCredit ?? 0 - yearBegin?.EndBalanceDebit ?? 0;
                            }
                            break;
                    }

                    var curr = new BalanceSheet()
                    {
                        Id = 0,
                        StoreId = store,
                        AccountingTypeId = b.AccountingTypeId,
                        AccountingOptionId = b.Id,
                        AccountingOptionName = b.Name,
                        PeriodDate = period,
                        //年初
                        InitialBalance = currYearBegint,
                        //期末
                        EndBalance = currEndAmount,
                        CreatedOnUtc = DateTime.Now
                    };

                    var node = new BalanceSheetTree
                    {
                        AccountingOption = b,
                        BalanceSheet = curr,
                        AccountingType = b.AccountingTypeId,
                        Children = new List<BalanceSheetTree>()
                    };

                    if (tempList != null && tempList.Count > 0)
                    {
                        //年初余额
                        node.BalanceSheet.InitialBalance += tempList.Sum(s => s.BalanceSheet?.InitialBalance ?? 0);
                        //期末余额
                        node.BalanceSheet.EndBalance += tempList.Sum(s => s.BalanceSheet?.InitialBalance ?? 0);

                        node.Children = tempList;
                    }

                    trees.Add(node);
                }
            }
            return trees;
        }


        /// <summary>
        /// 获取利润树
        /// </summary>
        /// <param name="store"></param>
        /// <param name="period"></param>
        /// <param name="types">
        /// (int)AccountingEnum.Income  收入
        /// (int)AccountingEnum.Expense 支出
        /// </param>
        /// <param name="parentId"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        [NonAction]
        private List<ProfitSheetTree> GetProfitSheetTree(int store, DateTime period, int[] types, int parentId, List<int> options)
        {
            List<ProfitSheetTree> trees = new List<ProfitSheetTree>();
            var perentList = _accountingService.GetAccountingOptionsByParentId(store, parentId, types);
            if (perentList != null && perentList.Count > 0)
            {
                foreach (var b in perentList)
                {
                    var tempList = GetProfitSheetTree(store, period, types, b.Id, options);
                    b.Selected = options != null ? options.Contains(b.Id) : false;

                    //获取科目当期损益结转贷方
                    var lossSettle = _recordingVoucherService.GetPeriodLossSettle(store, b.Id, period);
                    //本年累计金额	
                    decimal cumulativeBalance = 0;
                    //本期金额
                    decimal currentBalance = 0;

                    //判别科目方向
                    switch (b.AccountingTypeId)
                    {
                        //收入类
                        case (int)AccountingEnum.Income:
                            {
                                //本年累计金额(查询时动态计算)
                                cumulativeBalance = 0;
                                currentBalance = lossSettle?.DebitAmount ?? 0 - lossSettle?.CreditAmount ?? 0;
                            }
                            break;
                        //支出类
                        case (int)AccountingEnum.Expense:
                            {
                                //本年累计金额(查询时动态计算)
                                cumulativeBalance = 0;
                                currentBalance = lossSettle?.CreditAmount ?? 0 - lossSettle?.DebitAmount ?? 0;
                            }
                            break;
                    }
                    var curr = new ProfitSheet()
                    {
                        Id = 0,
                        StoreId = store,
                        AccountingTypeId = b.AccountingTypeId,
                        AccountingOptionId = b.Id,
                        AccountingOptionName = b.Name,
                        PeriodDate = period,
                        //本年累计金额
                        AccumulatedAmountOfYear = cumulativeBalance,
                        //本期金额
                        CurrentAmount = currentBalance,
                        CreatedOnUtc = DateTime.Now
                    };

                    var node = new ProfitSheetTree
                    {
                        AccountingOption = b,
                        ProfitSheet = curr,
                        AccountingType = b.AccountingTypeId,
                        Children = new List<ProfitSheetTree>()
                    };

                    if (tempList != null && tempList.Count > 0)
                    {
                        //本年累计金额
                        node.ProfitSheet.AccumulatedAmountOfYear += tempList.Sum(s => s.ProfitSheet?.AccumulatedAmountOfYear ?? 0);
                        //本期金额
                        node.ProfitSheet.CurrentAmount += tempList.Sum(s => s.ProfitSheet?.CurrentAmount ?? 0);

                        node.Children = tempList;
                    }

                    trees.Add(node);
                }
            }
            return trees;
        }

        #endregion


    }
}