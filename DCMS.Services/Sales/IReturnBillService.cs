using DCMS.Core;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Domain.WareHouses;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Sales
{
    public interface IReturnBillService
    {
        bool Exists(int billId);
        void DeleteReturnBill(ReturnBill r);
        IPagedList<ReturnBill> GetReturnBillList(int? store, int? makeuserId, int? terminalId, string terminalName, int? businessUserId, int? deliveryUserId, string billNumber = "", int? wareHouseId = null, string remark = "", DateTime? start = null, DateTime? end = null, int? districtId = null, bool? auditedStatus = null, bool? sortByAuditedTime = null, bool? showReverse = null, int? paymentMethodType = null, int? billSourceType = null, bool? receipted = null, bool? deleted = null, bool? handleStatus = null, int? productId = 0, int pageIndex = 0, int pageSize = int.MaxValue);
        IList<ReturnBill> GetReturnBillsByStoreId(int? storeId, bool? auditedStatus = null, bool? reversedStatus = null);
        IList<ReturnBill> GetReturnBillsByStoreId(int? storeId, bool? auditedStatus = null, bool? reversedStatus = null, int? businessUserId = null, DateTime? startTime = null, DateTime? endTime = null);
        IList<ReturnBill> GetReturnBillsByStoreId(int? storeId, bool? auditedStatus = null, bool? reversedStatus = null, DateTime? beginDate = null);

        IList<ReturnBill> GetHotSaleRanking(int? store, int? terminalId, int? businessUserId, DateTime? startTime, DateTime? endTime);
        IList<ReturnBill> GetCostProfitRanking(int? store, int? terminalId, int? businessUserId, DateTime? startTime, DateTime? endTime);

        IList<ReturnBill> GetReturnBillByStoreIdTerminalId(int storeId, int terminalId);

        void UpdateReturnBillReceipted(string billNumber);
        void UpdateReturnBillOweCash(string billNumber, decimal oweCash);

        void InsertReturnBill(ReturnBill returnBill);
        ReturnBill GetReturnBillById(int? store, int returnBillId);
        ReturnBill GetReturnBillById(int? store, int returnBillId, bool isInclude = false);
        ReturnBill GetReturnBillByReturnReservationBillId(int? store, int returnReservationBillId);

        /// <summary>
        /// 根据退货订单ids 获取退货单
        /// </summary>
        /// <param name="sIds"></param>
        /// <returns></returns>
        IList<ReturnBill> GetReturnBillsByReturnReservationIds(int? store, int[] sIds);
        int GetBillId(int? store, string billNumber);
        ReturnBill GetReturnBillByNumber(int? store, string billNumber);
        void UpdateReturnBill(ReturnBill returnBill);

        void SetReturnBillAmount(int returnBillId);

        void DeleteReturnItem(ReturnItem returnItem);
        List<ReturnItem> GetReturnItemList(int returnBillId);
        IList<ReturnItem> GetReturnItemByReturnBillId(int returnBillId, int? userId, int? storeId, int pageIndex, int pageSize);
        void UpdateReturnItem(ReturnItem returnItem);
        ReturnItem GetReturnItemById(int returnItemId);
        void InsertReturnItem(ReturnItem returnItem);
        int ReturnItemQtySum(int storeId, int productId, int returnBillId);

        IPagedList<ReturnBillAccounting> GetReturnBillAccountingsByReturnBillId(int storeId, int userId, int returnBillId, int pageIndex, int pageSize);
        IList<ReturnBillAccounting> GetReturnBillAccountingsByReturnBillId(int? store, int returnBillId);
        ReturnBillAccounting GetReturnBillAccountingById(int returnBillAccountingId);
        void InsertReturnBillAccounting(ReturnBillAccounting returnBillAccounting);
        void UpdateReturnBillAccounting(ReturnBillAccounting returnBillAccounting);
        void DeleteReturnBillAccounting(ReturnBillAccounting returnBillAccounting);

        IList<ReturnBill> GetReturnBillListToFinanceReceiveAccount(int? storeId, bool status = false, DateTime? start = null, DateTime? end = null, int? businessUserId = null, int? payeer = null);

        IList<ReturnBillAccounting> GetAllReturnBillAccountingsByBillIds(int? store, int[] billIds);

        void UpdateReturnBillActive(int? store, int? billId, int? user);

        IList<ReturnBill> GetReturnBillListToFinanceReceiveAccount(int? storeId, int? employeeId = null, DateTime? start = null, DateTime? end = null);

        IList<BaseItem> GetReturnBillsByBusinessUsers(int? storeId, bool? auditedStatus = null, bool? reversedStatus = null, int[] businessUserIds = null, DateTime? startTime = null, DateTime? endTime = null);
        IList<BaseItem> GetReturnBillsByDeliveryUsers(int? storeId, bool? auditedStatus = null, bool? reversedStatus = null, int[] deliveryUserIds = null, DateTime? startTime = null, DateTime? endTime = null);

        BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, ReturnBill returnBill, List<ReturnBillAccounting> accountingOptions, List<AccountingOption> accountings, ReturnBillUpdate data, List<ReturnItem> items, List<ProductStockItem> productStockItemThiss, bool isAdmin = false,bool doAudit = true);

        BaseResult Auditing(int storeId, int userId, ReturnBill returnBill);
        BaseResult Reverse(int userId, ReturnBill returnBill);
        BaseResult Delete(int userId, ReturnBill returnBill);

        void UpdateReceived(int? store, int billId, ReceiptStatus receiptStatus);
        void UpdateHandInStatus(int? store, int billId, bool handInStatus);
    }
}
