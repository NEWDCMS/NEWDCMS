using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using System.Collections.Generic;

namespace DCMS.ViewModel.Models.Products
{
    //[Validator(typeof(SpecificationAttributeOptionValidator))]
    public partial class SpecificationAttributeOptionModel : BaseEntityModel
    {
        public int SpecificationAttributeId { get; set; } = 0;

        [HintDisplayName("名称", "属性项名称")]

        public string Name { get; set; }

        [HintDisplayName("排序", "排序")]
        public int DisplayOrder { get; set; } = 0;

        [HintDisplayName("换算数量", "换算数量")]
        public int? ConvertedQuantity { get; set; } = 0;

        [HintDisplayName("单位换算", "单位换算")]
        public string UnitConversion { get; set; }

        [HintDisplayName("关联商品数", "关联商品数")]
        public int NumberOfAssociatedProducts { get; set; } = 0;
    }


    public class SpecificationModel
    {
        public IList<SpecificationAttributeOptionModel> smallOptions { get; set; } = new List<SpecificationAttributeOptionModel>();
        public IList<SpecificationAttributeOptionModel> strokOptions { get; set; } = new List<SpecificationAttributeOptionModel>();
        public IList<SpecificationAttributeOptionModel> bigOptions { get; set; } = new List<SpecificationAttributeOptionModel>();
    }

}