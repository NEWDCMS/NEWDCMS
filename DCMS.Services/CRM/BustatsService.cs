using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.CRM;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Caching;
using DCMS.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DCMS.Services.CRM
{

    /// <summary>
    /// 终端业态配置表服务
    /// </summary>
    public partial class BustatsService : BaseService, IBustatsService
    {
        public BustatsService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
        }


        public virtual IPagedList<CRM_BUSTAT> GetAllBustats(string name = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;

            var query = CRM_BUSTATRepository.Table;
            query = query.OrderByDescending(c => c.Id);

            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<CRM_BUSTAT>(plists, pageIndex, pageSize, totalCount);

        }
        public virtual CRM_BUSTAT GetBustatById(int bustatId)
        {
            if (bustatId == 0)
            {
                return null;
            }

            return CRM_BUSTATRepository.ToCachedGetById(bustatId);
        }
        public virtual IList<CRM_BUSTAT> GetBustatsByIds(int[] sIds)
        {
            if (sIds == null || sIds.Length == 0)
            {
                return new List<CRM_BUSTAT>();
            }

            var query = from c in CRM_BUSTATRepository.Table
                        where sIds.Contains(c.Id)
                        select c;
            var bustats = query.ToList();

            var sortedBustats = new List<CRM_BUSTAT>();
            foreach (int id in sIds)
            {
                var bustat = bustats.Find(x => x.Id == id);
                if (bustat != null)
                {
                    sortedBustats.Add(bustat);
                }
            }

            return sortedBustats;
        }

        public virtual void DeleteBustat(CRM_BUSTAT bustat)
        {
            if (bustat == null)
            {
                throw new ArgumentNullException(nameof(bustat));
            }

            if (bustat is IEntityForCaching)
            {
                throw new ArgumentException("Cacheable entities are not supported by Entity Framework");
            }

            var uow = CRM_BUSTATRepository.UnitOfWork;
            CRM_BUSTATRepository.Delete(bustat);
            uow.SaveChanges();

            _eventPublisher.EntityDeleted(bustat);
        }
        public virtual void InsertBustat(CRM_BUSTAT bustat)
        {
            if (bustat == null)
            {
                throw new ArgumentNullException("bustat");
            }

            var uow = CRM_BUSTATRepository.UnitOfWork;
            CRM_BUSTATRepository.Insert(bustat);
            uow.SaveChanges();
            _eventPublisher.EntityInserted(bustat);
        }
        public virtual void UpdateBustat(CRM_BUSTAT bustat)
        {
            if (bustat == null)
            {
                throw new ArgumentNullException("bustat");
            }

            var uow = CRM_BUSTATRepository.UnitOfWork;
            CRM_BUSTATRepository.Update(bustat);
            uow.SaveChanges();
            _eventPublisher.EntityUpdated(bustat);
        }

        public virtual void DeleteBustat(IList<CRM_BUSTAT> bustats)
        {
            if (bustats == null || !bustats.Any())
            {
                throw new ArgumentNullException(nameof(bustats));
            }

            var uow = CRM_BUSTATRepository.UnitOfWork;

            foreach (var s in bustats)
            {
                CRM_BUSTATRepository.Delete(s);
            }

            uow.SaveChanges();

            foreach (var s in bustats)
            {
                _eventPublisher.EntityDeleted(s);
            }

        }
        public virtual void InsertBustat(IList<CRM_BUSTAT> bustats)
        {
            if (bustats == null || !bustats.Any())
            {
                throw new ArgumentNullException(nameof(bustats));
            }

            var uow = CRM_BUSTATRepository.UnitOfWork;

            foreach (var s in bustats)
            {
                CRM_BUSTATRepository.Insert(s);
            }

            uow.SaveChanges();

            foreach (var s in bustats)
            {
                _eventPublisher.EntityInserted(s);
            }
        }
        public virtual void UpdateBustat(IList<CRM_BUSTAT> bustats)
        {
            if (bustats == null || !bustats.Any())
            {
                throw new ArgumentNullException(nameof(bustats));
            }

            var uow = CRM_BUSTATRepository.UnitOfWork;
            foreach (var s in bustats)
            {
                CRM_BUSTATRepository.Update(s);
            }

            uow.SaveChanges();

            foreach (var s in bustats)
            {
                _eventPublisher.EntityUpdated(s);
            }
        }
    }
}