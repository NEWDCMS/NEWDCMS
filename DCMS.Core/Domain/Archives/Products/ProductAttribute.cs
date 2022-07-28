
namespace DCMS.Core.Domain.Products
{
    /// <summary>
    ///  表示商品属性
    /// </summary>
    public partial class ProductAttribute : BaseEntity
    {



        /// <summary>
        /// 属性名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
    }
}
