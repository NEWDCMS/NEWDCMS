namespace DCMS.Core.Domain.Products
{
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// 用于表示商品类别实体
    /// </summary>
    public class Category : BaseEntity
    {

        /// <summary>
        /// 分类名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 经销商ID
        /// </summary>
        //public int StoreId { get; set; }
        /// <summary>
        /// 父类代码（用于优先遍历）
        /// </summary>
        public int ParentId { get; set; }

        /// <summary>
        /// 继承路径(用于关联ERP)
        /// </summary>
        public string PathCode { get; set; }

        /// <summary>
        /// 统计类别
        /// </summary>
        public int StatisticalType { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int OrderNo { get; set; }

        /// <summary>
        /// 品牌
        /// </summary>
        public string BrandName { get; set; }

        /// <summary>
        /// 品牌Id
        /// </summary>
        public int? BrandId { get; set; } = 0;

        /// <summary>
        /// 是否移除（数据库保留）
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Deleted { get; set; }

        /// <summary>
        /// 是否允许发布
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Published { get; set; }

        /// <summary>
        /// 提成方案
        /// </summary>
        public int? PercentageId { get; set; } = 0;


        /// <summary>
        /// 是否预设
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool IsPreset { get; set; }


    }
}
