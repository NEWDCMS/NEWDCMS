using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Campaigns;
using DCMS.Core.Domain.Plan;
using DCMS.Core.Domain.Products;
using DCMS.Core.Events;
using DCMS.Services.Events;

namespace DCMS.Web.Infrastructure.Cache
{
    public partial class ArchivesCacheEventConsumer :
        //Campaign
        IConsumer<EntityInsertedEvent<Campaign>>,
         IConsumer<EntityUpdatedEvent<Campaign>>,
         IConsumer<EntityDeletedEvent<Campaign>>,

         //PercentagePlan
         IConsumer<EntityInsertedEvent<PercentagePlan>>,
         IConsumer<EntityUpdatedEvent<PercentagePlan>>,
         IConsumer<EntityDeletedEvent<PercentagePlan>>,

        //Brand
        IConsumer<EntityInsertedEvent<Brand>>,
         IConsumer<EntityUpdatedEvent<Brand>>,
         IConsumer<EntityDeletedEvent<Brand>>,

        //Category
        IConsumer<EntityInsertedEvent<Category>>,
         IConsumer<EntityUpdatedEvent<Category>>,
         IConsumer<EntityDeletedEvent<Category>>,

        //GiveQuota
        IConsumer<EntityInsertedEvent<GiveQuota>>,
         IConsumer<EntityUpdatedEvent<GiveQuota>>,
         IConsumer<EntityDeletedEvent<GiveQuota>>,

        //Manufacturer
        IConsumer<EntityInsertedEvent<Manufacturer>>,
         IConsumer<EntityUpdatedEvent<Manufacturer>>,
         IConsumer<EntityDeletedEvent<Manufacturer>>,

        //Product
        IConsumer<EntityInsertedEvent<Product>>,
         IConsumer<EntityUpdatedEvent<Product>>,
         IConsumer<EntityDeletedEvent<Product>>,

        //ProductAttribute
        IConsumer<EntityInsertedEvent<ProductAttribute>>,
         IConsumer<EntityUpdatedEvent<ProductAttribute>>,
         IConsumer<EntityDeletedEvent<ProductAttribute>>,

        //ProductCategory
        IConsumer<EntityInsertedEvent<ProductCategory>>,
         IConsumer<EntityUpdatedEvent<ProductCategory>>,
         IConsumer<EntityDeletedEvent<ProductCategory>>,

        //Combination
        IConsumer<EntityInsertedEvent<Combination>>,
         IConsumer<EntityUpdatedEvent<Combination>>,
         IConsumer<EntityDeletedEvent<Combination>>,

        //ProductCombination
        IConsumer<EntityInsertedEvent<ProductCombination>>,
         IConsumer<EntityUpdatedEvent<ProductCombination>>,
         IConsumer<EntityDeletedEvent<ProductCombination>>,

        //ProductFlavor
        IConsumer<EntityInsertedEvent<ProductFlavor>>,
         IConsumer<EntityUpdatedEvent<ProductFlavor>>,
         IConsumer<EntityDeletedEvent<ProductFlavor>>,

        //ProductManufacturer
        IConsumer<EntityInsertedEvent<ProductManufacturer>>,
         IConsumer<EntityUpdatedEvent<ProductManufacturer>>,
         IConsumer<EntityDeletedEvent<ProductManufacturer>>,

        //ProductPicture
        IConsumer<EntityInsertedEvent<ProductPicture>>,
         IConsumer<EntityUpdatedEvent<ProductPicture>>,
         IConsumer<EntityDeletedEvent<ProductPicture>>,

        //ProductPrice
        IConsumer<EntityInsertedEvent<ProductPrice>>,
         IConsumer<EntityUpdatedEvent<ProductPrice>>,
         IConsumer<EntityDeletedEvent<ProductPrice>>,

        //ProductSetting
        //ProductSpecificationAttribute
        IConsumer<EntityInsertedEvent<ProductSpecificationAttribute>>,
         IConsumer<EntityUpdatedEvent<ProductSpecificationAttribute>>,
         IConsumer<EntityDeletedEvent<ProductSpecificationAttribute>>,

        //ProductTierPrice
        IConsumer<EntityInsertedEvent<ProductTierPrice>>,
         IConsumer<EntityUpdatedEvent<ProductTierPrice>>,
         IConsumer<EntityDeletedEvent<ProductTierPrice>>,

