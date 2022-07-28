using System;
using System.Collections.Generic;


namespace DCMS.Core.Domain.WareHouses
{

    /// <summary>
    /// 用于表示库存商品拆分单
    /// </summary>
    public class SplitProductBill : BaseBill<SplitProductItem>
    {

        public SplitProductBill()
        {
            BillType = BillTypeEnum.SplitProductBill;
        }


        //private ICollection<SplitProductItem> _splitProductItems;


        /// <summary>
        /// 仓库
        /// </summary>
        public int WareHouseId { get; set; }

        /// <summary>
        /// 主商品
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 主商品数量
        /// </summary>
        public int? Quantity { get; set; }

        /// <summary>
        /// 主商品成本
        /// </summary>
        public decimal? ProductCost { get; set; }

        /// <summary>
        /// 主商品成本金额
        /// </summary>
        public decimal? ProductCostAmount { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        //public string Remark { get; set; }

        /// <summary>
        /// 成本差额
        /// </summary>
        public decimal? CostDifference { get; set; }

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


        //public DateTime CreatedOnUtc { get; set; }

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
        ///// (导航)
        ///// </summary>
        //public virtual ICollection<SplitProductItem> SplitProductItems
        //{
        //    get { return _splitProductItems ?? (_splitProductItems = new List<SplitProductItem>()); }
        //    protected set { _splitProductItems = value; }
        //}


        /// <summary>
        /// 记账凭证
        /// </summary>
        public int VoucherId { get; set; }
    }


    /// <summary>
    /// 库存商品拆分单项目
    /// </summary>

    public class SplitProductItem : BaseEntity
    {

        /// <summary>
        /// 库存商品拆分单
        /// </summary>
        public int SplitProductBillId { get; set; }

        /// <summary>
        /// 商品Id
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 商品单位
        /// </summary>
        public int UnitId { get; set; }

        /// <summary>
        /// 子商品单位
        /// </summary>
        public int? SubProductUnitId { get; set; } = 0;

        /// <summary>
        /// 子商品数量
        /// </summary>
        public int? SubProductQuantity { get; set; } = 0;

        /// <summary>
        /// 数量
        /// </summary>
        public int? Quantity { get; set; } = 0;

        /// <summary>
        /// 单位成本
        /// </summary>
        public decimal? CostPrice { get; set; }

        /// <summary>
        /// 成本金额
        /// </summary>
        public decimal? CostAmount { get; set; }

        /// <summary>
        /// 库存
        /// </summary>
        public int? Stock { get; set; } = 0;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        //(导航) 
        public virtual SplitProductBill SplitProductBill { get; set; }
    }

    /// <summary>
    /// 项目保存或者编辑
    /// </summary>
    public class SplitProductBillUpdate : BaseEntity
    {

        public SplitProductBillUpdate()
        {
            Items = new List<SplitProductItem>();
        }

        /// <summary>
        /// 仓库
        /// </summary>
        public int WareHouseId { get; set; } = 0;

        /// <summary>
        /// 主商品
        /// </summary>
        public int ProductId { get; set; } = 0;

        /// <summary>
        /// 主商品数量
        /// </summary>
        public int? Quantity { get; set; } = 0;

        /// <summary>
        /// 主商品成本
        /// </summary>
        public decimal? ProductCost { get; set; } = 0;

        /// <summary>
        /// 主商品成本金额
        /// </summary>
        public decimal? ProductCostAmount { get; set; } = 0;

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 操作源
        /// </summary>
        public int? Operation { get; set; } = 0;

        /// <summary>
        /// 成本差额
        /// </summary>
        public decimal? CostDifference { get; set; } = 0;

        /// <summary>
        /// 项目
        /// </summary>
        public List<SplitProductItem> Items { get; set; }

    }

}
