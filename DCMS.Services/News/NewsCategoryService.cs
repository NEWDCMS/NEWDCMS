using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.News;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using DCMS.Services.Security;
using DCMS.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;

namespace DCMS.Services.News
{

    public partial class NewsCategoryService : BaseService, INewsCategoryService
    {

        private readonly IStoreMappingService _storeMappingService;
        private readonly IAclService _aclService;



        public NewsCategoryService(IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher,
            IStoreMappingService storeMappingService,
            IAclService aclService,
            IServiceGetter getter) : base(getter, cacheManager, eventPublisher)
        {

            _storeMappingService = storeMappingService;
            _aclService = aclService;

        }

        #region 方法

        /// <summary>
        /// 删除类别
        /// </summary>
        /// <param name="newscategory"></param>
        public virtual void DeleteCategory(int storeId, int userId, NewsCategory newscategory)
        {
            if (newscategory == null)
            {
                throw new ArgumentNullException("newscategory");
            }

            newscategory.Deleted = true;
            UpdateCategory(newscategory);

            //将子元素的父Id置为0
            var subcategories = GetAllCategoriesByParentCategoryId(storeId, userId, newscategory.Id);
            foreach (var subcategory in subcategories)
            {
                subcategory.ParentId = 0;

                UpdateCategory(subcategory);
            }
        }

        /// <summary>
        /// 获取所有类别数据
        /// </summary>
        /// <param name="categoryName"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="showHidden"></param>
        /// <returns></returns>
        public virtual IPagedList<NewsCategory> GetAllCategories(int storeId, int userId, string categoryName = "", int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = NewsCategoryRepository.Table;

            query = query.Where(c => c.Deleted == false);

            if (storeId > 0)
            {
                query = query.Where(c => c.StoreId == storeId);
            }

            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                query = query.Where(c => c.Name.Contains(categoryName));
            }

            if (showHidden)
            {
                query = query.Where(c => c.ShowOnHomePage == true);
            }

            //分页
            return new PagedList<NewsCategory>(query.ToList(), pageIndex, pageSize);
        }

