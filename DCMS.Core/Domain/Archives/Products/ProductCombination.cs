using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
namespace DCMS.Core.Domain.Products
{

    /// <summary>
    /// 表示商品组合销售
    /// </summary>
    public class Combination : BaseEntity
    {

        private ICollection<ProductCombination> _productCombinations;


        /// <summary>
        /// 是否启用
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Enabled { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// 主商品Id
        /// </summary>
        public int ProductId { get; set; }


        public string ProductName { get; set; }

        public virtual Product Product { get; set; }

        /// <summary>
        /// (导航) 商品组合
        /// </summary>

        public virtual ICollection<ProductCombination> ProductCombinations
        {
            get { return _productCombinations ?? (_productCombinations = new List<ProductCombination>()); }
            protected set { _productCombinations = value; }
        }
    }


    /// <summary>
    /// 表示商品组合销售
    /// </summary>
    public class ProductCombination : BaseEntity
    {


        /// <summary>
        /// 组合Id
        /// </summary>
        public int CombinationId { get; set; }

        /// <summary>
        /// 子商品Id
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 单位(规格属性项)
        /// </summary>
        public int UnitId { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int DisplayOrder { get; set; }

        public virtual Combination Combination { get; set; }

        public virtual Product Product { get; set; }
    }
}
