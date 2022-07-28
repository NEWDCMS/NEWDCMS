using DCMS.Core.Caching;
using DCMS.Core.Domain.Messages;
using DCMS.Core.Events;
using DCMS.Services.Events;

namespace DCMS.Web.Infrastructure.Cache
{
    public partial class MessagesCacheEventConsumer :
        //EmailAccount
        IConsumer<EntityInsertedEvent<EmailAccount>>,
         IConsumer<EntityUpdatedEvent<EmailAccount>>,
         IConsumer<EntityDeletedEvent<EmailAccount>>,

        //NewsLetterSubscription
        IConsumer<EntityInsertedEvent<NewsLetterSubscription>>,
         IConsumer<EntityUpdatedEvent<NewsLetterSubscription>>,
         IConsumer<EntityDeletedEvent<NewsLetterSubscription>>,

        //PrivateMessage
        IConsumer<EntityInsertedEvent<PrivateMessage>>,
         IConsumer<EntityUpdatedEvent<PrivateMessage>>,
         IConsumer<EntityDeletedEvent<PrivateMessage>>,

        //QueuedEmail
        IConsumer<EntityInsertedEvent<QueuedEmail>>,
         IConsumer<EntityUpdatedEvent<QueuedEmail>>,
         IConsumer<EntityDeletedEvent<QueuedEmail>>
    {

        protected readonly IStaticCacheManager _cacheManager;
        public MessagesCacheEventConsumer(IStaticCacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        #region EmailAccount
        public void HandleEvent(EntityInsertedEvent<EmailAccount> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<EmailAccount> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<EmailAccount> eventMessage)
        {
        }
        #endregion

        #region NewsLetterSubscription
        public void HandleEvent(EntityInsertedEvent<NewsLetterSubscription> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<NewsLetterSubscription> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<NewsLetterSubscription> eventMessage)
        {
        }
        #endregion

        #region PrivateMessage
        public void HandleEvent(EntityInsertedEvent<PrivateMessage> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<PrivateMessage> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<PrivateMessage> eventMessage)
        {
        }
        #endregion

        #region QueuedEmail
        public void HandleEvent(EntityInsertedEvent<QueuedEmail> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<QueuedEmail> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<QueuedEmail> eventMessage)
        {
        }
        #endregion


    }
}
