using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Terminals;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using DCMS.Services.Caching;

namespace DCMS.Services.Terminals
{
    /// <summary>
    /// 等级服务
    /// </summary>
    public partial class RankService : BaseService, IRankService
    {
        
        public RankService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
            
        }


        /// <summary>
        /// 获取经销商所有等级
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public virtual IList<Rank> GetAll(int? storeId)
        {
            return _cacheManager.Get(DCMSDefaults.RANK_ALL_STORE_KEY.FillCacheKey(storeId), () =>
           {
               var query = from c in RanksRepository.Table
                           orderby c.Name
                           where !c.Deleted
                           select c;
               if (storeId != null)
               {
                   query = query.Where(c => c.StoreId == storeId);
               }

               return query.ToList();
           });
        }

        /// <summary>
        /// 绑定经销商等级信息
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public virtual IList<Rank> BindRankList(int? storeId)
        {
            return _cacheManager.Get(DCMSDefaults.BINDRANK_ALL_STORE_KEY.FillCacheKey(storeId), () =>
           {
               var query = from c in RanksRepository.Table
                           where !c.Deleted
                           select c;
               if (storeId != null)
               {
                   query = query.Where(c => c.StoreId == storeId);
               }

               var result = query.Select(q => new { Id = q.Id, Name = q.Name }).ToList().Select(x => new Rank { Id = x.Id, Name = x.Name }).ToList();
               return result;
           });
        }


        /// <summary>
        /// 分页获取等级信息列表
        /// </summary>
        /// <param name="searchStr"></param>
        /// <param name="storeId">经销商Id</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual IPagedList<Rank> GetRanks(string searchStr, int? storeId, int pageIndex, int pageSize)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = from p in RanksRepository.Table
                        orderby p.Id descending
                        where !p.Deleted
                        select p;
            if (storeId != null)
            {
                query = query.Where(c => c.StoreId == storeId);
            }

            if (!string.IsNullOrEmpty(searchStr))
            {
                query = query.Where(t => t.Name.Contains(searchStr));
            }

            //return new PagedList<Rank>(query.ToList(), pageIndex, pageSize);
            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<Rank>(plists, pageIndex, pageSize, totalCount);
        }

        /// <summary>
        /// 根据主键Id获取等级
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual Rank GetRankById(int? store, int id)
        {
            if (id == 0)
            {
                return null;
            }

            return RanksRepository.ToCachedGetById(id);
        }

        public virtual int GetRankByName(int store, string name)
        {
            var query = RanksRepository.Table;
            if (string.IsNullOrWhiteSpace(name))
            {
                return 0;
            }

            return query.Where(s => s.Name == name).Select(s => s.Id).FirstOrDefault();
        }


        public virtual IList<Rank> GetRanksByIds(int? store, int[] idArr)
        {
            if (idArr == null || idArr.Length == 0)
            {
                return new List<Rank>();
            }

            var key = DCMSDefaults.RANK_BY_IDS_KEY.FillCacheKey(store ?? 0, idArr.OrderBy(a => a));
            return _cacheManager.Get(key, () =>
            {

                var query = from c in RanksRepository.Table
                            where idArr.Contains(c.Id)
                            select c;
                var list = query.ToList();
                return list;
            });


        }
        /// <summary>
        /// 新增等级
        /// </summary>
        /// <param name="rank"></param>
        public virtual void InsertRank(Rank rank)
        {
            if (rank == null)
            {
                throw new ArgumentNullException("rank");
            }

            var uow = RanksRepository.UnitOfWork;
            RanksRepository.Insert(rank);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityInserted(rank);
        }
        /// <summary>
        /// 删除等级
        /// </summary>
        /// <param name="rank"></param>
        public virtual void DeleteRank(Rank rank)
        {
            if (rank == null)
            {
                throw new ArgumentNullException("rank");
            }

            var uow = RanksRepository.UnitOfWork;
            RanksRepository.Delete(rank);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityDeleted(rank);
        }
        /// <summary>
        /// 修改等级
        /// </summary>
        /// <param name="rank"></param>
        public virtual void UpdateRank(Rank rank)
        {
            if (rank == null)
            {
                throw new ArgumentNullException("rank");
            }

            var uow = RanksRepository.UnitOfWork;
            RanksRepository.Update(rank);
            uow.SaveChanges();

            //通知
            _eventPublisher.EntityUpdated(rank);
        }

    }
}
