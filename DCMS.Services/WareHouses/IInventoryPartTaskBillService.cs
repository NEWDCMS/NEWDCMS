using DCMS.Core;
using DCMS.Core.Domain.WareHouses;
using System;
using System.Collections.Generic;

namespace DCMS.Services.WareHouses
{
    public interface IInventoryPartTaskBillService
    {
        bool Exists(int billId);
        void DeleteInventoryPartTaskBill(InventoryPartTaskBill inventoryPartTaskBill);
        void DeleteInventoryPartTaskItem(InventoryPartTaskItem inventoryPartTaskItem);
        IList<InventoryPartTaskBill> GetAllInventoryPartTaskBills();
        IList<InventoryPartTaskBill> CheckInventory(int? store, int? inventoryPerson, int? wareHouseId);

        IList<InventoryPartTaskBill> CheckInventory(int? store, int? inventoryPerson, int? wareHouseId, int? productId);

        IPagedList<InventoryPartTaskBill> GetAllInventoryPartTaskBills(int? store, int? makeuserId, int? inventoryPerson, int? wareHouseId, string billNumber = "", bool? status = null, DateTime? start = null, DateTime? end = null, int? inventoryStatus = -1, bool? showReverse = null, bool? sortByCompletedTime = null, string remark = "", bool? deleted = null, int pageIndex = 0, int pageSize = int.MaxValue);

        InventoryPartTaskBill GetInventoryPartTaskBillById(int? store, int inventoryPartTaskBillId);
        InventoryPartTaskBill GetInventoryPartTaskBillById(int? store, int inventoryPartTaskBillId, bool isInclude = false);
        InventoryPartTaskBill GetInventoryPartTaskBillByNumber(int? store, string billNumber);
        InventoryPartTaskItem GetInventoryPartTaskItemById(int? store, int inventoryPartTaskItemId);
        IPagedList<InventoryPartTaskItem> GetInventoryPartTaskItemsByInventoryPartTaskBillId(int inventoryPartTaskBillId, int? userId, int? storeId, int pageIndex, int pageSize);
        List<InventoryPartTaskItem> GetInventoryPartTaskItemList(int inventoryPartTaskBillId);

        void InsertInventoryPartTaskBill(InventoryPartTaskBill inventoryPartTaskBill);
        void InsertInventoryPartTaskItem(InventoryPartTaskItem inventoryPartTaskItem);
        void UpdateInventoryPartTaskBill(InventoryPartTaskBill inventoryPartTaskBill);
        void UpdateInventoryPartTaskItem(InventoryPartTaskItem inventoryPartTaskItem);

        void UpdateInventoryPartTaskBillActive(int? store, int? billId, int? user);

        BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, InventoryPartTaskBill inventoryPartTaskBill, InventoryPartTaskBillUpdate data, List<InventoryPartTaskItem> items, out int inventoryPartTaskBillId, bool isAdmin = false);

        BaseResult CancelTakeInventory(int storeId, int userId, InventoryPartTaskBill inventoryPartTaskBill);
        BaseResult SetInventoryCompleted(int storeId, int userId, InventoryPartTaskBill inventoryPartTaskBill);

    }
}