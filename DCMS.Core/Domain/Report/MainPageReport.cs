using DCMS.Core.Domain.Sales;
using System.Collections.Generic;

namespace DCMS.Core.Domain.Report
{


    #region 仪表盘

    /// <summary>
    /// 仪表盘
    /// </summary>
    public class DashboardReport
    {
        /// <summary>
        /// 今日销售额
        /// </summary>
        public decimal TodaySaleAmount { get; set; }

        /// <summary>
        /// 昨日销售额
        /// </summary>
        public decimal YesterdaySaleAmount { get; set; }

        /// <summary>
        /// 今日订单数
        /// </summary>
        public int TodayOrderQuantity { get; set; }

        /// <summary>
        /// 昨日订单数
        /// </summary>
        public int YesterdayOrderQuantity { get; set; }

        /// <summary>
        /// 今日新增客户
        /// </summary>
        public int TodayAddTerminalQuantity { get; set; }

        /// <summary>
        /// 昨日新增客户
        /// </summary>
        public int YesterdayAddTerminalQuantity { get; set; }

        /// <summary>
        /// 今日拜访客户
        /// </summary>
        public int TodayVisitQuantity { get; set; }

        /// <summary>
        /// 昨日拜访客户
        /// </summary>
        public int YesterdayVisitQuantity { get; set; }
    }

    #endregion

    #region 当月销量
    /// <summary>
    /// 当月销量
    /// </summary>
    public class MonthSaleReport
    {
        /// <summary>
        /// 品牌Id
        /// </summary>
        public int BrandId { get; set; }

        /// <summary>
        /// 品牌名称
        /// </summary>
        public string BrandName { get; set; }

        /// <summary>
        /// 销量
        /// </summary>
        public int SaleQuantity { get; set; }

        /// <summary>
        /// 销售额
        /// </summary>
        public decimal SaleAmount { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public string SaleDate { get; set; }

    }
    #endregion

    #region 当月销量进度
    /// <summary>
    /// 当月销量进度
    /// </summary>
    public class SalePercentReport
    {
        /// <summary>
        /// 业务员Id
        /// </summary>
        public int BusinessUserId { get; set; }
        public string BussinessUserName { get; set; }
        public double SaleAmount { get; set; }
        public double SaleQuantity { get; set; }
        /// <summary>
        /// 销量百分比
        /// </summary>
        public int SalePercent { get; set; }
    }
    #endregion

    #region 当日销量
    /// <summary>
    /// 当日销量
    /// </summary>
    public class BussinessVisitStoreReport
    {
        /// <summary>
        /// 拜访量
        /// </summary>
        public int VisitStoreAmount { get; set; }

        /// <summary>
        /// 业务员Id
        /// </summary>
        public int BusinessUserId { get; set; }
        public string BussinessUserName { get; set; }

    }
    #endregion

    #region 所有经销商统计（用于Manage站点）

    /// <summary>
    /// 经销商信息统计（manage主页上面的4个）
    /// </summary>
    public class AllStoreDashboard
    {
        /// <summary>
        /// 订单总数
        /// </summary>
        public int TotalSumOrderQuantity { get; set; }

        /// <summary>
        /// 经销商总数
        /// </summary>
        public int TotalSumStoreQuantity { get; set; }

        /// <summary>
        /// 经销商商品总数
        /// </summary>
        public int TotalSumProductQuantity { get; set; }

        /// <summary>
        /// 商品销量总数
        /// </summary>
        public int TotalSumSaleQuantity { get; set; }

    }

    /// <summary>
    /// 所有经销商销售信息
    /// </summary>
    public class AllStoreSaleInformation
    {
        public AllStoreDashboard AllStoreDashboard { get; set; } = new AllStoreDashboard();

        /// <summary>
        /// 订单总计
        /// </summary>
        public List<AllStoreOrderTotal> AllStoreOrderTotals { get; set; } = new List<AllStoreOrderTotal>();

        /// <summary>
        /// 未完成订单
        /// </summary>
        public List<AllStoreUnfinishedOrder> AllStoreUnfinishedOrders { get; set; } = new List<AllStoreUnfinishedOrder>();

        /// <summary>
        /// 热销销量
        /// </summary>
        public List<HotSaleRanking> AllStoreHotSaleQuantityRankings { get; set; } = new List<HotSaleRanking>();
        /// <summary>
        /// 热销金额
        /// </summary>
        public List<HotSaleRanking> AllStoreHotSaleAmountRankings { get; set; } = new List<HotSaleRanking>();
        /// <summary>
        /// 热订销量
        /// </summary>
        public List<HotSaleRanking> AllStoreHotOrderQuantityRankings { get; set; } = new List<HotSaleRanking>();
        /// <summary>
        /// 热订金额
        /// </summary>
        public List<HotSaleRanking> AllStoreHotOrderAmountRankings { get; set; } = new List<HotSaleRanking>();

    }

    /// <summary>
    /// 订单总计
    /// </summary>
    public class AllStoreOrderTotal
    {
        /// <summary>
        /// 订单状态：1,未审核 2,已审核 3,已红冲 4,已收款 5,已付款
        /// </summary>
        public int OrderStatus { get; set; }

        /// <summary>
        /// 订单状态名称
        /// </summary>
        public string OrderStatusName { get; set; }

        /// <summary>
        /// 今日
        /// </summary>
        public int Today { get; set; }

        /// <summary>
        /// 本周
        /// </summary>
        public int ThisWeek { get; set; }

        /// <summary>
        /// 本月
        /// </summary>
        public int ThisMonth { get; set; }

        /// <summary>
        /// 今年
        /// </summary>
        public int ThisYear { get; set; }

        /// <summary>
        /// 全部
        /// </summary>
        public int Total { get; set; }


    }

    /// <summary>
    /// 未完成订单
    /// </summary>
    public class AllStoreUnfinishedOrder
    {
        /// <summary>
        /// 事由 1,未审核 2,未转单 3,未调度 4,未收款 5,未付款
        /// </summary>
        public int ReasonStatus { get; set; }

        /// <summary>
        /// 事由名称
        /// </summary>
        public string ReasonStatusName { get; set; }

        /// <summary>
        /// 经销商Id
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// 经销商名称
        /// </summary>
        public string StoreName { get; set; }

        /// <summary>
        /// 金额小计
        /// </summary>
        public decimal TotalSumSub { get; set; }

        /// <summary>
        /// 单据数量
        /// </summary>
        public int TotalSumOrderQuantity { get; set; }


    }


    #endregion



    /// <summary>
    /// 待处理事项统计
    /// </summary>
    public class PendingCount
    {
        /// <summary>
        /// 待审核
        /// </summary>
        public int AuditCount { get; set; }
        /// <summary>
        /// 订货单
        /// </summary>
        public int OrderCount { get; set; }
        //销售订单	
        public int SaleOrderCount { get; set; }
        /// <summary>
        /// 退货订单
        /// </summary>
        public int ReturnOrderCount { get; set; }
        /// <summary>
        /// 销售单
        /// </summary>
        public int SaleCount { get; set; }
        /// <summary>
        /// 退货单
        /// </summary>
        public int ReturnCount { get; set; }
        /// <summary>
        /// 调拨单
        /// </summary>
        public int AllocationCount { get; set; }
        /// <summary>
        /// 收款单
        /// </summary>
        public int CashReceiptCount { get; set; }
        /// <summary>
        /// 待调度
        /// </summary>
        public int DispatchCount { get; set; }
        /// <summary>
        /// 待转单
        /// </summary>
        public int ChangeCount { get; set; }
    }
}
