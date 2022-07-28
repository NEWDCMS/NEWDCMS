using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.WareHouses;
using DCMS.Core.Events;
using DCMS.Services.Events;

namespace DCMS.Web.Infrastructure.Cache
{
    public partial class WareHousesCacheEventConsumer :
        //AllocationBill
        IConsumer<EntityInsertedEvent<AllocationBill>>,
         IConsumer<EntityUpdatedEvent<AllocationBill>>,
         IConsumer<EntityDeletedEvent<AllocationBill>>,

        //AllocationItem
        IConsumer<EntityInsertedEvent<AllocationItem>>,
         IConsumer<EntityUpdatedEvent<AllocationItem>>,
         IConsumer<EntityDeletedEvent<AllocationItem>>,

        //CombinationProductBill
        IConsumer<EntityInsertedEvent<CombinationProductBill>>,
         IConsumer<EntityUpdatedEvent<CombinationProductBill>>,
         IConsumer<EntityDeletedEvent<CombinationProductBill>>,

        //CombinationProductItem
        IConsumer<EntityInsertedEvent<CombinationProductItem>>,
         IConsumer<EntityUpdatedEvent<CombinationProductItem>>,
         IConsumer<EntityDeletedEvent<CombinationProductItem>>,

        //CostAdjustmentBill
        IConsumer<EntityInsertedEvent<CostAdjustmentBill>>,
         IConsumer<EntityUpdatedEvent<CostAdjustmentBill>>,
         IConsumer<EntityDeletedEvent<CostAdjustmentBill>>,

        //CostAdjustmentItem
        IConsumer<EntityInsertedEvent<CostAdjustmentItem>>,
         IConsumer<EntityUpdatedEvent<CostAdjustmentItem>>,
         IConsumer<EntityDeletedEvent<CostAdjustmentItem>>,

        //InventoryAllTaskBill
        IConsumer<EntityInsertedEvent<InventoryAllTaskBill>>,
         IConsumer<EntityUpdatedEvent<InventoryAllTaskBill>>,
         IConsumer<EntityDeletedEvent<InventoryAllTaskBill>>,

        //InventoryAllTaskItem
        IConsumer<EntityInsertedEvent<InventoryAllTaskItem>>,
         IConsumer<EntityUpdatedEvent<InventoryAllTaskItem>>,
         IConsumer<EntityDeletedEvent<InventoryAllTaskItem>>,

        //InventoryPartTaskBill
        IConsumer<EntityInsertedEvent<InventoryPartTaskBill>>,
         IConsumer<EntityUpdatedEvent<InventoryPartTaskBill>>,
         IConsumer<EntityDeletedEvent<InventoryPartTaskBill>>,

        //InventoryPartTaskItem
        IConsumer<EntityInsertedEvent<InventoryPartTaskItem>>,
         IConsumer<EntityUpdatedEvent<InventoryPartTaskItem>>,
         IConsumer<EntityDeletedEvent<InventoryPartTaskItem>>,

        //InventoryProfitLossBill
        IConsumer<EntityInsertedEvent<InventoryProfitLossBill>>,
         IConsumer<EntityUpdatedEvent<InventoryProfitLossBill>>,
         IConsumer<EntityDeletedEvent<InventoryProfitLossBill>>,

        //InventoryProfitLossItem
        IConsumer<EntityInsertedEvent<InventoryProfitLossItem>>,
         IConsumer<EntityUpdatedEvent<InventoryProfitLossItem>>,
         IConsumer<EntityDeletedEvent<InventoryProfitLossItem>>,

        //InventoryReportBill
        IConsumer<EntityInsertedEvent<InventoryReportBill>>,
         IConsumer<EntityUpdatedEvent<InventoryReportBill>>,
         IConsumer<EntityDeletedEvent<InventoryReportBill>>,

        //InventoryReportItem
        IConsumer<EntityInsertedEvent<InventoryReportItem>>,
         IConsumer<EntityUpdatedEvent<InventoryReportItem>>,
         IConsumer<EntityDeletedEvent<InventoryReportItem>>,

        //InventoryReportStoreQuantity
        IConsumer<EntityInsertedEvent<InventoryReportStoreQuantity>>,
         IConsumer<EntityUpdatedEvent<InventoryReportStoreQuantity>>,
         IConsumer<EntityDeletedEvent<InventoryReportStoreQuantity>>,

        //InventoryReportSummary
        IConsumer<EntityInsertedEvent<InventoryReportSummary>>,
         IConsumer<EntityUpdatedEvent<InventoryReportSummary>>,
         IConsumer<EntityDeletedEvent<InventoryReportSummary>>,

        //ScrapProductBill
        IConsumer<EntityInsertedEvent<ScrapProductBill>>,
         IConsumer<EntityUpdatedEvent<ScrapProductBill>>,
         IConsumer<EntityDeletedEvent<ScrapProductBill>>,

        //ScrapProductItem
        IConsumer<EntityInsertedEvent<ScrapProductItem>>,
         IConsumer<EntityUpdatedEvent<ScrapProductItem>>,
         IConsumer<EntityDeletedEvent<ScrapProductItem>>,

