//
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;

namespace DCMS.ViewModel.Models.Products
{
    public partial class ManufacturerListModel : BaseModel
    {
        [HintDisplayName("搜索产品提供商", "搜索产品提供商名称")]

        public string SearchManufacturerName { get; set; }
    }
}