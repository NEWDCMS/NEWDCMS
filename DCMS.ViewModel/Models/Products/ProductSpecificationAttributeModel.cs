//
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;

namespace DCMS.ViewModel.Models.Products
{
    public partial class ProductSpecificationAttributeModel : BaseEntityModel
    {
        [HintDisplayName("属性名", "属性名")]

        public string SpecificationAttributeName { get; set; }

        [HintDisplayName("属性项名", "属性项名")]

        public string SpecificationAttributeOptionName { get; set; }

        [HintDisplayName("自定义值", "自定义值")]

        public string CustomValue { get; set; }

        [HintDisplayName("允许筛选", "允许筛选")]
        public bool AllowFiltering { get; set; }

        [HintDisplayName("排序", "排序")]
        public int DisplayOrder { get; set; }
    }
}