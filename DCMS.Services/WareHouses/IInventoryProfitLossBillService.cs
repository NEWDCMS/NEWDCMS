using DCMS.Core;
using DCMS.Core.Domain.WareHouses;
using System;
using System.Collections.Generic;

namespace DCMS.Services.WareHouses
{
    public interface IInventoryProfitLossBillService
    {
        bool Exists(int billId);
        void DeleteInventoryProfitLossBill(InventoryProfitLossBill inventoryProfitLossBill);
        void DeleteInventoryProfitLossItem(InventoryProfitLossItem inventoryProfitLossItem);
        IList<InventoryProfitLossBill> GetAllInventoryProfitLossBills();
        IPagedList<InventoryProfitLossBill> GetAllInventoryProfitLossBills(int? store, int? makeuserId, int? chargePerson, int? wareHouseId, string billNumber = "", bool? status = null, DateTime? start = null, DateTime? end = null, bool? isShowReverse = null, bool? sortByAuditedTime = null, bool? deleted = null, string remark = "", int pageIndex = 0, int pageSize = int.MaxValue);

        InventoryProfitLossBill GetInventoryProfitLossBillById(int? store, int inventoryProfitLossBillId);
        InventoryProfitLossBill GetInventoryProfitLossBillById(int? store, int inventoryProfitLossBillId, bool isInclude = false);
        InventoryProfitLossBill GetInventoryProfitLossBillByNumber(int? store, string billNumber);
        InventoryProfitLossItem GetInventoryProfitLossItemById(int? store, int inventoryProfitLossItemId);
        IPagedList<InventoryProfitLossItem> GetInventoryProfitLossItemsByInventoryProfitLossBillId(int inventoryProfitLossBillId, int? userId, int? storeId, int pageIndex, int pageSize);
        List<InventoryProfitLossItem> GetInventoryProfitLossItemList(int inventoryProfitLossBillId);

        void InsertInventoryProfitLossBill(InventoryProfitLossBill inventoryProfitLossBill);
        void InsertInventoryProfitLossItem(InventoryProfitLossItem inventoryProfitLossItem);
        void UpdateInventoryProfitLossBill(InventoryProfitLossBill inventoryProfitLossBill);
        void UpdateInventoryProfitLossItem(InventoryProfitLossItem inventoryProfitLossItem);

        BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, InventoryProfitLossBill inventoryProfitLossBill, InventoryProfitLossBillUpdate data, List<InventoryProfitLossItem> items, List<ProductStockItem> productStockItemLoses, bool isAdmin = false);

        BaseResult Auditing(int storeId, int userId, InventoryProfitLossBill inventoryProfitLossBill);
        BaseResult Reverse(int userId, InventoryProfitLossBill inventoryProfitLossBill);
        BaseResult Delete(int userId, InventoryProfitLossBill inventoryProfitLossBill);

    }
}