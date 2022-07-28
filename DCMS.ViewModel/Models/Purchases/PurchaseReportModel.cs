using DCMS.Core.Domain.Purchases;
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DCMS.ViewModel.Models.Purchases
{

    #region 采购明细表
    /// <summary>
    /// 采购明细表
    /// </summary>
    public class PurchaseReportItemListModel : BaseModel
    {
        public PurchaseReportItemListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<PurchaseReportItem>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<PurchaseReportItem> Items { get; set; }

        #region  用于条件检索

        [HintDisplayName("商品", "商品")]
        public int? ProductId { get; set; } = 0;
        public string ProductName { get; set; }

        [HintDisplayName("商品类别", "商品类别")]
        public int? CategoryId { get; set; } = 0;
        public string CategoryName { get; set; }
        public SelectList Categories { get; set; }

        [HintDisplayName("供应商", "供应商")]
        public int? ManufacturerId { get; set; } = 0;
        public SelectList Manufacturers { get; set; }

        [HintDisplayName("仓库", "仓库")]
        public int? WareHouseId { get; set; } = 0;
        public SelectList WareHouses { get; set; }

        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }

        [HintDisplayName("单据类型", "单据类型")]
        public int? PurchaseTypeId { get; set; } = 0;
        public string PurchaseTypeName { get; set; }
        public SelectList PurchaseTypes { get; set; }

        [DisplayName("开始时间")]

        [UIHint("DateTime")] public DateTime StartTime { get; set; }

        [DisplayName("结束时间")]

        [UIHint("DateTime")] public DateTime EndTime { get; set; }

        [HintDisplayName("明细备注", "明细备注")]
        public string Remark { get; set; }

        #endregion

        #region 小计、合计

        /// <summary>
        /// 金额（当前页）
        /// </summary>
        public decimal? PageSumAmount { get; set; } = 0;
        /// <summary>
        /// 金额（总）
        /// </summary>
        public decimal? TotalSumAmount { get; set; } = 0;

        #endregion

    }
    #endregion

    #region 采购汇总（按商品）

    /// <summary>
    /// 采购汇总（按商品）
    /// </summary>
    public class PurchaseReportSummaryProductListModel : BaseModel
    {
        public PurchaseReportSummaryProductListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<PurchaseReportSummaryProduct>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<PurchaseReportSummaryProduct> Items { get; set; }

        #region  用于条件检索

        [HintDisplayName("商品类别", "商品类别")]
        public int? CategoryId { get; set; } = 0;
        public string CategoryName { get; set; }
        public SelectList Categories { get; set; }

        [HintDisplayName("商品", "商品")]
        public int? ProductId { get; set; } = 0;
        public string ProductName { get; set; }

        [HintDisplayName("供应商", "供应商")]
        public int? ManufacturerId { get; set; } = 0;
        public SelectList Manufacturers { get; set; }

        [HintDisplayName("仓库", "仓库")]
        public int? WareHouseId { get; set; } = 0;
        public SelectList WareHouses { get; set; }

        [DisplayName("开始时间")]

        public DateTime? StartTime { get; set; }

        [DisplayName("结束时间")]

        public DateTime? EndTime { get; set; }

        #endregion

        #region 总计

        /// <summary>
        /// 采购数量（总）
        /// </summary>
        public string TotalSumPurchaseQuantityConversion { get; set; }

        /// <summary>
        /// 采购金额（总）
        /// </summary>
        public decimal? TotalSumPurchaseAmount { get; set; } = 0;

        /// <summary>
        /// 赠送数量（总）
        /// </summary>
        public string TotalSumGiftQuantityConversion { get; set; }

        /// <summary>
        /// 退购数量（总）
        /// </summary>
        public string TotalSumPurchaseReturnQuantityConversion { get; set; }

        /// <summary>
        /// 退购金额（总）
        /// </summary>
        public decimal? TotalSumPurchaseReturnAmount { get; set; } = 0;

        /// <summary>
        /// 数量小计（总）
        /// </summary>
        public string TotalSumQuantityConversion { get; set; }

        /// <summary>
        /// 金额小计（总）
        /// </summary>
        public decimal? TotalSumAmount { get; set; } = 0;

        #endregion

    }

    #endregion

    #region 采购汇总（按供应商）

    /// <summary>
    /// 采购汇总（按供应商）
    /// </summary>
    public class PurchaseReportSummaryManufacturerListModel : BaseModel
    {
        public PurchaseReportSummaryManufacturerListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<PurchaseReportSummaryManufacturer>();
            DynamicColumns = new List<string>();
            TotalDynamicDatas = new List<PurchaseReportSumStatisticalType>();

        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<PurchaseReportSummaryManufacturer> Items { get; set; }
        public List<string> DynamicColumns { get; set; }
        public IList<PurchaseReportSumStatisticalType> TotalDynamicDatas { get; set; }

        #region  用于条件检索

        [DisplayName("开始时间")]

        [UIHint("DateTime")] public DateTime StartTime { get; set; }

        [DisplayName("结束时间")]

        [UIHint("DateTime")] public DateTime EndTime { get; set; }

        [HintDisplayName("供应商", "供应商")]
        public int? ManufacturerId { get; set; } = 0;
        public SelectList Manufacturers { get; set; }

        #endregion


        #region 合计

        /// <summary>
        /// 采购数量
        /// </summary>
        public int? TotalSumPurchaseSmallQuantity { get; set; } = 0;

        /// <summary>
        /// 单据金额
        /// </summary>
        public decimal? TotalSumOrderAmount { get; set; } = 0;


        #endregion

    }

    #endregion


    /// <summary>
    /// 统计类别汇总
    /// </summary>
    public class PurchaseReportSumStatisticalType
    {
        /// <summary>
        /// 统计类别Id
        /// </summary>
        public int StatisticalTypeId { get; set; } = 0;

        /// <summary>
        /// 采购数量
        /// </summary>
        public int? PurchaseSmallQuantity { get; set; } = 0;

        /// <summary>
        /// 单据金额
        /// </summary>
        public decimal? OrderAmount { get; set; } = 0;

    }

}