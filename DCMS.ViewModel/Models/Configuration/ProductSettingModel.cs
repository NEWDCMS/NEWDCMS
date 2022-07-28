using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Xml.Serialization;

namespace DCMS.ViewModel.Models.Configuration
{
    public class ProductSettingModel : BaseModel, ISettingsModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [HintDisplayName("商品小单位规格映射", "商品小单位规格映射")]
        public int SmallUnitSpecificationAttributeOptionsMapping { get; set; } = 0;

        [HintDisplayName("商品中单位规格映射", "商品中单位规格映射")]
        public int StrokeUnitSpecificationAttributeOptionsMapping { get; set; } = 0;

        [HintDisplayName("商品大单位规格映射", "商品大单位规格映射")]
        public int BigUnitSpecificationAttributeOptionsMapping { get; set; } = 0;

        [XmlIgnore]
        public SelectList SmallUnits { get; set; }
        [XmlIgnore]
        public SelectList StrokeUnits { get; set; }
        [XmlIgnore]
        public SelectList BigUnits { get; set; }


    }
}