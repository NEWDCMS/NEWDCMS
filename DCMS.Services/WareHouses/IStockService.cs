using DCMS.Core;
using DCMS.Core.Domain.WareHouses;
using DCMS.Services.Products;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DCMS.Services.WareHouses
{
    public interface IStockService
    {
        void DeleteStock(Stock stock);
        void DeleteStockFlow(StockFlow stockFlow);
        void DeleteStockInOutRecord(StockInOutRecord stockInOutRecord);
        IList<StockInOutRecord> GetAllStockInOutRecords();
        IPagedList<StockInOutRecord> GetAllStockInOutRecords(int? store, string billCode, int? billType, DateTime? start = null, DateTime? end = null, int pageIndex = 0, int pageSize = int.MaxValue);
        IList<Stock> GetAllStocks(int? store);
        IList<Stock> GetAllStocksByProductIds(int? store, int[] productIds);
        IList<StockQty> GetAllStockProducts(int? store);
        IPagedList<Stock> GetAllStocks(int? store, int? wareHouseId, int? productId, DateTime? start = null, DateTime? end = null, int pageIndex = 0, int pageSize = int.MaxValue);
        Stock GetStockById(int? store, int stockId);
        StockFlow GetStockFlowById(int? store, int stockFlowId);
        IPagedList<StockFlow> GetStockFlowsByStockId(int stockId, int? userId, int? storeId, int pageIndex, int pageSize);
        StockInOutRecord GetStockInOutRecordById(int? store, int stockInOutRecordId);
        StockInOutRecord GetStockInOutRecordByBillCode(int storeId, BillTypeEnum billTypeEnum, string billCode);


        bool CheckOrderQuantity(int storeId, BillTypeEnum billTypeEnum, string billCode, int wareHouseId);
        bool CheckOrderQuantity(int storeId, BillTypeEnum billTypeEnum, string billCode, int wareHouseId, int productId = 0);
        StockInOutRecordStockFlow GetStockInOutRecordStockFlowByStockFlowId(int stockFlowId);

        void InsertStock(Stock stock);
        void InsertStockFlow(StockFlow stockFlow);
        void InsertStockInOutRecord(StockInOutRecord stockInOutRecord);
        void UpdateStock(Stock stock);
        void InsertStockInOutDetails(StockInOutDetails stockInOutDetail);
        void InsertStockInOutDetails(List<StockInOutDetails> stockInOutDetails);
        void UpdateStockInOutRecord(StockInOutRecord stockInOutRecord);
        bool StockExist(int? store, int? wareHouseId, int? productId);
        Stock GetCurrentStock(int? store, int? wareHouseId, int? productId);
        StockFlow GetLastTimeStockFlow(int storeId, int stockId, int productId);
        //获取商品的采购入库记录
        IQueryable<StockInOutRecord> getStockInOutRecordsByProductAndStock(int store, int stockId, int productId);


        IList<StockInOutRecordStockFlow> GetStockInOutRecordStockFlowByStockInOutRecordId(int stockInOutRecordId, int? userId, int? storeId);
        StockInOutRecordStockFlow GetStockInOutRecordStockFlowById(int stockInOutRecordStockFlowId);
        void InsertStockInOutRecordStockFlow(StockInOutRecordStockFlow stockInOutRecordStockFlow);
        void UpdateStockInOutRecordStockFlow(StockInOutRecordStockFlow stockInOutRecordStockFlow);
        void DeleteStockInOutRecordStockFlow(StockInOutRecordStockFlow stockInOutRecordStockFlow);

        /// <summary>
        /// 库存验证
        /// </summary>
        /// <param name="_productService"></param>
        /// <param name="_specificationAttributeService"></param>
        /// <param name="storeId"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="stockProducts"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        bool CheckStockQty(IProductService _productService, ISpecificationAttributeService _specificationAttributeService, int storeId, int wareHouseId, List<ProductStockItem> stockProducts, out string errMsg, bool enableOrderQuantity = true);


        /// <summary>
        /// 库存修改
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="bill"></param>
        /// <param name="_productService"></param>
        /// <param name="_specificationAttributeService"></param>
        /// <param name="directionEnum"></param>
        /// <param name="stockQuantityType"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="stockProducts"></param>
        /// <param name="stockFlowChangeTypeEnum"></param>
        /// <param name="historyDatas"></param>
        /// <returns></returns>
        Tuple<List<ProductStockItem>, Tuple<StockInOutRecord, StockInOutRecord>, Tuple<List<StockFlow>, List<StockFlow>>, Tuple<List<StockInOutRecordStockFlow>, List<StockInOutRecordStockFlow>>, Tuple<List<Stock>, List<Stock>>> AdjustStockQty<T, T1>(T bill,
            IProductService _productService,
            ISpecificationAttributeService _specificationAttributeService,
            DirectionEnum directionEnum,
            StockQuantityType stockQuantityType,
            int wareHouseId,
            List<ProductStockItem> stockProducts,
            StockFlowChangeTypeEnum stockFlowChangeTypeEnum) where T : BaseBill<T1> where T1 : BaseEntity;
        Tuple<List<ProductStockItem>, Tuple<StockInOutRecord, StockInOutRecord>, Tuple<List<StockFlow>, List<StockFlow>>, Tuple<List<StockInOutRecordStockFlow>, List<StockInOutRecordStockFlow>>, Tuple<List<Stock>, List<Stock>>> AdjustStockQty<T, T1>(T bill,
            DirectionEnum directionEnum,
            StockQuantityType stockQuantityType,
            int wareHouseId,
            List<ProductStockItem> stockProducts,
            StockFlowChangeTypeEnum stockFlowChangeTypeEnum) where T : BaseBill<T1> where T1 : BaseEntity;

        /// <summary>
        ///调整库存
        /// </summary>
        /// <param name="newStock"></param>
        /// <param name="oldStock"></param>
        /// <param name="store"></param>
        /// <param name="userId"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        bool AdjustmentStock(Stock newStock, Stock oldStock, int? store, int? userId, int? wareHouseId, int? productId);

        int GetProductCurrentQuantity(int storeId, int productId, int wareHouseId);
        int GetProductUsableQuantity(int storeId, int productId, int wareHouseId);

        /// <summary>
        /// 获取指定期间商品出入库存量
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="billTypes">12, 14, 22, 24, 32, 33, 34, 37, 38</param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        IList<StockQty> GetStockQTY(int storeId, int[] billTypes, DateTime start, DateTime end);

        /// <summary>
        /// 获取指定期间商品出入库存量汇总
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="billTypes">12, 14, 22, 24, 32, 33, 34, 37, 38</param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        IList<StockQtySummery> GetStockQTYSummery(int storeId, int[] billTypes, DateTime start, DateTime end);


        void RoolBackChanged(Tuple<List<ProductStockItem>, Tuple<StockInOutRecord, StockInOutRecord>, Tuple<List<StockFlow>, List<StockFlow>>, Tuple<List<StockInOutRecordStockFlow>, List<StockInOutRecordStockFlow>>, Tuple<List<Stock>, List<Stock>>> tuple);

        StockInOutDetails GetFIFOProduct(int storeId, int wareHouseId, int productId);
        void UpdateStockInOutDetails(StockInOutDetails stockInOutDetail);
    }
}