        //SplitProductBill
        IConsumer<EntityInsertedEvent<SplitProductBill>>,
         IConsumer<EntityUpdatedEvent<SplitProductBill>>,
         IConsumer<EntityDeletedEvent<SplitProductBill>>,

        //SplitProductItem
        IConsumer<EntityInsertedEvent<SplitProductItem>>,
         IConsumer<EntityUpdatedEvent<SplitProductItem>>,
         IConsumer<EntityDeletedEvent<SplitProductItem>>,

        //Stock
        IConsumer<EntityInsertedEvent<Stock>>,
         IConsumer<EntityUpdatedEvent<Stock>>,
         IConsumer<EntityDeletedEvent<Stock>>,

        //StockInOutRecord
        IConsumer<EntityInsertedEvent<StockInOutRecord>>,
         IConsumer<EntityUpdatedEvent<StockInOutRecord>>,
         IConsumer<EntityDeletedEvent<StockInOutRecord>>,

        //StockFlow
        IConsumer<EntityInsertedEvent<StockFlow>>,
         IConsumer<EntityUpdatedEvent<StockFlow>>,
         IConsumer<EntityDeletedEvent<StockFlow>>,

        //StockInOutRecordStockFlow
        IConsumer<EntityInsertedEvent<StockInOutRecordStockFlow>>,
         IConsumer<EntityUpdatedEvent<StockInOutRecordStockFlow>>,
         IConsumer<EntityDeletedEvent<StockInOutRecordStockFlow>>,

