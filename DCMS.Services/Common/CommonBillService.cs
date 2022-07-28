using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Common;
using DCMS.Core.Domain.Finances;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Purchases;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Domain.Terminals;
using DCMS.Core.Domain.WareHouses;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using DCMS.Services.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DCMS.Services.Common
{
    public class CommonBillService : BaseService, ICommonBillService
    {
        private readonly IUserService _userService;

        public CommonBillService(IStaticCacheManager cacheManager,
            IServiceGetter getter,
            IEventPublisher eventPublisher,
            IUserService userService
            ) : base(getter, cacheManager, eventPublisher)
        {
            _userService = userService;
        }

        #region v3.0


        /// <summary>
        /// 获取经销商剩余预收款金额
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="terminalId"></param>
        /// <param name="accountCodeTypeId"></param>
        /// <returns></returns>
        public virtual decimal GetTerminalBalance(int storeId, int terminalId, int accountCodeTypeId)
        {
            // 销售单（预收款科目）
            string saleSql = @"SELECT 
                                    IFNULL( SUM(sa.CollectionAmount),0) as Value
                                FROM
                                    SaleBill_Accounting_Mapping AS sa 
                                    LEFT JOIN AccountingOptions ao ON sa.AccountingOptionId = ao.Id
                                    LEFT JOIN SaleBills sb ON sa.SaleBillId = sb.Id
                                WHERE sa.StoreId = " + storeId + " and sa.TerminalId = " + terminalId + " AND sb.AuditedStatus=1 AND sb.ReversedStatus=0 AND ao.AccountCodeTypeId IN (SELECT AccountCodeTypeId AS Value FROM (SELECT t1.AccountCodeTypeId,IF(FIND_IN_SET(ParentId, @pids) > 0, @pids:=CONCAT(@pids, ',', AccountCodeTypeId), 0) AS ischild  FROM (SELECT  AccountCodeTypeId, ParentId FROM dcms.AccountingOptions t WHERE StoreId = " + storeId + " AND ParentId = '" + accountCodeTypeId + "' ORDER BY ParentId , AccountCodeTypeId) t1, (SELECT @pids:='" + accountCodeTypeId + "') t2) t3)";

            //退货单（预收款科目）
            string returnSql = @"SELECT 
                                    IFNULL( SUM(sa.CollectionAmount),0) as Value
                                FROM
                                    ReturnBill_Accounting_Mapping AS sa LEFT JOIN
                                    AccountingOptions ao ON sa.AccountingOptionId = ao.Id
                                    LEFT JOIN ReturnBills rb ON sa.ReturnBillId = rb.Id
                                WHERE sa.StoreId = " + storeId + " and sa.TerminalId = " + terminalId + " AND rb.AuditedStatus=1 AND rb.ReversedStatus=0 AND  ao.AccountCodeTypeId IN (SELECT AccountCodeTypeId AS Value FROM (SELECT t1.AccountCodeTypeId,IF(FIND_IN_SET(ParentId, @pids) > 0, @pids:=CONCAT(@pids, ',', AccountCodeTypeId), 0) AS ischild  FROM (SELECT  AccountCodeTypeId, ParentId FROM dcms.AccountingOptions t WHERE StoreId = " + storeId + " AND ParentId = '" + accountCodeTypeId + "' ORDER BY ParentId , AccountCodeTypeId) t1, (SELECT @pids:='" + accountCodeTypeId + "') t2) t3)";

            //预收款单（预收款科目）
            //string advanceReceiptSql = @"SELECT 
            //                        IFNULL( SUM(sa.CollectionAmount),0) as Value
            //                    FROM
            //                        AdvanceReceiptBill_Accounting_Mapping AS sa LEFT JOIN
            //                        AccountingOptions ao ON sa.AccountingOptionId = ao.Id
            //                    WHERE sa.StoreId = " + storeId + " and sa.TerminalId = " + terminalId + "  AND ao.AccountCodeTypeId IN (SELECT AccountCodeTypeId AS Value FROM (SELECT t1.AccountCodeTypeId,IF(FIND_IN_SET(ParentId, @pids) > 0, @pids:=CONCAT(@pids, ',', AccountCodeTypeId), 0) AS ischild  FROM (SELECT  AccountCodeTypeId, ParentId FROM dcms.AccountingOptions t WHERE StoreId = " + storeId + " AND ParentId = '" + accountCodeTypeId + "' ORDER BY ParentId , AccountCodeTypeId) t1, (SELECT @pids:='" + accountCodeTypeId + "') t2) t3)";
            string advanceReceiptSql = @"SELECT IFNULL( SUM(ar.AdvanceAmount),0) as Value FROM dcms.AdvanceReceiptBills as ar left join                             AccountingOptions as a on ar.AccountingOptionId = a.id 
                                            where a.StoreId = " + storeId + " and a.AccountCodeTypeId IN (SELECT AccountCodeTypeId AS Value FROM (SELECT t1.AccountCodeTypeId,IF(FIND_IN_SET(ParentId, @pids) > 0, @pids:=CONCAT(@pids, ',', AccountCodeTypeId), 0) AS ischild  FROM (SELECT  AccountCodeTypeId, ParentId FROM dcms.AccountingOptions t WHERE StoreId = " + storeId + " AND ParentId = '" + accountCodeTypeId + "' ORDER BY ParentId , AccountCodeTypeId) t1, (SELECT @pids:='" + accountCodeTypeId + "') t2) t3) and ar.StoreId = " + storeId + " and  ar.CustomerId = " + terminalId + " and ar.AuditedStatus=1 and ar.ReversedStatus=0";


            //收款单（预收款科目）
            string cashReceiptSql = @"SELECT 
                                    IFNULL( SUM(sa.CollectionAmount),0) as Value
                                FROM
                                    CashReceiptBill_Accounting_Mapping AS sa LEFT JOIN
                                    AccountingOptions ao ON sa.AccountingOptionId = ao.Id
                                    LEFT JOIN CashReceiptBills cb ON sa.CashReceiptBillId = cb.Id
                                WHERE sa.StoreId = " + storeId + " and sa.TerminalId = " + terminalId + " AND cb.AuditedStatus=1 AND cb.ReversedStatus=2  AND ao.AccountCodeTypeId IN (SELECT AccountCodeTypeId AS Value FROM (SELECT t1.AccountCodeTypeId,IF(FIND_IN_SET(ParentId, @pids) > 0, @pids:=CONCAT(@pids, ',', AccountCodeTypeId), 0) AS ischild  FROM (SELECT  AccountCodeTypeId, ParentId FROM dcms.AccountingOptions t WHERE StoreId = " + storeId + " AND ParentId = '" + accountCodeTypeId + "' ORDER BY ParentId , AccountCodeTypeId) t1, (SELECT @pids:='" + accountCodeTypeId + "') t2) t3)";

            // 400 500 200
            decimal saleAmount = SaleBillsRepository_RO.QueryFromSql<DecimalQueryType>(saleSql).FirstOrDefault().Value ?? 0;
            decimal returnAmount = SaleBillsRepository_RO.QueryFromSql<DecimalQueryType>(returnSql).FirstOrDefault().Value ?? 0;
            decimal advanceReceiptAmount = SaleBillsRepository_RO.QueryFromSql<DecimalQueryType>(advanceReceiptSql).FirstOrDefault().Value ?? 0;
            decimal cashReceiptAmount = SaleBillsRepository_RO.QueryFromSql<DecimalQueryType>(cashReceiptSql).FirstOrDefault().Value ?? 0;

            //预收款 + 退货预收 - 销售预收-  收款预收
            return Convert.ToDecimal(Convert.ToDouble(advanceReceiptAmount) + Convert.ToDouble(returnAmount) - Convert.ToDouble(saleAmount) - Convert.ToDouble(cashReceiptAmount));
        }


        /// <summary>
        /// 快速预先收款单据(欠款)
        /// </summary>
        /// <returns></returns>
        public List<CashReceiptItemView> GetCashReceiptItemView(int storeId, int terminalId)
        {
            try
            {
                string queryString = @"(select b.Id,b.BillNumber,b.id as BillId,12 as BillTypeId,b.CreatedOnUtc as MakeBillDate,b.SumAmount as Amount,b.PreferentialAmount as DiscountAmount,0.00 as PaymentedAmount,IFNULL(b.OweCash,0) as ArrearsAmount,0.00 DiscountAmountOnce,0.00 ReceivableAmountOnce,0.00 AmountOwedAfterReceipt,0 as CashReceiptBillId,b.Remark,null as CreatedOnUtc,b.storeid from dcms.SaleBills as b where b.StoreId = " + storeId + " and  b.TerminalId = " + terminalId + " and b.OweCash > 0 and b.AuditedStatus=1 and b.ReversedStatus=0) " +
                "UNION ALL " +
                "(select b.Id, b.BillNumber,b.id as BillId,14 as BillTypeId,b.CreatedOnUtc as MakeBillDate,b.SumAmount as Amount,b.PreferentialAmount as DiscountAmount,0.00 as PaymentedAmount,IFNULL(b.OweCash,0) as ArrearsAmount,0.00 DiscountAmountOnce,0.00 ReceivableAmountOnce,0.00 AmountOwedAfterReceipt,0 as CashReceiptBillId,b.Remark,null as CreatedOnUtc,b.storeid from dcms.ReturnBills as b where b.StoreId = " + storeId + " and  b.TerminalId = " + terminalId + " and b.OweCash > 0 and b.AuditedStatus=1 and b.ReversedStatus=0)";

                var query = SaleBillsRepository_RO.QueryFromSql<CashReceiptItemView>(queryString);
                return query.ToList();
            }
            catch (Exception)
            {
                return new List<CashReceiptItemView>();
            }
        }

        /// <summary>
        /// 快速预先付款单据(欠款)
        /// </summary>
        /// <returns></returns>
        public List<CashReceiptItemView> GetPaymentReceiptItemView(int storeId, int manufacturerid)
        {
            try
            {
                string queryString = @"(select b.Id,b.BillNumber,b.id as BillId,22 as BillTypeId,b.CreatedOnUtc as MakeBillDate,b.SumAmount as Amount,b.PreferentialAmount as DiscountAmount,0 as PaymentedAmount,IFNULL(b.OweCash,0) as ArrearsAmount,0 DiscountAmountOnce,0 ReceivableAmountOnce,0 AmountOwedAfterReceipt,0 as CashReceiptBillId,b.Remark,null as CreatedOnUtc,b.storeid from dcms.PurchaseBills as b where b.StoreId = " + storeId + " and  b.ManufacturerId = " + manufacturerid + " and b.OweCash > 0 and b.AuditedStatus=1 and b.ReversedStatus=0) " +
                "UNION ALL " +
                "(select b.Id, b.BillNumber,b.id as BillId,24 as BillTypeId,b.CreatedOnUtc as MakeBillDate,b.SumAmount as Amount,b.PreferentialAmount as DiscountAmount,0 as PaymentedAmount,IFNULL(b.OweCash,0) as ArrearsAmount,0 DiscountAmountOnce,0 ReceivableAmountOnce,0 AmountOwedAfterReceipt,0 as CashReceiptBillId,b.Remark,null as CreatedOnUtc,b.storeid from dcms.PurchaseReturnBills as b where b.StoreId = " + storeId + " and  b.ManufacturerId = " + manufacturerid + " and b.OweCash > 0 and b.AuditedStatus=1 and b.ReversedStatus=0)";

                var query = PurchaseBillsRepository_RO.QueryFromSql<CashReceiptItemView>(queryString);
                return query.ToList();
            }
            catch (Exception)
            {
                return new List<CashReceiptItemView>();
            }
        }

        /// <summary>
        /// 获取收款单据收款项
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="billid"></param>
        /// <returns></returns>
        private CashReceiptItem GetCashReceiptItems(int storeId, int billid)
        {
            var item = CashReceiptItemsRepository.Table
                .Where(s => s.StoreId == storeId && s.BillId == billid && s.AmountOwedAfterReceipt == 0).FirstOrDefault();
            //欠款没有结完/或者没有收款
            if (item == null)
            {
                item = CashReceiptItemsRepository.Table
                  .Where(s => s.StoreId == storeId && s.BillId == billid && s.AmountOwedAfterReceipt != 0).OrderByDescending(s => s.CreatedOnUtc).FirstOrDefault();
                //没有收款
                if (item == null)
                {
                    //返回空
                    return null;
                }
                else
                {
                    //返回部分收款
                    return item;
                }
            }
            else
            {
                //返回欠款结完
                return item;
            }
        }

        /// <summary>
        /// 获取付款单据付款项
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="billid"></param>
        /// <returns></returns>
        private PaymentReceiptItem GetPaymentReceiptItems(int storeId, int billid)
        {
            var item = PaymentReceiptItemsRepository.Table
                .Where(s => s.StoreId == storeId && s.BillId == billid && s.AmountOwedAfterReceipt == 0).FirstOrDefault();
            //欠款没有结完/或者没有收款
            if (item == null)
            {
                item = PaymentReceiptItemsRepository.Table
                  .Where(s => s.StoreId == storeId && s.BillId == billid && s.AmountOwedAfterReceipt != 0).OrderByDescending(s => s.CreatedOnUtc).FirstOrDefault();
                //没有收款
                if (item == null)
                {
                    //返回空
                    return null;
                }
                else
                {
                    //返回部分收款
                    return item;
                }
            }
            else
            {
                //返回欠款结完
                return item;
            }
        }


        /// <summary>
        /// 计算经销商账户余额
        /// </summary>
        /// <returns></returns>
        public virtual TerminalBalance CalcTerminalBalance(int storeId, int terminalId)
        {
            var terminalBalance = new TerminalBalance();

            try
            {
                //最大欠款额度
                decimal maxAmount = TerminalsRepository.TableNoTracking
                    .Where(t => t.StoreId == storeId && t.Id == terminalId)
                    .Select(s => s.MaxAmountOwed).FirstOrDefault();

                //初始欠款
                //decimal beginAmount = ReceivablesRepository.TableNoTracking
                //    .Where(r => r.StoreId == storeId && r.TerminalId == terminalId)
                //    .Sum(c => c.OweCash);

                //剩余预收款金额(预收账款科目下的所有子类科目)：
                decimal advanceAmountBalance = GetTerminalBalance(storeId, terminalId, (int)AccountingCodeEnum.AdvanceReceipt);

                //总欠款
                decimal totalOweCash = 0;
                //获取所有销售和退货单的尚欠金额
                var bills = GetCashReceiptItemView(storeId, terminalId);
                //获取所有收款单
                bills.ForEach(b =>
                {
                    //获取当前单据的收款情况
                    var cr = GetCashReceiptItems(storeId, b.BillId);
                    //没有收款
                    if (cr == null)
                    {
                        //累计当前收款后尚欠金额
                        if (b.BillTypeId == (int)BillTypeEnum.ReturnBill)
                        {
                            totalOweCash += (-b.ArrearsAmount ?? 0);
                        }
                        else
                        {
                            totalOweCash += b.ArrearsAmount ?? 0;
                        }

                    }
                    //收完/或者部分收款
                    else
                    {
                        //累计收款后尚欠金额
                        totalOweCash += cr.AmountOwedAfterReceipt ?? 0;
                    }
                });

                //剩余欠款额度 = 最大额度 +-（总欠款合计）
                decimal oweCashBalance = maxAmount + totalOweCash;

                terminalBalance.MaxOweCashBalance = maxAmount;
                terminalBalance.AdvanceAmountBalance = advanceAmountBalance;
                terminalBalance.TotalOweCash = totalOweCash;
                terminalBalance.OweCashBalance = oweCashBalance;

                return terminalBalance;
            }
            catch (Exception)
            {
                return terminalBalance;
            }
        }


        /// <summary>
        /// 计算供应商账户余额
        /// </summary>
        /// <returns></returns>
        public virtual ManufacturerBalance CalcManufacturerBalance(int storeId, int manufacturerId)
        {
            var manufacturerBalance = new ManufacturerBalance();

            try
            {
                //剩余预付款金额(预付账款科目下的所有子类科目)：
                decimal advanceAmountBalance = GetManufacturerBalance(storeId, manufacturerId, (int)AccountingCodeEnum.AdvancePayment);

                //总欠款
                decimal totalOweCash = 0;
                //获取所有采购和退货单的尚欠金额
                var bills = GetPaymentReceiptItemView(storeId, manufacturerId);
                //获取所有收款单
                bills.ForEach(b =>
                {
                    //获取当前单据的付款情况
                    var cr = GetPaymentReceiptItems(storeId, b.BillId);
                    //没有收款
                    if (cr == null)
                    {
                        //累计当前付款后尚欠金额
                        if (b.BillTypeId == (int)BillTypeEnum.PurchaseReturnBill)
                        {
                            totalOweCash += (-b.ArrearsAmount ?? 0);
                        }
                        else
                        {
                            totalOweCash += b.ArrearsAmount ?? 0;
                        }
                    }
                    //付完/或者部分付款
                    else
                    {
                        //累计付款后尚欠金额
                        totalOweCash += cr.AmountOwedAfterReceipt ?? 0;
                    }
                });

                //剩余欠款额度 = 最大额度 +-（总欠款合计）
                decimal oweCashBalance = totalOweCash;

                manufacturerBalance.AdvanceAmountBalance = advanceAmountBalance;
                manufacturerBalance.TotalOweCash = totalOweCash;
                manufacturerBalance.OweCashBalance = oweCashBalance;

                return manufacturerBalance;
            }
            catch (Exception)
            {
                return manufacturerBalance;
            }
        }


        /// <summary>
        /// 获取供应商剩余预付款余额
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="manufacturerId"></param>
        /// <param name="accountCodeTypeId"></param>
        /// <returns></returns>
        public virtual decimal GetManufacturerBalance(int storeId, int manufacturerId, int accountCodeTypeId)
        {


            // 采购单（预付款科目）
            string saleSql = @"SELECT 
                                    IFNULL( SUM(sa.CollectionAmount),0) as Value
                                FROM
                                    PurchaseBill_Accounting_Mapping AS sa LEFT JOIN
                                    AccountingOptions ao ON sa.AccountingOptionId = ao.Id
                                    LEFT JOIN PurchaseBills pb ON sa.PurchaseBillId = pb.Id
                                WHERE sa.StoreId = " + storeId + " and sa.ManufacturerId = " + manufacturerId + " AND pb.AuditedStatus=1 AND pb.ReversedStatus=0  AND ao.AccountCodeTypeId IN (SELECT AccountCodeTypeId AS Value FROM (SELECT t1.AccountCodeTypeId,IF(FIND_IN_SET(ParentId, @pids) > 0, @pids:=CONCAT(@pids, ',', AccountCodeTypeId), 0) AS ischild  FROM (SELECT  AccountCodeTypeId, ParentId FROM dcms.AccountingOptions t WHERE StoreId = " + storeId + " AND ParentId = '" + accountCodeTypeId + "' ORDER BY ParentId , AccountCodeTypeId) t1, (SELECT @pids:='" + accountCodeTypeId + "') t2) t3)";

            //采购退货单（预付款科目）
            string returnSql = @"SELECT 
                                    IFNULL( SUM(sa.CollectionAmount),0) as Value
                                FROM
                                    PurchaseReturnBill_Accounting_Mapping AS sa LEFT JOIN
                                    AccountingOptions ao ON sa.AccountingOptionId = ao.Id
                                    LEFT JOIN PurchaseReturnBills pr ON sa.PurchaseReturnBillId = pr.Id
                                WHERE sa.StoreId = " + storeId + " and sa.ManufacturerId = " + manufacturerId + " AND pr.AuditedStatus=1 AND pr.ReversedStatus=0  AND ao.AccountCodeTypeId IN (SELECT AccountCodeTypeId AS Value FROM (SELECT t1.AccountCodeTypeId,IF(FIND_IN_SET(ParentId, @pids) > 0, @pids:=CONCAT(@pids, ',', AccountCodeTypeId), 0) AS ischild  FROM (SELECT  AccountCodeTypeId, ParentId FROM dcms.AccountingOptions t WHERE StoreId = " + storeId + " AND ParentId = '" + accountCodeTypeId + "' ORDER BY ParentId , AccountCodeTypeId) t1, (SELECT @pids:='" + accountCodeTypeId + "') t2) t3)";

            //预付款单（预付款科目）
            string advanceReceiptSql = @"SELECT 
                                    IFNULL( SUM(sa.CollectionAmount),0) as Value
                                FROM
                                    AdvancePaymentBill_Accounting_Mapping AS sa LEFT JOIN
                                    AccountingOptions ao ON sa.AccountingOptionId = ao.Id
                                    LEFT JOIN AdvancePaymentBills ap ON sa.AdvancePaymentBillId = ap.Id
                                WHERE sa.StoreId = " + storeId + " and sa.ManufacturerId = " + manufacturerId + " AND ap.AuditedStatus=1 AND ap.ReversedStatus=0  AND ao.AccountCodeTypeId IN (SELECT AccountCodeTypeId AS Value FROM (SELECT t1.AccountCodeTypeId,IF(FIND_IN_SET(ParentId, @pids) > 0, @pids:=CONCAT(@pids, ',', AccountCodeTypeId), 0) AS ischild  FROM (SELECT  AccountCodeTypeId, ParentId FROM dcms.AccountingOptions t WHERE StoreId = " + storeId + " AND ParentId = '" + accountCodeTypeId + "' ORDER BY ParentId , AccountCodeTypeId) t1, (SELECT @pids:='" + accountCodeTypeId + "') t2) t3)";


            //付款单（预付款科目）
            string cashReceiptSql = @"SELECT 
                                    IFNULL( SUM(sa.CollectionAmount),0) as Value
                                FROM
                                    PaymentReceiptBill_Accounting_Mapping AS sa LEFT JOIN
                                    AccountingOptions ao ON sa.AccountingOptionId = ao.Id
                                   LEFT JOIN PaymentReceiptBills pr ON sa.PaymentReceiptBillId = pr.Id
                                WHERE sa.StoreId = " + storeId + " and sa.ManufacturerId = " + manufacturerId + " AND pr.AuditedStatus=1 AND pr.ReversedStatus=0  AND ao.AccountCodeTypeId IN (SELECT AccountCodeTypeId AS Value FROM (SELECT t1.AccountCodeTypeId,IF(FIND_IN_SET(ParentId, @pids) > 0, @pids:=CONCAT(@pids, ',', AccountCodeTypeId), 0) AS ischild  FROM (SELECT  AccountCodeTypeId, ParentId FROM dcms.AccountingOptions t WHERE StoreId = " + storeId + " AND ParentId = '" + accountCodeTypeId + "' ORDER BY ParentId , AccountCodeTypeId) t1, (SELECT @pids:='" + accountCodeTypeId + "') t2) t3)";

            //采购预付
            decimal purchaseAmount = PurchaseBillsRepository_RO.QueryFromSql<DecimalQueryType>(saleSql).FirstOrDefault().Value ?? 0;
            //退货预付
            decimal returnAmount = PurchaseBillsRepository_RO.QueryFromSql<DecimalQueryType>(returnSql).FirstOrDefault().Value ?? 0;
            //预付款预付
            decimal advancePaymentAmount = PurchaseBillsRepository_RO.QueryFromSql<DecimalQueryType>(advanceReceiptSql).FirstOrDefault().Value ?? 0;
            //付款预付
            decimal paymentAmount = PurchaseBillsRepository_RO.QueryFromSql<DecimalQueryType>(cashReceiptSql).FirstOrDefault().Value ?? 0;

            // 预付款预付 + 退货预付 -  付款预付  - 采购预付
            return Convert.ToDecimal(Convert.ToDouble(returnAmount) + Convert.ToDouble(advancePaymentAmount) - Convert.ToDouble(paymentAmount) - Convert.ToDouble(purchaseAmount));
        }

        #endregion


        #region 开单员工所开单据欠款
        /// <summary>
        /// 开单员工所开单据欠款
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public virtual decimal GetUserUsedAmount(int storeId, int userId)
        {
            try
            {
                //销售单欠款
                var query1 = from a in SaleBillsRepository_RO.TableNoTracking
                             where a.StoreId == storeId
                                   && a.BusinessUserId == userId
                                   && a.AuditedStatus == true
                                   && a.ReversedStatus == false
                             select a.OweCash;
                decimal d1 = query1.Sum(a => a);

                //销售订单欠款
                var query2 = from a in SaleReservationBillsRepository_RO.TableNoTracking
                             where a.StoreId == storeId
                                   && a.BusinessUserId == userId
                                   && a.AuditedStatus == true
                                   && a.ReversedStatus == false
                             select a.OweCash;
                decimal d2 = query2.Sum(a => a);

                //销售订单转销售单的单据
                var query3 = from a in SaleReservationBillsRepository_RO.TableNoTracking
                             from e in SaleBillsRepository_RO.TableNoTracking
                             where a.StoreId == storeId
                                   && a.BusinessUserId == userId
                                   && a.AuditedStatus == true
                                   && a.ReversedStatus == false
                                   && a.Id == e.SaleReservationBillId
                             select a.OweCash;

                decimal d3 = query3.Sum(a => a);

                decimal totalamount = d1 + d2 - d3;

                return totalamount;

            }
            catch (Exception)
            {
                return 0;
            }
        }

        public virtual Tuple<decimal, decimal> GetUserAvailableOweCash(int storeId, int userId)
        {
            var useOweCash = GetUserUsedAmount(storeId, userId);
            var maxAmountOwed = _userService.GetUserMaxAmountOfArrears(storeId, userId);
            var result = maxAmountOwed - useOweCash;
            return new Tuple<decimal, decimal>(useOweCash, result < 0 ? 0 : result);
        }

        #endregion


        #region  


        /// <summary>
        /// 获取收款单据已经收款部分的本次优惠合计
        /// </summary>
        /// <returns></returns>
        public decimal GetBillDiscountAmountOnce(int storeId, int billId)
        {
            var query = from a in CashReceiptBillsRepository.Table
                        join b in CashReceiptItemsRepository.Table on a.Id equals b.CashReceiptBillId
                        where a.StoreId == storeId
                        && b.StoreId == storeId
                        && a.AuditedStatus == true
                        && a.ReversedStatus == false
                        && b.BillId == billId
                        select b.DiscountAmountOnce;

            var amountOnce = query.Sum(s => s ?? 0);

            return amountOnce;
        }
        /// <summary>
        /// 获取收款单据已经收款部分的本次收款合计
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="billId"></param>
        /// <returns></returns>
        public decimal GetBillReceivableAmountOnce(int storeId, int billId)
        {
            var query = from a in CashReceiptBillsRepository.Table
                        join b in CashReceiptItemsRepository.Table on a.Id equals b.CashReceiptBillId
                        where a.StoreId == storeId
                        && b.StoreId == storeId
                        && a.AuditedStatus == true
                        && a.ReversedStatus == false
                        && b.BillId == billId
                        select b.ReceivableAmountOnce;

            var amountOnce = query.Sum(s => s ?? 0);

            return amountOnce;
        }



        /// <summary>
        /// 获取付款单据已经付款部分的本次优惠合计
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="billId"></param>
        /// <returns></returns>
        public decimal GetPayBillDiscountAmountOnce(int storeId, int billId)
        {
            var query = from a in PaymentReceiptBillsRepository.Table
                        join b in PaymentReceiptItemsRepository.Table on a.Id equals b.PaymentReceiptBillId
                        where a.StoreId == storeId
                        && b.StoreId == storeId
                        && a.AuditedStatus == true
                        && a.ReversedStatus == false
                        && b.BillId == billId
                        select b.DiscountAmountOnce;

            var amountOnce = query.Sum(s => s ?? 0);

            return amountOnce;
        }
        /// <summary>
        /// 获取付款单据已经付款部分的本次收款合计
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="billId"></param>
        /// <returns></returns>
        public decimal GetPayBillReceivableAmountOnce(int storeId, int billId)
        {
            var query = from a in PaymentReceiptBillsRepository.Table
                        join b in PaymentReceiptItemsRepository.Table on a.Id equals b.PaymentReceiptBillId
                        where a.StoreId == storeId
                        && b.StoreId == storeId
                        && a.AuditedStatus == true
                        && a.ReversedStatus == false
                        && b.BillId == billId
                        select b.ReceivableAmountOnce;

            var amountOnce = query.Sum(s => s ?? 0);

            return amountOnce;
        }




        /// <summary>
        /// 获取 销售单 收款金额（收款账户）
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="billId"></param>
        /// <returns></returns>
        public decimal GetBillCollectionAmount(int storeId, int billId, BillTypeEnum billTypeEnum)
        {
            //销售单
            if (billTypeEnum == BillTypeEnum.SaleBill)
            {

                var amountOnce = SaleBillAccountingMappingRepository.Table
              .Where(s => s.StoreId == storeId && s.BillId == billId).Sum(s => s.CollectionAmount);
                return amountOnce;
            }
            //退货单
            else if (billTypeEnum == BillTypeEnum.ReturnBill)
            {
                var amountOnce = ReturnBillAccountingRepository.Table
                    .Where(s => s.StoreId == storeId && s.BillId == billId).Sum(s => s.CollectionAmount);
                return amountOnce;

            }
            //预收款单
            else if (billTypeEnum == BillTypeEnum.AdvanceReceiptBill)
            {
                //注意：要排除预收款
                var amountOnce = AdvanceReceiptBillAccountingMappingRepository.Table
                 .Where(s => s.StoreId == storeId && s.BillId == billId && s.Copy == false)
                 .Sum(s => s.CollectionAmount);
                return amountOnce;

            }
            //费用支出
            else if (billTypeEnum == BillTypeEnum.CostExpenditureBill)
            {

                var amountOnce = CostExpenditureBillAccountingMappingRepository.Table
          .Where(s => s.StoreId == storeId && s.BillId == billId).Sum(s => s.CollectionAmount);
                return amountOnce;
            }
            //其它收入
            else if (billTypeEnum == BillTypeEnum.FinancialIncomeBill)
            {
                var amountOnce = FinancialIncomeBillAccountingMappingRepository.Table
                               .Where(s => s.StoreId == storeId && s.BillId == billId).Sum(s => s.CollectionAmount);
                return amountOnce ;
            }
            else
            {
                return 0;
            }
        }



        /// <summary>
        /// 获取 采购单 付款金额（收款账户）
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="billId"></param>
        /// <returns></returns>
        public decimal GetPayBillCollectionAmount(int storeId, int billId, BillTypeEnum billTypeEnum)
        {
            //采购单
            if (billTypeEnum == BillTypeEnum.PurchaseBill)
            {

                var amountOnce = PurchaseBillAccountingMappingRepository.Table
              .Where(s => s.StoreId == storeId && s.BillId == billId).Sum(s => s.CollectionAmount);
                return amountOnce;
            }
            //采购退货单
            else if (billTypeEnum == BillTypeEnum.PurchaseReturnBill)
            {
                var amountOnce = PurchaseReturnBillAccountingMappingRepository.Table
                    .Where(s => s.StoreId == storeId && s.BillId == billId).Sum(s => s.CollectionAmount);
                return amountOnce;

            }
            //预付款单
            else if (billTypeEnum == BillTypeEnum.AdvancePaymentBill)
            {
                //注意：要排除预收款
                var amountOnce = AdvancePaymentBillAccountingMappingRepository.Table
                 .Where(s => s.StoreId == storeId && s.BillId == billId && s.Copy == false)
                 .Sum(s => s.CollectionAmount);
                return amountOnce ;

            }
            //其它收入
            else if (billTypeEnum == BillTypeEnum.FinancialIncomeBill)
            {
                var amountOnce = FinancialIncomeBillAccountingMappingRepository.Table
                               .Where(s => s.StoreId == storeId && s.BillId == billId).Sum(s => s.CollectionAmount);
                return amountOnce ;
            }
            else
            {
                return 0;
            }


        }

        #endregion

        public void RollBackPurchaseBillItems(int billId)
        {
            var uow = PurchaseItemsRepository.UnitOfWork;
            var items = PurchaseItemsRepository.Table.Where(s => s.PurchaseBillId == billId);
            if (items != null)
            {
                PurchaseItemsRepository.Delete(items);
            }

            uow.SaveChanges();
        }

        /// <summary>
        /// 回滚采购单据
        /// </summary>
        /// <param name="billId"></param>
        /// <returns></returns>
        public bool RollBackPurchaseBill(int billId)
        {

            if (billId > 0)
            {
                try
                {
                    RollBackPurchaseBillItems(billId);

                    var uow = PurchaseBillsRepository.UnitOfWork;
                    PurchaseBillsRepository.Delete(billId);
                    uow.SaveChanges();

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return true;
        }

        public void RollBackPurschaseReturnBillItems(int billId)
        {
            var uow = PurchaseReturnItemsRepository.UnitOfWork;
            var items = PurchaseReturnItemsRepository.Table.Where(s => s.PurchaseReturnBillId == billId);
            if (items != null)
            {
                PurchaseReturnItemsRepository.Delete(items);
            }

            uow.SaveChanges();
        }
        public bool RollBackPurschaseReturnBill(int billId)
        {
            if (billId > 0)
            {
                try
                {
                    RollBackPurschaseReturnBillItems(billId);

                    var uow = PurchaseReturnBillsRepository.UnitOfWork;
                    PurchaseReturnBillsRepository.Delete(billId);
                    uow.SaveChanges();

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 创建失败后回滚单据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="bill"></param>
        /// <returns></returns>
        public bool RollBackBill<T, T1>(T bill) where T : BaseBill<T1> where T1 : BaseEntity
        {
            try
            {
                var result = false;

                //销售单
                if (bill is SaleBill sb)
                {
                }
                //退货单
                else if (bill is ReturnBill rb)
                {
                }
                //采购单 
                else if (bill is PurchaseBill pb)
                {
                    if (pb != null)
                    {
                        result = RollBackPurchaseBill(pb.Id);
                    }
                }
                //采购退货单 
                else if (bill is PurchaseReturnBill prb)
                {
                    if (prb != null)
                    {
                        result = RollBackPurschaseReturnBill(prb.Id);
                    }
                }
                //收款单 
                else if (bill is CashReceiptBill crb)
                {
                }
                //预收款单 
                else if (bill is AdvanceReceiptBill arb)
                {
                }
                //付款单 
                else if (bill is PaymentReceiptBill prcb)
                {
                }
                //预付款单 
                else if (bill is AdvancePaymentBill apb)
                {
                }
                //其他收入 
                else if (bill is FinancialIncomeBill fib)
                {
                }
                //成本调价单 
                else if (bill is CostAdjustmentBill cab)
                {
                }
                //报损单 
                else if (bill is ScrapProductBill spb)
                {
                }
                //费用支出 
                else if (bill is CostExpenditureBill ceb)
                {
                }
                //盘点盈亏单
                else if (bill is InventoryProfitLossBill ipb)
                {
                }

                return result;

            }
            catch (Exception)
            {
                return false;
            }
        }






    }
}
