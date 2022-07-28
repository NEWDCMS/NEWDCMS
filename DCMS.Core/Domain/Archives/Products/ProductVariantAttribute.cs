using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DCMS.Core.Domain.Products
{
    /// <summary>
    /// 商品变体属性
    /// </summary>
    public partial class ProductVariantAttribute : BaseEntity
    {
        private ICollection<ProductVariantAttributeValue> _productVariantAttributeValues;

        /// <summary>
        /// 提示
        /// </summary>
        public string TextPrompt { get; set; } = "";

        /// <summary>
        /// 是否包含商品
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool IsRequired { get; set; } = false;

        /// <summary>
        /// 销售类型ID
        /// </summary>
        public int AttributeControlTypeId { get; set; } = 0;

        /// <summary>
        /// 排序
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// 更新属性类型
        /// </summary>
        public AttributeControlType AttributeControlType
        {
            get
            {
                return (AttributeControlType)AttributeControlTypeId;
            }
            set
            {
                AttributeControlTypeId = (int)value;
            }
        }





        /// <summary>
        ///商品ID
        /// </summary>
        public virtual int ProductId { get; set; } = 0;

        /// <summary>
        /// 商品属性ID
        /// </summary>
        public virtual int ProductAttributeId { get; set; } = 0;


        /// <summary>
        /// 获取商品属性
        /// </summary>
        public virtual ProductAttribute ProductAttribute { get; set; }

        /// <summary>
        /// 获取商品
        /// </summary>
        public virtual Product Product { get; set; }


        /// <summary>
        /// 获取变体属性
        /// </summary>
        public virtual ICollection<ProductVariantAttributeValue> ProductVariantAttributeValues
        {
            get { return _productVariantAttributeValues ?? (_productVariantAttributeValues = new List<ProductVariantAttributeValue>()); }
            protected set { _productVariantAttributeValues = value; }
        }

    }

}
