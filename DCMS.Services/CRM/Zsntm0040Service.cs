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
    /// 终端三级类型配置服务
    /// </summary>
    public partial class Zsntm0040Service : BaseService, IZsntm0040Service
    {
        public Zsntm0040Service(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
        }


        public virtual IPagedList<CRM_ZSNTM0040> GetAllZsntm0040s(string name = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;

            var query = CRM_ZSNTM0040Repository.Table;
            query = query.OrderByDescending(c => c.Id);

            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<CRM_ZSNTM0040>(plists, pageIndex, pageSize, totalCount);

        }
        public virtual CRM_ZSNTM0040 GetZsntm0040ById(int zsntm0040Id)
        {
            if (zsntm0040Id == 0)
            {
                return null;
            }

            return CRM_ZSNTM0040Repository.ToCachedGetById(zsntm0040Id);
        }
        public virtual IList<CRM_ZSNTM0040> GetZsntm0040sByIds(int[] sIds)
        {
            if (sIds == null || sIds.Length == 0)
            {
                return new List<CRM_ZSNTM0040>();
            }

            var query = from c in CRM_ZSNTM0040Repository.Table
                        where sIds.Contains(c.Id)
                        select c;
            var zsntm0040s = query.ToList();

            var sortedZsntm0040s = new List<CRM_ZSNTM0040>();
            foreach (int id in sIds)
            {
                var zsntm0040 = zsntm0040s.Find(x => x.Id == id);
                if (zsntm0040 != null)
                {
                    sortedZsntm0040s.Add(zsntm0040);
                }
            }

            return sortedZsntm0040s;
        }

        public virtual void DeleteZsntm0040(CRM_ZSNTM0040 zsntm0040)
        {
            if (zsntm0040 == null)
            {
                throw new ArgumentNullException(nameof(zsntm0040));
            }

            if (zsntm0040 is IEntityForCaching)
            {
                throw new ArgumentException("Cacheable entities are not supported by Entity Framework");
            }

            var uow = CRM_ZSNTM0040Repository.UnitOfWork;
            CRM_ZSNTM0040Repository.Delete(zsntm0040);
            uow.SaveChanges();

            _eventPublisher.EntityDeleted(zsntm0040);
        }
        public virtual void InsertZsntm0040(CRM_ZSNTM0040 zsntm0040)
        {
            if (zsntm0040 == null)
            {
                throw new ArgumentNullException("zsntm0040");
            }

            var uow = CRM_ZSNTM0040Repository.UnitOfWork;
            CRM_ZSNTM0040Repository.Insert(zsntm0040);
            uow.SaveChanges();
            _eventPublisher.EntityInserted(zsntm0040);
        }
        public virtual void UpdateZsntm0040(CRM_ZSNTM0040 zsntm0040)
        {
            if (zsntm0040 == null)
            {
                throw new ArgumentNullException("zsntm0040");
            }

            var uow = CRM_ZSNTM0040Repository.UnitOfWork;
            CRM_ZSNTM0040Repository.Update(zsntm0040);
            uow.SaveChanges();
            _eventPublisher.EntityUpdated(zsntm0040);
        }

        public virtual void DeleteZsntm0040(IList<CRM_ZSNTM0040> zsntm0040s)
        {
            if (zsntm0040s == null || !zsntm0040s.Any())
            {
                throw new ArgumentNullException(nameof(zsntm0040s));
            }

            var uow = CRM_ZSNTM0040Repository.UnitOfWork;

            foreach (var s in zsntm0040s)
            {
                CRM_ZSNTM0040Repository.Delete(s);
            }

            uow.SaveChanges();

            foreach (var s in zsntm0040s)
            {
                _eventPublisher.EntityDeleted(s);
            }

        }
        public virtual void InsertZsntm0040(IList<CRM_ZSNTM0040> zsntm0040s)
        {
            if (zsntm0040s == null || !zsntm0040s.Any())
            {
                throw new ArgumentNullException(nameof(zsntm0040s));
            }

            var uow = CRM_ZSNTM0040Repository.UnitOfWork;

            foreach (var s in zsntm0040s)
            {
                CRM_ZSNTM0040Repository.Insert(s);
            }

            uow.SaveChanges();

            foreach (var s in zsntm0040s)
            {
                _eventPublisher.EntityInserted(s);
            }
        }
        public virtual void UpdateZsntm0040(IList<CRM_ZSNTM0040> zsntm0040s)
        {
            if (zsntm0040s == null || !zsntm0040s.Any())
            {
                throw new ArgumentNullException(nameof(zsntm0040s));
            }

            var uow = CRM_ZSNTM0040Repository.UnitOfWork;
            foreach (var s in zsntm0040s)
            {
                CRM_ZSNTM0040Repository.Update(s);
            }

            uow.SaveChanges();

            foreach (var s in zsntm0040s)
            {
                _eventPublisher.EntityUpdated(s);
            }
        }
    }
}