using System;
using System.Collections.Generic;


namespace DCMS.Core.Domain.WareHouses
{

    /// <summary>
    /// 用于表示库存商品组合单
    /// </summary>
    public class CombinationProductBill : BaseBill<CombinationProductItem>
    {

        public CombinationProductBill()
        {
            BillType = BillTypeEnum.CombinationProductBill;
        }



        //private ICollection<CombinationProductItem> _combinationProductItems;


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
        ///// (导航)
        ///// </summary>
        //public virtual ICollection<CombinationProductItem> CombinationProductItems
        //{
        //    get { return _combinationProductItems ?? (_combinationProductItems = new List<CombinationProductItem>()); }
        //    protected set { _combinationProductItems = value; }
        //}

    }


    /// <summary>
    /// 库存商品组合单项目
    /// </summary>

    public class CombinationProductItem : BaseEntity
    {

        /// <summary>
        /// 库存商品组合单
        /// </summary>
        public int CombinationProductBillId { get; set; }

        /// <summary>
        /// 子商品商品Id
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 商品单位
        /// </summary>
        public int UnitId { get; set; }

        /// <summary>
        /// 子商品单位
        /// </summary>
        public int? SubProductUnitId { get; set; }

        /// <summary>
        /// 子商品数量
        /// </summary>
        public int? SubProductQuantity { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int? Quantity { get; set; }

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
        public int? Stock { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        //(导航) 库存商品组合单
        public virtual CombinationProductBill CombinationProductBill { get; set; }
    }

    /// <summary>
    /// 项目保存或者编辑
    /// </summary>
    public class CombinationProductBillUpdate : BaseEntity
    {

        public CombinationProductBillUpdate()
        {
            Items = new List<CombinationProductItem>();
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
        public int? Quantity { get; set; }

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
        public int? Operation { get; set; }

        /// <summary>
        /// 成本差额
        /// </summary>
        public decimal? CostDifference { get; set; } = 0;

        /// <summary>
        /// 项目
        /// </summary>
        public List<CombinationProductItem> Items { get; set; }

    }


}
