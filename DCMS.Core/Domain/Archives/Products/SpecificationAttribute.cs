using System.Collections.Generic;

namespace DCMS.Core.Domain.Products
{

    public enum SpecificationAttributeType
    {
        /// <summary>
        /// Option
        /// </summary>
        Option = 0,

        /// <summary>
        /// Custom text
        /// </summary>
        CustomText = 10,

        /// <summary>
        /// Custom HTML text
        /// </summary>
        CustomHtmlText = 20,

        /// <summary>
        /// Hyperlink
        /// </summary>
        Hyperlink = 30
    }

    /// <summary>
    ///  表示规格属性
    /// </summary>
    public partial class SpecificationAttribute : BaseEntity
    {
        private ICollection<SpecificationAttributeOption> _specificationAttributeOptions;


        /// <summary>
        /// 属性名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// 属性项
        /// </summary>
        public virtual ICollection<SpecificationAttributeOption> SpecificationAttributeOptions
        {
            get { return _specificationAttributeOptions ?? (_specificationAttributeOptions = new List<SpecificationAttributeOption>()); }
            protected set { _specificationAttributeOptions = value; }
        }
    }
}
