using DCMS.Core.Caching;
using DCMS.Core.Domain.Discounts;
using DCMS.Core.Events;
using DCMS.Services.Events;

namespace DCMS.Web.Infrastructure.Cache
{
    public partial class DiscountsCacheEventConsumer :
        //Discount
        IConsumer<EntityInsertedEvent<Discount>>,
         IConsumer<EntityUpdatedEvent<Discount>>,
         IConsumer<EntityDeletedEvent<Discount>>,

        //DiscountCategoryMapping
        IConsumer<EntityInsertedEvent<DiscountCategoryMapping>>,
         IConsumer<EntityUpdatedEvent<DiscountCategoryMapping>>,
         IConsumer<EntityDeletedEvent<DiscountCategoryMapping>>,

        //DiscountManufacturerMapping
        IConsumer<EntityInsertedEvent<DiscountManufacturerMapping>>,
         IConsumer<EntityUpdatedEvent<DiscountManufacturerMapping>>,
         IConsumer<EntityDeletedEvent<DiscountManufacturerMapping>>,

        //DiscountProductMapping
        IConsumer<EntityInsertedEvent<DiscountProductMapping>>,
         IConsumer<EntityUpdatedEvent<DiscountProductMapping>>,
         IConsumer<EntityDeletedEvent<DiscountProductMapping>>,

        //DiscountRequirement
        IConsumer<EntityInsertedEvent<DiscountRequirement>>,
         IConsumer<EntityUpdatedEvent<DiscountRequirement>>,
         IConsumer<EntityDeletedEvent<DiscountRequirement>>,

        //DiscountUsageHistory
        IConsumer<EntityInsertedEvent<DiscountUsageHistory>>,
         IConsumer<EntityUpdatedEvent<DiscountUsageHistory>>,
         IConsumer<EntityDeletedEvent<DiscountUsageHistory>>
    {

        protected readonly IStaticCacheManager _cacheManager;
        public DiscountsCacheEventConsumer(IStaticCacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        #region Discount
        public void HandleEvent(EntityInsertedEvent<Discount> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<Discount> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<Discount> eventMessage)
        {
        }
        #endregion

        #region DiscountCategoryMapping
        public void HandleEvent(EntityInsertedEvent<DiscountCategoryMapping> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<DiscountCategoryMapping> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<DiscountCategoryMapping> eventMessage)
        {
        }
        #endregion

        #region DiscountManufacturerMapping
        public void HandleEvent(EntityInsertedEvent<DiscountManufacturerMapping> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<DiscountManufacturerMapping> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<DiscountManufacturerMapping> eventMessage)
        {
        }
        #endregion

        #region DiscountProductMapping
        public void HandleEvent(EntityInsertedEvent<DiscountProductMapping> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<DiscountProductMapping> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<DiscountProductMapping> eventMessage)
        {
        }
        #endregion

        #region DiscountRequirement
        public void HandleEvent(EntityInsertedEvent<DiscountRequirement> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<DiscountRequirement> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<DiscountRequirement> eventMessage)
        {
        }
        #endregion

        #region DiscountUsageHistory
        public void HandleEvent(EntityInsertedEvent<DiscountUsageHistory> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<DiscountUsageHistory> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<DiscountUsageHistory> eventMessage)
        {
        }
        #endregion
    }
}
