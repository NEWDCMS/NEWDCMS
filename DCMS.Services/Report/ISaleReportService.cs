using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Sales;
using System;
using System.Collections.Generic;
using DCMS.Core;

namespace DCMS.Services.Report
{
    public interface ISaleReportService
    {
        PagedList<SaleReportItem> GetSaleReportItem(int? storeId, int? productId, string productName, int? categoryId, int? brandId, int? terminalId, string terminalName, string billNumber, int? saleTypeId, int? bussinessUserId, int? wareHouseId, int? payTypeId, int? deliveryUserId, int? rankId, string remark, int? channelId, DateTime? startTime, DateTime? endTime, bool? exchange, int? districtId, bool force = false, int pageIndex = 0,int pageSize = int.MaxValue,bool? auditedStatus = null);

        IList<SaleReportSummaryProduct> GetSaleReportSummaryProduct(int? storeId, int? productId, string productName, int? categoryId, int? bussinessUserId, int? wareHouseId, int? terminalId, string terminalName, int? deliveryUserId, int? rankId, string remark, int? payTypeId, DateTime? startTime, DateTime? endTime, bool? exchange, int? channelId, int? districtId, bool force = false,bool? auditedStatus = null);

        IList<SaleReportSummaryCustomer> GetSaleReportSummaryCustomer(int? storeId, int? terminalId, string terminalName, DateTime? startTime, DateTime? endTime, int? brandId, int? productId, string productName, int? categoryId, int? districtId, int? channelId, int? rankId, string remark, int? bussinessUserId, int? wareHouseId, Dictionary<int, string> dic, bool force = false, bool? auditedStatus = null);

        IList<SaleReportSummaryBusinessUser> GetSaleReportSummaryBusinessUser(int? storeId, int? bussinessUserId, DateTime? startTime, DateTime? endTime, int? brandId, Dictionary<int, string> dic, bool force = false, bool? auditedStatus = null);

        PagedList<SaleReportSummaryCustomerProduct> GetSaleReportSummaryCustomerProduct(int? storeId, int? wareHouseId, int? productId, string productName, int? categoryId, int? brandId, int? channelId, int? rankId, int? bussinessUserId, int? deliveryUserId, int? terminalId, string terminalName, string remark, DateTime? startTime, DateTime? endTime, bool force = false, int pageIndex = 0, int pageSize = 100, bool? auditedStatus = null);

        IList<SaleReportSummaryWareHouse> GetSaleReportSummaryWareHouse(int? storeId, int? wareHouseId, DateTime? startTime, DateTime? endTime, Dictionary<int, string> dic, bool force = false, bool? auditedStatus = null);

        IList<SaleReportSummaryBrand> GetSaleReportSummaryBrand(int? storeId, int[] brandIds, int? districtId, int? channelId, int? bussinessUserId, int? deliveryUserId, DateTime? startTime, DateTime? endTime, Dictionary<int, string> dic, bool force = false, bool? auditedStatus = null);

        IList<SaleReportOrderItem> GetSaleReportOrderItem(int? storeId, int? productId, string productName, int? categoryId, int? brandId, int? terminalId, string terminalName, string billNumber, int? rankId, int? bussinessUserId, int? wareHouseId, int? saleTypeId, int? channelId, DateTime? startTime, DateTime? endTime, int? districtId, string remark, bool? costContractProduct, bool? occupyStock, bool force = false);

        IList<SaleReportSummaryOrderProduct> GetSaleReportSummaryOrderProduct(int? storeId, int? productId, string productName, int? categoryId, int? bussinessUserId, int? wareHouseId, int? districtId, int? terminalId, string terminalName, int? channelId, string remark, DateTime? startTime, DateTime? endTime, int? deliveryUserId, int? rankId, bool? costContractProduct, bool force = false);


        IList<SaleReportCostContractItem> GetSaleReportCostContractItem(int? storeId, int? terminalId, string terminalName, int? productId, string productName, int? bussinessUserId, int? accountingOptionId,
            string billNumber, int? categoryId, int? cashTypeId, string remark,
            int? statusTypeId, DateTime? startTime, DateTime? endTime, bool force = false);

        IList<GiveQuotaRecordsSummery> GetSaleReportSummaryGiveQuota(int? storeId, int? productId, int? terminalId, int? categoryId, string remark,
             DateTime? startTime, DateTime? endTime, int? bussinessUserId, bool force = false);

        IList<SaleReportHotSale> GetSaleReportHotSale(int? storeId, int? productId, string productName, int? bussinessUserId, int? wareHouseId, int? terminalId, string terminalName, int? brandId,
            int? categoryId, DateTime? startTime, DateTime? endTime, int? topNumber, bool force = false);

        IList<SaleReportHotSale> GetOrderReportHotSale(int? storeId, int? productId, int? businessUserId, int? wareHouseId, int? terminalId, int? brandId, int? categoryId, DateTime? startTime, DateTime? endTime, int? topNumber, bool force = false);

        IList<SaleReportSaleQuantityTrend> GetSaleReportSaleQuantityTrend(int? storeId, DateTime? startTime, DateTime? endTime, int? groupByTypeId, bool force = false);


