using DCMS.Core;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Finances;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Finances
{
    public interface ICostExpenditureBillService
    {
        bool Exists(int billId);
        void DeleteCostExpenditureBill(CostExpenditureBill costExpenditureBill);
        void DeleteCostExpenditureBillAccounting(CostExpenditureBillAccounting costExpenditureBillAccounting);
        void DeleteCostExpenditureItem(CostExpenditureItem costExpenditureItem);
        IList<CostExpenditureBill> GetAllCostExpenditureBills();
        CostExpenditureBill GetCostExpenditureBillByNumber(int? store, string billNumber);
        IPagedList<CostExpenditureBill> GetAllCostExpenditureBills(int? store, int? makeuserId, int? employeeId, int? terminalId, string billNumber = "", bool? status = null, DateTime? start = null, DateTime? end = null, bool? isShowReverse = null, bool? sortByAuditedTime = null, bool? deleted = null, bool? handleStatus = null, int? sign = null, int pageIndex = 0, int pageSize = int.MaxValue);
        CostExpenditureBillAccounting GetCostExpenditureBillAccountingById(int costExpenditureBillAccountingId);
        IList<CostExpenditureBillAccounting> GetCostExpenditureBillAccountingsByCostExpenditureBillId(int costExpenditureBillId);
        IPagedList<CostExpenditureBillAccounting> GetCostExpenditureBillAccountingsByCostExpenditureBillId(int storeId, int userId, int costExpenditureBillId, int pageIndex, int pageSize);

        IList<CostExpenditureBillAccounting> GetAllCostExpenditureBillAccountingsByBillIds(int[] billIds);

        CostExpenditureBill GetCostExpenditureBillById(int? store, int costExpenditureBillId);
        CostExpenditureBill GetCostExpenditureBillById(int? store, int costExpenditureBillId, bool isInclude = false);


        CostExpenditureItem GetCostExpenditureItemById(int? store, int costExpenditureItemId);
        IPagedList<CostExpenditureItem> GetCostExpenditureItemsByCostExpenditureBillId(int costExpenditureBillId, int? userId, int? storeId, int pageIndex, int pageSize);
        List<CostExpenditureItem> GetCostExpenditureItemList(int costExpenditureBillId);

        void InsertCostExpenditureBill(CostExpenditureBill costExpenditureBill);
        void InsertCostExpenditureBillAccounting(CostExpenditureBillAccounting costExpenditureBillAccounting);
        void InsertCostExpenditureItem(CostExpenditureItem costExpenditureItem);
        void UpdateCostExpenditureBill(CostExpenditureBill costExpenditureBill);
        void UpdateCostExpenditureBillAccounting(CostExpenditureBillAccounting costExpenditureBillAccounting);
        void UpdateCostExpenditureItem(CostExpenditureItem costExpenditureItem);

        IList<CostExpenditureItem> GetCostExpenditureItemByCostContractId(int? store, int? costContractId);

        void UpdateCostExpenditureBillActive(int? store, int? billId, int? user);

        IList<CostExpenditureBill> GetCostExpenditureBillListToFinanceReceiveAccount(int? storeId, int? employeeId = null, DateTime? start = null, DateTime? end = null);

        BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, CostExpenditureBill costExpenditureBill, List<CostExpenditureBillAccounting> accountingOptions, List<AccountingOption> accountings, CostExpenditureBillUpdate data, List<CostExpenditureItem> items, bool isAdmin = false,bool doAudit = true);

        BaseResult Auditing(int userId, CostExpenditureBill costExpenditureBill);
        BaseResult Reverse(int userId, CostExpenditureBill costExpenditureBill);

        void UpdateReceived(int? store, int billId, ReceiptStatus receiptStatus);
        void UpdateHandInStatus(int? store, int billId, bool handInStatus);

    }
}