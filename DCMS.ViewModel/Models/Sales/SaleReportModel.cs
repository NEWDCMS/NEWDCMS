using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Sales;
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DCMS.ViewModel.Models.Sales
{

    #region 销售明细表
    /// <summary>
    /// 销售明细表
    /// </summary>
    public class SaleReportItemListModel : SaleReportQueryModel
    {
        public SaleReportItemListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<SaleReportItem>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<SaleReportItem> Items { get; set; }

        #region  用于条件检索

        [HintDisplayName("销售类型", "销售类型")]
        public int? SaleTypeId { get; set; } = 0;
        public string SaleTypeName { get; set; }
        public SelectList SaleTypes { get; set; }

        [HintDisplayName("费用合同兑现商品", " 费用合同兑现商品")]
        public bool? CostContractProduct { get; set; }



        #endregion


        #region 小计、合计

        /// <summary>
        /// 数量（当前页）
        /// </summary>
        public string PageSumQuantityConversion { get; set; }
        /// <summary>
        /// 数量（总）
        /// </summary>
        public string TotalSumQuantityConversion { get; set; }

        /// <summary>
        /// 金额（当前页）
        /// </summary>
        public decimal? PageSumAmount { get; set; } = 0;
        /// <summary>
        /// 金额（总）
        /// </summary>
        public decimal? TotalSumAmount { get; set; } = 0;

        /// <summary>
        /// 成本金额（当前页）
        /// </summary>
        public decimal? PageSumCostAmount { get; set; } = 0;
        /// <summary>
        /// 成本金额（总）
        /// </summary>
        public decimal? TotalSumCostAmount { get; set; } = 0;

        /// <summary>
        /// 利润（当前页）
        /// </summary>
        public decimal? PageSumProfit { get; set; } = 0;
        /// <summary>
        /// 利润（总）
        /// </summary>
        public decimal? TotalSumProfit { get; set; } = 0;

        /// <summary>
        /// 成本利润率（当前页）
        /// </summary>
        public decimal? PageSumCostProfitRate { get; set; } = 0;
        /// <summary>
        /// 成本利润率（总）
        /// </summary>
        public decimal? TotalSumCostProfitRate { get; set; } = 0;

        /// <summary>
        /// 变动差额（当前页）
        /// </summary>
        public decimal? PageSumChangeDifference { get; set; } = 0;
        /// <summary>
        /// 变动差额（总）
        /// </summary>
        public decimal? TotalSumChangeDifference { get; set; } = 0;


        #endregion

    }
    #endregion

    #region 销售汇总（按商品）
    /// <summary>
    /// 销售汇总（按商品）
    /// </summary>
    public class SaleReportSummaryProductListModel : SaleReportQueryModel
    {
        public SaleReportSummaryProductListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<SaleReportSummaryProduct>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<SaleReportSummaryProduct> Items { get; set; }

        #region  用于条件检索

        [HintDisplayName("费用合同兑现商品", " 费用合同兑现商品")]
        public bool? CostContractProduct { get; set; }

        #endregion


        #region 总计

        /// <summary>
        /// 销售数量（总）
        /// </summary>
        public string TotalSumSaleQuantityConversion { get; set; }

        /// <summary>
        /// 销售金额（总）
        /// </summary>
        public decimal? TotalSumSaleAmount { get; set; } = 0;

        /// <summary>
        /// 赠送数量（总）
        /// </summary>
        public string TotalSumGiftQuantityConversion { get; set; }

        /// <summary>
        /// 退货数量（总）
        /// </summary>
        public string TotalSumReturnQuantityConversion { get; set; }

        /// <summary>
        /// 退货金额（总）
        /// </summary>
        public decimal? TotalSumReturnAmount { get; set; } = 0;

        /// <summary>
        /// 净销售量（总）
        /// </summary>
        public string TotalSumNetQuantityConversion { get; set; }

        /// <summary>
        /// 销售净额（总）
        /// </summary>
        public decimal? TotalSumNetAmount { get; set; } = 0;

        /// <summary>
        /// 成本金额（总）
        /// </summary>
        public decimal? TotalSumCostAmount { get; set; } = 0;

        /// <summary>
        /// 利润（总）
        /// </summary>
        public decimal? TotalSumProfit { get; set; } = 0;

        /// <summary>
        /// 成本利润率（总）
        /// </summary>
        public decimal? TotalSumCostProfitRate { get; set; } = 0;


        #endregion


    }
    #endregion

    #region 销售汇总（按客户）
    /// <summary>
    /// 销售汇总（按客户）
    /// </summary>
    public class SaleReportSummaryCustomerListModel : SaleReportQueryModel
    {
        public SaleReportSummaryCustomerListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<SaleReportSummaryCustomer>();
            DynamicColumns = new List<string>();
            TotalDynamicDatas = new List<SaleReportSumStatisticalType>();

        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<SaleReportSummaryCustomer> Items { get; set; }
        public List<string> DynamicColumns { get; set; }
        public IList<SaleReportSumStatisticalType> TotalDynamicDatas { get; set; }

        #region  用于条件检索
        #endregion


        #region 合计

        /// <summary>
        /// 销售数量
        /// </summary>
        public int? TotalSumSaleSmallQuantity { get; set; } = 0;

        /// <summary>
        /// 退货数量
        /// </summary>
        public int? TotalSumReturnSmallQuantity { get; set; } = 0;

        /// <summary>
        /// 净销售量 = 销售数量 - 退货数量
        /// </summary>
        public int? TotalSumNetSmallQuantity { get; set; } = 0;

        /// <summary>
        /// 销售金额
        /// </summary>
        public decimal? TotalSumSaleAmount { get; set; } = 0;

        /// <summary>
        /// 退货金额
        /// </summary>
        public decimal? TotalSumReturnAmount { get; set; } = 0;

        /// <summary>
        /// 销售净额 = 销售金额 - 退货金额
        /// </summary>
        public decimal? TotalSumNetAmount { get; set; } = 0;

        /// <summary>
        /// 优惠金额
        /// </summary>
        public decimal? TotalSumDiscountAmount { get; set; } = 0;

        /// <summary>
        /// 成本金额?
        /// </summary>
        public decimal? TotalSumCostAmount { get; set; } = 0;

        /// <summary>
        /// 利润
        /// </summary>
        public decimal? TotalSumProfit { get; set; } = 0;

        /// <summary>
        /// 成本利润率
        /// </summary>
        public decimal? TotalSumCostProfitRate { get; set; } = 0;
        /// <summary>
        /// 赠送数量
        /// </summary>
        public int? TotalSumGiftQuantity { get; set; } = 0;
        #endregion


    }

    #endregion

    #region 销售汇总（按业务员）
    /// <summary>
    /// 销售汇总（按业务员）
    /// </summary>
    public class SaleReportSummaryBusinessUserListModel : SaleReportQueryModel
    {
        public SaleReportSummaryBusinessUserListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<SaleReportSummaryBusinessUser>();
            DynamicColumns = new List<string>();
            TotalDynamicDatas = new List<SaleReportSumStatisticalType>();

        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<SaleReportSummaryBusinessUser> Items { get; set; }
        public List<string> DynamicColumns { get; set; }
        public IList<SaleReportSumStatisticalType> TotalDynamicDatas { get; set; }

        #region  用于条件检索

        #endregion

        #region 合计

        /// <summary>
        /// 销售数量
        /// </summary>
        public int? TotalSumSaleSmallQuantity { get; set; } = 0;

        /// <summary>
        /// 退货数量
        /// </summary>
        public int? TotalSumReturnSmallQuantity { get; set; } = 0;

        /// <summary>
        /// 净销售量 = 销售数量 - 退货数量
        /// </summary>
        public int? TotalSumNetSmallQuantity { get; set; } = 0;

        /// <summary>
        /// 销售金额
        /// </summary>
        public decimal? TotalSumSaleAmount { get; set; } = 0;

        /// <summary>
        /// 退货金额
        /// </summary>
        public decimal? TotalSumReturnAmount { get; set; } = 0;

        /// <summary>
        /// 销售净额 = 销售金额 - 退货金额
        /// </summary>
        public decimal? TotalSumNetAmount { get; set; } = 0;

        /// <summary>
        /// 优惠金额
        /// </summary>
        public decimal? TotalSumDiscountAmount { get; set; } = 0;

        /// <summary>
        /// 成本金额?
        /// </summary>
        public decimal? TotalSumCostAmount { get; set; } = 0;

        /// <summary>
        /// 利润
        /// </summary>
        public decimal? TotalSumProfit { get; set; } = 0;

        /// <summary>
        /// 成本利润率
        /// </summary>
        public decimal? TotalSumCostProfitRate { get; set; } = 0;
        /// <summary>
        /// 赠送数量
        /// </summary>
        public int? TotalSumGiftQuantity { get; set; } = 0;

        #endregion

    }


    /// <summary>
    /// 业务员排行榜（API）
    /// </summary>
    public class BusinessRankingModel : BaseEntityModel
    {

        /// <summary>
        /// 业务员
        /// </summary>
        public int BusinessUserId { get; set; } = 0;
        public string BusinessUserName { get; set; }


        /// <summary>
        /// 利润
        /// </summary>
        public decimal? Profit { get; set; } = 0;

        /// <summary>
        /// 销售
        /// </summary>
        public decimal? SaleAmount { get; set; } = 0;

        /// <summary>
        /// 销退
        /// </summary>
        public decimal? SaleReturnAmount { get; set; } = 0;

        /// <summary>
        /// 净额
        /// </summary>
        public decimal? NetAmount { get; set; } = 0;
    }

    #endregion

    #region 销售汇总（按客户/商品）
    /// <summary>
    /// 销售汇总（按客户商品）
    /// </summary>
    public class SaleReportSummaryCustomerProductListModel : SaleReportQueryModel
    {
        public SaleReportSummaryCustomerProductListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<SaleReportSummaryCustomerProduct>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<SaleReportSummaryCustomerProduct> Items { get; set; }

        #region  用于条件检索

        #endregion


        #region 总计

        /// <summary>
        /// 销售数量（总）
        /// </summary>
        public string TotalSumSaleQuantityConversion { get; set; }

        /// <summary>
        /// 销售金额（总）
        /// </summary>
        public decimal? TotalSumSaleAmount { get; set; } = 0;

        /// <summary>
        /// 退货数量（总）
        /// </summary>
        public string TotalSumReturnQuantityConversion { get; set; }

        /// <summary>
        /// 退货金额（总）
        /// </summary>
        public decimal? TotalSumReturnAmount { get; set; } = 0;

        /// <summary>
        /// 还货数量（总）
        /// </summary>
        public string TotalSumRepaymentQuantityConversion { get; set; }

        /// <summary>
        /// 还货金额（总）
        /// </summary>
        public decimal? TotalSumRepaymentAmount { get; set; } = 0;

        /// <summary>
        /// 总数量
        /// </summary>
        public string TotalSumQuantityConversion { get; set; }

        /// <summary>
        /// 总金额
        /// </summary>
        public decimal? TotalSumAmount { get; set; } = 0;

        /// <summary>
        /// 成本金额（总）
        /// </summary>
        public decimal? TotalSumCostAmount { get; set; } = 0;

        /// <summary>
        /// 利润（总）
        /// </summary>
        public decimal? TotalSumProfit { get; set; } = 0;

        /// <summary>
        /// 成本利润率（总）
        /// </summary>
        public decimal? TotalSumCostProfitRate { get; set; } = 0;

        /// <summary>
        /// 赠送数量（总）
        /// </summary>
        public string TotalSumGiftQuantityConversion { get; set; }
        #endregion


    }
    #endregion

    #region 销售汇总（按仓库）
    /// <summary>
    /// 销售汇总（按仓库）
    /// </summary>
    public class SaleReportSummaryWareHouseListModel : SaleReportQueryModel
    {
        public SaleReportSummaryWareHouseListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<SaleReportSummaryWareHouse>();
            DynamicColumns = new List<string>();
            TotalDynamicDatas = new List<SaleReportSumStatisticalType>();

        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<SaleReportSummaryWareHouse> Items { get; set; }
        public List<string> DynamicColumns { get; set; }
        public IList<SaleReportSumStatisticalType> TotalDynamicDatas { get; set; }

        #region  用于条件检索

        #endregion

        #region 合计

        /// <summary>
        /// 销售数量
        /// </summary>
        public int? TotalSumSaleSmallQuantity { get; set; } = 0;

        /// <summary>
        /// 退货数量
        /// </summary>
        public int? TotalSumReturnSmallQuantity { get; set; } = 0;

        /// <summary>
        /// 净销售量 = 销售数量 - 退货数量
        /// </summary>
        public int? TotalSumNetSmallQuantity { get; set; } = 0;

        /// <summary>
        /// 销售金额
        /// </summary>
        public decimal? TotalSumSaleAmount { get; set; } = 0;

        /// <summary>
        /// 退货金额
        /// </summary>
        public decimal? TotalSumReturnAmount { get; set; } = 0;

        /// <summary>
        /// 销售净额 = 销售金额 - 退货金额
        /// </summary>
        public decimal? TotalSumNetAmount { get; set; } = 0;

        /// <summary>
        /// 优惠金额
        /// </summary>
        public decimal? TotalSumDiscountAmount { get; set; } = 0;

        /// <summary>
        /// 成本金额?
        /// </summary>
        public decimal? TotalSumCostAmount { get; set; } = 0;

        /// <summary>
        /// 利润
        /// </summary>
        public decimal? TotalSumProfit { get; set; } = 0;

        /// <summary>
        /// 成本利润率
        /// </summary>
        public decimal? TotalSumCostProfitRate { get; set; } = 0;
        /// <summary>
        /// 赠送数量
        /// </summary>
        public int? TotalSumGiftSmallQuantity { get; set; } = 0;
        #endregion

    }

    #endregion

    #region 销售汇总（按品牌）


    /// <summary>
    /// 销售汇总（按品牌）
    /// </summary>
    public class SaleReportSummaryBrandListModel : SaleReportQueryModel
    {
        public SaleReportSummaryBrandListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<SaleReportSummaryBrand>();
            DynamicColumns = new List<string>();
            TotalDynamicDatas = new List<SaleReportSumStatisticalType>();

        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<SaleReportSummaryBrand> Items { get; set; }
        public List<string> DynamicColumns { get; set; }
        public IList<SaleReportSumStatisticalType> TotalDynamicDatas { get; set; }

        #region  用于条件检索
        #endregion

        #region 合计

        /// <summary>
        /// 销售数量
        /// </summary>
        public int? TotalSumSaleSmallQuantity { get; set; } = 0;

        /// <summary>
        /// 退货数量
        /// </summary>
        public int? TotalSumReturnSmallQuantity { get; set; } = 0;

        /// <summary>
        /// 净销售量 = 销售数量 - 退货数量
        /// </summary>
        public int? TotalSumNetSmallQuantity { get; set; } = 0;

        /// <summary>
        /// 销售金额
        /// </summary>
        public decimal? TotalSumSaleAmount { get; set; } = 0;

        /// <summary>
        /// 退货金额
        /// </summary>
        public decimal? TotalSumReturnAmount { get; set; } = 0;

        /// <summary>
        /// 销售净额 = 销售金额 - 退货金额
        /// </summary>
        public decimal? TotalSumNetAmount { get; set; } = 0;

        /// <summary>
        /// 优惠金额
        /// </summary>
        public decimal? TotalSumDiscountAmount { get; set; } = 0;

        /// <summary>
        /// 成本金额?
        /// </summary>
        public decimal? TotalSumCostAmount { get; set; } = 0;

        /// <summary>
        /// 利润
        /// </summary>
        public decimal? TotalSumProfit { get; set; } = 0;

        /// <summary>
        /// 成本利润率
        /// </summary>
        public decimal? TotalSumCostProfitRate { get; set; } = 0;
        /// <summary>
        /// 赠送数量
        /// </summary>
        public int? TotalSumGiftSmallQuantity { get; set; } = 0;
        #endregion

        /// <summary>
        /// 图表数据
        /// </summary>
        public string Charts { get; set; }

    }


    /// <summary>
    /// 品牌销量汇总(API)
    /// </summary>
    public class BrandRankingModel : BaseEntityModel
    {
        /// <summary>
        /// 品牌
        /// </summary>
        public int BrandId { get; set; } = 0;
        public string BrandName { get; set; }


        /// <summary>
        /// 利润
        /// </summary>
        public decimal? Profit { get; set; } = 0;

        /// <summary>
        /// 销售
        /// </summary>
        public decimal? SaleAmount { get; set; } = 0;

        /// <summary>
        /// 销退
        /// </summary>
        public decimal? SaleReturnAmount { get; set; } = 0;

        /// <summary>
        /// 净额
        /// </summary>
        public decimal? NetAmount { get; set; } = 0;

        /// <summary>
        /// 比例
        /// </summary>
        public double? Percentage { get; set; } = 0;
    }

    #endregion

    #region 订单明细
    /// <summary>
    /// 订单明细
    /// </summary>
    public class SaleReportOrderItemListModel : SaleReportQueryModel
    {
        public SaleReportOrderItemListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<SaleReportOrderItem>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<SaleReportOrderItem> Items { get; set; }

        #region  用于条件检索

        [HintDisplayName("销售类型", "销售类型")]
        public int? SaleTypeId { get; set; } = 0;
        public string SaleTypeName { get; set; }
        public SelectList SaleTypes { get; set; }

        /// <summary>
        /// 费用合同兑现商品
        /// </summary>
        [HintDisplayName("费用合同兑现商品", "费用合同兑现商品")]
        public bool? CostContractProduct { get; set; }
        /// <summary>
        /// 只展示占用库存商品
        /// </summary>
        [HintDisplayName("只展示占用库存商品", "只展示占用库存商品")]
        public bool? OccupyStock { get; set; }

        #endregion

        #region 小计、合计

        /// <summary>
        /// 数量（当前页）
        /// </summary>
        public string PageSumQuantityConversion { get; set; }
        /// <summary>
        /// 数量（总）
        /// </summary>
        public string TotalSumQuantityConversion { get; set; }

        /// <summary>
        /// 金额（当前页）
        /// </summary>
        public decimal? PageSumAmount { get; set; } = 0;
        /// <summary>
        /// 金额（总）
        /// </summary>
        public decimal? TotalSumAmount { get; set; } = 0;

        /// <summary>
        /// 成本金额（当前页）
        /// </summary>
        public decimal? PageSumCostAmount { get; set; } = 0;
        /// <summary>
        /// 成本金额（总）
        /// </summary>
        public decimal? TotalSumCostAmount { get; set; } = 0;

        /// <summary>
        /// 利润（当前页）
        /// </summary>
        public decimal? PageSumProfit { get; set; } = 0;
        /// <summary>
        /// 利润（总）
        /// </summary>
        public decimal? TotalSumProfit { get; set; } = 0;

        /// <summary>
        /// 成本利润率（当前页）
        /// </summary>
        public decimal? PageSumCostProfitRate { get; set; } = 0;
        /// <summary>
        /// 成本利润率（总）
        /// </summary>
        public decimal? TotalSumCostProfitRate { get; set; } = 0;

        #endregion

    }
    #endregion

    #region 订单汇总（按商品）
    /// <summary>
    /// 订单汇总（按商品）
    /// </summary>
    public class SaleReportSummaryOrderProductListModel : SaleReportQueryModel
    {
        public SaleReportSummaryOrderProductListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<SaleReportSummaryOrderProduct>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<SaleReportSummaryOrderProduct> Items { get; set; }

        #region  用于条件检索

        /// <summary>
        /// 费用合同兑现商品 
        /// </summary>
        [HintDisplayName("费用合同兑现商品", "费用合同兑现商品")]
        public bool? CostContractProduct { get; set; }


        #endregion


        #region 总计

        /// <summary>
        /// 订单数量（总）
        /// </summary>
        public string TotalSumSaleQuantityConversion { get; set; }

        /// <summary>
        /// 订单金额（总）
        /// </summary>
        public decimal? TotalSumSaleAmount { get; set; } = 0;

        /// <summary>
        /// 赠送数量（总）
        /// </summary>
        public string TotalSumGiftQuantityConversion { get; set; }

        /// <summary>
        /// 退货数量（总）
        /// </summary>
        public string TotalSumReturnQuantityConversion { get; set; }

        /// <summary>
        /// 退货金额（总）
        /// </summary>
        public decimal? TotalSumReturnAmount { get; set; } = 0;

        /// <summary>
        /// 净销售量（总）
        /// </summary>
        public string TotalSumNetQuantityConversion { get; set; }

        /// <summary>
        /// 销售净额（总）
        /// </summary>
        public decimal? TotalSumNetAmount { get; set; } = 0;

        /// <summary>
        /// 成本金额（总）
        /// </summary>
        public decimal? TotalSumCostAmount { get; set; } = 0;

        /// <summary>
        /// 利润（总）
        /// </summary>
        public decimal? TotalSumProfit { get; set; } = 0;

        /// <summary>
        /// 成本利润率（总）
        /// </summary>
        public decimal? TotalSumCostProfitRate { get; set; } = 0;


        #endregion


    }
    #endregion

    #region 费用合同明细表
    /// <summary>
    /// 费用合同明细表
    /// </summary>
    public class SaleReportCostContractItemListModel : SaleReportQueryModel
    {
        public SaleReportCostContractItemListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<SaleReportCostContractItem>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<SaleReportCostContractItem> Items { get; set; }

        #region  用于条件检索

        //兑现方式
        [HintDisplayName("兑现方式", "兑现方式")]
        public int? CashTypeId { get; set; } = 0;
        public string CashTypeName { get; set; }
        public SelectList CashTypes { get; set; }

        //状态
        [HintDisplayName("状态", "状态")]
        public int? StatusTypeId { get; set; } = 0;
        public string StatusTypeName { get; set; }
        public SelectList StatusTypes { get; set; }

        public int? AccountingOptionId { get; set; }
        [HintDisplayName("费用类别", "费用类别")]
        public string AccountingOptionName { get; set; }

        #endregion


        #region 小计、合计

        /// <summary>
        /// 数量（总）
        /// </summary>
        public string TotalSumQuantityConversion { get; set; }

        #endregion

    }
    #endregion

    #region 赠品汇总
    /// <summary>
    /// 赠品汇总
    /// </summary>
    public class SaleReportSummaryGiveQuotaListModel : SaleReportQueryModel
    {
        public SaleReportSummaryGiveQuotaListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<GiveQuotaRecordsSummery>();
            RemarkConfigs = new List<RemarkConfig>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<GiveQuotaRecordsSummery> Items { get; set; }
        public IList<RemarkConfig> RemarkConfigs { get; set; }
        public IList<TotalOrdinaryGiftSummery> TotalOrdinaryGiftSummerys { get; set; }

        #region  用于条件检索
        public string TotalGeneralQuantity { get; set; }
        public decimal TotalGeneralCostAmount { get; set; } = 0;

        /// <summary>
        /// 订货赠品
        /// </summary>
        public string TotalOrderQuantity { get; set; }
        public decimal TotalOrderCostAmount { get; set; } = 0;

        /// <summary>
        /// 促销赠品
        /// </summary>
        public string TotalPromotionalQuantity { get; set; }
        public decimal TotalPromotionalCostAmount { get; set; } = 0;

        /// <summary>
        /// 费用合同
        /// </summary>
        public string TotalContractQuantity { get; set; }
        public decimal TotalContractCostAmount { get; set; } = 0;
        #endregion


    }

    public class TotalOrdinaryGiftSummery
    {
        public int RemarkConfigId { get; set; }

        public int TotalBigQuantity { get; set; } = 0;

        public int TotalStockQuantity { get; set; } = 0;

        public int TotalSmailQuantity { get; set; } = 0;

        public decimal TotalCostAmount { get; set; } = 0;
    }
    #endregion

    #region 热销排行榜

    /// <summary>
    /// 热销排行榜
    /// </summary>
    public class SaleReportHotSaleListModel : SaleReportQueryModel
    {
        public SaleReportHotSaleListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<SaleReportHotSale>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<SaleReportHotSale> Items { get; set; }

        #region  用于条件检索

        [DisplayName("统计前")]
        public int? TopNumber { get; set; } = 0;

        #endregion

        #region 合计

        /// <summary>
        /// 销售数量
        /// </summary>
        public string TotalSumSaleQuantityConversion { get; set; }

        /// <summary>
        /// 销售金额
        /// </summary>
        public decimal? TotalSumSaleAmount { get; set; } = 0;

        /// <summary>
        /// 退货数量
        /// </summary>
        public string TotalSumReturnQuantityConversion { get; set; }

        /// <summary>
        /// 退货金额
        /// </summary>
        public decimal? TotalSumReturnAmount { get; set; } = 0;

        /// <summary>
        /// 净销售量 = 销售数量 - 退货数量
        /// </summary>
        public string TotalSumNetQuantityConversion { get; set; }

        /// <summary>
        /// 销售净额 = 销售金额 - 退货金额
        /// </summary>
        public decimal? TotalSumNetAmount { get; set; } = 0;

        /// <summary>
        /// 成本金额
        /// </summary>
        public decimal? TotalSumCostAmount { get; set; } = 0;

        /// <summary>
        /// 利润
        /// </summary>
        public decimal? TotalSumProfit { get; set; } = 0;

        /// <summary>
        /// 成本利润率
        /// </summary>
        public decimal? TotalSumCostProfitRate { get; set; } = 0;

        #endregion

        /// <summary>
        /// 图表数据
        /// </summary>
        public string Charts { get; set; }

    }


    /// <summary>
    /// 热销排行榜(API)
    /// </summary>
    public class HotSaleRankingModel : SalwReportAPIModel
    {
        /// <summary>
        /// 销售数量
        /// </summary>
        public int TotalSumSaleQuantity { get; set; } = 0;

        /// <summary>
        /// 销售金额
        /// </summary>
        public decimal? TotalSumSaleAmount { get; set; } = 0;

        /// <summary>
        /// 退货数量
        /// </summary>
        public int TotalSumReturnQuantity { get; set; } = 0;

        /// <summary>
        /// 退货金额
        /// </summary>
        public decimal? TotalSumReturnAmount { get; set; } = 0;

        /// <summary>
        /// 净销售量 = 销售数量 - 退货数量
        /// </summary>
        public int TotalSumNetQuantity { get; set; } = 0;

        /// <summary>
        /// 销售净额 = 销售金额 - 退货金额
        /// </summary>
        public decimal? TotalSumNetAmount { get; set; } = 0;


        /// <summary>
        /// 退货率 = 退货数量  /  销售数量
        /// </summary>
        public double? ReturnRate { get; set; } = 0;

    }


    #endregion

    #region 销量走势图
    /// <summary>
    /// 销量走势图
    /// </summary>
    public class SaleReportSaleQuantityTrendListModel : SaleReportQueryModel
    {
        public SaleReportSaleQuantityTrendListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<SaleReportSaleQuantityTrend>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<SaleReportSaleQuantityTrend> Items { get; set; }

        #region  用于条件检索

        [HintDisplayName("统计方式", "统计方式")]
        public int? GroupByTypeId { get; set; } = 0;
        public SelectList GroupByTypes { get; set; }

        #endregion

        #region 合计

        #endregion

        /// <summary>
        /// 图表数据
        /// </summary>
        public string Charts { get; set; }

    }

    /// <summary>
    /// 销量走势图(API)
    /// </summary>
    public class SaleTrending
    {

        /// <summary>
        /// 日期类型
        /// </summary>
        public string DateType { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public DateTime SaleDate { get; set; }

        /// <summary>
        /// 销售
        /// </summary>
        public decimal? SaleAmount { get; set; } = 0;

        /// <summary>
        /// 销退
        /// </summary>
        public decimal? SaleReturnAmount { get; set; } = 0;

        /// <summary>
        /// 净额
        /// </summary>
        public decimal? NetAmount { get; set; } = 0;


    }

    #endregion

    #region 销售商品成本利润
    /// <summary>
    /// 销售商品成本利润
    /// </summary>
    public class SaleReportProductCostProfitListModel : SaleReportQueryModel
    {
        public SaleReportProductCostProfitListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<SaleReportProductCostProfit>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<SaleReportProductCostProfit> Items { get; set; }

        #region  用于条件检索
        #endregion


        #region 总计

        /// <summary>
        /// 销售数量（总）
        /// </summary>
        public string TotalSumSaleQuantityConversion { get; set; }

        /// <summary>
        /// 销售金额（总）
        /// </summary>
        public decimal? TotalSumSaleAmount { get; set; } = 0;

        /// <summary>
        /// 退货数量（总）
        /// </summary>
        public string TotalSumReturnQuantityConversion { get; set; }

        /// <summary>
        /// 退货金额（总）
        /// </summary>
        public decimal? TotalSumReturnAmount { get; set; } = 0;

        /// <summary>
        /// 净销售量（总）
        /// </summary>
        public string TotalSumNetQuantityConversion { get; set; }

        /// <summary>
        /// 销售净额（总）
        /// </summary>
        public decimal? TotalSumNetAmount { get; set; } = 0;

        /// <summary>
        /// 成本金额（总）
        /// </summary>
        public decimal? TotalSumCostAmount { get; set; } = 0;

        /// <summary>
        /// 利润（总）
        /// </summary>
        public decimal? TotalSumProfit { get; set; } = 0;

        /// <summary>
        /// 销售利润率（总）
        /// </summary>
        public decimal? TotalSumSaleProfitRate { get; set; } = 0;

        /// <summary>
        /// 成本利润率（总）
        /// </summary>
        public decimal? TotalSumCostProfitRate { get; set; } = 0;


        #endregion


    }

    /// <summary>
    /// 销售商品成本利润(API)
    /// </summary>
    public class CostProfitRankingModel : SalwReportAPIModel
    {
        /// <summary>
        /// 净销售量
        /// </summary>
        public int? TotalSumNetQuantity { get; set; } = 0;

        /// <summary>
        /// 净销售量单位转化 如：xx 箱 xx 瓶
        /// </summary>
        public string TotalSumNetQuantityConversion { get; set; }

        /// <summary>
        /// 销售净额
        /// </summary>
        public decimal? TotalSumNetAmount { get; set; } = 0;

        /// <summary>
        /// 利润
        /// </summary>
        public decimal? TotalSumProfit { get; set; } = 0;


    }



    #endregion

    #region 销售额分析（API）
    /// <summary>
    /// 销售额分析（API）
    /// </summary>
    public class SaleAnalysisModel : SalwReportAPIModel
    {
        /// <summary>
        /// 今日
        /// </summary>
        public Sale Today { get; set; } = new Sale();

        /// <summary>
        /// 今日上周同期
        /// </summary>
        public Sale SamePeriodLastWeek { get; set; } = new Sale();

        /// <summary>
        /// 昨天
        /// </summary>
        public Sale Yesterday { get; set; } = new Sale();
        /// <summary>
        /// 前天
        /// </summary>
        public Sale BeforeYesterday { get; set; } = new Sale();
        /// <summary>
        /// 上周
        /// </summary>
        public Sale LastWeek { get; set; } = new Sale();
        /// <summary>
        /// 本周
        /// </summary>
        public Sale ThisWeek { get; set; } = new Sale();
        /// <summary>
        /// 上月
        /// </summary>
        public Sale LastMonth { get; set; } = new Sale();
        /// <summary>
        /// 本月
        /// </summary>
        public Sale ThisMonth { get; set; } = new Sale();
        /// <summary>
        /// 本年
        /// </summary>
        public Sale ThisYear { get; set; } = new Sale();


        /// <summary>
        /// 嵌套
        /// </summary>
        public class Sale
        {

            /// <summary>
            /// 销售
            /// </summary>
            public decimal? SaleAmount { get; set; } = 0;

            /// <summary>
            /// 销退
            /// </summary>
            public decimal? SaleReturnAmount { get; set; } = 0;

            /// <summary>
            /// 净额
            /// </summary>
            public decimal? NetAmount { get; set; } = 0;


        }

    }
    #endregion

    #region 客户拜访分析（API）
    /// <summary>
    /// 客户拜访分析（API）
    /// </summary>
    public class CustomerVistAnalysisModel : SalwReportAPIModel
    {
        /// <summary>
        /// 总客户数
        /// </summary>
        public int TotalCustomer { get; set; } = 0;


        /// <summary>
        /// 今日
        /// </summary>
        public Vist Today { get; set; } = new Vist();

        /// <summary>
        /// 昨天
        /// </summary>
        public Vist Yesterday { get; set; } = new Vist();
        /// <summary>
        /// 前天
        /// </summary>
        public Vist BeforeYesterday { get; set; } = new Vist();
        /// <summary>
        /// 上周
        /// </summary>
        public Vist LastWeek { get; set; } = new Vist();
        /// <summary>
        /// 本周
        /// </summary>
        public Vist ThisWeek { get; set; } = new Vist();
        /// <summary>
        /// 上月
        /// </summary>
        public Vist LastMonth { get; set; } = new Vist();
        /// <summary>
        /// 本月
        /// </summary>
        public Vist ThisMonth { get; set; } = new Vist();

        /// <summary>
        /// 本年
        /// </summary>
        public Vist ThisYear { get; set; } = new Vist();


        /// <summary>
        /// 嵌套
        /// </summary>
        public class Vist
        {
            /// <summary>
            /// 拜访数
            /// </summary>
            public int? VistCount { get; set; } = 0;

            /// <summary>
            /// 拜访比例 = 拜访数/总客户数
            /// </summary>
            public double? Percentage { get; set; } = 0;



        }
    }
    #endregion

    #region 新增加客户分析（API）
    /// <summary>
    /// 新增加客户分析（API）
    /// </summary>
    public class NewCustomerAnalysisModel : SalwReportAPIModel
    {

        /// <summary>
        /// 总客户数
        /// </summary>
        public int TotalCustomer { get; set; } = 0;


        /// <summary>
        /// 今日
        /// </summary>
        public Signin Today { get; set; } = new Signin();
        /// <summary>
        /// 今日上周同期签约客户
        /// </summary>
        public Signin SamePeriodLastWeek { get; set; } = new Signin();

        /// <summary>
        /// 昨天
        /// </summary>
        public Signin Yesterday { get; set; } = new Signin();
        /// <summary>
        /// 前天
        /// </summary>
        public Signin BeforeYesterday { get; set; } = new Signin();
        /// <summary>
        /// 上周
        /// </summary>
        public Signin LastWeek { get; set; } = new Signin();
        /// <summary>
        /// 本周
        /// </summary>
        public Signin ThisWeek { get; set; } = new Signin();
        /// <summary>
        /// 上月
        /// </summary>
        public Signin LastMonth { get; set; } = new Signin();
        /// <summary>
        /// 本月
        /// </summary>
        public Signin ThisMonth { get; set; } = new Signin();

        /// <summary>
        /// 本年
        /// </summary>
        public Signin ThisYear { get; set; } = new Signin();

        /// <summary>
        /// 嵌套
        /// </summary>
        public class Signin
        {
            /// <summary>
            /// 签约客户数
            /// </summary>
            public int? Count { get; set; } = 0;

            /// <summary>
            /// 签约客户的Id 列表
            /// </summary>
            public List<int> TerminalIds { get; set; } = new List<int>();
        }

        /// <summary>
        /// 月统计
        /// </summary>
        public Dictionary<string, double> ChartDatas { get; set; } = new Dictionary<string, double>();
    }
    #endregion

    #region 新增订单分析（API）
    /// <summary>
    /// 新增订单分析（API）
    /// </summary>
    public class NewOrderAnalysisModel : SalwReportAPIModel
    {
        public int TotalOrders { get; set; } = 0;

        /// <summary>
        /// 今日
        /// </summary>
        public Order Today { get; set; } = new Order();
        /// <summary>
        /// 今日上周同期签约客户
        /// </summary>
        public Order SamePeriodLastWeek { get; set; } = new Order();

        /// <summary>
        /// 昨天
        /// </summary>
        public Order Yesterday { get; set; } = new Order();
        /// <summary>
        /// 前天
        /// </summary>
        public Order BeforeYesterday { get; set; } = new Order();
        /// <summary>
        /// 上周
        /// </summary>
        public Order LastWeek { get; set; } = new Order();
        /// <summary>
        /// 本周
        /// </summary>
        public Order ThisWeek { get; set; } = new Order();
        /// <summary>
        /// 上月
        /// </summary>
        public Order LastMonth { get; set; } = new Order();
        /// <summary>
        /// 本月
        /// </summary>
        public Order ThisMonth { get; set; } = new Order();

        /// <summary>
        /// 本年
        /// </summary>
        public Order ThisYear { get; set; } = new Order();

        /// <summary>
        /// 嵌套
        /// </summary>
        public class Order
        {
            /// <summary>
            /// 签约客户数
            /// </summary>
            public int? Count { get; set; } = 0;

            /// <summary>
            /// 签约客户的Id 列表
            /// </summary>
            public List<int> BillIds { get; set; } = new List<int>();
        }

        /// <summary>
        /// 月统计
        /// </summary>
        public Dictionary<string, double> ChartDatas { get; set; } = new Dictionary<string, double>();
    }
    #endregion


    #region 公共类

    /// <summary>
    /// 统计类别汇总
    /// </summary>
    public class SaleReportSumStatisticalType
    {
        /// <summary>
        /// 统计类别Id
        /// </summary>
        public int StatisticalTypeId { get; set; } = 0;

        /// <summary>
        /// 净销售量 = 销售数量 - 退货数量 (最小单位数量)
        /// </summary>
        public int? NetSmallQuantity { get; set; } = 0;

        /// <summary>
        /// 销售净额 = 销售金额 - 退货金额
        /// </summary>
        public decimal? NetAmount { get; set; } = 0;

        /// <summary>
        /// 成本金额
        /// </summary>
        public decimal? CostAmount { get; set; } = 0;

        /// <summary>
        /// 利润
        /// </summary>
        public decimal? Profit { get; set; } = 0;

        /// <summary>
        /// 成本利润率
        /// </summary>
        public decimal? CostProfitRate { get; set; } = 0;

    }



    /// <summary>
    /// API公共类
    /// </summary>
    public partial class SalwReportAPIModel : BaseEntityModel
    {
        /// <summary>
        /// 商品
        /// </summary>
        public int? ProductId { get; set; } = 0;

        /// <summary>
        /// 商品
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 客户Id
        /// </summary>
        public int? TerminalId { get; set; } = 0;

        /// <summary>
        /// 业务员
        /// </summary>
        public int? BusinessUserId { get; set; } = 0;
        public string BusinessUserName { get; set; }

        /// <summary>
        /// 品牌
        /// </summary>
        public int? BrandId { get; set; } = 0;
        public string BrandName { get; set; }

        /// <summary>
        /// 商品类别
        /// </summary>
        public int? CategoryId { get; set; } = 0;
        public string CategoryName { get; set; }
    }
    #endregion


    #region 经营报表
    public partial class SaleReportSummaryBusinessDailyListModel : SaleReportQueryModel
    {
        public SaleReportSummaryBusinessDailyListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<SaleReportSummaryBusinessDaily>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<SaleReportSummaryBusinessDaily> Items { get; set; }
    }

    public partial class SaleReportSummaryBusinessMonthlyListModel : SaleReportQueryModel
    {
        public SaleReportSummaryBusinessMonthlyListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<SaleReportSummaryBusinessDaily>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<SaleReportSummaryBusinessDaily> Items { get; set; }

        #region 用于查询检索
        public int Year { get; set; }
        public int Month { get; set; }
        #endregion
    }

    public partial class SaleReportSummaryBusinessYearlyListModel : SaleReportQueryModel
    {
        public SaleReportSummaryBusinessYearlyListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<SaleReportSummaryBusinessDaily>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<SaleReportSummaryBusinessDaily> Items { get; set; }

        #region 用于查询检索
        public int Year { get; set; }
        #endregion
    }

    public class SaleReportSummaryBusinessDaily 
    {
        /// <summary>
        /// 日期
        /// </summary>
        public string DateName { get; set; }
        /// <summary>
        /// 业务员
        /// </summary>
        public string BusinessName { get; set; }
        /// <summary>
        /// 销售数量
        /// </summary>
        public int SaleQuantity { get; set; }
        /// <summary>
        /// 销售金额
        /// </summary>
        public decimal SaleAmount { get; set; }
        /// <summary>
        /// 赠送数量
        /// </summary>
        public int GiftQuantity { get; set; }
        /// <summary>
        /// 赠送成本
        /// </summary>
        public decimal GiftCostAmount { get; set; }
        /// <summary>
        /// 退货数量
        /// </summary>
        public int ReturnQuantity { get; set; }
        /// <summary>
        /// 退货金额
        /// </summary>
        public decimal ReturnAmount { get; set; }
        /// <summary>
        /// 优惠金额
        /// </summary>
        public decimal PreferentialAmount { get; set; }
        /// <summary>
        /// 收款金额
        /// </summary>
        public decimal ProceedsAmount { get; set; }
        /// <summary>
        /// 欠款金额
        /// </summary>
        public decimal BalanceAmount { get; set; }
    }
    #endregion
}