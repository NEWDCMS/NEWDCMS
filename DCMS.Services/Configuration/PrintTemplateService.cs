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
    //打印配置
    public partial class PrintTemplateService : BaseService, IPrintTemplateService
    {
        public PrintTemplateService(IStaticCacheManager cacheManager,
            IServiceGetter getter,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
        }


        #region 方法

        public virtual void DeletePrintTemplate(PrintTemplate printTemplates)
        {
            if (printTemplates == null)
            {
                throw new ArgumentNullException("printTemplates");
            }

            var uow = PrintTemplatesRepository.UnitOfWork;
            PrintTemplatesRepository.Delete(printTemplates);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityDeleted(printTemplates);
        }


        public virtual IPagedList<PrintTemplate> GetAllPrintTemplates(int? store, int? type, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = PrintTemplatesRepository.Table;


            if (store.HasValue && store.Value != 0)
            {
                query = query.Where(c => c.StoreId == store);
            }

            if (type.HasValue)
            {
                query = query.Where(c => c.TemplateType == type);
            }

            query = query.OrderByDescending(c => c.Id);
            //var printTemplates = new PagedList<PrintTemplate>(query.ToList(), pageIndex, pageSize);
            //return printTemplates;
            //总页数
            var totalCount = query.Count();
            var plists = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            return new PagedList<PrintTemplate>(plists, pageIndex, pageSize, totalCount);
        }


        public virtual IList<PrintTemplate> GetAllPrintTemplates(int? store)
        {
            var key = DCMSDefaults.PRINTTEMPLATE_ALL_KEY.FillCacheKey(store);
            return _cacheManager.Get(key, () =>
            {
                var query = from s in PrintTemplatesRepository.Table
                            where s.StoreId == store.Value
                            orderby s.Id
                            select s;
                var printTemplate = query.ToList();
                return printTemplate;
            });
        }

        public virtual PrintTemplate GetPrintTemplateById(int? store, int printTemplatesId)
        {
            if (printTemplatesId == 0)
            {
                return null;
            }
            return PrintTemplatesRepository.ToCachedGetById(printTemplatesId);
        }


        public virtual IList<PrintTemplate> GetPrintTemplatesByIds(int[] sIds)
        {
            if (sIds == null || sIds.Length == 0)
            {
                return new List<PrintTemplate>();
            }

            var query = from c in PrintTemplatesRepository.Table
                        where sIds.Contains(c.Id)
                        select c;
            var printTemplate = query.ToList();

            var sortedPrintTemplates = new List<PrintTemplate>();
            foreach (int id in sIds)
            {
                var printTemplates = printTemplate.Find(x => x.Id == id);
                if (printTemplates != null)
                {
                    sortedPrintTemplates.Add(printTemplates);
                }
            }
            return sortedPrintTemplates;
        }



        public virtual void InsertPrintTemplate(PrintTemplate printTemplates)
        {
            if (printTemplates == null)
            {
                throw new ArgumentNullException("printTemplates");
            }

            var uow = PrintTemplatesRepository.UnitOfWork;
            PrintTemplatesRepository.Insert(printTemplates);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityInserted(printTemplates);
        }


        public virtual void UpdatePrintTemplate(PrintTemplate printTemplates)
        {
            if (printTemplates == null)
            {
                throw new ArgumentNullException("printTemplates");
            }

            var uow = PrintTemplatesRepository.UnitOfWork;
            PrintTemplatesRepository.Update(printTemplates);
            uow.SaveChanges();

            //event notification
            _eventPublisher.EntityUpdated(printTemplates);
        }
        #endregion
    }
}