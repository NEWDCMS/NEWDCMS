using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DCMS.Core.Domain.WareHouses
{
    /// <summary>
    /// 用于表示库存变化汇总
    /// </summary>
    public class StockChangeSummary
    {
        /// <summary>
        /// 经销商Id
        /// </summary>
        public int StoreId { get; set; }
        /// <summary>
        /// 产品Id
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// 商品编码
        /// </summary>
        public string ProductSKU { get; set; }
        /// <summary>
        /// 条形码
        /// </summary>
        public string BarCode { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        public int UnitId { get; set; }
        public int BrandId { get; set; }
        /// <summary>
        /// 单位名称
        /// </summary>
        public string UnitName { get; set; }
        /// <summary>
        /// 单位换算
        /// </summary>
        public string UnitConversion { get; set; }
        public decimal Price { get; set; }
        public string PriceName { get; set; }

        public int SmallUnitId { get; set; } = 0;
        public string SmallUnitName { get; set; }
        public int? StrokeUnitId { get; set; } = 0;
        public string StrokeUnitName { get; set; }
        public int BigUnitId { get; set; } = 0;
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
        /// 期初数量
        /// </summary>
        public int InitialQuantity { get; set; }
        public string InitialQuantityName { get; set; }

        /// <summary>
        /// 期初金额
        /// </summary>
        public decimal InitialAmount { get; set; }

        /// <summary>
        /// 期末数量
        /// </summary>
        public int EndQuantity { get; set; }
        public string EndQuantityName { get; set; }


        /// <summary>
        /// 期末金额
        /// </summary>
        public decimal EndAmount { get; set; }


        /// <summary>
        /// 本期采购
        /// </summary>
        public int CurrentPurchaseQuantity { get; set; }
        /// <summary>
        /// 本期退购
        /// </summary>
        public int CurrentReturnQuantity { get; set; }
        /// <summary>
        /// 本期调入
        /// </summary>
        public int CurrentAllocationInQuantity { get; set; }
        /// <summary>
        /// 本期调出
        /// </summary>
        public int CurrentAllocationOutQuantity { get; set; }
        /// <summary>
        /// 本期销售
        /// </summary>
        public int CurrentSaleQuantity { get; set; }
        /// <summary>
        /// 本期退售
        /// </summary>
        public int CurrentSaleReturnQuantity { get; set; }
        /// <summary>
        /// 本期组合
        /// </summary>
        public int CurrentCombinationQuantity { get; set; }
        /// <summary>
        /// 本期拆分
        /// </summary>
        public int CurrentSplitReturnQuantity { get; set; }

        /// <summary>
        /// 本期报损
        /// </summary>
        public int CurrentWasteQuantity { get; set; }
        /// <summary>
        /// 本期盘盈
        /// </summary>
        public int CurrentVolumeQuantity { get; set; }
        /// <summary>
        /// 本期盘亏
        /// </summary>
        public int CurrentLossesQuantity { get; set; }


        //==========


        /// <summary>
        /// 本期采购
        /// </summary>
        public string CurrentPurchaseQuantityName { get; set; }
        /// <summary>
        /// 本期退购
        /// </summary>
        public string CurrentReturnQuantityName { get; set; }
        /// <summary>
        /// 本期调入
        /// </summary>
        public string CurrentAllocationInQuantityName { get; set; }
        /// <summary>
        /// 本期调出
        /// </summary>
        public string CurrentAllocationOutQuantityName { get; set; }
        /// <summary>
        /// 本期销售
        /// </summary>
        public string CurrentSaleQuantityName { get; set; }
        /// <summary>
        /// 本期退售
        /// </summary>
        public string CurrentSaleReturnQuantityName { get; set; }
        /// <summary>
        /// 本期组合
        /// </summary>
        public string CurrentCombinationQuantityName { get; set; }
        /// <summary>
        /// 本期拆分
        /// </summary>
        public string CurrentSplitReturnQuantityName { get; set; }
        /// <summary>
        /// 本期报损
        /// </summary>
        public string CurrentWasteQuantityName { get; set; }
        /// <summary>
        /// 本期盘盈
        /// </summary>
        public string CurrentVolumeQuantityName { get; set; }
        /// <summary>
        /// 本期盘亏
        /// </summary>
        public string CurrentLossesQuantityName { get; set; }
        /// <summary>
        /// 赠送数量
        /// </summary>
        public int? GiftQuantity { get; set; } = 0;
        /// <summary>
        /// 赠送金额
        /// </summary>
        [NotMapped]
        public decimal GiftAmount { get; set; } = 0;
        /// <summary>
        /// 本期盘盈金额
        /// </summary>
        [NotMapped]
        public decimal CurrentVolumeAmount { get; set; } = 0;
        /// <summary>
        /// 本期盘亏金额
        /// </summary>
        [NotMapped]
        public decimal CurrentLossesAmount { get; set; } = 0;
    }

    /// <summary>
    /// 用于表示库存变化汇总
    /// </summary>
    public class StockChangeSummaryOrder
    {

        /// <summary>
        /// 经销商Id
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// 产品Id
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// 商品编码
        /// </summary>
        public string ProductSKU { get; set; }
        /// <summary>
        /// 条形码
        /// </summary>
        public string BarCode { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        public int UnitId { get; set; }
        /// <summary>
        /// 单位名称
        /// </summary>
        public string UnitName { get; set; }

        public int BrandId { get; set; }

        /// <summary>
        /// 商品类别
        /// </summary>
        public int CategoryId { get; set; }


        /// <summary>
        /// 单位换算
        /// </summary>
        public string UnitConversion { get; set; }
        public decimal Price { get; set; }
        public string PriceName { get; set; }

        public int? SmallUnitId { get; set; }
        public int? StrokeUnitId { get; set; }
        public int? BigUnitId { get; set; }

        /// <summary>
        /// 可用改变量
        /// </summary>
        public int UsableQuantityChange { get; set; }

        /// <summary>
        /// 可用改变量（数量转换）
        /// </summary>
        public string UsableQuantityChangeConversion { get; set; }

        /// <summary>
        /// 当前可用
        /// </summary>
        public int UsableQuantityAfter { get; set; }

        /// <summary>
        /// 当前可用（数量转换）
        /// </summary>
        public string UsableQuantityAfterConversion { get; set; }

        /// <summary>
        /// 流水
        /// </summary>
        public int StockFlowId { get; set; }

        /// <summary>
        /// 出入库
        /// </summary>
        public int StockInOutRecordId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }


        /// <summary>
        /// 单据类型
        /// </summary>
        public int BillType { get; set; }
        /// <summary>
        /// 单据类型
        /// </summary>
        public string BillTypeName { get; set; }

        /// <summary>
        /// 单据编号
        /// </summary>
        public string BillCode { get; set; }

        public string LinkUrl { get; set; }

        public int Direction { get; set; }

        public int BillId { get; set; }

        /// <summary>
        /// 客户/供应商
        /// </summary>
        public string CustomerSupplier { get; set; }

        /// <summary>
        /// 交易时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// 审核时间
        /// </summary>
        public DateTime? AuditedDate { get; set; }

        /// <summary>
        /// 库存
        /// </summary>
        public int WareHouseId { get; set; }
        public string WareHouseName { get; set; }

        public bool Difference { get; set; }
        public Tuple<int, int, int> UsableQuantityChangePart { get; set; } = new Tuple<int, int, int>(0, 0, 0);
        public Tuple<int, int, int> UsableQuantityChangeAfterPart { get; set; } = new Tuple<int, int, int>(0, 0, 0);
    }

    /// <summary>
    /// 用于表示门店库存上报表 delete
    /// </summary>
    public class StockReportOrderList
    {

        /// <summary>
        /// 客户
        /// </summary>
        public int TerminalId { get; set; }
        public string TerminalName { get; set; }

        /// <summary>
        /// 业务员
        /// </summary>
        public int BusinessUserId { get; set; }
        public string BusinessUserName { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        /// <summary>
        /// 商品编码
        /// </summary>
        public string ProductSKU { get; set; }
        /// <summary>
        /// 条形码
        /// </summary>
        public string BarCode { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        public int UnitId { get; set; }
        public int BrandId { get; set; }
        /// <summary>
        /// 单位名称
        /// </summary>
        public string UnitName { get; set; }
        /// <summary>
        /// 单位换算
        /// </summary>
        public string UnitConversion { get; set; }
        public decimal Price { get; set; }
        public string PriceName { get; set; }

        public int SmallUnitId { get; set; }
        public int StrokeUnitId { get; set; }
        public int BigUnitId { get; set; }

        /// <summary>
        /// 经销商Id
        /// </summary>
        //移除

        /// <summary>
        /// 片区
        /// </summary>

        public int DistrictId { get; set; }


        /// <summary>
        /// 期初数量及时间
        /// </summary>
        public double? BeginQuantity { get; set; }
        public string BeginQuantityConversion { get; set; }
        public DateTime BeginTime { get; set; }

        /// <summary>
        /// 采购量
        /// </summary>
        public double? PurchaseQuantity { get; set; }
        public string PurchaseQuantityConversion { get; set; }

        /// <summary>
        /// 期末数量及时间
        /// </summary>
        public double? EndQuantity { get; set; }
        public string EndQuantityConversion { get; set; }
        public DateTime? EndTime { get; set; }


        /// <summary>
        /// 销售量
        /// </summary>
        public double? SaleQuantity { get; set; }
        public string SaleQuantityEndQuantity { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

    }

    /// <summary>
    /// 用于表示门店库存上报表
    /// </summary>
    public class InventoryReportList
    {
        /// <summary>
        /// 经销商
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// 客户
        /// </summary>
        public int TerminalId { get; set; }
        public string TerminalName { get; set; }

        /// <summary>
        /// 业务员
        /// </summary>
        public int BusinessUserId { get; set; }
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
        /// 期初数量
        /// </summary>
        public int BeginStoreQuantity { get; set; }
        public string BeginStoreQuantityConversion { get; set; }
        /// <summary>
        /// 期初时间
        /// </summary>
        public DateTime BeginDate { get; set; }

        /// <summary>
        /// 期末数量
        /// </summary>
        public int EndStoreQuantity { get; set; }
        public string EndStoreQuantityConversion { get; set; }
        /// <summary>
        /// 期末时间
        /// </summary>
        public DateTime? EndDate { get; set; }


        /// <summary>
        /// 采购量
        /// </summary>
        public int PurchaseQuantity { get; set; }
        public string PurchaseQuantityConversion { get; set; }

        /// <summary>
        /// 销售量
        /// </summary>
        public int SaleQuantity { get; set; }
        public string SaleQuantityConversion { get; set; }

        public DateTime ManufactureDete { get; set; }
    }

    /// <summary>
    /// 用于表示调拨明细
    /// </summary>
    public class AllocationDetailsList
    {
        public int Id { get; set; }

        /// <summary>
        /// 单据编号
        /// </summary>
        public string BillNumber { get; set; }
        public string LinkUrl { get; set; }
        
        /// <summary>
        /// 出货仓库
        /// </summary>
        public int ShipmentWareHouseId { get; set; }
        public string ShipmentWareHouseName { get; set; }

        /// <summary>
        /// 入货仓库
        /// </summary>
        public int IncomeWareHouseId { get; set; }
        public string IncomeWareHouseName { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        /// <summary>
        /// 商品编码
        /// </summary>
        public string ProductSKU { get; set; }
        /// <summary>
        /// 条形码
        /// </summary>
        public string BarCode { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        public int UnitId { get; set; }
        public int BrandId { get; set; }
        /// <summary>
        /// 单位名称
        /// </summary>
        public string UnitName { get; set; }
        /// <summary>
        /// 单位换算
        /// </summary>
        public string UnitConversion { get; set; }
        public decimal Price { get; set; }
        public string PriceName { get; set; }

        /// <summary>
        /// 经销商Id
        /// </summary>
        //移除

        /// <summary>
        /// 审核时间
        /// </summary>

        public DateTime? AuditedDate { get; set; }
        /// <summary>
        /// 审核人
        /// </summary>
        public int? AuditedUserId { get; set; }
        /// <summary>
        /// 审核状态
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool AuditedStatus { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 数量转换
        /// </summary>
        public string QuantityConversion { get; set; }

        /// <summary>
        /// 调拨日期
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

    }


    #region 库存滞销报表

    /// <summary>
    /// 库存滞销报表
    /// </summary>
    public class StockUnsalable
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
        /// 库存数
        /// </summary>
        public int StockQuantity { get; set; }

        /// <summary>
        /// 库存数量转换
        /// </summary>
        public string StockQuantityConversion { get; set; }

        /// <summary>
        /// 销售数量
        /// </summary>
        public int SaleQuantity { get; set; }

        /// <summary>
        /// 数量转换
        /// </summary>
        public string SaleQuantityConversion { get; set; }

        /// <summary>
        /// 退货数量
        /// </summary>
        public int ReturnQuantity { get; set; }

        /// <summary>
        /// 数量转换
        /// </summary>
        public string ReturnQuantityConversion { get; set; }

        /// <summary>
        /// 净销数量
        /// </summary>
        public int NetQuantity { get; set; }

        /// <summary>
        /// 数量转换
        /// </summary>
        public string NetQuantityConversion { get; set; }

        /// <summary>
        /// 销售金额
        /// </summary>
        public decimal? SaleAmount { get; set; }

        /// <summary>
        /// 退货金额
        /// </summary>
        public decimal? ReturnAmount { get; set; }

        /// <summary>
        /// 销售净额
        /// </summary>
        public decimal? NetAmount { get; set; }
    }

    #endregion

    #region 库存预警表
    /// <summary>
    /// 库存预警表
    /// </summary>
    public class EarlyWarning
    {

        public int StoreId { get; set; }
        public int? WareHouseId { get; set; }
        public int? ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }

        public string SmallBarCode { get; set; }
        public string StrokeBarCode { get; set; }
        public string BigBarCode { get; set; }

        public string UnitConversion { get; set; }

        public int StrokeQuantity { get; set; }
        public int BigQuantity { get; set; }

        public string SmallUnitName { get; set; }
        public string StrokeUnitName { get; set; }
        public string BigUnitName { get; set; }

        public int StockQuantity { get; set; }
        public string StockQuantityConversion { get; set; }


        public int LessQuantity { get; set; }
        public string LessQuantityConversion { get; set; }

        public int MoreQuantity { get; set; }
        public string MoreQuantityConversion { get; set; }

    }

    #endregion

    #region 临期预警表
    public class ExpirationWarning
    {
        /// <summary>
        /// 仓库ID
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

        public int StrokeQuantity { get; set; }
        public int BigQuantity { get; set; }

        public string SmallUnitName { get; set; }
        public string StrokeUnitName { get; set; }
        public string BigUnitName { get; set; }

        /// <summary>
        /// 单位换算
        /// </summary>
        public string UnitConversion { get; set; }

        /// <summary>
        /// 保质期（天）
        /// </summary>
        public int? ExpirationDays { get; set; }

        /// <summary>
        /// 临期预警（天）
        /// </summary>
        public int? AdventDays { get; set; } = 0;

        /// <summary>
        /// 库存数量
        /// </summary>
        public int StockQuantity { get; set; }


        /// <summary>
        /// 1/3 保质期（天）
        /// </summary>
        public int OneThirdDay { get; set; }
        /// <summary>
        /// 2/3 保质期（天）
        /// </summary>
        public int TwoThirdDay { get; set; }

        /// <summary>
        /// 1/3 库存量
        /// </summary>
        public int OneThirdQuantity { get; set; }
        public string OneThirdQuantityUnitConversion { get; set; }

        /// <summary>
        /// 2/3 库存量
        /// </summary>
        public int TwoThirdQuantity { get; set; }
        public string TwoThirdQuantityUnitConversion { get; set; }

        /// <summary>
        /// 预警量
        /// </summary>
        public int WarningQuantity { get; set; }
        public string WarningQuantityUnitConversion { get; set; }


        /// <summary>
        /// 过期量
        /// </summary>
        public int ExpiredQuantity { get; set; }
        public string ExpiredQuantityUnitConversion { get; set; }


        public int OneThird_BTotalQuantity { get; set; }
        public int OneThird_STotalQuantity { get; set; }
        public int OneThird_MTotalQuantity { get; set; }


        public int TwoThird_BTotalQuantity { get; set; }
        public int TwoThird_STotalQuantity { get; set; }
        public int TwoThird_MTotalQuantity { get; set; }


        public int Expired_BTotalQuantity { get; set; }
        public int Expired_STotalQuantity { get; set; }
        public int Expired_MTotalQuantity { get; set; }


        public int Warning_BTotalQuantity { get; set; }
        public int Warning_STotalQuantity { get; set; }
        public int Warning_MTotalQuantity { get; set; }
    }
    #endregion

    public class StockReportProduct
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
        /// 大单位转化量
        /// </summary>
        public int? BigQuantity { get; set; }
        /// <summary>
        /// 中单位转化量
        /// </summary>
        public int? StrokeQuantity { get; set; }

        /// <summary>
        /// 单位换算
        /// </summary>
        public string UnitConversion { get; set; }

        /// <summary>
        /// 商品类别Id
        /// </summary>
        public int CategoryId { get; set; }
        /// <summary>
        /// 商品类别名称
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// 商品品牌Id
        /// </summary>
        public int BrandId { get; set; }
        /// <summary>
        /// 商品品牌名称
        /// </summary>
        public string BrandName { get; set; }

        public int WareHouseId { get; set; }

        /// <summary>
        /// 现货库存数量
        /// </summary>
        public int CurrentQuantity { get; set; }
        /// <summary>
        /// 现货库存数量(数量转换)
        /// </summary>
        public string CurrentQuantityConversion { get; set; }
        public Tuple<int, int, int> CurrentQuantityPart { get; set; } = new Tuple<int, int, int>(0, 0, 0);


        /// <summary>
        /// 可用库存数量
        /// </summary>
        public int UsableQuantity { get; set; }
        /// <summary>
        /// 可用库存数量(数量转换)
        /// </summary>
        public string UsableQuantityConversion { get; set; }
        public Tuple<int, int, int> UsableQuantityPart { get; set; } = new Tuple<int, int, int>(0, 0, 0);


        /// <summary>
        /// 预占库存数量
        /// </summary>
        public int OrderQuantity { get; set; }
        /// <summary>
        /// 预占库存数量(数量转换)
        /// </summary>
        public string OrderQuantityConversion { get; set; }

        public Tuple<int, int, int> OrderQuantityPart { get; set; } = new Tuple<int, int, int>(0, 0, 0);

        /// <summary>
        /// 成本单价(元)
        /// </summary>
        public decimal CostPrice { get; set; }
        /// <summary>
        /// 成本金额(元)
        /// </summary>
        public decimal CostAmount { get; set; }

        /// <summary>
        /// 批发单价(元)
        /// </summary>
        public decimal TradePrice { get; set; }
        /// <summary>
        /// 批发金额(元)
        /// </summary>
        public decimal TradeAmount { get; set; }



        public class UnitQuantityPart
        {
            /// <summary>
            /// 小单位量
            /// </summary>
            public int Small { get; set; }
            /// <summary>
            /// 中单位量
            /// </summary>
            public int Stroke { get; set; }
            /// <summary>
            /// 大单位量
            /// </summary>
            public int Big { get; set; }

        }
    }

}
