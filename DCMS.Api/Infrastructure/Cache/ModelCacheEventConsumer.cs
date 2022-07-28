using DCMS.Core.Caching;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Finances;
using DCMS.Core.Domain.Media;
using DCMS.Core.Domain.News;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Purchases;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Domain.Terminals;
using DCMS.Core.Domain.Users;
using DCMS.Core.Domain.WareHouses;
using DCMS.Core.Events;
using DCMS.Services.Events;


namespace DCMS.Api.Infrastructure.Cache
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

         IConsumer<EntityInsertedEvent<SaleReservationBill>>,
         IConsumer<EntityUpdatedEvent<SaleReservationBill>>,
        IConsumer<EntityDeletedEvent<SaleReservationBill>>,

         IConsumer<EntityInsertedEvent<SaleBill>>,
         IConsumer<EntityUpdatedEvent<SaleBill>>,
         IConsumer<EntityDeletedEvent<SaleBill>>,

        IConsumer<EntityInsertedEvent<ReturnBill>>,
         IConsumer<EntityUpdatedEvent<ReturnBill>>,
         IConsumer<EntityDeletedEvent<ReturnBill>>,

        IConsumer<EntityInsertedEvent<ReturnReservationBill>>,
         IConsumer<EntityUpdatedEvent<ReturnReservationBill>>,
         IConsumer<EntityDeletedEvent<ReturnReservationBill>>,

        IConsumer<EntityInsertedEvent<PurchaseBill>>,
         IConsumer<EntityUpdatedEvent<PurchaseBill>>,
         IConsumer<EntityDeletedEvent<PurchaseBill>>,

        IConsumer<EntityInsertedEvent<PurchaseReturnBill>>,
         IConsumer<EntityUpdatedEvent<PurchaseReturnBill>>,
         IConsumer<EntityDeletedEvent<PurchaseReturnBill>>,

        IConsumer<EntityInsertedEvent<AdvancePaymentBill>>,
         IConsumer<EntityUpdatedEvent<AdvancePaymentBill>>,
         IConsumer<EntityDeletedEvent<AdvancePaymentBill>>,

        IConsumer<EntityInsertedEvent<AdvanceReceiptBill>>,
         IConsumer<EntityUpdatedEvent<AdvanceReceiptBill>>,
         IConsumer<EntityDeletedEvent<AdvanceReceiptBill>>,

        IConsumer<EntityInsertedEvent<CostContractBill>>,
         IConsumer<EntityUpdatedEvent<CostContractBill>>,
         IConsumer<EntityDeletedEvent<CostContractBill>>,

        IConsumer<EntityInsertedEvent<CostExpenditureBill>>,
         IConsumer<EntityUpdatedEvent<CostExpenditureBill>>,
         IConsumer<EntityDeletedEvent<CostExpenditureBill>>,

        IConsumer<EntityInsertedEvent<CashReceiptBill>>,
         IConsumer<EntityUpdatedEvent<CashReceiptBill>>,
         IConsumer<EntityDeletedEvent<CashReceiptBill>>,

        IConsumer<EntityInsertedEvent<AllocationBill>>,
         IConsumer<EntityUpdatedEvent<AllocationBill>>,
         IConsumer<EntityDeletedEvent<AllocationBill>>,

        IConsumer<EntityInsertedEvent<Stock>>,
         IConsumer<EntityUpdatedEvent<Stock>>,
         IConsumer<EntityDeletedEvent<Stock>>,

        IConsumer<EntityInsertedEvent<Terminal>>,
         IConsumer<EntityUpdatedEvent<Terminal>>,
         IConsumer<EntityDeletedEvent<Terminal>>
    {



        protected readonly IStaticCacheManager _cacheManager;
        public ModelCacheEventConsumer(IStaticCacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }


        public void HandleEvent(EntityUpdatedEvent<Setting> eventMessage)
        {
        }

        //manufacturers
        public void HandleEvent(EntityInsertedEvent<Manufacturer> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ManufacturerNavigationPrefixCacheKey);

        }
        public void HandleEvent(EntityUpdatedEvent<Manufacturer> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ManufacturerNavigationPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductManufacturersPrefixCacheKey);
            _cacheManager.RemoveByPrefix(string.Format(DCMSModelCacheDefaults.ManufacturerPicturePrefixCacheKeyById, eventMessage.Entity.Id));
        }
        public void HandleEvent(EntityDeletedEvent<Manufacturer> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ManufacturerNavigationPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductManufacturersPrefixCacheKey);

        }

        //product manufacturers
        public void HandleEvent(EntityInsertedEvent<ProductManufacturer> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSModelCacheDefaults.ProductManufacturersPrefixCacheKeyById, eventMessage.Entity.ProductId));
            _cacheManager.RemoveByPrefix(string.Format(DCMSModelCacheDefaults.ManufacturerHasFeaturedProductsPrefixCacheKeyById, eventMessage.Entity.ManufacturerId));
        }
        public void HandleEvent(EntityUpdatedEvent<ProductManufacturer> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSModelCacheDefaults.ProductManufacturersPrefixCacheKeyById, eventMessage.Entity.ProductId));
            _cacheManager.RemoveByPrefix(string.Format(DCMSModelCacheDefaults.ManufacturerHasFeaturedProductsPrefixCacheKeyById, eventMessage.Entity.ManufacturerId));
        }
        public void HandleEvent(EntityDeletedEvent<ProductManufacturer> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSModelCacheDefaults.ProductManufacturersPrefixCacheKeyById, eventMessage.Entity.ProductId));
            _cacheManager.RemoveByPrefix(string.Format(DCMSModelCacheDefaults.ManufacturerHasFeaturedProductsPrefixCacheKeyById, eventMessage.Entity.ManufacturerId));
        }

        //categories
        public void HandleEvent(EntityInsertedEvent<Category> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.SearchCategoriesPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CategoryAllPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CategoryXmlAllPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CategorySubcategoriesPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CategoryHomepagePrefixCacheKey);

            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CategoriesAPIPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CategoriesAllAPIPrefixCacheKey);

        }
        public void HandleEvent(EntityUpdatedEvent<Category> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.SearchCategoriesPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductBreadcrumbPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CategoryAllPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CategoryXmlAllPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CategoryBreadcrumbPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CategorySubcategoriesPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CategoryHomepagePrefixCacheKey);
            _cacheManager.RemoveByPrefix(string.Format(DCMSModelCacheDefaults.CategoryPicturePrefixCacheKeyById, eventMessage.Entity.Id));

            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CategoriesAPIPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CategoriesAllAPIPrefixCacheKey);
        }
        public void HandleEvent(EntityDeletedEvent<Category> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.SearchCategoriesPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductBreadcrumbPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CategoryAllPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CategoryXmlAllPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CategoryBreadcrumbPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CategorySubcategoriesPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CategoryHomepagePrefixCacheKey);

            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CategoriesAPIPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CategoriesAllAPIPrefixCacheKey);
        }

        //product categories
        public void HandleEvent(EntityInsertedEvent<ProductCategory> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSModelCacheDefaults.ProductBreadcrumbPrefixCacheKeyById, eventMessage.Entity.ProductId));
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CategoryAllPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CategoryXmlAllPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CategoryNumberOfProductsPrefixCacheKey);
            _cacheManager.RemoveByPrefix(string.Format(DCMSModelCacheDefaults.CategoryHasFeaturedProductsPrefixCacheKeyById, eventMessage.Entity.CategoryId));
        }
        public void HandleEvent(EntityUpdatedEvent<ProductCategory> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSModelCacheDefaults.ProductBreadcrumbPrefixCacheKeyById, eventMessage.Entity.ProductId));
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CategoryNumberOfProductsPrefixCacheKey);
            _cacheManager.RemoveByPrefix(string.Format(DCMSModelCacheDefaults.CategoryHasFeaturedProductsPrefixCacheKeyById, eventMessage.Entity.CategoryId));
        }
        public void HandleEvent(EntityDeletedEvent<ProductCategory> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSModelCacheDefaults.ProductBreadcrumbPrefixCacheKeyById, eventMessage.Entity.ProductId));
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CategoryAllPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CategoryXmlAllPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CategoryNumberOfProductsPrefixCacheKey);
            _cacheManager.RemoveByPrefix(string.Format(DCMSModelCacheDefaults.CategoryHasFeaturedProductsPrefixCacheKeyById, eventMessage.Entity.CategoryId));
        }

        //products
        public void HandleEvent(EntityInsertedEvent<Product> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductByIdPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductAlloctionPrefixCacheKey);
        }
        public void HandleEvent(EntityUpdatedEvent<Product> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.HomepageBestsellersIdsPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductsAlsoPurchasedIdsPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductsRelatedIdsPrefixCacheKey);
            _cacheManager.RemoveByPrefix(string.Format(DCMSModelCacheDefaults.ProductReviewsPrefixCacheKeyById, eventMessage.Entity.Id));
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductByIdPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductAlloctionPrefixCacheKey);
        }
        public void HandleEvent(EntityDeletedEvent<Product> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.HomepageBestsellersIdsPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductsAlsoPurchasedIdsPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductsRelatedIdsPrefixCacheKey);

            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductByIdPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductAlloctionPrefixCacheKey);
        }

        //specification attributes
        public void HandleEvent(EntityUpdatedEvent<SpecificationAttribute> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductSpecsPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.SpecsFilterPrefixCacheKey);
        }
        public void HandleEvent(EntityDeletedEvent<SpecificationAttribute> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductSpecsPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.SpecsFilterPrefixCacheKey);
        }

        //specification attribute options
        public void HandleEvent(EntityUpdatedEvent<SpecificationAttributeOption> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductSpecsPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.SpecsFilterPrefixCacheKey);
        }
        public void HandleEvent(EntityDeletedEvent<SpecificationAttributeOption> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductSpecsPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.SpecsFilterPrefixCacheKey);
        }

        //Product specification attribute
        public void HandleEvent(EntityInsertedEvent<ProductSpecificationAttribute> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSModelCacheDefaults.ProductSpecsPrefixCacheKeyById, eventMessage.Entity.ProductId));
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.SpecsFilterPrefixCacheKey);
        }
        public void HandleEvent(EntityUpdatedEvent<ProductSpecificationAttribute> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSModelCacheDefaults.ProductSpecsPrefixCacheKeyById, eventMessage.Entity.ProductId));
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.SpecsFilterPrefixCacheKey);
        }
        public void HandleEvent(EntityDeletedEvent<ProductSpecificationAttribute> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSModelCacheDefaults.ProductSpecsPrefixCacheKeyById, eventMessage.Entity.ProductId));
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.SpecsFilterPrefixCacheKey);
        }

        //Pictures
        public void HandleEvent(EntityInsertedEvent<Picture> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductAttributePicturePrefixCacheKey);
        }
        public void HandleEvent(EntityUpdatedEvent<Picture> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductAttributePicturePrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductDetailsPicturesPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductDefaultPicturePrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CategoryPicturePrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ManufacturerPicturePrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.VendorPicturePrefixCacheKey);
        }
        public void HandleEvent(EntityDeletedEvent<Picture> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductAttributePicturePrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductDetailsPicturesPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductDefaultPicturePrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CategoryPicturePrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ManufacturerPicturePrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.VendorPicturePrefixCacheKey);
        }

        //Product picture mappings
        public void HandleEvent(EntityInsertedEvent<ProductPicture> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSModelCacheDefaults.ProductDefaultPicturePrefixCacheKeyById, eventMessage.Entity.ProductId));
            _cacheManager.RemoveByPrefix(string.Format(DCMSModelCacheDefaults.ProductDetailsPicturesPrefixCacheKeyById, eventMessage.Entity.ProductId));
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductAttributePicturePrefixCacheKey);

        }
        public void HandleEvent(EntityUpdatedEvent<ProductPicture> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSModelCacheDefaults.ProductDefaultPicturePrefixCacheKeyById, eventMessage.Entity.ProductId));
            _cacheManager.RemoveByPrefix(string.Format(DCMSModelCacheDefaults.ProductDetailsPicturesPrefixCacheKeyById, eventMessage.Entity.ProductId));
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductAttributePicturePrefixCacheKey);

        }
        public void HandleEvent(EntityDeletedEvent<ProductPicture> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSModelCacheDefaults.ProductDefaultPicturePrefixCacheKeyById, eventMessage.Entity.ProductId));
            _cacheManager.RemoveByPrefix(string.Format(DCMSModelCacheDefaults.ProductDetailsPicturesPrefixCacheKeyById, eventMessage.Entity.ProductId));
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductAttributePicturePrefixCacheKey);

        }


        //News items
        public void HandleEvent(EntityInsertedEvent<NewsItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.NewsPrefixCacheKey);
        }
        public void HandleEvent(EntityUpdatedEvent<NewsItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.NewsPrefixCacheKey);
        }
        public void HandleEvent(EntityDeletedEvent<NewsItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.NewsPrefixCacheKey);
        }


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

        //SaleReservationBill
        public void HandleEvent(EntityInsertedEvent<SaleReservationBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.SaleReservationBillPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductPrefixCacheKey);
        }
        public void HandleEvent(EntityUpdatedEvent<SaleReservationBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.SaleReservationBillPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductPrefixCacheKey);
        }
        public void HandleEvent(EntityDeletedEvent<SaleReservationBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.SaleReservationBillPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductPrefixCacheKey);
        }

        //SaleBill
        public void HandleEvent(EntityInsertedEvent<SaleBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.SaleBillPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductPrefixCacheKey);
        }
        public void HandleEvent(EntityUpdatedEvent<SaleBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.SaleBillPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductPrefixCacheKey);
        }
        public void HandleEvent(EntityDeletedEvent<SaleBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.SaleBillPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductPrefixCacheKey);
        }

        //ReturnBill
        public void HandleEvent(EntityInsertedEvent<ReturnBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ReturnBillPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductPrefixCacheKey);
        }
        public void HandleEvent(EntityUpdatedEvent<ReturnBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ReturnBillPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductPrefixCacheKey);
        }
        public void HandleEvent(EntityDeletedEvent<ReturnBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ReturnBillPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductPrefixCacheKey);
        }

        //ReturnReservationBill
        public void HandleEvent(EntityInsertedEvent<ReturnReservationBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ReturnReservationBillPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductPrefixCacheKey);
        }
        public void HandleEvent(EntityUpdatedEvent<ReturnReservationBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ReturnReservationBillPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductPrefixCacheKey);
        }
        public void HandleEvent(EntityDeletedEvent<ReturnReservationBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ReturnReservationBillPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductPrefixCacheKey);
        }

        //PurchaseBill
        public void HandleEvent(EntityInsertedEvent<PurchaseBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.PurchaseBillPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductPrefixCacheKey);
        }
        public void HandleEvent(EntityUpdatedEvent<PurchaseBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.PurchaseBillPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductPrefixCacheKey);
        }
        public void HandleEvent(EntityDeletedEvent<PurchaseBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.PurchaseBillPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductPrefixCacheKey);
        }

        //PurchaseReturnBill
        public void HandleEvent(EntityInsertedEvent<PurchaseReturnBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.PurchaseReturnBillPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductPrefixCacheKey);
        }
        public void HandleEvent(EntityUpdatedEvent<PurchaseReturnBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.PurchaseReturnBillPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductPrefixCacheKey);
        }
        public void HandleEvent(EntityDeletedEvent<PurchaseReturnBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.PurchaseReturnBillPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductPrefixCacheKey);
        }

        //AdvancePaymentBill
        public void HandleEvent(EntityInsertedEvent<AdvancePaymentBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.AdvancePaymentBillPrefixCacheKey);
        }
        public void HandleEvent(EntityUpdatedEvent<AdvancePaymentBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.AdvancePaymentBillPrefixCacheKey);
        }
        public void HandleEvent(EntityDeletedEvent<AdvancePaymentBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.AdvancePaymentBillPrefixCacheKey);
        }

        //AdvanceReceiptBill
        public void HandleEvent(EntityInsertedEvent<AdvanceReceiptBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.AdvanceReceiptBillPrefixCacheKey);
        }
        public void HandleEvent(EntityUpdatedEvent<AdvanceReceiptBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.AdvanceReceiptBillPrefixCacheKey);
        }
        public void HandleEvent(EntityDeletedEvent<AdvanceReceiptBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.AdvanceReceiptBillPrefixCacheKey);
        }

        //CostContractBill
        public void HandleEvent(EntityInsertedEvent<CostContractBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CostContractBillPrefixCacheKey);
        }
        public void HandleEvent(EntityUpdatedEvent<CostContractBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CostContractBillPrefixCacheKey);
        }
        public void HandleEvent(EntityDeletedEvent<CostContractBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CostContractBillPrefixCacheKey);
        }

        //CostExpenditureBill
        public void HandleEvent(EntityInsertedEvent<CostExpenditureBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CostExpenditureBillPrefixCacheKey);
        }
        public void HandleEvent(EntityUpdatedEvent<CostExpenditureBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CostExpenditureBillPrefixCacheKey);
        }
        public void HandleEvent(EntityDeletedEvent<CostExpenditureBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CostExpenditureBillPrefixCacheKey);
        }

        //CashReceiptBill
        public void HandleEvent(EntityInsertedEvent<CashReceiptBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CashReceiptBillPrefixCacheKey);
        }
        public void HandleEvent(EntityUpdatedEvent<CashReceiptBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CashReceiptBillPrefixCacheKey);
        }
        public void HandleEvent(EntityDeletedEvent<CashReceiptBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.CashReceiptBillPrefixCacheKey);
        }

        //AllocationBill
        public void HandleEvent(EntityInsertedEvent<AllocationBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.AllocationBillPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductPrefixCacheKey);
        }
        public void HandleEvent(EntityUpdatedEvent<AllocationBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.AllocationBillPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductPrefixCacheKey);
        }
        public void HandleEvent(EntityDeletedEvent<AllocationBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.AllocationBillPrefixCacheKey);
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductPrefixCacheKey);
        }

        //Stock
        public void HandleEvent(EntityInsertedEvent<Stock> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductPrefixCacheKey);
        }
        public void HandleEvent(EntityUpdatedEvent<Stock> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductPrefixCacheKey);
        }
        public void HandleEvent(EntityDeletedEvent<Stock> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.ProductPrefixCacheKey);
        }

        //Terminal
        public void HandleEvent(EntityInsertedEvent<Terminal> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.GETTerminalAPIPrefixCacheKey);
        }
        public void HandleEvent(EntityUpdatedEvent<Terminal> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.GETTerminalAPIPrefixCacheKey);
        }
        public void HandleEvent(EntityDeletedEvent<Terminal> eventMessage)
        {
            _cacheManager.RemoveByPrefix(DCMSModelCacheDefaults.GETTerminalAPIPrefixCacheKey);
        }
    }
}