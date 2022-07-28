using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.News;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;

namespace DCMS.Services.News
{
    public partial class NewsService : BaseService, INewsService
    {
         public NewsService(
            IStaticCacheManager cacheManager,
            IServiceGetter getter,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
            
        }

        #region 新闻相关方法
        /// <summary>
        /// 删除新闻
        /// </summary>
        /// <param name="newsItem"></param>
        public virtual void DeleteNews(NewsItem newsItem)
        {
            if (newsItem == null)
            {
                throw new ArgumentNullException("newsItem");
            }

            var uow = NewsItemRepository.UnitOfWork;
            NewsItemRepository.Delete(newsItem);
            uow.SaveChanges();

            _eventPublisher.EntityDeleted(newsItem);
        }

        /// <summary>
        /// 获取具体一条新闻信息
        /// </summary>
        /// <param name="newsId"></param>
        /// <returns></returns>
        public virtual NewsItem GetNewsById(int newsId, int? storeId)
        {
            if (newsId == 0)
            {
                return null;
            }
            return NewsItemRepository.ToCachedGetById(newsId);
        }

        /// <summary>
        /// 获取上一条
        /// </summary>
        /// <param name="newsId"></param>
        /// <returns></returns>
        public virtual NewsItem GetPreNewsById(int newsId, int? storeId)
        {
            var query = NewsItemRepository.Table;
            if (newsId == 0)
            {
                return null;
            }

            query = query.Where(f => f.Id > newsId)
                  .OrderBy(o => o.Id);

            if (query.FirstOrDefault() == null)
            {
                return null;
            }

            return GetNewsById(query.FirstOrDefault().Id, storeId);
        }

        /// <summary>
        /// 获取下一条
        /// </summary>
        /// <param name="newsId"></param>
        /// <returns></returns>
        public virtual NewsItem GetNextNewsById(int newsId, int? storeId)
        {
            var query = NewsItemRepository.Table;
            if (newsId == 0)
            {
                return null;
            }

            query = query.Where(f => f.Id < newsId)
                 .OrderByDescending(o => o.Id);

            if (query.FirstOrDefault() == null)
            {
                return null;
            }

            return GetNewsById(query.FirstOrDefault().Id, storeId);
        }

        /// <summary>
        /// 获取新闻列表
        /// </summary>
        /// <param name="newsIds"></param>
        /// <returns></returns>
        public virtual IList<NewsItem> GetNewsByIds(int[] newsIds)
        {
            if (newsIds == null || newsIds.Length == 0)
            {
                return new List<NewsItem>();
            }

            var query = from p in NewsItemRepository.Table
                        where newsIds.Contains(p.Id)
                        select p;
            var newses = query.ToList();
            //sort by passed identifiers
            var sortedNews = new List<NewsItem>();
            foreach (int id in newsIds)
            {
                var news = newses.Find(x => x.Id == id);
                if (news != null)
                {
                    sortedNews.Add(news);
                }
            }
            return sortedNews;
        }

        public IList<NewsItem> GetNewsByCount(int num)
        {
            var query = NewsItemRepository.Table;
            if (num == 0)
            {
                return null;
            }

            query = query.OrderByDescending(o => o.CreatedOnUtc)
                .Take(num);
            return query.ToList();
        }

        /// <summary>
        /// 获取所有新闻
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="languageId"></param>
        /// <param name="storeId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="showHidden"></param>
        /// <returns></returns>
        public virtual IPagedList<NewsItem> GetAllNews(
            string title,
            int? categoryId,
            int languageId = 0,
            int storeId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue,  //Int32.MaxValue
            bool showHidden = false)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = NewsItemRepository.Table.Where(s => s.StoreId == 0);

            if (categoryId > 0)
            {
                query = query.Where(n => categoryId == n.NewsCategoryId);
            }

            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(x => x.Title.Contains(title));
            }

            query = query.OrderByDescending(n => n.CreatedOnUtc);

            ////Store mapping
            //query = from n in query
            //        join sm in StoreMappingRepository.Table
            //        on new { c1 = n.Id, c2 = "NewsItem" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into n_sm
            //        from sm in n_sm.DefaultIfEmpty()
            //        where !n.LimitedToStores || storeId == sm.StoreId
            //        select n;

