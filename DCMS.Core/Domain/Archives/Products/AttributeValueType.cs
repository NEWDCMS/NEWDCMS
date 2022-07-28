namespace DCMS.Core.Domain.Products
{

    /// <summary>
    /// 属性值类型 type
    /// </summary>
    public enum AttributeValueType
    {
        /// <summary>
        /// 简单的属性值
        /// </summary>
        Simple = 0,
        /// <summary>
        /// 相关产品（用于在配置捆绑产品时）
        /// </summary>
        AssociatedToProduct = 10,
    }
}
