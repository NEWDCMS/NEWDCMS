using DCMS.Core;
using DCMS.Core.Domain.Products;
using System.Collections.Generic;

namespace DCMS.Services.Products
{
    public interface IProductFlavorService
    {
        void DeleteProductFlavor(ProductFlavor productFlavor);
        ProductFlavor GetProductFlavorById(int productFlavorId);
        IList<ProductFlavor> GetProductFlavors();
        IList<ProductFlavor> GetProductFlavorsByProductId(int? pid);
        IList<ProductFlavor> GetProductFlavorsByParentId(int? pid);
        void InsertProductFlavor(ProductFlavor productFlavor);
        void UpdateProductFlavor(ProductFlavor productFlavor);

        int GetProductId(int flavorId);
        IList<int> GetProductIds(int[] flavorIds);

        IPagedList<ProductFlavor> GetProductFlavors(string key = null, int parentId = 0, int pageIndex = 0, int pageSize = int.MaxValue);
    }
}