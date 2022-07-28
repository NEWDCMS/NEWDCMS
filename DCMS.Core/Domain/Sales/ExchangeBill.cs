using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DCMS.Core.Domain.Sales
{
    /// <summary>
    /// 换货单
    /// </summary>
    public class ExchangeBill : BaseBill<ExchangeItem>
    {

        public ExchangeBill()
        {
            BillType = BillTypeEnum.ExchangeBill;
        }

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
        /// 操作源
        /// </summary>
        public int? Operation { get; set; } = 0;
        public OperationEnum Operations
        {
            get { return (OperationEnum)Operation; }
            set { Operation = (int)value; }
        }

        /// <summary>
        /// 配送时间
        /// </summary>
        public DateTime DeliverDate { get; set; }
        public string AMTimeRange { get; set; }
        public string PMTimeRange { get; set; }

        /// <summary>
        /// 销售订单Id
        /// </summary>
        public int SaleReservationBillId { get; set; } = 0;
        public string SaleReservationBillNumber { get; set; }
    }
    /// <summary>
    /// 换货单明细
    /// </summary>
    public class ExchangeItem : BaseItem
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

        public int SaleReservationBillId { get; set; }

        public int ExchangeBillId { get; set; }

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

        public virtual ExchangeBill ExchangeBill { get; set; }


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

    }

    /// <summary>
    /// 更新结构
    /// </summary>
    public class ExchangeBillUpdate : BaseEntity
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
        public List<ExchangeItem> Items { get; set; }

        /// <summary>
        /// 配送时间
        /// </summary>
        public DateTime DeliverDate { get; set; }




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
