using DCMS.Core;
using DCMS.Core.Domain.WareHouses;
using System;
using System.Collections.Generic;

namespace DCMS.Services.WareHouses
{
    public interface IInventoryAllTaskBillService
    {
        bool Exists(int billId);
        void DeleteInventoryAllTaskBill(InventoryAllTaskBill inventoryAllTaskBill);
        void DeleteInventoryAllTaskItem(InventoryAllTaskItem inventoryAllTaskItem);
        IList<InventoryAllTaskBill> GetAllInventoryAllTaskBills();
        IList<InventoryAllTaskBill> CheckInventory(int? store, int? inventoryPerson, int? wareHouseId);
        IList<InventoryAllTaskBill> CheckInventory(int? store, int? inventoryPerson, int? wareHouseId, int? productId);
        IPagedList<InventoryAllTaskBill> GetAllInventoryAllTaskBills(int? store, int? makeuserId, int? inventoryPerson, int? wareHouseId, string billNumber = "", bool? status = null, DateTime? start = null, DateTime? end = null, int? inventoryStatus = -1, bool? showReverse = null, bool? sortByCompletedTime = null, string remark = "", int pageIndex = 0, int pageSize = int.MaxValue);

        InventoryAllTaskBill GetInventoryAllTaskBillById(int? store, int inventoryAllTaskBillId);
        InventoryAllTaskBill GetInventoryAllTaskBillById(int? store, int inventoryAllTaskBillId, bool isInclude = false);
        InventoryAllTaskBill GetInventoryAllTaskBillByNumber(int? store, string billNumber);
        InventoryAllTaskItem GetInventoryAllTaskItemById(int? store, int inventoryAllTaskItemId);
        IPagedList<InventoryAllTaskItem> GetInventoryAllTaskItemsByInventoryAllTaskBillId(int inventoryAllTaskBillId, int? userId, int? storeId, int pageIndex, int pageSize);
        List<InventoryAllTaskItem> GetInventoryAllTaskItemList(int inventoryAllTaskBillId);

        void InsertInventoryAllTaskBill(InventoryAllTaskBill inventoryAllTaskBill);
        void InsertInventoryAllTaskItem(InventoryAllTaskItem inventoryAllTaskItem);
        void UpdateInventoryAllTaskBill(InventoryAllTaskBill inventoryAllTaskBill);
        void UpdateInventoryAllTaskItem(InventoryAllTaskItem inventoryAllTaskItem);

        BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, InventoryAllTaskBill inventoryAllTaskBill, InventoryAllTaskBillUpdate data, List<InventoryAllTaskItem> items, out int inventoryAllTaskBillId, bool isAdmin = false);


        BaseResult CancelTakeInventory(int storeId, int userId, InventoryAllTaskBill inventoryAllTaskBill);
        BaseResult SetInventoryCompleted(int storeId, int userId, InventoryAllTaskBill inventoryAllTaskBill);

    }
}