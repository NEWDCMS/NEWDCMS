using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Finances;
using DCMS.Core.Domain.Media;
using DCMS.Core.Domain.News;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Security;
using DCMS.Core.Domain.Users;
using DCMS.Core.Domain.WareHouses;
using DCMS.Core.Events;
using DCMS.Services.Events;


namespace DCMS.Web.Infrastructure.Cache
{
    /// <summary>
    /// 缓存移除，用于模型缓存订阅消费
    /// </summary>
    public partial class ModelCacheEventConsumer :

         //settings
         IConsumer<EntityUpdatedEvent<Setting>>,

         //manufacturers
         IConsumer<EntityInsertedEvent<Manufacturer>>,
         IConsumer<EntityUpdatedEvent<Manufacturer>>,
         IConsumer<EntityDeletedEvent<Manufacturer>>,

         //product manufacturers
         IConsumer<EntityInsertedEvent<ProductManufacturer>>,
         IConsumer<EntityUpdatedEvent<ProductManufacturer>>,
         IConsumer<EntityDeletedEvent<ProductManufacturer>>,

         //categories
         IConsumer<EntityInsertedEvent<Category>>,
         IConsumer<EntityUpdatedEvent<Category>>,
         IConsumer<EntityDeletedEvent<Category>>,

         //product categories
         IConsumer<EntityInsertedEvent<ProductCategory>>,
         IConsumer<EntityUpdatedEvent<ProductCategory>>,
         IConsumer<EntityDeletedEvent<ProductCategory>>,

         //products
         IConsumer<EntityInsertedEvent<Product>>,
         IConsumer<EntityUpdatedEvent<Product>>,
         IConsumer<EntityDeletedEvent<Product>>,

         //specification attributes
         IConsumer<EntityUpdatedEvent<SpecificationAttribute>>,
         IConsumer<EntityDeletedEvent<SpecificationAttribute>>,

         //specification attribute options
         IConsumer<EntityUpdatedEvent<SpecificationAttributeOption>>,
         IConsumer<EntityDeletedEvent<SpecificationAttributeOption>>,

         //Product specification attribute
         IConsumer<EntityInsertedEvent<ProductSpecificationAttribute>>,
         IConsumer<EntityUpdatedEvent<ProductSpecificationAttribute>>,
         IConsumer<EntityDeletedEvent<ProductSpecificationAttribute>>,

         //Picture
         IConsumer<EntityInsertedEvent<Picture>>,
         IConsumer<EntityUpdatedEvent<Picture>>,
         IConsumer<EntityDeletedEvent<Picture>>,

         //news items
         IConsumer<EntityInsertedEvent<NewsItem>>,
         IConsumer<EntityUpdatedEvent<NewsItem>>,
         IConsumer<EntityDeletedEvent<NewsItem>>,

         IConsumer<EntityInsertedEvent<User>>,
         IConsumer<EntityUpdatedEvent<User>>,
         IConsumer<EntityDeletedEvent<User>>,

         IConsumer<EntityInsertedEvent<Stock>>,
         IConsumer<EntityUpdatedEvent<Stock>>,
         IConsumer<EntityDeletedEvent<Stock>>,

         IConsumer<EntityInsertedEvent<AclRecord>>,
         IConsumer<EntityUpdatedEvent<AclRecord>>,
         IConsumer<EntityDeletedEvent<AclRecord>>,

         //AccountingType
         IConsumer<EntityInsertedEvent<AccountingType>>,
         IConsumer<EntityUpdatedEvent<AccountingType>>,
         IConsumer<EntityDeletedEvent<AccountingType>>

