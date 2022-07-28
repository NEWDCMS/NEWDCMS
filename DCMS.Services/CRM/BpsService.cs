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
    /// 业务员级组织岗位信息服务
    /// </summary>
    public partial class BpsService : BaseService, IBpsService
    {
        public BpsService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
        }


        public virtual IPagedList<CRM_BP> GetAllBpss(string name = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;

            var query = CRM_BPRepository.Table;
            query = query.OrderByDescending(c => c.Id);

            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<CRM_BP>(plists, pageIndex, pageSize, totalCount);

        }
        public virtual CRM_BP GetBpsById(int bpId)
        {
            if (bpId == 0)
            {
                return null;
            }

            return CRM_BPRepository.ToCachedGetById(bpId);
        }
        public virtual IList<CRM_BP> GetBpssByIds(int[] sIds)
        {
            if (sIds == null || sIds.Length == 0)
            {
                return new List<CRM_BP>();
            }

            var query = from c in CRM_BPRepository.Table
                        where sIds.Contains(c.Id)
                        select c;
            var bps = query.ToList();

            var sortedBpss = new List<CRM_BP>();
            foreach (int id in sIds)
            {
                var bp = bps.Find(x => x.Id == id);
                if (bp != null)
                {
                    sortedBpss.Add(bp);
                }
            }

            return sortedBpss;
        }

        public virtual void DeleteBps(CRM_BP bp)
        {
            if (bp == null)
            {
                throw new ArgumentNullException(nameof(bp));
            }

            if (bp is IEntityForCaching)
            {
                throw new ArgumentException("Cacheable entities are not supported by Entity Framework");
            }

            var uow = CRM_BPRepository.UnitOfWork;
            CRM_BPRepository.Delete(bp);
            uow.SaveChanges();

            _eventPublisher.EntityDeleted(bp);
        }
        public virtual void InsertBps(CRM_BP bp)
        {
            if (bp == null)
            {
                throw new ArgumentNullException("bp");
            }

            var uow = CRM_BPRepository.UnitOfWork;
            CRM_BPRepository.Insert(bp);
            uow.SaveChanges();
            _eventPublisher.EntityInserted(bp);
        }
        public virtual void UpdateBps(CRM_BP bp)
        {
            if (bp == null)
            {
                throw new ArgumentNullException("bp");
            }

            var uow = CRM_BPRepository.UnitOfWork;
            CRM_BPRepository.Update(bp);
            uow.SaveChanges();
            _eventPublisher.EntityUpdated(bp);
        }

        public virtual void DeleteBps(IList<CRM_BP> bps)
        {
            if (bps == null || !bps.Any())
            {
                throw new ArgumentNullException(nameof(bps));
            }

            var uow = CRM_BPRepository.UnitOfWork;

            foreach (var s in bps)
            {
                CRM_BPRepository.Delete(s);
            }

            uow.SaveChanges();

            foreach (var s in bps)
            {
                _eventPublisher.EntityDeleted(s);
            }

        }
        public virtual void InsertBps(IList<CRM_BP> bps)
        {
            if (bps == null || !bps.Any())
            {
                throw new ArgumentNullException(nameof(bps));
            }

            var uow = CRM_BPRepository.UnitOfWork;

            foreach (var s in bps)
            {
                CRM_BPRepository.Insert(s);
            }

            uow.SaveChanges();

            foreach (var s in bps)
            {
                _eventPublisher.EntityInserted(s);
            }
        }
        public virtual void UpdateBps(IList<CRM_BP> bps)
        {
            if (bps == null || !bps.Any())
            {
                throw new ArgumentNullException(nameof(bps));
            }

            var uow = CRM_BPRepository.UnitOfWork;
            foreach (var s in bps)
            {
                CRM_BPRepository.Update(s);
            }

            uow.SaveChanges();

            foreach (var s in bps)
            {
                _eventPublisher.EntityUpdated(s);
            }
        }
    }
}