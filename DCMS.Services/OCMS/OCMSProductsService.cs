using DCMS.Core.Caching;
using DCMS.Core.Domain.OCMS;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCMS.Services.OCMS
{
    public partial class OCMSProductsService : BaseService, IOCMSProductsService
    {
        public OCMSProductsService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
        }

        public OCMS_Products FindByCode(string code)
        {
            try
            {
                var entity = OCMS_ProductsRepository_RO.Table.Where(w => w.PRODUCT_CODE == code).FirstOrDefault();
                return entity;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }
    }
}
