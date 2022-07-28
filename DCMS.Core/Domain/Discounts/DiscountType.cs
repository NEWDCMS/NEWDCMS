namespace DCMS.Core.Domain.Discounts
{
    /// <summary>
    /// 折扣类型
    /// </summary>
    public enum DiscountType
    {
        /// <summary>
        /// 分配给订单总计
        /// </summary>
        AssignedToOrderTotal = 1,
        AssignedToSkus = 2,
        AssignedToCategories = 5,
        AssignedToManufacturers = 6,
        AssignedToShipping = 10,
        AssignedToOrderSubTotal = 20
    }
}
