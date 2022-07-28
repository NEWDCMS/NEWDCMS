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
    /// 终端制高点分类分级配置服务
    /// </summary>
    public partial class HeightConfsService : BaseService, IHeightConfsService
    {
        public HeightConfsService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
        }


        public virtual IPagedList<CRM_HEIGHT_CONF> GetAllHeightConfs(string name = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize >= 50)
                pageSize = 50;

            var query = CRM_HEIGHT_CONFRepository.Table;
            query = query.OrderByDescending(c => c.Id);

            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<CRM_HEIGHT_CONF>(plists, pageIndex, pageSize, totalCount);

        }
        public virtual CRM_HEIGHT_CONF GetHeightConfById(int heightConfId)
        {
            if (heightConfId == 0)
            {
                return null;
            }

            return CRM_HEIGHT_CONFRepository.ToCachedGetById(heightConfId);
        }
        public virtual IList<CRM_HEIGHT_CONF> GetHeightConfsByIds(int[] sIds)
        {
            if (sIds == null || sIds.Length == 0)
            {
                return new List<CRM_HEIGHT_CONF>();
            }

            var query = from c in CRM_HEIGHT_CONFRepository.Table
                        where sIds.Contains(c.Id)
                        select c;
            var heightConfs = query.ToList();

            var sortedHeightConfs = new List<CRM_HEIGHT_CONF>();
            foreach (int id in sIds)
            {
                var heightConf = heightConfs.Find(x => x.Id == id);
                if (heightConf != null)
                {
                    sortedHeightConfs.Add(heightConf);
                }
            }

            return sortedHeightConfs;
        }

        public virtual void DeleteHeightConf(CRM_HEIGHT_CONF heightConf)
        {
            if (heightConf == null)
            {
                throw new ArgumentNullException(nameof(heightConf));
            }

            if (heightConf is IEntityForCaching)
            {
                throw new ArgumentException("Cacheable entities are not supported by Entity Framework");
            }

            var uow = CRM_HEIGHT_CONFRepository.UnitOfWork;
            CRM_HEIGHT_CONFRepository.Delete(heightConf);
            uow.SaveChanges();

            _eventPublisher.EntityDeleted(heightConf);
        }
        public virtual void InsertHeightConf(CRM_HEIGHT_CONF heightConf)
        {
            if (heightConf == null)
            {
                throw new ArgumentNullException("heightConf");
            }

            var uow = CRM_HEIGHT_CONFRepository.UnitOfWork;
            CRM_HEIGHT_CONFRepository.Insert(heightConf);
            uow.SaveChanges();
            _eventPublisher.EntityInserted(heightConf);
        }
        public virtual void UpdateHeightConf(CRM_HEIGHT_CONF heightConf)
        {
            if (heightConf == null)
            {
                throw new ArgumentNullException("heightConf");
            }

            var uow = CRM_HEIGHT_CONFRepository.UnitOfWork;
            CRM_HEIGHT_CONFRepository.Update(heightConf);
            uow.SaveChanges();
            _eventPublisher.EntityUpdated(heightConf);
        }

        public virtual void DeleteHeightConf(IList<CRM_HEIGHT_CONF> heightConfs)
        {
            if (heightConfs == null || !heightConfs.Any())
            {
                throw new ArgumentNullException(nameof(heightConfs));
            }

            var uow = CRM_HEIGHT_CONFRepository.UnitOfWork;

            foreach (var s in heightConfs)
            {
                CRM_HEIGHT_CONFRepository.Delete(s);
            }

            uow.SaveChanges();

            foreach (var s in heightConfs)
            {
                _eventPublisher.EntityDeleted(s);
            }

        }
        public virtual void InsertHeightConf(IList<CRM_HEIGHT_CONF> heightConfs)
        {
            if (heightConfs == null || !heightConfs.Any())
            {
                throw new ArgumentNullException(nameof(heightConfs));
            }

            var uow = CRM_HEIGHT_CONFRepository.UnitOfWork;

            foreach (var s in heightConfs)
            {
                CRM_HEIGHT_CONFRepository.Insert(s);
            }

            uow.SaveChanges();

            foreach (var s in heightConfs)
            {
                _eventPublisher.EntityInserted(s);
            }
        }
        public virtual void UpdateHeightConf(IList<CRM_HEIGHT_CONF> heightConfs)
        {
            if (heightConfs == null || !heightConfs.Any())
            {
                throw new ArgumentNullException(nameof(heightConfs));
            }

            var uow = CRM_HEIGHT_CONFRepository.UnitOfWork;
            foreach (var s in heightConfs)
            {
                CRM_HEIGHT_CONFRepository.Update(s);
            }

            uow.SaveChanges();

            foreach (var s in heightConfs)
            {
                _eventPublisher.EntityUpdated(s);
            }
        }
    }
}