        /// <summary>
        /// 获取子类别
        /// </summary>
        /// <param name="parentCategoryId"></param>
        /// <param name="showHidden"></param>
        /// <returns></returns>
        public IList<NewsCategory> GetAllCategoriesByParentCategoryId(int storeId, int userId, int parentCategoryId,
            bool showHidden = false)
        {
            //string key = string.Format(NEWSCATEGORIES_BY_PARENT_CATEGORY_ID_KEY.FillCacheKey( parentCategoryId, showHidden, _workContext.CurrentUser.Id, _workContext.CurrentStore.Id);
            var key = DCMSDefaults.NEWSCATEGORIES_BY_PARENT_CATEGORY_ID_KEY.FillCacheKey(storeId, parentCategoryId, showHidden, userId);

            return _cacheManager.Get(key, () =>
            {
                var query = NewsCategoryRepository.Table;
                var acl_query = AclRecordRepository.Table.ToList();

                if (!showHidden)
                {
                    query = query.Where(c => c.Published);
                }

                query = query.Where(c => c.ParentId == parentCategoryId);
                query = query.Where(c => !c.Deleted);
                query = query.OrderBy(c => c.DisplayOrder);

                if (!showHidden)
                {
                    //ACL (access control list)
                    if (acl_query != null && acl_query.Count > 0)
                    {
                        //var allowedCustomerRolesIds = _workContext.CurrentUser.UserRoles
                        //.Where(cr => cr.Active).Select(cr => cr.Id).ToList();
                        //var allowedCustomerRolesIds = userId.UserRoles
                        //.Where(cr => cr.Active).Select(cr => cr.Id).ToList();

                        query = from c in query
                                join acl in AclRecordRepository.Table
                                on new { c1 = c.Id, c2 = "NewsCategory" } equals new { c1 = acl.EntityId, c2 = acl.EntityName } into c_acl
                                from acl in c_acl.DefaultIfEmpty()
                                where !c.SubjectToAcl //|| allowedCustomerRolesIds.Contains(acl.UserRoleId)
                                select c;
                    }

                    //Store mapping
                    //var currentStoreId = _workContext.CurrentStore.Id;
                    var currentStoreId = storeId;
                    query = from c in query
                            join sm in StoreMappingRepository.Table
                            on new { c1 = c.Id, c2 = "NewsCategory" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into c_sm
                            from sm in c_sm.DefaultIfEmpty()
                            where !c.LimitedToStores || currentStoreId == sm.StoreId
                            select c;

                    query = query.OrderBy(c => c.DisplayOrder);
                }

                var categories = query.ToList();
                return categories;
            });

        }

        /// <summary>
        /// 获取所有首页显示的消息类别
        /// </summary>
        /// <returns></returns>
        public virtual IList<NewsCategory> GetAllCategoriesDisplayedOnHomePage()
        {
            var query = from c in NewsCategoryRepository.Table
                        orderby c.DisplayOrder
                        where c.Published &&
                        !c.Deleted &&
                        c.ShowOnHomePage
                        select c;

            var categories = query.ToList();
            return categories;
        }


        public virtual NewsCategory GetCategoryById(int? store, int categoryId)
        {
            if (categoryId == 0)
            {
                return null;
            }

            return NewsCategoryRepository.ToCachedGetById(categoryId);
        }

        public virtual string GetNewCategoryName(int? store, int categoryId)
        {
            if (categoryId == 0)
            {
                return "";
            }

            var key = DCMSDefaults.NEWCATEGORY_NAME_BY_ID_KEY.FillCacheKey(store ?? 0, categoryId);
            return _cacheManager.Get(key, () =>
            {
                return NewsCategoryRepository.Table.Where(a => a.Id == categoryId).Select(a => a.Name).FirstOrDefault();
            });
        }


        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="newscategory"></param>
        public virtual void InsertCategory(NewsCategory newscategory)
        {
            if (newscategory == null)
            {
                throw new ArgumentNullException("newscategory");
            }

            var uow = NewsCategoryRepository.UnitOfWork;
            NewsCategoryRepository.Insert(newscategory);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityInserted(newscategory);
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="newscategory"></param>
        public virtual void UpdateCategory(NewsCategory newscategory)
        {
            if (newscategory == null)
            {
                throw new ArgumentNullException("newscategory");
            }

            //validate newscategory hierarchy
            var parentCategory = GetCategoryById(0, newscategory.ParentId ?? 0);
            while (parentCategory != null)
            {
                if (newscategory.Id == parentCategory.Id)
                {
                    newscategory.ParentId = 0;
                    break;
                }
                parentCategory = GetCategoryById(0, parentCategory.ParentId ?? 0);
            }

            var uow = NewsCategoryRepository.UnitOfWork;
            NewsCategoryRepository.Update(newscategory);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityUpdated(newscategory);
        }


        public virtual void UpdateHasDiscountsApplied(NewsCategory newscategory)
        {
            if (newscategory == null)
            {
                throw new ArgumentNullException("newscategory");
            }

            UpdateCategory(newscategory);
        }

        /// <summary>
        /// 删除新闻类别
        /// </summary>
        /// <param name="newsnewsCategory"></param>
        public virtual void DeleteNewsCategory(NewsCategory newsnewsCategory)
        {
            if (newsnewsCategory == null)
            {
                throw new ArgumentNullException("newsnewsCategory");
            }

            var uow = NewsCategoryRepository.UnitOfWork;
            NewsCategoryRepository.Delete(newsnewsCategory);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(newsnewsCategory);
        }


        public virtual IPagedList<NewsCategory> GetNewsCategoriesByCategoryId(int storeId, int userId, int categoryId, int pageIndex, int pageSize, bool showHidden = false)
        {
            if (pageSize >= 50)
                pageSize = 50;
            if (categoryId == 0)
            {
                return new PagedList<NewsCategory>(new List<NewsCategory>(), pageIndex, pageSize);
            }

            //string key = string.Format(NEWSNEWSCATEGORIES_ALLBYCATEGORYID_KEY.FillCacheKey( showHidden, categoryId, pageIndex, pageSize, _workContext.CurrentUser.Id, _workContext.CurrentStore.Id);
            var key = DCMSDefaults.NEWSNEWSCATEGORIES_ALLBYCATEGORYID_KEY.FillCacheKey(storeId, showHidden, categoryId, pageIndex, pageSize, userId);
            return _cacheManager.Get(key, () =>
            {
                //
                var query = from pc in NewsCategoryRepository.Table
                            join p in NewsItemRepository.Table on pc.NewsItemId equals p.Id
                            where pc.Id == categoryId && (showHidden || p.Published)
                            orderby pc.DisplayOrder
                            select pc;

                if (!showHidden)
                {
                    //ACL (access control list)
                    //var allowedCustomerRolesIds = _workContext.CurrentUser.UserRoles
                    //    .Where(cr => cr.Active).Select(cr => cr.Id).ToList();
                    //var allowedCustomerRolesIds = userId.UserRoles.Where(cr => cr.Active).Select(cr => cr.Id).ToList();
                    query = from pc in query
                            join c in NewsCategoryRepository.Table on pc.Id equals c.Id
                            join acl in AclRecordRepository.Table
                            on new { c1 = c.Id, c2 = "NewsCategory" } equals new { c1 = acl.EntityId, c2 = acl.EntityName } into c_acl
                            from acl in c_acl.DefaultIfEmpty()
                            where !c.SubjectToAcl //|| allowedCustomerRolesIds.Contains(acl.UserRoleId)
                            select pc;

                    //Store mapping
                    //var currentStoreId = _workContext.CurrentStore.Id;
                    var currentStoreId = storeId;
                    query = from pc in query
                            join c in NewsCategoryRepository.Table on pc.Id equals c.Id
                            join sm in StoreMappingRepository.Table
                            on new { c1 = c.Id, c2 = "NewsCategory" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into c_sm
                            from sm in c_sm.DefaultIfEmpty()
                            where /*!c.LimitedToStores ||*/ currentStoreId == sm.StoreId
                            select pc;


                    query = query.OrderBy(pc => pc.DisplayOrder);
                }

                var productCategories = new PagedList<NewsCategory>(query, pageIndex, pageSize);
                return productCategories;
            });
        }

        public virtual IList<NewsCategory> GetNewsCategoriesByNewsId(int storeId, int userId, int newsItemtId, bool showHidden = false)
        {
            if (newsItemtId == 0)
            {
                return new List<NewsCategory>();
            }

            //string key = string.Format(NEWSNEWSCATEGORIES_ALLBYPRODUCTID_KEY.FillCacheKey( showHidden, newsItemtId, _workContext.CurrentUser.Id, _workContext.CurrentStore.Id);
            var key = DCMSDefaults.NEWSNEWSCATEGORIES_ALLBYPRODUCTID_KEY.FillCacheKey(storeId, showHidden, newsItemtId, userId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pc in NewsCategoryRepository.Table
                            join c in NewsCategoryRepository.Table on pc.Id equals c.Id
                            where pc.NewsItemId == newsItemtId &&
                                  !c.Deleted &&
                                  (showHidden || c.Published)
                            orderby pc.DisplayOrder
                            select pc;

                var allNewsCategories = query.ToList();
                var result = new List<NewsCategory>();
                if (!showHidden)
                {
                    foreach (var pc in allNewsCategories)
                    {
                        //ACL(access control list) and store mapping
                        var newscategory = pc.NewsCategories;
                        if (_aclService.Authorize(newscategory) && _storeMappingService.Authorize(newscategory))
                        {
                            result.Add(pc);
                        }
                    }
                }
                else
                {
                    //no filtering
                    result.AddRange(allNewsCategories);
                }
                return result;
            });
        }


        public virtual NewsCategory GetNewsCategoryById(int productCategoryId)
        {
            if (productCategoryId == 0)
            {
                return null;
            }

            return NewsCategoryRepository.ToCachedGetById(productCategoryId);
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="newsnewsCategory"></param>
        public virtual void InsertNewsCategory(NewsCategory newsnewsCategory)
        {
            if (newsnewsCategory == null)
            {
                throw new ArgumentNullException("newsnewsCategory");
            }

            var uow = NewsCategoryRepository.UnitOfWork;
            NewsCategoryRepository.Insert(newsnewsCategory);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityInserted(newsnewsCategory);
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="newsnewsCategory"></param>
        public virtual void UpdateNewsCategory(NewsCategory newsnewsCategory)
        {
            if (newsnewsCategory == null)
            {
                throw new ArgumentNullException("newsnewsCategory");
            }

            var uow = NewsCategoryRepository.UnitOfWork;
            NewsCategoryRepository.Update(newsnewsCategory);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityUpdated(newsnewsCategory);
        }

        #endregion

        public IList<NewsCategory> GetNewsCategoriesByIds(int[] newsCategoryIds)
        {
            if (newsCategoryIds == null || newsCategoryIds.Length == 0)
            {
                return new List<NewsCategory>();
            }

            var query = from c in NewsCategoryRepository.Table
                        where newsCategoryIds.Contains(c.Id)
                        select c;
            var userGroups = query.ToList();
            //sort by passed identifiers
            var sortedNewsCategories = new List<NewsCategory>();
            foreach (int id in newsCategoryIds)
            {
                var newCategory = userGroups.Find(x => x.Id == id);
                if (newCategory != null)
                {
                    sortedNewsCategories.Add(newCategory);
                }
            }
            return sortedNewsCategories;
        }
    }
}
