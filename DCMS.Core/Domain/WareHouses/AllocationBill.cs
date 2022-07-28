using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


namespace DCMS.Core.Domain.WareHouses
{

    /// <summary>
    /// 用于表示调拨单
    /// </summary>
    public class AllocationBill : BaseBill<AllocationItem>
    {
        /// <summary>
        /// 调拨单
        /// </summary>
        public AllocationBill()
        {
            BillType = BillTypeEnum.AllocationBill;
        }

        // private ICollection<AllocationItem> _allocationItems;


        /// <summary>
        /// 出货仓库
        /// </summary>
        public int ShipmentWareHouseId { get; set; }

        /// <summary>
        /// 入货仓库
        /// </summary>
        public int IncomeWareHouseId { get; set; }

        /// <summary>
        /// 调度单Id
        /// </summary>
        public int? DispatchBillId { get; set; }

        /// <summary>
        /// 调度单编号
        /// </summary>
        public string DispatchBillNumber { get; set; }

        /// <summary>
        /// 调拨日期
        /// </summary>
        //public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// 是否按最小单位调拨
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool AllocationByMinUnit { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        //public string Remark { get; set; }

        ///// <summary>
        ///// 制单人
        ///// </summary>
        //public int MakeUserId { get; set; }

        ///// <summary>
        ///// 审核人
        ///// </summary>
        //public int? AuditedUserId { get; set; }
        ///// <summary>
        ///// 状态(审核)
        ///// </summary>
        //[Column(TypeName = "BIT(1)")]
        //public bool AuditedStatus { get; set; }

        ///// <summary>
        ///// 审核时间
        ///// </summary>
        //public DateTime? AuditedDate { get; set; }

        ///// <summary>
        ///// 红冲人
        ///// </summary>
        //public int? ReversedUserId { get; set; }

        ///// <summary>
        ///// 红冲状态
        ///// </summary>
        //[Column(TypeName = "BIT(1)")]
        //public bool ReversedStatus { get; set; }

        ///// <summary>
        ///// 红冲时间
        ///// </summary>
        //public DateTime? ReversedDate { get; set; }

        /// <summary>
        /// 打印数
        /// </summary>
        public int? PrintNum { get; set; } = 0;


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
        ///// (导航)调拨单项目
        ///// </summary>
        //public virtual ICollection<AllocationItem> AllocationItems
        //{
        //    get { return _allocationItems ?? (_allocationItems = new List<AllocationItem>()); }
        //    protected set { _allocationItems = value; }
        //}

    }


    /// <summary>
    /// 调拨单项目
    /// </summary>

    public class AllocationItem : BaseEntity
    {

        /// <summary>
        /// 调拨单Id
        /// </summary>
        public int AllocationBillId { get; set; }

        /// <summary>
        /// 商品Id
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public int UnitId { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 批发价
        /// </summary>
        public decimal? TradePrice { get; set; }

        /// <summary>
        /// 批发金额
        /// </summary>
        public decimal? WholesaleAmount { get; set; }

        /// <summary>
        /// 出库库存
        /// </summary>
        public int OutgoingStock { get; set; }

        /// <summary>
        /// 入库库存
        /// </summary>
        public int WarehousingStock { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// 生产日期
        /// </summary>
        public DateTime? ManufactureDete { get; set; }

        


        //(导航) 调拨单
        public virtual AllocationBill AllocationBill { get; set; }
    }


    public class QuickAllocationItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    /// <summary>
    /// 项目保存或者编辑
    /// </summary>
    public class AllocationBillUpdate : BaseEntity
    {

        public AllocationBillUpdate()
        {
            Items = new List<AllocationItem>();
        }
        public string BillNumber { get; set; }
        /// <summary>
        /// 出货仓库
        /// </summary>
        public int ShipmentWareHouseId { get; set; } = 0;

        /// <summary>
        /// 入货仓库
        /// </summary>
        public int IncomeWareHouseId { get; set; } = 0;

        /// <summary>
        /// 调拨日期
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// 是否按最小单位调拨
        /// </summary>
        public bool AllocationByMinUnit { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 操作源
        /// </summary>
        public int? Operation { get; set; } = 0;

        /// <summary>
        /// 项目
        /// </summary>
        public List<AllocationItem> Items { get; set; }

    }


}
