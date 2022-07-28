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
    /// 组织层级关系服务
    /// </summary>
    public partial class OrgsService : BaseService, IOrgsService
    {
        public OrgsService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
        }


        public virtual IPagedList<CRM_ORG> GetAllOrgs(string name = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;

            var query = CRM_ORGRepository.Table;
            query = query.OrderByDescending(c => c.Id);

            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<CRM_ORG>(plists, pageIndex, pageSize, totalCount);

        }
        public virtual CRM_ORG GetOrgById(int orgId)
        {
            if (orgId == 0)
            {
                return null;
            }

            return CRM_ORGRepository.ToCachedGetById(orgId);
        }
        public virtual IList<CRM_ORG> GetOrgsByIds(int[] sIds)
        {
            if (sIds == null || sIds.Length == 0)
            {
                return new List<CRM_ORG>();
            }

            var query = from c in CRM_ORGRepository.Table
                        where sIds.Contains(c.Id)
                        select c;
            var orgs = query.ToList();

            var sortedOrgs = new List<CRM_ORG>();
            foreach (int id in sIds)
            {
                var org = orgs.Find(x => x.Id == id);
                if (org != null)
                {
                    sortedOrgs.Add(org);
                }
            }

            return sortedOrgs;
        }

        public virtual void DeleteOrg(CRM_ORG org)
        {
            if (org == null)
            {
                throw new ArgumentNullException(nameof(org));
            }

            if (org is IEntityForCaching)
            {
                throw new ArgumentException("Cacheable entities are not supported by Entity Framework");
            }

            var uow = CRM_ORGRepository.UnitOfWork;
            CRM_ORGRepository.Delete(org);
            uow.SaveChanges();

            _eventPublisher.EntityDeleted(org);
        }
        public virtual void InsertOrg(CRM_ORG org)
        {
            if (org == null)
            {
                throw new ArgumentNullException("org");
            }

            var uow = CRM_ORGRepository.UnitOfWork;
            CRM_ORGRepository.Insert(org);
            uow.SaveChanges();
            _eventPublisher.EntityInserted(org);
        }
        public virtual void UpdateOrg(CRM_ORG org)
        {
            if (org == null)
            {
                throw new ArgumentNullException("org");
            }

            var uow = CRM_ORGRepository.UnitOfWork;
            CRM_ORGRepository.Update(org);
            uow.SaveChanges();
            _eventPublisher.EntityUpdated(org);
        }

        public virtual void DeleteOrg(IList<CRM_ORG> orgs)
        {
            if (orgs == null || !orgs.Any())
            {
                throw new ArgumentNullException(nameof(orgs));
            }

            var uow = CRM_ORGRepository.UnitOfWork;

            foreach (var s in orgs)
            {
                CRM_ORGRepository.Delete(s);
            }

            uow.SaveChanges();

            foreach (var s in orgs)
            {
                _eventPublisher.EntityDeleted(s);
            }

        }
        public virtual void InsertOrg(IList<CRM_ORG> orgs)
        {
            if (orgs == null || !orgs.Any())
            {
                throw new ArgumentNullException(nameof(orgs));
            }

            var uow = CRM_ORGRepository.UnitOfWork;

            foreach (var s in orgs)
            {
                CRM_ORGRepository.Insert(s);
            }

            uow.SaveChanges();

            foreach (var s in orgs)
            {
                _eventPublisher.EntityInserted(s);
            }
        }
        public virtual void UpdateOrg(IList<CRM_ORG> orgs)
        {
            if (orgs == null || !orgs.Any())
            {
                throw new ArgumentNullException(nameof(orgs));
            }

            var uow = CRM_ORGRepository.UnitOfWork;
            foreach (var s in orgs)
            {
                CRM_ORGRepository.Update(s);
            }

            uow.SaveChanges();

            foreach (var s in orgs)
            {
                _eventPublisher.EntityUpdated(s);
            }
        }
    }
}