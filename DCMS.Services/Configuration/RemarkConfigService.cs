using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;

namespace DCMS.Services.Configuration
{
    //备注配置
    public partial class RemarkConfigService : BaseService, IRemarkConfigService
    {
        

        public RemarkConfigService(IStaticCacheManager cacheManager,
            IServiceGetter getter,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
            
        }

        #region 方法

        public virtual void DeleteRemarkConfig(RemarkConfig remarkConfigs)
        {
            if (remarkConfigs == null)
            {
                throw new ArgumentNullException("remarkConfigs");
            }

            var uow = RemarkConfigsRepository.UnitOfWork;
            RemarkConfigsRepository.Delete(remarkConfigs);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(remarkConfigs);
        }


        public virtual IPagedList<RemarkConfig> GetAllRemarkConfigs(int? store, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = RemarkConfigsRepository.Table;


            if (store.HasValue && store.Value != 0)
            {
                query = query.Where(c => c.StoreId == store);
            }

            query = query.OrderByDescending(c => c.Id);
            //var remarkConfigs = new PagedList<RemarkConfig>(query.ToList(), pageIndex, pageSize);
            //return remarkConfigs;
            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<RemarkConfig>(plists, pageIndex, pageSize, totalCount);
        }


        public virtual IList<RemarkConfig> GetAllRemarkConfigs(int? store)
        {
            var key = DCMSDefaults.REMARKCONFIG_ALL_KEY.FillCacheKey(store);
            return _cacheManager.Get(key, () =>
            {
                var query = from s in RemarkConfigsRepository.Table
                            where s.StoreId == store.Value
                            orderby s.Id
                            select s;
                var remarkConfig = query.ToList();
                return remarkConfig;
            });
        }

        public virtual RemarkConfig GetRemarkConfigById(int? store, int remarkConfigsId)
        {
            if (remarkConfigsId == 0)
            {
                return null;
            }
            return RemarkConfigsRepository.ToCachedGetById(remarkConfigsId);
        }


        public virtual IList<RemarkConfig> GetRemarkConfigsByIds(int[] sIds)
        {
            if (sIds == null || sIds.Length == 0)
            {
                return new List<RemarkConfig>();
            }

            var query = from c in RemarkConfigsRepository.Table
                        where sIds.Contains(c.Id)
                        select c;
            var remarkConfig = query.ToList();

            var sortedRemarkConfigs = new List<RemarkConfig>();
            foreach (int id in sIds)
            {
                var remarkConfigs = remarkConfig.Find(x => x.Id == id);
                if (remarkConfigs != null)
                {
                    sortedRemarkConfigs.Add(remarkConfigs);
                }
            }
            return sortedRemarkConfigs;
        }



        public virtual void InsertRemarkConfig(RemarkConfig remarkConfigs)
        {
            if (remarkConfigs == null)
            {
                throw new ArgumentNullException("remarkConfigs");
            }

            var uow = RemarkConfigsRepository.UnitOfWork;
            var temp = RemarkConfigsRepository.TableNoTracking.FirstOrDefault(s => s.StoreId == remarkConfigs.StoreId && s.Name == remarkConfigs.Name);
            if (temp == null)
            {
                RemarkConfigsRepository.Insert(remarkConfigs);
            }
            uow.SaveChanges();
            //event notification
            _eventPublisher.EntityInserted(remarkConfigs);
        }


        public virtual void UpdateRemarkConfig(RemarkConfig remarkConfigs)
        {
            if (remarkConfigs == null)
            {
                throw new ArgumentNullException("remarkConfigs");
            }

            var uow = RemarkConfigsRepository.UnitOfWork;
            var temp = RemarkConfigsRepository.TableNoTracking.FirstOrDefault(s => s.StoreId == remarkConfigs.StoreId && s.Name == remarkConfigs.Name);
            if (temp != null)
            {
                RemarkConfigsRepository.Update(remarkConfigs);
            }
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityUpdated(remarkConfigs);
        }

        /// <summary>
        /// 绑定备注信息
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public IList<RemarkConfig> BindRemarkConfigList(int? storeId)
        {
            return _cacheManager.Get(DCMSDefaults.BINDREMARKCONFIG_ALL_STORE_KEY.FillCacheKey(storeId), () =>
           {
               var query = from c in RemarkConfigsRepository.Table
                           select c;
               if (storeId != null)
               {
                   query = query.Where(c => c.StoreId == storeId);
               }

               var result = query.Select(q => new { q.Id, q.Name }).ToList().Select(x => new RemarkConfig { Id = x.Id, Name = x.Name }).ToList();
               return result;
           });
        }


        #endregion
    }
}