using DCMS.Core.Domain.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DCMS.Core.Domain.Sales
{
    /// <summary>
    /// 退货订单
    /// </summary>
    public class ReturnReservationBill : BaseBill<ReturnReservationItem>
    {
        public ReturnReservationBill()
        {
            BillType = BillTypeEnum.ReturnReservationBill;
        }

        private ICollection<ReturnReservationBillAccounting> _returnReservationBillAccountings;


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


        /// <summary>
        /// 单据总成本价
        /// </summary>
        public decimal SumCostPrice { get; set; }
        /// <summary>
        ///  单据总成本金额
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
        public virtual ICollection<ReturnReservationBillAccounting> ReturnReservationBillAccountings
        {
            get { return _returnReservationBillAccountings ?? (_returnReservationBillAccountings = new List<ReturnReservationBillAccounting>()); }
            protected set { _returnReservationBillAccountings = value; }
        }


        #region 装车调度
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
    }

    /// <summary>
    /// 退货订单明细
    /// </summary>
    public class ReturnReservationItem : BaseItem
    {
        /// <summary>
        /// 税率%
        /// </summary>
        public decimal TaxRate { get; set; }
        /// <summary>
        /// 实际出库数量
        /// </summary>
        public int RealOutQty { get; set; }

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
        /// 退货订单Id
        /// </summary>
        public int ReturnReservationBillId { get; set; }

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

        public virtual ReturnReservationBill ReturnReservationBill { get; set; }


        #endregion

        /// <summary>
        /// 大单位赠送量 2019-07-24
        /// </summary>
        public int? BigGiftQuantity { get; set; } = 0;

        /// <summary>
        /// 小单位赠送量 2019-07-24
        /// </summary>
        public int? SmallGiftQuantity { get; set; } = 0;
    }

    /// <summary>
    ///  收款账户（退货订单单科目映射表）
    /// </summary>
    public class ReturnReservationBillAccounting : BaseAccount
    {
        private int ReturnReservationBillId;

        /// <summary>
        /// 客户
        /// </summary>
        public int TerminalId { get; set; }

        //(导航) 会计科目
        public virtual AccountingOption AccountingOption { get; set; }
        //(导航) 退货订单
        public virtual ReturnReservationBill ReturnReservationBill { get; set; }
    }

    /// <summary>
    /// 项目保存或者编辑
    /// </summary>
    public class ReturnReservationBillUpdate : BaseEntity
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
        /// 待支付金额
        /// </summary>
        public decimal OweCash { get; set; } = 0;

        /// <summary>
        /// 操作源
        /// </summary>
        public int? Operation { get; set; } = 0;

        /// <summary>
        /// 项目
        /// </summary>
        public List<ReturnReservationItem> Items { get; set; }

        /// <summary>
        /// 收款账户
        /// </summary>
        public List<ReturnReservationBillAccounting> Accounting { get; set; }

    }

}
