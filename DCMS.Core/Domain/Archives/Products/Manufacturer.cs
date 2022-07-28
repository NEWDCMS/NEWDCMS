using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DCMS.Core.Domain.Products
{
    /// <summary>
    ///  表示商品提供商
    /// </summary>
    public partial class Manufacturer : BaseEntity
    {
        public Manufacturer() { Status = true; }
        /// <summary>
        /// 提供商名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 助记码
        /// </summary>
        public string MnemonicName { get; set; }
        /// <summary>
        /// 联系人
        /// </summary>
        public string ContactName { get; set; }
        /// <summary>
        /// 联系人电话
        /// </summary>
        public string ContactPhone { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Status { get; set; }
        /// <summary>
        /// 是否删除
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Deleted { get; set; }
        /// <summary>
        /// 价格范围
        /// </summary>
        public string PriceRanges { get; set; }


        /// <summary>
        /// 排序
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedOnUtc { get; set; }
    }


    /// <summary>
    /// 供应商账户余额
    /// </summary>
    public class ManufacturerBalance : BaseEntity
    {
        /// <summary>
        /// 科目Id
        /// </summary>
        public int AccountingOptionId { get; set; }

        /// <summary>
        /// 科目名称
        /// </summary>
        public string AccountingName { get; set; }

        /// <summary>
        /// 剩余预付款金额
        /// </summary>
        public decimal AdvanceAmountBalance { get; set; } = 0;
        /// <summary>
        /// 总欠款
        /// </summary>
        public decimal TotalOweCash { get; set; } = 0;
        /// <summary>
        /// 剩余欠款额度
        /// </summary>
        public decimal OweCashBalance { get; set; } = 0;

    }
}
