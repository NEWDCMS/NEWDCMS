using DCMS.Core.Domain.News;
using DCMS.Services.Security;
using DCMS.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DCMS.Services.News
{
    /// <summary>
    /// 新闻扩展
    /// </summary>
    public static class NewsCategoryExtensions
    {
        public static IList<NewsCategory> SortCategoriesForTree(this IList<NewsCategory> source, int parentId = 0, bool ignoreCategoriesWithoutExistingParent = false)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            var result = new List<NewsCategory>();

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


        public static NewsCategory FindNewsCategory(this IList<NewsCategory> source,
            int newsId, int categoryId)
        {
            foreach (var newsCategory in source)
            {
                if (newsCategory.NewsItemId == newsId && newsCategory.Id == categoryId)
                {
                    return newsCategory;
                }
            }

            return null;
        }

        /// <summary>
        /// 面包导航
        /// </summary>
        /// <param name="category"></param>
        /// <param name="categoryService"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string GetFormattedBreadCrumb(this NewsCategory category,
            INewsCategoryService categoryService,
            string separator = ">>")
        {
            if (category == null)
            {
                throw new ArgumentNullException("category");
            }

            string result = string.Empty;

            var alreadyProcessedCategoryIds = new List<int>() { };

            while (category != null &&  //not null
                !category.Deleted &&  //not deleted
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

                category = categoryService.GetCategoryById(0, category.ParentId.Value);

            }
            return result;
        }

        /// <summary>
        /// 面包导航
        /// </summary>
        /// <param name="category"></param>
        /// <param name="categoryService"></param>
        /// <param name="aclService"></param>
        /// <param name="storeMappingService"></param>
        /// <param name="showHidden"></param>
        /// <returns></returns>
        public static IList<NewsCategory> GetCategoryBreadCrumb(this NewsCategory category,
            INewsCategoryService categoryService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            bool showHidden = false)
        {
            if (category == null)
            {
                throw new ArgumentNullException("category");
            }

            var result = new List<NewsCategory>();

            var alreadyProcessedCategoryIds = new List<int>() { };

            while (category != null && //not null
                !category.Deleted && //not deleted
                                     //(showHidden || category.Published) && //published
                                     //(showHidden || aclService.Authorize(category)) && //ACL
                                     //(showHidden || storeMappingService.Authorize(category)) && //Store mapping
                !alreadyProcessedCategoryIds.Contains(category.Id)) //prevent circular references
            {
                result.Add(category);

                alreadyProcessedCategoryIds.Add(category.Id);

                category = categoryService.GetCategoryById(0, category.ParentId.Value);
            }
            result.Reverse();
            return result;
        }

    }
}
