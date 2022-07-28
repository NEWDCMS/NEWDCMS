using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Census;
using DCMS.Core.Domain.Terminals;
using DCMS.Core.Domain.Visit;
using DCMS.Core.Events;
using DCMS.Services.Events;

namespace DCMS.Web.Infrastructure.Cache
{
    public partial class CensusCacheEventConsumer :
         //Channel
         IConsumer<EntityInsertedEvent<Channel>>,
         IConsumer<EntityUpdatedEvent<Channel>>,
         IConsumer<EntityDeletedEvent<Channel>>,

        //District
        IConsumer<EntityInsertedEvent<District>>,
         IConsumer<EntityUpdatedEvent<District>>,
         IConsumer<EntityDeletedEvent<District>>,

        //Rank
        IConsumer<EntityInsertedEvent<Rank>>,
         IConsumer<EntityUpdatedEvent<Rank>>,
         IConsumer<EntityDeletedEvent<Rank>>,

        //Receivable
        IConsumer<EntityInsertedEvent<Receivable>>,
         IConsumer<EntityUpdatedEvent<Receivable>>,
         IConsumer<EntityDeletedEvent<Receivable>>,

        //ReceivableDetail
        IConsumer<EntityInsertedEvent<ReceivableDetail>>,
         IConsumer<EntityUpdatedEvent<ReceivableDetail>>,
         IConsumer<EntityDeletedEvent<ReceivableDetail>>,

        //Terminal
        IConsumer<EntityInsertedEvent<Terminal>>,
         IConsumer<EntityUpdatedEvent<Terminal>>,
         IConsumer<EntityDeletedEvent<Terminal>>,

        //LineTier
        IConsumer<EntityInsertedEvent<LineTier>>,
         IConsumer<EntityUpdatedEvent<LineTier>>,
         IConsumer<EntityDeletedEvent<LineTier>>,

        //DisplayPhoto
        IConsumer<EntityInsertedEvent<DisplayPhoto>>,
         IConsumer<EntityUpdatedEvent<DisplayPhoto>>,
         IConsumer<EntityDeletedEvent<DisplayPhoto>>,

        //DoorheadPhoto
        IConsumer<EntityInsertedEvent<DoorheadPhoto>>,
         IConsumer<EntityUpdatedEvent<DoorheadPhoto>>,
         IConsumer<EntityDeletedEvent<DoorheadPhoto>>,

        //Restaurant
        IConsumer<EntityInsertedEvent<Restaurant>>,
         IConsumer<EntityUpdatedEvent<Restaurant>>,
         IConsumer<EntityDeletedEvent<Restaurant>>,

        //SalesProduct
        IConsumer<EntityInsertedEvent<SalesProduct>>,
         IConsumer<EntityUpdatedEvent<SalesProduct>>,
         IConsumer<EntityDeletedEvent<SalesProduct>>,

        //Tradition
        IConsumer<EntityInsertedEvent<Tradition>>,
         IConsumer<EntityUpdatedEvent<Tradition>>,
         IConsumer<EntityDeletedEvent<Tradition>>,

        //VisitBase
        IConsumer<EntityInsertedEvent<VisitBase>>,
         IConsumer<EntityUpdatedEvent<VisitBase>>,
         IConsumer<EntityDeletedEvent<VisitBase>>,

        //VisitStore
        IConsumer<EntityInsertedEvent<VisitStore>>,
         IConsumer<EntityUpdatedEvent<VisitStore>>,
         IConsumer<EntityDeletedEvent<VisitStore>>,

        //Tracking
        IConsumer<EntityInsertedEvent<Tracking>>,
         IConsumer<EntityUpdatedEvent<Tracking>>,
         IConsumer<EntityDeletedEvent<Tracking>>
    {

        protected readonly IStaticCacheManager _cacheManager;
        public CensusCacheEventConsumer(IStaticCacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        #region Channel
        public void HandleEvent(EntityInsertedEvent<Channel> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.CHANNEL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<Channel> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.CHANNEL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<Channel> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.CHANNEL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region District
        public void HandleEvent(EntityInsertedEvent<District> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.DISTRICT_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<District> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.DISTRICT_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<District> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.DISTRICT_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region Rank
        public void HandleEvent(EntityInsertedEvent<Rank> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.RANK_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<Rank> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.RANK_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<Rank> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.RANK_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region Receivable
        public void HandleEvent(EntityInsertedEvent<Receivable> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.RECEIVABLE_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<Receivable> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.RECEIVABLE_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<Receivable> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.RECEIVABLE_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region ReceivableDetail
        public void HandleEvent(EntityInsertedEvent<ReceivableDetail> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.RECEIVABLEDETAIL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<ReceivableDetail> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.RECEIVABLEDETAIL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<ReceivableDetail> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.RECEIVABLEDETAIL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region Terminal
        public void HandleEvent(EntityInsertedEvent<Terminal> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.TERMINAL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<Terminal> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.TERMINAL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<Terminal> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.TERMINAL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region LineTier
        public void HandleEvent(EntityInsertedEvent<LineTier> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.LINETIER_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<LineTier> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.LINETIER_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<LineTier> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.LINETIER_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region DisplayPhoto
        public void HandleEvent(EntityInsertedEvent<DisplayPhoto> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<DisplayPhoto> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<DisplayPhoto> eventMessage)
        {
        }
        #endregion

        #region DoorheadPhoto
        public void HandleEvent(EntityInsertedEvent<DoorheadPhoto> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<DoorheadPhoto> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<DoorheadPhoto> eventMessage)
        {
        }
        #endregion

        #region Restaurant
        public void HandleEvent(EntityInsertedEvent<Restaurant> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<Restaurant> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<Restaurant> eventMessage)
        {
        }
        #endregion

        #region SalesProduct
        public void HandleEvent(EntityInsertedEvent<SalesProduct> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<SalesProduct> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<SalesProduct> eventMessage)
        {
        }
        #endregion

        #region Tradition
        public void HandleEvent(EntityInsertedEvent<Tradition> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<Tradition> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<Tradition> eventMessage)
        {
        }
        #endregion

        #region VisitBase
        public void HandleEvent(EntityInsertedEvent<VisitBase> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<VisitBase> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<VisitBase> eventMessage)
        {
        }
        #endregion

        #region VisitStore
        public void HandleEvent(EntityInsertedEvent<VisitStore> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<VisitStore> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<VisitStore> eventMessage)
        {
        }
        #endregion

        #region Tracking
        public void HandleEvent(EntityInsertedEvent<Tracking> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.TRACKING_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<Tracking> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.TRACKING_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<Tracking> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.TRACKING_PK, eventMessage.Entity.StoreId));
        }
        #endregion

    }
}
