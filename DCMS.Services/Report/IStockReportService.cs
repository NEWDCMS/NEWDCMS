using DCMS.Core;
using DCMS.Core.Domain.WareHouses;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Report
{
    public interface IStockReportService
    {
        StockChangeSummary GetStockChangeSummary(int? store, int? productId, int? wareHouseId, double? price, DateTime? startTime = null, DateTime? endTime = null);

        IList<StockChangeSummary> GetAllStockChangeSummary(int? store, int? wareHouseId, double? price, DateTime? startTime = null, DateTime? endTime = null);

        IList<StockChangeSummary> GetAllStockChangeSummary(int? store, int? wareHouseId, int? productId, string productName, int? brandId, DateTime? startTime = null, DateTime? endTime = null);

        IList<StockChangeSummaryOrder> GetStockChangeSummaryByOrder(int? store, int? productId, string productName, int? categoryId, int? billType, int? wareHouseId, string billCode = "", DateTime? startTime = null, DateTime? endTime = null, bool crossMonth = true);
        //门店库存上报表
        IList<InventoryReportList> GetInventoryReportList(int? store, int? businessUserId, int? terminalId, string TerminalName, int? channelId, int? rankId, int? districtId, int? productId, string productName, DateTime? startTime = null, DateTime? endTime = null);
        //门店库存上报表
        IPagedList<InventoryReportList> GetInventoryReportListApi(int? store, int? makeuserId, int? businessUserId, int? terminalId, int? channelId, int? rankId, int? districtId, int? productId, DateTime? startTime = null, DateTime? endTime = null, int pageIndex = 0, int pageSize = int.MaxValue);

        //门店库存上报表汇总
        IList<InventoryReportList> GetInventoryReportSummaryList(int? store, int? channelId, int? rankId, int? districtId, int? productId, string productName, DateTime? startTime, DateTime? endTime);
        //调拨明细表
        IList<AllocationDetailsList> GetAllocationDetails(int? store, int? shipmentWareHouseId, int? incomeWareHouseId, int? productId, string productName, int? categoryId, string billNumber = "", int? StatusId = null, DateTime? startTime = null, DateTime? endTime = null);
        //调拨明细表（按商品）
        IList<AllocationDetailsList> GetAllocationDetailsByProducts(int? store, int? shipmentWareHouseId, int? incomeWareHouseId, int? productId, string productName, int? categoryId, DateTime? startTime = null, DateTime? endTime = null);

        /// <summary>
        /// 库存滞销报表API
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="brandIds"></param>
        /// <param name="productId"></param>
        /// <param name="categoryId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        IList<StockUnsalable> GetStockUnsalableAPI(int? storeId, int[] brandIds, int? productId, int? categoryId, DateTime? startTime = null, DateTime? endTime = null);

        /// <summary>
        /// 库存之下报表
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="productId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="brandId"></param>
        /// <param name="categoryId"></param>
        /// <param name="lessNetSaleQuantity"></param>
        /// <returns></returns>
        IList<StockUnsalable> GetStockUnsalable(int? storeId, int? productId, string productName, DateTime? startTime = null, DateTime? endTime = null, int? wareHouseId = 0, int? brandId = 0, int? categoryId = 0,
            int? lessNetSaleQuantity = 0);

        /// <summary>
        /// 库存预警表
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="categoryId"></param>
        /// <param name="brandId"></param>
        /// <param name="unitShowTypeId"></param>
        /// <returns></returns>
        IList<EarlyWarning> GetEarlyWarning(int? storeId, int? wareHouseId = 0, int? categoryId = 0, int? brandId = 0,
              int? unitShowTypeId = -1);


        /// <summary>
        /// 临期预警表
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="wareHouseId"></param>
        /// <param name="categoryId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        IList<ExpirationWarning> GetExpirationWarning(int? storeId, int? wareHouseId = 0, int? categoryId = 0, int? productId = 0, string productName = null);

        IPagedList<StockReportProduct> GetStockReportProduct(int? storeId, int? wareHouseId, int? categoryId, int? productId, string productName, int? brandId, bool? status, int? maxStock, bool? showZeroStack, int pageIndex, int pageSize = 50);

        Tuple<IList<StockInOutRecordQuery>, int> AsyncStockInOutRecords(int? store, int? productId, int pageIndex = 0, int pageSize = 10);
        Tuple<IList<StockFlowQuery>, int> AsyncStockFlows(int? store, int? productId, int pageIndex = 0, int pageSize = 10);
    }
}