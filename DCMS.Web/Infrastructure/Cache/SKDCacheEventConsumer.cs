using DCMS.Core.Caching;
using DCMS.Core.Domain.Tasks;
using DCMS.Core.Events;
using DCMS.Services.Events;

namespace DCMS.Web.Infrastructure.Cache
{
    public partial class SKDCacheEventConsumer :
        //QueuedMessage
        IConsumer<EntityInsertedEvent<QueuedMessage>>,
         IConsumer<EntityUpdatedEvent<QueuedMessage>>,
         IConsumer<EntityDeletedEvent<QueuedMessage>>,

        //MessageStructure
        IConsumer<EntityInsertedEvent<MessageStructure>>,
         IConsumer<EntityUpdatedEvent<MessageStructure>>,
         IConsumer<EntityDeletedEvent<MessageStructure>>,

        //ScheduleTask
        IConsumer<EntityInsertedEvent<ScheduleTask>>,
         IConsumer<EntityUpdatedEvent<ScheduleTask>>,
         IConsumer<EntityDeletedEvent<ScheduleTask>>
    {

        protected readonly IStaticCacheManager _cacheManager;
        public SKDCacheEventConsumer(IStaticCacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        #region QueuedMessage
        public void HandleEvent(EntityInsertedEvent<QueuedMessage> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<QueuedMessage> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<QueuedMessage> eventMessage)
        {
        }
        #endregion

        #region MessageStructure
        public void HandleEvent(EntityInsertedEvent<MessageStructure> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<MessageStructure> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<MessageStructure> eventMessage)
        {
        }
        #endregion

        #region ScheduleTask
        public void HandleEvent(EntityInsertedEvent<ScheduleTask> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<ScheduleTask> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<ScheduleTask> eventMessage)
        {
        }
        #endregion
    }
}
