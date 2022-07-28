using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Logging;
using DCMS.Core.Events;
using DCMS.Services.Events;

namespace DCMS.Web.Infrastructure.Cache
{
    public partial class LoggingCacheEventConsumer :
        //ActivityLog
        IConsumer<EntityInsertedEvent<ActivityLog>>,
         IConsumer<EntityUpdatedEvent<ActivityLog>>,
         IConsumer<EntityDeletedEvent<ActivityLog>>,

        //ActivityLogType
        IConsumer<EntityInsertedEvent<ActivityLogType>>,
         IConsumer<EntityUpdatedEvent<ActivityLogType>>,
         IConsumer<EntityDeletedEvent<ActivityLogType>>,

         //Log
         IConsumer<EntityInsertedEvent<Log>>,
         IConsumer<EntityUpdatedEvent<Log>>,
         IConsumer<EntityDeletedEvent<Log>>
    {

        protected readonly IStaticCacheManager _cacheManager;
        public LoggingCacheEventConsumer(IStaticCacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        #region ActivityLog
        public void HandleEvent(EntityInsertedEvent<ActivityLog> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<ActivityLog> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<ActivityLog> eventMessage)
        {
        }
        #endregion

        #region ActivityLogType
        public void HandleEvent(EntityInsertedEvent<ActivityLogType> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.ACTIVITYTYPE_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<ActivityLogType> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.ACTIVITYTYPE_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<ActivityLogType> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.ACTIVITYTYPE_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region Log
        public void HandleEvent(EntityInsertedEvent<Log> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<Log> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<Log> eventMessage)
        {
        }
        #endregion
    }
}
