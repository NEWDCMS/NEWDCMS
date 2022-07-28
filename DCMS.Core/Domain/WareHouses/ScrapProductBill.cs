using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


namespace DCMS.Core.Domain.WareHouses
{

    /// <summary>
    /// 用于表示商品报损单
    /// </summary>
    public class ScrapProductBill : BaseBill<ScrapProductItem>
    {
        public ScrapProductBill()
        {
            BillType = BillTypeEnum.ScrapProductBill;
        }

        //private ICollection<ScrapProductItem> _scrapProductItems;


        /// <summary>
        /// 经办人
        /// </summary>
        public int ChargePerson { get; set; }

        /// <summary>
        /// 仓库
        /// </summary>
        public int WareHouseId { get; set; }

        /// <summary>
        /// 是否按基本单位报损
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool ScrapByBaseUnit { get; set; }

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
        ///// (导航)成本调价单项目
        ///// </summary>
        //public virtual ICollection<ScrapProductItem> ScrapProductItems
        //{
        //    get { return _scrapProductItems ?? (_scrapProductItems = new List<ScrapProductItem>()); }
        //    protected set { _scrapProductItems = value; }
        //}

        /// <summary>
        /// 记账凭证
        /// </summary>
        public int VoucherId { get; set; }


        /// <summary>
        /// 报损：0：营业内，1：营业外
        /// </summary>
        public int Reason { get; set; }
    }


    /// <summary>
    /// 商品报损单项目
    /// </summary>

    public class ScrapProductItem : BaseEntity
    {

        /// <summary>
        /// 商品报损单
        /// </summary>
        public int ScrapProductBillId { get; set; }

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
        /// 批发价
        /// </summary>
        public decimal? TradePrice { get; set; }

        /// <summary>
        /// 批发金额
        /// </summary>
        public decimal? TradeAmount { get; set; }


        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }


        //(导航) 商品报损单
        public virtual ScrapProductBill ScrapProductBill { get; set; }
    }

    /// <summary>
    /// 项目保存或者编辑
    /// </summary>
    public class ScrapProductBillUpdate : BaseEntity
    {
        public ScrapProductBillUpdate()
        {
            Items = new List<ScrapProductItem>();
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
        /// 是否按基本单位报损
        /// </summary>
        public bool ScrapByBaseUnit { get; set; }

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
        public List<ScrapProductItem> Items { get; set; }

    }

}
