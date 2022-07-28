using DCMS.Core.Domain.Products;
using DCMS.Services.Security;
using DCMS.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DCMS.Services.Products
{
    /// <summary>
    /// 产品类别扩展
    /// </summary>
    public static class CategoryExtensions
    {

        /// <summary>
        /// 排序类别树
        /// </summary>
        /// <param name="source">源</param>
        /// <param name="parentId">父节点</param>
        /// <param name="ignoreCategoriesWithoutExistingParent">是否忽略不存在的父级</param>
        /// <returns></returns>
        public static IList<Category> SortCategoriesForTree(this IList<Category> source, int parentId = 0, bool ignoreCategoriesWithoutExistingParent = false)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            var result = new List<Category>();

            foreach (var cat in source.ToList().FindAll(c => c.ParentId == parentId))
            {
                result.Add(cat);
                result.AddRange(SortCategoriesForTree(source, cat.Id, ignoreCategoriesWithoutExistingParent));
            }
            if (!ignoreCategoriesWithoutExistingParent && result.Count != source.Count)
            {
                foreach (var cat in source)
                {
                    if (result.FirstOrDefault(x => x.Id == cat.Id) == null)
                    {
                        if (cat.Id == parentId)
                        {
                            result.Add(cat);
                        }
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// 根据商品和类别查找审批类别
        /// </summary>
        /// <param name="source"></param>
        /// <param name="productId"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public static ProductCategory FindProductCategory(this IList<ProductCategory> source,
            int productId, int categoryId)
        {
            foreach (var productCategory in source)
            {
                if (productCategory.ProductId == productId && productCategory.CategoryId == categoryId)
                {
                    return productCategory;
                }
            }

            return null;
        }

        /// <summary>
        /// 格式化类别（返回类别面包导航，如：酒>>啤酒>>雪花啤酒）
        /// </summary>
        /// <param name="category"></param>
        /// <param name="categoryService"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string GetFormattedBreadCrumb(this Category category,
            ICategoryService categoryService,
            string separator = ">>")
        {
            if (category == null)
            {
                throw new ArgumentNullException("category");
            }

            string result = string.Empty;


            var alreadyProcessedCategoryIds = new List<int>() { };

            while (category != null &&
                !alreadyProcessedCategoryIds.Contains(category.Id))
            {
                if (string.IsNullOrEmpty(result))
                {
                    result = category.Name;
                }
                else
                {
                    result = string.Format("{0} {1} {2}", category.Name, separator, result);
                }

                alreadyProcessedCategoryIds.Add(category.Id);

                category = categoryService.GetCategoryById(category.StoreId, category.ParentId);

            }
            return result;
        }

        /// <summary>
        /// 获取类别面包导航
        /// </summary>
        /// <param name="category"></param>
        /// <param name="categoryService"></param>
        /// <param name="aclService"></param>
        /// <param name="storeMappingService"></param>
        /// <param name="showHidden"></param>
        /// <returns></returns>
        public static IList<Category> GetCategoryBreadCrumb(this Category category,
            ICategoryService categoryService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            bool showHidden = false)
        {
            if (category == null)
            {
                throw new ArgumentNullException("category");
            }

            var result = new List<Category>();

            var alreadyProcessedCategoryIds = new List<int>() { };

            while (category != null &&
                (showHidden) &&
                !alreadyProcessedCategoryIds.Contains(category.Id))
            {
                result.Add(category);

                alreadyProcessedCategoryIds.Add(category.Id);

                category = categoryService.GetCategoryById(category.StoreId, category.ParentId);
            }
            result.Reverse();
            return result;
        }

    }
}
