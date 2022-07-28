namespace DCMS.Core.Domain.Terminals
{
    using System.ComponentModel.DataAnnotations.Schema;
    /// <summary>
    /// 片区
    /// </summary>
    public class District : BaseEntity
    {

        /// <summary>
        /// 片区名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 父Id
        /// </summary>
        public int ParentId { get; set; }
        /// <summary>
        /// 排序号
        /// </summary>
        public int? OrderNo { get; set; } = 0;
        /// <summary>
        /// 描述
        /// </summary>
        public string Describe { get; set; }
        /// <summary>
        /// 是否删除
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Deleted { get; set; }
    }
}
