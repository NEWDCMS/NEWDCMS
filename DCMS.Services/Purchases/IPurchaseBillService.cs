using DCMS.Core;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Purchases;
using DCMS.Core.Domain.WareHouses;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Purchases
{
    public interface IPurchaseBillService
    {
        bool Exists(int billId);
        void DeletePurchaseBill(PurchaseBill purchaseBill);
        void DeletePurchaseBillAccounting(PurchaseBillAccounting purchaseBillAccounting);
        void DeletePurchaseItem(PurchaseItem purchaseItem);
        PurchaseBillAccounting GetPurchaseBillAccountingById(int purchaseBillAccountingId);
        IList<PurchaseBillAccounting> GetPurchaseBillAccountingsByPurchaseBillId(int? store, int purchaseBillId);
        IPagedList<PurchaseBillAccounting> GetPurchaseBillAccountingsByPurchaseBillId(int storeId, int userId, int purchaseBillId, int pageIndex, int pageSize);
        PurchaseBill GetPurchaseBillById(int? store, int purchaseBillId);
        PurchaseBill GetPurchaseBillById(int? store, int purchaseBillId, bool isInclude = false);
        PurchaseBill GetPurchaseBillByNumber(int? store, string billNumber);
        PurchaseItem GetPurchaseItemById(int purchaseItemId);
        IList<PurchaseItem> GetPurchaseItemByPurchaseId(int storeId, int userId, int purchaseBillId, int pageIndex, int pageSize);
        List<PurchaseItem> GetPurchaseItemList(int purchaseBillId);

        IPagedList<PurchaseBill> GetPurchaseBillList(int? store, int? makeuserId, int? businessUserId, int? manufacturerId, int? wareHouseId = null, string billNumber = "", bool? printStatus = null, DateTime? start = null, DateTime? end = null, bool? auditedStatus = null, bool? reversedStatus = null, string remark = "", bool? sortByAuditedTime = null, bool? showReverse = null, int? paymented = null, bool? deleted = null, int pageIndex = 0, int pageSize = int.MaxValue);
        IList<PurchaseBill> GetPurchasesBillByStoreId(int? storeId);

        IList<PurchaseBill> GetPurchaseBillByStoreIdManufacturerId(int storeId, int manufacturerId);

        void InsertPurchaseBill(PurchaseBill purchaseBill);
        void InsertPurchaseBillAccounting(PurchaseBillAccounting purchaseBillAccounting);
        void InsertPurchaseItem(PurchaseItem purchaseItem);
        int PurchaseItemQtySum(int storeId, int productId, int purchaseBillId);
        void SetPurchaseBillAmount(int purchaseBillId);
        void UpdatePurchaseBill(PurchaseBill purchaseBill);
        void UpdatePurchaseBillAccounting(PurchaseBillAccounting purchaseBillAccounting);
        void UpdatePurchaseItem(PurchaseItem purchaseItem);
        void UpdatePurchaseBillOweCash(string billNumber, decimal? oweCash);
        void UpdatePaymented(int? store, int billId, PayStatus payStatus);

        IList<PurchaseBillAccounting> GetAllPurchaseBillAccountingsByBillIds(int? store, int[] billIds);

        PurchaseItem GetPurchaseItemByProductId(int store, int productId);

        /// <summary>
        /// 获取商品大、中、小单位 最新以前的采购价格
        /// </summary>
        /// <param name="product"></param>
        /// <param name="storeId"></param>
        /// <param name="productId"></param>
        /// <param name="beforeTax"></param>
        /// <returns></returns>
        Tuple<PurchaseItem, PurchaseItem, PurchaseItem> GetPurchaseItemByProduct(Product product, int storeId, int productId, bool beforeTax = false);

        void UpdatePurchaseBillActive(int? store, int? billId, int? user);

        /// <summary>
        /// 获取商品成本价
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        Dictionary<int, decimal> GetReferenceCostPrice(Product product);
        /// <summary>
        /// 获取商品成本价
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="productId"></param>
        /// <param name="unitId"></param>
        /// <returns></returns>
        decimal GetReferenceCostPrice(int storeId, int productId, int unitId);

        /// <summary>
        /// 获取平均采购价格
        /// </summary>
        /// <param name="productId">商品Id</param>
        /// <param name="unitId">单位Id</param>
        /// <param name="averageNumber">平均次数</param>
        /// <returns></returns>
        decimal GetLastAveragePurchasePrice(int store, int productId, int unitId, int averageNumber);

        BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, PurchaseBill purchaseBill, List<PurchaseBillAccounting> accountingOptions, List<AccountingOption> accountings, PurchaseBillUpdate data, List<PurchaseItem> items, List<ProductStockItem> productStockItemThiss, bool isAdmin = false, bool doAudit = true);

        BaseResult Auditing(int storeId, int userId, PurchaseBill purchaseBill);
        BaseResult Reverse(int userId, PurchaseBill purchaseBill);
        BaseResult Delete(int userId, PurchaseBill purchaseBill);
    }
}