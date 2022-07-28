using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


namespace DCMS.Core.Domain.Sales
{
    public class Count<T>
    {
        public T Value { get; set; }
    }

    #region 销售明细表

    /// <summary>
    /// 销售明细表
    /// </summary>
    public class SaleReportItem
    {
        /// <summary>
        /// 单据Id
        /// </summary>
        public int? BillId { get; set; }

        /// <summary>
        /// 单据编号
        /// </summary>
        public string BillNumber { get; set; }

        /// <summary>
        /// 订单Id
        /// </summary>
        public int? ReservationBillId { get; set; }

        /// <summary>
        /// 订单编号
        /// </summary>
        public string ReservationBillNumber { get; set; }

        /// <summary>
        /// 单据类型Id
        /// </summary>
        public int? BillTypeId { get; set; }

        /// <summary>
        /// 单价类型名称
        /// </summary>
        public string BillTypeName { get; set; }

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
        /// 业务员Id
        /// </summary>
        public int? BusinessUserId { get; set; }

        /// <summary>
        /// 业务员名称
        /// </summary>
        public string BusinessUserName { get; set; }

        /// <summary>
        /// 送货员Id
        /// </summary>
        public int? DeliveryUserId { get; set; }

        /// <summary>
        /// 送货员名称
        /// </summary>
        public string DeliveryUserName { get; set; }

        /// <summary>
        /// 交易时间
        /// </summary>
        public DateTime? TransactionDate { get; set; }

        /// <summary>
        /// 审核时间
        /// </summary>
        public DateTime? AuditedDate { get; set; }

        /// <summary>
        /// 仓库Id
        /// </summary>
        public int? WareHouseId { get; set; }

        /// <summary>
        /// 仓库名称
        /// </summary>
        public string WareHouseName { get; set; }

        /// <summary>
        /// 商品Id
        /// </summary>
        public int? ProductId { get; set; }

        /// <summary>
        /// 商品编码
        /// </summary>
        public string ProductSKU { get; set; }

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
        /// 条形码
        /// </summary>
        public string BarCode { get; set; }

        /// <summary>
        /// 单位Id（小）
        /// </summary>
        public int? SmallUnitId { get; set; }
        /// <summary>
        /// 单位名称（小）
        /// </summary>
        public string SmallUnitName { get; set; }
        /// <summary>
        /// 单位Id（中）
        /// </summary>
        public int? StrokeUnitId { get; set; }
        /// <summary>
        /// 单位名称（中）
        /// </summary>
        public string StrokeUnitName { get; set; }
        /// <summary>
        /// 单位Id（大）
        /// </summary>
        public int? BigUnitId { get; set; }
        /// <summary>
        /// 单位名称（大）
        /// </summary>
        public string BigUnitName { get; set; }

        /// <summary>
        /// 大转小
        /// </summary>
        public int? BigQuantity { get; set; }
        /// <summary>
        /// 中转小
        /// </summary>
        public int? StrokeQuantity { get; set; }

        /// <summary>
        /// 销售、退货数量(小)
        /// </summary>
        public int? SaleReturnSmallQuantity { get; set; }
        /// <summary>
        /// 销售、退货数量(中)
        /// </summary>
        public int? SaleReturnStrokeQuantity { get; set; }
        /// <summary>
        /// 销数、退货量(大)
        /// </summary>
        public int? SaleReturnBigQuantity { get; set; }

        /// <summary>
        /// 单位换算
        /// </summary>
        public string UnitConversion { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int? Quantity { get; set; }

        /// <summary>
        /// 单位Id
        /// </summary>
        public int? UnitId { get; set; }

        /// <summary>
        /// 单位名称
        /// </summary>
        public string UnitName { get; set; }


        /// <summary>
        /// 单价
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// 成本金额
        /// </summary>
        public decimal? CostAmount { get; set; }

        /// <summary>
        /// 利润
        /// </summary>
        public decimal? Profit { get; set; }

        /// <summary>
        /// 成本利润率
        /// </summary>
        public decimal? CostProfitRate { get; set; }

        /// <summary>
        /// 系统价
        /// </summary>
        public decimal? SystemPrice { get; set; }

        /// <summary>
        /// 变动差额=（实际价格-系统价格）*商品数量
        /// </summary>
        public decimal? ChangeDifference { get; set; }

        /// <summary>
        /// 预设进价
        /// </summary>
        public decimal? PresetPrice { get; set; }

        /// <summary>
        /// 最近采购价
        /// </summary>
        public decimal? RecentPurchasesPrice { get; set; }

        /// <summary>
        /// 最近结算成本价
        /// </summary>
        public decimal? RecentSettlementCostPrice { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 商品分类名称
        /// </summary>

        public string CategoryName { get; set; }
        /// <summary>
        /// 品牌名称
        /// </summary>
        public string BrandName { get; set; }
    }
    #endregion

    #region 销售汇总（按商品）

    /// <summary>
    /// 销售汇总（按商品）
    /// </summary>
    public class SaleReportSummaryProduct
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
        /// 单位Id（小）
        /// </summary>
        public int? SmallUnitId { get; set; }
        /// <summary>
        /// 单位名称（小）
        /// </summary>
        public string SmallUnitName { get; set; }
        /// <summary>
        /// 单位Id（中）
        /// </summary>
        public int? StrokeUnitId { get; set; }
        /// <summary>
        /// 单位名称（中）
        /// </summary>
        public string StrokeUnitName { get; set; }
        /// <summary>
        /// 单位Id（大）
        /// </summary>
        public int? BigUnitId { get; set; }
        /// <summary>
        /// 单位名称（大）
        /// </summary>
        public string BigUnitName { get; set; }

        /// <summary>
        /// 大转小
        /// </summary>
        public int? BigQuantity { get; set; }
        /// <summary>
        /// 中转小
        /// </summary>
        public int? StrokeQuantity { get; set; }

        /// <summary>
        /// 单位换算
        /// </summary>
        public string UnitConversion { get; set; }

        /// <summary>
        /// 销售数量(小)
        /// </summary>
        public int? SaleSmallQuantity { get; set; }
        /// <summary>
        /// 销售数量(中)
        /// </summary>
        public int? SaleStrokeQuantity { get; set; }
        /// <summary>
        /// 销售数量(大)
        /// </summary>
        public int? SaleBigQuantity { get; set; }
        /// <summary>
        /// 销售数量(数量转换)
        /// </summary>
        public string SaleQuantityConversion { get; set; }

        /// <summary>
        /// 销售金额
        /// </summary>
        public decimal? SaleAmount { get; set; }

        /// <summary>
        /// 赠送数量(小)
        /// </summary>
        public int? GiftSmallQuantity { get; set; }
        /// <summary>
        /// 赠送数量(中)
        /// </summary>
        public int? GiftStrokeQuantity { get; set; }
        /// <summary>
        /// 赠送数量(大)
        /// </summary>
        public int? GiftBigQuantity { get; set; }
        /// <summary>
        /// 赠送数量(数量转换)
        /// </summary>
        public string GiftQuantityConversion { get; set; }

        /// <summary>
        /// 退货数量(小)
        /// </summary>
        public int? ReturnSmallQuantity { get; set; }
        /// <summary>
        /// 退货数量(中)
        /// </summary>
        public int? ReturnStrokeQuantity { get; set; }
        /// <summary>
        /// 退货数量(大)
        /// </summary>
        public int? ReturnBigQuantity { get; set; }
        /// <summary>
        /// 退货数量(数量转换)
        /// </summary>
        public string ReturnQuantityConversion { get; set; }

        /// <summary>
        /// 退货金额
        /// </summary>
        public decimal? ReturnAmount { get; set; }

        /// <summary>
        /// 净销售量 = 销售数量 - 退货数量 (小)
        /// </summary>
        public int? NetSmallQuantity { get; set; }
        /// <summary>
        /// 净销售量 = 销售数量 - 退货数量 (中)
        /// </summary>
        public int? NetStrokeQuantity { get; set; }
        /// <summary>
        /// 净销售量 = 销售数量 - 退货数量 (大)
        /// </summary>
        public int? NetBigQuantity { get; set; }
        /// <summary>
        /// 净销售量 = 销售数量 - 退货数量 (数量转换)
        /// </summary>
        public string NetQuantityConversion { get; set; }

        /// <summary>
        /// 销售净额 = 销售金额 - 退货金额
        /// </summary>
        public decimal? NetAmount { get; set; }

        /// <summary>
        /// 成本金额
        /// </summary>
        public decimal? CostAmount { get; set; }

        /// <summary>
        /// 利润
        /// </summary>
        public decimal? Profit { get; set; }

        /// <summary>
        /// 成本利润率
        /// </summary>
        public decimal? CostProfitRate { get; set; }

    }
    #endregion

    #region 销售汇总（按客户）

