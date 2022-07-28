using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Stores;
using DCMS.Core.Events;
using DCMS.Services.Events;

namespace DCMS.Web.Infrastructure.Cache
{
    public partial class StoresCacheEventConsumer :
        //Store
        IConsumer<EntityInsertedEvent<Store>>,
         IConsumer<EntityUpdatedEvent<Store>>,
         IConsumer<EntityDeletedEvent<Store>>,

        //StoreMapping
        IConsumer<EntityInsertedEvent<StoreMapping>>,
         IConsumer<EntityUpdatedEvent<StoreMapping>>,
         IConsumer<EntityDeletedEvent<StoreMapping>>
    {

        protected readonly IStaticCacheManager _cacheManager;
        public StoresCacheEventConsumer(IStaticCacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        #region Store
        public void HandleEvent(EntityInsertedEvent<Store> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STORES_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<Store> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STORES_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<Store> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STORES_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region StoreMapping
        public void HandleEvent(EntityInsertedEvent<StoreMapping> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STORES_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<StoreMapping> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STORES_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<StoreMapping> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STORES_PK, eventMessage.Entity.StoreId));
        }
        #endregion
    }
}
