namespace DCMS.Core.Domain.Report
{

    //市场报表

    #region 客户活跃度
    /// <summary>
    /// 客户活跃度
    /// </summary>
    public class MarketReportTerminalActive
    {

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

        /// <summary>
        /// 无拜访天数
        /// </summary>
        public int NoVisitDays { get; set; }

        /// <summary>
        /// 无销售天数
        /// </summary>
        public int NoSaleDays { get; set; }

        /// <summary>
        /// 片区Id
        /// </summary>
        public int DistrictId { get; set; }

        /// <summary>
        /// 片区名称
        /// </summary>
        public string DistrictName { get; set; }


    }
    #endregion

    #region 客户价值分析
    /// <summary>
    /// 客户价值分析
    /// </summary>
    public class MarketReportTerminalValueAnalysis
    {
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

        /// <summary>
        /// 类型Id
        /// </summary>
        public int TerminalTypeId { get; set; }

        /// <summary>
        /// 类型名称
        /// </summary>
        public string TerminalTypeName { get; set; }

        /// <summary>
        /// 未采购天数（R）
        /// </summary>
        public int NoPurchaseDays { get; set; }

        /// <summary>
        /// 未采次数（F）
        /// </summary>
        public int PurchaseNumber { get; set; }

        /// <summary>
        /// 采购额度（M）
        /// </summary>
        public decimal PurchaseAmount { get; set; }


        /// <summary>
        /// RFM得分
        /// </summary>
        public double R_S { get; set; }
        public double F_S { get; set; }
        public double M_S { get; set; }

        /// <summary>
        /// 客户价值排名
        /// </summary>
        public int RFMScore { get; set; }

        /// <summary>
        /// 片区Id
        /// </summary>
        public int DistrictId { get; set; }

        /// <summary>
        /// 片区名称
        /// </summary>
        public string DistrictName { get; set; }

        /// <summary>
        /// 未拜访天数
        /// </summary>
        public int NoVisitDays { get; set; }

    }
    #endregion


    #region 客户流失预警
    /// <summary>
    /// 客户流失预警
    /// </summary>
    public class MarketReportTerminalLossWarning
    {
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

        /// <summary>
        /// 类型Id
        /// </summary>
        public int TerminalTypeId { get; set; }

        /// <summary>
        /// 类型名称
        /// </summary>
        public string TerminalTypeName { get; set; }

        /// <summary>
        /// 未采购天数（R）
        /// </summary>
        public int NoPurchaseDays { get; set; }

        /// <summary>
        /// 未采次数（F）
        /// </summary>
        public int PurchaseNumber { get; set; }

        /// <summary>
        /// 采购额度（M）
        /// </summary>
        public decimal PurchaseAmount { get; set; }


        /// <summary>
        /// RFM得分
        /// </summary>
        public double R_S { get; set; }
        public double F_S { get; set; }
        public double M_S { get; set; }

        /// <summary>
        /// 客户价值排名
        /// </summary>
        public int RFMScore { get; set; }

        /// <summary>
        /// 片区Id
        /// </summary>
        public int DistrictId { get; set; }

        /// <summary>
        /// 片区名称
        /// </summary>
        public string DistrictName { get; set; }

        /// <summary>
        /// 未拜访天数
        /// </summary>
        public int NoVisitDays { get; set; }


        /// <summary>
        /// 拜访备注
        /// </summary>
        public string VisitRemark { get; set; }

        /// <summary>
        /// 其他备注
        /// </summary>
        public string OtherRemark { get; set; }
    }

    #endregion

    #region 铺市率报表
    /// <summary>
    /// 铺市率报表
    /// </summary>
    public class MarketReportShopRate
    {
        /// <summary>
        /// 商品Id
        /// </summary>
        public int? ProductId { get; set; }

        /// <summary>
        /// 商品编号
        /// </summary>
        public string ProductCode { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 条形码（小）
        /// </summary>
        public string SmallBarCode { get; set; }

        /// <summary>
        /// 条形码（中）
        /// </summary>
        public string StrokeBarCode { get; set; }

        /// <summary>
        /// 条形码（大）
        /// </summary>
        public string BigBarCode { get; set; }

        /// <summary>
        /// 单位换算
        /// </summary>
        public string UnitConversion { get; set; }

        /// <summary>
        /// 销售金额
        /// </summary>
        public decimal? SaleAmount { get; set; }

        /// <summary>
        /// 退货数
        /// </summary>
        public decimal? ReturnAmount { get; set; }

        /// <summary>
        /// 门店数
        /// </summary>
        public int? DoorQuantity { get; set; }

        /// <summary>
        /// 期内
        /// </summary>
        public int? InsideQuantity { get; set; }

        /// <summary>
        /// 减少（期末-期内）
        /// </summary>
        public int? DecreaseQuantity { get; set; }

        /// <summary>
        /// 期初
        /// </summary>
        public int? BeginQuantity { get; set; }

        /// <summary>
        /// 增加（期末-期初）
        /// </summary>
        public int? AddQuantity { get; set; }

        /// <summary>
        /// 期末
        /// </summary>
        public int? EndQuantity { get; set; }

        /// <summary>
        /// 百分比
        /// </summary>
        public double Percent { get; set; }


    }
    #endregion

}