        IList<SaleReportProductCostProfit> GetSaleReportProductCostProfit(int? storeId, int? productId, string productName, int? categoryId, int? brandId, int? channelId,
            int? terminalId, string terminalName, int? bussinessUserId, int? wareHouseId, DateTime? startTime, DateTime? endTime, bool force = false);


        /// <summary>
        /// 销售额分析API
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="businessUserId"></param>
        /// <param name="brandId"></param>
        /// <param name="productId"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        SaleAnalysis GetSaleAnalysis(int? storeId, int? businessUserId, int? brandId, int? productId, int? categoryId, bool force = false);

        /// <summary>
        /// 客户拜访分析
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="businessUserId"></param>
        /// <returns></returns>
        CustomerVistAnalysis GetCustomerVistAnalysis(int? storeId, int? businessUserId, bool force = false);

        /// <summary>
        /// 新增加客户分析
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="businessUserId"></param>
        /// <returns></returns>
        NewCustomerAnalysis GetNewUserAnalysis(int? storeId, int? businessUserId, bool force = false);


        /// <summary>
        /// 获取经销商品牌销量汇总API
        /// </summary>
        /// <param name="store"></param>
        /// <param name="brandIds"></param>
        /// <param name="businessUserId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        IList<BrandRanking> GetBrandRanking(int? storeId, int[] brandIds, int? businessUserId, DateTime? startTime, DateTime? endTime, bool force = false, bool? auditedStatus = null);

        /// <summary>
        /// 获取经销商业务员排行榜API
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="businessUserId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        IList<BusinessRanking> GetBusinessRanking(int? storeId, int? businessUserId, DateTime? startTime, DateTime? endTime, bool force = false);

        /// <summary>
        /// 获取客户排行榜API
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="terminalId"></param>
        /// <param name="districtId"></param>
        /// <param name="businessUserId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        IList<CustomerRanking> GetCustomerRanking(int? storeId, int? terminalId, int? districtId, int? businessUserId, DateTime? startTime, DateTime? endTime, bool force = false);

        /// <summary>
        /// 获取经销商滞销排行榜
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="businessUserId"></param>
        /// <param name="brandId"></param>
        /// <param name="categoryId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        IList<UnSaleRanking> GetUnSaleRanking(int? storeId, int? businessUserId, int? brandId, int? categoryId, DateTime? startTime, DateTime? endTime, bool force = false);


        /// <summary>
        /// 订单额分析
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="businessUserId"></param>
        /// <param name="brandId"></param>
        /// <param name="productId"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        OrderQuantityAnalysisQuery GetOrderQuantityAnalysis(int? storeId, int? businessUserId, int? brandId, int? productId, int? categoryId, bool force = false);

        /// <summary>
        /// 获取经销商热销排行榜
        /// </summary>
        /// <param name="store"></param>
        /// <param name="terminalId"></param>
        /// <param name="businessUserId"></param>
        /// <param name="brandId"></param>
        /// <param name="categoryId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        IList<HotSaleRanking> GetHotSaleRanking(int? store, int? terminalId, int? businessUserId, int? brandId, int? categoryId, DateTime? startTime, DateTime? endTime, bool force = false);

        /// <summary>
        /// 获取经销商热定(销订)排行榜
        /// </summary>
        /// <param name="store"></param>
        /// <param name="terminalId"></param>
        /// <param name="businessUserId"></param>
        /// <param name="brandId"></param>
        /// <param name="categoryId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        IList<HotSaleRanking> GetHotOrderRanking(int? store, int? terminalId, int? businessUserId, int? brandId, int? categoryId, DateTime? startTime, DateTime? endTime, bool force = false);

        NewOrderAnalysis GetNewOrderAnalysis(int? storeId, int? businessUserId, bool force = false);

        BusinessAnalysis GetBusinessAnalysis(int type, int? storeId, DateTime? start = null, DateTime? end = null, int userId = 0);

        IList<SaleReportItem> GetSaleReportData(int? storeId, int? productId, string productName, int? categoryId, int? brandId, int? terminalId, string terminalName, string billNumber, int? saleTypeId, int? bussinessUserId, int? wareHouseId, int? payTypeId, int? deliveryUserId, int? rankId, string remark, int? channelId, DateTime? startTime, DateTime? endTime, bool? exchange, int? districtId, bool force = false, bool? auditedStatus = null);

        IList<SaleReportSummaryCustomerProduct> GetSaleReportSummaryCustomerProductData(int? storeId, int? wareHouseId, int? productId, string productName, int? categoryId, int? brandId, int? channelId, int? rankId, int? bussinessUserId, int? deliveryUserId, int? terminalId, string terminalName1, string remark, DateTime? startTime, DateTime? endTime, bool force = false, int pageIndex = 0, int pageSize = 100, bool? auditedStatus = null);
        /// <summary>
        /// 获取经营日报数据
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        IList<SaleReportBusinessDaily> GetSaleReportSummaryBusinessDaily(int storeId, DateTime? start = null, DateTime? end = null, int userId = 0);
        /// <summary>
        /// 经营年报
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="year"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        IList<SaleReportBusinessDaily> GetSaleReportSummaryBusinessYearly(int storeId, int year, int userId = 0);
    }
}