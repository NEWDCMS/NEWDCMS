using DCMS.Core.Caching;
using DCMS.Core.Domain.Seo;
using DCMS.Core.Events;
using DCMS.Services.Events;

namespace DCMS.Web.Infrastructure.Cache
{
    public partial class SeoCacheEventConsumer :
        //UrlRecord
        IConsumer<EntityInsertedEvent<UrlRecord>>,
         IConsumer<EntityUpdatedEvent<UrlRecord>>,
         IConsumer<EntityDeletedEvent<UrlRecord>>
    {

        protected readonly IStaticCacheManager _cacheManager;
        public SeoCacheEventConsumer(IStaticCacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        #region UrlRecord
        public void HandleEvent(EntityInsertedEvent<UrlRecord> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<UrlRecord> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<UrlRecord> eventMessage)
        {
        }
        #endregion
    }
}