        //ProductTierPricePlan
        IConsumer<EntityInsertedEvent<ProductTierPricePlan>>,
         IConsumer<EntityUpdatedEvent<ProductTierPricePlan>>,
         IConsumer<EntityDeletedEvent<ProductTierPricePlan>>,

        //ProductVariantAttribute
        IConsumer<EntityInsertedEvent<ProductVariantAttribute>>,
         IConsumer<EntityUpdatedEvent<ProductVariantAttribute>>,
         IConsumer<EntityDeletedEvent<ProductVariantAttribute>>,

         //ProductVariantAttributeCombination
         IConsumer<EntityInsertedEvent<ProductVariantAttributeCombination>>,
         IConsumer<EntityUpdatedEvent<ProductVariantAttributeCombination>>,
         IConsumer<EntityDeletedEvent<ProductVariantAttributeCombination>>,

        //ProductVariantAttributeValue
        IConsumer<EntityInsertedEvent<ProductVariantAttributeValue>>,
         IConsumer<EntityUpdatedEvent<ProductVariantAttributeValue>>,
         IConsumer<EntityDeletedEvent<ProductVariantAttributeValue>>,

        //RecentPrice
        IConsumer<EntityInsertedEvent<RecentPrice>>,
         IConsumer<EntityUpdatedEvent<RecentPrice>>,
         IConsumer<EntityDeletedEvent<RecentPrice>>,

        //SpecificationAttribute
        IConsumer<EntityInsertedEvent<SpecificationAttribute>>,
         IConsumer<EntityUpdatedEvent<SpecificationAttribute>>,
         IConsumer<EntityDeletedEvent<SpecificationAttribute>>,

         //SpecificationAttributeOption
         IConsumer<EntityInsertedEvent<SpecificationAttributeOption>>,
         IConsumer<EntityUpdatedEvent<SpecificationAttributeOption>>,
         IConsumer<EntityDeletedEvent<SpecificationAttributeOption>>,

        //StatisticalTypes
        IConsumer<EntityInsertedEvent<StatisticalTypes>>,
         IConsumer<EntityUpdatedEvent<StatisticalTypes>>,
         IConsumer<EntityDeletedEvent<StatisticalTypes>>

