using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;

namespace DCMS.ViewModel.Models.Configuration
{
    public partial class CatalogSettingsModel : BaseModel
    {
        public int ActiveStoreScopeConfiguration { get; set; } = 0;

        [HintDisplayName("是否显示商品SKU", "是否显示商品SKU")]
        public bool ShowProductSku { get; set; }
        public bool ShowProductSku_OverrideForStore { get; set; }

    }
}