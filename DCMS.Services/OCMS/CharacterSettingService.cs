using DCMS.Core.Caching;
using DCMS.Core.Infrastructure.DependencyManagement;
using DCMS.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCMS.Services.OCMS
{
    public partial class CharacterSettingService : BaseService, ICharacterSettingService
    {
        public CharacterSettingService(IServiceGetter getter,
            IStaticCacheManager cacheManager,
            IEventPublisher eventPublisher) : base(getter, cacheManager, eventPublisher)
        {
        }
        public bool Exists(string customerCode, int productId)
        {
            try
            {
                var rst = OCMS_CharacterSettingRepository_RO.Table.Where(w=>w.CUSTOMER_CODE == customerCode && w.MD_PRODUCT_ID == productId).Count();
                return rst > 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
