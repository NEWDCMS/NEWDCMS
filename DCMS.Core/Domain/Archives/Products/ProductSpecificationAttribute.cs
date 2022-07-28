namespace DCMS.Core.Domain.Products
{
    using Newtonsoft.Json;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// 表示商品规格属性
    /// </summary>
    public partial class ProductSpecificationAttribute : BaseEntity
    {
        /// <summary>
        /// 商品标识
        /// </summary>
        public int ProductId { get; set; } = 0;

        /// <summary>
        /// 规格属性项标识
        /// </summary>
        public int SpecificationAttributeOptionId { get; set; } = 0;

        /// <summary>
        /// 自定义值
        /// </summary>
        public string CustomValue { get; set; } = "";

        /// <summary>
        /// 是否允许筛选
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool AllowFiltering { get; set; } = false;

        /// <summary>
        /// 是否允许在商品页面显示
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool ShowOnProductPage { get; set; } = false;

        /// <summary>
        /// 排序
        /// </summary>
        public int DisplayOrder { get; set; } = 0;



        /// <summary>
        /// 商品
        /// </summary>
        [JsonIgnore]
        public virtual Product Product { get; set; }

        /// <summary>
        /// 规格项
        /// </summary>
        [JsonIgnore]
        public virtual SpecificationAttributeOption SpecificationAttributeOption { get; set; }
    }
}
