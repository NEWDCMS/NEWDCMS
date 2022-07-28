using DCMS.Core;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Finances;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Finances
{
    public interface IFinancialIncomeBillService
    {
        bool Exists(int billId);
        void DeleteFinancialIncomeBill(FinancialIncomeBill financialIncomeBill);
        void DeleteFinancialIncomeBillAccounting(FinancialIncomeBillAccounting financialIncomeBillAccounting);
        void DeleteFinancialIncomeItem(FinancialIncomeItem financialIncomeItem);
        IList<FinancialIncomeBill> GetAllFinancialIncomeBills();
        IPagedList<FinancialIncomeBill> GetAllFinancialIncomeBills(int? store, int? makeuserId, int? salesmanId, int? customerId, int? manufacturerId, string billNumber = "", bool? status = null, DateTime? start = null, DateTime? end = null, bool? isShowReverse = null, bool? sortByAuditedTime = null, string remark = "", int pageIndex = 0, int pageSize = int.MaxValue);
        FinancialIncomeBillAccounting GetFinancialIncomeBillAccountingById(int financialIncomeBillAccountingId);
        IList<FinancialIncomeBillAccounting> GetFinancialIncomeBillAccountingsByFinancialIncomeBillId(int? store, int financialIncomeBillId);
        IPagedList<FinancialIncomeBillAccounting> GetFinancialIncomeBillAccountingsByFinancialIncomeBillId(int storeId, int userId, int financialIncomeBillId, int pageIndex, int pageSize);

        FinancialIncomeBill GetFinancialIncomeBillById(int? store, int financialIncomeBillId);
        FinancialIncomeBill GetFinancialIncomeBillById(int? store, int financialIncomeBillId, bool isInclude = false);
        FinancialIncomeItem GetFinancialIncomeItemById(int? store, int financialIncomeItemId);
        IPagedList<FinancialIncomeItem> GetFinancialIncomeItemsByFinancialIncomeBillId(int financialIncomeBillId, int? userId, int? storeId, int pageIndex, int pageSize);
        List<FinancialIncomeItem> GetFinancialIncomeItemList(int financialIncomeBillId);

        void InsertFinancialIncomeBill(FinancialIncomeBill financialIncomeBill);
        void InsertFinancialIncomeBillAccounting(FinancialIncomeBillAccounting financialIncomeBillAccounting);
        void InsertFinancialIncomeItem(FinancialIncomeItem financialIncomeItem);
        void UpdateFinancialIncomeBill(FinancialIncomeBill financialIncomeBill);
        void UpdateFinancialIncomeBillAccounting(FinancialIncomeBillAccounting financialIncomeBillAccounting);
        void UpdateFinancialIncomeItem(FinancialIncomeItem financialIncomeItem);

        BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, FinancialIncomeBill financialIncomeBill, List<FinancialIncomeBillAccounting> accountingOptions, List<AccountingOption> accountings, FinancialIncomeBillUpdate data, List<FinancialIncomeItem> items, bool isAdmin = false,bool doAudit = true);

        BaseResult Auditing(int storeId, int userId, FinancialIncomeBill financialIncomeBill);
        BaseResult Reverse(int userId, FinancialIncomeBill financialIncomeBill);

        void UpdatePaymented(int? store, int billId, PayStatus payStatus);
        void UpdateReceived(int? store, int billId, ReceiptStatus receiptStatus);
    }
}