using DCMS.Core;
using DCMS.Core.Domain.News;
using System.Collections.Generic;

namespace DCMS.Services.News
{
    /// <summary>
    /// 表示新闻服务接口
    /// </summary>
    public partial interface INewsService
    {
        #region 新闻相关
        /// <summary>
        /// 删除新闻
        /// </summary>
        /// <param name="newsItem">News item</param>
        void DeleteNews(NewsItem newsItem);

        /// <summary>
        /// 获取新闻
        /// </summary>
        /// <param name="newsId">The news identifier</param>
        /// <returns>News</returns>
        NewsItem GetNewsById(int newsId, int? storeId);

        /// <summary>
        /// 获取上一条
        /// </summary>
        /// <param name="newsId"></param>
        /// <returns></returns>
        NewsItem GetPreNewsById(int newsId, int? storeId);

        /// <summary>
        /// 获取下一条
        /// </summary>
        /// <param name="newsId"></param>
        /// <returns></returns>
        NewsItem GetNextNewsById(int newsId, int? storeId);

        /// <summary>
        /// 获取新闻列表
        /// </summary>
        /// <param name="newsIds"></param>
        /// <returns></returns>
        IList<NewsItem> GetNewsByIds(int[] newsIds);

        /// <summary>
        /// 获取指定条数新闻
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        IList<NewsItem> GetNewsByCount(int num);

        /// <summary>
        /// 获取全部新闻
        /// </summary>
        /// <param name="languageId">Language identifier; 0 if you want to get all records</param>
        /// <param name="storeId">Store identifier; 0 if you want to get all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>News items</returns>
        IPagedList<NewsItem> GetAllNews(
            string titile,
            int? categoryId,
             int languageId = 0,
             int storeId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue,  //Int32.MaxValue
            bool showHidden = false);


        /// <summary>
        /// 添加新闻
        /// </summary>
        /// <param name="news">News item</param>
        void InsertNews(NewsItem news);

        /// <summary>
        /// 更新新闻
        /// </summary>
        /// <param name="news">News item</param>
        void UpdateNews(NewsItem news);
        #endregion

        #region 评论相关
        /// <summary>
        /// 获取全部评论
        /// </summary>
        /// <param name="customerId">Customer identifier; 0 to load all records</param>
        /// <returns>Comments</returns>
        //IList<NewsComment> GetAllComments(int customerId);

        /// <summary>
        /// 获取一条评论
        /// </summary>
        /// <param name="newsCommentId">News comment identifier</param>
        /// <returns>News comment</returns>
        //NewsComment GetNewsCommentById(int newsCommentId);

        /// <summary>
        /// 删除一条评论 
        /// </summary>
        /// <param name="newsComment">News comment</param>
        //void DeleteNewsComment(NewsComment newsComment);
        #endregion

        #region 新闻图片

        ///删除图片
        void DeleteNewsPicture(NewsPicture newsPicture);
        /// <summary>
        /// 获取图片列表
        /// </summary>
        /// <param name="newsId"></param>
        /// <returns></returns>
        IList<NewsPicture> GetNewsPicturesByNewsId(int newsId);
        /// <summary>
        /// 获取具体图片
        /// </summary>
        /// <param name="newsPictureId"></param>
        /// <returns></returns>
        NewsPicture GetNewsPictureById(int newsPictureId);
        /// <summary>
        /// 新增图片
        /// </summary>
        /// <param name="newsPicture"></param>
        void InsertNewsPicture(NewsPicture newsPicture);
        /// <summary>
        /// 修改图片
        /// </summary>
        /// <param name="newsPicture"></param>
        void UpdateNewsPicture(NewsPicture newsPicture);

        #endregion
    }
}
