using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


namespace DCMS.Core.Domain.WareHouses
{

    /// <summary>
    /// 用于表示成本调价单
    /// </summary>
    public class CostAdjustmentBill : BaseBill<CostAdjustmentItem>
    {

        public CostAdjustmentBill()
        {
            BillType = BillTypeEnum.CostAdjustmentBill;
        }


        // private ICollection<CostAdjustmentItem> _costAdjustmentItems;

        /// <summary>
        /// 调价日期
        /// </summary>
        public DateTime AdjustmentDate { get; set; }

        /// <summary>
        /// 是否按最小单位调价
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool AdjustmentByMinUnit { get; set; }

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
        ///// (导航)成本调价单项目
        ///// </summary>
        //public virtual ICollection<CostAdjustmentItem> CostAdjustmentItems
        //{
        //    get { return _costAdjustmentItems ?? (_costAdjustmentItems = new List<CostAdjustmentItem>()); }
        //    protected set { _costAdjustmentItems = value; }
        //}

        /// <summary>
        /// 记账凭证
        /// </summary>
        public int VoucherId { get; set; }

    }


    /// <summary>
    /// 成本调价单项目
    /// </summary>

    public class CostAdjustmentItem : BaseEntity
    {

        /// <summary>
        /// 成本调价单
        /// </summary>
        public int CostAdjustmentBillId { get; set; }

        /// <summary>
        /// 商品Id
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public int UnitId { get; set; }


        /// <summary>
        /// 调整前价格
        /// </summary>
        public decimal? AdjustmentPriceBefore { get; set; }

        /// <summary>
        /// 调整后价格
        /// </summary>
        public decimal? AdjustedPrice { get; set; }


        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }


        //(导航) 成本调价单
        public virtual CostAdjustmentBill CostAdjustmentBill { get; set; }
    }

    /// <summary>
    /// 项目保存或者编辑
    /// </summary>
    public class CostAdjustmentBillUpdate : BaseEntity
    {
        public CostAdjustmentBillUpdate()
        {
            Items = new List<CostAdjustmentItem>();
        }

        /// <summary>
        /// 调价日期
        /// </summary>
        public DateTime AdjustmentDate { get; set; }

        /// <summary>
        /// 是否按最小单位调价
        /// </summary>
        public bool AdjustmentByMinUnit { get; set; }

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
        public List<CostAdjustmentItem> Items { get; set; }

    }

}
