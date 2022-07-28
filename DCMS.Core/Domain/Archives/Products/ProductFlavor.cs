namespace DCMS.Core.Domain.Products
{
    /// <summary>
    /// 商品口味实体
    /// </summary>
    public class ProductFlavor : BaseEntity
    {
        /// <summary>
        /// 父商品Id
        /// </summary>
        public int ParentId { get; set; }

        /// <summary>
        /// 商品Id
        /// </summary>
        public int ProductId { get; set; }
        /// <summary>
        /// 口味
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 小单位条码
        /// </summary>
        public string SmallUnitBarCode { get; set; }
        /// <summary>
        /// 中单位条码
        /// </summary>
        public string StrokeUnitBarCode { get; set; }
        /// <summary>
        /// 大单位条码
        /// </summary>
        public string BigUnitBarCode { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 添加方式 0：输入，1：选择
        /// 输入：productId = 当前表Id
        /// 选择：productId = 用户选择商品Id
        /// </summary>
        public int AddType { get; set; }

    }
}
