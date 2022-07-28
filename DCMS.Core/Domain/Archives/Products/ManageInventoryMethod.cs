namespace DCMS.Core.Domain.Products
{
    /// <summary>
    /// 表示用于库存管理办法
    /// </summary>
    public enum ManageInventoryMethod
    {
        /// <summary>
        /// 不跟踪产品库存
        /// </summary>
        DontManageStock = 0,
        /// <summary>
        /// 跟踪产品库存
        /// </summary>
        ManageStock = 1,
        /// <summary>
        /// 按产品属性跟踪产品库存
        /// </summary>
        ManageStockByAttributes = 2,
    }

    /// <summary>
    /// 低库存时的处理方法
    /// </summary>
    public enum LowStockActivity
    {
        /// <summary>
        /// 不处理
        /// </summary>
        Nothing = 0,
        /// <summary>
        /// 禁用下单按钮
        /// </summary>
        DisablePlaceButton = 1,
        /// <summary>
        /// 不发布商品
        /// </summary>
        Unpublish = 2,
    }
}
