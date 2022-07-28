using DCMS.Core.Caching;
using DCMS.Core.Domain.Tax;
using DCMS.Core.Events;
using DCMS.Services.Events;

namespace DCMS.Web.Infrastructure.Cache
{
    public partial class TaxCacheEventConsumer :
        //TaxCategory
        IConsumer<EntityInsertedEvent<TaxCategory>>,
         IConsumer<EntityUpdatedEvent<TaxCategory>>,
         IConsumer<EntityDeletedEvent<TaxCategory>>
    {

        protected readonly IStaticCacheManager _cacheManager;
        public TaxCacheEventConsumer(IStaticCacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        #region TaxCategory
        public void HandleEvent(EntityInsertedEvent<TaxCategory> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<TaxCategory> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<TaxCategory> eventMessage)
        {
        }
        #endregion
    }
}
