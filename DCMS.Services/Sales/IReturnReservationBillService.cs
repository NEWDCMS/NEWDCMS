using DCMS.Core;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Domain.WareHouses;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Sales
{
    public interface IReturnReservationBillService
    {
        bool Exists(int billId);
        void DeleteReturnReservationBill(ReturnReservationBill returnReservationBill);
        IPagedList<ReturnReservationBill> GetReturnReservationBillList(int? store, int? makeuserId, int? terminalId, string terminalName, int? businessUserId, int? deliveryUserId, string billNumber = "", int? wareHouseId = null, string remark = "", DateTime? start = null, DateTime? end = null, int? districtId = null, bool? auditedStatus = null, bool? sortByAuditedTime = null, bool? showReverse = null, bool? alreadyChange = null, bool? deleted = null, int pageIndex = 0, int pageSize = int.MaxValue);
        IList<ReturnReservationBill> GetReturnReservationBillsByStoreId(int? storeId);

        IList<ReturnReservationBill> GetReturnReservationBillByStoreIdTerminalId(int storeId, int terminalId);

        IList<ReturnReservationBill> GetReturnReservationBillsNullWareHouseByStoreId(int storeId);

        IList<ReturnReservationBill> GetHotSaleReservationRanking(int? store, int? terminalId, int? businessUserId, DateTime? startTime, DateTime? endTime);

        void InsertReturnReservationBill(ReturnReservationBill returnReservationBill);
        ReturnReservationBill GetReturnReservationBillById(int? store, int returnReservationBillId, bool isInclude = false);

        IList<ReturnReservationBill> GetReturnReservationBillsByIds(int[] sIds, bool isInclude = false);
        int GetBillId(int? store, string billNumber);
        ReturnReservationBill GetReturnReservationBillByNumber(int? store, string billNumber);
        void UpdateReturnReservationBill(ReturnReservationBill returnReservationBill);
        void UpdateReturnReservationBill(IList<ReturnReservationBill> returnReservationBills);
        void ChangedBill(int billId, int userId);
        IList<ReturnReservationBill> GetReturnReservationBillListToFinanceReceiveAccount(int? storeId, bool status = false, DateTime? start = null, DateTime? end = null, int? businessUserId = null);
        void SetReturnReservationBillAmount(int returnReservationBillId);

        void DeleteReturnReservationItem(ReturnReservationItem returnReservationItem);
        List<ReturnReservationItem> GetReturnReservationItemList(int saleId);
        IList<ReturnReservationItem> GetReturnReservationItemByReturnReservationId(int returnReservationBillId, int? userId, int storeId, int pageIndex, int pageSize);
        void UpdateReturnReservationItem(ReturnReservationItem returnReservationItem);
        ReturnReservationItem GetReturnReservationItemById(int returnReservationItemId);
        void InsertReturnReservationItem(ReturnReservationItem returnReservationItem);
        int ReturnReservationItemQtySum(int storeId, int productId, int saleId);

        IPagedList<ReturnReservationBillAccounting> GetReturnReservationBillAccountingsByReturnReservationId(int storeId, int userId, int returnReservationBillId, int pageIndex, int pageSize);
        IList<ReturnReservationBillAccounting> GetReturnReservationBillAccountingsByReturnReservationId(int? store, int returnReservationBillId);
        ReturnReservationBillAccounting GetReturnReservationBillAccountingById(int returnReservationAccountingId);
        void InsertReturnReservationBillAccounting(ReturnReservationBillAccounting returnReservationAccounting);
        void UpdateReturnReservationBillAccounting(ReturnReservationBillAccounting returnReservationAccounting);
        void DeleteReturnReservationBillAccounting(ReturnReservationBillAccounting returnReservationAccounting);

        //退货订单转退货单查询
        IPagedList<ReturnReservationBill> GetReturnReservationBillToChangeList(int? storeId, DateTime? start = null, DateTime? end = null, int? businessUserId = null, int? deliveryUserId = null, string billNumber = "", string remark = "", bool? changedStatus = null, bool? dispatchedStatus = null, int pageIndex = 0, int pageSize = int.MaxValue);

        //车辆对货单查询
        IList<ReturnReservationBill> GetReturnReservationBillsToCarGood(int? storeId, int? makeuserId, int? deliveryUserId, DateTime? start = null, DateTime? end = null);

        //装车调度查询
        IList<ReturnReservationBill> GetReturnReservationBillToDispatch(int? storeId, int? makeuserId, DateTime? start = null, DateTime? end = null, int? businessUserId = null, int? districtId = null, int? terminalId = null,
                   string billNumber = "", int? deliveryUserId = null, int? channelId = null, int? rankId = null, int? billTypeId = null, bool? showDispatchReserved = null, bool? dispatchStatus = null);

        IList<ReturnReservationBillAccounting> GetAllReservationBillAccountingsByBillIds(int? store, int[] billIds);



        void UpdateReturnReservationBillActive(int? store, int? billId, int? user);

        BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, ReturnReservationBill returnReservationBill, List<ReturnReservationBillAccounting> accountingOptions, List<AccountingOption> accountings, ReturnReservationBillUpdate data, List<ReturnReservationItem> items, List<ProductStockItem> productStockItemThiss, bool isAdmin = false, bool doAudit = true);

        BaseResult Auditing(int storeId, int userId, ReturnReservationBill returnReservationBill);
        BaseResult Reverse(int userId, ReturnReservationBill returnReservationBill);
        BaseResult Delete(int userId, ReturnReservationBill returnReservationBill);
    }
}
