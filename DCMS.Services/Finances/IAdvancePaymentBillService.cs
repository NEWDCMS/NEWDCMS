using DCMS.Core;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Finances;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Finances
{
    public interface IAdvancePaymentBillService
    {
        bool Exists(int billId);
        void DeleteAdvancePaymentBill(AdvancePaymentBill advancePaymentBill);
        void DeleteAdvancePaymentBillAccounting(AdvancePaymentBillAccounting advancePaymentBillAccounting);
        AdvancePaymentBillAccounting GetAdvancePaymentBillAccountingById(int advancePaymentBillAccountingId);
        AdvancePaymentBill GetAdvancePaymentBillByNumber(int? store, string billNumber);
        IList<AdvancePaymentBill> GetAdvancePaymentBillByStoreIdManufacturerId(int storeId, int manufacturerId);
        decimal GetAdvanceAmountByManufacturerId(int storeId, int manufacturerId);
        IList<AdvancePaymentBillAccounting> GetAdvancePaymentBillAccountingsByAdvancePaymentBillId(int? store, int advancePaymentBillId);
        IPagedList<AdvancePaymentBillAccounting> GetAdvancePaymentBillAccountingsByAdvancePaymentBillId(int storeId, int userId, int advancePaymentBillId, int pageIndex, int pageSize);
        AdvancePaymentBill GetAdvancePaymentBillById(int? store, int advancePaymentBillId);
        AdvancePaymentBill GetAdvancePaymentBillById(int? store, int advancePaymentBillId, bool isInclude = false);
        IList<AdvancePaymentBill> GetAllAdvancePaymentBills();
        IPagedList<AdvancePaymentBill> GetAllAdvancePaymentBills(int? store, int? makeuserId, int? draweer, int? manufacturerId, string billNumber = "", bool? status = null, DateTime? start = null, DateTime? end = null, bool? isShowReverse = null, bool? sortByAuditedTime = null, int? accountingOptionId = null, int pageIndex = 0, int pageSize = int.MaxValue);
        void InsertAdvancePaymentBill(AdvancePaymentBill advancePaymentBill);
        void InsertAdvancePaymentBillAccounting(AdvancePaymentBillAccounting advancePaymentBillAccounting);
        void UpdateAdvancePaymentBill(AdvancePaymentBill advancePaymentBill);
        void UpdateAdvancePaymentBillAccounting(AdvancePaymentBillAccounting advancePaymentBillAccounting);

        BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, AdvancePaymentBill advancePaymentBill, List<AdvancePaymentBillAccounting> accountingOptions, List<AccountingOption> accountings, AdvancePaymenBillUpdate data, bool isAdmin = false,bool doAudit = true);

        BaseResult Auditing(int storeId, int userId, AdvancePaymentBill advancePaymentBill);
        BaseResult Reverse(int userId, AdvancePaymentBill advancePaymentBill);

    }
}