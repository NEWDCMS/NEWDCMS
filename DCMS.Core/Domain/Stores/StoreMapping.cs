namespace DCMS.Core.Domain.Stores
{
    /// <summary>
    /// 网点(经销商)实体映射
    /// </summary>
    public partial class StoreMapping : BaseEntity
    {
        /// <summary>
        /// 实体标识
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// 实体名称
        /// </summary>
        public string EntityName { get; set; }


    }
}
