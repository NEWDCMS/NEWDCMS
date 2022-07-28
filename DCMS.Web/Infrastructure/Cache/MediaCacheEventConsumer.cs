using DCMS.Core.Caching;
using DCMS.Core.Domain.Media;
using DCMS.Core.Events;
using DCMS.Services.Events;

namespace DCMS.Web.Infrastructure.Cache
{
    public partial class MediaCacheEventConsumer :
        //Download
        IConsumer<EntityInsertedEvent<Download>>,
         IConsumer<EntityUpdatedEvent<Download>>,
         IConsumer<EntityDeletedEvent<Download>>,

        //Picture
        IConsumer<EntityInsertedEvent<Picture>>,
         IConsumer<EntityUpdatedEvent<Picture>>,
         IConsumer<EntityDeletedEvent<Picture>>,

        //PictureBinary
        IConsumer<EntityInsertedEvent<PictureBinary>>,
         IConsumer<EntityUpdatedEvent<PictureBinary>>,
         IConsumer<EntityDeletedEvent<PictureBinary>>
    {

        protected readonly IStaticCacheManager _cacheManager;
        public MediaCacheEventConsumer(IStaticCacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        #region Download
        public void HandleEvent(EntityInsertedEvent<Download> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<Download> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<Download> eventMessage)
        {
        }
        #endregion

        #region Picture
        public void HandleEvent(EntityInsertedEvent<Picture> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<Picture> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<Picture> eventMessage)
        {
        }
        #endregion

        #region PictureBinary
        public void HandleEvent(EntityInsertedEvent<PictureBinary> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<PictureBinary> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<PictureBinary> eventMessage)
        {
        }
        #endregion


    }
}
