using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DCMS.Core.Domain.Products
{

    public class Brand : BaseEntity
    {

        /// <summary>
        /// 品牌名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Status { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// 是否预设
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool IsPreset { get; set; }

    }
}
