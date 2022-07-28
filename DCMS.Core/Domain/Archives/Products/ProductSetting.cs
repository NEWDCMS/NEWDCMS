using DCMS.Core.Configuration;


namespace DCMS.Core.Domain.Products
{
    public class ProductSetting : ISettings
    {
        /// <summary>
        /// 商品小单位规格映射
        /// </summary>
        public int SmallUnitSpecificationAttributeOptionsMapping { get; set; }

        /// <summary>
        /// 商品中单位规格映射
        /// </summary>
        public int StrokeUnitSpecificationAttributeOptionsMapping { get; set; }

        /// <summary>
        /// 商品大单位规格映射
        /// </summary>
        public int BigUnitSpecificationAttributeOptionsMapping { get; set; }
    }
}