    public class SaleReportSummaryCustomerQuery
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
        /// 统计类别Id
        /// </summary>
        public int? StatisticalTypeId { get; set; }

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
        /// 单位Id（小）
        /// </summary>
        public int? SmallUnitId { get; set; }
        /// <summary>
        /// 单位名称（小）
        /// </summary>
        public string SmallUnitName { get; set; }
        /// <summary>
        /// 单位Id（中）
        /// </summary>
        public int? StrokeUnitId { get; set; }
        /// <summary>
        /// 单位名称（中）
        /// </summary>
        public string StrokeUnitName { get; set; }
        /// <summary>
        /// 单位Id（大）
        /// </summary>
        public int? BigUnitId { get; set; }
        /// <summary>
        /// 单位名称（大）
        /// </summary>
        public string BigUnitName { get; set; }

        /// <summary>
        /// 大转小
        /// </summary>
        public int? BigQuantity { get; set; }
        /// <summary>
        /// 中转小
        /// </summary>
        public int? StrokeQuantity { get; set; }

        /// <summary>
        /// 销售数量
        /// </summary>
        public int? SaleQuantity { get; set; }

        /// <summary>
        /// 销售单位
        /// </summary>
        public int? SaleUnitId { get; set; }

        /// <summary>
        /// 销售金额
        /// </summary>
        public decimal? SaleAmount { get; set; }

        ///// <summary>
        ///// 赠送数量
        ///// </summary>
        //public int? GiftQuantity { get; set; }

        ///// <summary>
        ///// 赠送单位
        ///// </summary>
        //public int? GiftUnitId { get; set; }

        /// <summary>
        /// 退货数量
        /// </summary>
        public int? ReturnQuantity { get; set; }

        /// <summary>
        /// 退货单位
        /// </summary>
        public int? ReturnUnitId { get; set; }

        /// <summary>
        /// 退货金额
        /// </summary>
        public decimal? ReturnAmount { get; set; }

        /// <summary>
        /// 净销售量 = 销售数量 - 退货数量
        /// </summary>
        public int? NetQuantity { get; set; }

        /// <summary>
        /// 销售净额 = 销售金额 - 退货金额
        /// </summary>
        public decimal? NetAmount { get; set; }

        /// <summary>
        /// 成本金额
        /// </summary>
        public decimal? CostAmount { get; set; }

        /// <summary>
        /// 利润
        /// </summary>
        public decimal? Profit { get; set; }

        /// <summary>
        /// 成本利润率
        /// </summary>
        public decimal? CostProfitRate { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool IsGifts { get; set; }

    }

    /// <summary>
    /// 销售汇总（按客户）
    /// </summary>
    public class SaleReportSummaryCustomer
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

        private ICollection<SaleReportStatisticalType> _saleReportStatisticalTypes;
        /// <summary>
        /// (导航)商品类别
        /// </summary>
        public virtual ICollection<SaleReportStatisticalType> SaleReportStatisticalTypes
        {
            get { return _saleReportStatisticalTypes ?? (_saleReportStatisticalTypes = new List<SaleReportStatisticalType>()); }
            protected set { _saleReportStatisticalTypes = value; }
        }

        /// <summary>
        /// 销售数量
        /// </summary>
        public int? SaleSmallQuantity { get; set; }

        /// <summary>
        /// 退货数量
        /// </summary>
        public int? ReturnSmallQuantity { get; set; }

        /// <summary>
        /// 净销售量 = 销售数量 - 退货数量
        /// </summary>
        public int? NetSmallQuantity { get; set; }

        /// <summary>
        /// 销售金额
        /// </summary>
        public decimal? SaleAmount { get; set; }

        /// <summary>
        /// 退货金额
        /// </summary>
        public decimal? ReturnAmount { get; set; }

        /// <summary>
        /// 销售净额 = 销售金额 - 退货金额
        /// </summary>
        public decimal? NetAmount { get; set; }

        /// <summary>
        /// 优惠金额
        /// </summary>
        public decimal? DiscountAmount { get; set; }

        /// <summary>
        /// 成本金额
        /// </summary>
        public decimal? CostAmount { get; set; }

        /// <summary>
        /// 利润
        /// </summary>
        public decimal? Profit { get; set; }

        /// <summary>
        /// 成本利润率
        /// </summary>
        public decimal? CostProfitRate { get; set; }
        /// <summary>
        /// 赠送数量
        /// </summary>
        public int? GiftQuantity { get; set; } = 0;
    }


    /// <summary>
    /// 客户排行榜（API）
    /// </summary>
    public class CustomerRanking
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
        /// 拜访数
        /// </summary>
        public int? VisitSum { get; set; }

        /// <summary>
        /// 销售
        /// </summary>
        public decimal? SaleAmount { get; set; }

        /// <summary>
        /// 销退
        /// </summary>
        public decimal? SaleReturnAmount { get; set; }

