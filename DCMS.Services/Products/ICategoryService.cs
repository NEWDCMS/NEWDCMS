using DCMS.Core;
using DCMS.Core.Domain.Products;
using System.Collections.Generic;

namespace DCMS.Services.Products
{
    /// <summary>
    /// 类别服务接口
    /// </summary>
    public partial interface ICategoryService
    {

        void DeleteCategory(Category category);

        IPagedList<Category> GetAllCategories(int? store, string categoryName = "",
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        List<Category> GetAllCategories(int? store);
        IList<Category> GetAllCategoriesByParentCategoryId(int? store, List<Category> allCategoies, int parentCategoryId, bool showHidden = false);
        IList<Category> GetAllCategoriesByParentCategoryId(int? store, int parentCategoryId,
            bool showHidden = false);


        IList<Category> GetAllCategoriesDisplayed(int? store);
        /// <summary>
        /// 绑定商品类别信息
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        IList<Category> BindCategoryList(int? store);

        IList<Category> GetAllCategoriesByIds(int? store, int[] ids);
        Dictionary<int, string> GetAllCategoriesNames(int? store, int[] ids);
        IList<Category> GetCategoriesByCategoryIds(int? store, int[] ids);

        int GetCategoriesMinId(int? store);

        Category GetCategoryById(int? store, int categoryId);
        string GetCategoryName(int? store, int categoryId);

        int GetCategoryId(int store, string categoryName);


        void InsertCategory(Category category);

        void UpdateCategory(Category category);


        void DeleteProductCategory(ProductCategory productCategory);

        IPagedList<ProductCategory> GetProductCategoriesByCategoryId(int categoryId, int? userId, int? storeId,
            int pageIndex, int pageSize, bool showHidden = false);


        IList<ProductCategory> GetProductCategoriesByProductId(int productId, int? userId, int? storeId, bool showHidden = false);


        ProductCategory GetProductCategoryById(int productCategoryId);


        void InsertProductCategory(ProductCategory productCategory);


        void UpdateProductCategory(ProductCategory productCategory);


        IList<int> GetTreeSubCategoryIds(int? store, int categoryId);
        List<int> GetSubCategoryIds(int storeId, int categoryId);
        Dictionary<int, string> GetAllCategoriesNames(int? store, int categoryId);
        BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, Category category, Category data, bool isAdmin = false);
        IList<int> GetProductedId(int? store, int CategoryId);
        IList<int> GetProductedId1(int? store, int CategoryId);

        string GetCategoriesName(int? store, int? categoryid);
        IList<Category> GetCategoriesIdsByCategoryIds(int? store, int[] ids);
    }
}
