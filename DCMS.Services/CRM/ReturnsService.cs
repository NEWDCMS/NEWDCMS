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
    /// 区域返回信息结构服务
    /// </summary>
    public partial class ReturnsService : BaseService, IReturnsService
    {
        public ReturnsService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
        }


        public virtual IPagedList<CRM_RETURN> GetAllReturns(string name = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;

            var query = CRM_RETURNRepository.Table;
            query = query.OrderByDescending(c => c.Id);

            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<CRM_RETURN>(plists, pageIndex, pageSize, totalCount);

        }
        public virtual CRM_RETURN GetReturnById(int returnId)
        {
            if (returnId == 0)
            {
                return null;
            }

            return CRM_RETURNRepository.ToCachedGetById(returnId);
        }
        public virtual IList<CRM_RETURN> GetReturnsByIds(int[] sIds)
        {
            if (sIds == null || sIds.Length == 0)
            {
                return new List<CRM_RETURN>();
            }

            var query = from c in CRM_RETURNRepository.Table
                        where sIds.Contains(c.Id)
                        select c;
            var returns = query.ToList();

            var sortedReturns = new List<CRM_RETURN>();
            foreach (int id in sIds)
            {
                var @return = returns.Find(x => x.Id == id);
                if (@return != null)
                {
                    sortedReturns.Add(@return);
                }
            }

            return sortedReturns;
        }

        public virtual void DeleteReturn(CRM_RETURN @return)
        {
            if (@return == null)
            {
                throw new ArgumentNullException(nameof(@return));
            }

            if (@return is IEntityForCaching)
            {
                throw new ArgumentException("Cacheable entities are not supported by Entity Framework");
            }

            var uow = CRM_RETURNRepository.UnitOfWork;
            CRM_RETURNRepository.Delete(@return);
            uow.SaveChanges();

            _eventPublisher.EntityDeleted(@return);
        }
        public virtual void InsertReturn(CRM_RETURN @return)
        {
            if (@return == null)
            {
                throw new ArgumentNullException("return");
            }

            var uow = CRM_RETURNRepository.UnitOfWork;
            CRM_RETURNRepository.Insert(@return);
            uow.SaveChanges();
            _eventPublisher.EntityInserted(@return);
        }
        public virtual void UpdateReturn(CRM_RETURN @return)
        {
            if (@return == null)
            {
                throw new ArgumentNullException("return");
            }

            var uow = CRM_RETURNRepository.UnitOfWork;
            CRM_RETURNRepository.Update(@return);
            uow.SaveChanges();
            _eventPublisher.EntityUpdated(@return);
        }

        public virtual void DeleteReturn(IList<CRM_RETURN> returns)
        {
            if (returns == null || !returns.Any())
            {
                throw new ArgumentNullException(nameof(returns));
            }

            var uow = CRM_RETURNRepository.UnitOfWork;

            foreach (var s in returns)
            {
                CRM_RETURNRepository.Delete(s);
            }

            uow.SaveChanges();

            foreach (var s in returns)
            {
                _eventPublisher.EntityDeleted(s);
            }

        }
        public virtual void InsertReturn(IList<CRM_RETURN> returns)
        {
            if (returns == null || !returns.Any())
            {
                throw new ArgumentNullException(nameof(returns));
            }

            var uow = CRM_RETURNRepository.UnitOfWork;

            foreach (var s in returns)
            {
                CRM_RETURNRepository.Insert(s);
            }

            uow.SaveChanges();

            foreach (var s in returns)
            {
                _eventPublisher.EntityInserted(s);
            }
        }
        public virtual void UpdateReturn(IList<CRM_RETURN> returns)
        {
            if (returns == null || !returns.Any())
            {
                throw new ArgumentNullException(nameof(returns));
            }

            var uow = CRM_RETURNRepository.UnitOfWork;
            foreach (var s in returns)
            {
                CRM_RETURNRepository.Update(s);
            }

            uow.SaveChanges();

            foreach (var s in returns)
            {
                _eventPublisher.EntityUpdated(s);
            }
        }
    }
}