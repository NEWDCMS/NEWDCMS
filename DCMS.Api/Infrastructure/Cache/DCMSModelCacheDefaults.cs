namespace DCMS.Api.Infrastructure.Cache
{
    public static partial class DCMSModelCacheDefaults
    {
        public static string SearchCategoriesPrefixCacheKey => "DCMS.PRES.SEARCH.CATEGORIES";
        public static string ManufacturerNavigationPrefixCacheKey => "DCMS.PRES.MANUFACTURER.NAVIGATION";

        public static string ManufacturerHasFeaturedProductsPrefixCacheKeyById => "DCMS.PRES.MANUFACTURER.HASFEATUREDPRODUCTS-{0}-";

        public static string CategoryAllPrefixCacheKey => "DCMS.PRES.CATEGORY.ALL";

        public static string CategoryNumberOfProductsPrefixCacheKey => "DCMS.PRES.CATEGORY.NUMBEROFPRODUCTS";

        public static string CategoryHasFeaturedProductsPrefixCacheKeyById => "DCMS.PRES.CATEGORY.HASFEATUREDPRODUCTS-{0}-";

        public static string CategoryBreadcrumbPrefixCacheKey => "DCMS.PRES.CATEGORY.BREADCRUMB";


        public static string CategorySubcategoriesPrefixCacheKey => "DCMS.PRES.CATEGORY.SUBCATEGORIES";

        public static string CategoryHomepagePrefixCacheKey => "DCMS.PRES.CATEGORY.HOMEPAGE";

        public const string CategoryXmlAllModelKey = "DCMS.PRES.CATEGORYXML.ALL-{0}-{1}-{2}";
        public const string CategoryXmlAllPrefixCacheKey = "DCMS.PRES.CATEGORYXML.ALL";


        public static string SpecsFilterPrefixCacheKey => "DCMS.PRES.FILTER.SPECS";

        public static string ProductBreadcrumbPrefixCacheKey => "DCMS.PRES.PRODUCT.BREADCRUMB";
        public static string ProductBreadcrumbPrefixCacheKeyById => "DCMS.PRES.PRODUCT.BREADCRUMB-{0}-";

        public static string ProductManufacturersPrefixCacheKey => "DCMS.PRES.PRODUCT.MANUFACTURERS";
        public static string ProductManufacturersPrefixCacheKeyById => "DCMS.PRES.PRODUCT.MANUFACTURERS-{0}-";

        public static string ProductSpecsPrefixCacheKey => "DCMS.PRES.PRODUCT.SPECS";
        public static string ProductSpecsPrefixCacheKeyById => "DCMS.PRES.PRODUCT.SPECS-{0}-";

        public static string HomepageBestsellersIdsPrefixCacheKey => "DCMS.PRES.BESTSELLERS.HOMEPAGE";

        public static string ProductsAlsoPurchasedIdsPrefixCacheKey => "DCMS.PRES.ALSOPUCHASED";


        public static string ProductsRelatedIdsKey => "DCMS.PRES.RELATED-{0}-{1}";
        public static string ProductsRelatedIdsPrefixCacheKey => "DCMS.PRES.RELATED";


        public static string ProductDefaultPicturePrefixCacheKey => "DCMS.PRES.PRODUCT.DETAILSPICTURES";
        public static string ProductDefaultPicturePrefixCacheKeyById => "DCMS.PRES.PRODUCT.DETAILSPICTURES-{0}-";

        public static string ProductDetailsPicturesPrefixCacheKey => "DCMS.PRES.PRODUCT.PICTURE";
        public static string ProductDetailsPicturesPrefixCacheKeyById => "DCMS.PRES.PRODUCT.PICTURE-{0}-";

        public static string ProductReviewsPrefixCacheKeyById => "DCMS.PRES.PRODUCT.REVIEWS-{0}-";


        public static string ProductAttributePicturePrefixCacheKey => "DCMS.PRES.PRODUCTATTRIBUTE.PICTURE";


        public static string CategoryPicturePrefixCacheKey => "DCMS.PRES.CATEGORY.PICTURE";
        public static string CategoryPicturePrefixCacheKeyById => "DCMS.PRES.CATEGORY.PICTURE-{0}-";


        public static string ManufacturerPicturePrefixCacheKey => "DCMS.PRES.MANUFACTURER.PICTURE";
        public static string ManufacturerPicturePrefixCacheKeyById => "DCMS.PRES.MANUFACTURER.PICTURE-{0}-";


        public static string VendorPicturePrefixCacheKey => "DCMS.PRES.VENDOR.PICTURE";


        public static string NewsPrefixCacheKey => "DCMS.NEWS.";

        //SaleReservationBill
        public static string SaleReservationBillPrefixCacheKey => "API_DCMS_SALERESERVATIONBILL_GETSALERESERVATIONBILL_";

        //SaleBill
        public static string SaleBillPrefixCacheKey => "API_DCMS_SALEBILL_GETSALEBILL_";

        //ReturnBill
        public static string ReturnBillPrefixCacheKey => "API_DCMS_RETURNBILL_GETRETURNBILL_";

        //ReturnReservationBill
        public static string ReturnReservationBillPrefixCacheKey => "API_DCMS_RETURNRESERVATIONBILL_GETRETURNRESERVATIONBILL_";

        //PurchaseBill
        public static string PurchaseBillPrefixCacheKey => "API_DCMS_PURCHASESBILL_GETPURCHASEBILL_";

        //PurchaseReturnBill
        public static string PurchaseReturnBillPrefixCacheKey => "API_DCMS_PURCHASERETURNBILL_GETPURCHASERETURNBILL_";

        //AdvancePaymentBill
        public static string AdvancePaymentBillPrefixCacheKey => "API_DCMS_ADVANCEPAYMENTBILL_GETADVANCEPAYMENTBILL_";

        //AdvanceReceiptBill
        public static string AdvanceReceiptBillPrefixCacheKey => "API_DCMS_ADVANCERECEIPT_GETADVANCERECEIPTBILL_";

        //CostContractBill
        public static string CostContractBillPrefixCacheKey => "API_DCMS_COSTCONTRACT_GETCOSTCONTRACTBILL_";

        //CostExpenditureBill
        public static string CostExpenditureBillPrefixCacheKey => "API_DCMS_COSTEXPENDITUREBILL_GETCOSTEXPENDITUREBILL_";

        //CashReceiptBill
        public static string CashReceiptBillPrefixCacheKey => "API_DCMS_RECEIPTCASHBILL_GETCASHRECEIPTBILL_";

        //AllocationBill
        public static string AllocationBillPrefixCacheKey => "API_DCMS_RECEIPTCASHBILL_GETALLOCATIONBILL_";

        //Product
        public static string ProductPrefixCacheKey => "API_DCMS_CONTROLLERS_PRODUCTCONTROLLER_GETPRODUCTS_";
        public static string ProductByIdPrefixCacheKey => "API_DCMS_CONTROLLERS_PRODUCTCONTROLLER_GETPRODUCTBYID_";
        public static string ProductAlloctionPrefixCacheKey => "API_DCMS_CONTROLLERS_PRODUCTCONTROLLER_GETALLOCATIONPRODUCTS_";

        //Categories
        public static string CategoriesAPIPrefixCacheKey => "API_DCMS_CONTROLLERS_GETFANCYTREE_";
        public static string CategoriesAllAPIPrefixCacheKey => "API_DCMS_CONTROLLERS_GETALLCATEGORIES_";

        //Terminal
        public static string GETTerminalAPIPrefixCacheKey => "API_DCMS_CONTROLLERS_TERMINALCONTROLLER_GETTERMINALS_";
    }
}
