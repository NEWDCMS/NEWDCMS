using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Data;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Domain.Tasks;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using DCMS.Services.Tasks;
using DCMS.Services.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DCMS.Services.Sales
{
    /// <summary>
    /// 表示收款对账服务
    /// </summary>
    public class FinanceReceiveAccountBillService : BaseService, IFinanceReceiveAccountBillService
    {
        private readonly IUserService _userService;
        private readonly IQueuedMessageService _queuedMessageService;


        public FinanceReceiveAccountBillService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher,
            IUserService userService,
            IQueuedMessageService queuedMessageService
            ) : base(getter, cacheManager, eventPublisher)
        {
            _userService = userService;
            _queuedMessageService = queuedMessageService;
        }

        #region 收款对账单


        /// <summary>
        /// 获取对账单据汇总
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="businessUserId"></param>
        /// <param name="payeer"></param>
        /// <param name="accountingOptionId"></param>
        /// <param name="billNumber"></param>
        /// <returns></returns>
        public IPagedList<FinanceReceiveAccountView> GetFinanceReceiveAccounts(int storeId,
            DateTime? start,
            DateTime? end,
            int? businessUserId,
            int? payeer,
            int? accountingOptionId,
            string billNumber = "",
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {

            try
            {

                billNumber = CommonHelper.FilterSQLChar(billNumber);

                //17:预收款枚举

                #region //销售单 
                //sb.BusinessUserId AS UserId
                string sqlString = @"(SELECT sb.Id as BillId, sb.BillNumber,sb.CreatedOnUtc,12 as BillType,
                           sb.MakeUserId AS UserId,
                            sb.TerminalId as TerminalId,
                            tt.Name as TerminalName,
                            sb.HandInStatus,
                            0 AS PaidAmount,
                            0 AS EPaymentAmount,
                            0 SaleAmountSum,
                            sb.ReceivableAmount AS SaleAmount,
                            IFNULL((SELECT 
                                            IFNULL(SUM(sa.CollectionAmount), 0)
                                        FROM
                                            SaleBill_Accounting_Mapping AS sa
                                                LEFT JOIN
                                            AccountingOptions ao ON sa.AccountingOptionId = ao.Id
                                        WHERE ";

                sqlString += @" sa.StoreId = " + storeId + " AND ao.StoreId = " + storeId + "";

                sqlString += @" AND sa.SaleBillId = sb.Id
                                                AND ABS(sa.CollectionAmount) > 0
                                                AND ao.AccountCodeTypeId IN ((SELECT 
                                                    AccountCodeTypeId AS Value
                                                FROM
                                                    (SELECT 
                                                        t1.AccountCodeTypeId,
                                                            IF(FIND_IN_SET(ParentId, @pids) > 0, @pids:=CONCAT(@pids, ',', AccountCodeTypeId), 0) AS ischild
                                                    FROM
                                                        (SELECT 
                                                        AccountCodeTypeId, ParentId
                                                    FROM
                                                        dcms.AccountingOptions t
                                                    WHERE";

                sqlString += @" StoreId = " + storeId + " AND ParentId = 17";

                sqlString += @" ORDER BY ParentId , AccountCodeTypeId) t1, (SELECT @pids:='17') t2) t3))
                                        GROUP BY AccountCodeTypeId),
                                    0) AS SaleAdvanceReceiptAmount,
                            sb.OweCash AS SaleOweCashAmount,
                            0 ReturnAmountSum,
                            0 ReturnAmount,
                            0 ReturnAdvanceReceiptAmount,
                            0 ReturnOweCashAmount,
                            0 ReceiptCashOweCashAmountSum,
                            0 ReceiptCashReceivableAmount,
                            0 ReceiptCashAdvanceReceiptAmount,
                            0 AdvanceReceiptSum,
                            0 AdvanceReceiptAmount,
                            0 AdvanceReceiptOweCashAmount,
                            0 CostExpenditureSum,
                            0 CostExpenditureAmount,
                            0 CostExpenditureOweCashAmount,
                            ssa.AccountingOptions,
                            sb.PreferentialAmount,
                            ifnull(subs.SaleProductCount,0) as SaleProductCount,
                            ifnull(subs.GiftProductCount,0) as GiftProductCount,
                            0 ReturnProductCount
                        FROM
                            dcms.SaleBills AS sb
                                LEFT JOIN
                            (SELECT 
                                sa.SaleBillId,
                                    GROUP_CONCAT(concat(sa.AccountingOptionId,'|',ao.Name,'|',sa.CollectionAmount)) AS AccountingOptions
                            FROM
                                SaleBill_Accounting_Mapping AS sa
                            LEFT JOIN AccountingOptions AS ao ON sa.AccountingOptionId = ao.Id
                            WHERE ";

                sqlString += @" sa.StoreId = " + storeId + " AND ao.StoreId = " + storeId + " ";

                if (accountingOptionId.HasValue && accountingOptionId.Value > 0)
                {
                    sqlString += @" AND sa.AccountingOptionId = " + accountingOptionId + " ";
                }

                sqlString += @" GROUP BY sa.SaleBillId) AS ssa ON sb.id = ssa.SaleBillId ";

                //商品
                sqlString += @" LEFT JOIN (SELECT ANY_VALUE(sub.SaleBillId) as SaleBillId,IFNULL((CASE ANY_VALUE(sub.IsGifts) WHEN 0 THEN SUM(IFNULL(sub.Quantity,0)) END), 0) AS SaleProductCount, IFNULL((CASE ANY_VALUE(sub.IsGifts) WHEN 1 THEN SUM(IFNULL(sub.Quantity,0)) END), 0) AS GiftProductCount FROM SaleItems AS sub WHERE sub.StoreId = " + storeId + ") as subs  ON sb.id = subs.SaleBillId ";

                
                sqlString += @" LEFT JOIN dcms_crm.CRM_Terminals AS tt ON sb.TerminalId = tt.Id ";

                sqlString += @" WHERE ";
                sqlString += @" sb.StoreId = " + storeId + "   AND sb.AuditedStatus = 1  AND sb.ReversedStatus = 0  AND (sb.ReceiptStatus = 1  or sb.ReceiptStatus = 2) AND sb.HandInStatus = 0 ";

                if (start.HasValue)
                {
                    sqlString += @" AND sb.CreatedOnUtc >= '" + start.Value.ToString("yyyy-MM-dd 00:00:00") + "'";
                }

                if (end.HasValue)
                {
                    sqlString += @" AND sb.CreatedOnUtc <= '" + end.Value.ToString("yyyy-MM-dd 23:59:59") + "'";
                }

                if (businessUserId.HasValue && businessUserId.Value > 0)
                {
                    //sqlString += @" AND sb.BusinessUserId = " + businessUserId + "";
                    sqlString += @" AND sb.MakeUserId = " + businessUserId + "";
                }

                if (!string.IsNullOrEmpty(billNumber))
                {
                    sqlString += @" AND sb.BillNumber = '" + billNumber + "'";
                }

                sqlString += @" AND (IFNULL(sb.Remark, '') not like '%应收款销售单/应收款期初备注%')";

                #endregion 

                #region //退货单
                sqlString += @" ) UNION ALL (SELECT sb.Id as BillId, sb.BillNumber,sb.CreatedOnUtc,14 as BillType,
                            sb.BusinessUserId AS UserId,
                            sb.TerminalId as TerminalId,
                            tt.Name as TerminalName,
                            sb.HandInStatus,
                            0 AS PaidAmount,
                            0 AS EPaymentAmount,
                            0 SaleAmountSum,
                            0 AS SaleAmount,
                            0 AS SaleAdvanceReceiptAmount,
                            0 AS SaleOweCashAmount,
                            0 ReturnAmountSum,
                            sb.ReceivableAmount AS ReturnAmount,
                            IFNULL((SELECT 
                                            IFNULL(SUM(sa.CollectionAmount), 0)
                                        FROM
                                            ReturnBill_Accounting_Mapping AS sa
                                                LEFT JOIN
                                            AccountingOptions ao ON sa.AccountingOptionId = ao.Id
                                        WHERE";
                sqlString += @"  sa.StoreId = " + storeId + " AND ao.StoreId = " + storeId + " ";

                sqlString += @" AND sa.ReturnBillId = sb.Id
                                                AND ABS(sa.CollectionAmount) > 0
                                                AND ao.AccountCodeTypeId IN ((SELECT 
                                                    AccountCodeTypeId AS Value
                                                FROM
                                                    (SELECT 
                                                        t1.AccountCodeTypeId,
                                                            IF(FIND_IN_SET(ParentId, @pids) > 0, @pids:=CONCAT(@pids, ',', AccountCodeTypeId), 0) AS ischild
                                                    FROM
                                                        (SELECT 
                                                        AccountCodeTypeId, ParentId
                                                    FROM
                                                        dcms.AccountingOptions t
                                                    WHERE";
                sqlString += @" StoreId = " + storeId + " AND ParentId = 17";
                sqlString += @" ORDER BY ParentId , AccountCodeTypeId) t1, (SELECT @pids:='17') t2) t3))
                                        GROUP BY AccountCodeTypeId),
                                    0) AS ReturnAdvanceReceiptAmount,
                            sb.OweCash AS ReturnOweCashAmount,
                            0 ReceiptCashOweCashAmountSum,
                            0 ReceiptCashReceivableAmount,
                            0 ReceiptCashAdvanceReceiptAmount,
                            0 AdvanceReceiptSum,
                            0 AdvanceReceiptAmount,
                            0 AdvanceReceiptOweCashAmount,
                            0 CostExpenditureSum,
                            0 CostExpenditureAmount,
                            0 CostExpenditureOweCashAmount,
                            ssa.AccountingOptions,
                            sb.PreferentialAmount,
                            0 SaleProductCount,
	                        ifnull(subs.GiftProductCount,0) as GiftProductCount,
                            ifnull(subs.ReturnProductCount,0) as ReturnProductCount
                        FROM
                            dcms.ReturnBills AS sb
                                LEFT JOIN
                            (SELECT 
                                sa.ReturnBillId,
                                    GROUP_CONCAT(concat(sa.AccountingOptionId,'|',ao.Name,'|',sa.CollectionAmount)) AS AccountingOptions
                            FROM
                                ReturnBill_Accounting_Mapping AS sa
                            LEFT JOIN AccountingOptions AS ao ON sa.AccountingOptionId = ao.Id
                            WHERE";

                sqlString += @" sa.StoreId = " + storeId + " AND ao.StoreId = " + storeId + "";

                if (accountingOptionId.HasValue && accountingOptionId.Value > 0)
                {
                    sqlString += @" AND sa.AccountingOptionId = " + accountingOptionId + " ";
                }

                sqlString += @" GROUP BY sa.ReturnBillId) AS ssa ON sb.id = ssa.ReturnBillId ";

                //商品
                sqlString += @" LEFT JOIN (SELECT ANY_VALUE(sub.ReturnBillId) as ReturnBillId, IFNULL((CASE ANY_VALUE(sub.IsGifts) WHEN 0 THEN SUM(IFNULL(sub.Quantity,0)) END), 0) AS ReturnProductCount, IFNULL((CASE ANY_VALUE(sub.IsGifts) WHEN 1 THEN SUM(IFNULL(sub.Quantity,0)) END), 0) AS GiftProductCount FROM ReturnItems AS sub WHERE sub.StoreId = " + storeId + ") as subs  ON sb.id = subs.ReturnBillId ";

                sqlString += @" LEFT JOIN dcms_crm.CRM_Terminals AS tt ON sb.TerminalId = tt.Id ";

                sqlString += @" WHERE";

                sqlString += @" sb.StoreId = " + storeId + "   AND sb.AuditedStatus = 1 AND sb.ReversedStatus = 0  AND (sb.ReceiptStatus = 1  or sb.ReceiptStatus = 2) AND sb.HandInStatus = 0 ";

                if (start.HasValue)
                {
                    sqlString += @" AND sb.CreatedOnUtc >= '" + start.Value.ToString("yyyy-MM-dd 00:00:00") + "'";
                }

                if (end.HasValue)
                {
                    sqlString += @" AND sb.CreatedOnUtc <= '" + end.Value.ToString("yyyy-MM-dd 23:59:59") + "'";
                }

                if (businessUserId.HasValue && businessUserId.Value > 0)
                {
                    sqlString += @" AND sb.BusinessUserId = " + businessUserId + "";
                }

                if (!string.IsNullOrEmpty(billNumber))
                {
                    sqlString += @" AND sb.BillNumber = '" + billNumber + "'";
                }
                #endregion

                #region //收款单
                sqlString += @" ) UNION ALL (SELECT sb.Id as BillId, sb.BillNumber,sb.CreatedOnUtc,41 as BillType,
                            sb.Payeer AS UserId,
                            sb.CustomerId as TerminalId,
                            tt.Name as TerminalName,
                            sb.HandInStatus,
                            0 AS PaidAmount,
                            0 AS EPaymentAmount,
                            0 SaleAmountSum,
                            0 AS SaleAmount,
                            0 AS SaleAdvanceReceiptAmount,
                            0 AS SaleOweCashAmount,
                            0 ReturnAmountSum,
                            0 ReturnAmount,
                            0 ReturnAdvanceReceiptAmount,
                            0 ReturnOweCashAmount,
                            0 ReceiptCashOweCashAmountSum,
                            sb.ReceivableAmount AS ReceiptCashReceivableAmount,
                            IFNULL((SELECT 
                                            IFNULL(SUM(sa.CollectionAmount), 0)
                                        FROM
                                            CashReceiptBill_Accounting_Mapping AS sa
                                                LEFT JOIN
                                            AccountingOptions ao ON sa.AccountingOptionId = ao.Id
                                        WHERE";

                sqlString += @" sa.StoreId = " + storeId + " AND ao.StoreId = " + storeId + " ";

                sqlString += @" AND sa.CashReceiptBillId = sb.Id
                                                AND ABS(sa.CollectionAmount) > 0
                                                AND ao.AccountCodeTypeId IN ((SELECT 
                                                    AccountCodeTypeId AS Value
                                                FROM
                                                    (SELECT 
                                                        t1.AccountCodeTypeId,
                                                            IF(FIND_IN_SET(ParentId, @pids) > 0, @pids:=CONCAT(@pids, ',', AccountCodeTypeId), 0) AS ischild
                                                    FROM
                                                        (SELECT 
                                                        AccountCodeTypeId, ParentId
                                                    FROM
                                                        dcms.AccountingOptions t
                                                    WHERE";

                sqlString += @" StoreId = " + storeId + " AND ParentId = 17";

                sqlString += @" ORDER BY ParentId , AccountCodeTypeId) t1, (SELECT @pids:='17') t2) t3))
                                        GROUP BY AccountCodeTypeId),
                                    0) AS ReceiptCashAdvanceReceiptAmount,
                            0 AdvanceReceiptSum,
                            0 AdvanceReceiptAmount,
                            0 AdvanceReceiptOweCashAmount,
                            0 CostExpenditureSum,
                            0 CostExpenditureAmount,
                            0 CostExpenditureOweCashAmount,
                            ssa.AccountingOptions,
                            sb.PreferentialAmount,
                            0 SaleProductCount,
	                        0 GiftProductCount,
                            0 ReturnProductCount
                        FROM
                            dcms.CashReceiptBills AS sb
                                LEFT JOIN
                            (SELECT 
                                sa.CashReceiptBillId,
                                    GROUP_CONCAT(concat(sa.AccountingOptionId,'|',ao.Name,'|',sa.CollectionAmount)) AS AccountingOptions
                            FROM
                                CashReceiptBill_Accounting_Mapping AS sa
                            LEFT JOIN AccountingOptions AS ao ON sa.AccountingOptionId = ao.Id ";

                sqlString += @" WHERE sa.StoreId = " + storeId + " AND ao.StoreId = " + storeId + "";

                if (accountingOptionId.HasValue && accountingOptionId.Value > 0)
                {
                    sqlString += @" AND sa.AccountingOptionId = " + accountingOptionId + " ";
                }

                sqlString += @" GROUP BY sa.CashReceiptBillId) AS ssa ON sb.id = ssa.CashReceiptBillId ";

                sqlString += @" LEFT JOIN dcms_crm.CRM_Terminals AS tt ON sb.CustomerId = tt.Id ";

                sqlString += @" WHERE sb.StoreId = " + storeId + "  AND sb.AuditedStatus = 1 AND sb.ReversedStatus = 0  AND sb.HandInStatus = 0  ";

                if (start.HasValue)
                {
                    sqlString += @" AND sb.CreatedOnUtc >= '" + start.Value.ToString("yyyy-MM-dd 00:00:00") + "'";
                }

                if (end.HasValue)
                {
                    sqlString += @" AND sb.CreatedOnUtc <= '" + end.Value.ToString("yyyy-MM-dd 23:59:59") + "'";
                }

                if (payeer.HasValue && payeer.Value > 0)
                {
                    sqlString += @" AND sb.Payeer = " + payeer + "";
                }

                if (!string.IsNullOrEmpty(billNumber))
                {
                    sqlString += @" AND sb.BillNumber = '" + billNumber + "'";
                }
                if (businessUserId.HasValue && businessUserId.Value > 0)
                {
                    sqlString += @" AND sb.MakeUserId = " + businessUserId + "";
                }
                #endregion 

                #region //预收款单
                sqlString += @" ) UNION ALL (SELECT sb.Id as BillId, sb.BillNumber,sb.CreatedOnUtc,43 as BillType,
                            sb.Payeer AS UserId,
                            sb.CustomerId as TerminalId,
                            tt.Name as TerminalName,
                            sb.HandInStatus,
                            0 AS PaidAmount,
                            0 AS EPaymentAmount,
                            0 SaleAmountSum,
                            0 AS SaleAmount,
                            0 AS SaleAdvanceReceiptAmount,
                            0 AS SaleOweCashAmount,
                            0 ReturnAmountSum,
                            0 ReturnAmount,
                            0 ReturnAdvanceReceiptAmount,
                            0 ReturnOweCashAmount,
                            0 ReceiptCashOweCashAmountSum,
                            0 ReceiptCashReceivableAmount,
                            0 ReceiptCashAdvanceReceiptAmount,
                            0 AdvanceReceiptSum,
                            sb.AdvanceAmount AS AdvanceReceiptAmount,
                            sb.OweCash AS AdvanceReceiptOweCashAmount,
                            0 CostExpenditureSum,
                            0 CostExpenditureAmount,
                            0 CostExpenditureOweCashAmount,
                            ssa.AccountingOptions,
                            sb.DiscountAmount AS PreferentialAmount,
                            0 SaleProductCount,
	                        0 GiftProductCount,
                            0 ReturnProductCount
                        FROM
                            dcms.AdvanceReceiptBills AS sb
                                LEFT JOIN
                            (SELECT 
                                sa.AdvanceReceiptBillId,
                                    GROUP_CONCAT(concat(sa.AccountingOptionId,'|',ao.Name,'|',sa.CollectionAmount)) AS AccountingOptions
                            FROM
                                AdvanceReceiptBill_Accounting_Mapping AS sa
                            LEFT JOIN AccountingOptions AS ao ON sa.AccountingOptionId = ao.Id ";

                sqlString += @" WHERE sa.StoreId = " + storeId + " AND ao.StoreId = " + storeId + " ";

                if (accountingOptionId.HasValue && accountingOptionId.Value > 0)
                {
                    sqlString += @" AND sa.AccountingOptionId = " + accountingOptionId + " ";
                }

                sqlString += @" GROUP BY sa.AdvanceReceiptBillId) AS ssa ON sb.id = ssa.AdvanceReceiptBillId ";

                sqlString += @" LEFT JOIN dcms_crm.CRM_Terminals AS tt ON sb.CustomerId = tt.Id ";

                sqlString += @" WHERE sb.StoreId = " + storeId + "   AND sb.AuditedStatus = 1 AND sb.ReversedStatus = 0  AND (sb.ReceiptStatus = 1  or sb.ReceiptStatus = 2) AND sb.HandInStatus = 0  ";

                if (start.HasValue)
                {
                    sqlString += @" AND sb.CreatedOnUtc >= '" + start.Value.ToString("yyyy-MM-dd 00:00:00") + "'";
                }

                if (end.HasValue)
                {
                    sqlString += @" AND sb.CreatedOnUtc <= '" + end.Value.ToString("yyyy-MM-dd 23:59:59") + "'";
                }

                if (payeer.HasValue && payeer.Value > 0)
                {
                    sqlString += @" AND sb.Payeer = " + payeer + "";
                }

                if (!string.IsNullOrEmpty(billNumber))
                {
                    sqlString += @" AND sb.BillNumber = '" + billNumber + "'";
                }
                if (businessUserId.HasValue && businessUserId.Value > 0)
                {
                    sqlString += @" AND sb.MakeUserId = " + businessUserId + "";
                }
                #endregion 

                #region //费用支出单
                sqlString += @" ) UNION ALL (SELECT sb.Id as BillId, sb.BillNumber,sb.CreatedOnUtc,45 as BillType,
                            sb.EmployeeId AS UserId,
                            0 as TerminalId,
                            '' as TerminalName,
                            sb.HandInStatus,
                            0 AS PaidAmount,
                            0 AS EPaymentAmount,
                            0 SaleAmountSum,
                            0 AS SaleAmount,
                            0 AS SaleAdvanceReceiptAmount,
                            0 AS SaleOweCashAmount,
                            0 ReturnAmountSum,
                            0 ReturnAmount,
                            0 ReturnAdvanceReceiptAmount,
                            0 ReturnOweCashAmount,
                            0 ReceiptCashOweCashAmountSum,
                            0 ReceiptCashReceivableAmount,
                            0 ReceiptCashAdvanceReceiptAmount,
                            0 AdvanceReceiptSum,
                            0 AdvanceReceiptAmount,
                            0 AdvanceReceiptOweCashAmount,
                            0 CostExpenditureSum,
                            sb.SumAmount as  CostExpenditureAmount,
                            sb.OweCash as CostExpenditureOweCashAmount,
                            ssa.AccountingOptions,
                            sb.DiscountAmount,
                            0 SaleProductCount,
	                        0 GiftProductCount,
                            0 ReturnProductCount
                        FROM
                            dcms.CostExpenditureBills AS sb
                                LEFT JOIN
                            (SELECT 
                                sa.CostExpenditureBillId,
                                    GROUP_CONCAT(concat(sa.AccountingOptionId,'|',ao.Name,'|',sa.CollectionAmount)) AS AccountingOptions
                            FROM
                                CostExpenditureBill_Accounting_Mapping AS sa
                            LEFT JOIN AccountingOptions AS ao ON sa.AccountingOptionId = ao.Id ";

                sqlString += @" WHERE sa.StoreId = " + storeId + " AND ao.StoreId = " + storeId + " ";

                if (accountingOptionId.HasValue && accountingOptionId.Value > 0)
                {
                    sqlString += @" AND sa.AccountingOptionId = " + accountingOptionId + " ";
                }

                sqlString += @" GROUP BY sa.CostExpenditureBillId) AS ssa ON sb.id = ssa.CostExpenditureBillId ";

                //sqlString += @" LEFT JOIN dcms_crm.CRM_Terminals AS tt ON sb.TerminalId = tt.Id ";

                sqlString += @" WHERE sb.StoreId = " + storeId + " AND sb.AuditedStatus = 1 AND sb.ReversedStatus = 0  AND (sb.ReceiptStatus = 1  or sb.ReceiptStatus = 2) AND sb.HandInStatus = 0 ";

                if (start.HasValue)
                {
                    sqlString += @" AND sb.CreatedOnUtc >= '" + start.Value.ToString("yyyy-MM-dd 00:00:00") + "'";
                }

                if (end.HasValue)
                {
                    sqlString += @" AND sb.CreatedOnUtc <= '" + end.Value.ToString("yyyy-MM-dd 23:59:59") + "'";
                }

                if (businessUserId.HasValue && businessUserId.Value > 0)
                {
                    sqlString += @" AND sb.EmployeeId = " + businessUserId + "";
                }

                if (!string.IsNullOrEmpty(billNumber))
                {
                    sqlString += @" AND sb.BillNumber = '" + billNumber + "'";
                }
                if (businessUserId.HasValue && businessUserId.Value > 0)
                {
                    sqlString += @" AND sb.MakeUserId = " + businessUserId + "";
                }
                sqlString += @" )";

                #endregion 

                var query = SaleBillsRepository.QueryFromSql<FinanceReceiveAccountView>(sqlString);

                var plist = new PagedList<FinanceReceiveAccountView>(query.ToList(), pageIndex, pageSize);

                return plist;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        /// <summary>
        /// 获取已经上交对账单
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="businessUserId"></param>
        /// <param name="billTypeId"></param>
        /// <param name="billNumber"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public IPagedList<FinanceReceiveAccountBill> GetSubmittedBills(int storeId, DateTime? start, DateTime? end, int? businessUserId, int? billTypeId, string billNumber = "", int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;

            try
            {
                var query = FinanceReceiveAccountBillsRepository.Table.Where(s => s.StoreId == storeId);

                //开始时间
                if (start != null)
                {
                    query = query.Where(a => a.HandInTransactionDate >= start);
                }

                //结束时间
                if (end != null)
                {
                    query = query.Where(a => a.HandInTransactionDate <= end);
                }

                if (businessUserId.HasValue && businessUserId.Value > 0)
                {
                    query = query.Where(a => a.UserId == businessUserId);
                }

                if (billTypeId.HasValue && billTypeId.Value > 0)
                {
                    query = query.Where(a => a.BillTypeId == billTypeId);
                }

                if (!string.IsNullOrEmpty(billNumber))
                {
                    query = query.Where(a => a.BillNumber.Contains(billNumber));
                }

                query = query.OrderBy(s => s.Id).OrderByDescending(s => s.HandInTransactionDate);

                var plist = new PagedList<FinanceReceiveAccountBill>(query.ToList(), pageIndex, pageSize);

                return plist;
            }
            catch (Exception)
            {
                return null;
            }
        }


        /// <summary>
        /// 单据
        /// </summary>
        /// <param name="bill"></param>
        public void DeleteFinanceReceiveAccountBill(FinanceReceiveAccountBill bill)
        {
            if (bill == null)
            {
                throw new ArgumentNullException("bill");
            }

            var uow = FinanceReceiveAccountBillsRepository.UnitOfWork;
            FinanceReceiveAccountBillsRepository.Delete(bill);
            uow.SaveChanges();

            _eventPublisher.EntityDeleted(bill);

        }
        public void InsertFinanceReceiveAccountBill(FinanceReceiveAccountBill bill)
        {
            var uow = FinanceReceiveAccountBillsRepository.UnitOfWork;
            FinanceReceiveAccountBillsRepository.Insert(bill);
            uow.SaveChanges();
            _eventPublisher.EntityInserted(bill);
        }
        public void UpdateFinanceReceiveAccountBill(FinanceReceiveAccountBill bill)
        {
            if (bill == null)
            {
                throw new ArgumentNullException("bill");
            }
            var uow = FinanceReceiveAccountBillsRepository.UnitOfWork;
            FinanceReceiveAccountBillsRepository.Update(bill);
            uow.SaveChanges();
            _eventPublisher.EntityUpdated(bill);
        }


        /// <summary>
        /// 科目映射
        /// </summary>
        /// <param name="financeReceiveAccountBillAccounting"></param>
        public virtual void InsertFinanceReceiveAccountBillAccounting(FinanceReceiveAccountBillAccounting financeReceiveAccountBillAccounting)
        {
            if (financeReceiveAccountBillAccounting == null)
            {
                throw new ArgumentNullException("financeReceiveAccountBillAccounting");
            }

            var uow = FinanceReceiveAccountBillAccountingMappingRepository.UnitOfWork;
            FinanceReceiveAccountBillAccountingMappingRepository.Insert(financeReceiveAccountBillAccounting);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(financeReceiveAccountBillAccounting);
        }
        public virtual void InsertFinanceReceiveAccountBillAccountings(List<FinanceReceiveAccountBillAccounting> financeReceiveAccountBillAccountings)
        {
            if (financeReceiveAccountBillAccountings == null)
            {
                throw new ArgumentNullException("financeReceiveAccountBillAccountings");
            }

            var uow = FinanceReceiveAccountBillAccountingMappingRepository.UnitOfWork;
            FinanceReceiveAccountBillAccountingMappingRepository.Insert(financeReceiveAccountBillAccountings);
            uow.SaveChanges();
            //通知
            financeReceiveAccountBillAccountings.ForEach(faba =>
            {
                _eventPublisher.EntityInserted(faba);
            });
        }
        public virtual void UpdateFinanceReceiveAccountBillAccounting(FinanceReceiveAccountBillAccounting financeReceiveAccountBillAccounting)
        {
            if (financeReceiveAccountBillAccounting == null)
            {
                throw new ArgumentNullException("financeReceiveAccountBillAccounting");
            }

            var uow = FinanceReceiveAccountBillAccountingMappingRepository.UnitOfWork;
            FinanceReceiveAccountBillAccountingMappingRepository.Update(financeReceiveAccountBillAccounting);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(financeReceiveAccountBillAccounting);
        }
        public virtual void DeleteFinanceReceiveAccountBillAccounting(FinanceReceiveAccountBillAccounting financeReceiveAccountBillAccounting)
        {
            if (financeReceiveAccountBillAccounting == null)
            {
                throw new ArgumentNullException("financeReceiveAccountBillAccounting");
            }

            var uow = FinanceReceiveAccountBillAccountingMappingRepository.UnitOfWork;
            FinanceReceiveAccountBillAccountingMappingRepository.Delete(financeReceiveAccountBillAccounting);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(financeReceiveAccountBillAccounting);
        }


        /// <summary>
        /// 对账商品汇总
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="userId"></param>
        /// <param name="billType"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public IPagedList<RankProduct> GetRankProducts(int storeId, bool gift, int? userId, int billType, DateTime? start, DateTime? end, int pageIndex = 0, int pageSize = int.MaxValue)
        {

            var lists = new List<RankProduct>();

            DateTime.TryParse(start?.ToString("yyyy-MM-dd 00:00:00"), out DateTime startDate);
            DateTime.TryParse(end?.ToString("yyyy-MM-dd 23:59:59"), out DateTime endDate);

            //销售
            if (billType == 12)
            {
                var query = from sb in SaleBillsRepository.Table
                            join si in SaleItemsRepository.Table on sb.Id equals si.SaleBillId into t
                            from sbsi in t.DefaultIfEmpty()
                            join p in ProductsRepository.Table on sbsi.ProductId equals p.Id into t1
                            from sbsip in t1.DefaultIfEmpty()
                            join c in CategoriesRepository.Table on sbsip.CategoryId equals c.Id into t2
                            from sbsipc in t2.DefaultIfEmpty()
                            join op in SpecificationAttributeOptionsRepository.Table on sbsi.UnitId equals op.Id into t3
                            from sbsipop in t3.DefaultIfEmpty()
                            join sop in SpecificationAttributeOptionsRepository.Table on sbsip.SmallUnitId equals sop.Id into t4
                            from sbsipop1 in t4.DefaultIfEmpty()
                            join bop in SpecificationAttributeOptionsRepository.Table on sbsip.BigUnitId equals bop.Id into t5
                            from sbsipop2 in t5.DefaultIfEmpty()
                            where sb.StoreId == storeId
                            && sb.BusinessUserId == userId
                            && sb.AuditedStatus == true
                            && sb.ReversedStatus == false
                            select new RankProduct
                            {
                                ProductId = sbsi.ProductId,
                                Name = sbsip.Name,
                                ProductCode = sbsip.ProductCode,
                                CategoryName = sbsipc.Name,
                                BigQuantity = sbsip.BigQuantity ?? 0,
                                StrokeQuantity = sbsip.StrokeQuantity ?? 0,
                                UnitConversion = $"1{sbsipop1.Name}={sbsip.BigQuantity}{sbsipop2.Name}",
                                Quantity = (sbsi.UnitId == sbsip.BigUnitId) ? (sbsi.Quantity * sbsip.BigQuantity) : sbsi.Quantity,
                                UnitName = $"{sbsipop.Name}",
                                Amount = sbsi.Amount,
                                CreatedOnUtc = sbsi.CreatedOnUtc,
                                Gift = sbsi.IsGifts
                            };

                if (start.HasValue)
                {
                    query = query.Where(s => s.CreatedOnUtc >= startDate);
                }

                if (end.HasValue)
                {
                    query = query.Where(s => s.CreatedOnUtc <= endDate);
                }

                query = query.Where(s => s.Gift == gift);

                lists = query.ToList();
            }
            //退货
            else if (billType == 14)
            {

                var query = from sb in ReturnBillsRepository.Table
                            join si in ReturnItemsRepository.Table on sb.Id equals si.ReturnBillId into t
                            from sbsi in t.DefaultIfEmpty()
                            join p in ProductsRepository.Table on sbsi.ProductId equals p.Id into t1
                            from sbsip in t1.DefaultIfEmpty()
                            join c in CategoriesRepository.Table on sbsip.CategoryId equals c.Id into t2
                            from sbsipc in t2.DefaultIfEmpty()
                            join op in SpecificationAttributeOptionsRepository.Table on sbsi.UnitId equals op.Id into t3
                            from sbsipop in t3.DefaultIfEmpty()
                            join sop in SpecificationAttributeOptionsRepository.Table on sbsip.SmallUnitId equals sop.Id into t4
                            from sbsipop1 in t4.DefaultIfEmpty()
                            join bop in SpecificationAttributeOptionsRepository.Table on sbsip.BigUnitId equals bop.Id into t5
                            from sbsipop2 in t5.DefaultIfEmpty()
                            where sb.StoreId == storeId
                            && sb.BusinessUserId == userId
                            && sb.AuditedStatus == true
                            && sb.ReversedStatus == false
                            select new RankProduct
                            {
                                ProductId = sbsi.ProductId,
                                Name = sbsip.Name,
                                ProductCode = sbsip.ProductCode,
                                CategoryName = sbsipc.Name,
                                BigQuantity = sbsip.BigQuantity ?? 0,
                                StrokeQuantity = sbsip.StrokeQuantity ?? 0,
                                UnitConversion = $"1{sbsipop1.Name}={sbsip.BigQuantity}{sbsipop2.Name}",
                                Quantity = (sbsi.UnitId == sbsip.BigUnitId) ? (sbsi.Quantity * sbsip.BigQuantity) : sbsi.Quantity,
                                UnitName = $"{sbsipop.Name}",
                                Amount = sbsi.Amount,
                                CreatedOnUtc = sbsi.CreatedOnUtc,
                                Gift = false
                            };

                if (start.HasValue)
                {
                    query = query.Where(s => s.CreatedOnUtc >= startDate);
                }

                if (end.HasValue)
                {
                    query = query.Where(s => s.CreatedOnUtc <= endDate);
                }

                query = query.Where(s => s.Gift == gift);

                lists = query.ToList();

            }

            var plist = new PagedList<RankProduct>(lists, pageIndex, pageSize);

            return plist;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="gift"></param>
        /// <param name="userId"></param>
        /// <param name="billType"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="billIds"></param>
        /// <returns></returns>
        public IList<RankProduct> GetRankProducts(int storeId, bool gift, int? userId, int billType, DateTime? start, DateTime? end, int[] billIds)
        {

            var lists = new List<RankProduct>();

            DateTime.TryParse(start?.ToString("yyyy-MM-dd 00:00:00"), out DateTime startDate);
            DateTime.TryParse(end?.ToString("yyyy-MM-dd 23:59:59"), out DateTime endDate);

            //销售
            if (billType == 12)
            {
                var query = from sb in SaleBillsRepository.Table
                            join si in SaleItemsRepository.Table on sb.Id equals si.SaleBillId into t
                            from sbsi in t.DefaultIfEmpty()
                            join p in ProductsRepository.Table on sbsi.ProductId equals p.Id into t1
                            from sbsip in t1.DefaultIfEmpty()
                            join c in CategoriesRepository.Table on sbsip.CategoryId equals c.Id into t2
                            from sbsipc in t2.DefaultIfEmpty()
                            join op in SpecificationAttributeOptionsRepository.Table on sbsi.UnitId equals op.Id into t3
                            from sbsipop in t3.DefaultIfEmpty()
                            join sop in SpecificationAttributeOptionsRepository.Table on sbsip.SmallUnitId equals sop.Id into t4
                            from sbsipop1 in t4.DefaultIfEmpty()
                            join bop in SpecificationAttributeOptionsRepository.Table on sbsip.BigUnitId equals bop.Id into t5
                            from sbsipop2 in t5.DefaultIfEmpty()
                            where sb.StoreId == storeId
                            && sb.AuditedStatus == true
                            && sb.ReversedStatus == false
                            && billIds.Contains(sb.Id)
                            select new RankProduct
                            {
                                BusinessUserId = sb.BusinessUserId,
                                ProductId = sbsi.ProductId,
                                Name = sbsip.Name,
                                ProductCode = sbsip.ProductCode,
                                CategoryId = sbsipc.Id,
                                CategoryName = sbsipc.Name,
                                BigQuantity = sbsip.BigQuantity ?? 0,
                                StrokeQuantity = sbsip.StrokeQuantity ?? 0,
                                UnitConversion = $"1{sbsipop1.Name}={sbsip.BigQuantity}{sbsipop2.Name}",
                                Quantity = (sbsi.UnitId == sbsip.BigUnitId) ? (sbsi.Quantity * sbsip.BigQuantity) : sbsi.Quantity,
                                UnitName = $"{sbsipop.Name}",
                                Amount = sbsi.Amount,
                                CreatedOnUtc = sbsi.CreatedOnUtc,
                                Gift = sbsi.IsGifts
                            };

                if (userId.HasValue && userId.Value > 0)
                {
                    query = query.Where(s => s.BusinessUserId >= userId);
                }

                if (start.HasValue)
                {
                    query = query.Where(s => s.CreatedOnUtc >= startDate);
                }

                if (end.HasValue)
                {
                    query = query.Where(s => s.CreatedOnUtc <= endDate);
                }

                query = query.Where(s => s.Gift == gift);

                lists = query.ToList();
            }
            //退货
            else if (billType == 14)
            {

                var query = from sb in ReturnBillsRepository.Table
                            join si in ReturnItemsRepository.Table on sb.Id equals si.ReturnBillId into t
                            from sbsi in t.DefaultIfEmpty()
                            join p in ProductsRepository.Table on sbsi.ProductId equals p.Id into t1
                            from sbsip in t1.DefaultIfEmpty()
                            join c in CategoriesRepository.Table on sbsip.CategoryId equals c.Id into t2
                            from sbsipc in t2.DefaultIfEmpty()
                            join op in SpecificationAttributeOptionsRepository.Table on sbsi.UnitId equals op.Id into t3
                            from sbsipop in t3.DefaultIfEmpty()
                            join sop in SpecificationAttributeOptionsRepository.Table on sbsip.SmallUnitId equals sop.Id into t4
                            from sbsipop1 in t4.DefaultIfEmpty()
                            join bop in SpecificationAttributeOptionsRepository.Table on sbsip.BigUnitId equals bop.Id into t5
                            from sbsipop2 in t5.DefaultIfEmpty()
                            where sb.StoreId == storeId
                            && sb.AuditedStatus == true
                            && sb.ReversedStatus == false
                             && billIds.Contains(sb.Id)
                            select new RankProduct
                            {
                                BusinessUserId = sb.BusinessUserId,
                                ProductId = sbsi.ProductId,
                                Name = sbsip.Name,
                                ProductCode = sbsip.ProductCode,
                                CategoryId = sbsipc.Id,
                                CategoryName = sbsipc.Name,
                                BigQuantity = sbsip.BigQuantity ?? 0,
                                StrokeQuantity = sbsip.StrokeQuantity ?? 0,
                                UnitConversion = $"1{sbsipop1.Name}={sbsip.BigQuantity}{sbsipop2.Name}",
                                Quantity = (sbsi.UnitId == sbsip.BigUnitId) ? (sbsi.Quantity * sbsip.BigQuantity) : sbsi.Quantity,
                                UnitName = $"{sbsipop.Name}",
                                Amount = sbsi.Amount,
                                CreatedOnUtc = sbsi.CreatedOnUtc,
                                Gift = false
                            };

                if (userId.HasValue && userId.Value > 0)
                {
                    query = query.Where(s => s.BusinessUserId >= userId);
                }

                if (start.HasValue)
                {
                    query = query.Where(s => s.CreatedOnUtc >= startDate);
                }

                if (end.HasValue)
                {
                    query = query.Where(s => s.CreatedOnUtc <= endDate);
                }

                query = query.Where(s => s.Gift == gift);

                lists = query.ToList();
            }

            return lists;
        }

        public IList<FinanceReceiveAccountBillAccounting> GetFinanceReceiveAccountBillAccountings(int storeId, int billId)
        {
            if (storeId == 0 || billId == 0)
            {
                return null;
            }
            var lists = new List<FinanceReceiveAccountBillAccounting>();
            var query = from ba in FinanceReceiveAccountBillAccountingMappingRepository_RO.Table
                        where ba.StoreId == storeId
                        && ba.BillId == billId
                        select ba;
            lists = query.ToList();
            return lists;
        }

        public FinanceReceiveAccountBill GetFinanceReceiveAccountBillById(int? store, int financeReceiveAccountBillId, bool isInclude = false)
        {
            if (financeReceiveAccountBillId == 0)
            {
                return null;
            }

            if (isInclude)
            {
                var query = FinanceReceiveAccountBillsRepository_RO.Table
                .Include(sb => sb.FinanceReceiveAccountBillAccountings);

                return query.FirstOrDefault(s => s.Id == financeReceiveAccountBillId);
            }

            return FinanceReceiveAccountBillsRepository.GetById(financeReceiveAccountBillId);
        }


        /// <summary>
        /// 上交对账单
        /// </summary>
        /// <param name="storeId">经销商</param>
        /// <param name="userId">上交人</param>
        /// <param name="bill">对账单</param>
        /// <returns></returns>
        public BaseResult SubmitAccountStatement(int storeId, int userId, FinanceReceiveAccountBill bill)
        {
            var uow = FinanceReceiveAccountBillsRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {
                transaction = uow.BeginOrUseTransaction();

                if (bill == null)
                    throw new Exception("单据信息不存在，上交失败");

                bill.Id = 0;
                bill.StoreId = storeId;
                bill.MakeUserId = userId;
                bill.CreatedOnUtc = DateTime.Now;

                //设置状态
                bill.FinanceReceiveAccountStatus = FinanceReceiveAccountStatus.HandedIn;
                bill.HandInStatus = (int)FinanceReceiveAccountStatus.HandedIn;
                bill.HandInTransactionDate = DateTime.Now;

                //添加对账单
                InsertFinanceReceiveAccountBill(bill);

                if (bill.Id == 0)
                    throw new Exception("单据创建失败，上交失败");


                #region 对账单科目账户映射


                if (bill.Accounts.Any())
                {
                    var accounts = new List<FinanceReceiveAccountBillAccounting>();
                    bill.Accounts.ForEach(acc =>
                    {
                        accounts.Add(new FinanceReceiveAccountBillAccounting
                        {
                            StoreId = storeId,
                            BillId = acc.BillId,
                            AccountingOptionId = acc.AccountingOptionId,
                            CollectionAmount = acc.CollectionAmount
                        });
                    });
                    InsertFinanceReceiveAccountBillAccountings(accounts);
                }

                #endregion


                #region 发送通知 管理员

                try
                {
                    //制单人、管理员
                    var userNumbers = _userService.GetAllAdminUserMobileNumbersByStore(storeId).ToList();

                    decimal amount = 0;
                    switch (bill.BillTypeId)
                    {
                        case (int)BillTypeEnum.SaleBill:
                            amount = bill.SaleAmountSum;
                            break;
                        case (int)BillTypeEnum.ReturnBill:
                            amount = bill.ReturnAmountSum;
                            break;
                        case (int)BillTypeEnum.CashReceiptBill:
                            amount = bill.ReceiptCashOweCashAmountSum;
                            break;
                        case (int)BillTypeEnum.AdvanceReceiptBill:
                            amount = bill.AdvanceReceiptSum;
                            break;
                        case (int)BillTypeEnum.CostExpenditureBill:
                            amount = bill.CostExpenditureSum;
                            break;
                    }

                    QueuedMessage queuedMessage = new QueuedMessage()
                    {
                        StoreId = storeId,
                        MType = MTypeEnum.Paymented,
                        Title = CommonHelper.GetEnumDescription(MTypeEnum.Paymented),
                        Date = DateTime.Now,
                        BillType = BillTypeEnum.FinanceReceiveAccount,
                        BillNumber = bill.BillNumber,
                        BillNumbers = bill.BillNumber,
                        Amount = amount,
                        BillId = bill.Id,
                        CreatedOnUtc = DateTime.Now
                    };
                    _queuedMessageService.InsertQueuedMessage(userNumbers,queuedMessage);
                }
                catch (Exception ex)
                {
                    _queuedMessageService.WriteLogs(ex.Message);
                }

                #endregion

                transaction.Commit();
                return new BaseResult { Success = true, Message = "单据上交成功" };
            }
            catch (Exception)
            {
                transaction?.Rollback();
                return new BaseResult { Success = false, Message = "单据上交失败" };
            }
            finally
            {
                using (transaction) { }
            }
        }



        /// <summary>
        /// 批量上交对账单
        /// </summary>
        /// <param name="storeId">经销商</param>
        /// <param name="userId">上交人</param>
        /// <param name="bill">对账单</param>
        /// <returns></returns>
        public BaseResult BatchSubmitAccountStatements(int storeId, int userId, List<FinanceReceiveAccountBill> bills)
        {
            var uow = FinanceReceiveAccountBillsRepository.UnitOfWork;

            ITransaction transaction = null;
            try
            {
                transaction = uow.BeginOrUseTransaction();

                if (bills == null || !bills.Any())
                    throw new Exception("单据信息不存在，上交失败");

                bills.ForEach(bill => 
                {
                    try
                    {
                        bill.Id = 0;
                        bill.StoreId = storeId;
                        bill.MakeUserId = userId;
                        bill.CreatedOnUtc = DateTime.Now;

                        //设置状态
                        bill.FinanceReceiveAccountStatus = FinanceReceiveAccountStatus.HandedIn;
                        bill.HandInStatus = (int)FinanceReceiveAccountStatus.HandedIn;
                        bill.HandInTransactionDate = DateTime.Now;

                        //添加对账单
                        InsertFinanceReceiveAccountBill(bill);

                        if (bill.Id == 0)
                            throw new Exception("单据创建失败，上交失败");


                        #region 对账单科目账户映射

                        if (bill.Accounts.Any())
                        {
                            var accounts = new List<FinanceReceiveAccountBillAccounting>();
                            bill.Accounts.ForEach(acc =>
                            {
                                accounts.Add(new FinanceReceiveAccountBillAccounting
                                {
                                    StoreId = storeId,
                                    BillId = acc.BillId,
                                    FinanceReceiveAccountBill = bill,
                                    AccountingOptionId = acc.AccountingOptionId,
                                    CollectionAmount = acc.CollectionAmount
                                });
                            });

                            InsertFinanceReceiveAccountBillAccountings(accounts);
                        }

                        #endregion
                    }
                    catch (Exception ex) 
                    {
                        throw new Exception(ex.Message);
                    }
                });

                #region 发送通知 管理员


                bills.ForEach(bill =>
                {
                    try
                    {
                        //制单人、管理员
                        var userNumbers = _userService.GetAllAdminUserMobileNumbersByStore(storeId).ToList();

                        decimal amount = 0;
                        switch (bill.BillTypeId)
                        {
                            case (int)BillTypeEnum.SaleBill:
                                amount = bill.SaleAmountSum;
                                break;
                            case (int)BillTypeEnum.ReturnBill:
                                amount = bill.ReturnAmountSum;
                                break;
                            case (int)BillTypeEnum.CashReceiptBill:
                                amount = bill.ReceiptCashOweCashAmountSum;
                                break;
                            case (int)BillTypeEnum.AdvanceReceiptBill:
                                amount = bill.AdvanceReceiptSum;
                                break;
                            case (int)BillTypeEnum.CostExpenditureBill:
                                amount = bill.CostExpenditureSum;
                                break;
                        }

                        QueuedMessage queuedMessage = new QueuedMessage()
                        {
                            StoreId = storeId,
                            MType = MTypeEnum.Paymented,
                            Title = CommonHelper.GetEnumDescription(MTypeEnum.Paymented),
                            Date = DateTime.Now,
                            BillType = BillTypeEnum.FinanceReceiveAccount,
                            BillNumber = bill.BillNumber,
                            BillId = bill.Id,
                            CreatedOnUtc = DateTime.Now,
                            BillNumbers = bill.BillNumber,
                            Amount = amount
                        };
                        _queuedMessageService.InsertQueuedMessage(userNumbers,queuedMessage);
                    }
                    catch (Exception ex)
                    {
                        _queuedMessageService.WriteLogs(ex.Message);
                    }
                });

                #endregion

                transaction.Commit();
                return new BaseResult { Success = true, Message = "单据上交成功" };
            }
            catch (Exception)
            {
                transaction?.Rollback();
                return new BaseResult { Success = false, Message = "单据上交失败" };
            }
            finally
            {
                using (transaction) { }
            }
        }
        #endregion


    }
}