    {

        protected readonly IStaticCacheManager _cacheManager;

        public ModelCacheEventConsumer(IStaticCacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        //
        #region
        public void HandleEvent(EntityInsertedEvent<AclRecord> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.ACLRECORD_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<AclRecord> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.ACLRECORD_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<AclRecord> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.ACLRECORD_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region
        public void HandleEvent(EntityUpdatedEvent<Setting> eventMessage)
        {
        }
        #endregion

        #region
        public void HandleEvent(EntityInsertedEvent<Manufacturer> eventMessage)
        {


        }
        public void HandleEvent(EntityUpdatedEvent<Manufacturer> eventMessage)
        {

        }
        public void HandleEvent(EntityDeletedEvent<Manufacturer> eventMessage)
        {

        }

        #endregion

        #region
        public void HandleEvent(EntityInsertedEvent<ProductManufacturer> eventMessage)
        {

        }
        public void HandleEvent(EntityUpdatedEvent<ProductManufacturer> eventMessage)
        {

        }
        public void HandleEvent(EntityDeletedEvent<ProductManufacturer> eventMessage)
        {

        }

        #endregion

        #region
        public void HandleEvent(EntityInsertedEvent<Category> eventMessage)
        {


        }
        public void HandleEvent(EntityUpdatedEvent<Category> eventMessage)
        {

        }
        public void HandleEvent(EntityDeletedEvent<Category> eventMessage)
        {


        }
        #endregion

        #region
        public void HandleEvent(EntityInsertedEvent<ProductCategory> eventMessage)
        {

        }
        public void HandleEvent(EntityUpdatedEvent<ProductCategory> eventMessage)
        {

        }
        public void HandleEvent(EntityDeletedEvent<ProductCategory> eventMessage)
        {

        }

        #endregion

        #region
        public void HandleEvent(EntityInsertedEvent<Product> eventMessage)
        {

        }
        public void HandleEvent(EntityUpdatedEvent<Product> eventMessage)
        {

        }
        public void HandleEvent(EntityDeletedEvent<Product> eventMessage)
        {

        }
        #endregion

        #region
        public void HandleEvent(EntityUpdatedEvent<SpecificationAttribute> eventMessage)
        {

        }
        public void HandleEvent(EntityDeletedEvent<SpecificationAttribute> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<SpecificationAttributeOption> eventMessage)
        {

        }
        public void HandleEvent(EntityDeletedEvent<SpecificationAttributeOption> eventMessage)
        {

        }
        #endregion

        #region
        public void HandleEvent(EntityInsertedEvent<ProductSpecificationAttribute> eventMessage)
        {

        }
        public void HandleEvent(EntityUpdatedEvent<ProductSpecificationAttribute> eventMessage)
        {

        }
        public void HandleEvent(EntityDeletedEvent<ProductSpecificationAttribute> eventMessage)
        {

        }
        #endregion

        #region
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

        #region
        public void HandleEvent(EntityInsertedEvent<ProductPicture> eventMessage)
        {


        }
        public void HandleEvent(EntityUpdatedEvent<ProductPicture> eventMessage)
        {


        }
        public void HandleEvent(EntityDeletedEvent<ProductPicture> eventMessage)
        {


        }
        #endregion

        #region
        public void HandleEvent(EntityInsertedEvent<NewsItem> eventMessage)
        {

        }
        public void HandleEvent(EntityUpdatedEvent<NewsItem> eventMessage)
        {

        }
        public void HandleEvent(EntityDeletedEvent<NewsItem> eventMessage)
        {

        }

        #endregion

        #region
        public void HandleEvent(EntityInsertedEvent<User> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSUserServiceDefaults.UserCacheKey);
        }
        public void HandleEvent(EntityUpdatedEvent<User> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSUserServiceDefaults.UserCacheKey);
        }
        public void HandleEvent(EntityDeletedEvent<User> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSUserServiceDefaults.UserCacheKey);
        }

        #endregion

        #region
        public void HandleEvent(EntityInsertedEvent<TrialBalance> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.ACCOUNTINGOPTION_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<TrialBalance> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.ACCOUNTINGOPTION_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<TrialBalance> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.ACCOUNTINGOPTION_PK, eventMessage.Entity.StoreId));
        }

        #endregion

        #region
        public void HandleEvent(EntityInsertedEvent<Stock> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STOCK_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<Stock> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STOCK_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<Stock> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STOCK_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region
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
    }
}