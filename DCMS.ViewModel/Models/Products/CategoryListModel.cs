//
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;

namespace DCMS.ViewModel.Models.Products
{
    public partial class CategoryListModel : BaseModel
    {
        [HintDisplayName("搜索类别", "搜索类别名称")]

        public string SearchCategoryName { get; set; }
    }
}