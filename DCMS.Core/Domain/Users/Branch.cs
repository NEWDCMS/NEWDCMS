using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DCMS.Core.Domain.Users
{
    /// <summary>
    /// 组织机构
    /// </summary>
    public partial class Branch : BaseEntity
    {
        /// <summary>
        /// 经销商
        /// </summary>
        //移除

        /// <summary>
        /// 部门编码
        /// </summary>
        public int DeptCode { get; set; }

        /// <summary>
        /// 部门名称
        /// </summary>
        public string DeptName { get; set; }

        public int DeptShort { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string DeptDesc { get; set; }

        /// <summary>
        /// 所属父级
        /// </summary>
        public int ParentId { get; set; }

        /// <summary>
        /// 级别深度树
        /// </summary>
        public string TreePath { get; set; }

        /// <summary>
        /// 直接上级领导
        /// </summary>
        public string BranchLeader { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateDateTime { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int? DisplayOrder { get; set; } = 0;

        /// <summary>
        /// 状态
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Status { get; set; }
    }
}