            query = query.OrderByDescending(n => n.CreatedOnUtc);

            var news = new PagedList<NewsItem>(query, pageIndex, pageSize);

            return news;

        }

        /// <summary>
        /// 新增新闻消息
        /// </summary>
        /// <param name="news"></param>
        public virtual void InsertNews(NewsItem news)
        {
            if (news == null)
            {
                throw new ArgumentNullException("news");
            }

            var uow = NewsItemRepository.UnitOfWork;
            NewsItemRepository.Insert(news);
            uow.SaveChanges();

            _eventPublisher.EntityInserted(news);
        }

        /// <summary>
        /// 更新新闻消息
        /// </summary>
        /// <param name="news"></param>
        public virtual void UpdateNews(NewsItem news)
        {
            if (news == null)
            {
                throw new ArgumentNullException("news");
            }

            var uow = NewsItemRepository.UnitOfWork;
            NewsItemRepository.Update(news);
            uow.SaveChanges();

            _eventPublisher.EntityUpdated(news);
        }

        //#region 评论相关
        //public virtual IList<NewsComment> GetAllComments(int customerId)
        //{
        //    var query = from c in _newsCommentRepository.Table
        //                orderby c.CreatedOnUtc
        //                where (customerId == 0 || c.CustomerId == customerId)
        //                select c;
        //    var content = query.ToList();
        //    return content;
        //}


        //public virtual NewsComment GetNewsCommentById(int newsCommentId)
        //{
        //    if (newsCommentId == 0)
        //        return null;

        //    return _newsCommentRepository.ToCachedGetById(newsCommentId);
        //}


        //public virtual void DeleteNewsComment(NewsComment newsComment)
        //{
        //    if (newsComment == null)
        //        throw new ArgumentNullException("newsComment");

        //    _newsCommentRepository.Delete(newsComment);
        //}
        //#endregion

        #endregion

        #region 新闻图片

        /// <summary>
        /// 删除新闻图片
        /// </summary>
        /// <param name="newsPicture">News picture</param>
        public virtual void DeleteNewsPicture(NewsPicture newsPicture)
        {
            if (newsPicture == null)
            {
                throw new ArgumentNullException("newsPicture");
            }

            var uow = NewsPictureRepository.UnitOfWork;
            NewsPictureRepository.Delete(newsPicture);
            uow.SaveChanges();
            //event notification
            _eventPublisher.EntityDeleted(newsPicture);
        }

        /// <summary>
        /// 获取新闻图片列表
        /// </summary>
        /// <param name="newsId">The news identifier</param>
        /// <returns>News pictures</returns>
        public virtual IList<NewsPicture> GetNewsPicturesByNewsId(int newsId)
        {
            var query = from pp in NewsPictureRepository.Table
                        where pp.NewsItemId == newsId
                        orderby pp.DisplayOrder
                        select pp;
            var newsPictures = query.ToList();
            return newsPictures;
        }

        /// <summary>
        /// 获取一条新闻图片
        /// </summary>
        /// <param name="newsPictureId">News picture identifier</param>
        /// <returns>News picture</returns>
        public virtual NewsPicture GetNewsPictureById(int newsPictureId)
        {
            if (newsPictureId == 0)
            {
                return null;
            }

            var pp = NewsPictureRepository.ToCachedGetById(newsPictureId);
            return pp;
        }

        /// <summary>
        /// 添加新闻图片
        /// </summary>
        /// <param name="newsPicture">News picture</param>
        public virtual void InsertNewsPicture(NewsPicture newsPicture)
        {
            if (newsPicture == null)
            {
                throw new ArgumentNullException("newsPicture");
            }

            var uow = NewsPictureRepository.UnitOfWork;
            NewsPictureRepository.Insert(newsPicture);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityInserted(newsPicture);
        }

        /// <summary>
        /// 更新新闻图片
        /// </summary>
        /// <param name="newsPicture">News picture</param>
        public virtual void UpdateNewsPicture(NewsPicture newsPicture)
        {
            if (newsPicture == null)
            {
                throw new ArgumentNullException("newsPicture");
            }

            var uow = NewsPictureRepository.UnitOfWork;
            NewsPictureRepository.Update(newsPicture);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityUpdated(newsPicture);
        }

        #endregion
    }
}
