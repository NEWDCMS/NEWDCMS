
using System.ComponentModel.DataAnnotations.Schema;

namespace DCMS.Core.Domain.Products
{
    /// <summary>
    /// 变体商品属性值
    /// </summary>
    public partial class ProductVariantAttributeValue : BaseEntity
    {


        /// <summary>
        /// 类型ID
        /// </summary>
        public int AttributeValueTypeId { get; set; }

        /// <summary>
        /// 关联商品ID
        /// </summary>
        public int AssociatedProductId { get; set; }

        /// <summary>
        /// 属性名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ColorSquaresRgb { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal PriceAdjustment { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal WeightAdjustment { get; set; }

        /// <summary>
        /// 是否预先选择
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool IsPreSelected { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// 关联图片
        /// </summary>
        public int PictureId { get; set; }



        /// <summary>
        /// 属性ID
        /// </summary>
        public virtual int ProductVariantAttributeId { get; set; }

        /// <summary>
        /// 商品变体属性
        /// </summary>
        public virtual ProductVariantAttribute ProductVariantAttribute { get; set; }

        /// <summary>
        /// 属性类型
        /// </summary>
        public AttributeValueType AttributeValueType
        {
            get
            {
                return (AttributeValueType)AttributeValueTypeId;
            }
            set
            {
                AttributeValueTypeId = (int)value;
            }
        }
    }
}
