using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System;


namespace DCMS.Core.Domain.Report
{

    //员工报表

    #region 业务员业绩
    /// <summary>
    /// 业务员业绩
    /// </summary>
    public class StaffReportBusinessUserAchievement
    {

        /// <summary>
        /// 业务员Id
        /// </summary>
        public int? BusinessUserId { get; set; }

        /// <summary>
        /// 业务员名称
        /// </summary>
        public string BusinessUserName { get; set; }

        /// <summary>
        /// 销售金额
        /// </summary>
        public decimal? SaleAmount { get; set; } = 0;
        public decimal? AdSaleAmount { get; set; } = 0;

        /// <summary>
        /// 退货金额
        /// </summary>
        public decimal? ReturnAmount { get; set; } = 0;
        public decimal? AdReturnAmount { get; set; } = 0;

        /// <summary>
        /// 销售净额
        /// </summary>
        public decimal? NetAmount { get; set; } = 0;
        /// <summary>
        /// 收款额
        /// </summary>
        [NotMapped]
        public decimal? ReceiptAmount { get; set; } = 0;
        /// <summary>
        /// 欠款额
        /// </summary>
        [NotMapped]
        public decimal? OweAmount { get; set; } = 0;
    }
    #endregion



    #region 员工提成汇总表

    /// <summary>
    /// 员工提成汇总表
    /// </summary>
    public class StaffReportPercentageSummary
    {
        /// <summary>
        /// 员工Id
        /// </summary>
        public int StaffUserId { get; set; }

        /// <summary>
        /// 员工名称
        /// </summary>
        public string StaffUserName { get; set; }

        /// <summary>
        /// 业务提成
        /// </summary>
        public decimal? BusinessPercentage { get; set; }

        /// <summary>
        /// 送货提成
        /// </summary>
        public decimal? DeliveryPercentage { get; set; }

        /// <summary>
        /// 提成合计
        /// </summary>
        public decimal? PercentageTotal { get; set; }

        /// <summary>
        /// 业务提成明细
        /// </summary>
        [NotMapped]
        public IList<StaffReportPercentageItem> SItems { get; set; } = new List<StaffReportPercentageItem>();
        /// <summary>
        /// 送货提成明细
        /// </summary>
        [NotMapped]
        public IList<StaffReportPercentageItem> DItems { get; set; } = new List<StaffReportPercentageItem>();

    }


    /// <summary>
    /// 提成明细表
    /// </summary>
    public class StaffReportPercentageItem
    {
        /// <summary>
        /// 员工Id
        /// </summary>
        public int StaffUserId { get; set; }

        /// <summary>
        /// 员工名称
        /// </summary>
        public string StaffUserName { get; set; }

        /// <summary>
        /// 商品Id
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 销售分段
        /// </summary>
        public string SaleFragment { get; set; } = "";

        /// <summary>
        /// 提成方式Id
        /// </summary>
        public int CalCulateMethodId { get; set; } = 0;

        /// <summary>
        /// 提成方式名称
        /// </summary>
        public string CalCulateMethodName { get; set; } = "";

        #region 提成
        /// <summary>
        /// （提成）合计
        /// </summary>
        public decimal? PercentageTotal { get; set; } = 0;

        /// <summary>
        /// （提成）销售
        /// </summary>
        public decimal? PercentageSale { get; set; } = 0;

        /// <summary>
        /// （提成）退货
        /// </summary>
        public decimal? PercentageReturn { get; set; } = 0;
        #endregion

        #region 数量
        /// <summary>
        /// （数量）净销售量
        /// </summary>
        public decimal? QuantityNetSaleQuantity { get; set; } = 0;

        /// <summary>
        /// （数量）销售
        /// </summary>
        public decimal? QuantitySale { get; set; } = 0;

        /// <summary>
        /// （数量）退货
        /// </summary>
        public decimal? QuantityReturn { get; set; } = 0;
        #endregion

        #region 金额
        /// <summary>
        /// （金额）销售净额
        /// </summary>
        public decimal? AmountSaleNet { get; set; } = 0;

        /// <summary>
        /// （金额）销售
        /// </summary>
        public decimal? AmountSale { get; set; } = 0;

        /// <summary>
        /// （金额）退货
        /// </summary>
        public decimal? AmountReturn { get; set; } = 0;
        #endregion

        #region 利润
        /// <summary>
        /// （利润）净利润
        /// </summary>
        public decimal? ProfitNet { get; set; } = 0;

        /// <summary>
        /// （利润）销售
        /// </summary>
        public decimal? ProfitSale { get; set; } = 0;

        /// <summary>
        /// （利润）退货
        /// </summary>
        public decimal? ProfitReturn { get; set; } = 0;
        #endregion


    }


    /// <summary>
    /// 员工销量提成导出
    /// </summary>
    public class StaffSaleQuery
    {
        public int BusinessUserId { get; set; }
        public string Name { get; set; }
        public string BigQuantity { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }

    public class VisitSummeryQuery
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int VistCount { get; set; }
        public int NewAddCount { get; set; }
        public int DoorheadPhotoCount { get; set; }
        public int DisplayPhotoCount { get; set; }
        [NotMapped]
        public string TotalDuration { get; set; }
    }

    #endregion

    #region 业务员拜访记录
    /// <summary>
    /// 业务员拜访记录
    /// </summary>
    public class StaffReportBusinessUserVisitRecord
    {

        /// <summary>
        /// 业务员Id
        /// </summary>
        public int? BusinessUserId { get; set; }

        /// <summary>
        /// 业务员名称
        /// </summary>
        public string BusinessUserName { get; set; }

        /// <summary>
        /// 客户Id
        /// </summary>
        public int? TerminalId { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        public string TerminalName { get; set; }

        /// <summary>
        /// 客户编码
        /// </summary>
        public string TerminalCode { get; set; }
    }
    #endregion

    #region 拜访达成表
    /// <summary>
    /// 拜访达成表
    /// </summary>
    public class StaffReportBusinessUserVisitSuccess
    {
        /// <summary>
        /// 业务员Id
        /// </summary>
        public int? BusinessUserId { get; set; }

        /// <summary>
        /// 业务员名称
        /// </summary>
        public string BusinessUserName { get; set; }
    }
    #endregion


    #region 业务员外勤轨迹
    /// <summary>
    /// 业务员外勤轨迹
    /// </summary>
    public class StaffReportBusinessUserLocus
    {


    }
    #endregion


    public class BusinessUserVisitOfYear 
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int Days1 { get; set; }
        public int Days2 { get; set; }
        public int Days3 { get; set; }
        public int Days4 { get; set; }
        public int Days5 { get; set; }
        public int Days6 { get; set; }
        public int Days7 { get; set; }
        public int Days8 { get; set; }
        public int Days9 { get; set; }
        public int Days10 { get; set; }
        public int Days11{ get; set; }
        public int Days12{ get; set; }
        public int Days13{ get; set; }
        public int Days14{ get; set; }
        public int Days15{ get; set; }
        public int Days16{ get; set; }
        public int Days17{ get; set; }
        public int Days18{ get; set; }
        public int Days19{ get; set; }
        public int Days20{ get; set; }
        public int Days21{ get; set; }
        public int Days22{ get; set; }
        public int Days23{ get; set; }
        public int Days24{ get; set; }
        public int Days25{ get; set; }
        public int Days26{ get; set; }
        public int Days27{ get; set; }
        public int Days28{ get; set; }
        public int Days29 { get; set; } = 0;
        public int Days30 { get; set; } = 0;
        public int Days31 { get; set; } = 0;
        public int Total { get; set; } = 0;
    }

}
