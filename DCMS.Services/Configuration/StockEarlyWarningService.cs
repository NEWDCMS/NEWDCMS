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
    //¿â´æÔ¤¾¯ÅäÖÃ
    public partial class StockEarlyWarningService : BaseService, IStockEarlyWarningService
    {
        public StockEarlyWarningService(IStaticCacheManager cacheManager,
            IServiceGetter getter,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
        }



        #region ·½·¨
        public virtual void DeleteStockEarlyWarning(StockEarlyWarning stockEarlyWarnings)
        {
            if (stockEarlyWarnings == null)
            {
                throw new ArgumentNullException("stockEarlyWarnings");
            }

            var uow = StockEarlyWarningsRepository.UnitOfWork;
            StockEarlyWarningsRepository.Delete(stockEarlyWarnings);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(stockEarlyWarnings);
        }


        public virtual IPagedList<StockEarlyWarning> GetAllStockEarlyWarnings(int? store, string name = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;
            var query = StockEarlyWarningsRepository.Table;

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(c => c.ProductName.Contains(name));
            }

            if (store.HasValue && store.Value != 0)
            {
                query = query.Where(c => c.StoreId == store);
            }

            query = query.OrderByDescending(c => c.CreatedOnUtc);
            //var stockEarlyWarnings = new PagedList<StockEarlyWarning>(query.ToList(), pageIndex, pageSize);
            //return stockEarlyWarnings;
            //×ÜÒ³Êý
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<StockEarlyWarning>(plists, pageIndex, pageSize, totalCount);
        }


        public virtual IList<StockEarlyWarning> GetAllStockEarlyWarnings(int? store)
        {
            var key = DCMSDefaults.STOCKEARLYWARING_ALL_KEY.FillCacheKey(store);
            return _cacheManager.Get(key, () =>
            {
                var query = from s in StockEarlyWarningsRepository.Table
                            where s.StoreId == store.Value
                            orderby s.CreatedOnUtc, s.ProductName
                            select s;
                var stockEarlyWarning = query.ToList();
                return stockEarlyWarning;
            });
        }

        public virtual StockEarlyWarning GetStockEarlyWarningById(int? store, int stockEarlyWarningsId)
        {
            if (stockEarlyWarningsId == 0)
            {
                return null;
            }

            return StockEarlyWarningsRepository.ToCachedGetById(stockEarlyWarningsId);
        }


        public virtual IList<StockEarlyWarning> GetStockEarlyWarningsByIds(int[] sIds)
        {
            if (sIds == null || sIds.Length == 0)
            {
                return new List<StockEarlyWarning>();
            }

            var query = from c in StockEarlyWarningsRepository.Table
                        where sIds.Contains(c.Id)
                        select c;
            var stockEarlyWarning = query.ToList();

            var sortedStockEarlyWarnings = new List<StockEarlyWarning>();
            foreach (int id in sIds)
            {
                var stockEarlyWarnings = stockEarlyWarning.Find(x => x.Id == id);
                if (stockEarlyWarnings != null)
                {
                    sortedStockEarlyWarnings.Add(stockEarlyWarnings);
                }
            }
            return sortedStockEarlyWarnings;
        }


        public virtual bool CheckExists(int productId, int wareHouseId)
        {
            var query = from c in StockEarlyWarningsRepository.Table
                        where c.ProductId == productId && c.WareHouseId == wareHouseId
                        select c;
            return query.ToList().Count() > 0;
        }

        public virtual bool CheckExists(int stockEarlyWarningsId, int productId, int wareHouseId)
        {
            var query = from c in StockEarlyWarningsRepository.Table
                        where c.Id != stockEarlyWarningsId && c.ProductId == productId && c.WareHouseId == wareHouseId
                        select c;
            return query.ToList().Count() > 0;
        }


        public virtual void InsertStockEarlyWarning(StockEarlyWarning stockEarlyWarnings)
        {
            if (stockEarlyWarnings == null)
            {
                throw new ArgumentNullException("stockEarlyWarnings");
            }

            var uow = StockEarlyWarningsRepository.UnitOfWork;
            StockEarlyWarningsRepository.Insert(stockEarlyWarnings);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityInserted(stockEarlyWarnings);
        }


        public virtual void UpdateStockEarlyWarning(StockEarlyWarning stockEarlyWarnings)
        {
            if (stockEarlyWarnings == null)
            {
                throw new ArgumentNullException("stockEarlyWarnings");
            }

            var uow = StockEarlyWarningsRepository.UnitOfWork;
            StockEarlyWarningsRepository.Update(stockEarlyWarnings);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityUpdated(stockEarlyWarnings);
        }

        #endregion
    }
}