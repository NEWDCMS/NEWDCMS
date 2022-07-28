namespace DCMS.Core.Domain.Stores
{
    /// <summary>
    /// 网点实体映射
    /// </summary>
    public partial interface IStoreMappingSupported
    {
        /// <summary>
        /// 是否限制
        /// </summary>
        bool LimitedToStores { get; set; }
    }
}
