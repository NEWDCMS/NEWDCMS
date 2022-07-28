namespace DCMS.Core.Domain.Security
{
    /// <summary>
    /// 用于自动访问控制识别
    /// </summary>
    public partial class AclRecord : BaseEntity
    {
        /// <summary>
        /// 实体标识
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// 实体名
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// 用户角色
        /// </summary>
        public int UserRoleId { get; set; }
    }
}
