using DCMS.Core;
using DCMS.Core.Domain.WareHouses;

namespace DCMS.Services.WareHouses
{
    public interface IInventoryReportBillService
    {

        IPagedList<InventoryReportBill> GetInventoryReportBillList(int? store, int? terminalId, int? businessUserId, int pageIndex = 0, int pageSize = int.MaxValue);

        InventoryReportBill GetInventoryReportBillById(int? store, int inventoryReportBillId);
        InventoryReportBill GetInventoryReportBillByNumber(int? store, string billNumber);
        void InsertInventoryReportBill(InventoryReportBill inventoryReportBill);
        void UpdateInventoryReportBill(InventoryReportBill inventoryReportBill);
        void DeleteInventoryReportBill(InventoryReportBill inventoryReportBill);


        InventoryReportItem GetInventoryReportItemById(int? store, int inventoryReportItemId);
        void InsertInventoryReportItem(InventoryReportItem inventoryReportItem);
        void UpdateInventoryReportItem(InventoryReportItem inventoryReportItem);
        void DeleteInventoryReportItem(InventoryReportItem inventoryReportItem);

        InventoryReportStoreQuantity GetInventoryReportStoreQuantityById(int? store, int inventoryReportStoreQuantityId);
        void InsertInventoryReportStoreQuantity(InventoryReportStoreQuantity inventoryReportStoreQuantity);
        void UpdateInventoryReportStoreQuantity(InventoryReportStoreQuantity inventoryReportStoreQuantity);
        void DeleteInventoryReportStoreQuantity(InventoryReportStoreQuantity inventoryReportStoreQuantity);

        InventoryReportSummary GetInventoryReportSummaryById(int? store, int inventoryReportSummaryId);
        InventoryReportSummary GetInventoryReportSummaryByTerminalIdProductId(int store, int terminalId, int productId);
        void InsertInventoryReportSummary(InventoryReportSummary inventoryReportSummary);
        void UpdateInventoryReportSummary(InventoryReportSummary inventoryReportSummary);
        void DeleteInventoryReportSummary(InventoryReportSummary inventoryReportSummary);

        void UpdateInventoryReportBillActive(int? store, int? billId, int? user);


    }
}