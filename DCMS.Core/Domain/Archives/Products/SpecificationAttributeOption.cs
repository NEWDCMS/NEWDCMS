using Newtonsoft.Json;
using System.Collections.Generic;

namespace DCMS.Core.Domain.Products
{
    /// <summary>
    /// 规格属性选项
    /// </summary>
    public partial class SpecificationAttributeOption : BaseEntity
    {
        private ICollection<ProductSpecificationAttribute> _productSpecificationAttributes;

        /// <summary>
        ///属性标识
        /// </summary>
        public int SpecificationAttributeId { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int DisplayOrder { get; set; }


        /// <summary>
        /// 换算数量
        /// </summary>
        public int? ConvertedQuantity { get; set; }

        /// <summary>
        /// 单位换算
        /// </summary>
        public string UnitConversion { get; set; }




        /// <summary>
        /// 导航属性
        /// </summary>
        [JsonIgnore]
        public virtual SpecificationAttribute SpecificationAttribute { get; set; }

        /// <summary>
        ///导航属性
        /// </summary>
        public virtual ICollection<ProductSpecificationAttribute> ProductSpecificationAttributes
        {
            get { return _productSpecificationAttributes ?? (_productSpecificationAttributes = new List<ProductSpecificationAttribute>()); }
            protected set { _productSpecificationAttributes = value; }
        }
    }
}
