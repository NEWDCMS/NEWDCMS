using DCMS.Core.Domain.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DCMS.Core.Domain.Sales
{

    /// <summary>
    /// 退货单
    /// </summary>
    public class ReturnBill : BaseBill<ReturnItem>
    {
        public ReturnBill()
        {
            BillType = BillTypeEnum.ReturnBill;
        }

        private ICollection<ReturnBillAccounting> _returnBillAccountings;

        /// <summary>
        /// 退货订单Id
        /// </summary>
        public int? ReturnReservationBillId { get; set; } = 0;

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
        /// 支付方式(已收款单据，欠款单据)
        /// </summary>
        public int PaymentMethodType { get; set; }

        /// <summary>
        /// 单据来源(订单转成，非订单转成)
        /// </summary>
        public int BillSourceType { get; set; }

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
        /// 欠款
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
        /// 打印数
        /// </summary>
        public int PrintNum { get; set; }


        /// <summary>
        /// 收款状态
        /// </summary>
        public int ReceiptStatus { get; set; }
        public ReceiptStatus ReceivedStatus
        {
            get { return (ReceiptStatus)ReceiptStatus; }
            set { ReceiptStatus = (int)value; }
        }


        public virtual ReturnReservationBill ReturnReservationBill { get; set; }

        /// <summary>
        /// 操作源
        /// </summary>
        public int? Operation { get; set; } = 0;
        public OperationEnum Operations
        {
            get { return (OperationEnum)Operation; }
            set { Operation = (int)value; }
        }

        ///// <summary>
        ///// (导航)商品类别
        ///// </summary>
        //public virtual ICollection<ReturnItem> ReturnItems
        //{
        //    get { return _returnItems ?? (_returnItems = new List<ReturnItem>()); }
        //    protected set { _returnItems = value; }
        //}

        /// <summary>
        /// (导航)收款账户
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<ReturnBillAccounting> ReturnBillAccountings
        {
            get { return _returnBillAccountings ?? (_returnBillAccountings = new List<ReturnBillAccounting>()); }
            protected set { _returnBillAccountings = value; }
        }

        /// <summary>
        /// 上交状态
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool? HandInStatus { get; set; } = false;

        /// <summary>
        /// 上交时间
        /// </summary>
        public DateTime? HandInDate { get; set; }


        /// <summary>
        /// 记账凭证Id
        /// </summary>
        public int VoucherId { get; set; }

        /// <summary>
        /// 总税额
        /// </summary>
        public decimal TaxAmount { get; set; } = 0;
    }



    /// <summary>
    /// 退货单明细
    /// </summary>
    public class ReturnItem : BaseItem
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
        public int ReturnBillId { get; set; }

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

        public virtual ReturnBill ReturnBill { get; set; }


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
    ///  收款账户（销售单科目映射表）
    /// </summary>
    public class ReturnBillAccounting : BaseAccount
    {
        private int ReturnBillId;

        /// <summary>
        /// 客户
        /// </summary>
        public int TerminalId { get; set; }

        //(导航) 会计科目
        public virtual AccountingOption AccountingOption { get; set; }
        //(导航) 退货单
        public virtual ReturnBill ReturnBill { get; set; }
    }

    /// <summary>
    /// 项目保存或者编辑
    /// </summary>
    public class ReturnBillUpdate : BaseEntity
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
        /// 仓库
        /// </summary>
        public int WareHouseId { get; set; } = 0;

        /// <summary>
        /// 送货员
        /// </summary>
        public int DeliveryUserId { get; set; } = 0;

        /// <summary>
        /// 交易日期
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// 按最小单位销售
        /// </summary>
        public bool IsMinUnitSale { get; set; }

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
        public List<ReturnItem> Items { get; set; }

        /// <summary>
        /// 收款账户
        /// </summary>
        public List<ReturnBillAccounting> Accounting { get; set; }

        /// <summary>
        /// 转订单
        /// </summary>
        public int OrderId { get; set; }
        /// <summary>
        /// 调度信息
        /// </summary>
        public DispatchBill dispatchBill { get; set; }
        /// <summary>
        /// 调度项目信息
        /// </summary>
        public DispatchItem dispatchItem { get; set; }
        /// <summary>
        /// 终端纬度坐标
        /// </summary>
        public double? Latitude { get; set; } = 0;
        /// <summary>
        /// 终端经度坐标
        /// </summary>
        public double? Longitude { get; set; } = 0;

    }

}