        //WareHouse
        IConsumer<EntityInsertedEvent<WareHouse>>,
         IConsumer<EntityUpdatedEvent<WareHouse>>,
         IConsumer<EntityDeletedEvent<WareHouse>>
    {
        protected readonly IStaticCacheManager _cacheManager;

        public WareHousesCacheEventConsumer(IStaticCacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        #region AllocationBill
        public void HandleEvent(EntityInsertedEvent<AllocationBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.ALLOCATIONBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<AllocationBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.ALLOCATIONBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<AllocationBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.ALLOCATIONBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region AllocationItem
        public void HandleEvent(EntityInsertedEvent<AllocationItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.ALLOCATIONBILLITEM_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<AllocationItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.ALLOCATIONBILLITEM_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<AllocationItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.ALLOCATIONBILLITEM_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region CombinationProductBill
        public void HandleEvent(EntityInsertedEvent<CombinationProductBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.COMBINATIONPRODUCTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<CombinationProductBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.COMBINATIONPRODUCTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<CombinationProductBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.COMBINATIONPRODUCTBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region CombinationProductItem
        public void HandleEvent(EntityInsertedEvent<CombinationProductItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.COMBINATIONPRODUCTBILLITEM_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<CombinationProductItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.COMBINATIONPRODUCTBILLITEM_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<CombinationProductItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.COMBINATIONPRODUCTBILLITEM_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region CostAdjustmentBill
        public void HandleEvent(EntityInsertedEvent<CostAdjustmentBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.CONSTADJUSTMENTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<CostAdjustmentBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.CONSTADJUSTMENTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<CostAdjustmentBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.CONSTADJUSTMENTBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region CostAdjustmentItem
        public void HandleEvent(EntityInsertedEvent<CostAdjustmentItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.CONSTADJUSTMENTBILLITEM_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<CostAdjustmentItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.CONSTADJUSTMENTBILLITEM_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<CostAdjustmentItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.CONSTADJUSTMENTBILLITEM_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region InventoryAllTaskBill
        public void HandleEvent(EntityInsertedEvent<InventoryAllTaskBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.INVENTORYALLTASKBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<InventoryAllTaskBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.INVENTORYALLTASKBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<InventoryAllTaskBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.INVENTORYALLTASKBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region InventoryAllTaskItem
        public void HandleEvent(EntityInsertedEvent<InventoryAllTaskItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.INVENTORYALLTASKBILLITEM_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<InventoryAllTaskItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.INVENTORYALLTASKBILLITEM_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<InventoryAllTaskItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.INVENTORYALLTASKBILLITEM_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region InventoryPartTaskBill
        public void HandleEvent(EntityInsertedEvent<InventoryPartTaskBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.INVENTORYPARTTASKBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<InventoryPartTaskBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.INVENTORYPARTTASKBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<InventoryPartTaskBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.INVENTORYPARTTASKBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region InventoryPartTaskItem
        public void HandleEvent(EntityInsertedEvent<InventoryPartTaskItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.INVENTORYPARTTASKBILLITEM_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<InventoryPartTaskItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.INVENTORYPARTTASKBILLITEM_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<InventoryPartTaskItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.INVENTORYPARTTASKBILLITEM_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region InventoryProfitLossBill
        public void HandleEvent(EntityInsertedEvent<InventoryProfitLossBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.INVENTORPROFITLOSSBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<InventoryProfitLossBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.INVENTORPROFITLOSSBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<InventoryProfitLossBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.INVENTORPROFITLOSSBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region InventoryProfitLossItem
        public void HandleEvent(EntityInsertedEvent<InventoryProfitLossItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.INVENTORPROFITLOSSBILLITEM_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<InventoryProfitLossItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.INVENTORPROFITLOSSBILLITEM_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<InventoryProfitLossItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.INVENTORPROFITLOSSBILLITEM_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region InventoryReportBill
        public void HandleEvent(EntityInsertedEvent<InventoryReportBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.INVENTORYREPORTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<InventoryReportBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.INVENTORYREPORTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<InventoryReportBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.INVENTORYREPORTBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region InventoryReportItem
        public void HandleEvent(EntityInsertedEvent<InventoryReportItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.INVENTORYREPORTBILLITEM_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<InventoryReportItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.INVENTORYREPORTBILLITEM_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<InventoryReportItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.INVENTORYREPORTBILLITEM_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region InventoryReportStoreQuantity
        public void HandleEvent(EntityInsertedEvent<InventoryReportStoreQuantity> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.INVENTORYREPORTSTOREQUANTITY_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<InventoryReportStoreQuantity> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.INVENTORYREPORTSTOREQUANTITY_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<InventoryReportStoreQuantity> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.INVENTORYREPORTSTOREQUANTITY_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region InventoryReportSummary
        public void HandleEvent(EntityInsertedEvent<InventoryReportSummary> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.INVENTORYREPORTSUMMARY_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<InventoryReportSummary> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.INVENTORYREPORTSUMMARY_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<InventoryReportSummary> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.INVENTORYREPORTSUMMARY_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region ScrapProductBill
        public void HandleEvent(EntityInsertedEvent<ScrapProductBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.SCRAPPRODUCTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<ScrapProductBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.SCRAPPRODUCTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<ScrapProductBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.SCRAPPRODUCTBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region ScrapProductItem
        public void HandleEvent(EntityInsertedEvent<ScrapProductItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.SCRAPPRODUCTBILLITEM_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<ScrapProductItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.SCRAPPRODUCTBILLITEM_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<ScrapProductItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.SCRAPPRODUCTBILLITEM_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region SplitProductBill
        public void HandleEvent(EntityInsertedEvent<SplitProductBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.SPLITPRODUCTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<SplitProductBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.SPLITPRODUCTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<SplitProductBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.SPLITPRODUCTBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region SplitProductItem
        public void HandleEvent(EntityInsertedEvent<SplitProductItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.SPLITPRODUCTBILLITEM_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<SplitProductItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.SPLITPRODUCTBILLITEM_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<SplitProductItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.SPLITPRODUCTBILLITEM_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region Stock
        public void HandleEvent(EntityInsertedEvent<Stock> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STOCK_PK, eventMessage.Entity.StoreId)); 
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STOCKREPORTSERVICE_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<Stock> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STOCK_PK, eventMessage.Entity.StoreId));
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STOCKREPORTSERVICE_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<Stock> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STOCK_PK, eventMessage.Entity.StoreId));
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STOCKREPORTSERVICE_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region StockInOutRecord
        public void HandleEvent(EntityInsertedEvent<StockInOutRecord> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STOCKINOUTRECORDSSTOCKFLOW_PK, eventMessage.Entity.StoreId));
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STOCKREPORTSERVICE_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<StockInOutRecord> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STOCKINOUTRECORDSSTOCKFLOW_PK, eventMessage.Entity.StoreId));
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STOCKREPORTSERVICE_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<StockInOutRecord> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STOCKINOUTRECORDSSTOCKFLOW_PK, eventMessage.Entity.StoreId));
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STOCKREPORTSERVICE_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region StockFlow
        public void HandleEvent(EntityInsertedEvent<StockFlow> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STOCKFLOW_PK, eventMessage.Entity.StoreId));
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STOCKREPORTSERVICE_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<StockFlow> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STOCKFLOW_PK, eventMessage.Entity.StoreId));
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STOCKREPORTSERVICE_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<StockFlow> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STOCKFLOW_PK, eventMessage.Entity.StoreId));
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STOCKREPORTSERVICE_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region StockInOutRecordStockFlow
        public void HandleEvent(EntityInsertedEvent<StockInOutRecordStockFlow> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STOCKINOUTRECORDSSTOCKFLOW_PK, eventMessage.Entity.StoreId));
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STOCKREPORTSERVICE_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<StockInOutRecordStockFlow> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STOCKINOUTRECORDSSTOCKFLOW_PK, eventMessage.Entity.StoreId));
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STOCKREPORTSERVICE_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<StockInOutRecordStockFlow> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STOCKINOUTRECORDSSTOCKFLOW_PK, eventMessage.Entity.StoreId));
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STOCKREPORTSERVICE_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region WareHouse
        public void HandleEvent(EntityInsertedEvent<WareHouse> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.WAREHOUSE_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<WareHouse> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.WAREHOUSE_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<WareHouse> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.WAREHOUSE_PK, eventMessage.Entity.StoreId));
        }
        #endregion


    }
}
