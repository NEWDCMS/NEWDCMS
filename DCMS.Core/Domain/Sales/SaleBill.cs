using DCMS.Core.Domain.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DCMS.Core.Domain.Sales
{

	/// <summary>
	/// 销售单
	/// </summary>
	public class SaleBill : BaseBill<SaleItem>
	{
		public SaleBill()
		{
			BillType = BillTypeEnum.SaleBill;
		}


		private ICollection<SaleBillAccounting> _saleBillAccountings;

		/// <summary>
		/// 销售订单Id
		/// </summary>
		public int? SaleReservationBillId { get; set; } = 0;

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
		/// 欠款金额
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



		/// <summary>
		/// 操作源
		/// </summary>
		public int? Operation { get; set; } = 0;
		public OperationEnum Operations
		{
			get { return (OperationEnum)Operation; }
			set { Operation = (int)value; }
		}

		public virtual SaleReservationBill SaleReservationBill { get; set; }


		/// <summary>
		/// (导航)收款账户
		/// </summary>
		[JsonIgnore]
		public virtual ICollection<SaleBillAccounting> SaleBillAccountings
		{
			get { return _saleBillAccountings ?? (_saleBillAccountings = new List<SaleBillAccounting>()); }
			protected set { _saleBillAccountings = value; }
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
		/// 入库凭证（入库记账凭证Id）
		/// </summary>
		public int VoucherId { get; set; }


		/// <summary>
		/// 总税额
		/// </summary>
		public decimal TaxAmount { get; set; } = 0;


		/// <summary>
		/// 签收状态：0待签收，1已签收，2拒收
		/// </summary>
		public int SignStatus { get; set; }



		//public void AddSaleBillAccounting(SaleBillAccounting sba)
		//{
		//	SaleBillAccountings.Add(sba);
		//	_saleBillAccountings = null;
		//}

  //      public void SetSaleBillAccounting(ICollection<SaleBillAccounting> sbas)
  //      {
  //          SaleBillAccountings = sbas;
  //          _saleBillAccountings = null;
  //      }

	}

	/// <summary>
	/// 销售单明细
	/// </summary>
	public class SaleItem : BaseItem
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
		/// 销售单Id
		/// </summary>
		public int SaleBillId { get; set; }

		/// <summary>
		/// 生产日期
		/// </summary>
		public DateTime? ManufactureDete { get; set; }

		/// <summary>
		/// 是否口味商品
		/// </summary>
		[Column(TypeName = "BIT(1)")]
		public bool IsFlavorProduct { get; set; } = false;

		/// <summary>
		/// 商品批次
		/// </summary>
		public string ProductionBatch { get; set; }
		/// <summary>
		/// ERP同步库存商品关联Id
		/// </summary>
		public int DealerStockId { get; set; }

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


		#region 导航

		public virtual SaleBill SaleBill { get; set; }

		#endregion

	}


	/// <summary>
	///  收款账户（销售单科目映射表）
	/// </summary>
	public class SaleBillAccounting : BaseAccount
	{
		private int SaleBillId;
		/// <summary>
		/// 客户
		/// </summary>
		public int TerminalId { get; set; }


		//(导航) 会计科目
		public virtual AccountingOption AccountingOption { get; set; }
		//(导航) 销售单
		public virtual SaleBill SaleBill { get; set; }
	}


	/// <summary>
	/// 项目保存或者编辑
	/// </summary>
	public class SaleBillUpdate : BaseEntity
	{
		public string BillNumber { get; set; }
		/// <summary>
		/// 客户
		/// </summary>
		public int TerminalId { get; set; } = 0;

		/// <summary>
		/// 收款人
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
		public List<SaleItem> Items { get; set; }

		/// <summary>
		/// 收款账户
		/// </summary>
		public List<SaleBillAccounting> Accounting { get; set; }


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
