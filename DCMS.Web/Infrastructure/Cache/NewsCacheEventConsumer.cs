using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.News;
using DCMS.Core.Events;
using DCMS.Services.Events;

namespace DCMS.Web.Infrastructure.Cache
{
    public partial class NewsCacheEventConsumer :
        //NewsCategory
        IConsumer<EntityInsertedEvent<NewsCategory>>,
         IConsumer<EntityUpdatedEvent<NewsCategory>>,
         IConsumer<EntityDeletedEvent<NewsCategory>>,

        //NewsItem
        IConsumer<EntityInsertedEvent<NewsItem>>,
         IConsumer<EntityUpdatedEvent<NewsItem>>,
         IConsumer<EntityDeletedEvent<NewsItem>>,

        //NewsPicture
        IConsumer<EntityInsertedEvent<NewsPicture>>,
         IConsumer<EntityUpdatedEvent<NewsPicture>>,
         IConsumer<EntityDeletedEvent<NewsPicture>>
    {

        protected readonly IStaticCacheManager _cacheManager;
        public NewsCacheEventConsumer(IStaticCacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        #region NewsCategory
        public void HandleEvent(EntityInsertedEvent<NewsCategory> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.NEWSCATEGORIES_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<NewsCategory> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.NEWSCATEGORIES_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<NewsCategory> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.NEWSCATEGORIES_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region NewsItem
        public void HandleEvent(EntityInsertedEvent<NewsItem> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<NewsItem> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<NewsItem> eventMessage)
        {
            ;
        }
        #endregion

        #region NewsPicture
        public void HandleEvent(EntityInsertedEvent<NewsPicture> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<NewsPicture> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<NewsPicture> eventMessage)
        {
            ;
        }
        #endregion
    }
}
