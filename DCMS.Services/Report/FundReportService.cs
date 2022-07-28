using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Report;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Common;
using DCMS.Services.Events;
using DCMS.Services.Settings;
using DCMS.Services.Terminals;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DCMS.Services.Report
{
    /// <summary>
    /// 资金报表
    /// </summary>
    public class FundReportService : BaseService, IFundReportService
    {
        private readonly IAccountingService _accountingService;
        private readonly IDistrictService _districtService;
        private readonly ICommonBillService _commonBillService;

        public FundReportService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IAccountingService accountingService,
            IEventPublisher eventPublisher,
            IDistrictService districtService,
            ICommonBillService commonBillService
            ) : base(getter, cacheManager, eventPublisher)
        {
            _accountingService = accountingService;
            _districtService = districtService;
            _commonBillService = commonBillService;
        }

        /// <summary>
        /// 客户往来账
        /// </summary>
        /// <param name="storeId">经销商Id</param>
        /// <param name="districtId">客户片区Id</param>
        /// <param name="channelId">客户渠道Id</param>
        /// <param name="terminalId">客户Id</param>
        /// <param name="billNumber">单据编号</param>
        /// <param name="billTypeId">单据类型Id</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="remark">备注</param>
        /// <returns></returns>
        public IList<CustomerAccountDealings> GetFundReportCustomerAccount(int? storeId, int? districtId, int? channelId, int? terminalId, string terminalName, string billNumber, int? billTypeId, DateTime? startTime, DateTime? endTime, string remark)
        {

            try
            {
                return _cacheManager.Get(DCMSDefaults.FUND_GETFUND_REPORTCUSTOMER_ACCOUNT_KEY.FillCacheKey(storeId, districtId, channelId, terminalId, terminalName, billNumber,
                      billTypeId, startTime, endTime), () =>
                      {
                          terminalName = CommonHelper.Filter(terminalName);
                          billNumber = CommonHelper.Filter(billNumber);
                          remark = CommonHelper.Filter(remark);

                          var reporting = new List<CustomerAccountDealings>();

                          string whereQuery = $" a.StoreId = {storeId ?? 0} ";
                          string whereQuery1 = "";
                          if (!string.IsNullOrEmpty(billNumber))
                          {
                              whereQuery += $" and a.BillNumber = '{billNumber}' ";
                          }

                          if (!string.IsNullOrEmpty(remark))
                          {
                              whereQuery += $" and a.Remark like '%{remark}%' ";
                          }

                          if (terminalId.HasValue && terminalId.Value != 0)
                          {
                              whereQuery += $" and a.TerminalId = '{terminalId}' ";
                          }
                          if (terminalName != null)
                          {
                              whereQuery += $" and b.Name like '%{terminalName}%' ";
                          }

                          if (districtId.HasValue && districtId.Value != 0)
                          {
                              //递归片区查询
                              var distinctIds = _districtService.GetSubDistrictIds(storeId ?? 0, districtId ?? 0);
                              if (distinctIds != null && distinctIds.Count > 0)
                              {
                                  string inDistinctIds = string.Join("','", distinctIds);
                                  whereQuery += $" and b.DistrictId in ('{inDistinctIds}') ";
                              }
                              else
                              {
                                  whereQuery += $" and b.DistrictId = '{districtId}' ";
                              }
                          }

                          if (channelId.HasValue && channelId.Value != 0)
                          {
                              whereQuery += $" and b.ChannelId = '{channelId}' ";
                          }
                          if (billTypeId.HasValue && billTypeId.Value != 0)
                          {
                              whereQuery1 += $" where BillTypeId = '{billTypeId}' ";
                          }

                          whereQuery += $" and a.ReversedStatus = false ";

                          //列表中预收款列
                          //预收款会计科目(销售,退售,收款) 具有预收款科目，预收款的收款科目金额都为预收款
                          string[] advanceAccountings = null;
                          var accountingOption = _accountingService.GetAccountingOptionByCode(storeId, "200301");
                          if (accountingOption != null)
                          {
                              advanceAccountings = new string[] { accountingOption.Id.ToString() };
                          }
                          if (advanceAccountings.Length == 0)
                          {
                              advanceAccountings = new string[] { "0" };
                          }

                          #region MSSQL

                          //string sqlString = $"(select a.StoreId ,a.Id as BillId  ,a.BillNumber  ,a.TerminalId  ,b.DistrictId  ,b.ChannelId  ,b.Name as TerminalName  ,b.Code as TerminalCode  ,12 BillTypeId ,'销售' BillTypeName  ,a.TransactionDate  ,a.SumAmount  BillAmount  ,a.PreferentialAmount  , (select ISNULL( sum(c.CollectionAmount),0) from SaleBill_Accounting_Mapping as c where c.SaleBillId= a.Id ) as CashReceiptAmount ,0.00 ReceivableAmountSubtract  ,0.00 ReceivableAmountAdd  ,0.00 ReceivableAmountOverage ,(select ISNULL( sum(c.CollectionAmount),0) from SaleBill_Accounting_Mapping as c where c.SaleBillId= a.Id and c.AccountingOptionId in ({string.Join(",", advanceAccountings)})  ) as CurAdvanceAmount ,0.00 AdvancePaymentAmountSubtract ,0.00 AdvancePaymentAmountAdd  ,0.00 AdvancePaymentAmountOverage  ,0.00 SubscribeCashAmountOverage ,0.00 AccountAmountOverage  ,a.Remark from  SaleBills as a  inner join dcms_crm.CRM_Terminals as b on a.TerminalId=b.Id  where {whereQuery} ) UNION ALL (select a.StoreId ,a.Id as BillId  ,a.BillNumber  ,a.TerminalId  ,b.DistrictId  ,b.ChannelId  ,b.Name as TerminalName  ,b.Code as TerminalCode  ,14 BillTypeId ,'退售' BillTypeName  ,a.TransactionDate  ,a.SumAmount BillAmount, a.PreferentialAmount  ,(select ISNULL(sum(c.CollectionAmount), 0) from ReturnBill_Accounting_Mapping as c where c.ReturnBillId = a.Id ) as CashReceiptAmount,0.00 ReceivableAmountSubtract  ,0.00 ReceivableAmountAdd  ,0.00 ReceivableAmountOverage ,(select ISNULL(sum(c.CollectionAmount), 0) from ReturnBill_Accounting_Mapping as c where c.ReturnBillId = a.Id and c.AccountingOptionId in ({string.Join(",", advanceAccountings)})  ) as CurAdvanceAmount ,0.00 AdvancePaymentAmountSubtract ,0.00 AdvancePaymentAmountAdd  ,0.00 AdvancePaymentAmountOverage  ,0.00 SubscribeCashAmountOverage ,0.00 AccountAmountOverage  ,a.Remark from  ReturnBills as a inner join dcms_crm.CRM_Terminals as b on a.TerminalId = b.Id where {whereQuery} ) UNION ALL(select a.StoreId , a.Id as BillId  ,a.BillNumber  ,a.CustomerId as TerminalId  ,b.DistrictId  ,b.ChannelId  ,b.Name as TerminalName  ,b.Code as TerminalCode  ,41 BillTypeId ,'收款' BillTypeName  ,a.CreatedOnUtc as TransactionDate  ,0.00 BillAmount  ,a.TotalDiscountAmount as PreferentialAmount  ,(select ISNULL(sum(c.CollectionAmount), 0) from CashReceiptBill_Accounting_Mapping as c where c.CashReceiptBillId = a.Id ) as CashReceiptAmount ,0.00 ReceivableAmountSubtract  ,0.00 ReceivableAmountAdd  ,0.00 ReceivableAmountOverage ,(select ISNULL(sum(c.CollectionAmount), 0) from CashReceiptBill_Accounting_Mapping as c where c.CashReceiptBillId = a.Id and c.AccountingOptionId in ({string.Join(",", advanceAccountings)})  ) as CurAdvanceAmount,0.00 AdvancePaymentAmountSubtract ,0.00 AdvancePaymentAmountAdd  ,0.00 AdvancePaymentAmountOverage  ,0.00 SubscribeCashAmountOverage ,0.00 AccountAmountOverage  ,a.Remark from  CashReceiptBills as a inner join dcms_crm.CRM_Terminals as b on a.CustomerId = b.Id where {whereQuery.Replace("TerminalId", "CustomerId")}) UNION ALL(select a.StoreId , a.Id as BillId  ,a.BillNumber  ,a.CustomerId as TerminalId  ,b.DistrictId  ,b.ChannelId  ,b.Name as TerminalName  ,b.Code as TerminalCode  ,43 BillTypeId ,'预收款' BillTypeName  ,a.CreatedOnUtc as TransactionDate ,a.AdvanceAmount BillAmount, a.DiscountAmount as PreferentialAmount  ,(select ISNULL(sum(c.CollectionAmount), 0) from AdvanceReceiptBill_Accounting_Mapping as c where c.AdvanceReceiptBillId = a.Id ) as CashReceiptAmount,0.00 ReceivableAmountSubtract  ,0.00 ReceivableAmountAdd  ,0.00 ReceivableAmountOverage ,0.00 CurAdvanceAmount,0.00 AdvancePaymentAmountSubtract ,0.00 AdvancePaymentAmountAdd  ,0.00 AdvancePaymentAmountOverage  ,0.00 SubscribeCashAmountOverage ,0.00 AccountAmountOverage  ,a.Remark from  AdvanceReceiptBills as a inner join dcms_crm.CRM_Terminals as b on a.CustomerId = b.Id where {whereQuery.Replace("TerminalId", "CustomerId")})";

                          #endregion


                          #region MYSQL

                          //string sqlString = $"select *  from (select a.StoreId ,a.Id as BillId  ,a.BillNumber  ,a.TerminalId  ,b.DistrictId  ,b.ChannelId  ,b.Name as TerminalName  ,b.Code as TerminalCode  ,12 BillTypeId ,'销售' BillTypeName  ,a.TransactionDate  ,a.ReceivableAmount  BillAmount  ,a.PreferentialAmount  , (select IFNULL( sum(c.CollectionAmount),0) from SaleBill_Accounting_Mapping as c where c.SaleBillId= a.Id ) as CashReceiptAmount ,0.00 ReceivableAmountSubtract  ,0.00 ReceivableAmountAdd  ,0.00 ReceivableAmountOverage ,(select IFNULL( sum(c.CollectionAmount),0) from SaleBill_Accounting_Mapping as c where c.SaleBillId= a.Id and c.AccountingOptionId in ({string.Join(",", advanceAccountings)})  ) as CurAdvanceAmount ,0.00 AdvancePaymentAmountSubtract ,0.00 AdvancePaymentAmountAdd  ,0.00 AdvancePaymentAmountOverage  ,0.00 SubscribeCashAmountOverage ,0.00 AccountAmountOverage  ,a.Remark from  SaleBills as a  inner join dcms_crm.CRM_Terminals as b on a.TerminalId=b.Id  where {whereQuery}  UNION ALL select a.StoreId ,a.Id as BillId  ,a.BillNumber  ,a.TerminalId  ,b.DistrictId  ,b.ChannelId  ,b.Name as TerminalName  ,b.Code as TerminalCode  ,14 BillTypeId ,'退售' BillTypeName  ,a.TransactionDate  ,a.ReceivableAmount BillAmount, a.PreferentialAmount  ,(select IFNULL(sum(c.CollectionAmount), 0) from ReturnBill_Accounting_Mapping as c where c.ReturnBillId = a.Id ) as CashReceiptAmount,0.00 ReceivableAmountSubtract  ,0.00 ReceivableAmountAdd  ,0.00 ReceivableAmountOverage ,(select IFNULL(sum(c.CollectionAmount), 0) from ReturnBill_Accounting_Mapping as c where c.ReturnBillId = a.Id and c.AccountingOptionId in ({string.Join(",", advanceAccountings)})  ) as CurAdvanceAmount ,0.00 AdvancePaymentAmountSubtract ,0.00 AdvancePaymentAmountAdd  ,0.00 AdvancePaymentAmountOverage  ,0.00 SubscribeCashAmountOverage ,0.00 AccountAmountOverage  ,a.Remark from  ReturnBills as a inner join dcms_crm.CRM_Terminals as b on a.TerminalId = b.Id where {whereQuery}  UNION ALL select a.StoreId , a.Id as BillId  ,a.BillNumber  ,a.CustomerId as TerminalId  ,b.DistrictId  ,b.ChannelId  ,b.Name as TerminalName  ,b.Code as TerminalCode  ,41 BillTypeId ,'收款' BillTypeName  ,a.CreatedOnUtc as TransactionDate  ,0.00 BillAmount  ,a.PreferentialAmount as PreferentialAmount  ,(select IFNULL(sum(c.CollectionAmount), 0) from CashReceiptBill_Accounting_Mapping as c where c.CashReceiptBillId = a.Id ) as CashReceiptAmount ,0.00 ReceivableAmountSubtract  ,0.00 ReceivableAmountAdd  ,0.00 ReceivableAmountOverage ,(select IFNULL(sum(c.CollectionAmount), 0) from CashReceiptBill_Accounting_Mapping as c where c.CashReceiptBillId = a.Id and c.AccountingOptionId in ({string.Join(",", advanceAccountings)})  ) as CurAdvanceAmount,0.00 AdvancePaymentAmountSubtract ,0.00 AdvancePaymentAmountAdd  ,0.00 AdvancePaymentAmountOverage  ,0.00 SubscribeCashAmountOverage ,0.00 AccountAmountOverage  ,a.Remark from  CashReceiptBills as a inner join dcms_crm.CRM_Terminals as b on a.CustomerId = b.Id where {whereQuery.Replace("TerminalId", "CustomerId")} UNION ALL select a.StoreId , a.Id as BillId, a.BillNumber, a.CustomerId as TerminalId, b.DistrictId, b.ChannelId  ,b.Name as TerminalName, b.Code as TerminalCode, 43 BillTypeId ,'预收款' BillTypeName, a.CreatedOnUtc as TransactionDate ,a.AdvanceAmount BillAmount, a.DiscountAmount as PreferentialAmount  ,(select IFNULL(sum(c.CollectionAmount), 0) from AdvanceReceiptBill_Accounting_Mapping as c where c.AdvanceReceiptBillId = a.Id and Copy=0 ) as CashReceiptAmount,0.00 ReceivableAmountSubtract  ,0.00 ReceivableAmountAdd  ,0.00 ReceivableAmountOverage ,0.00 CurAdvanceAmount,0.00 AdvancePaymentAmountSubtract ,0.00 AdvancePaymentAmountAdd  ,0.00 AdvancePaymentAmountOverage  ,0.00 SubscribeCashAmountOverage ,0.00 AccountAmountOverage  ,a.Remark from  AdvanceReceiptBills as a inner join dcms_crm.CRM_Terminals as b on a.CustomerId = b.Id where {whereQuery.Replace("TerminalId", "CustomerId")}) as a  {whereQuery1}";
                          //string sqlString = $"select *  from (select a.StoreId ,a.Id as BillId  ,a.BillNumber  ,a.TerminalId  ,b.DistrictId  ,b.ChannelId  ,b.Name as TerminalName  ,b.Code as TerminalCode  ,12 BillTypeId ,'销售' BillTypeName  ,a.TransactionDate  ,a.SumAmount  BillAmount  ,a.PreferentialAmount  , (select IFNULL( sum(c.CollectionAmount),0) from SaleBill_Accounting_Mapping as c where c.SaleBillId= a.Id ) as CashReceiptAmount ,0.00 ReceivableAmountSubtract  ,0.00 ReceivableAmountAdd  ,0.00 ReceivableAmountOverage ,(select IFNULL( sum(c.CollectionAmount),0) from SaleBill_Accounting_Mapping as c where c.SaleBillId= a.Id and c.AccountingOptionId in ({string.Join(",", advanceAccountings)})  ) as CurAdvanceAmount ,0.00 AdvancePaymentAmountSubtract ,0.00 AdvancePaymentAmountAdd  ,0.00 AdvancePaymentAmountOverage  ,0.00 SubscribeCashAmountOverage ,0.00 AccountAmountOverage  ,a.Remark from  SaleBills as a  inner join dcms_crm.CRM_Terminals as b on a.TerminalId=b.Id  where {whereQuery}  UNION ALL select a.StoreId ,a.Id as BillId  ,a.BillNumber  ,a.TerminalId  ,b.DistrictId  ,b.ChannelId  ,b.Name as TerminalName  ,b.Code as TerminalCode  ,14 BillTypeId ,'退售' BillTypeName  ,a.TransactionDate  ,a.ReceivableAmount BillAmount, a.PreferentialAmount  ,(select IFNULL(sum(c.CollectionAmount), 0) from ReturnBill_Accounting_Mapping as c where c.ReturnBillId = a.Id ) as CashReceiptAmount,0.00 ReceivableAmountSubtract  ,0.00 ReceivableAmountAdd  ,0.00 ReceivableAmountOverage ,(select IFNULL(sum(c.CollectionAmount), 0) from ReturnBill_Accounting_Mapping as c where c.ReturnBillId = a.Id and c.AccountingOptionId in ({string.Join(",", advanceAccountings)})  ) as CurAdvanceAmount ,0.00 AdvancePaymentAmountSubtract ,0.00 AdvancePaymentAmountAdd  ,0.00 AdvancePaymentAmountOverage  ,0.00 SubscribeCashAmountOverage ,0.00 AccountAmountOverage  ,a.Remark from  ReturnBills as a inner join dcms_crm.CRM_Terminals as b on a.TerminalId = b.Id where {whereQuery}  UNION ALL select a.StoreId , a.Id as BillId  ,a.BillNumber  ,a.CustomerId as TerminalId  ,b.DistrictId  ,b.ChannelId  ,b.Name as TerminalName  ,b.Code as TerminalCode  ,41 BillTypeId ,'收款' BillTypeName  ,a.CreatedOnUtc as TransactionDate  ,0.00 BillAmount  ,a.PreferentialAmount as PreferentialAmount  ,(select IFNULL(sum(c.CollectionAmount), 0) from CashReceiptBill_Accounting_Mapping as c where c.CashReceiptBillId = a.Id ) as CashReceiptAmount ,0.00 ReceivableAmountSubtract  ,0.00 ReceivableAmountAdd  ,0.00 ReceivableAmountOverage ,(select IFNULL(sum(c.CollectionAmount), 0) from CashReceiptBill_Accounting_Mapping as c where c.CashReceiptBillId = a.Id and c.AccountingOptionId in ({string.Join(",", advanceAccountings)})  ) as CurAdvanceAmount,0.00 AdvancePaymentAmountSubtract ,0.00 AdvancePaymentAmountAdd  ,0.00 AdvancePaymentAmountOverage  ,0.00 SubscribeCashAmountOverage ,0.00 AccountAmountOverage  ,a.Remark from  CashReceiptBills as a inner join dcms_crm.CRM_Terminals as b on a.CustomerId = b.Id where {whereQuery.Replace("TerminalId", "CustomerId")} UNION ALL select a.StoreId , a.Id as BillId, a.BillNumber, a.CustomerId as TerminalId, b.DistrictId, b.ChannelId  ,b.Name as TerminalName, b.Code as TerminalCode, 43 BillTypeId ,'预收款' BillTypeName, a.CreatedOnUtc as TransactionDate ,a.AdvanceAmount BillAmount, a.DiscountAmount as PreferentialAmount  ,(select IFNULL(sum(c.CollectionAmount), 0) from AdvanceReceiptBill_Accounting_Mapping as c where c.AdvanceReceiptBillId = a.Id and Copy=0 ) as CashReceiptAmount,0.00 ReceivableAmountSubtract  ,0.00 ReceivableAmountAdd  ,0.00 ReceivableAmountOverage ,0.00 CurAdvanceAmount,0.00 AdvancePaymentAmountSubtract ,0.00 AdvancePaymentAmountAdd  ,0.00 AdvancePaymentAmountOverage  ,0.00 SubscribeCashAmountOverage ,0.00 AccountAmountOverage  ,a.Remark from  AdvanceReceiptBills as a inner join dcms_crm.CRM_Terminals as b on a.CustomerId = b.Id where {whereQuery.Replace("TerminalId", "CustomerId")}) as a  {whereQuery1}";
                          var sqlString = @"SELECT *
                                        FROM (
	                                        SELECT a.StoreId, a.Id AS BillId, a.BillNumber, a.TerminalId, b.DistrictId
		                                        , b.ChannelId, b.Name AS TerminalName, b.Code AS TerminalCode, 12 AS BillTypeId, '销售' AS BillTypeName
		                                        , a.TransactionDate, a.SumAmount AS BillAmount, a.PreferentialAmount
		                                        , IFNULL(c.CashReceiptAmount,0) AS CashReceiptAmount, 0.00 AS ReceivableAmountSubtract, 0.00 AS ReceivableAmountAdd, 0.00 AS ReceivableAmountOverage
		                                        , IFNULL(c.CurAdvanceAmount,0) AS CurAdvanceAmount, 0.00 AS AdvancePaymentAmountSubtract, 0.00 AS AdvancePaymentAmountAdd, 0.00 AS AdvancePaymentAmountOverage
		                                        , 0.00 AS SubscribeCashAmountOverage, 0.00 AS AccountAmountOverage, a.Remark
	                                        FROM SaleBills a
		                                        INNER JOIN dcms_crm.CRM_Terminals b ON a.TerminalId = b.Id
		                                        LEFT JOIN (SELECT SUM(CASE WHEN m.AccountingOptionId in({0}) THEN m.CollectionAmount ELSE 0 END) CurAdvanceAmount,SUM(m.CollectionAmount) CashReceiptAmount
		                                                   ,m.SaleBillId FROM SaleBill_Accounting_Mapping m GROUP BY m.SaleBillId) c on c.SaleBillId=a.Id
	                                          WHERE {1}
	                                        UNION ALL
	                                        SELECT a.StoreId, a.Id AS BillId, a.BillNumber, a.TerminalId, b.DistrictId
		                                        , b.ChannelId, b.Name AS TerminalName, b.Code AS TerminalCode, 14 AS BillTypeId, '退售' AS BillTypeName
		                                        , a.TransactionDate, a.ReceivableAmount AS BillAmount, a.PreferentialAmount
		                                        , IFNULL(c.CashReceiptAmount,0) AS CashReceiptAmount, 0.00 AS ReceivableAmountSubtract, 0.00 AS ReceivableAmountAdd, 0.00 AS ReceivableAmountOverage
		                                        , IFNULL(c.CurAdvanceAmount,0) AS CurAdvanceAmount, 0.00 AS AdvancePaymentAmountSubtract, 0.00 AS AdvancePaymentAmountAdd, 0.00 AS AdvancePaymentAmountOverage
		                                        , 0.00 AS SubscribeCashAmountOverage, 0.00 AS AccountAmountOverage, a.Remark
	                                        FROM ReturnBills a
		                                        INNER JOIN dcms_crm.CRM_Terminals b ON a.TerminalId = b.Id
		                                        LEFT JOIN (SELECT SUM(CASE WHEN m.AccountingOptionId in({0}) THEN m.CollectionAmount ELSE 0 END) CurAdvanceAmount,SUM(m.CollectionAmount) CashReceiptAmount
		                                        ,m.ReturnBillId FROM ReturnBill_Accounting_Mapping m GROUP BY m.ReturnBillId) c on c.ReturnBillId=a.Id
	                                        WHERE {1}
	                                        UNION ALL
	                                        SELECT a.StoreId, a.Id AS BillId, a.BillNumber, a.CustomerId AS TerminalId, b.DistrictId
		                                        , b.ChannelId, b.Name AS TerminalName, b.Code AS TerminalCode, 41 AS BillTypeId, '收款' AS BillTypeName
		                                        , a.CreatedOnUtc AS TransactionDate, 0.00 AS BillAmount, a.PreferentialAmount AS PreferentialAmount
		                                        , IFNULL(c.CashReceiptAmount,0) AS CashReceiptAmount, 0.00 AS ReceivableAmountSubtract, 0.00 AS ReceivableAmountAdd, 0.00 AS ReceivableAmountOverage
		                                        , IFNULL(c.CurAdvanceAmount,0) AS CurAdvanceAmount, 0.00 AS AdvancePaymentAmountSubtract, 0.00 AS AdvancePaymentAmountAdd, 0.00 AS AdvancePaymentAmountOverage
		                                        , 0.00 AS SubscribeCashAmountOverage, 0.00 AS AccountAmountOverage, a.Remark
	                                        FROM CashReceiptBills a
		                                        INNER JOIN dcms_crm.CRM_Terminals b ON a.CustomerId = b.Id
		                                        LEFT JOIN (SELECT SUM(CASE WHEN m.AccountingOptionId in({0}) THEN m.CollectionAmount ELSE 0 END) CurAdvanceAmount,SUM(m.CollectionAmount) CashReceiptAmount
		                                        ,m.CashReceiptBillId FROM CashReceiptBill_Accounting_Mapping m GROUP BY m.CashReceiptBillId) c on c.CashReceiptBillId=a.Id
	                                        WHERE {2}
	                                        UNION ALL
	                                        SELECT a.StoreId, a.Id AS BillId, a.BillNumber, a.CustomerId AS TerminalId, b.DistrictId
		                                        , b.ChannelId, b.Name AS TerminalName, b.Code AS TerminalCode, 43 AS BillTypeId, '预收款' AS BillTypeName
		                                        , a.CreatedOnUtc AS TransactionDate, a.AdvanceAmount AS BillAmount, a.DiscountAmount AS PreferentialAmount
		                                        , IFNULL(c.CashReceiptAmount,0) AS CashReceiptAmount, 0.00 AS ReceivableAmountSubtract, 0.00 AS ReceivableAmountAdd, 0.00 AS ReceivableAmountOverage, 0.00 AS CurAdvanceAmount
		                                        , 0.00 AS AdvancePaymentAmountSubtract, 0.00 AS AdvancePaymentAmountAdd, 0.00 AS AdvancePaymentAmountOverage, 0.00 AS SubscribeCashAmountOverage, 0.00 AS AccountAmountOverage
		                                        , a.Remark
	                                        FROM AdvanceReceiptBills a
		                                        INNER JOIN dcms_crm.CRM_Terminals b ON a.CustomerId = b.Id
		                                        LEFT JOIN (SELECT SUM(CASE WHEN m.AccountingOptionId in({0}) THEN m.CollectionAmount ELSE 0 END) CurAdvanceAmount,SUM(m.CollectionAmount) CashReceiptAmount
		                                        ,m.AdvanceReceiptBillId FROM AdvanceReceiptBill_Accounting_Mapping m WHERE m.Copy=0 GROUP BY m.AdvanceReceiptBillId) c on c.AdvanceReceiptBillId=a.Id
	                                        WHERE {2}
                                        ) a {3}";
                          #endregion
                          sqlString = string.Format(sqlString, string.Join(",", advanceAccountings), whereQuery, whereQuery.Replace("TerminalId", "CustomerId"), whereQuery1);
                          reporting = SaleBillsRepository_RO.QueryFromSql<CustomerAccountDealings>(sqlString).ToList();


                          reporting.ForEach(b =>
                          {
                              decimal? increase_s = 0, reduce_s = 0, sum_s = 0;
                              decimal? increase_r = 0, reduce_r = 0, sum_r = 0;
                              sum_s = (b.BillAmount ?? 0 - b.PreferentialAmount ?? 0 - b.CashReceiptAmount ?? 0);
                              sum_r = b.CurAdvanceAmount ?? 0;

                              if (sum_s > 0)
                              {
                                  increase_s = sum_s;
                              }

                              if (sum_s < 0)
                              {
                                  reduce_s = sum_s;
                              }

                              //应收款减  单据金额-优惠金额-收款金额 >0 增加 
                              b.ReceivableAmountSubtract = reduce_s;
                              //应收款加 单据金额-优惠金额-收款金额 < 0 减少
                              b.ReceivableAmountAdd = increase_s;

                              if (sum_r > 0)
                              {
                                  reduce_r = -sum_r;
                              }

                              if (sum_r < 0)
                              {
                                  increase_r = Math.Abs(sum_r ?? 0);
                              }

                              //预收款减  本次预收款>0 为减少
                              b.AdvancePaymentAmountSubtract = reduce_r;
                              //预收款加 本次预收款<0 为增加
                              b.AdvancePaymentAmountAdd = increase_r;

                          });

                          if (startTime.HasValue)
                          {
                              startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));
                          }

                          if (endTime.HasValue)
                          {
                              endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));
                          }

                          //期初应收款余额
                          var startASubtract = reporting.Where(s => s.TransactionDate < startTime).Select(s => s.ReceivableAmountSubtract).Sum();
                          var startAAdd = reporting.Where(s => s.TransactionDate < startTime).Select(s => s.ReceivableAmountAdd).Sum();
                          var startABlanance = startASubtract + startAAdd;

                          //期初预收款余额
                          var startPSubtract = reporting.Where(s => s.TransactionDate < startTime).Select(s => s.AdvancePaymentAmountSubtract).Sum();
                          var startPAdd = reporting.Where(s => s.TransactionDate < startTime).Select(s => s.AdvancePaymentAmountAdd).Sum();
                          var startPBlanance = startPSubtract + startPAdd;

                          if (startTime.HasValue)
                          {
                              reporting = reporting.Where(s => s.TransactionDate >= startTime).ToList();
                          }

                          if (endTime.HasValue)
                          {
                              reporting = reporting.Where(s => s.TransactionDate <= endTime).ToList();
                          }

                          decimal? lastA = startABlanance;
                          decimal? lastP = startPBlanance;

                          //计算余额
                          foreach (var b in reporting.OrderBy(s => s.TransactionDate).ToList())
                          {
                              if (b.BillTypeId == (int)BillTypeEnum.ReturnBill) 
                              {
                                  b.BillAmount = -b.BillAmount;
                                  b.PreferentialAmount = -b.PreferentialAmount;
                                  b.CashReceiptAmount = -b.CashReceiptAmount;
                              }
                              if (b.BillTypeId == (int)BillTypeEnum.AdvanceReceiptBill)
                              {
                                  b.AdvancePaymentAmountAdd += b.BillAmount;
                              }
                              var receivableAmount = b.BillAmount - b.CashReceiptAmount - b.PreferentialAmount;
                              //如果是收款单并且使用预收款
                              if (b.BillTypeId == (int)BillTypeEnum.CashReceiptBill && b.AdvancePaymentAmountSubtract != 0) 
                              {
                                  receivableAmount = 0;
                              }
                              lastA += receivableAmount;
                              if (receivableAmount < 0)
                              {
                                  b.ReceivableAmountSubtract = receivableAmount;
                              }
                              else if (receivableAmount > 0)
                              {
                                  b.ReceivableAmountAdd = receivableAmount;
                              }
                              else
                              {
                                  b.ReceivableAmountSubtract = 0;
                                  b.ReceivableAmountAdd = 0;
                              }
                              ////使用预收款 则应收减为0
                              //if (b.AdvancePaymentAmountSubtract != 0 && b.ReceivableAmountSubtract != 0)
                              //{
                              //    b.ReceivableAmountSubtract = 0;
                              //}
                              //if (b.BillTypeId == (int)BillTypeEnum.ReturnBill)
                              //{
                              //    b.BillAmount = -b.BillAmount;
                              //    b.PreferentialAmount = -b.PreferentialAmount;
                              //    b.CashReceiptAmount = -b.CashReceiptAmount;
                              //    b.ReceivableAmountSubtract = -b.ReceivableAmountSubtract;
                              //    b.ReceivableAmountAdd = -b.ReceivableAmountAdd;
                              //    b.ReceivableAmountOverage = -b.ReceivableAmountOverage;
                              //    b.AdvancePaymentAmountSubtract = -b.AdvancePaymentAmountSubtract;
                              //    b.AdvancePaymentAmountAdd = -b.AdvancePaymentAmountAdd;
                              //    b.AdvancePaymentAmountOverage = -b.AdvancePaymentAmountOverage;
                              //    b.SubscribeCashAmountOverage = -b.SubscribeCashAmountOverage;
                              //    b.AccountAmountOverage = -b.AccountAmountOverage;
                              //}
                              //if (b.BillTypeId == (int)BillTypeEnum.AdvanceReceiptBill) 
                              //{
                              //    b.AdvancePaymentAmountAdd = b.BillAmount;
                              //}
                              //if (b.BillTypeId == (int)BillTypeEnum.CashReceiptBill || b.BillTypeId == (int)BillTypeEnum.SaleBill)
                              //{
                              //    lastA -= b.CashReceiptAmount;
                              //}

                              //if (b.BillTypeId != (int)BillTypeEnum.AdvanceReceiptBill) 
                              //{
                              //    lastA += (b.ReceivableAmountSubtract + b.ReceivableAmountAdd);
                              //}
                              //if (b.BillTypeName == "退货")
                              //{
                              //    lastA -= (b.ReceivableAmountSubtract + b.ReceivableAmountAdd);
                              //}
                              lastP += (b.AdvancePaymentAmountSubtract + b.AdvancePaymentAmountAdd);

                              //应收款余额
                              b.ReceivableAmountOverage = lastA;
                              //预收款余额
                              b.AdvancePaymentAmountOverage = lastP;
                              //往来账余额
                              b.AccountAmountOverage = lastA - lastP;

                              b.ReceivableAmountSubtract = Math.Abs(b.ReceivableAmountSubtract??0);
                              b.AdvancePaymentAmountSubtract = Math.Abs(b.AdvancePaymentAmountSubtract??0);
                          }

                          return reporting.OrderBy(s => s.TransactionDate).ToList();
                      });
            }
            catch (Exception ex)
            {
                return new List<CustomerAccountDealings>();
            }
        }

        /// <summary>
        /// 客户应收款
        /// </summary>
        /// <param name="storeId">经销商Id</param>
        /// <param name="channelId">客户渠道Id</param>
        /// <param name="bussinessUserId">业务员Id</param>
        /// <param name="districtId">客户片区Id</param>
        /// <param name="terminalId">客户Id</param>
        /// <param name="moreDay">账期大于...天</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <param name="remark">整单备注</param>
        /// <returns></returns>
        public IList<FundReportCustomerReceiptCash> GetFundReportCustomerReceiptCash(int? storeId, int? channelId, int? bussinessUserId, int? districtId, int? terminalId, string terminalName, int? moreDay, DateTime? startTime, DateTime? endTime, string remark)
        {
            try
            {
                terminalName = CommonHelper.Filter(terminalName);
                remark = CommonHelper.Filter(remark);

                var reporting = new List<FundReportCustomerReceiptCash>();

                string districtWhere = " and 1 = 1 ";
                if (districtId.HasValue && districtId.Value != 0)
                {
                    //递归片区查询
                    var distinctIds = _districtService.GetChildDistrict(storeId ?? 0, districtId ?? 0).Select(s => s.Id).ToList();
                    if (distinctIds != null && distinctIds.Count() > 0)
                    {
                        distinctIds.Add(districtId ?? 0);
                        string inDistinctIds = string.Join("','", distinctIds);
                        districtWhere += $" and t.DistrictId in ('{inDistinctIds}') ";
                    }
                    else
                    {
                        districtWhere += $" and t.DistrictId = '{districtId}' ";
                    }
                }

                //ReceivableAmount 修改为SumAmount
                var queryString = @"(SELECT DATEDIFF(CURRENT_DATE, sb.CreatedOnUtc) AS sDay,sb.StoreId,
                                sb.Id AS BillId,
                                sb.BillNumber AS BillNumber,
                                12 AS BillTypeId,'销售单' as BillTypeName,
                                sb.TerminalId AS TerminalId,
                                t.Code AS TerminalCode,
                                t.Name AS TerminalName,
                                t.ChannelId,
                                sb.BusinessUserId,

                                sb.OweCash AS OweCase, 
                                sb.ReceivableAmount AS SaleAmount,
                                0.00 ReturnAmount,
                                0.00 NetAmount,
                                sb.CreatedOnUtc, 
                                NOW() AS FirstOweCaseDate, 
                                NOW() AS LastOweCaseDate,

                                sb.SumAmount AS Amount,
                                sb.PreferentialAmount AS DiscountAmount,
                                0 AS PaymentedAmount
                            FROM
                                dcms.SaleBills AS sb
                                inner join dcms_crm.CRM_Terminals AS t on sb.TerminalId=t.Id
                            WHERE
                                sb.StoreId = " + storeId + "  AND  sb.auditedStatus = 1 AND sb.ReversedStatus = 0 AND abs(sb.OweCash) > 0 AND (sb.ReceiptStatus = 0 or sb.ReceiptStatus = 1)";

                queryString += districtWhere;

                if (bussinessUserId.HasValue && bussinessUserId.Value != 0)
                    queryString += $" and sb.BusinessUserId = '{bussinessUserId}' ";

                if (terminalId.HasValue && terminalId.Value != 0)
                    queryString += $" and sb.TerminalId = '{terminalId}' ";

                if (channelId.HasValue && channelId.Value != 0)
                    queryString += $" and t.ChannelId = '{channelId}' ";

                if (terminalName != null)
                    queryString += $" and t.Name like '%{terminalName}%' ";

                if (startTime.HasValue)
                {
                    queryString += @" AND sb.CreatedOnUtc >= '" + startTime.Value.ToString("yyyy-MM-dd 00:00:00") + "'";
                }

                if (endTime.HasValue)
                {
                    queryString += @" AND sb.CreatedOnUtc <= '" + endTime.Value.ToString("yyyy-MM-dd 23:59:59") + "'";
                }

                if (moreDay.HasValue && moreDay.Value != 0)
                {
                    queryString += $" and  DATEDIFF(current_date,sb.CreatedOnUtc) > {moreDay} ";
                }

                queryString += @" ) UNION ALL (SELECT DATEDIFF(CURRENT_DATE, sb.CreatedOnUtc) AS sDay,sb.StoreId,
                                sb.Id AS BillId,
                                sb.BillNumber AS BillNumber,
                                14 AS BillTypeId,'退货单' as BillTypeName,
                                sb.TerminalId AS TerminalId,
                                t.Code AS TerminalCode,
                                t.Name AS TerminalName,
                                t.ChannelId,
                                sb.BusinessUserId,

                                sb.OweCash AS OweCase, 
                                0.00 AS SaleAmount,
                                sb.ReceivableAmount As ReturnAmount,
                                0.00 NetAmount,
                                sb.CreatedOnUtc, 
                                NOW() AS FirstOweCaseDate, 
                                NOW() AS LastOweCaseDate,

                                sb.ReceivableAmount AS Amount,
                                sb.PreferentialAmount AS DiscountAmount,
                                0 AS PaymentedAmount
                            FROM
                                dcms.ReturnBills AS sb
                                inner join dcms_crm.CRM_Terminals AS t on sb.TerminalId=t.Id
                            WHERE
                                sb.StoreId = " + storeId + "  AND   sb.auditedStatus = 1 AND sb.ReversedStatus = 0 AND abs(sb.OweCash) > 0 AND (sb.ReceiptStatus = 0 or sb.ReceiptStatus = 1)";

                queryString += districtWhere;

                if (bussinessUserId.HasValue && bussinessUserId.Value != 0)
                    queryString += $" and sb.BusinessUserId = '{bussinessUserId}' ";

                if (terminalId.HasValue && terminalId.Value != 0)
                    queryString += $" and sb.TerminalId = '{terminalId}' ";

                if (channelId.HasValue && channelId.Value != 0)
                    queryString += $" and t.ChannelId = '{channelId}' ";

                if (terminalName != null)
                    queryString += $" and t.Name like '%{terminalName}%' ";

                if (startTime.HasValue)
                {
                    queryString += @" AND sb.CreatedOnUtc >= '" + startTime.Value.ToString("yyyy-MM-dd 00:00:00") + "'";
                }

                if (endTime.HasValue)
                {
                    queryString += @" AND sb.CreatedOnUtc <= '" + endTime.Value.ToString("yyyy-MM-dd 23:59:59") + "'";
                }

                if (moreDay.HasValue && moreDay.Value != 0)
                {
                    queryString += $" and  DATEDIFF(current_date,sb.CreatedOnUtc) > {moreDay} ";
                }

                queryString += @") UNION ALL (SELECT  DATEDIFF(CURRENT_DATE, sb.CreatedOnUtc) AS sDay,sb.StoreId,
                                sb.Id AS BillId,
                                sb.BillNumber AS BillNumber,
                                43 AS BillTypeId,'预收款单' as BillTypeName,
                                sb.CustomerId AS TerminalId,
                                t.Name AS TerminalName,
                                t.Code AS TerminalCode,
                                t.ChannelId,
                                sb.Payeer AS BusinessUserId,

                                sb.OweCash AS OweCase, 
                                0.00 AS SaleAmount,
                                0.00 ReturnAmount,
                                0.00 NetAmount,
                                sb.CreatedOnUtc, 
                                NOW() AS FirstOweCaseDate, 
                                NOW() AS LastOweCaseDate,

                                sb.AdvanceAmount AS Amount,
                                sb.DiscountAmount AS DiscountAmount,
                                0 AS PaymentedAmount
                            FROM
                                dcms.AdvanceReceiptBills AS sb
                                inner join dcms_crm.CRM_Terminals AS t on sb.CustomerId=t.Id
                            WHERE
                                 sb.StoreId = " + storeId + "  AND   sb.auditedStatus = 1 AND sb.ReversedStatus = 0 AND abs(sb.OweCash) > 0 AND (sb.ReceiptStatus = 0 or sb.ReceiptStatus = 1)";

                queryString += districtWhere;

                if (bussinessUserId.HasValue && bussinessUserId.Value != 0)
                    queryString += $" and sb.Payeer = '{bussinessUserId}' ";

                if (terminalId.HasValue && terminalId.Value != 0)
                    queryString += $" and sb.CustomerId = '{terminalId}' ";

                if (channelId.HasValue && channelId.Value != 0)
                    queryString += $" and t.ChannelId = '{channelId}' ";

                if (terminalName != null)
                    queryString += $" and t.Name like '%{terminalName}%' ";

                if (startTime.HasValue)
                {
                    queryString += @" AND sb.CreatedOnUtc >= '" + startTime.Value.ToString("yyyy-MM-dd 00:00:00") + "'";
                }

                if (endTime.HasValue)
                {
                    queryString += @" AND sb.CreatedOnUtc <= '" + endTime.Value.ToString("yyyy-MM-dd 23:59:59") + "'";
                }

                if (moreDay.HasValue && moreDay.Value != 0)
                {
                    queryString += $" and  DATEDIFF(current_date,sb.CreatedOnUtc) > {moreDay} ";
                }

                queryString += @" ) UNION ALL (SELECT DATEDIFF(CURRENT_DATE, sb.CreatedOnUtc) AS sDay,sb.StoreId,
                                sb.Id AS BillId,
                                sb.BillNumber AS BillNumber,
                                45 AS BillTypeId,'费用支出' as BillTypeName,
                                sb.TerminalId AS TerminalId,
                                t.Code AS TerminalCode,
                                t.Name AS TerminalName,
                                t.ChannelId,
                                sb.EmployeeId AS BusinessUserId,

                                sb.OweCash AS OweCase, 
                                0.00 AS SaleAmount,
                                0.00 ReturnAmount,
                                0.00 NetAmount,
                                sb.CreatedOnUtc, 
                                NOW() AS FirstOweCaseDate, 
                                NOW() AS LastOweCaseDate,

                                sb.SumAmount AS Amount,
                                sb.DiscountAmount AS DiscountAmount,
                                0 AS PaymentedAmount
                            FROM
                                dcms.CostExpenditureBills AS sb
                                inner join dcms.CostExpenditureItems cs on sb.Id=cs.CostExpenditureBillId
                                inner join dcms_crm.CRM_Terminals AS t on cs.CustomerId=t.Id
                            WHERE
                                 sb.StoreId = " + storeId + "  AND  sb.auditedStatus = 1 AND sb.ReversedStatus = 0 AND abs(sb.OweCash) > 0 AND (sb.ReceiptStatus = 0 or sb.ReceiptStatus = 1)";

                queryString += districtWhere;

                if (bussinessUserId.HasValue && bussinessUserId.Value != 0)
                    queryString += $" and sb.EmployeeId = '{bussinessUserId}' ";

                if (terminalId.HasValue && terminalId.Value != 0)
                    queryString += $" and sb.TerminalId = '{terminalId}' ";

                if (channelId.HasValue && channelId.Value != 0)
                    queryString += $" and t.ChannelId = '{channelId}' ";

                if (terminalName != null)
                    queryString += $" and t.Name like '%{terminalName}%' ";

                if (startTime.HasValue)
                {
                    queryString += @" AND sb.CreatedOnUtc >= '" + startTime.Value.ToString("yyyy-MM-dd 00:00:00") + "'";
                }

                if (endTime.HasValue)
                {
                    queryString += @" AND sb.CreatedOnUtc <= '" + endTime.Value.ToString("yyyy-MM-dd 23:59:59") + "'";
                }

                if (moreDay.HasValue && moreDay.Value != 0)
                {
                    queryString += $" and  DATEDIFF(current_date,sb.CreatedOnUtc) > {moreDay} ";
                }

                queryString += @" ) UNION ALL (SELECT  DATEDIFF(CURRENT_DATE, sb.CreatedOnUtc) AS sDay,sb.StoreId,
                                sb.Id AS BillId,
                                sb.BillNumber AS BillNumber,
                                47 AS BillTypeId,'其它收入' as BillTypeName,
                                sb.TerminalId AS TerminalId,
                                t.Name AS TerminalName,
                                t.Code AS TerminalCode,
                                t.ChannelId,
                                sb.SalesmanId AS BusinessUserId,

                                sb.OweCash AS OweCase, 
                                0.00 AS SaleAmount,
                                0.00 ReturnAmount,
                                0.00 NetAmount,
                                sb.CreatedOnUtc, 
                                NOW() AS FirstOweCaseDate, 
                                NOW() AS LastOweCaseDate,

                                sb.SumAmount AS Amount,
                                sb.DiscountAmount AS DiscountAmount,
                                0 AS PaymentedAmount
                            FROM
                                dcms.FinancialIncomeBills AS sb
                                inner join dcms_crm.CRM_Terminals AS t on sb.TerminalId=t.Id
                            WHERE
                                 sb.StoreId = " + storeId + "  AND  sb.auditedStatus = 1 AND sb.ReversedStatus = 0 AND abs(sb.OweCash) > 0 AND (sb.ReceiptStatus = 0 or sb.ReceiptStatus = 1)";

                queryString += districtWhere;

                if (bussinessUserId.HasValue && bussinessUserId.Value != 0)
                    queryString += $" and sb.SalesmanId = '{bussinessUserId}' ";

                if (terminalId.HasValue && terminalId.Value != 0)
                    queryString += $" and sb.TerminalId = '{terminalId}' ";

                if (channelId.HasValue && channelId.Value != 0)
                    queryString += $" and t.ChannelId = '{channelId}' ";

                if (terminalName != null)
                    queryString += $" and t.Name like '%{terminalName}%' ";

                if (startTime.HasValue)
                {
                    queryString += @" AND sb.CreatedOnUtc >= '" + startTime.Value.ToString("yyyy-MM-dd 00:00:00") + "'";
                }

                if (endTime.HasValue)
                {
                    queryString += @" AND sb.CreatedOnUtc <= '" + endTime.Value.ToString("yyyy-MM-dd 23:59:59") + "'";
                }

                if (moreDay.HasValue && moreDay.Value != 0)
                {
                    queryString += $" and  DATEDIFF(current_date,sb.CreatedOnUtc) > {moreDay} ";
                }

                queryString += @" )";

                var bills = SaleBillsRepository_RO.QueryFromSql<FundReportCustomerReceiptCash>(queryString).ToList();

                //重写计算： 优惠金额	 已收金额  尚欠金额
                foreach (var bill in bills)
                {
                    //销售单
                    if (bill.BillTypeId == (int)BillTypeEnum.SaleBill)
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
                        var discountAmountOnce = _commonBillService.GetBillDiscountAmountOnce(storeId ?? 0, bill.BillId);

                        //优惠金额 =  单据优惠金额  + （单已经收款部分的本次优惠合计）
                        calc_discountAmount = Convert.ToDecimal(Convert.ToDouble(bill.DiscountAmount ?? 0) + Convert.ToDouble(discountAmountOnce));

                        //单据收款金额（收款账户）
                        var collectionAmount = _commonBillService.GetBillCollectionAmount(storeId ?? 0, bill.BillId, BillTypeEnum.SaleBill);

                        //单已经收款部分的本次收款合计
                        var receivableAmountOnce = _commonBillService.GetBillReceivableAmountOnce(storeId ?? 0, bill.BillId);

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
                        bill.OweCase = calc_arrearsAmount;

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
                        var discountAmountOnce = _commonBillService.GetBillDiscountAmountOnce(storeId ?? 0, bill.BillId);

                        //优惠金额 =  单据优惠金额  + （单已经收款部分的本次优惠合计）
                        calc_discountAmount = bill.DiscountAmount ?? 0 + discountAmountOnce;

                        //单据收款金额（收款账户）
                        var collectionAmount = _commonBillService.GetBillCollectionAmount(storeId ?? 0, bill.BillId, BillTypeEnum.ReturnBill);

                        //单已经收款部分的本次收款合计
                        var receivableAmountOnce = _commonBillService.GetBillReceivableAmountOnce(storeId ?? 0, bill.BillId);

                        //已收金额 = 单据收款金额（收款账户） + （单已经收款部分的本次收款合计）
                        calc_paymentedAmount = collectionAmount + receivableAmountOnce;

                        //尚欠金额
                        calc_arrearsAmount = Convert.ToDecimal(Convert.ToDouble(calc_billAmount) - Convert.ToDouble(calc_discountAmount) - Math.Abs(Convert.ToDouble(calc_paymentedAmount)));

                        #endregion

                        //重新赋值
                        bill.Amount = -calc_billAmount;
                        bill.DiscountAmount = -calc_discountAmount;
                        bill.PaymentedAmount = -calc_paymentedAmount;
                        bill.OweCase = -calc_arrearsAmount;

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
                        var discountAmountOnce = _commonBillService.GetBillDiscountAmountOnce(storeId ?? 0, bill.BillId);

                        //优惠金额 =  单据优惠金额  + （已经收款部分的本次优惠合计）
                        calc_discountAmount = bill.DiscountAmount ?? 0 + discountAmountOnce;

                        //单据收款金额（收款账户）
                        var collectionAmount = _commonBillService.GetBillCollectionAmount(storeId ?? 0, bill.BillId, BillTypeEnum.AdvanceReceiptBill);

                        //单已经收款部分的本次收款合计
                        var receivableAmountOnce = _commonBillService.GetBillReceivableAmountOnce(storeId ?? 0, bill.BillId);

                        //已收金额 = 单据收款金额（收款账户） + （已经收款部分的本次收款合计）
                        calc_paymentedAmount = collectionAmount + receivableAmountOnce;

                        calc_arrearsAmount = Convert.ToDecimal(Convert.ToDouble(calc_billAmount) - Convert.ToDouble(calc_discountAmount) - Convert.ToDouble(calc_paymentedAmount));

                        #endregion

                        //重新赋值
                        bill.Amount = calc_billAmount;
                        bill.DiscountAmount = calc_discountAmount;
                        bill.PaymentedAmount = calc_paymentedAmount;
                        bill.OweCase = calc_arrearsAmount;
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
                        var discountAmountOnce = _commonBillService.GetBillDiscountAmountOnce(storeId ?? 0, bill.BillId);

                        //优惠金额 =  单据优惠金额  + （单已经收款部分的本次优惠合计）
                        calc_discountAmount = bill.DiscountAmount ?? 0 + discountAmountOnce;

                        //单据收款金额（收款账户）
                        var collectionAmount = _commonBillService.GetBillCollectionAmount(storeId ?? 0, bill.BillId, BillTypeEnum.CostExpenditureBill);

                        //单已经收款部分的本次收款合计
                        var receivableAmountOnce = _commonBillService.GetBillReceivableAmountOnce(storeId ?? 0, bill.BillId);

                        //已收金额 = 单据收款金额（收款账户） + （单已经收款部分的本次收款合计）
                        calc_paymentedAmount = collectionAmount + receivableAmountOnce;

                        //尚欠金额 
                        calc_arrearsAmount = Convert.ToDecimal(Convert.ToDouble(calc_billAmount) - Convert.ToDouble(calc_discountAmount) - Math.Abs(Convert.ToDouble(calc_paymentedAmount)));

                        #endregion

                        //重新赋值
                        bill.Amount = -Math.Abs(calc_billAmount);
                        bill.DiscountAmount = -Math.Abs(calc_discountAmount);
                        bill.PaymentedAmount = -Math.Abs(calc_paymentedAmount);
                        bill.OweCase = -Math.Abs(calc_arrearsAmount);
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
                        var discountAmountOnce = _commonBillService.GetBillDiscountAmountOnce(storeId ?? 0, bill.BillId);

                        //优惠金额 =  单据优惠金额  + （单已经收款部分的本次优惠合计）
                        calc_discountAmount = bill.DiscountAmount ?? 0 + discountAmountOnce;

                        //单据收款金额（收款账户）
                        var collectionAmount = _commonBillService.GetBillCollectionAmount(storeId ?? 0, bill.BillId, BillTypeEnum.FinancialIncomeBill);

                        //单已经收款部分的本次收款合计
                        var receivableAmountOnce = _commonBillService.GetBillReceivableAmountOnce(storeId ?? 0, bill.BillId);

                        //已收金额 = 单据收款金额（收款账户） + （单已经收款部分的本次收款合计）
                        calc_paymentedAmount = collectionAmount + receivableAmountOnce;

                        //尚欠金额 
                        calc_arrearsAmount = Convert.ToDecimal(Convert.ToDouble(calc_billAmount) - Convert.ToDouble(calc_discountAmount) - Convert.ToDouble(calc_paymentedAmount));

                        #endregion

                        //重新赋值
                        bill.Amount = calc_billAmount;
                        bill.DiscountAmount = calc_discountAmount;
                        bill.PaymentedAmount = calc_paymentedAmount;
                        bill.OweCase = calc_arrearsAmount;

                    }
                }


                if (bills != null)
                {
                    foreach (IGrouping<int, FundReportCustomerReceiptCash> groups in bills.GroupBy(s => s.TerminalId ?? 0))
                    {
                        var first = groups.First();
                        var frcrc = new FundReportCustomerReceiptCash
                        {
                            /// 账期天数
                            sDay = groups.Select(s => s.sDay).Sum(),
                            /// 客户Id
                            TerminalId = first.TerminalId,
                            /// 客户名称
                            TerminalName = first.TerminalName,
                            /// 客户编码
                            TerminalCode = first.TerminalCode,
                            /// 累计欠款
                            OweCase = groups.Select(s => s.OweCase).Sum(),
                            /// 销售金额
                            SaleAmount = groups.Select(s => s.SaleAmount).Sum(),
                            /// 退货金额
                            ReturnAmount = groups.Select(s => s.ReturnAmount).Sum(),
                            /// 净销售额=(销售金额-退货金额)
                            NetAmount = groups.Select(s => s.SaleAmount).Sum() - groups.Select(s => s.ReturnAmount).Sum(),
                            /// 首次欠款时间
                            FirstOweCaseDate = groups.OrderBy(s => s.CreatedOnUtc).Select(s => s.CreatedOnUtc).FirstOrDefault(),
                            /// 末次欠款时间
                            LastOweCaseDate = groups.OrderBy(s => s.CreatedOnUtc).Select(s => s.CreatedOnUtc).LastOrDefault(),
                            //
                            Bills = groups.ToDictionary(k => k.BillId, v => v.BillTypeId)
                        };

                        //if (frcrc.OweCase > 0)
                        reporting.Add(frcrc);
                    }
                }

                return reporting;
            }
            catch (Exception ex)
            {
                return new List<FundReportCustomerReceiptCash>();
            }
        }

        /// <summary>
        /// 供应商往来账
        /// </summary>
        /// <param name="storeId">经销商Id</param>
        /// <param name="billNumber">单据编号</param>
        /// <param name="billTypeId">单据类型Id</param>
        /// <param name="manufacturerId">供应商Id</param>
        /// <param name="remark">备注</param>
        /// <param name="startTime">开始日期</param>
        /// <param name="endTime">结束日期</param>
        /// <returns></returns>
        public IList<FundReportManufacturerAccount> GetFundReportManufacturerAccount(int? storeId, string billNumber, int? billTypeId, int? manufacturerId, string remark, DateTime? startTime, DateTime? endTime)
        {
            try
            {
                return _cacheManager.Get(DCMSDefaults.FUND_GETFUND_MANUFACTURERACCOUNT_KEY.FillCacheKey(storeId, billNumber, billTypeId, manufacturerId, remark, startTime, endTime), () =>
                {

                    billNumber = CommonHelper.Filter(billNumber);
                    remark = CommonHelper.Filter(remark);
                    var reporting = new List<FundReportManufacturerAccount>();


                    string whereQuery = $" a.StoreId = {storeId ?? 0} ";
                    string whereQuery1 = "";
                    if (!string.IsNullOrEmpty(billNumber))
                    {
                        whereQuery += $" and a.BillNumber like '%{billNumber}%' ";
                    }

                    if (!string.IsNullOrEmpty(remark))
                    {
                        whereQuery += $" and a.Remark like '%{remark}%' ";
                    }
                    if (manufacturerId.HasValue && manufacturerId.Value != 0)
                    {
                        whereQuery += $" and  b.Id = {manufacturerId} ";
                    }

                        //已审核
                        whereQuery += $" and a.AuditedStatus=1 ";
                        //未红冲
                        whereQuery += $" and a.ReversedStatus=0 ";

                        //列表中预付款列
                        //预付款款会计科目(采购,退购,付款) 具有预付款科目，预付款的付款科目金额都为预付款
                        string[] advanceAccountings = null;
                    var accountingOption = _accountingService.GetAccountingOptionByCode(storeId, "100501");
                    if (accountingOption != null)
                    {
                        advanceAccountings = new string[] { accountingOption.Id.ToString() };
                    }
                    if (advanceAccountings.Length == 0)
                    {
                        advanceAccountings = new string[] { "0" };
                    }
                    if (billTypeId.HasValue && billTypeId.Value != 0)
                    {
                        whereQuery1 += $" where BillTypeId = '{billTypeId}' ";
                    }
                        //22 24 42 44
                        #region MSSQL
                        //string sqlString = $"(select a.StoreId ,a.Id as BillId  ,a.BillNumber  ,a.ManufacturerId  ,b.Name as ManufacturerName ,22 BillTypeId ,'采购' BillTypeName  ,a.TransactionDate  ,a.SumAmount  BillAmount  ,a.PreferentialAmount  ,PayCashAmount = (select ISNULL( sum(c.CollectionAmount),0) from PurchaseBill_Accounting_Mapping as c where c.PurchaseBillId= a.Id ) ,0.00 PayAmountSubtract  ,0.00 PayAmountAdd  ,0.00 PayAmountOverage ,CurAdvancePayAmount = (select ISNULL(sum(c.CollectionAmount), 0) from PurchaseBill_Accounting_Mapping as c where c.PurchaseBillId = a.Id and c.AccountingOptionId in ({string.Join(",", advanceAccountings)})  ) ,0.00 AdvancePayAmountSubtract ,0.00 AdvancePaymentAmountAdd  ,0.00 AdvancePayAmountOverage  ,0.00 AccountAmountOverage  ,a.Remark from  PurchaseBills as a  inner join Manufacturer as b on a.ManufacturerId = b.Id  where {whereQuery} ) UNION ALL(select a.StoreId , a.Id as BillId  ,a.BillNumber  ,a.ManufacturerId  ,b.Name as ManufacturerName ,24 BillTypeId ,'退购' BillTypeName  ,a.TransactionDate  ,a.SumAmount BillAmount, a.PreferentialAmount  ,PayCashAmount = (select ISNULL(sum(c.CollectionAmount), 0) from PurchaseReturnBill_Accounting_Mapping as c where c.PurchaseReturnBillId = a.Id ) ,0.00 PayAmountSubtract  ,0.00 PayAmountAdd  ,0.00 PayAmountOverage ,CurAdvancePayAmount = (select ISNULL(sum(c.CollectionAmount), 0) from PurchaseReturnBill_Accounting_Mapping as c where c.PurchaseReturnBillId = a.Id and c.AccountingOptionId in ({string.Join(",", advanceAccountings)})  ) ,0.00 AdvancePayAmountSubtract ,0.00 AdvancePayAmountAdd  ,0.00 AdvancePayAmountOverage   ,0.00 AccountAmountOverage  ,a.Remark from  PurchaseReturnBills as a inner join Manufacturer as b on a.ManufacturerId = b.Id where {whereQuery} ) UNION ALL(select a.StoreId , a.Id as BillId  ,a.BillNumber  ,a.CustomerId as TerminalId  ,b.Name as ManufacturerName   ,42 BillTypeId ,'付款' BillTypeName  ,a.CreatedOnUtc as TransactionDate  ,0.00 BillAmount  ,a.TotalDiscountAmount as PreferentialAmount  ,PayCashAmount = (select ISNULL(sum(c.CollectionAmount), 0) from PaymentReceiptBill_Accounting_Mapping as c where c.PaymentReceiptBillId = a.Id ) ,0.00 PayAmountSubtract  ,0.00 PayAmountAdd  ,0.00 PayAmountOverage ,CurAdvancePayAmount = (select ISNULL(sum(c.CollectionAmount), 0) from PaymentReceiptBill_Accounting_Mapping as c where c.PaymentReceiptBillId = a.Id and c.AccountingOptionId in ({string.Join(",", advanceAccountings)})  ) ,0.00 AdvancePayAmountSubtract ,0.00 AdvancePayAmountAdd  ,0.00 AdvancePayAmountOverage  ,0.00 AccountAmountOverage  ,a.Remark from  CashReceiptBills as a inner join Manufacturer as b on a.CustomerId = b.Id  where {whereQuery} ) UNION ALL(select a.StoreId , a.Id as BillId  ,a.BillNumber  ,a.CustomerId as TerminalId  ,b.Name as ManufacturerName  ,44 BillTypeId ,'预付款' BillTypeName  ,a.CreatedOnUtc as TransactionDate ,a.AdvanceAmount BillAmount, a.DiscountAmount as PreferentialAmount  ,PayCashAmount = (select ISNULL(sum(c.CollectionAmount), 0) from AdvancePaymentBill_Accounting_Mapping as c where c.AdvancePaymentBillId = a.Id ) ,0.00 PayAmountSubtract  ,0.00 PayAmountAdd  ,0.00 PayAmountOverage ,0.00 CurAdvancePayAmount,0.00 AdvancePayAmountSubtract ,0.00 AdvancePayAmountAdd  ,0.00 AdvancePayAmountOverage   ,0.00 AccountAmountOverage  ,a.Remark from  AdvanceReceiptBills as a inner join Manufacturer as b on a.CustomerId = b.Id where {whereQuery} )";
                        #endregion


                        #region MYSQL
                        string sqlString = $"select * from (select a.StoreId ,a.Id as BillId  ,a.BillNumber  ,a.ManufacturerId  ,b.Name as ManufacturerName ,22 BillTypeId ,'采购' BillTypeName  ,a.TransactionDate  ,a.ReceivableAmount  BillAmount  ,a.PreferentialAmount  , (select IFNULL( sum(c.CollectionAmount),0) from PurchaseBill_Accounting_Mapping as c where c.PurchaseBillId= a.Id ) as PayCashAmount ,0.00 PayAmountSubtract  ,0.00 PayAmountAdd  ,0.00 PayAmountOverage , (select IFNULL(sum(c.CollectionAmount), 0) from PurchaseBill_Accounting_Mapping as c where c.PurchaseBillId = a.Id and c.AccountingOptionId in ({string.Join(",", advanceAccountings)})  ) as CurAdvancePayAmount ,0.00 AdvancePayAmountSubtract , 0.00 AdvancePayAmountAdd,0.00 AdvancePayAmountOverage,0.00 AccountAmountOverage  ,a.Remark from  PurchaseBills as a  inner join Manufacturer as b on a.ManufacturerId = b.Id  where {whereQuery}  UNION ALL select a.StoreId , a.Id as BillId  ,a.BillNumber  ,a.ManufacturerId  ,b.Name as ManufacturerName ,24 BillTypeId ,'退购' BillTypeName  ,a.TransactionDate  ,a.ReceivableAmount BillAmount, a.PreferentialAmount  , (select IFNULL(sum(c.CollectionAmount), 0) from PurchaseReturnBill_Accounting_Mapping as c where c.PurchaseReturnBillId = a.Id ) as PayCashAmount,0.00 PayAmountSubtract  ,0.00 PayAmountAdd  ,0.00 PayAmountOverage , (select IFNULL(sum(c.CollectionAmount), 0) from PurchaseReturnBill_Accounting_Mapping as c where c.PurchaseReturnBillId = a.Id and c.AccountingOptionId in ({string.Join(",", advanceAccountings)})  ) as CurAdvancePayAmount,0.00 AdvancePayAmountSubtract ,0.00 AdvancePayAmountAdd,0.00 AdvancePayAmountOverage,0.00 AccountAmountOverage  ,a.Remark from  PurchaseReturnBills as a inner join Manufacturer as b on a.ManufacturerId = b.Id where {whereQuery}  UNION ALL select a.StoreId , a.Id as BillId  ,a.BillNumber  ,a.ManufacturerId as TerminalId  ,b.Name as ManufacturerName   ,42 BillTypeId ,'付款' BillTypeName  ,a.CreatedOnUtc as TransactionDate  ,0.00 BillAmount  ,a.DiscountAmount as PreferentialAmount,(select IFNULL(sum(c.CollectionAmount), 0) from PaymentReceiptBill_Accounting_Mapping as c where c.PaymentReceiptBillId = a.Id ) as PayCashAmount,0.00 PayAmountSubtract  ,0.00 PayAmountAdd  ,0.00 PayAmountOverage ,  (select IFNULL(sum(c.CollectionAmount), 0) from PaymentReceiptBill_Accounting_Mapping as c where c.PaymentReceiptBillId = a.Id and c.AccountingOptionId in ({string.Join(",", advanceAccountings)})  ) as CurAdvancePayAmount ,0.00 AdvancePayAmountSubtract ,0.00 AdvancePayAmountAdd,0.00 AdvancePayAmountOverage,0.00 AccountAmountOverage , a.Remark from  PaymentReceiptBills as a inner join Manufacturer as b on a.ManufacturerId = b.Id  where {whereQuery}   UNION ALL   select a.StoreId , a.Id as BillId  ,a.BillNumber  ,a.CustomerId as TerminalId  ,b.Name as ManufacturerName  ,44 BillTypeId ,'预付款' BillTypeName  ,a.CreatedOnUtc as TransactionDate ,a.AdvanceAmount BillAmount, a.DiscountAmount as PreferentialAmount  , (select IFNULL(sum(c.CollectionAmount), 0) from AdvancePaymentBill_Accounting_Mapping as c where c.AdvancePaymentBillId = a.Id and c.Copy=0 ) as PayCashAmount ,0.00 PayAmountSubtract  ,0.00 PayAmountAdd  ,0.00 PayAmountOverage ,0.00 CurAdvancePayAmount,0.00 AdvancePayAmountSubtract,0.00 AdvancePayAmountAdd,0.00 AdvancePayAmountOverage,0.00 AccountAmountOverage,a.Remark from  AdvanceReceiptBills as a inner join Manufacturer as b on a.CustomerId = b.Id where {whereQuery} ) as a {whereQuery1}";
                        #endregion

                        reporting = SaleBillsRepository_RO.QueryFromSql<FundReportManufacturerAccount>(sqlString).ToList();

                    reporting.ForEach(b =>
                    {
                        decimal? increase_s = 0, reduce_s = 0, sum_s = 0;
                        decimal? increase_r = 0, reduce_r = 0, sum_r = 0;
                        sum_s = (b.BillAmount ?? 0 - b.PreferentialAmount ?? 0 - b.PayCashAmount ?? 0);
                        sum_r = b.CurAdvancePayAmount ?? 0;

                        if (sum_s > 0)
                        {
                            increase_s = sum_s;
                        }

                        if (sum_s < 0)
                        {
                            reduce_s = sum_s;
                        }

                            //应付款减  单据金额-优惠金额-付款金额 >0 增加 
                            b.PayAmountSubtract = reduce_s;
                            //应付款加 单据金额-优惠金额-付款金额 < 0 减少
                            b.PayAmountAdd = increase_s;

                        if (sum_r > 0)
                        {
                            reduce_r = -sum_r;
                        }

                        if (sum_r < 0)
                        {
                            increase_r = Math.Abs(sum_r ?? 0);
                        }

                            //预付款减  本次预付款>0 为减少
                            b.AdvancePayAmountSubtract = reduce_r;
                            //预付款加 本次预付款<0 为增加
                            b.AdvancePayAmountAdd = increase_r;

                    });

                    if (startTime.HasValue)
                    {
                        startTime = DateTime.Parse(((DateTime)startTime).ToString("yyyy-MM-dd 00:00:00"));
                    }

                    if (endTime.HasValue)
                    {
                        endTime = DateTime.Parse(((DateTime)endTime).ToString("yyyy-MM-dd 23:59:59"));
                    }

                        //期初应付款余额
                        var startASubtract = reporting.Where(s => s.TransactionDate < startTime).Select(s => s.PayAmountSubtract).Sum();
                    var startAAdd = reporting.Where(s => s.TransactionDate < startTime).Select(s => s.PayAmountAdd).Sum();
                    var startABlanance = startASubtract + startAAdd;

                        //期初预付款余额
                        var startPSubtract = reporting.Where(s => s.TransactionDate < startTime).Select(s => s.AdvancePayAmountSubtract).Sum();
                    var startPAdd = reporting.Where(s => s.TransactionDate < startTime).Select(s => s.AdvancePayAmountAdd).Sum();
                    var startPBlanance = startPSubtract + startPAdd;

                    if (startTime.HasValue)
                    {
                        reporting = reporting.Where(s => s.TransactionDate >= startTime).ToList();
                    }

                    if (endTime.HasValue)
                    {
                        reporting = reporting.Where(s => s.TransactionDate <= endTime).ToList();
                    }

                    decimal? lastA = startABlanance;
                    decimal? lastP = startPBlanance;

                        //计算余额
                        foreach (var b in reporting.OrderBy(s => s.TransactionDate).ToList())
                    {
                        if (b.BillTypeName != "退购")
                        {
                            lastA += (b.PayAmountSubtract + b.PayAmountAdd);
                        }
                        else
                        {
                            lastA -= (b.PayAmountSubtract + b.PayAmountAdd);

                        }
                        lastP += (b.AdvancePayAmountSubtract + b.AdvancePayAmountAdd);

                            //应付款余额
                            b.PayAmountOverage = lastA;
                            //预付款余额
                            b.AdvancePayAmountOverage = lastP;
                            //往来账余额
                            b.AccountAmountOverage = lastA - lastP;
                    }

                    return reporting.OrderBy(s => s.TransactionDate).ToList();

                });
            }
            catch (Exception)
            {
                return new List<FundReportManufacturerAccount>();
            }
        }

        /// <summary>
        /// 供应商应付款
        /// </summary>
        /// <param name="storeId">经销商Id</param>
        /// <param name="bussinessUserId">业务员Id</param>
        /// <param name="moreDay">账期大于...天</param>
        /// <returns></returns>
        public IList<FundReportManufacturerPayCash> GetFundReportManufacturerPayCash(int? storeId, int? bussinessUserId, int? moreDay, DateTime? startTime, DateTime? endTime)
        {
            try
            {

                return _cacheManager.Get(DCMSDefaults.FUND_GETFUND_MANUFACTURERPAYCASH_KEY.FillCacheKey(storeId, bussinessUserId, moreDay, startTime, endTime), () =>
               {
                   var reporting = new List<FundReportManufacturerPayCash>();

                   string whereQuery = $" a.StoreId = {storeId ?? 0} ";

                   if (bussinessUserId.HasValue && bussinessUserId.Value != 0)
                   {
                       whereQuery += $" and a.BusinessUserId = '{bussinessUserId}' ";
                   }

                   if (moreDay.HasValue && moreDay.Value != 0)
                   {
                           //whereQuery += $" and  DATEDIFF(day, a.CreatedOnUtc, GETDATE()) > {moreDay}) ";
                           whereQuery += $" and  DATEDIFF(current_date,a.CreatedOnUtc) > {moreDay} ";
                   }

                   if (startTime.HasValue)
                   {
                       whereQuery += $" and a.CreatedOnUtc >= '{startTime?.ToString("yyyy-MM-dd 00:00:00")}' ";
                   }

                   if (endTime.HasValue)
                   {
                       whereQuery += $" and b.CreatedOnUtc <='{endTime?.ToString("yyyy-MM-dd 23:59:59")}' ";
                   }

                       //已审核
                       whereQuery += $" and a.AuditedStatus=1 ";
                       //未红冲
                       whereQuery += $" and a.ReversedStatus=0 ";

                       #region MSSQL
                       //string sqlString = $"(select a.ManufacturerId ,DATEDIFF(day, a.CreatedOnUtc, GETDATE()) as sDay, a.Id as BillId,12 as BillType,b.Name as ManufacturerName ,'' as ManufacturerCode ,a.OweCash as OweCase,a.SumAmount as SaleAmount ,0.00 ReturnAmount ,0.00 NetAmount ,a.CreatedOnUtc from  PurchaseBills  as a inner join Manufacturer as b on a.ManufacturerId = b.Id  where {whereQuery} ) UNION ALL(select a.ManufacturerId,DATEDIFF(day, a.CreatedOnUtc, GETDATE()) as sDay ,a.Id as BillId,14 as BillType,b.Name as TerminalName ,'' as ManufacturerCode ,a.OweCash as OweCase,0.00 as SaleAmount ,a.SumAmount ReturnAmount ,0.00 NetAmount ,a.CreatedOnUtc from  PurchaseReturnBills  as a inner join Manufacturer as b on a.ManufacturerId = b.Id where {whereQuery} )";
                       #endregion

                       #region MYSQL
                       //DATEDIFF(current_date,a.CreatedOnUtc)
                       string sqlString = $"(select a.ManufacturerId ,DATEDIFF(current_date,a.CreatedOnUtc) as sDay, a.Id as BillId,12 as BillType,b.Name as ManufacturerName ,'' as ManufacturerCode ,a.OweCash as OweCase,a.SumAmount as SaleAmount ,0.00 ReturnAmount ,0.00 NetAmount ,a.CreatedOnUtc,now() as FirstOweCaseDate,now() as LastOweCaseDate from  PurchaseBills  as a inner join Manufacturer as b on a.ManufacturerId = b.Id  where {whereQuery} ) UNION ALL(select a.ManufacturerId,DATEDIFF(current_date,a.CreatedOnUtc) as sDay ,a.Id as BillId,14 as BillType,b.Name as TerminalName ,'' as ManufacturerCode ,a.OweCash as OweCase,0.00 as SaleAmount ,a.SumAmount ReturnAmount ,0.00 NetAmount ,a.CreatedOnUtc,now() as FirstOweCaseDate,now() as LastOweCaseDate from  PurchaseReturnBills  as a inner join Manufacturer as b on a.ManufacturerId = b.Id where {whereQuery} )";
                       #endregion

                       var results = SaleBillsRepository_RO.QueryFromSql<FundReportManufacturerPayCash>(sqlString).ToList();
                   if (results != null)
                   {
                       foreach (IGrouping<int, FundReportManufacturerPayCash> groups in results.GroupBy(s => s.ManufacturerId ?? 0))
                       {
                           var first = groups.First();
                           var frcrc = new FundReportManufacturerPayCash
                           {
                                   /// 账期天数
                                   sDay = groups.Select(s => s.sDay).Sum(),
                                   /// 客户Id
                                   ManufacturerId = first.ManufacturerId,
                                   /// 客户名称
                                   ManufacturerName = first.ManufacturerName,
                                   /// 客户编码
                                   ManufacturerCode = first.ManufacturerCode,
                                   /// 累计欠款
                                   OweCase = groups.Select(s => s.OweCase).Sum(),
                                   /// 销售金额
                                   SaleAmount = groups.Select(s => s.SaleAmount).Sum(),
                                   /// 退货金额
                                   ReturnAmount = groups.Select(s => s.ReturnAmount).Sum(),
                                   /// 净销售额=(销售金额-退货金额)
                                   NetAmount = groups.Select(s => s.SaleAmount).Sum() - groups.Select(s => s.ReturnAmount).Sum(),
                                   /// 首次欠款时间
                                   FirstOweCaseDate = groups.OrderBy(s => s.CreatedOnUtc).Select(s => s.CreatedOnUtc).FirstOrDefault(),
                                   /// 末次欠款时间
                                   LastOweCaseDate = groups.OrderBy(s => s.CreatedOnUtc).Select(s => s.CreatedOnUtc).LastOrDefault(),
                                   //
                                   Bills = groups.ToDictionary(k => k.BillId, v => v.BillType)
                           };

                           reporting.Add(frcrc);
                       }
                   }

                   return reporting;
               });
            }
            catch (Exception)
            {
                return new List<FundReportManufacturerPayCash>();
            }
        }

        /// <summary>
        /// 预收款余额
        /// </summary>
        /// <param name="storeId">经销商Id</param>
        /// <param name="terminalId">客户Id</param>
        /// <returns></returns>
        public IList<FundReportAdvanceReceiptOverage> GetFundReportAdvanceReceiptOverage(int? storeId, int? terminalId)
        {
            try
            {
                var key = DCMSDefaults.FUND_GETFUND_ADVANCERECEIPTOVERAGE_KEY.FillCacheKey(storeId, terminalId);
                _cacheManager.Remove(key);
                return _cacheManager.Get(DCMSDefaults.FUND_GETFUND_ADVANCERECEIPTOVERAGE_KEY.FillCacheKey(storeId, terminalId), () =>
               {

                   var reporting = new List<FundReportAdvanceReceiptOverage>();

                   string whereQuery = $" alls.StoreId = {storeId ?? 0} ";

                   if (terminalId.HasValue && terminalId.Value != 0)
                   {
                       whereQuery += $" and alls.TerminalId = '{terminalId}' ";
                   }

                       //23 预收款 科目

                       //MSSQL
                       //string sqlString = $"select alls.StoreId, alls.TerminalId, alls.TerminalName,alls.TerminalCode, ISNULL( sum(alls.AdvanceAmount),0) as AdvanceReceiptOverageAmount,ReceivableOverageAmount = (select ISNULL( sum(sb.SumAmount),0) from SaleBills sb where sb.TerminalId= alls.TerminalId and sb.Receipted = 'FALSE' and sb.AuditedStatus = 'TRUE' and sb.ReversedStatus = 'FALSE') + (select  ISNULL( sum(sb.SumAmount),0) from ReturnBills sb where sb.TerminalId= alls.TerminalId and sb.Receipted = 'FALSE' and sb.AuditedStatus = 'TRUE' and sb.ReversedStatus = 'FALSE') ,0.00 OverageAmount ,(STUFF(( SELECT  ',' + CONVERT(VARCHAR(100) ,aa.AccountingOptionId ) +'|'+ CONVERT(VARCHAR(100),aa.TotalAdvanceAmount) from (select t.AccountingOptionId,sum(t.AdvanceAmount) TotalAdvanceAmount  from  AdvanceReceiptBills as t where t.CustomerId = alls.TerminalId and t.AccountingOptionId in (select ap.Id from AccountingOptions ap where ap.ParentId=23) group by t.AccountingOptionId) as aa   FOR XML PATH('') ), 1, 1, '')) as Accounts from (select a.StoreId,a.CustomerId as TerminalId,b.Name as TerminalName,b.Code as TerminalCode,a.AccountingOptionId,a.AdvanceAmount from AdvanceReceiptBills as a inner join dcms_crm.CRM_Terminals as b on a.CustomerId = b.Id where a.StoreId = {storeId ?? 0} and a.AccountingOptionId in (select ap.Id from AccountingOptions ap where ap.ParentId=23)) as alls where {whereQuery}  group by alls.StoreId,alls.TerminalId,alls.TerminalName,alls.TerminalCode,alls.AccountingOptionId";

                       //MYSQL 20200211
                       string sqlString = $"select alls.StoreId, alls.TerminalId, alls.AccountingOptionId, alls.TerminalName,alls.TerminalCode,0 as AdvanceReceiptOverageAmount, IFNULL( sum(alls.AdvanceAmount),0) as AdvanceReceiptAmount,((select IFNULL(sum(sb.ReceivableAmount), 0) from SaleBills sb where sb.TerminalId = alls.TerminalId and sb.AuditedStatus = '1' and sb.ReversedStatus = '0') + (select  IFNULL(sum(sb.ReceivableAmount), 0) from ReturnBills sb where sb.TerminalId = alls.TerminalId and sb.AuditedStatus = '1' and sb.ReversedStatus = '0')) as ReceivableOverageAmount ,0.00 OverageAmount,group_concat(CONCAT(alls.AccountingOptionId, '|', alls.AdvanceAmount) separator ',') as Accounts from(select a.StoreId, a.CustomerId as TerminalId, b.Name as TerminalName, b.Code as TerminalCode, a.AccountingOptionId, a.AdvanceAmount from AdvanceReceiptBills as a inner join dcms_crm.CRM_Terminals as b on a.CustomerId = b.Id where a.StoreId ={storeId ?? 0} and a.AuditedStatus=1 and a.ReversedStatus=0 and a.AccountingOptionId in (select ap.Id from AccountingOptions ap where ap.AccountCodeTypeId = 24)) as alls where {whereQuery}  group by alls.StoreId,alls.TerminalId,alls.TerminalName,alls.TerminalCode,alls.AccountingOptionId";
                       //string sqlString = $"select alls.StoreId, alls.TerminalId, alls.AccountingOptionId, alls.TerminalName,alls.TerminalCode,0 as AdvanceReceiptAmount, IFNULL( sum(alls.AdvanceAmount),0) as AdvanceReceiptOverageAmount,((select IFNULL(sum(sb.SumAmount), 0) from SaleBills sb where sb.TerminalId = alls.TerminalId and sb.Receipted = '0' and sb.AuditedStatus = '1' and sb.ReversedStatus = '0') + (select  IFNULL(sum(sb.SumAmount), 0) from ReturnBills sb where sb.TerminalId = alls.TerminalId and sb.Receipted = '0' and sb.AuditedStatus = '1' and sb.ReversedStatus = '0')) as ReceivableOverageAmount ,0.00 OverageAmount,group_concat(CONCAT(alls.AccountingOptionId, '|', alls.AdvanceAmount1) separator ',') as Accounts from(select a.StoreId, a.CustomerId as TerminalId, b.Name as TerminalName, b.Code as TerminalCode, a.AccountingOptionId,a.AdvanceAmount as AdvanceAmount1, a.AdvanceAmount-d.CollectionAmount as AdvanceAmount from AdvanceReceiptBills as a left join dcms_crm.CRM_Terminals as b on a.CustomerId = b.Id left join dcms.CashReceiptBills c on b.id=c.CustomerId left join dcms.CashReceiptBill_Accounting_Mapping d on c.id=d.CashReceiptBillId where a.StoreId ={storeId ?? 0} and a.AuditedStatus=1 and a.ReversedStatus=0 and a.AccountingOptionId in (select ap.Id from AccountingOptions ap where ap.AccountCodeTypeId = 24)) as alls where {whereQuery}  group by alls.StoreId,alls.TerminalId,alls.TerminalName,alls.TerminalCode,alls.AccountingOptionId";


                       reporting = SaleBillsRepository_RO.QueryFromSql<FundReportAdvanceReceiptOverage>(sqlString).ToList();
                   if (reporting != null)
                   {
                       reporting.ForEach(b =>
                       {
                           var accounts = new List<Accounting>();
                               //31|5200.0000,32|5200.0000
                               var acts = b.Accounts.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                           acts.ForEach(s =>
                           {
                               var amounts = s.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                               if (accounts.Count(c => c.AccountingOptionId == int.Parse(amounts[0])) > 0)
                               {
                                   accounts.FirstOrDefault(c => c.AccountingOptionId == int.Parse(amounts[0])).CollectionAmount += decimal.Parse(amounts[1]);
                               }
                               else
                               {
                                   accounts.Add(new Accounting
                                   {
                                       AccountingOptionId = int.Parse(amounts[0]),
                                       CollectionAmount = decimal.Parse(amounts[1]),
                                   });
                               }
                           });

                           var accountOptions = _accountingService.GetAccountingOptionsByIds(storeId, accounts.Select(s => s.AccountingOptionId).ToArray()).Select(s => s).ToList();

                           accounts.ForEach(a =>
                           {
                               a.Name = accountOptions.Where(s => s.Id == a.AccountingOptionId).First().Name;
                           });

                           b.AccountingOptions = accounts;

                               //预收款余额
                               b.AdvanceReceiptOverageAmount = _commonBillService.GetTerminalBalance(storeId ?? 0, b.TerminalId ?? 0, 17);

                               //余额 = 预收款余额 - 应收款余额
                               b.OverageAmount = b.AdvanceReceiptOverageAmount - b.ReceivableOverageAmount;
                               //预收款账户
                               b.AccountingOptions = accounts ?? new List<Accounting>();

                       });
                   }
                   return reporting;
               });
            }
            catch (Exception)
            {
                return new List<FundReportAdvanceReceiptOverage>();
            }
        }

        /// <summary>
        /// 预付款余额
        /// </summary>
        /// <param name="storeId">经销商Id</param>
        /// <param name="manufacturerId">供应商Id</param>
        /// <returns></returns>
        public IList<FundReportAdvancePaymentOverage> GetFundReportAdvancePaymentOverage(int? storeId, int? manufacturerId, DateTime? endTime)
        {
            try
            {
                var key = DCMSDefaults.FUND_GETFUND_REPORTADVANCEPAYMENTOVERAGE_KEY.FillCacheKey(storeId, manufacturerId);
                _cacheManager.Remove(key);
                return _cacheManager.Get(DCMSDefaults.FUND_GETFUND_REPORTADVANCEPAYMENTOVERAGE_KEY.FillCacheKey(storeId, manufacturerId), () =>
               {
                   var reporting = new List<FundReportAdvancePaymentOverage>();

                   string whereQuery = $"a.StoreId = {storeId ?? 0} ";
                   string whereQuery2 = $" ";

                   if (manufacturerId.HasValue && manufacturerId.Value != 0)
                   {
                       whereQuery += $"and a.ManufacturerId = '{manufacturerId}' and";
                   }

                   if (endTime.HasValue)
                   {
                       whereQuery2 += $" and p.CreatedOnUtc <='{endTime?.ToString("yyyy-MM-dd 23:59:59")}' ";
                   }

                       //10 预付款账户 科目

                       //MSSQL
                       //string sqlString = $"select a.ManufacturerId,m.Name as ManufacturerName,isnull(sum(a.AdvanceAmount),0) as AdvancePaymentAmount,PurchaseAdvancePaymentAmount =(select isnull(sum(pa.CollectionAmount),0) from PurchaseBills as p inner join PurchaseBill_Accounting_Mapping as pa on p.Id = pa.PurchaseBillId where p.ManufacturerId = a.ManufacturerId and p.AuditedStatus = 'TRUE' and p.ReversedStatus = 'FALSE' and pa.AccountingOptionId in (select ap.Id from AccountingOptions ap where ap.ParentId= 10)  {whereQuery2}),PurchaseReturnAdvancePaymentAmount = (select isnull(sum(pa.CollectionAmount),0) from PurchaseReturnBills as p inner join PurchaseReturnBill_Accounting_Mapping as pa on p.Id = pa.PurchaseReturnBillId where p.ManufacturerId = a.ManufacturerId and p.AuditedStatus = 'TRUE' and p.ReversedStatus = 'FALSE' and pa.AccountingOptionId in (select ap.Id from AccountingOptions ap where ap.ParentId= 10) {whereQuery2}),0.00 OverageAmount,(select (STUFF(( SELECT  ',' + CONVERT(VARCHAR(100) ,aa.AccountingOptionId ) +'|'+ CONVERT(VARCHAR(100),aa.TotalAdvanceAmount) from (select t.AccountingOptionId,sum(t.AdvanceAmount) TotalAdvanceAmount  from  AdvancePaymentBills as t where t.ManufacturerId = a.ManufacturerId and t.AccountingOptionId in (select ap.Id from AccountingOptions ap where ap.ParentId= 10) and t.AuditedStatus = 'TRUE' and t.ReversedStatus = 'FALSE' group by t.AccountingOptionId) as aa   FOR XML PATH('') ), 1, 1, '')) ) as Accounts from AdvancePaymentBills as a inner join Manufacturer as m on a.ManufacturerId = m.Id where {whereQuery} and a.AuditedStatus = 'TRUE' and a.ReversedStatus = 'FALSE' group by a.ManufacturerId,m.Name";

                       //MYSQL
                       //string sqlString = $"select a.ManufacturerId,m.Name as ManufacturerName,IFNULL(sum(a.AdvanceAmount),0) as AdvancePaymentAmount, (select IFNULL(sum(pa.CollectionAmount),0) from PurchaseBills as p inner join PurchaseBill_Accounting_Mapping as pa on p.Id = pa.PurchaseBillId where p.ManufacturerId = a.ManufacturerId and p.AuditedStatus = 'TRUE' and p.ReversedStatus = 'FALSE' and pa.AccountingOptionId in (select ap.Id from AccountingOptions ap where ap.ParentId = 0)  {whereQuery2}) as PurchaseAdvancePaymentAmount, (select IFNULL(sum(pa.CollectionAmount),0) from PurchaseReturnBills as p inner join PurchaseReturnBill_Accounting_Mapping as pa on p.Id = pa.PurchaseReturnBillId where p.ManufacturerId = a.ManufacturerId and p.AuditedStatus = 'TRUE' and p.ReversedStatus = 'FALSE' and pa.AccountingOptionId in (select ap.Id from AccountingOptions ap where ap.ParentId = 0) {whereQuery2} ) as PurchaseReturnAdvancePaymentAmount,0.00 OverageAmount,(SELECT group_concat(CONCAT(alls.AccountingOptionId, '|', alls.TotalAdvanceAmount) separator ',') from(select t.AccountingOptionId, sum(t.AdvanceAmount) TotalAdvanceAmount  from AdvancePaymentBills as t where t.ManufacturerId = a.ManufacturerId and t.AccountingOptionId in (select ap.Id from AccountingOptions ap where ap.ParentId = 0) and t.AuditedStatus = 'TRUE' and t.ReversedStatus = 'FALSE' group by t.AccountingOptionId) as alls ) as Accounts from AdvancePaymentBills as a inner join Manufacturer as m on a.ManufacturerId = m.Id where {whereQuery} and a.AuditedStatus = TRUE and a.ReversedStatus = FALSE group by a.ManufacturerId,m.Name";
                       string sqlString = $"select a.ManufacturerId,m.Name as ManufacturerName,IFNULL(sum(a.AdvanceAmount),0) as AdvancePaymentAmount, ((select IFNULL(sum(pa.CollectionAmount),0) from PurchaseBills as p inner join PurchaseBill_Accounting_Mapping as pa on p.Id = pa.PurchaseBillId where p.ManufacturerId = a.ManufacturerId and p.AuditedStatus = TRUE and p.ReversedStatus = FALSE and pa.AccountingOptionId in (select ap.Id from AccountingOptions ap where ap.AccountCodeTypeId = 14)  {whereQuery2})+(SELECT IFNULL(SUM(pa.CollectionAmount), 0) FROM dcms.PaymentReceiptBills AS p INNER JOIN dcms.PaymentReceiptBill_Accounting_Mapping AS pa ON p.Id = pa.PaymentReceiptBillId WHERE p.ManufacturerId = a.ManufacturerId AND p.AuditedStatus = TRUE AND p.ReversedStatus = FALSE AND pa.AccountingOptionId IN(SELECT ap.Id FROM dcms.AccountingOptions ap WHERE ap.AccountCodeTypeId = 14) { whereQuery2})) as PurchaseAdvancePaymentAmount, (select IFNULL(sum(pa.CollectionAmount),0) from PurchaseReturnBills as p inner join PurchaseReturnBill_Accounting_Mapping as pa on p.Id = pa.PurchaseReturnBillId where p.ManufacturerId = a.ManufacturerId and p.AuditedStatus = TRUE and p.ReversedStatus = FALSE and pa.AccountingOptionId in (select ap.Id from AccountingOptions ap where ap.AccountCodeTypeId = 14) {whereQuery2} ) as PurchaseReturnAdvancePaymentAmount, 0.00 OverageAmount,(SELECT group_concat(CONCAT(alls.AccountingOptionId, '|', alls.TotalAdvanceAmount) separator ',') from(select t.AccountingOptionId, sum(t.AdvanceAmount) TotalAdvanceAmount  from AdvancePaymentBills as t where t.ManufacturerId = a.ManufacturerId and t.AccountingOptionId in (select ap.Id from AccountingOptions ap where ap.AccountCodeTypeId = 14) and t.AuditedStatus = TRUE and t.ReversedStatus = FALSE group by t.AccountingOptionId) as alls ) as Accounts from AdvancePaymentBills as a inner join Manufacturer as m on a.ManufacturerId = m.Id where {whereQuery} and a.AuditedStatus = TRUE and a.ReversedStatus = FALSE group by a.ManufacturerId,m.Name";

                   reporting = SaleBillsRepository_RO.QueryFromSql<FundReportAdvancePaymentOverage>(sqlString).ToList();
                   if (reporting != null)
                   {
                       reporting.ForEach(b =>
                       {
                           var accounts = new List<Accounting>();
                               //31|5200.0000,32|5200.0000
                               if (b.Accounts != null)
                           {
                               var acts = b.Accounts.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                               acts.ForEach(s =>
                               {
                                   var amounts = s.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                                   accounts.Add(new Accounting
                                   {
                                       AccountingOptionId = int.Parse(amounts[0]),
                                       CollectionAmount = decimal.Parse(amounts[1]),
                                   });
                               });
                           }


                           var accountOptions = _accountingService.GetAccountingOptionsByIds(storeId, accounts.Select(s => s.AccountingOptionId).ToArray()).Select(s => s).ToList();

                           accounts.ForEach(a =>
                           {
                               a.Name = accountOptions.Where(s => s.Id == a.AccountingOptionId).First().Name;
                           });

                           b.AccountingOptions = accounts;

                               //余额  =  所有预付款单金额总和 - 所有采购（使用预付款支付）的账户金额总和 +  所有采购退货（使用预付款支付）的账户金额总和
                               b.OverageAmount = b.AdvancePaymentAmount - b.PurchaseAdvancePaymentAmount + b.PurchaseReturnAdvancePaymentAmount;
                               //预收款账户
                               b.AccountingOptions = accounts ?? new List<Accounting>();

                       });
                   }
                   return reporting;
               });
            }
            catch (Exception)
            {
                return new List<FundReportAdvancePaymentOverage>();
            }
        }

    }
}