    {
        protected readonly IStaticCacheManager _cacheManager;

        public ArchivesCacheEventConsumer(IStaticCacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        #region Campaign
        public void HandleEvent(EntityInsertedEvent<Campaign> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.CAMPAIGN_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<Campaign> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.CAMPAIGN_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<Campaign> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.CAMPAIGN_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region PercentagePlan
        public void HandleEvent(EntityInsertedEvent<PercentagePlan> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PLAN_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<PercentagePlan> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PLAN_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<PercentagePlan> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PLAN_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region Brand
        public void HandleEvent(EntityInsertedEvent<Brand> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.BRAND_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<Brand> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.BRAND_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<Brand> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.BRAND_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region Category
        public void HandleEvent(EntityInsertedEvent<Category> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.CATEGORIES_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<Category> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.CATEGORIES_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<Category> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.CATEGORIES_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region GiveQuota
        public void HandleEvent(EntityInsertedEvent<GiveQuota> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.GIVEQUOTA_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<GiveQuota> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.GIVEQUOTA_PK, eventMessage.Entity.StoreId));
            //_cacheManager.RemoveByPrefix(BaseEntity.GetEntityCacheKey(typeof(GiveQuota), eventMessage.Entity.Id));
        }
        public void HandleEvent(EntityDeletedEvent<GiveQuota> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.GIVEQUOTA_PK, eventMessage.Entity.StoreId));
            //_cacheManager.RemoveByPrefix(BaseEntity.GetEntityCacheKey(typeof(GiveQuota), eventMessage.Entity.Id));
        }
        #endregion

        #region Manufacturer
        public void HandleEvent(EntityInsertedEvent<Manufacturer> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.MANUFACTURER_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<Manufacturer> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.MANUFACTURER_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<Manufacturer> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.MANUFACTURER_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region Product
        public void HandleEvent(EntityInsertedEvent<Product> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRODUCTS_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<Product> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRODUCTS_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<Product> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRODUCTS_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region ProductAttribute
        public void HandleEvent(EntityInsertedEvent<ProductAttribute> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRODUCTATTRIBUTES_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<ProductAttribute> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRODUCTATTRIBUTES_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<ProductAttribute> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRODUCTATTRIBUTES_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region ProductCategory
        public void HandleEvent(EntityInsertedEvent<ProductCategory> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.CATEGORIES_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<ProductCategory> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.CATEGORIES_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<ProductCategory> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.CATEGORIES_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region Combination
        public void HandleEvent(EntityInsertedEvent<Combination> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.COMBINATIONPRODUCTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<Combination> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.COMBINATIONPRODUCTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<Combination> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.COMBINATIONPRODUCTBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region ProductCombination
        public void HandleEvent(EntityInsertedEvent<ProductCombination> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.COMBINATIONPRODUCTBILLITEM_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<ProductCombination> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.COMBINATIONPRODUCTBILLITEM_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<ProductCombination> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.COMBINATIONPRODUCTBILLITEM_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region ProductFlavor
        public void HandleEvent(EntityInsertedEvent<ProductFlavor> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRODUCTFLAVOR_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<ProductFlavor> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRODUCTFLAVOR_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<ProductFlavor> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRODUCTFLAVOR_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region ProductManufacturer
        public void HandleEvent(EntityInsertedEvent<ProductManufacturer> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.MANUFACTURER_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<ProductManufacturer> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.MANUFACTURER_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<ProductManufacturer> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.MANUFACTURER_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region ProductPicture
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

        #region ProductPrice
        public void HandleEvent(EntityInsertedEvent<ProductPrice> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRODUCTS_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<ProductPrice> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRODUCTS_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<ProductPrice> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRODUCTS_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region ProductSpecificationAttribute
        public void HandleEvent(EntityInsertedEvent<ProductSpecificationAttribute> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRICEPLAN_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<ProductSpecificationAttribute> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRICEPLAN_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<ProductSpecificationAttribute> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRICEPLAN_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region ProductTierPrice
        public void HandleEvent(EntityInsertedEvent<ProductTierPrice> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRICEPLAN_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<ProductTierPrice> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRICEPLAN_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<ProductTierPrice> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRICEPLAN_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region ProductTierPricePlan
        public void HandleEvent(EntityInsertedEvent<ProductTierPricePlan> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRICEPLAN_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<ProductTierPricePlan> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRICEPLAN_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<ProductTierPricePlan> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRICEPLAN_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region ProductVariantAttribute
        public void HandleEvent(EntityInsertedEvent<ProductVariantAttribute> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRODUCTATTRIBUTES_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<ProductVariantAttribute> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRODUCTATTRIBUTES_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<ProductVariantAttribute> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRODUCTATTRIBUTES_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region ProductVariantAttributeCombination
        public void HandleEvent(EntityInsertedEvent<ProductVariantAttributeCombination> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRODUCTATTRIBUTES_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<ProductVariantAttributeCombination> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRODUCTATTRIBUTES_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<ProductVariantAttributeCombination> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRODUCTATTRIBUTES_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region ProductVariantAttributeValue
        public void HandleEvent(EntityInsertedEvent<ProductVariantAttributeValue> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRODUCTATTRIBUTES_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<ProductVariantAttributeValue> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRODUCTATTRIBUTES_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<ProductVariantAttributeValue> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRODUCTATTRIBUTES_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region RecentPrice
        public void HandleEvent(EntityInsertedEvent<RecentPrice> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<RecentPrice> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<RecentPrice> eventMessage)
        {
        }
        #endregion

        #region SpecificationAttribute
        public void HandleEvent(EntityInsertedEvent<SpecificationAttribute> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRICEPLAN_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<SpecificationAttribute> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRICEPLAN_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<SpecificationAttribute> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PRICEPLAN_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region SpecificationAttributeOption
        public void HandleEvent(EntityInsertedEvent<SpecificationAttributeOption> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<SpecificationAttributeOption> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<SpecificationAttributeOption> eventMessage)
        {
        }
        #endregion

        #region StatisticalTypes
        public void HandleEvent(EntityInsertedEvent<StatisticalTypes> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STATISTICALTYPE_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<StatisticalTypes> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STATISTICALTYPE_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<StatisticalTypes> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.STATISTICALTYPE_PK, eventMessage.Entity.StoreId));
        }
        #endregion


    }
}
