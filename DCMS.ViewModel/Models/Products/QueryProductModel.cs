using DCMS.Core.Domain.Products;
using DCMS.Web.Framework.Models;


namespace DCMS.ViewModel.Models.Products
{
    public partial class QueryProductModel : BaseEntityModel
    {
        public Product Product { get; set; }
        public string CategoryName { get; set; }
        public string BrandName { get; set; }

    }
}