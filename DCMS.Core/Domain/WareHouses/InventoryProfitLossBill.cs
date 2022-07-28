using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


namespace DCMS.Core.Domain.WareHouses
{

    /// <summary>
    /// 用于表示盘点盈亏单
    /// </summary>
    public class InventoryProfitLossBill : BaseBill<InventoryProfitLossItem>
    {
        public InventoryProfitLossBill()
        {
            BillType = BillTypeEnum.InventoryProfitLossBill;
        }

        // private ICollection<InventoryProfitLossItem> _inventoryProfitLossItems;


        /// <summary>
        /// 经办人
        /// </summary>
        public int ChargePerson { get; set; }

        /// <summary>
        /// 仓库
        /// </summary>
        public int WareHouseId { get; set; }

        /// <summary>
        /// 盘点时间
        /// </summary>
        public DateTime InventoryDate { get; set; }

        /// <summary>
        /// 是否按最小单位盘点
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool InventoryByMinUnit { get; set; }

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
        public int? PrintNum { get; set; }


        //public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// 操作源
        /// </summary>
        public int? Operation { get; set; }
        public OperationEnum Operations
        {
            get { return (OperationEnum)Operation; }
            set { Operation = (int)value; }
        }


        ///// <summary>
        ///// (导航)盘点盈亏单项目
        ///// </summary>
        //public virtual ICollection<InventoryProfitLossItem> InventoryProfitLossItems
        //{
        //    get { return _inventoryProfitLossItems ?? (_inventoryProfitLossItems = new List<InventoryProfitLossItem>()); }
        //    protected set { _inventoryProfitLossItems = value; }
        //}

        /// <summary>
        /// 记账凭证
        /// </summary>
        public int VoucherId { get; set; }
    }


    /// <summary>
    /// 盘点盈亏单项目
    /// </summary>

    public class InventoryProfitLossItem : BaseEntity
    {

        /// <summary>
        /// 盘点盈亏单
        /// </summary>
        public int InventoryProfitLossBillId { get; set; }

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
        /// 成本价
        /// </summary>
        public decimal? CostPrice { get; set; }

        /// <summary>
        /// 成本金额
        /// </summary>
        public decimal? CostAmount { get; set; }

        /// <summary>
        /// 库存数量
        /// </summary>
        public int StockQty { get; set; }


        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// 生产日期
        /// </summary>
        public DateTime? ManufactureDete { get; set; }


        //(导航) 盘点盈亏单
        public virtual InventoryProfitLossBill InventoryProfitLossBill { get; set; }
    }

    /// <summary>
    /// 项目保存或者编辑
    /// </summary>
    public class InventoryProfitLossBillUpdate : BaseEntity
    {
        public InventoryProfitLossBillUpdate()
        {
            Items = new List<InventoryProfitLossItem>();
        }

        /// <summary>
        /// 经办人
        /// </summary>
        public int ChargePerson { get; set; } = 0;

        /// <summary>
        /// 仓库
        /// </summary>
        public int WareHouseId { get; set; } = 0;

        /// <summary>
        /// 盘点时间
        /// </summary>
        public DateTime InventoryDate { get; set; }

        /// <summary>
        /// 是否按最小单位盘点
        /// </summary>
        public bool InventoryByMinUnit { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 操作源
        /// </summary>
        public int? Operation { get; set; }

        /// <summary>
        /// 项目
        /// </summary>
        public List<InventoryProfitLossItem> Items { get; set; }

    }

}
