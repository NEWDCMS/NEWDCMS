using DCMS.Core;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Finances;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Finances
{
    public interface IPaymentReceiptBillService
    {
        bool Exists(int billId);
        void DeletePaymentReceiptBill(PaymentReceiptBill paymentReceiptBill);
        void DeletePaymentReceiptBillAccounting(PaymentReceiptBillAccounting paymentReceiptBillAccounting);
        void DeletePaymentReceiptItem(PaymentReceiptItem paymentReceiptItem);
        IList<PaymentReceiptBill> GetAllPaymentReceiptBills();
        IPagedList<PaymentReceiptBill> GetAllPaymentReceiptBills(int? store, int? makeuserId, int? draweer, int? manufacturerId, string billNumber = "", bool? status = null, DateTime? start = null, DateTime? end = null, bool? isShowReverse = null, bool? sortByAuditedTime = null, int pageIndex = 0, int pageSize = int.MaxValue);
        PaymentReceiptBillAccounting GetPaymentReceiptBillAccountingById(int paymentReceiptBillAccountingId);
        IList<PaymentReceiptBillAccounting> GetPaymentReceiptBillAccountingsByPaymentReceiptBillId(int? store, int paymentReceiptBillId);
        IPagedList<PaymentReceiptBillAccounting> GetPaymentReceiptBillAccountingsByPaymentReceiptBillId(int storeId, int userId, int paymentReceiptBillId, int pageIndex, int pageSize);

        PaymentReceiptBill GetPaymentReceiptBillById(int? store, int paymentReceiptBillId);
        PaymentReceiptBill GetPaymentReceiptBillById(int? store, int paymentReceiptBillId, bool isInclude = false);
        PaymentReceiptBill GetPaymentReceiptBillNumber(int? store, string billNumber);
        PaymentReceiptItem GetPaymentReceiptItemById(int? store, int paymentReceiptItemId);
        IPagedList<PaymentReceiptItem> GetPaymentReceiptItemsByPaymentReceiptBillId(int paymentReceiptBillId, int? userId, int? storeId, int pageIndex, int pageSize);
        List<PaymentReceiptItem> GetPaymentReceiptItemList(int paymentReceiptBillId);

        void InsertPaymentReceiptBill(PaymentReceiptBill paymentReceiptBill);
        void InsertPaymentReceiptBillAccounting(PaymentReceiptBillAccounting paymentReceiptBillAccounting);
        void InsertPaymentReceiptItem(PaymentReceiptItem paymentReceiptItem);
        void UpdatePaymentReceiptBill(PaymentReceiptBill paymentReceiptBill);
        void UpdatePaymentReceiptBillAccounting(PaymentReceiptBillAccounting paymentReceiptBillAccounting);
        void UpdatePaymentReceiptItem(PaymentReceiptItem paymentReceiptItem);

        BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, PaymentReceiptBill paymentReceiptBill, List<PaymentReceiptBillAccounting> accountingOptions, List<AccountingOption> accountings, PaymentReceiptBillUpdate data, List<PaymentReceiptItem> items, bool isAdmin = false,bool doAudit = true);

        BaseResult Auditing(int storeId, int userId, PaymentReceiptBill paymentReceiptBill);
        BaseResult Reverse(int userId, PaymentReceiptBill paymentReceiptBill);

        /// <summary>
        /// 验证单据是否已经付款
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="billTypeId"></param>
        /// <param name="billNumber"></param>
        /// <returns></returns>
        Tuple<bool, string> CheckBillPaymentReceipt(int storeId, int billTypeId, string billNumber);

        IList<BillCashReceiptSummary> GetBillPaymentReceiptSummary(int storeId, int? payeer,
            int? manufacturerId,
            int? billTypeId,
            string billNumber = "",
            string remark = "",
            DateTime? startTime = null,
            DateTime? endTime = null);

        /// <summary>
        /// 判断指定单据是否尚有欠款
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="billTypeId"></param>
        /// <param name="billId"></param>
        /// <returns></returns>
        bool ThereAnyDebt(int storeId, int? billTypeId, int billId);

    }
}