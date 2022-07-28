using DCMS.Core.Domain.Report;
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace DCMS.ViewModel.Models.Report
{

    #region 客户活跃度
    /// <summary>
    /// 客户活跃度
    /// </summary>
    public class MarketReportTerminalActiveListModel : BaseModel
    {
        public MarketReportTerminalActiveListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<MarketReportTerminalActive>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<MarketReportTerminalActive> Items { get; set; }

        #region  用于条件检索

        [HintDisplayName("无拜访天数大于", "无拜访天数大于")]
        public int? NoVisitDayMore { get; set; } = 0;

        [HintDisplayName("客户Id", "客户Id")]
        public int? TerminalId { get; set; } = 0;
        [HintDisplayName("客户", "客户")]
        public string TerminalName { get; set; }

        [HintDisplayName("无销售天数大于", "无销售天数大于")]
        public int? NoSaleDayMore { get; set; } = 0;

        [HintDisplayName("客户片区Id", "客户片区Id")]
        public int? DistrictId { get; set; } = 0;
        public SelectList Districts { get; set; }

        [HintDisplayName("员工Id", "员工Id")]
        public int? StaffUserId { get; set; } = 0;
        public SelectList StaffUsers { get; set; }

        #endregion

        #region 小计、合计

        #endregion

    }
    #endregion

    #region 客户价值分析
    /// <summary>
    /// 客户价值分析
    /// </summary>
    public class MarketReportTerminalValueAnalysisListModel : BaseModel
    {
        public MarketReportTerminalValueAnalysisListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<MarketReportTerminalValueAnalysis>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<MarketReportTerminalValueAnalysis> Items { get; set; }

        public IList<ValueAnalysisGroupModel> Groups { get; set; } = new List<ValueAnalysisGroupModel>();

        #region  用于条件检索

        [HintDisplayName("客户Id", "客户Id")]
        public int? TerminalId { get; set; } = 0;
        [HintDisplayName("客户", "客户")]
        public string TerminalName { get; set; }

        [HintDisplayName("客户片区Id", "客户片区Id")]
        public int? DistrictId { get; set; } = 0;
        public SelectList Districts { get; set; }

        [HintDisplayName("客户价值", "客户价值")]
        public int? TerminalValueId { get; set; } = 0;
        public string TerminalValueName { get; set; }


        #endregion
    }


    public class ValueAnalysisGroupModel
    {
        public int? TerminalValueId { get; set; } = 0;
        public string TerminalValueName { get; set; }
    }
    #endregion


    #region 铺市率报表
    /// <summary>
    /// 铺市率报表
    /// </summary>
    public class MarketReportShopRateListModel : BaseModel
    {
        public MarketReportShopRateListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<MarketReportShopRate>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<MarketReportShopRate> Items { get; set; }

        #region  用于条件检索

        [HintDisplayName("商品", "商品")]
        public int? ProductId { get; set; } = 0;
        public string ProductName { get; set; }

        [HintDisplayName("商品类别", "商品类别")]
        public int? CategoryId { get; set; } = 0;
        public string CategoryName { get; set; }
        public SelectList Categories { get; set; }

        [HintDisplayName("品牌", "品牌")]
        public int? BrandId { get; set; } = 0;
        public string BrandName { get; set; }
        public SelectList Brands { get; set; }

        [HintDisplayName("客户片区", "客户片区")]
        public int? DistrictId { get; set; } = 0;
        public SelectList Districts { get; set; }

        [DisplayName("开始时间")]

        [UIHint("DateTime")] public DateTime StartTime { get; set; }

        [DisplayName("结束时间")]

        [UIHint("DateTime")] public DateTime EndTime { get; set; }

        [HintDisplayName("业务员", "业务员")]
        public int? BusinessUserId { get; set; } = 0;
        public SelectList BusinessUsers { get; set; }

        #endregion

        #region 小计、合计

        /// <summary>
        /// 销售金额（总）
        /// </summary>
        public decimal? TotalSumSaleAmount { get; set; } = 0;

        /// <summary>
        /// 退货数（总）
        /// </summary>
        public decimal? TotalSumReturnAmount { get; set; } = 0;

        /// <summary>
        /// 门店数（总）
        /// </summary>
        public int TotalSumDoorQuantity { get; set; } = 0;

        /// <summary>
        /// 期内（总）
        /// </summary>
        public int TotalSumInsideQuantity { get; set; } = 0;

        /// <summary>
        /// 减少（总）
        /// </summary>
        public int TotalSumDecreaseQuantity { get; set; } = 0;

        /// <summary>
        /// 期初（总）
        /// </summary>
        public int TotalSumBeginQuantity { get; set; } = 0;

        /// <summary>
        /// 增加（总）
        /// </summary>
        public int TotalSumAddQuantity { get; set; } = 0;

        /// <summary>
        /// 期末（总）
        /// </summary>
        public int TotalSumEndQuantity { get; set; } = 0;

        #endregion

    }
    #endregion





}