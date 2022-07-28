using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Events;
using DCMS.Services.Events;

namespace DCMS.Web.Infrastructure.Cache
{
    public partial class ConfigurationCacheEventConsumer :
        //AccountingType
        IConsumer<EntityInsertedEvent<AccountingType>>,
         IConsumer<EntityUpdatedEvent<AccountingType>>,
         IConsumer<EntityDeletedEvent<AccountingType>>,

        //AccountingOption
        IConsumer<EntityInsertedEvent<AccountingOption>>,
         IConsumer<EntityUpdatedEvent<AccountingOption>>,
         IConsumer<EntityDeletedEvent<AccountingOption>>,

        //PricingStructure
        IConsumer<EntityInsertedEvent<PricingStructure>>,
         IConsumer<EntityUpdatedEvent<PricingStructure>>,
         IConsumer<EntityDeletedEvent<PricingStructure>>,

        //PrintTemplate
         IConsumer<EntityInsertedEvent<PrintTemplate>>,
         IConsumer<EntityUpdatedEvent<PrintTemplate>>,
         IConsumer<EntityDeletedEvent<PrintTemplate>>,

         //RemarkConfig
         IConsumer<EntityInsertedEvent<RemarkConfig>>,
         IConsumer<EntityUpdatedEvent<RemarkConfig>>,
         IConsumer<EntityDeletedEvent<RemarkConfig>>,

         //settings
         IConsumer<EntityUpdatedEvent<Setting>>,

        //StockEarlyWarning
        IConsumer<EntityInsertedEvent<StockEarlyWarning>>,
         IConsumer<EntityUpdatedEvent<StockEarlyWarning>>,
         IConsumer<EntityDeletedEvent<StockEarlyWarning>>

    {

        protected readonly IStaticCacheManager _cacheManager;
        public ConfigurationCacheEventConsumer(IStaticCacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        #region AccountingType
        public void HandleEvent(EntityInsertedEvent<AccountingType> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.ACCOUNTTYPES_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<AccountingType> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.ACCOUNTTYPES_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<AccountingType> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.ACCOUNTTYPES_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region AccountingOption
        public void HandleEvent(EntityInsertedEvent<AccountingOption> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.ACCOUNTINGOPTION_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<AccountingOption> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.ACCOUNTINGOPTION_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<AccountingOption> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.ACCOUNTINGOPTION_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region PricingStructure
        public void HandleEvent(EntityInsertedEvent<PricingStructure> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRICESTRUCTURE_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<PricingStructure> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRICESTRUCTURE_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<PricingStructure> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRICESTRUCTURE_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region PrintTemplate
        public void HandleEvent(EntityInsertedEvent<PrintTemplate> eventMessage)
        {
            _cacheManager.Remove(new CacheKey(BaseEntity.GetEntityCacheKey(typeof(PrintTemplate), eventMessage.Entity.Id)));
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRINTTEMPLATE_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<PrintTemplate> eventMessage)
        {
            _cacheManager.Remove(new CacheKey(BaseEntity.GetEntityCacheKey(typeof(PrintTemplate), eventMessage.Entity.Id)));
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRINTTEMPLATE_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<PrintTemplate> eventMessage)
        {
            _cacheManager.Remove(new CacheKey(BaseEntity.GetEntityCacheKey(typeof(PrintTemplate), eventMessage.Entity.Id)));
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRINTTEMPLATE_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region RemarkConfig
        public void HandleEvent(EntityInsertedEvent<RemarkConfig> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.REMARKCONFIG_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<RemarkConfig> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.REMARKCONFIG_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<RemarkConfig> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.REMARKCONFIG_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region Setting
        public void HandleEvent(EntityUpdatedEvent<Setting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.SETTINGS_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region StockEarlyWarning
        public void HandleEvent(EntityInsertedEvent<StockEarlyWarning> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STOCKEARLYWARING_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<StockEarlyWarning> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STOCKEARLYWARING_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<StockEarlyWarning> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STOCKEARLYWARING_PK, eventMessage.Entity.StoreId));
        }
        #endregion

    }
}
