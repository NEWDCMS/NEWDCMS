using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Common;
using DCMS.Core.Events;
using DCMS.Services.Events;

namespace DCMS.Web.Infrastructure.Cache
{
    public partial class CommonCacheEventConsumer :
        //AddressSettings

        //GenericAttribute
        IConsumer<EntityInsertedEvent<GenericAttribute>>,
         IConsumer<EntityUpdatedEvent<GenericAttribute>>,
         IConsumer<EntityDeletedEvent<GenericAttribute>>,

        //SearchTerm
        IConsumer<EntityInsertedEvent<SearchTerm>>,
         IConsumer<EntityUpdatedEvent<SearchTerm>>,
         IConsumer<EntityDeletedEvent<SearchTerm>>
    {
        protected readonly IStaticCacheManager _cacheManager;

        public CommonCacheEventConsumer(IStaticCacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        #region AddressSettings

        #endregion

        #region GenericAttribute
        public void HandleEvent(EntityInsertedEvent<GenericAttribute> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.GENERICATTRIBUTE_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<GenericAttribute> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.GENERICATTRIBUTE_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<GenericAttribute> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.GENERICATTRIBUTE_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region SearchTerm
        public void HandleEvent(EntityInsertedEvent<SearchTerm> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<SearchTerm> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<SearchTerm> eventMessage)
        {
        }
        #endregion
    }
}
