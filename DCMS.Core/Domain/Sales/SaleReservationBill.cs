using DCMS.Core.Domain.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DCMS.Core.Domain.Sales
{
    /// <summary>
    /// 销售订单
    /// </summary>
    public class SaleReservationBill : BaseBill<SaleReservationItem>
    {

        public SaleReservationBill()
        {
            BillType = BillTypeEnum.SaleReservationBill;
        }

        // private ICollection<SaleReservationItem> _saleReservationItems;
        private ICollection<SaleReservationBillAccounting> _saleReservationBillAccountings;

        /// <summary>
        /// 客户Id
        /// </summary>
        public int TerminalId { get; set; }

        /// <summary>
        /// 业务员
        /// </summary>
        public int BusinessUserId { get; set; }

        /// <summary>
        /// 送货员
        /// </summary>
        public int DeliveryUserId { get; set; }

        /// <summary>
        /// 部门
        /// </summary>
        public int DepartmentId { get; set; }

        /// <summary>
        /// 片区Id
        /// </summary>
        public int DistrictId { get; set; }

        /// <summary>
        /// 仓库
        /// </summary>
        public int WareHouseId { get; set; }

        /// <summary>
        /// 付款方式
        /// </summary>
        public int PayTypeId { get; set; }

        /// <summary>
        /// 交易日期
        /// </summary>
        public DateTime? TransactionDate { get; set; }

        /// <summary>
        /// 按最小单位销售
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool IsMinUnitSale { get; set; }

        /// <summary>
        /// 默认售价
        /// </summary>
        public string DefaultAmountId { get; set; }

        /// <summary>
        /// 总金额
        /// </summary>
        public decimal SumAmount { get; set; }

        /// <summary>
        /// 应收金额
        /// </summary>
        public decimal ReceivableAmount { get; set; }

        /// <summary>
        /// 优惠金额
        /// </summary>
        public decimal PreferentialAmount { get; set; }

        /// <summary>
        /// 待支付金额
        /// </summary>
        public decimal OweCash { get; set; }


        //(注意：结转成本后，已审核业务单据中的成本价，将会被替换成结转后的全月平均价!)
        /// <summary>
        /// 成本价
        /// </summary>
        public decimal SumCostPrice { get; set; }
        /// <summary>
        /// 成本金额
        /// </summary>
        public decimal SumCostAmount { get; set; }

        /// <summary>
        /// 利润
        /// </summary>
        public decimal SumProfit { get; set; }

        /// <summary>
        /// 成本利润率
        /// </summary>
        public decimal SumCostProfitRate { get; set; }


        /// <summary>
        /// 调度人
        /// </summary>
        public int? DispatchedUserId { get; set; } = 0;

        /// <summary>
        /// 调度状态
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool DispatchedStatus { get; set; }

        /// <summary>
        /// 调度时间
        /// </summary>
        public DateTime? DispatchedDate { get; set; }

        /// <summary>
        /// 转单人
        /// </summary>
        public int? ChangedUserId { get; set; } = 0;

        /// <summary>
        /// 转单状态
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool ChangedStatus { get; set; }

        /// <summary>
        /// 转单时间
        /// </summary>
        public DateTime? ChangedDate { get; set; }

        /// <summary>
        /// 打印数
        /// </summary>
        public int PrintNum { get; set; }


        /// <summary>
        /// 是否已经开具收款单（已收账）
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Receipted { get; set; }

        /// <summary>
        /// 操作源
        /// </summary>
        public int? Operation { get; set; } = 0;
        public OperationEnum Operations
        {
            get { return (OperationEnum)Operation; }
            set { Operation = (int)value; }
        }


        /// <summary>
        /// (导航)收款账户
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<SaleReservationBillAccounting> SaleReservationBillAccountings
        {
            get { return _saleReservationBillAccountings ?? (_saleReservationBillAccountings = new List<SaleReservationBillAccounting>()); }
            protected set { _saleReservationBillAccountings = value; }
        }

        #region 仓库分拣、装车调度
        /// <summary>
        /// 整箱拆零状态
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool? PickingWholeScrapStatus { get; set; }
        /// <summary>
        /// 整箱拆零打印数量
        /// </summary>
        public int? PickingWholeScrapPrintNum { get; set; } = 0;
        /// <summary>
        /// 整箱拆零打印时间
        /// </summary>
        public DateTime? PickingWholeScrapPrintDate { get; set; }

        /// <summary>
        /// 整箱状态
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool? PickingWholeStatus { get; set; }
        /// <summary>
        /// 整箱打印数量
        /// </summary>
        public int? PickingWholePrintNum { get; set; } = 0;
        /// <summary>
        /// 整箱打印时间
        /// </summary>
        public DateTime? PickingWholePrintDate { get; set; }

        /// <summary>
        /// 拆零状态
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool? PickingScrapStatus { get; set; }
        /// <summary>
        /// 拆零打印数量
        /// </summary>
        public int? PickingScrapPrintNum { get; set; } = 0;
        /// <summary>
        /// 拆零打印时间
        /// </summary>
        public DateTime? PickingScrapPrintDate { get; set; }
        #endregion


        /// <summary>
        /// 总税额
        /// </summary>
        public decimal TaxAmount { get; set; } = 0;

        public DateTime DeliverDate { get; set; }
        public string AMTimeRange { get; set; }
        public string PMTimeRange { get; set; }
    }
    /// <summary>
    /// 销售订单明细
    /// </summary>
    public class SaleReservationItem : BaseItem
    {
        /// <summary>
        /// 税率%
        /// </summary>
        public decimal TaxRate { get; set; }


        /// <summary>
        /// 库存数量
        /// </summary>
        public int StockQty { get; set; }

        /// <summary>
        /// 剩余还款数量
        /// </summary>
        public int RemainderQty { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// 销售订单Id
        /// </summary>
        public int SaleReservationBillId { get; set; }

        /// <summary>
        /// 生产日期
        /// </summary>
        public DateTime? ManufactureDete { get; set; }

        /// <summary>
        /// 是否口味商品
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool IsFlavorProduct { get; set; } = false;

        #region 导航

        public virtual SaleReservationBill SaleReservationBill { get; set; }


        #endregion

        /// <summary>
        /// 大单位赠送量 2019-07-24
        /// </summary>
        public int? BigGiftQuantity { get; set; } = 0;

        /// <summary>
        /// 小单位赠送量 2019-07-24
        /// </summary>
        public int? SmallGiftQuantity { get; set; } = 0;

        #region 赠品信息

        /// <summary>
        /// 销售商品类型 关联 SaleProductTypeEnum 枚举
        /// </summary>
        public int? SaleProductTypeId { get; set; } = 0;

        /// <summary>
        /// 赠品类型 关联 GiveTypeEnum 枚举
        /// </summary>
        public int? GiveTypeId { get; set; } = 0;

        /// <summary>
        /// 促销活动Id
        /// </summary>
        public int? CampaignId { get; set; } = 0;

        /// <summary>
        /// 促销活动购买Id
        /// </summary>
        public int? CampaignBuyProductId { get; set; } = 0;

        /// <summary>
        /// 促销活动赠送Id
        /// </summary>
        public int? CampaignGiveProductId { get; set; } = 0;

        /// <summary>
        /// 销售赠送关联号
        /// </summary>
        public string CampaignLinkNumber { get; set; }

        /// <summary>
        /// 费用合同Id
        /// </summary>
        public int? CostContractId { get; set; } = 0;

        /// <summary>
        /// 费用合同明细Id
        /// </summary>
        public int? CostContractItemId { get; set; } = 0;

        /// <summary>
        /// 费用合同使用几月份，具体使用几月份，扣除几月份数据
        /// </summary>
        public int? CostContractMonth { get; set; } = 0;

        #endregion

        /// <summary>
        /// 备注类型
        /// </summary>
        public int RemarkConfigId { get; set; }


    }
    /// <summary>
    ///  收款账户（销售订单科目映射表）
    /// </summary>
    public class SaleReservationBillAccounting : BaseAccount
    {
        #region Fields
        private int SaleReservationBillId;
        #endregion

        /// <summary>
        /// 客户
        /// </summary>
        public int TerminalId { get; set; }

        //(导航) 会计科目
        public virtual AccountingOption AccountingOption { get; set; }
        //(导航) 销售订单
        public virtual SaleReservationBill SaleReservationBill { get; set; }
    }

    public class SaleReservationBillUpdate : BaseEntity
    {
        public string BillNumber { get; set; }
        /// <summary>
        /// 客户
        /// </summary>
        public int TerminalId { get; set; } = 0;

        /// <summary>
        /// 业务员
        /// </summary>
        public int BusinessUserId { get; set; } = 0;

        /// <summary>
        /// 送货员
        /// </summary>
        public int DeliveryUserId { get; set; } = 0;

        /// <summary>
        /// 仓库
        /// </summary>
        public int WareHouseId { get; set; } = 0;

        /// <summary>
        /// 付款方式
        /// </summary>
        public int PayTypeId { get; set; } = 0;

        /// <summary>
        /// 交易日期
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// 按最小单位销售
        /// </summary>
        public bool IsMinUnitSale { get; set; }

        ///// <summary>
        ///// 送货员
        ///// </summary>
        //public int DeliveryUserId { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 价格体系
        /// </summary>
        public string DefaultAmountId { get; set; }

        /// <summary>
        /// 优惠金额
        /// </summary>
        public decimal PreferentialAmount { get; set; } = 0;
        /// <summary>
        /// 优惠后金额
        /// </summary>
        public decimal PreferentialEndAmount { get; set; } = 0;

        /// <summary>
        /// 欠款金额
        /// </summary>
        public decimal OweCash { get; set; } = 0;

        /// <summary>
        /// 操作源
        /// </summary>
        public int? Operation { get; set; } = 0;

        /// <summary>
        /// 项目
        /// </summary>
        public List<SaleReservationItem> Items { get; set; }

        /// <summary>
        /// 收款账户
        /// </summary>
        public List<SaleReservationBillAccounting> Accounting { get; set; }

        /// <summary>
        /// 配送时间
        /// </summary>
        public DateTime DeliverDate { get; set; }
        public string AMTimeRange { get; set; }
        public string PMTimeRange { get; set; }
    }

}
