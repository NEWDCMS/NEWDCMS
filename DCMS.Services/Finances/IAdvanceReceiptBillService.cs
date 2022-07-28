using DCMS.Core;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Finances;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Finances
{
    public interface IAdvanceReceiptBillService
    {
        bool Exists(int billId);
        void DeleteAdvanceReceiptBill(AdvanceReceiptBill advanceReceiptBill);
        void DeleteAdvanceReceiptBillAccounting(AdvanceReceiptBillAccounting advanceReceiptBillAccounting);
        AdvanceReceiptBillAccounting GetAdvanceReceiptBillAccountingById(int advanceReceiptBillAccountingId);

        IList<AdvanceReceiptBill> GetAdvanceReceiptBillByStoreIdTerminalId(int storeId, int terminalId);

        IList<AdvanceReceiptBillAccounting> GetAdvanceReceiptBillAccountingsByAdvanceReceiptBillId(int? store, int advanceReceiptBillId);
        IPagedList<AdvanceReceiptBillAccounting> GetAdvanceReceiptBillAccountingsByAdvanceReceiptBillId(int storeId, int userId, int advanceReceiptBillId, int pageIndex, int pageSize);
        AdvanceReceiptBill GetAdvanceReceiptBillById(int? store, int advanceReceiptBillId);
        AdvanceReceiptBill GetAdvanceReceiptBillById(int? store, int advanceReceiptBillId, bool isInclude = false);
        AdvanceReceiptBill GetAdvanceReceiptBillByNumber(int? store, string billNumber);

        IList<AdvanceReceiptBill> GetAllAdvanceReceiptBills();
        IPagedList<AdvanceReceiptBill> GetAllAdvanceReceiptBills(int? store, int? makeuserId, int? customerId, string customerName, int? payeerId, string billNumber = "", bool? status = null, DateTime? start = null, DateTime? end = null, bool? isShowReverse = null, bool? sortByAuditedTime = null, int? accountingOptionId = null, bool? deleted = null, bool? handleStatus = null, int pageIndex = 0, int pageSize = int.MaxValue);
        void InsertAdvanceReceiptBill(AdvanceReceiptBill advanceReceiptBill);
        void InsertAdvanceReceiptBillAccounting(AdvanceReceiptBillAccounting advanceReceiptBillAccounting);
        void UpdateAdvanceReceiptBill(AdvanceReceiptBill advanceReceiptBill);
        void UpdateAdvanceReceiptBillAccounting(AdvanceReceiptBillAccounting advanceReceiptBillAccounting);

        void UpdateAdvanceReceiptBillActive(int? store, int? billId, int? user);

        IList<AdvanceReceiptBill> GetAdvanceReceiptBillListToFinanceReceiveAccount(int? storeId, int? employeeId = null, DateTime? start = null, DateTime? end = null);

        BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, AdvanceReceiptBill advanceReceiptBill, List<AdvanceReceiptBillAccounting> accountingOptions, List<AccountingOption> accountings, AdvanceReceiptBillUpdate data, bool isAdmin = false,bool doAudit = true);

        BaseResult Auditing(int storeId, int userId, AdvanceReceiptBill advanceReceiptBill);
        BaseResult Reverse(int userId, AdvanceReceiptBill advanceReceiptBill);


        void UpdateReceived(int? store, int billId, ReceiptStatus receiptStatus);
        void UpdateHandInStatus(int? store, int billId, bool handInStatus);

    }
}