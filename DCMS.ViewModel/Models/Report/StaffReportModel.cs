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

    #region 业务员业绩
    /// <summary>
    /// 业务员业绩
    /// </summary>
    public class StaffReportBusinessUserAchievementListModel : BaseModel
    {
        public StaffReportBusinessUserAchievementListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<StaffReportBusinessUserAchievement>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<StaffReportBusinessUserAchievement> Items { get; set; }

        #region  用于条件检索

        [HintDisplayName("商品类别", "商品类别")]
        public int? CategoryId { get; set; } = 0;
        public string CategoryName { get; set; }
        public SelectList Categories { get; set; }

        [HintDisplayName("仓库", "仓库")]
        public int? WareHouseId { get; set; } = 0;
        public SelectList WareHouses { get; set; }

        [HintDisplayName("客户Id", "客户Id")]
        public int? TerminalId { get; set; } = 0;
        [HintDisplayName("客户", "客户")]
        public string TerminalName { get; set; }

        [DisplayName("统计前")]
        public int? TopNumber { get; set; } = 0;

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

        /// <summary>
        /// 图表数据
        /// </summary>
        public string Charts { get; set; }

    }


    /// <summary>
    /// 拜访汇总
    /// </summary>
    public class VisitSummeryListModel : BaseModel
    {
        public VisitSummeryListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<VisitSummeryQuery>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<VisitSummeryQuery> Items { get; set; }

        #region  用于条件检索

        [HintDisplayName("客户Id", "客户Id")]
        public int? TerminalId { get; set; } = 0;
        [HintDisplayName("客户", "客户")]
        public string TerminalName { get; set; }


        [DisplayName("开始时间")]
        [UIHint("DateTime")]
        public DateTime StartTime { get; set; }

        [DisplayName("结束时间")]
        [UIHint("DateTime")]
        public DateTime EndTime { get; set; }

        #endregion


        /// <summary>
        /// 图表数据
        /// </summary>
        public string Charts { get; set; }

    }

    #endregion


    /// <summary>
    /// 员工提成汇总列表
    /// </summary>
    public class StaffReportPercentageSummaryListModel : BaseModel
    {
        public StaffReportPercentageSummaryListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<StaffReportPercentageSummary>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<StaffReportPercentageSummary> Items { get; set; }

        #region  用于条件检索

        [DisplayName("开始时间")]
        [UIHint("DateTime")]
        public DateTime StartTime { get; set; }

        [DisplayName("结束时间")]
        [UIHint("DateTime")]
        public DateTime EndTime { get; set; }

        [HintDisplayName("员工", "员工")]
        public int? StaffUserId { get; set; } = 0;
        public SelectList StaffUsers { get; set; }

        [HintDisplayName("商品类别", "商品类别")]
        public int? CategoryId { get; set; } = 0;
        public string CategoryName { get; set; }
        public SelectList Categories { get; set; }

        [HintDisplayName("商品", "商品")]
        public int? ProductId { get; set; } = 0;
        public string ProductName { get; set; }

        #endregion

        #region 小计、合计

        /// <summary>
        /// 业务提成（总）
        /// </summary>
        public decimal? TotalSumBusinessPercentage { get; set; } = 0;

        /// <summary>
        /// 送货提成（总）
        /// </summary>
        public decimal? TotalSumDeliveryPercentage { get; set; } = 0;

        /// <summary>
        /// 提成合计（总）
        /// </summary>
        public decimal? TotalSumPercentageTotal { get; set; } = 0;

        #endregion

    }

    /// <summary>
    /// 员工提成明细列表
    /// </summary>
    public class StaffReportPercentageItemListModel : BaseModel
    {
        public StaffReportPercentageItemListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<StaffReportPercentageItem>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<StaffReportPercentageItem> Items { get; set; }

        [UIHint("DateTime")] public DateTime StartTime { get; set; }
        [UIHint("DateTime")] public DateTime EndTime { get; set; }
        public int? StaffUserId { get; set; } = 0;
        public string StaffUserName { get; set; }
        public string CategoryName { get; set; }
        public string ProductName { get; set; }




    }


    /// <summary>
    /// 拜访汇总
    /// </summary>
    public class BusinessUserVisitOfYearListModel : BaseModel
    {
        public BusinessUserVisitOfYearListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<BusinessUserVisitOfYear>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<BusinessUserVisitOfYear> Items { get; set; }

        #region 用于查询检索
        public int Year { get; set; }
        public int Month { get; set; }
        #endregion
    }
}