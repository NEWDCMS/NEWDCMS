using DCMS.Core;
using DCMS.Core.Domain.WareHouses;
using System;
using System.Collections.Generic;

namespace DCMS.Services.WareHouses
{
    public interface IAllocationBillService
    {
        bool Exists(int billId);
        void DeleteAllocationBill(AllocationBill allocationBill);
        void DeleteAllocationItem(AllocationItem allocationItem);
        IList<AllocationBill> GetAllAllocationBills();
        IPagedList<AllocationBill> GetAllAllocationBills(int? store, int? makeuserId, int? businessUserId, int? shipmentWareHouseId, int? IncomeWareHouseId, string billNumber = "", bool? status = null, DateTime? start = null, DateTime? end = null, bool? isShowReverse = null, bool? deleted = null, int? productId = 0, int pageIndex = 0, int pageSize = int.MaxValue);

        AllocationBill GetAllocationBillById(int? store, int allocationBillId);
        AllocationBill GetAllocationBillById(int? store, int allocationBillId, bool isInclude=false);
        AllocationBill GetAllocationBillByNumber(int? store, string billNumber);
        AllocationItem GetAllocationItemById(int? store, int allocationItemId);
        IPagedList<AllocationItem> GetAllocationItemsByAllocationBillId(int allocationBillId, int? userId, int? storeId, int pageIndex, int pageSize);
        List<AllocationItem> GetAllocationItemList(int allocationBillId);

        void InsertAllocationBill(AllocationBill allocationBill);
        void InsertAllocationItem(AllocationItem allocationItem);
        void UpdateAllocationBill(AllocationBill allocationBill);
        void UpdateAllocationItem(AllocationItem allocationItem);


        IList<QuickAllocationItem> GetQuickAllocation(int? storeId, int allocationType, int wareHouseId, int deliveryUserId, string categoryIds, string loadDataNameIds);

        void UpdateAllocationBillActive(int? store, int? billId, int? user);


        BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, AllocationBill allocationBill, AllocationBillUpdate data, List<AllocationItem> items, List<ProductStockItem> productStockItemThiss, bool isAdmin = false, bool doAudit = true);

        BaseResult Auditing(int storeId, int userId, AllocationBill allocationBill);
        BaseResult Reverse(int userId, AllocationBill allocationBill);

        BaseResult Delete(int userId, AllocationBill allocationBill);

    }
}