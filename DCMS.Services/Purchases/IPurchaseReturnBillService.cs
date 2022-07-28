using DCMS.Core;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Purchases;
using DCMS.Core.Domain.WareHouses;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Purchases
{
    public interface IPurchaseReturnBillService
    {
        bool Exists(int billId);
        void DeletePurchaseReturnBill(PurchaseReturnBill purchaseReturnBill);
        void DeletePurchaseReturnBillAccounting(PurchaseReturnBillAccounting purchaseReturnBillAccounting);
        void DeletePurchaseReturnItem(PurchaseReturnItem purchaseReturnItem);
        PurchaseReturnBillAccounting GetPurchaseReturnBillAccountingById(int purchaseReturnBillAccountingId);
        IList<PurchaseReturnBillAccounting> GetPurchaseReturnBillAccountingsByPurchaseReturnBillId(int? store, int purchaseReturnBillId);
        IPagedList<PurchaseReturnBillAccounting> GetPurchaseReturnBillAccountingsByPurchaseReturnBillId(int storeId, int userId, int purchaseReturnBillId, int pageIndex, int pageSize);
        PurchaseReturnBill GetPurchaseReturnBillById(int? store, int purchaseReturnBillId);
        PurchaseReturnBill GetPurchaseReturnBillById(int? store, int purchaseReturnBillId, bool isInclude = false);
        PurchaseReturnBill GetPurchaseReturnBillByNumber(int? store, string billNumber);
        PurchaseReturnItem GetPurchaseReturnItemById(int purchaseReturnItemId);
        IList<PurchaseReturnItem> GetPurchaseReturnItemByPurchaseReturnBillId(int purchaseReturnBillId, int? userId, int? storeId, int pageIndex, int pageSize);
        List<PurchaseReturnItem> GetPurchaseReturnItemList(int purchaseReturnBillId);
        IPagedList<PurchaseReturnBill> GetPurchaseReturnBillList(int? store, int? makeuserId, int? businessUserId, int? manufacturerId, int? wareHouseId = null, string billNumber = "", bool? printStatus = null, DateTime? start = null, DateTime? end = null, bool? auditedStatus = null, bool? reversedStatus = null, string remark = "", bool? sortByAuditedTime = null, bool? showReverse = null, int? paymented = null, bool? deleted = null, int pageIndex = 0, int pageSize = int.MaxValue);
        IList<PurchaseReturnBill> GetPurchaseReturnBillsByStoreId(int? storeId);

        IList<PurchaseReturnBill> GetPurchaseReturnBillByStoreIdManufacturerId(int storeId, int manufacturerId);

        void InsertPurchaseReturnBill(PurchaseReturnBill purchaseReturnBill);
        void InsertPurchaseReturnBillAccounting(PurchaseReturnBillAccounting purchaseReturnBillAccounting);
        void InsertPurchaseReturnItem(PurchaseReturnItem purchaseReturnItem);
        int PurchaseReturnItemQtySum(int storeId, int productId, int purchaseReturnId);
        void SetPurchaseReturnBillAmount(int purchaseBillReturnId);
        void UpdatePurchaseReturnBill(PurchaseReturnBill purchaseReturnBill);
        void UpdatePurchaseReturnBillAccounting(PurchaseReturnBillAccounting purchaseReturnBillAccounting);
        void UpdatePurchaseReturnBillItem(PurchaseReturnItem purchaseReturnItem);
        void UpdatePurchaseReturnBillOweCash(string billNumber, decimal? oweCash);
        void UpdatePaymented(int? store, int billId, PayStatus payStatus);

        IList<PurchaseReturnBillAccounting> GetAllPurchaseReturnBillAccountingsByBillIds(int? store, int[] billIds);

        BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, PurchaseReturnBill purchaseReturnBill, List<PurchaseReturnBillAccounting> accountingOptions, List<AccountingOption> accountings, PurchaseReturnBillUpdate data, List<PurchaseReturnItem> items, List<ProductStockItem> productStockItemThiss, bool isAdmin = false, bool doAudit = true);

        BaseResult Auditing(int storeId, int userId, PurchaseReturnBill purchaseReturnBill);

        BaseResult Reverse(int userId, PurchaseReturnBill purchaseReturnBill);

        IList<int> GetProductPurchaseIds(int StoreId, int manufacterId = 0);


    }
}