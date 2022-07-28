using DCMS.Core;
using DCMS.Core.Domain.News;
using System.Collections.Generic;

namespace DCMS.Services.News
{

    /// <summary>
    /// 表示新闻类别
    /// </summary>
    public partial interface INewsCategoryService
    {

        /// <summary>
        ///  删除类别
        /// </summary>
        /// <param name="newscategory"></param>
        void DeleteCategory(int storeId, int userId, NewsCategory newscategory);

        /// <summary>
        ///  删除消息类别
        /// </summary>
        /// <param name="newsnewsCategory"></param>
        void DeleteNewsCategory(NewsCategory newsnewsCategory);

        /// <summary>
        ///  获取所有类别
        /// </summary>
        /// <param name="categoryName"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="showHidden"></param>
        /// <returns></returns>
        IPagedList<NewsCategory> GetAllCategories(int storeId, int userId, string categoryName = "", int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        /// <summary>
        /// 根据父级Id获取所有类别
        /// </summary>
        /// <param name="parentCategoryId"></param>
        /// <param name="showHidden"></param>
        /// <returns></returns>
        IList<NewsCategory> GetAllCategoriesByParentCategoryId(int storeId, int userId, int parentCategoryId, bool showHidden = false);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IList<NewsCategory> GetAllCategoriesDisplayedOnHomePage();

        /// <summary>
        ///  根据类别Id获取类别
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        NewsCategory GetCategoryById(int? store, int categoryId);

        string GetNewCategoryName(int? store, int categoryId);

        /// <summary>
        ///  根据Id分页获取
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="showHidden"></param>
        /// <returns></returns>
        IPagedList<NewsCategory> GetNewsCategoriesByCategoryId(int storeId, int userId, int categoryId, int pageIndex, int pageSize, bool showHidden = false);

        /// <summary>
        /// 根据消息获取类别
        /// </summary>
        /// <param name="newsItemtId"></param>
        /// <param name="showHidden"></param>
        /// <returns></returns>
        IList<NewsCategory> GetNewsCategoriesByNewsId(int storeId, int userId, int newsItemtId, bool showHidden = false);

        /// <summary>
        ///  根据类别Id获取
        /// </summary>
        /// <param name="newsCategoryId"></param>
        /// <returns></returns>
        NewsCategory GetNewsCategoryById(int newsCategoryId);

        /// <summary>
        ///  新增类别
        /// </summary>
        /// <param name="newscategory"></param>
        void InsertCategory(NewsCategory newscategory);

        /// <summary>
        /// 新增消息类别
        /// </summary>
        /// <param name="newsnewsCategory"></param>
        void InsertNewsCategory(NewsCategory newsnewsCategory);

        /// <summary>
        ///  更新类别
        /// </summary>
        /// <param name="newscategory"></param>
        void UpdateCategory(NewsCategory newscategory);

        /// <summary>
        ///  
        /// </summary>
        /// <param name="newscategory"></param>
        void UpdateHasDiscountsApplied(NewsCategory newscategory);

        /// <summary>
        ///  更新消息类别
        /// </summary>
        /// <param name="newsnewsCategory"></param>
        void UpdateNewsCategory(NewsCategory newsnewsCategory);

        /// <summary>
        /// 批量获取
        /// </summary>
        /// <param name="newsCategoryIds"></param>
        /// <returns></returns>
        IList<NewsCategory> GetNewsCategoriesByIds(int[] newsCategoryIds);
    }
}