        /// <summary>
        /// 净额
        /// </summary>
        public decimal? NetAmount { get; set; }
    }



    #endregion

    #region 销售汇总（按业务员）

    public class SaleReportSummaryBusinessUserQuery
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
        /// 统计类别Id
        /// </summary>
        public int? StatisticalTypeId { get; set; }

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
        /// 单位Id（小）
        /// </summary>
        public int? SmallUnitId { get; set; }
        /// <summary>
        /// 单位名称（小）
        /// </summary>
        public string SmallUnitName { get; set; }
        /// <summary>
        /// 单位Id（中）
        /// </summary>
        public int? StrokeUnitId { get; set; }
        /// <summary>
        /// 单位名称（中）
        /// </summary>
        public string StrokeUnitName { get; set; }
        /// <summary>
        /// 单位Id（大）
        /// </summary>
        public int? BigUnitId { get; set; }
        /// <summary>
        /// 单位名称（大）
        /// </summary>
        public string BigUnitName { get; set; }

        /// <summary>
        /// 大转小
        /// </summary>
        public int? BigQuantity { get; set; }
        /// <summary>
        /// 中转小
        /// </summary>
        public int? StrokeQuantity { get; set; }

        /// <summary>
        /// 销售数量
        /// </summary>
        public int? SaleQuantity { get; set; }

        /// <summary>
        /// 销售单位
        /// </summary>
        public int? SaleUnitId { get; set; }

        /// <summary>
        /// 销售金额
        /// </summary>
        public decimal? SaleAmount { get; set; }

        ///// <summary>
        ///// 赠送数量
        ///// </summary>
        //public int? GiftQuantity { get; set; }

        ///// <summary>
        ///// 赠送单位
        ///// </summary>
        //public int? GiftUnitId { get; set; }

        /// <summary>
        /// 退货数量
        /// </summary>
        public int? ReturnQuantity { get; set; }

        /// <summary>
        /// 退货单位
        /// </summary>
        public int? ReturnUnitId { get; set; }

        /// <summary>
        /// 退货金额
        /// </summary>
        public decimal? ReturnAmount { get; set; }

        /// <summary>
        /// 净销售量 = 销售数量 - 退货数量
        /// </summary>
        public int? NetQuantity { get; set; }

        /// <summary>
        /// 销售净额 = 销售金额 - 退货金额
        /// </summary>
        public decimal? NetAmount { get; set; }

        /// <summary>
        /// 成本金额
        /// </summary>
        public decimal? CostAmount { get; set; }

        /// <summary>
        /// 利润
        /// </summary>
        public decimal? Profit { get; set; }

        /// <summary>
        /// 成本利润率
        /// </summary>
        public decimal? CostProfitRate { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool IsGifts { get; set; }
    }

    /// <summary>
    /// 销售汇总（按业务员）
    /// </summary>
    public class SaleReportSummaryBusinessUser
    {
        /// <summary>
        /// 业务员Id
        /// </summary>
        public int? BusinessUserId { get; set; }

        /// <summary>
        /// 业务员名称
        /// </summary>
        public string BusinessUserName { get; set; }


        private ICollection<SaleReportStatisticalType> _saleReportStatisticalTypes;
        /// <summary>
        /// (导航)商品类别
        /// </summary>
        public virtual ICollection<SaleReportStatisticalType> SaleReportStatisticalTypes
        {
            get { return _saleReportStatisticalTypes ?? (_saleReportStatisticalTypes = new List<SaleReportStatisticalType>()); }
            protected set { _saleReportStatisticalTypes = value; }
        }

        /// <summary>
        /// 销售数量
        /// </summary>
        public int? SaleSmallQuantity { get; set; }

        /// <summary>
        /// 退货数量
        /// </summary>
        public int? ReturnSmallQuantity { get; set; }

        /// <summary>
        /// 净销售量 = 销售数量 - 退货数量
        /// </summary>
        public int? NetSmallQuantity { get; set; }

        /// <summary>
        /// 销售金额
        /// </summary>
        public decimal? SaleAmount { get; set; }

        /// <summary>
        /// 退货金额
        /// </summary>
        public decimal? ReturnAmount { get; set; }

        /// <summary>
        /// 销售净额 = 销售金额 - 退货金额
        /// </summary>
        public decimal? NetAmount { get; set; }

        /// <summary>
        /// 优惠金额
        /// </summary>
        public decimal? DiscountAmount { get; set; }

        /// <summary>
        /// 成本金额
        /// </summary>
        public decimal? CostAmount { get; set; }

        /// <summary>
        /// 利润
        /// </summary>
        public decimal? Profit { get; set; }

        /// <summary>
        /// 成本利润率
        /// </summary>
        public decimal? CostProfitRate { get; set; }
        /// <summary>
        /// 赠送数量
        /// </summary>
        public int? GiftQuantity { get; set; } = 0;
    }


    /// <summary>
    /// 业务员排行榜（API）
    /// </summary>
    public class BusinessRanking : BaseEntity
    {

        /// <summary>
        /// 业务员
        /// </summary>
        public int BusinessUserId { get; set; }
        public string BusinessUserName { get; set; }


        /// <summary>
        /// 利润
        /// </summary>
        public decimal? Profit { get; set; }

        /// <summary>
        /// 销售
        /// </summary>
        public decimal? SaleAmount { get; set; }

        /// <summary>
        /// 销退
        /// </summary>
        public decimal? SaleReturnAmount { get; set; }

        /// <summary>
        /// 净额
        /// </summary>
        public decimal? NetAmount { get; set; }

        public object Model<T>()
        {
            throw new NotImplementedException();
        }
    }

    #endregion

    #region 销售汇总（客户/商品）

    /// <summary>
    /// 销售汇总（客户/商品）
    /// </summary>
    public class SaleReportSummaryCustomerProduct
    {
        /// <summary>
        /// 单据id
        /// </summary>
        public int BillId { get; set; }

        /// <summary>
        /// 汇总行（根据客户汇总）
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool? SumRowType { get; set; }

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
        /// 单位Id（小）
        /// </summary>
        public int? SmallUnitId { get; set; }
        /// <summary>
        /// 单位名称（小）
        /// </summary>
        public string SmallUnitName { get; set; }
        /// <summary>
        /// 单位Id（中）
        /// </summary>
        public int? StrokeUnitId { get; set; }
        /// <summary>
        /// 单位名称（中）
        /// </summary>
        public string StrokeUnitName { get; set; }
        /// <summary>
        /// 单位Id（大）
        /// </summary>
        public int? BigUnitId { get; set; }
        /// <summary>
        /// 单位名称（大）
        /// </summary>
        public string BigUnitName { get; set; }

        /// <summary>
        /// 大转小
        /// </summary>
        public int? BigQuantity { get; set; }
        /// <summary>
        /// 中转小
        /// </summary>
        public int? StrokeQuantity { get; set; }

        /// <summary>
        /// 销售数量(小)
        /// </summary>
        public int? SaleSmallQuantity { get; set; }
        /// <summary>
        /// 销售数量(中)
        /// </summary>
        public int? SaleStrokeQuantity { get; set; }
        /// <summary>
        /// 销售数量(大)
        /// </summary>
        public int? SaleBigQuantity { get; set; }
        /// <summary>
        /// 销售数量(数量转换)
        /// </summary>
        public string SaleQuantityConversion { get; set; }

        /// <summary>
        /// 销售金额
        /// </summary>
        public decimal? SaleAmount { get; set; }

        /// <summary>
        /// 退货数量(小)
        /// </summary>
        public int? ReturnSmallQuantity { get; set; }
        /// <summary>
        /// 退货数量(中)
        /// </summary>
        public int? ReturnStrokeQuantity { get; set; }
        /// <summary>
        /// 退货数量(大)
        /// </summary>
        public int? ReturnBigQuantity { get; set; }
        /// <summary>
        /// 退货数量(数量转换)
        /// </summary>
        public string ReturnQuantityConversion { get; set; }

        /// <summary>
        /// 退货金额
        /// </summary>
        public decimal? ReturnAmount { get; set; }

        /// <summary>
        /// 还货数量(小)
        /// </summary>
        public int? RepaymentSmallQuantity { get; set; }
        /// <summary>
        /// 还货数量(中)
        /// </summary>
        public int? RepaymentStrokeQuantity { get; set; }
        /// <summary>
        /// 还货数量(大)
        /// </summary>
        public int? RepaymentBigQuantity { get; set; }
        /// <summary>
        /// 还货数量(数量转换)
        /// </summary>
        public string RepaymentQuantityConversion { get; set; }

        /// <summary>
        /// 还货金额
        /// </summary>
        public decimal? RepaymentAmount { get; set; }

        /// <summary>
        /// 总数量
        /// </summary>
        public string SumQuantityConversion { get; set; }

        /// <summary>
        /// 总金额
        /// </summary>
        public decimal? SumAmount { get; set; }

        /// <summary>
        /// 成本金额
        /// </summary>
        public decimal? CostAmount { get; set; }

        /// <summary>
        /// 利润
        /// </summary>
        public decimal? Profit { get; set; }

        /// <summary>
        /// 成本利润率
        /// </summary>
        public decimal? CostProfitRate { get; set; }
        /// <summary>
        /// 优惠
        /// </summary>
        public decimal? DiscountAmount { get; set; }
        /// <summary>
        /// 是否赠品
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool IsGifts { get; set; }
        /// <summary>
        /// 赠送数量
        /// </summary>
        [NotMapped]
        public string GiftQuantityConversion { get; set; }
        /// <summary>
        /// 赠送数量(小)
        /// </summary>
        [NotMapped]
        public int? GiftSmallQuantity { get; set; } = 0;
        /// <summary>
        /// 赠送数量(中)
        /// </summary>
        [NotMapped]
        public int? GiftStrokeQuantity { get; set; } = 0;
        /// <summary>
        /// 赠送数量(大)
        /// </summary>
        [NotMapped]
        public int? GiftBigQuantity { get; set; } = 0;
    }

    #endregion

    #region 销售汇总（按仓库）

    public class SaleReportSummaryWareHouseQuery
    {
        public int BillId { get; set; }

        /// <summary>
        /// 仓库Id
        /// </summary>
        public int? WareHouseId { get; set; }

        /// <summary>
        /// 仓库名称
        /// </summary>
        public string WareHouseName { get; set; }

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
        /// 统计类别Id
        /// </summary>
        public int? StatisticalTypeId { get; set; }

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
        /// 单位Id（小）
        /// </summary>
        public int? SmallUnitId { get; set; }
        /// <summary>
        /// 单位名称（小）
        /// </summary>
        public string SmallUnitName { get; set; }
        /// <summary>
        /// 单位Id（中）
        /// </summary>
        public int? StrokeUnitId { get; set; }
        /// <summary>
        /// 单位名称（中）
        /// </summary>
        public string StrokeUnitName { get; set; }
        /// <summary>
        /// 单位Id（大）
        /// </summary>
        public int? BigUnitId { get; set; }
        /// <summary>
        /// 单位名称（大）
        /// </summary>
        public string BigUnitName { get; set; }

        /// <summary>
        /// 大转小
        /// </summary>
        public int? BigQuantity { get; set; }
        /// <summary>
        /// 中转小
        /// </summary>
        public int? StrokeQuantity { get; set; }

        /// <summary>
        /// 销售数量
        /// </summary>
        public int? SaleQuantity { get; set; }

        /// <summary>
        /// 销售单位
        /// </summary>
        public int? SaleUnitId { get; set; }

        /// <summary>
        /// 销售金额
        /// </summary>
        public decimal? SaleAmount { get; set; }

        ///// <summary>
        ///// 赠送数量
        ///// </summary>
        //public int? GiftQuantity { get; set; }

        ///// <summary>
        ///// 赠送单位
        ///// </summary>
        //public int? GiftUnitId { get; set; }

        /// <summary>
        /// 退货数量
        /// </summary>
        public int? ReturnQuantity { get; set; }

        /// <summary>
        /// 退货单位
        /// </summary>
        public int? ReturnUnitId { get; set; }

        /// <summary>
        /// 退货金额
        /// </summary>
        public decimal? ReturnAmount { get; set; }

        /// <summary>
        /// 净销售量 = 销售数量 - 退货数量
        /// </summary>
        public int? NetQuantity { get; set; }

        /// <summary>
        /// 销售净额 = 销售金额 - 退货金额
        /// </summary>
        public decimal? NetAmount { get; set; }

        /// <summary>
        /// 成本金额
        /// </summary>
        public decimal? CostAmount { get; set; }

        /// <summary>
        /// 利润
        /// </summary>
        public decimal? Profit { get; set; }

        /// <summary>
        /// 成本利润率
        /// </summary>
        public decimal? CostProfitRate { get; set; }
        /// <summary>
        /// 是否赠品
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool IsGifts { get; set; }
    }

    /// <summary>
    /// 销售汇总（按仓库）
    /// </summary>
    public class SaleReportSummaryWareHouse
    {
        /// <summary>
        /// 仓库Id
        /// </summary>
        public int? WareHouseId { get; set; }

        /// <summary>
        /// 仓库名称
        /// </summary>
        public string WareHouseName { get; set; }

        private ICollection<SaleReportStatisticalType> _saleReportStatisticalTypes;
        /// <summary>
        /// (导航)商品类别
        /// </summary>
        public virtual ICollection<SaleReportStatisticalType> SaleReportStatisticalTypes
        {
            get { return _saleReportStatisticalTypes ?? (_saleReportStatisticalTypes = new List<SaleReportStatisticalType>()); }
            protected set { _saleReportStatisticalTypes = value; }
        }

        /// <summary>
        /// 销售数量
        /// </summary>
        public int? SaleSmallQuantity { get; set; }

        /// <summary>
        /// 退货数量
        /// </summary>
        public int? ReturnSmallQuantity { get; set; }

        /// <summary>
        /// 净销售量 = 销售数量 - 退货数量
        /// </summary>
        public int? NetSmallQuantity { get; set; }

        /// <summary>
        /// 销售金额
        /// </summary>
        public decimal? SaleAmount { get; set; }

        /// <summary>
        /// 退货金额
        /// </summary>
        public decimal? ReturnAmount { get; set; }

        /// <summary>
        /// 销售净额 = 销售金额 - 退货金额
        /// </summary>
        public decimal? NetAmount { get; set; }

        /// <summary>
        /// 优惠金额
        /// </summary>
        public decimal? DiscountAmount { get; set; }

        /// <summary>
        /// 成本金额
        /// </summary>
        public decimal? CostAmount { get; set; }

        /// <summary>
        /// 利润
        /// </summary>
        public decimal? Profit { get; set; }

        /// <summary>
        /// 成本利润率
        /// </summary>
        public decimal? CostProfitRate { get; set; }
        /// <summary>
        /// 赠送数量
        /// </summary>
        public int? GiftQuantity { get; set; }

    }

    #endregion

    #region 销售汇总（按品牌）

    public class SaleReportSummaryBrandQuery
    {
        /// <summary>
        /// 品牌Id
        /// </summary>
        public int? BrandId { get; set; }

        /// <summary>
        /// 品牌名称
        /// </summary>
        public string BrandName { get; set; }

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
        /// 统计类别Id
        /// </summary>
        public int? StatisticalTypeId { get; set; }

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
        /// 单位Id（小）
        /// </summary>
        public int? SmallUnitId { get; set; }
        /// <summary>
        /// 单位名称（小）
        /// </summary>
        public string SmallUnitName { get; set; }
        /// <summary>
        /// 单位Id（中）
        /// </summary>
        public int? StrokeUnitId { get; set; }
        /// <summary>
        /// 单位名称（中）
        /// </summary>
        public string StrokeUnitName { get; set; }
        /// <summary>
        /// 单位Id（大）
        /// </summary>
        public int? BigUnitId { get; set; }
        /// <summary>
        /// 单位名称（大）
        /// </summary>
        public string BigUnitName { get; set; }

        /// <summary>
        /// 大转小
        /// </summary>
        public int? BigQuantity { get; set; }
        /// <summary>
        /// 中转小
        /// </summary>
        public int? StrokeQuantity { get; set; }

        /// <summary>
        /// 销售数量
        /// </summary>
        public int? SaleQuantity { get; set; }

        /// <summary>
        /// 销售单位
        /// </summary>
        public int? SaleUnitId { get; set; }

        /// <summary>
        /// 销售金额
        /// </summary>
        public decimal? SaleAmount { get; set; }

        ///// <summary>
        ///// 赠送数量
        ///// </summary>
        //public int? GiftQuantity { get; set; }

        ///// <summary>
        ///// 赠送单位
        ///// </summary>
        //public int? GiftUnitId { get; set; }

        /// <summary>
        /// 退货数量
        /// </summary>
        public int? ReturnQuantity { get; set; }

        /// <summary>
        /// 退货单位
        /// </summary>
        public int? ReturnUnitId { get; set; }

        /// <summary>
        /// 退货金额
        /// </summary>
        public decimal? ReturnAmount { get; set; }

        /// <summary>
        /// 净销售量 = 销售数量 - 退货数量
        /// </summary>
        public int? NetQuantity { get; set; }

        /// <summary>
        /// 销售净额 = 销售金额 - 退货金额
        /// </summary>
        public decimal? NetAmount { get; set; }

        /// <summary>
        /// 成本金额
        /// </summary>
        public decimal? CostAmount { get; set; }

        /// <summary>
        /// 利润
        /// </summary>
        public decimal? Profit { get; set; }

        /// <summary>
        /// 成本利润率
        /// </summary>
        public decimal? CostProfitRate { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool IsGifts { get; set; }
    }

    /// <summary>
    /// 销售汇总（按品牌）
    /// </summary>
    public class SaleReportSummaryBrand
    {
        /// <summary>
        /// 品牌Id
        /// </summary>
        public int? BrandId { get; set; }

        /// <summary>
        /// 品牌名称
        /// </summary>
        public string BrandName { get; set; }

        private ICollection<SaleReportStatisticalType> _saleReportStatisticalTypes;
        /// <summary>
        /// (导航)商品类别
        /// </summary>
        public virtual ICollection<SaleReportStatisticalType> SaleReportStatisticalTypes
        {
            get { return _saleReportStatisticalTypes ?? (_saleReportStatisticalTypes = new List<SaleReportStatisticalType>()); }
            protected set { _saleReportStatisticalTypes = value; }
        }

        /// <summary>
        /// 销售数量
        /// </summary>
        public int? SaleSmallQuantity { get; set; }

        /// <summary>
        /// 退货数量
        /// </summary>
        public int? ReturnSmallQuantity { get; set; }

        /// <summary>
        /// 净销售量 = 销售数量 - 退货数量
        /// </summary>
        public int? NetSmallQuantity { get; set; }

        /// <summary>
        /// 销售金额
        /// </summary>
        public decimal? SaleAmount { get; set; }

        /// <summary>
        /// 退货金额
        /// </summary>
        public decimal? ReturnAmount { get; set; }

        /// <summary>
        /// 销售净额 = 销售金额 - 退货金额
        /// </summary>
        public decimal? NetAmount { get; set; }

        /// <summary>
        /// 优惠金额
        /// </summary>
        public decimal? DiscountAmount { get; set; }

        /// <summary>
        /// 成本金额
        /// </summary>
        public decimal? CostAmount { get; set; }

        /// <summary>
        /// 利润
        /// </summary>
        public decimal? Profit { get; set; }

        /// <summary>
        /// 成本利润率
        /// </summary>
        public decimal? CostProfitRate { get; set; }
        /// <summary>
        /// 赠送数量
        /// </summary>
        public int? GiftSmallQuantity { get; set; } = 0;
    }

    /// <summary>
    /// 品牌销量汇总(API)
    /// </summary>
    public class BrandRanking : BaseEntity
    {
        /// <summary>
        /// 品牌
        /// </summary>
        public int BrandId { get; set; }
        public string BrandName { get; set; }

        /// <summary>
        /// 利润
        /// </summary>
        public decimal? Profit { get; set; }

        /// <summary>
        /// 销售
        /// </summary>
        public decimal? SaleAmount { get; set; }

        /// <summary>
        /// 销退
        /// </summary>
        public decimal? SaleReturnAmount { get; set; }

        /// <summary>
        /// 净额
        /// </summary>
        public decimal? NetAmount { get; set; }

        /// <summary>
        /// 比例
        /// </summary>
        public double? Percentage { get; set; }
    }

    #endregion

    #region 订单明细
    /// <summary>
    /// 订单明细
    /// </summary>
    public class SaleReportOrderItem
    {

        /// <summary>
        /// 订单Id
        /// </summary>
        public int? ReservationBillId { get; set; }

        /// <summary>
        /// 订单编号
        /// </summary>
        public string ReservationBillNumber { get; set; }

        /// <summary>
        /// 单据类型Id
        /// </summary>
        public int? BillTypeId { get; set; }

        /// <summary>
        /// 单价类型名称
        /// </summary>
        public string BillTypeName { get; set; }

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
        /// 业务员Id
        /// </summary>
        public int? BusinessUserId { get; set; }

        /// <summary>
        /// 业务员名称
        /// </summary>
        public string BusinessUserName { get; set; }

        /// <summary>
        /// 交易时间
        /// </summary>
        public DateTime? TransactionDate { get; set; }

        /// <summary>
        /// 审核时间
        /// </summary>
        public DateTime? AuditedDate { get; set; }

        /// <summary>
        /// 仓库Id
        /// </summary>
        public int? WareHouseId { get; set; }

        /// <summary>
        /// 仓库名称
        /// </summary>
        public string WareHouseName { get; set; }

        /// <summary>
        /// 商品Id
        /// </summary>
        public int? ProductId { get; set; }

        /// <summary>
        /// 商品编码
        /// </summary>
        public string ProductSKU { get; set; }

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
        /// 单位Id（小）
        /// </summary>
        public int? SmallUnitId { get; set; }
        /// <summary>
        /// 单位名称（小）
        /// </summary>
        public string SmallUnitName { get; set; }
        /// <summary>
        /// 单位Id（中）
        /// </summary>
        public int? StrokeUnitId { get; set; }
        /// <summary>
        /// 单位名称（中）
        /// </summary>
        public string StrokeUnitName { get; set; }
        /// <summary>
        /// 单位Id（大）
        /// </summary>
        public int? BigUnitId { get; set; }
        /// <summary>
        /// 单位名称（大）
        /// </summary>
        public string BigUnitName { get; set; }

        /// <summary>
        /// 大转小
        /// </summary>
        public int? BigQuantity { get; set; }
        /// <summary>
        /// 中转小
        /// </summary>
        public int? StrokeQuantity { get; set; }

        /// <summary>
        /// 销售、退货数量(小)
        /// </summary>
        public int? SaleReturnSmallQuantity { get; set; }
        /// <summary>
        /// 销售、退货数量(中)
        /// </summary>
        public int? SaleReturnStrokeQuantity { get; set; }
        /// <summary>
        /// 销数、退货量(大)
        /// </summary>
        public int? SaleReturnBigQuantity { get; set; }

        /// <summary>
        /// 条形码
        /// </summary>
        public string BarCode { get; set; }

        /// <summary>
        /// 单位换算
        /// </summary>
        public string UnitConversion { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int? Quantity { get; set; }

        /// <summary>
        /// 单位Id
        /// </summary>
        public int? UnitId { get; set; }

        /// <summary>
        /// 单位名称
        /// </summary>
        public string UnitName { get; set; }

        /// <summary>
        /// 单位类型（大，中，小）
        /// </summary>
        public string UnitBigStrokeSmall { get; set; }

        /// <summary>
        /// 单价
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// 成本金额
        /// </summary>
        public decimal? CostAmount { get; set; }

        /// <summary>
        /// 利润
        /// </summary>
        public decimal? Profit { get; set; }

        /// <summary>
        /// 成本利润率
        /// </summary>
        public decimal? CostProfitRate { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 是否费用合同兑现商品
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool CostContractProduct { get; set; }
        /// <summary>
        /// 是否只展示占用库存商品
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool OccupyStock { get; set; }

    }
    #endregion

    #region 订单汇总（按商品）
    /// <summary>
    /// 订单汇总（按商品）
    /// </summary>
    public class SaleReportSummaryOrderProduct
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
        /// 单位Id（小）
        /// </summary>
        public int? SmallUnitId { get; set; }
        /// <summary>
        /// 单位名称（小）
        /// </summary>
        public string SmallUnitName { get; set; }
        /// <summary>
        /// 单位Id（中）
        /// </summary>
        public int? StrokeUnitId { get; set; }
        /// <summary>
        /// 单位名称（中）
        /// </summary>
        public string StrokeUnitName { get; set; }
        /// <summary>
        /// 单位Id（大）
        /// </summary>
        public int? BigUnitId { get; set; }
        /// <summary>
        /// 单位名称（大）
        /// </summary>
        public string BigUnitName { get; set; }

        /// <summary>
        /// 大转小
        /// </summary>
        public int? BigQuantity { get; set; }
        /// <summary>
        /// 中转小
        /// </summary>
        public int? StrokeQuantity { get; set; }

        /// <summary>
        /// 单位换算
        /// </summary>
        public string UnitConversion { get; set; }

        /// <summary>
        /// 销售数量(小)
        /// </summary>
        public int? SaleSmallQuantity { get; set; }
        /// <summary>
        /// 销售数量(中)
        /// </summary>
        public int? SaleStrokeQuantity { get; set; }
        /// <summary>
        /// 销售数量(大)
        /// </summary>
        public int? SaleBigQuantity { get; set; }
        /// <summary>
        /// 销售数量(数量转换)
        /// </summary>
        public string SaleQuantityConversion { get; set; }

        /// <summary>
        /// 销售金额
        /// </summary>
        public decimal? SaleAmount { get; set; }

        /// <summary>
        /// 赠送数量(小)
        /// </summary>
        public int? GiftSmallQuantity { get; set; }
        /// <summary>
        /// 赠送数量(中)
        /// </summary>
        public int? GiftStrokeQuantity { get; set; }
        /// <summary>
        /// 赠送数量(大)
        /// </summary>
        public int? GiftBigQuantity { get; set; }
        /// <summary>
        /// 赠送数量(数量转换)
        /// </summary>
        public string GiftQuantityConversion { get; set; }

        /// <summary>
        /// 退货数量(小)
        /// </summary>
        public int? ReturnSmallQuantity { get; set; }
        /// <summary>
        /// 退货数量(中)
        /// </summary>
        public int? ReturnStrokeQuantity { get; set; }
        /// <summary>
        /// 退货数量(大)
        /// </summary>
        public int? ReturnBigQuantity { get; set; }
        /// <summary>
        /// 退货数量(数量转换)
        /// </summary>
        public string ReturnQuantityConversion { get; set; }

        /// <summary>
        /// 退货金额
        /// </summary>
        public decimal? ReturnAmount { get; set; }

        /// <summary>
        /// 净销售量 = 销售数量 - 退货数量 (小)
        /// </summary>
        public int? NetSmallQuantity { get; set; }
        /// <summary>
        /// 净销售量 = 销售数量 - 退货数量 (中)
        /// </summary>
        public int? NetStrokeQuantity { get; set; }
        /// <summary>
        /// 净销售量 = 销售数量 - 退货数量 (大)
        /// </summary>
        public int? NetBigQuantity { get; set; }
        /// <summary>
        /// 净销售量 = 销售数量 - 退货数量 (数量转换)
        /// </summary>
        public string NetQuantityConversion { get; set; }

        /// <summary>
        /// 销售净额 = 销售金额 - 退货金额
        /// </summary>
        public decimal? NetAmount { get; set; }

        /// <summary>
        /// 成本金额
        /// </summary>
        public decimal? CostAmount { get; set; }

        /// <summary>
        /// 利润
        /// </summary>
        public decimal? Profit { get; set; }

        /// <summary>
        /// 成本利润率
        /// </summary>
        public decimal? CostProfitRate { get; set; }

    }


    /// <summary>
    /// 订单额分析
    /// </summary>
    public class OrderQuantityAnalysisQuery
    {
        /// <summary>
        /// 业务员
        /// </summary>
        public int BusinessUserId { get; set; }
        public string BusinessUserName { get; set; }

        /// <summary>
        /// 品牌
        /// </summary>
        public int BrandId { get; set; }
        public string BrandName { get; set; }

        /// <summary>
        /// 商品
        /// </summary>
        public int ProductId { get; set; }
        public string ProductName { get; set; }

        /// <summary>
        /// 商品类别
        /// </summary>
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }

        /// 今日
        public Sale Today { get; set; } = new Sale();
        /// 今日上周同期
        public Sale LastWeekSame { get; set; } = new Sale();
        /// 昨天
        public Sale Yesterday { get; set; } = new Sale();
        ///  前天
        public Sale BeforeYesterday { get; set; } = new Sale();
        /// 上周
        public Sale LastWeek { get; set; } = new Sale();
        /// 本周
        public Sale ThisWeek { get; set; } = new Sale();
        ///  上月
        public Sale LastMonth { get; set; } = new Sale();
        /// 本月
        public Sale ThisMonth { get; set; } = new Sale();
        /// 本季
        public Sale ThisQuarter { get; set; } = new Sale();
        /// 本年
        public Sale ThisYear { get; set; } = new Sale();
    }



    /// <summary>
    /// 订单额分析 （API）
    /// </summary>
    public class OrderQuantityAnalysis
    {
        /// <summary>
        /// 业务员
        /// </summary>
        public int BusinessUserId { get; set; }
        public string BusinessUserName { get; set; }

        /// <summary>
        /// 品牌
        /// </summary>
        public int BrandId { get; set; }
        public string BrandName { get; set; }

        /// <summary>
        /// 商品
        /// </summary>
        public int ProductId { get; set; }
        public string ProductName { get; set; }

        /// <summary>
        /// 商品类别
        /// </summary>
        public int CatagoryId { get; set; }
        public string CatagoryName { get; set; }



        /// <summary>
        /// 今日
        /// </summary>
        public Sale Today { get; set; }
        /// <summary>
        /// 今日上周同期净额
        /// </summary>
        public Sale SamePeriodLastWeek { get; set; }
        /// <summary>
        /// 今日和上周同期净额差= SamePeriodLastWeek - Today
        /// </summary>
        public decimal? NetAmountBalance { get; set; } = 0;



        /// <summary>
        /// 昨天
        /// </summary>
        public Sale Yesterday { get; set; }
        /// <summary>
        /// 前天
        /// </summary>
        public Sale BeforeYesterday { get; set; }
        /// <summary>
        /// 上周
        /// </summary>
        public Sale LastWeek { get; set; }
        /// <summary>
        /// 本周
        /// </summary>
        public Sale ThisWeek { get; set; }
        /// <summary>
        /// 上月
        /// </summary>
        public Sale LastMonth { get; set; }
        /// <summary>
        /// 本月
        /// </summary>
        public Sale ThisMonth { get; set; }

        /// <summary>
        /// 本年
        /// </summary>
        public Sale ThisYear { get; set; }


        /// 本季
        public Sale ThisQuarter { get; set; } = new Sale();

    }

    #endregion

    #region 费用合同明细表
    /// <summary>
    /// 费用合同明细表
    /// </summary>
    public class SaleReportCostContractItem
    {
        /// <summary>
        /// 单据Id
        /// </summary>
        public int? BillId { get; set; }

        /// <summary>
        /// 项目类型：0 商品，1：现金
        /// </summary>
        public int CType { get; set; }

        /// <summary>
        /// 合同类型：0:按月兑付,1:按单位量总计兑付,2:从主管赠品扣减
        /// </summary>
        public int? ContractType { get; set; } = 0;

        /// <summary>
        /// 单据编号
        /// </summary>
        public string BillNumber { get; set; }

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
        /// 费用类别Id
        /// </summary>
        public int AccountingOptionId { get; set; }

        /// <summary>
        /// 费用类别名称
        /// </summary>
        public string AccountingOptionName { get; set; }

        /// <summary>
        /// 业务员Id
        /// </summary>
        public int? BusinessUserId { get; set; }

        /// <summary>
        /// 业务员名称
        /// </summary>
        public string BusinessUserName { get; set; }

        /// <summary>
        /// 交易时间
        /// </summary>
        public DateTime? TransactionDate { get; set; }

        /// <summary>
        /// 审核时间
        /// </summary>
        public DateTime? AuditedDate { get; set; }

        /// <summary>
        /// 商品Id
        /// </summary>
        public int? ProductId { get; set; }

        /// <summary>
        /// 商品编码
        /// </summary>
        public string ProductSKU { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 条形码
        /// </summary>
        public string BarCode { get; set; }

        /// <summary>
        /// 单位换算
        /// </summary>
        public string UnitConversion { get; set; }

        /// <summary>
        /// 单位Id
        /// </summary>
        public int? UnitId { get; set; }

        /// <summary>
        /// 单位名称
        /// </summary>
        public string UnitName { get; set; }

        /// <summary>
        /// 单位类型（大，中，小）
        /// </summary>
        public string UnitBigStrokeSmall { get; set; }

        //--
        public decimal? Jan { get; set; }
        public decimal? Feb { get; set; }
        public decimal? Mar { get; set; }
        public decimal? Apr { get; set; }
        public decimal? May { get; set; }
        public decimal? Jun { get; set; }
        public decimal? Jul { get; set; }
        public decimal? Aug { get; set; }
        public decimal? Sep { get; set; }
        public decimal? Oct { get; set; }
        public decimal? Nov { get; set; }
        public decimal? Dec { get; set; }
        //--

        /// <summary>
        /// 总计
        /// </summary>
        public decimal? Total { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }


    }
    #endregion

    #region 订货汇总
    /// <summary>
    /// 订单汇总
    /// </summary>
    public class SaleReportSummaryOrder
    {

    }
    #endregion

    #region 赠品汇总


    ///// <summary>
    ///// 赠品汇总
    ///// </summary>
    //public class SaleReportSummaryGiveQuota
    //{

    //    /// <summary>
    //    /// 客户Id
    //    /// </summary>
    //    public int? TerminalId { get; set; }

    //    /// <summary>
    //    /// 客户名称
    //    /// </summary>
    //    public string TerminalName { get; set; }

    //    /// <summary>
    //    /// 客户编码
    //    /// </summary>
    //    public string TerminalCode { get; set; }

    //    /// <summary>
    //    /// 商品Id
    //    /// </summary>
    //    public int? ProductId { get; set; }

    //    /// <summary>
    //    /// 商品编号
    //    /// </summary>
    //    public string ProductCode { get; set; }

    //    /// <summary>
    //    /// 商品名称
    //    /// </summary>
    //    public string ProductName { get; set; }

    //    /// <summary>
    //    /// 条形码（小）
    //    /// </summary>
    //    public string SmallBarCode { get; set; }
    //    /// <summary>
    //    /// 条形码（中）
    //    /// </summary>
    //    public string StrokeBarCode { get; set; }
    //    /// <summary>
    //    /// 条形码（大）
    //    /// </summary>
    //    public string BigBarCode { get; set; }

    //    /// <summary>
    //    /// 条形码
    //    /// </summary>
    //    public string BarCode { get; set; }

    //    /// <summary>
    //    /// 单位换算
    //    /// </summary>
    //    public string UnitConversion { get; set; }

    //    /// <summary>
    //    /// 单位Id（小）
    //    /// </summary>
    //    public int? SmallUnitId { get; set; }
    //    /// <summary>
    //    /// 单位名称（小）
    //    /// </summary>
    //    public string SmallUnitName { get; set; }
    //    /// <summary>
    //    /// 单位Id（中）
    //    /// </summary>
    //    public int? StrokeUnitId { get; set; }
    //    /// <summary>
    //    /// 单位名称（中）
    //    /// </summary>
    //    public string StrokeUnitName { get; set; }
    //    /// <summary>
    //    /// 单位Id（大）
    //    /// </summary>
    //    public int? BigUnitId { get; set; }
    //    /// <summary>
    //    /// 单位名称（大）
    //    /// </summary>
    //    public string BigUnitName { get; set; }

    //    /// <summary>
    //    /// 大转小
    //    /// </summary>
    //    public int? BigQuantity { get; set; }
    //    /// <summary>
    //    /// 中转小
    //    /// </summary>
    //    public int? StrokeQuantity { get; set; }

    //    /// <summary>
    //    /// 普通赠品 数量(小)
    //    /// </summary>
    //    public int? NormalSmallQuantity { get; set; }
    //    /// <summary>
    //    /// 普通赠品 数量(中)
    //    /// </summary>
    //    public int? NormalStrokeQuantity { get; set; }
    //    /// <summary>
    //    /// 普通赠品 数量(大)
    //    /// </summary>
    //    public int? NormalBigQuantity { get; set; }
    //    /// <summary>
    //    /// 普通赠品 数量(数量转换)
    //    /// </summary>
    //    public string NormalQuantityConversion { get; set; }

    //    /// <summary>
    //    /// 普通赠品 成本
    //    /// </summary>
    //    public decimal? NormalCostAmount { get; set; }

    //    /// <summary>
    //    /// 订货赠品 数量(小)
    //    /// </summary>
    //    public int? OrderSmallQuantity { get; set; }
    //    /// <summary>
    //    /// 订货赠品 数量(中)
    //    /// </summary>
    //    public int? OrderStrokeQuantity { get; set; }
    //    /// <summary>
    //    /// 订货赠品 数量(大)
    //    /// </summary>
    //    public int? OrderBigQuantity { get; set; }
    //    /// <summary>
    //    /// 订货赠品 数量(数量转换)
    //    /// </summary>
    //    public string OrderQuantityConversion { get; set; }

    //    /// <summary>
    //    /// 订货赠品 成本
    //    /// </summary>
    //    public decimal? OrderCostAmount { get; set; }

    //    /// <summary>
    //    /// 促销赠品 数量(小)
    //    /// </summary>
    //    public int? CampaignSmallQuantity { get; set; }
    //    /// <summary>
    //    /// 促销赠品 数量(中)
    //    /// </summary>
    //    public int? CampaignStrokeQuantity { get; set; }
    //    /// <summary>
    //    /// 促销赠品 数量(大)
    //    /// </summary>
    //    public int? CampaignBigQuantity { get; set; }
    //    /// <summary>
    //    /// 促销赠品 数量(数量转换)
    //    /// </summary>
    //    public string CampaignQuantityConversion { get; set; }

    //    /// <summary>
    //    /// 促销赠品 成本
    //    /// </summary>
    //    public decimal? CampaignCostAmount { get; set; }

    //    /// <summary>
    //    /// 费用合同 数量(小)
    //    /// </summary>
    //    public int? CostContractSmallQuantity { get; set; }
    //    /// <summary>
    //    /// 费用合同 数量(中)
    //    /// </summary>
    //    public int? CostContractStrokeQuantity { get; set; }
    //    /// <summary>
    //    /// 费用合同 数量(大)
    //    /// </summary>
    //    public int? CostContractBigQuantity { get; set; }
    //    /// <summary>
    //    /// 费用合同 数量(数量转换)
    //    /// </summary>
    //    public string CostContractQuantityConversion { get; set; }

    //    /// <summary>
    //    /// 费用合同 成本
    //    /// </summary>
    //    public decimal? CostContractCostAmount { get; set; }


    //    /// <summary>
    //    /// 总数量
    //    /// </summary>
    //    public string SumQuantityConversion { get; set; }

    //    /// <summary>
    //    /// 总成本金额
    //    /// </summary>
    //    public decimal? SumCostAmount { get; set; }

    //}



    #endregion

    #region 热销排行榜
    /// <summary>
    /// 热销排行榜
    /// </summary>
    public class SaleReportHotSale
    {
        /// <summary>
        /// 商品Id
        /// </summary>
        public int? ProductId { get; set; }

        public int BusinessUserId { get; set; }

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
        /// 单位Id（小）
        /// </summary>
        public int? SmallUnitId { get; set; }
        /// <summary>
        /// 单位名称（小）
        /// </summary>
        public string SmallUnitName { get; set; }
        /// <summary>
        /// 单位Id（中）
        /// </summary>
        public int? StrokeUnitId { get; set; }
        /// <summary>
        /// 单位名称（中）
        /// </summary>
        public string StrokeUnitName { get; set; }
        /// <summary>
        /// 单位Id（大）
        /// </summary>
        public int? BigUnitId { get; set; }
        /// <summary>
        /// 单位名称（大）
        /// </summary>
        public string BigUnitName { get; set; }

        /// <summary>
        /// 大转小
        /// </summary>
        public int? BigQuantity { get; set; }
        /// <summary>
        /// 中转小
        /// </summary>
        public int? StrokeQuantity { get; set; }

        /// <summary>
        /// 单位换算
        /// </summary>
        public string UnitConversion { get; set; }

        /// <summary>
        /// 销售数量(小)
        /// </summary>
        public int? SaleSmallQuantity { get; set; }
        /// <summary>
        /// 销售数量(中)
        /// </summary>
        public int? SaleStrokeQuantity { get; set; }
        /// <summary>
        /// 销售数量(大)
        /// </summary>
        public int? SaleBigQuantity { get; set; }
        /// <summary>
        /// 销售数量(数量转换)
        /// </summary>
        public string SaleQuantityConversion { get; set; }

        /// <summary>
        /// 销售金额
        /// </summary>
        public decimal? SaleAmount { get; set; }

        /// <summary>
        /// 退货数量(小)
        /// </summary>
        public int? ReturnSmallQuantity { get; set; }
        /// <summary>
        /// 退货数量(中)
        /// </summary>
        public int? ReturnStrokeQuantity { get; set; }
        /// <summary>
        /// 退货数量(大)
        /// </summary>
        public int? ReturnBigQuantity { get; set; }
        /// <summary>
        /// 退货数量(数量转换)
        /// </summary>
        public string ReturnQuantityConversion { get; set; }

        /// <summary>
        /// 退货金额
        /// </summary>
        public decimal? ReturnAmount { get; set; }

        /// <summary>
        /// 净销售量 = 销售数量 - 退货数量 (小)
        /// </summary>
        public int? NetSmallQuantity { get; set; }
        /// <summary>
        /// 净销售量 = 销售数量 - 退货数量 (中)
        /// </summary>
        public int? NetStrokeQuantity { get; set; }
        /// <summary>
        /// 净销售量 = 销售数量 - 退货数量 (大)
        /// </summary>
        public int? NetBigQuantity { get; set; }
        /// <summary>
        /// 净销售量 = 销售数量 - 退货数量 (数量转换)
        /// </summary>
        public string NetQuantityConversion { get; set; }

        /// <summary>
        /// 销售净额 = 销售金额 - 退货金额
        /// </summary>
        public decimal? NetAmount { get; set; }

        /// <summary>
        /// 成本金额
        /// </summary>
        public decimal? CostAmount { get; set; }

        /// <summary>
        /// 利润
        /// </summary>
        public decimal? Profit { get; set; }

        /// <summary>
        /// 成本利润率
        /// </summary>
        public decimal? CostProfitRate { get; set; }
    }

    /// <summary>
    /// 热销排行榜(API)
    /// </summary>
    public class HotSaleRanking : BaseEntity
    {
        /// <summary>
        /// 商品
        /// </summary>
        public int? ProductId { get; set; }

        /// <summary>
        /// 商品
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 客户Id
        /// </summary>
        public int? TerminalId { get; set; }

        /// <summary>
        /// 业务员
        /// </summary>
        public int? BusinessUserId { get; set; }

        /// <summary>
        /// 品牌
        /// </summary>
        public int? BrandId { get; set; }

        /// <summary>
        /// 商品类别
        /// </summary>
        public int? CategoryId { get; set; }

        /// <summary>
        /// 销售数量
        /// </summary>
        public int TotalSumSaleQuantity { get; set; }

        /// <summary>
        /// 销售数量转换
        /// </summary>
        public string TotalSumSaleQuantityConversion { get; set; }

        /// <summary>
        /// 销售金额
        /// </summary>
        public decimal? TotalSumSaleAmount { get; set; }

        /// <summary>
        /// 退货数量
        /// </summary>
        public int TotalSumReturnQuantity { get; set; }

        /// <summary>
        /// 退货数量转换
        /// </summary>
        public string TotalSumReturnQuantityConversion { get; set; }

        /// <summary>
        /// 退货金额
        /// </summary>
        public decimal? TotalSumReturnAmount { get; set; }

        /// <summary>
        /// 净销售量 = 销售数量 - 退货数量
        /// </summary>
        public int TotalSumNetQuantity { get; set; }

        /// <summary>
        /// 净销售量转换
        /// </summary>
        public string TotalSumNetQuantityConversion { get; set; }

        /// <summary>
        /// 销售净额 = 销售金额 - 退货金额
        /// </summary>
        public decimal? TotalSumNetAmount { get; set; }


        /// <summary>
        /// 退货率 = 退货数量  /  销售数量
        /// </summary>
        public double? ReturnRate { get; set; }

    }


    /// <summary>
    /// 滞销排行榜(API) 销量=零，消退>=0
    /// </summary>
    public class UnSaleRanking
    {
        /// <summary>
        /// 商品
        /// </summary>
        public int? ProductId { get; set; }

        /// <summary>
        /// 商品
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 客户Id
        /// </summary>
        public int? TerminalId { get; set; }

        /// <summary>
        /// 业务员
        /// </summary>
        public int? BusinessUserId { get; set; }

        /// <summary>
        /// 品牌
        /// </summary>
        public int? BrandId { get; set; }

        /// <summary>
        /// 商品类别
        /// </summary>
        public int? CategoryId { get; set; }

        /// <summary>
        /// 销售数量
        /// </summary>
        public int TotalSumSaleQuantity { get; set; }

        /// <summary>
        /// 销售数量转换
        /// </summary>
        public string TotalSumSaleQuantityConversion { get; set; }

        /// <summary>
        /// 销售金额
        /// </summary>
        public decimal? TotalSumSaleAmount { get; set; }


        /// <summary>
        /// 退货数量
        /// </summary>
        public int TotalSumReturnQuantity { get; set; }

        /// <summary>
        /// 销售数量转换
        /// </summary>
        public string TotalSumReturnQuantityConversion { get; set; }

        /// <summary>
        /// 退货金额
        /// </summary>
        public decimal? TotalSumReturnAmount { get; set; }


        /// <summary>
        /// 净销售量 = 销售数量 - 退货数量
        /// </summary>
        public int TotalSumNetQuantity { get; set; }

        /// <summary>
        /// 净销售量转换
        /// </summary>
        public string TotalSumNetQuantityConversion { get; set; }

        /// <summary>
        /// 销售净额 = 销售金额 - 退货金额
        /// </summary>
        public decimal? TotalSumNetAmount { get; set; }

    }

    #endregion

    #region 销量走势图

    /// <summary>
    /// 销量走势图
    /// </summary>
    public class SaleReportSaleQuantityTrend
    {
        /// <summary>
        /// 统计日期
        /// </summary>
        public string GroupDate { get; set; }

        /// <summary>
        /// 销售金额
        /// </summary>
        public decimal? SaleAmount { get; set; }

        /// <summary>
        /// 退货金额
        /// </summary>
        public decimal? ReturnAmount { get; set; }


        /// <summary>
        /// 日期
        /// </summary>
        public string ShowDate { get; set; }

        /// <summary>
        /// 销售净额 = 销售金额 - 退货金额
        /// </summary>
        public decimal? NetAmount { get; set; }

    }
    #endregion

    #region 销售商品成本利润
    /// <summary>
    /// 销售商品成本利润
    /// </summary>
    public class SaleReportProductCostProfit
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
        /// 单位Id（小）
        /// </summary>
        public int? SmallUnitId { get; set; }
        /// <summary>
        /// 单位名称（小）
        /// </summary>
        public string SmallUnitName { get; set; }
        /// <summary>
        /// 单位Id（中）
        /// </summary>
        public int? StrokeUnitId { get; set; }
        /// <summary>
        /// 单位名称（中）
        /// </summary>
        public string StrokeUnitName { get; set; }
        /// <summary>
        /// 单位Id（大）
        /// </summary>
        public int? BigUnitId { get; set; }
        /// <summary>
        /// 单位名称（大）
        /// </summary>
        public string BigUnitName { get; set; }

        /// <summary>
        /// 大转小
        /// </summary>
        public int? BigQuantity { get; set; }
        /// <summary>
        /// 中转小
        /// </summary>
        public int? StrokeQuantity { get; set; }

        /// <summary>
        /// 单位换算
        /// </summary>
        public string UnitConversion { get; set; }

        /// <summary>
        /// 销售数量(小)
        /// </summary>
        public int? SaleSmallQuantity { get; set; }
        /// <summary>
        /// 销售数量(中)
        /// </summary>
        public int? SaleStrokeQuantity { get; set; }
        /// <summary>
        /// 销售数量(大)
        /// </summary>
        public int? SaleBigQuantity { get; set; }
        /// <summary>
        /// 销售数量(数量转换)
        /// </summary>
        public string SaleQuantityConversion { get; set; }

        /// <summary>
        /// 销售金额
        /// </summary>
        public decimal? SaleAmount { get; set; }

        /// <summary>
        /// 退货数量(小)
        /// </summary>
        public int? ReturnSmallQuantity { get; set; }
        /// <summary>
        /// 退货数量(中)
        /// </summary>
        public int? ReturnStrokeQuantity { get; set; }
        /// <summary>
        /// 退货数量(大)
        /// </summary>
        public int? ReturnBigQuantity { get; set; }
        /// <summary>
        /// 退货数量(数量转换)
        /// </summary>
        public string ReturnQuantityConversion { get; set; }

        /// <summary>
        /// 退货金额
        /// </summary>
        public decimal? ReturnAmount { get; set; }

        /// <summary>
        /// 净销售量 = 销售数量 - 退货数量 (小)
        /// </summary>
        public int? NetSmallQuantity { get; set; }
        /// <summary>
        /// 净销售量 = 销售数量 - 退货数量 (中)
        /// </summary>
        public int? NetStrokeQuantity { get; set; }
        /// <summary>
        /// 净销售量 = 销售数量 - 退货数量 (大)
        /// </summary>
        public int? NetBigQuantity { get; set; }
        /// <summary>
        /// 净销售量 = 销售数量 - 退货数量 (数量转换)
        /// </summary>
        public string NetQuantityConversion { get; set; }

        /// <summary>
        /// 销售净额 = 销售金额 - 退货金额
        /// </summary>
        public decimal? NetAmount { get; set; }

        /// <summary>
        /// 成本金额
        /// </summary>
        public decimal? CostAmount { get; set; }

        /// <summary>
        /// 利润
        /// </summary>
        public decimal? Profit { get; set; }

        /// <summary>
        /// 销售利润率
        /// </summary>
        public decimal? SaleProfitRate { get; set; }

        /// <summary>
        /// 成本利润率
        /// </summary>
        public decimal? CostProfitRate { get; set; }


    }

    /// <summary>
    /// 销售商品成本利润(API)
    /// </summary>
    public class CostProfitRanking
    {

        /// <summary>
        /// 商品
        /// </summary>
        public int? ProductId { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 商品类别
        /// </summary>
        public int? CategoryId { get; set; }

        /// <summary>
        /// 品牌
        /// </summary>
        public int? BrandId { get; set; }


        /// <summary>
        /// 客户Id
        /// </summary>
        public int? TerminalId { get; set; }


        /// <summary>
        /// 业务员
        /// </summary>
        public int? BusinessUserId { get; set; }


        /// <summary>
        /// 净销售量
        /// </summary>
        public int? TotalSumNetQuantity { get; set; }

        /// <summary>
        /// 净销售量单位转化 如：xx 箱 xx 瓶
        /// </summary>
        public string TotalSumNetQuantityConversion { get; set; }

        /// <summary>
        /// 销售净额
        /// </summary>
        public decimal? TotalSumNetAmount { get; set; }

        /// <summary>
        /// 利润
        /// </summary>
        public decimal? TotalSumProfit { get; set; }


    }

    #endregion


    /// <summary>
    /// 统计类别
    /// </summary>
    public class SaleReportStatisticalType
    {
        /// <summary>
        /// 统计类别Id
        /// </summary>
        public int StatisticalTypeId { get; set; }

        /// <summary>
        /// 净销售量 = 销售数量 - 退货数量 (最小单位数量)
        /// </summary>
        public int? NetSmallQuantity { get; set; }

        /// <summary>
        /// 销售净额 = 销售金额 - 退货金额
        /// </summary>
        public decimal? NetAmount { get; set; }

        /// <summary>
        /// 成本金额
        /// </summary>
        public decimal? CostAmount { get; set; }

        /// <summary>
        /// 利润
        /// </summary>
        public decimal? Profit { get; set; }

        /// <summary>
        /// 成本利润率
        /// </summary>
        public decimal? CostProfitRate { get; set; }
        /// <summary>
        /// 赠送数
        /// </summary>
        public int? GiftQuantity { get; set; }

    }


    #region 销售额分析

    /// <summary>
    /// 销售额分析（API）
    /// </summary>
    public class SaleAnalysis
    {
        /// <summary>
        /// 业务员
        /// </summary>
        public int BusinessUserId { get; set; }
        public string BusinessUserName { get; set; }

        /// <summary>
        /// 品牌
        /// </summary>
        public int BrandId { get; set; }
        public string BrandName { get; set; }

        /// <summary>
        /// 商品
        /// </summary>
        public int ProductId { get; set; }
        public string ProductName { get; set; }


        /// <summary>
        /// 商品类别
        /// </summary>
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }


        /// <summary>
        /// 今日
        /// </summary>
        public Sale Today { get; set; } = new Sale();

        /// <summary>
        /// 今日上周同期
        /// </summary>
        public Sale LastWeekSame { get; set; } = new Sale();

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

    }

    #endregion

    #region 客户拜访分析

    /// <summary>
    /// 客户拜访分析
    /// </summary>
    public class CustomerVistAnalysis
    {
        /// <summary>
        /// 业务员
        /// </summary>
        public int BusinessUserId { get; set; }
        public string BusinessUserName { get; set; }

        /// <summary>
        /// 总拜访数
        /// </summary>
        public int TotalVist { get; set; }

        /// <summary>
        /// 总客户数
        /// </summary>
        public int TotalCustomer { get; set; }

        /// <summary>
        /// 今日
        /// </summary>
        public Vist Today { get; set; } = new Vist();
        /// <summary>
        /// 今日上周同期
        /// </summary>
        public Vist LastWeekSame { get; set; } = new Vist();
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

    }

    #endregion

    #region 新增客户分析

    /// <summary>
    /// 新增加客户分析
    /// </summary>
    public class NewCustomerAnalysis
    {
        /// <summary>
        /// 业务员
        /// </summary>
        public int BusinessUserId { get; set; }
        public string BusinessUserName { get; set; }

        /// <summary>
        /// 总客户数
        /// </summary>
        public int TotalCustomer { get; set; }

        /// <summary>
        /// 今日
        /// </summary>
        public Signin Today { get; set; } = new Signin();
        /// <summary>
        /// 今日上周同期签约客户
        /// </summary>
        public Signin LastWeekSame { get; set; } = new Signin();
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
        /// 月统计
        /// </summary>
        public Dictionary<string, double> ChartDatas { get; set; } = new Dictionary<string, double>();
    }

    #endregion

    #region 新增订单分析

    /// <summary>
    /// 新增订单分析
    /// </summary>
    public class NewOrderAnalysis
    {
        /// <summary>
        /// 业务员
        /// </summary>
        public int BusinessUserId { get; set; }
        public string BusinessUserName { get; set; }

        /// <summary>
        /// 总订单数
        /// </summary>
        public int TotalOrders { get; set; }

        /// <summary>
        /// 今日
        /// </summary>
        public Order Today { get; set; } = new Order();
        /// <summary>
        /// 今日上周同期签约客户
        /// </summary>
        public Order LastWeekSame { get; set; } = new Order();
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
        /// 月统计
        /// </summary>
        public Dictionary<string, double> ChartDatas { get; set; } = new Dictionary<string, double>();
    }

    #endregion


    #region 业务员综合分析

    public class BusinessAnalysis
    {
        public List<string> UserNames { get; set; } = new List<string>();
        /// <summary>
        /// 拜访数
        /// </summary>
        public List<int> VistCounts { get; set; } = new List<int>();
        /// <summary>
        /// 销单数
        /// </summary>
        public List<int> SaleCounts { get; set; } = new List<int>();
        /// <summary>
        /// 订单数
        /// </summary>
        public List<int> OrderCounts { get; set; } = new List<int>();
        /// <summary>
        /// 新增数
        /// </summary>
        public List<int> NewAddCounts { get; set; } = new List<int>();

    }

    public class BusinessAnalysisQuery
    {
        public string UserName { get; set; }
        /// <summary>
        /// 拜访数
        /// </summary>
        public int VistCount { get; set; }
        /// <summary>
        /// 销单数
        /// </summary>
        public int SaleCount { get; set; }
        /// <summary>
        /// 订单数
        /// </summary>
        public int OrderCount { get; set; }
        /// <summary>
        /// 新增数
        /// </summary>
        public int NewAddCount { get; set; }
    }

    #endregion

    #region 经营日报
    public class SaleReportBusinessDaily
    {
        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }
        /// <summary>
        /// 业务员ID
        /// </summary>
        public int BusinessUserId { get; set; }
        /// <summary>
        /// 单据金额
        /// </summary>
        public decimal SumAmount { get; set; }
        /// <summary>
        /// 成品金额
        /// </summary>
        public decimal SumCostAmount { get; set; }
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
        /// <summary>
        /// 数量
        /// </summary>
        public int SaleQuantity { get; set; }
        /// <summary>
        /// 赠品数量
        /// </summary>
        public int GiftQuantity { get; set; }
        /// <summary>
        /// 赠品成本
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
        /// 单据类型 1、销售单 2、退货单
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public int TypeId { get; set; }

        public int Id { get; set; }
    }
    #endregion

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

    public class Order
    {
        /// <summary>
        /// 拜访数
        /// </summary>
        public int? OrderCount { get; set; } = 0;
        /// <summary>
        /// 新增单据的Id 列表
        /// </summary>
        public List<int> BillIds { get; set; } = new List<int>();
    }
}
