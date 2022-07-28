using System;
namespace DCMS.Core.Domain.Products
{

    /// <summary>
    ///表示商品的最近销售价格
    /// </summary>
    public class RecentPrice : BaseEntity
    {


        /// <summary>
        /// 客户名称
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// 商品
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 小单位价格
        /// </summary>
        public decimal? SmallUnitPrice { get; set; }

        /// <summary>
        /// 中单位价格
        /// </summary>
        public decimal? StrokeUnitPrice { get; set; }

        /// <summary>
        /// 大单位价格
        /// </summary>
        public decimal? BigUnitPrice { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime UpdateTime { get; set; }


        /// <summary>
        ///商品（导航）
        /// </summary>
        public virtual Product Product { get; set; }
    }
}
