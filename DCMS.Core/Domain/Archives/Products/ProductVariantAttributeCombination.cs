namespace DCMS.Core.Domain.Products
{
    using System.ComponentModel.DataAnnotations.Schema;
    /// <summary>
    /// 组合属性
    /// </summary>
    public partial class ProductVariantAttributeCombination : BaseEntity
    {
        /// <summary>
        /// 商品ID
        /// </summary>
        public int ProductId { get; set; } = 0;

        /// <summary>
        /// 属性XML
        /// </summary>
        public string AttributesXml { get; set; } = "";

        /// <summary>
        /// 库存量
        /// </summary>
        public int StockQuantity { get; set; } = 0;

        /// <summary>
        /// 是否允许在缺货时下单
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool AllowOutOfStockOrders { get; set; } = false;

        /// <summary>
        /// 商品SKU码
        /// </summary>
        public string Sku { get; set; } = "";

        /// <summary>
        /// 商品指定提供商编码
        /// </summary>
        public string ManufacturerPartNumber { get; set; } = "";

        /// <summary>
        /// Gtin 编码
        /// </summary>
        public string Gtin { get; set; } = "";

        /// <summary>
        /// 商品
        /// </summary>
        public virtual Product Product { get; set; }

    }
}
