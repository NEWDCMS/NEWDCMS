using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DCMS.Core.Domain.Terminals
{
    /// <summary>
    /// 终端等级信息
    /// </summary>
    public class Rank : BaseEntity
    {

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Describe { get; set; }
        /// <summary>
        /// 是否删除
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Deleted { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateDate { get; set; }
    }
}
