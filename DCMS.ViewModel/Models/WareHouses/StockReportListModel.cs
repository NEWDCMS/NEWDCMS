using DCMS.Core.Domain.WareHouses;
using DCMS.ViewModel.Models.Products;
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DCMS.ViewModel.Models.WareHouses
{

    public partial class StockReportListModel : BaseModel
    {
        public PagingFilteringModel PagingFilteringContext { get; set; } = new PagingFilteringModel();
        public List<StockCategoryTree> StockCategoryTrees { get; set; } = new List<StockCategoryTree>();
        public IList<StockReportProduct> Items { get; set; } = new List<StockReportProduct>();


        #region  用于条件检索

        [HintDisplayName("商品类别", "商品类别")]
        public int? CategoryId { get; set; } = 0;
        public string CategoryName { get; set; }
        public SelectList Categories { get; set; }


        [HintDisplayName("仓库", "仓库")]
        public int? WareHouseId { get; set; } = 0;
        public string WareHouseName { get; set; }
        public SelectList WareHouses { get; set; }


        [HintDisplayName("数量小于", "数量小于(最大库存)")]
        public int? MaxStock { get; set; } = 0;


        [HintDisplayName("商品", "商品")]
        public int ProductId { get; set; } = 0;
        public string ProductName { get; set; }


        [HintDisplayName("商品状态", "如果停用，开单时无法选该商品")]
        public bool? Status { get; set; }


        [HintDisplayName("品牌", "品牌")]
        public int? BrandId { get; set; } = 0;
        public string BrandName { get; set; }
        public SelectList Brands { get; set; }


        [HintDisplayName("是否显示零库存", "是否显示零库存")]
        public bool? ShowZeroStack { get; set; }

        #endregion 
    }



    public class StockCategoryTree
    {
        public bool Visible { get; set; }
        public StockCategoryModel StockCategory { get; set; }
        public int MaxItems { get; set; } = 0;
        public List<StockCategoryTree> Children { get; set; }
    }

    public partial class StockCategoryModel : BaseEntityModel
    {

        public StockCategoryModel()
        {
            Products = new List<ProductModel>();
        }
        public bool Selected { get; set; }

        [HintDisplayName("分类名称", "分类名称")]
        public string Name { get; set; }

        [HintDisplayName("状态", "状态")]
        public int Status { get; set; } = 0;

        [HintDisplayName("排序", "排序")]
        public int OrderNo { get; set; } = 0;

        [HintDisplayName("品牌", "品牌")]
        public int BrandId { get; set; } = 0;
        [HintDisplayName("品牌", "品牌")]
        public string BrandName { get; set; }

        public List<ProductModel> Products { get; set; }
    }



    public class StockCategoryGroupModel
    {
        /// <summary>
        /// 类别
        /// </summary>
        public int CategoryId { get; set; } = 0;
        /// <summary>
        /// 类别
        /// </summary>
        public string CategoryName { get; set; }

        public List<StockProductModel> Products { get; set; } = new List<StockProductModel>();
    }


    public class StockProductModel
    {

        /// <summary>
        /// 类别
        /// </summary>
        public int CategoryId { get; set; } = 0;

        /// <summary>
        /// 类别
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// 商品
        /// </summary>
        public int ProductId { get; set; } = 0;

        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { get; set; }


        /// <summary>
        /// 单位换算
        /// </summary>
        public string UnitConversion { get; set; }

        /// <summary>
        /// 库存数量
        /// </summary>
        public int? StockQty { get; set; } = 0;


        /// <summary>
        /// 成本金额
        /// </summary>
        public decimal? CostAmount { get; set; } = 0;


    }



    public partial class StockChangeSummaryListModel : BaseModel
    {
        public StockChangeSummaryListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<StockChangeSummary>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<StockChangeSummary> Items { get; set; }

        #region  用于条件检索

        [HintDisplayName("商品类别", "商品类别")]
        public int CategoryId { get; set; } = 0;
        public string CategoryName { get; set; }
        public SelectList Categories { get; set; }


        [HintDisplayName("仓库", "仓库")]
        public int? WareHouseId { get; set; } = 0;
        public string WareHouseName { get; set; }
        public SelectList WareHouses { get; set; }


        [HintDisplayName("商品", "商品")]
        public int ProductId { get; set; } = 0;
        public string ProductName { get; set; }


        [HintDisplayName("品牌", "品牌")]
        public int? BrandId { get; set; } = 0;
        public string BrandName { get; set; }
        public SelectList Brands { get; set; }


        [HintDisplayName("单价显示方式", "单价显示方式")]
        public int? PriceType { get; set; } = 0;


        [HintDisplayName("按单位", "按单位")]
        public int? UnitType { get; set; } = 0;


        [HintDisplayName("开始日期", "开始日期")]
        [UIHint("DateTimeNullable")]
        public DateTime? StartTime { get; set; }

        [HintDisplayName("截止日期", "截止日期")]
        [UIHint("DateTimeNullable")]
        public DateTime? EndTime { get; set; }

        #endregion 


    }


    public partial class StockChangeSummaryOrderListModel : BaseModel
    {
        public StockChangeSummaryOrderListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<StockChangeSummaryOrder>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<StockChangeSummaryOrder> Items { get; set; }

        #region  用于条件检索


        [HintDisplayName("仓库", "仓库")]
        public int? WareHouseId { get; set; } = 0;
        public string WareHouseName { get; set; }
        public SelectList WareHouses { get; set; }


        [HintDisplayName("商品", "商品")]
        public int ProductId { get; set; } = 0;
        public string ProductName { get; set; }


        [HintDisplayName("单据号", "单据号")]
        public string BillCode { get; set; }


        [HintDisplayName("单据类型", "单据类型")]
        public int? BillType { get; set; } = 0;
        public string BillTypeName { get; set; }
        public SelectList BillTypes { get; set; }


        [HintDisplayName("商品类别", "商品类别")]
        public int? CategoryId { get; set; } = 0;
        public string CategoryName { get; set; }
        public SelectList Categories { get; set; }


        [HintDisplayName("开始日期", "开始日期")]
        [UIHint("DateTime")]
        public DateTime StartTime { get; set; }

        [HintDisplayName("截止日期", "截止日期")]
        [UIHint("DateTime")]
        public DateTime EndTime { get; set; }


        [HintDisplayName("排除跨月", "排除跨月")]
        public bool CrossMonth { get; set; }
        #endregion 
    }



    public partial class InventoryReportListModel : BaseModel
    {
        public InventoryReportListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<InventoryReportList>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<InventoryReportList> Items { get; set; }

        #region  用于条件检索

        [HintDisplayName("商品", "商品")]
        public int? ProductId { get; set; } = 0;
        public string ProductName { get; set; }

        [HintDisplayName("客户", "客户")]
        public int? TerminalId { get; set; } = 0;
        public string TerminalName { get; set; }

        [HintDisplayName("业务员", "业务员")]
        public int? BusinessUserId { get; set; } = 0;
        public string BusinessUserName { get; set; }
        public SelectList BusinessUsers { get; set; }


        [HintDisplayName("客户渠道", "客户渠道")]
        public int? ChannelId { get; set; } = 0;
        public string ChannelName { get; set; }
        public SelectList Channels { get; set; }


        [HintDisplayName("客户等级", "客户等级")]
        public int? RankId { get; set; } = 0;
        public string RankName { get; set; }
        public SelectList Ranks { get; set; }

        [HintDisplayName("客户片区", "客户片区")]
        public int? DistrictId { get; set; } = 0;
        public string DistrictName { get; set; }
        public SelectList Districts { get; set; }


        [HintDisplayName("开始日期", "开始日期")]
        [UIHint("DateTime")]
        public DateTime StartTime { get; set; }

        [HintDisplayName("截止日期", "截止日期")]
        [UIHint("DateTime")] public DateTime EndTime { get; set; }

        #endregion 
    }

    //=======




    /// <summary>
    /// 调拨明细页面模型
    /// </summary>
    public partial class AllocationDetailsListModel : BaseModel
    {
        public AllocationDetailsListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<AllocationDetailsList>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<AllocationDetailsList> Items { get; set; }

        #region  用于条件检索

        [HintDisplayName("出货仓库", "出货仓库")]
        public int? ShipmentWareHouseId { get; set; } = 0;
        public string ShipmentWareHouseName { get; set; }

        [HintDisplayName("入货仓库", "入货仓库")]
        public int? IncomeWareHouseId { get; set; } = 0;
        public string IncomeWareHouseName { get; set; }
        public SelectList WareHouses { get; set; }

        [HintDisplayName("商品", "商品")]
        public int? ProductId { get; set; } = 0;
        public string ProductName { get; set; }

        [HintDisplayName("商品类别", "商品类别")]
        public int? CategoryId { get; set; } = 0;
        public string CategoryName { get; set; }
        public SelectList Categories { get; set; }

        [HintDisplayName("状态", "状态")]
        public int? StatusId { get; set; } = 0;
        public string StatusName { get; set; }
        public SelectList Status { get; set; }


        [HintDisplayName("单据号", "单据号")]
        public string BillNumber { get; set; }

        [HintDisplayName("开始日期", "开始日期")]
        [UIHint("DateTime")]
        public DateTime StartTime { get; set; }

        [HintDisplayName("截止日期", "截止日期")]
        [UIHint("DateTime")]
        public DateTime EndTime { get; set; }

        #endregion 
    }



    #region 库存滞销报表
    /// <summary>
    /// 库存滞销报表
    /// </summary>
    public class StockUnsalableListModel : BaseModel
    {
        public StockUnsalableListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<StockUnsalable>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<StockUnsalable> Items { get; set; }

        #region  用于条件检索

        [HintDisplayName("商品", "商品")]
        public int? ProductId { get; set; } = 0;
        public string ProductName { get; set; }

        [HintDisplayName("仓库", "仓库")]
        public int? WareHouseId { get; set; } = 0;
        public SelectList WareHouses { get; set; }

        [HintDisplayName("品牌", "品牌")]
        public int? BrandId { get; set; } = 0;
        public string BrandName { get; set; }
        public SelectList Brands { get; set; }

        [HintDisplayName("商品类别", "商品类别")]
        public int? CategoryId { get; set; } = 0;
        public string CategoryName { get; set; }
        public SelectList Categories { get; set; }

        [HintDisplayName("净销售量小于", "净销售量小于")]
        public int? LessNetSaleQuantity { get; set; } = 0;

        [DisplayName("开始时间")]
        [UIHint("DateTime")]
        public DateTime StartTime { get; set; }

        [DisplayName("结束时间")]
        [UIHint("DateTime")]
        public DateTime EndTime { get; set; }

        #endregion

        #region 小计、合计

        /// <summary>
        /// 销售金额（总）
        /// </summary>
        public decimal? TotalSumSaleAmount { get; set; } = 0;

        /// <summary>
        /// 退货金额（总）
        /// </summary>
        public decimal? TotalSumReturnAmount { get; set; } = 0;

        /// <summary>
        /// 销售净额（总）
        /// </summary>
        public decimal? TotalSumNetAmount { get; set; } = 0;

        #endregion

    }
    #endregion

    #region 库存预警表
    /// <summary>
    /// 库存预警表
    /// </summary>
    public class EarlyWarningListModel : BaseModel
    {
        public EarlyWarningListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<EarlyWarning>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<EarlyWarning> Items { get; set; }

        #region  用于条件检索

        [HintDisplayName("仓库", "仓库")]
        public int? WareHouseId { get; set; } = 0;
        public string WareHouseName { get; set; }
        public SelectList WareHouses { get; set; }

        [HintDisplayName("商品类别", "商品类别")]
        public int? CategoryId { get; set; } = 0;
        public string CategoryName { get; set; }
        public SelectList Categories { get; set; }

        [HintDisplayName("品牌", "品牌")]
        public int? BrandId { get; set; } = 0;
        public string BrandName { get; set; }
        public SelectList Brands { get; set; }

        //单位
        [HintDisplayName("单位类型", "单位类型")]
        public int? UnitShowTypeId { get; set; } = 0;
        public string UnitShowTypeName { get; set; }
        public SelectList UnitShowTypes { get; set; }

        #endregion

        #region 小计、合计

        #endregion

    }
    #endregion


    #region 临期预警表
    /// <summary>
    /// 临期预警表
    /// </summary>
    public class ExpirationWarningListModel : BaseModel
    {
        public ExpirationWarningListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<ExpirationWarning>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<ExpirationWarning> Items { get; set; }

        #region  用于条件检索

        [HintDisplayName("仓库", "仓库")]
        public int? WareHouseId { get; set; } = 0;
        public string WareHouseName { get; set; }
        public SelectList WareHouses { get; set; }

        [HintDisplayName("商品类别", "商品类别")]
        public int? CategoryId { get; set; } = 0;
        public string CategoryName { get; set; }
        public SelectList Categories { get; set; }

        [HintDisplayName("商品", "商品")]
        public int ProductId { get; set; } = 0;
        public string ProductName { get; set; }

        #endregion


    }
    #endregion


}