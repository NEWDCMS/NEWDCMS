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
    /// 经销商终端关系服务
    /// </summary>
    public partial class RelationsService : BaseService, IRelationsService
    {
        public RelationsService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
        }


        public virtual IPagedList<CRM_RELATION> GetAllRelations(string name = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;

            var query = CRM_RELATIONRepository.Table;
            query = query.OrderByDescending(c => c.Id);

            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<CRM_RELATION>(plists, pageIndex, pageSize, totalCount);

        }
        public virtual CRM_RELATION GetRelationById(int relationId)
        {
            if (relationId == 0)
            {
                return null;
            }

            return CRM_RELATIONRepository.ToCachedGetById(relationId);
        }
        public virtual IList<CRM_RELATION> GetRelationsByIds(int[] sIds)
        {
            if (sIds == null || sIds.Length == 0)
            {
                return new List<CRM_RELATION>();
            }

            var query = from c in CRM_RELATIONRepository.Table
                        where sIds.Contains(c.Id)
                        select c;
            var relations = query.ToList();

            var sortedRelations = new List<CRM_RELATION>();
            foreach (int id in sIds)
            {
                var relation = relations.Find(x => x.Id == id);
                if (relation != null)
                {
                    sortedRelations.Add(relation);
                }
            }

            return sortedRelations;
        }

        public virtual void DeleteRelation(CRM_RELATION relation)
        {
            if (relation == null)
            {
                throw new ArgumentNullException(nameof(relation));
            }

            if (relation is IEntityForCaching)
            {
                throw new ArgumentException("Cacheable entities are not supported by Entity Framework");
            }

            var uow = CRM_RELATIONRepository.UnitOfWork;
            CRM_RELATIONRepository.Delete(relation);
            uow.SaveChanges();

            _eventPublisher.EntityDeleted(relation);
        }
        public virtual void InsertRelation(CRM_RELATION relation)
        {
            if (relation == null)
            {
                throw new ArgumentNullException("relation");
            }

            var uow = CRM_RELATIONRepository.UnitOfWork;
            CRM_RELATIONRepository.Insert(relation);
            uow.SaveChanges();
            _eventPublisher.EntityInserted(relation);
        }
        public virtual void UpdateRelation(CRM_RELATION relation)
        {
            if (relation == null)
            {
                throw new ArgumentNullException("relation");
            }

            var uow = CRM_RELATIONRepository.UnitOfWork;
            CRM_RELATIONRepository.Update(relation);
            uow.SaveChanges();
            _eventPublisher.EntityUpdated(relation);
        }

        public virtual void DeleteRelation(IList<CRM_RELATION> relations)
        {
            if (relations == null || !relations.Any())
            {
                throw new ArgumentNullException(nameof(relations));
            }

            var uow = CRM_RELATIONRepository.UnitOfWork;

            foreach (var s in relations)
            {
                CRM_RELATIONRepository.Delete(s);
            }

            uow.SaveChanges();

            foreach (var s in relations)
            {
                _eventPublisher.EntityDeleted(s);
            }

        }
        public virtual void InsertRelation(IList<CRM_RELATION> relations)
        {
            if (relations == null || !relations.Any())
            {
                throw new ArgumentNullException(nameof(relations));
            }

            var uow = CRM_RELATIONRepository.UnitOfWork;

            foreach (var s in relations)
            {
                CRM_RELATIONRepository.Insert(s);
            }

            uow.SaveChanges();

            foreach (var s in relations)
            {
                _eventPublisher.EntityInserted(s);
            }
        }
        public virtual void UpdateRelation(IList<CRM_RELATION> relations)
        {
            if (relations == null || !relations.Any())
            {
                throw new ArgumentNullException(nameof(relations));
            }

            var uow = CRM_RELATIONRepository.UnitOfWork;
            foreach (var s in relations)
            {
                CRM_RELATIONRepository.Update(s);
            }

            uow.SaveChanges();

            foreach (var s in relations)
            {
                _eventPublisher.EntityUpdated(s);
            }
        }
    }
}