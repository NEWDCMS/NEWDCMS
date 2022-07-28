using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Products;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;


namespace DCMS.Services.Products
{
    /// <summary>
    /// 统计类别服务
    /// </summary>
    public partial class StatisticalTypeService : BaseService, IStatisticalTypeService
    {


        public StatisticalTypeService(IStaticCacheManager cacheManager,
            IServiceGetter getter,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {

        }


        #region 方法


        /// <summary>
        ///  删除
        /// </summary>
        /// <param name="statisticalTypes"></param>
        public virtual void DeleteStatisticalTypes(StatisticalTypes statisticalTypes)
        {
            if (statisticalTypes == null)
            {
                throw new ArgumentNullException("statisticalTypes");
            }

            var uow = StatisticalTypesRepository.UnitOfWork;
            StatisticalTypesRepository.Delete(statisticalTypes);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(statisticalTypes);
        }

        /// <summary>
        /// 获取全部类别
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual IPagedList<StatisticalTypes> GetAllStatisticalTypess(int? store, string name = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = StatisticalTypesRepository.Table;

            if (store.HasValue && store.Value > 0)
            {
                query = query.Where(c => c.StoreId == store);
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(c => c.Name.Contains(name));
            }

            query = query.OrderByDescending(c => c.CreatedOnUtc);

            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<StatisticalTypes>(plists, pageIndex, pageSize, totalCount);

        }


        /// <summary>
        /// 获取全部
        /// </summary>
        /// <returns></returns>
        public virtual IList<StatisticalTypes> GetAllStatisticalTypess(int? store)
        {
            var key = DCMSDefaults.STATISTICALTYPE_ALL_KEY.FillCacheKey(store ?? 0);
            return _cacheManager.Get(key, () =>
            {
                var query = from s in StatisticalTypesRepository.Table
                            where s.StoreId == store.Value
                            orderby s.CreatedOnUtc, s.Name
                            select s;
                var statisticalType = query.ToList();
                return statisticalType;
            });
        }

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <returns></returns>
        public virtual IList<StatisticalTypes> GetAllStatisticalTypess()
        {
            var key = DCMSDefaults.STATISTICALTYPE_ALL_KEY.FillCacheKey(0);
            return _cacheManager.Get(key, () =>
            {
                var query = from s in StatisticalTypesRepository.Table
                            orderby s.CreatedOnUtc, s.Name
                            select s;
                var statisticalType = query.ToList();
                return statisticalType;
            });
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="statisticalTypesId"></param>
        /// <returns></returns>
        public virtual StatisticalTypes GetStatisticalTypesById(int? store, int statisticalTypesId)
        {
            if (statisticalTypesId == 0)
            {
                return null;
            }

            return StatisticalTypesRepository.ToCachedGetById(statisticalTypesId);
        }


        public virtual IList<StatisticalTypes> GetStatisticalTypessByIds(int[] sIds)
        {
            if (sIds == null || sIds.Length == 0)
            {
                return new List<StatisticalTypes>();
            }

            var query = from c in StatisticalTypesRepository.Table
                        where sIds.Contains(c.Id)
                        select c;
            var statisticalType = query.ToList();

            var sortedStatisticalTypess = new List<StatisticalTypes>();
            foreach (int id in sIds)
            {
                var statisticalTypes = statisticalType.Find(x => x.Id == id);
                if (statisticalTypes != null)
                {
                    sortedStatisticalTypess.Add(statisticalTypes);
                }
            }
            return sortedStatisticalTypess;
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="statisticalTypes"></param>
        public virtual void InsertStatisticalTypes(StatisticalTypes statisticalTypes)
        {
            if (statisticalTypes == null)
            {
                throw new ArgumentNullException("statisticalTypes");
            }

            var uow = StatisticalTypesRepository.UnitOfWork;
            StatisticalTypesRepository.Insert(statisticalTypes);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityInserted(statisticalTypes);
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="statisticalTypes"></param>
        public virtual void UpdateStatisticalTypes(StatisticalTypes statisticalTypes)
        {
            if (statisticalTypes == null)
            {
                throw new ArgumentNullException("statisticalTypes");
            }

            var uow = StatisticalTypesRepository.UnitOfWork;
            StatisticalTypesRepository.Update(statisticalTypes);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityUpdated(statisticalTypes);
        }

        #endregion
    }
}