namespace DCMS.Core.Domain.Products
{
    using System.ComponentModel.DataAnnotations.Schema;
    /// <summary>
    /// 表示商品提供商
    /// </summary>
    public partial class ProductManufacturer : BaseEntity
    {
        /// <summary>
        /// 商品标识
        /// </summary>
        public int ProductId { get; set; } = 0;

        /// <summary>
        /// 提供商ID
        /// </summary>
        public int ManufacturerId { get; set; } = 0;

        /// <summary>
        /// 是否特色商品
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool IsFeaturedProduct { get; set; } = false;

        /// <summary>
        /// 排序
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// 提供商
        /// </summary>
        public virtual Manufacturer Manufacturer { get; set; }

        /// <summary>
        /// 商品
        /// </summary>
        public virtual Product Product { get; set; }
    }

}
