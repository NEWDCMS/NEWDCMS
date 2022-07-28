using System;
using System.ComponentModel.DataAnnotations.Schema;


namespace DCMS.Core.Domain.Terminals
{
    /// <summary>
    /// 渠道信息
    /// </summary>
    public class Channel : BaseEntity
    {

        /// <summary>
        /// 序号
        /// </summary>
        public int? OrderNo { get; set; } = 0;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Describe { get; set; }
        /// <summary>
        /// 预设属性(枚举1 特殊通道 2 餐饮 3 小超 4 大超 5 其他)
        /// </summary>
        public int Attribute { get; set; }
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
