using DCMS.Core;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Finances;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Finances
{
    public interface ICashReceiptBillService
    {
        bool Exists(int billId);
        void DeleteCashReceiptBill(CashReceiptBill cashReceiptBill);
        void DeleteCashReceiptItem(CashReceiptItem cashReceiptItem);
        IList<CashReceiptBill> GetAllCashReceiptBills();
        IPagedList<CashReceiptBill> GetAllCashReceiptBills(int? store, int? makeuserId, int? customerId, int? payeerId, string billNumber = "", bool? status = null, DateTime? start = null, DateTime? end = null, bool? isShowReverse = null, bool? sortByAuditedTime = null, string remark = "", bool? deleted = null, bool? handleStatus = null, int pageIndex = 0, int pageSize = int.MaxValue);

        CashReceiptBill GetCashReceiptBillById(int? store, int cashReceiptBillId);
        CashReceiptBill GetCashReceiptBillById(int? store, int cashReceiptBillId, bool isInclude = false);
        CashReceiptBill GetCashReceiptBillByNumber(int? store, string billNumber);
        CashReceiptItem GetCashReceiptItemById(int? store, int cashReceiptItemId);
        IPagedList<CashReceiptItem> GetCashReceiptItemsByCashReceiptBillId(int cashReceiptBillId, int? userId, int? storeId, int pageIndex, int pageSize);
        List<CashReceiptItem> GetCashReceiptItemList(int cashReceiptBillId);
        void InsertCashReceiptBill(CashReceiptBill cashReceiptBill);
        void InsertCashReceiptItem(CashReceiptItem cashReceiptItem);
        void UpdateCashReceiptBill(CashReceiptBill cashReceiptBill);
        void UpdateCashReceiptItem(CashReceiptItem cashReceiptItem);

        bool CheckTerminalCashReceiptSettled(int storeId, int? terminalId, decimal oweCaseAmount);
        IPagedList<CashReceiptBillAccounting> GetCashReceiptBillAccountingsByCashReceiptBillId(int storeId, int userId, int cashReceiptBillId, int pageIndex, int pageSize);

        IList<CashReceiptBillAccounting> GetAllCashReceiptBillAccountingsByBillIds(int? store, int[] billIds);

        IList<CashReceiptBillAccounting> GetCashReceiptBillAccountingsByCashReceiptBillId(int? store, int cashReceiptBillId);
        CashReceiptBillAccounting GetCashReceiptBillAccountingById(int cashReceiptBillAccountingId);
        void InsertCashReceiptBillAccounting(CashReceiptBillAccounting cashReceiptBillAccounting);
        void UpdateCashReceiptBillAccounting(CashReceiptBillAccounting cashReceiptBillAccounting);
        void DeleteCashReceiptBillAccounting(CashReceiptBillAccounting cashReceiptBillAccounting);

        void UpdateCashReceiptBillActive(int? store, int? billId, int? user);

        IList<CashReceiptBill> GetCashReceiptBillListToFinanceReceiveAccount(int? storeId, int? employeeId = null, DateTime? start = null, DateTime? end = null);

        BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, CashReceiptBill cashReceiptBill, List<CashReceiptBillAccounting> accountingOptions, List<AccountingOption> accountings, CashReceiptBillUpdate data, List<CashReceiptItem> items, bool isAdmin = false, bool doAudit = true);

        BaseResult Auditing(int storeId, int userId, CashReceiptBill cashReceiptBill);
        BaseResult Reverse(int userId, CashReceiptBill cashReceiptBill);

        /// <summary>
        /// 验证单据是否已经收款
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="billTypeId"></param>
        /// <param name="billNumber"></param>
        /// <returns></returns>
        Tuple<bool, string> CheckBillCashReceipt(int storeId, int billTypeId, string billNumber);


        /// <summary>
        /// 获取待收款欠款单据
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="payeer"></param>
        /// <param name="terminalId"></param>
        /// <param name="billTypeId"></param>
        /// <param name="billNumber"></param>
        /// <param name="remark"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        IList<BillCashReceiptSummary> GetBillCashReceiptSummary(int storeId, int? payeer,
            int? terminalId,
            int? billTypeId,
            string billNumber = "",
            string remark = "",
            DateTime? startTime = null,
            DateTime? endTime = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue);

        /// <summary>
        /// 判断指定单据是否尚有欠款(是否已经收完款)
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="billTypeId"></param>
        /// <param name="billId"></param>
        /// <returns></returns>
        bool ThereAnyDebt(int storeId, int? billTypeId, int billId);

        void UpdateHandInStatus(int? store, int billId, bool handInStatus);

        /// <summary>
        /// 获取待收款欠款单据
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="userIds"></param>
        /// <param name="terminalId"></param>
        /// <param name="billTypeId"></param>
        /// <param name="billNumber"></param>
        /// <param name="remark"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>userIds
        /// <returns></returns>
        IList<BillCashReceiptSummary> GetBillCashReceiptList(int storeId, IList<int> userIds,
            int? terminalId,
            int? billTypeId,
            string billNumber = "",
            string remark = "",
            DateTime? startTime = null,
            DateTime? endTime = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue);
        /// <summary>
        /// 是否存在未审核单据(根据单据编号)
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="billNumber"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        bool ExistsUnAuditedByBillNumber(int storeId,string billNumber, int id);
        /// <summary>
        /// 根据单据编号获取收款明细
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="billId"></param>
        /// <returns></returns>
        List<CashReceiptItem> GetCashReceiptItemListByBillId(int storeId, int billId);
    }
}