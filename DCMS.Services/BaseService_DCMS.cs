using DCMS.Core.Data;
using DCMS.Core.Domain.Campaigns;
using DCMS.Core.Domain.Census;
using DCMS.Core.Domain.Common;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Finances;
using DCMS.Core.Domain.Media;
using DCMS.Core.Domain.News;
using DCMS.Core.Domain.Plan;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Purchases;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Domain.Stores;
using DCMS.Core.Domain.Terminals;
using DCMS.Core.Domain.Users;
using DCMS.Core.Domain.Visit;
using DCMS.Core.Domain.WareHouses;
using Product = DCMS.Core.Domain.Products.Product;

namespace DCMS.Services
{
    /// <summary>
    /// 服务基类
    /// </summary>
    public partial class BaseService
    {
      
        #region DCMS


        #region RO

        protected IRepositoryReadOnly<VisitStore> VisitStoreRepository_RO => _getter.RO<VisitStore>(DCMS);
        protected IRepositoryReadOnly<GiveQuotaOption> GiveQuotaOptionRepository_RO => _getter.RO<GiveQuotaOption>(DCMS);
        protected IRepositoryReadOnly<StockEarlyWarning> StockEarlyWarningsRepository_RO => _getter.RO<StockEarlyWarning>(DCMS);
        protected IRepositoryReadOnly<StockFlow> StockFlowsRepository_RO => _getter.RO<StockFlow>(DCMS);
        protected IRepositoryReadOnly<InventoryAllTaskItem> InventoryAllTaskItemsRepository_RO => _getter.RO<InventoryAllTaskItem>(DCMS);
        protected IRepositoryReadOnly<StockInOutRecord> StockInOutRecordsRepository_RO => _getter.RO<StockInOutRecord>(DCMS);
        protected IRepositoryReadOnly<DeliverySign> DeliverySignsRepository_RO => _getter.RO<DeliverySign>(DCMS);
        protected IRepositoryReadOnly<StockInOutRecordStockFlow> StockInOutRecordsStockFlowsMappingRepository_RO => _getter.RO<StockInOutRecordStockFlow>(DCMS);
        protected IRepositoryReadOnly<InventoryPartTaskItem> InventoryPartTaskItemsRepository_RO => _getter.RO<InventoryPartTaskItem>(DCMS);
        protected IRepositoryReadOnly<Stock> StocksRepository_RO => _getter.RO<Stock>(DCMS);
        protected IRepositoryReadOnly<InventoryProfitLossBill> InventoryProfitLossBillsRepository_RO => _getter.RO<InventoryProfitLossBill>(DCMS);
        protected IRepositoryReadOnly<RecentPrice> RecentPricesRepository_RO => _getter.RO<RecentPrice>(DCMS);
        //protected IRepositoryReadOnly<Store> StoreRepository_RO => _getter.RO<Store>(DCMS);
        protected IRepositoryReadOnly<InventoryProfitLossItem> InventoryProfitLossItemsRepository_RO => _getter.RO<InventoryProfitLossItem>(DCMS);
        protected IRepositoryReadOnly<ReturnBill> ReturnBillsRepository_RO => _getter.RO<ReturnBill>(DCMS);
        protected IRepositoryReadOnly<StoreMapping> StoreMappingRepository_RO => _getter.RO<StoreMapping>(DCMS);
        protected IRepositoryReadOnly<LineTierOption> LineTierOptionsRepository_RO => _getter.RO<LineTierOption>(DCMS);
        protected IRepositoryReadOnly<LineTier> LineTiersRepository_RO => _getter.RO<LineTier>(DCMS);
        protected IRepositoryReadOnly<InventoryAllTaskBill> InventoryAllTaskBillsRepository_RO => _getter.RO<InventoryAllTaskBill>(DCMS);
        protected IRepositoryReadOnly<FinanceReceiveAccountBill> FinanceReceiveAccountBillsRepository_RO => _getter.RO<FinanceReceiveAccountBill>(DCMS);
        protected IRepositoryReadOnly<Manufacturer> ManufacturerRepository_RO => _getter.RO<Manufacturer>(DCMS);
        protected IRepositoryReadOnly<Receivable> ReceivablesRepository_RO => _getter.RO<Receivable>(DCMS);
        protected IRepositoryReadOnly<InventoryPartTaskBill> InventoryPartTaskBillsRepository_RO => _getter.RO<InventoryPartTaskBill>(DCMS);
        protected IRepositoryReadOnly<TrialBalance> TrialBalancesRepository_RO => _getter.RO<TrialBalance>(DCMS);
        protected IRepository<ClosingAccounts> ClosingAccountsRepository_RO => _getter.RW<ClosingAccounts>(DCMS);
        protected IRepository<CostPriceSummery> CostPriceSummeryRepository_RO => _getter.RW<CostPriceSummery>(DCMS);
        protected IRepository<CostPriceChangeRecords> CostPriceChangeRecordsRepository_RO => _getter.RW<CostPriceChangeRecords>(DCMS);

        protected IRepositoryReadOnly<PaymentReceiptBillAccounting> PaymentReceiptBillAccountingMappingRepository_RO => _getter.RO<PaymentReceiptBillAccounting>(DCMS);
        protected IRepositoryReadOnly<InventoryReportBill> InventoryReportBillsRepository_RO => _getter.RO<InventoryReportBill>(DCMS);
        protected IRepositoryReadOnly<AdvanceReceiptBill> AdvanceReceiptBillsRepository_RO => _getter.RO<AdvanceReceiptBill>(DCMS);
        protected IRepositoryReadOnly<PaymentReceiptBill> PaymentReceiptBillsRepository_RO => _getter.RO<PaymentReceiptBill>(DCMS);
        protected IRepositoryReadOnly<VoucherItem> VoucherItemsRepository_RO => _getter.RO<VoucherItem>(DCMS);
        protected IRepositoryReadOnly<PaymentReceiptItem> PaymentReceiptItemsRepository_RO => _getter.RO<PaymentReceiptItem>(DCMS);
        protected IRepositoryReadOnly<InventoryReportStoreQuantity> InventoryReportStoreQuantitiesRepository_RO => _getter.RO<InventoryReportStoreQuantity>(DCMS);
        protected IRepositoryReadOnly<CostExpenditureBill> CostExpenditureBillsRepository_RO => _getter.RO<CostExpenditureBill>(DCMS);
        protected IRepositoryReadOnly<WareHouse> WareHousesRepository_RO => _getter.RO<WareHouse>(DCMS);
        protected IRepositoryReadOnly<AccountingOption> AccountingOptionsRepository_RO => _getter.RO<AccountingOption>(DCMS);
        protected IRepositoryReadOnly<PercentagePlan> PercentagePlanRepository_RO => _getter.RO<PercentagePlan>(DCMS);
        protected IRepositoryReadOnly<NewsItem> NewsItemRepository_RO => _getter.RO<NewsItem>(DCMS);
        protected IRepositoryReadOnly<PickingItem> PickingItemsRepository_RO => _getter.RO<PickingItem>(DCMS);
        protected IRepositoryReadOnly<InventoryReportItem> InventoryReportItemsRepository_RO => _getter.RO<InventoryReportItem>(DCMS);
        protected IRepositoryReadOnly<PickingBill> PickingsRepository_RO => _getter.RO<PickingBill>(DCMS);
        protected IRepositoryReadOnly<Picture> PicturesRepository_RO => _getter.RO<Picture>(DCMS);
        protected IRepositoryReadOnly<FinancialIncomeItem> FinancialIncomeItemsRepository_RO => _getter.RO<FinancialIncomeItem>(DCMS);
        //protected IRepositoryReadOnly<Terminal> TerminalsRepository_RO => _getter.RO<Terminal>(DCMS);
        protected IRepositoryReadOnly<PricingStructure> PricingStructuresRepository_RO => _getter.RO<PricingStructure>(DCMS);
        protected IRepositoryReadOnly<PrintTemplate> PrintTemplatesRepository_RO => _getter.RO<PrintTemplate>(DCMS);
        protected IRepositoryReadOnly<ReturnItem> ReturnItemsRepository_RO => _getter.RO<ReturnItem>(DCMS);
        protected IRepositoryReadOnly<ProductAttribute> ProductAttributeRepository_RO => _getter.RO<ProductAttribute>(DCMS);
        protected IRepositoryReadOnly<ProductCombination> ProductCombinationsRepository_RO => _getter.RO<ProductCombination>(DCMS);
        protected IRepositoryReadOnly<ProductFlavor> ProductFlavorsRepository_RO => _getter.RO<ProductFlavor>(DCMS);
        protected IRepositoryReadOnly<ProductManufacturer> ProductManufacturersRepository_RO => _getter.RO<ProductManufacturer>(DCMS);
        protected IRepositoryReadOnly<Product> ProductsRepository_RO => _getter.RO<Product>(DCMS);
        protected IRepositoryReadOnly<PurchaseReturnBill> PurchaseReturnBillsRepository_RO => _getter.RO<PurchaseReturnBill>(DCMS);
        protected IRepositoryReadOnly<ProductCategory> ProductsCategoryMappingRepository_RO => _getter.RO<ProductCategory>(DCMS);
        protected IRepositoryReadOnly<InventoryReportSummary> InventoryReportSummariesRepository_RO => _getter.RO<InventoryReportSummary>(DCMS);
        protected IRepositoryReadOnly<ProductPicture> ProductsPicturesMappingRepository_RO => _getter.RO<ProductPicture>(DCMS);
        protected IRepositoryReadOnly<AccountingType> AccountingTypesRepository_RO => _getter.RO<AccountingType>(DCMS);
        protected IRepositoryReadOnly<ProductVariantAttribute> ProductsProductAttributeMappingRepository_RO => _getter.RO<ProductVariantAttribute>(DCMS);
        protected IRepositoryReadOnly<ReturnReservationBill> ReturnReservationBillsRepository_RO => _getter.RO<ReturnReservationBill>(DCMS);
        protected IRepositoryReadOnly<AdvancePaymentBillAccounting> AdvancePaymentBillAccountingMappingRepository_RO => _getter.RO<AdvancePaymentBillAccounting>(DCMS);
        protected IRepositoryReadOnly<SaleReservationBill> SaleReservationBillsRepository_RO => _getter.RO<SaleReservationBill>(DCMS);
        protected IRepositoryReadOnly<ProductSpecificationAttribute> ProductsSpecificationAttributeMappingRepository_RO => _getter.RO<ProductSpecificationAttribute>(DCMS);
        protected IRepositoryReadOnly<AdvancePaymentBill> AdvancePaymentBillsRepository_RO => _getter.RO<AdvancePaymentBill>(DCMS);
        protected IRepositoryReadOnly<ProductTierPricePlan> ProductTierPricePlansRepository_RO => _getter.RO<ProductTierPricePlan>(DCMS);
        protected IRepositoryReadOnly<AdvanceReceiptBillAccounting> AdvanceReceiptBillAccountingMappingRepository_RO => _getter.RO<AdvanceReceiptBillAccounting>(DCMS);

        protected IRepositoryReadOnly<Setting> SettingsRepository_RO => _getter.RO<Setting>(DCMS);
        protected IRepositoryReadOnly<ProductTierPrice> ProductTierPricesRepository_RO => _getter.RO<ProductTierPrice>(DCMS);
        protected IRepositoryReadOnly<ProductVariantAttributeCombination> ProductVariantAttributeCombinationRepository_RO => _getter.RO<ProductVariantAttributeCombination>(DCMS);
        protected IRepositoryReadOnly<AllocationBill> AllocationBillsRepository_RO => _getter.RO<AllocationBill>(DCMS);
        protected IRepositoryReadOnly<ProductVariantAttributeValue> ProductVariantAttributeValueRepository_RO => _getter.RO<ProductVariantAttributeValue>(DCMS);
        protected IRepositoryReadOnly<AllocationItem> AllocationItemsRepository_RO => _getter.RO<AllocationItem>(DCMS);
        protected IRepositoryReadOnly<ProfitSheet> ProfitSheetsRepository_RO => _getter.RO<ProfitSheet>(DCMS);
        protected IRepositoryReadOnly<BalanceSheet> BalanceSheetsRepository_RO => _getter.RO<BalanceSheet>(DCMS);
        protected IRepositoryReadOnly<PurchaseBillAccounting> PurchaseBillAccountingMappingRepository_RO => _getter.RO<PurchaseBillAccounting>(DCMS);
        protected IRepositoryReadOnly<Brand> BrandsRepository_RO => _getter.RO<Brand>(DCMS);
        protected IRepositoryReadOnly<GiveQuotaRecords> GiveQuotaRecordsRepository_RO => _getter.RO<GiveQuotaRecords>(DCMS);
        protected IRepositoryReadOnly<PurchaseBill> PurchaseBillsRepository_RO => _getter.RO<PurchaseBill>(DCMS);
        protected IRepositoryReadOnly<CampaignChannel> CampaignChannelMappingRepository_RO => _getter.RO<CampaignChannel>(DCMS);
        protected IRepositoryReadOnly<CampaignBuyProduct> CampaignBuyProductsRepository_RO => _getter.RO<CampaignBuyProduct>(DCMS);
        protected IRepositoryReadOnly<PurchaseReturnBillAccounting> PurchaseReturnBillAccountingMappingRepository_RO => _getter.RO<PurchaseReturnBillAccounting>(DCMS);
        protected IRepositoryReadOnly<CampaignGiveProduct> CampaignGiveProductsRepository_RO => _getter.RO<CampaignGiveProduct>(DCMS);
        protected IRepositoryReadOnly<Campaign> CampaignsRepository_RO => _getter.RO<Campaign>(DCMS);
        protected IRepositoryReadOnly<ReturnReservationItem> ReturnReservationItemsRepository_RO => _getter.RO<ReturnReservationItem>(DCMS);
        protected IRepositoryReadOnly<Rank> RanksRepository_RO => _getter.RO<Rank>(DCMS);
        protected IRepositoryReadOnly<CashReceiptBillAccounting> CashReceiptBillAccountingMappingRepository_RO => _getter.RO<CashReceiptBillAccounting>(DCMS);
        protected IRepositoryReadOnly<CashReceiptBill> CashReceiptBillsRepository_RO => _getter.RO<CashReceiptBill>(DCMS);
        protected IRepositoryReadOnly<NewsPicture> NewsPictureRepository_RO => _getter.RO<NewsPicture>(DCMS);
        protected IRepositoryReadOnly<RecordingVoucher> RecordingVouchersRepository_RO => _getter.RO<RecordingVoucher>(DCMS);
        protected IRepositoryReadOnly<CashReceiptItem> CashReceiptItemsRepository_RO => _getter.RO<CashReceiptItem>(DCMS);
        protected IRepositoryReadOnly<NewsCategory> NewsCategoryRepository_RO => _getter.RO<NewsCategory>(DCMS);
        protected IRepositoryReadOnly<RemarkConfig> RemarkConfigsRepository_RO => _getter.RO<RemarkConfig>(DCMS);
        protected IRepositoryReadOnly<Category> CategoriesRepository_RO => _getter.RO<Category>(DCMS);
        protected IRepositoryReadOnly<PurchaseReturnBillAccounting> ReturnBillAccountingMappingRepository_RO => _getter.RO<PurchaseReturnBillAccounting>(DCMS);
        protected IRepositoryReadOnly<Channel> ChannelsRepository_RO => _getter.RO<Channel>(DCMS);
        protected IRepositoryReadOnly<CombinationProductBill> CombinationProductBillsRepository_RO => _getter.RO<CombinationProductBill>(DCMS);
        protected IRepositoryReadOnly<CombinationProductItem> CombinationProductItemsRepository_RO => _getter.RO<CombinationProductItem>(DCMS);
        //protected IRepositoryReadOnly<Main> MainsRepository_RO => _getter.RO<Main>(DCMS);
        protected IRepositoryReadOnly<ReturnReservationBillAccounting> ReturnReservationBillAccountingMappingRepository_RO => _getter.RO<ReturnReservationBillAccounting>(DCMS);
        protected IRepositoryReadOnly<Combination> CombinationsRepository_RO => _getter.RO<Combination>(DCMS);
        protected IRepositoryReadOnly<SaleItem> SaleItemsRepository_RO => _getter.RO<SaleItem>(DCMS);
        protected IRepositoryReadOnly<CostAdjustmentBill> CostAdjustmentBillsRepository_RO => _getter.RO<CostAdjustmentBill>(DCMS);
        protected IRepositoryReadOnly<CostAdjustmentItem> CostAdjustmentItemsRepository_RO => _getter.RO<CostAdjustmentItem>(DCMS);
        protected IRepositoryReadOnly<SaleBillAccounting> SaleBillAccountingMappingRepository_RO => _getter.RO<SaleBillAccounting>(DCMS);
        protected IRepositoryReadOnly<CostContractBill> CostContractBillsRepository_RO => _getter.RO<CostContractBill>(DCMS);
        protected IRepositoryReadOnly<Percentage> PercentageRepository_RO => _getter.RO<Percentage>(DCMS);
        protected IRepositoryReadOnly<SaleBill> SaleBillsRepository_RO => _getter.RO<SaleBill>(DCMS);
        protected IRepositoryReadOnly<CostExpenditureBillAccounting> CostExpenditureBillAccountingMappingRepository_RO => _getter.RO<CostExpenditureBillAccounting>(DCMS);
        protected IRepositoryReadOnly<PercentageRangeOption> PercentageRangeOptionRepository_RO => _getter.RO<PercentageRangeOption>(DCMS);
        protected IRepositoryReadOnly<SaleReservationBillAccounting> SaleReservationBillAccountingMappingRepository_RO => _getter.RO<SaleReservationBillAccounting>(DCMS);
        protected IRepositoryReadOnly<CostExpenditureItem> CostExpenditureItemsRepository_RO => _getter.RO<CostExpenditureItem>(DCMS);
        protected IRepositoryReadOnly<ProductPrice> ProductPricesRepository_RO => _getter.RO<ProductPrice>(DCMS);
        protected IRepositoryReadOnly<SaleReservationItem> SaleReservationItemsRepository_RO => _getter.RO<SaleReservationItem>(DCMS);
        protected IRepositoryReadOnly<DispatchBill> DispatchBillsRepository_RO => _getter.RO<DispatchBill>(DCMS);
        protected IRepositoryReadOnly<PurchaseItem> PurchaseItemsRepository_RO => _getter.RO<PurchaseItem>(DCMS);
        protected IRepositoryReadOnly<ScrapProductBill> ScrapProductBillsRepository_RO => _getter.RO<ScrapProductBill>(DCMS);
        protected IRepositoryReadOnly<District> DistrictsRepository_RO => _getter.RO<District>(DCMS);
        protected IRepositoryReadOnly<ScrapProductItem> ScrapProductItemsRepository_RO => _getter.RO<ScrapProductItem>(DCMS);
        protected IRepositoryReadOnly<FinancialIncomeBillAccounting> FinancialIncomeBillAccountingMappingRepository_RO => _getter.RO<FinancialIncomeBillAccounting>(DCMS);
        protected IRepositoryReadOnly<SpecificationAttributeOption> SpecificationAttributeOptionsRepository_RO => _getter.RO<SpecificationAttributeOption>(DCMS);
        protected IRepositoryReadOnly<FinancialIncomeBill> FinancialIncomeBillsRepository_RO => _getter.RO<FinancialIncomeBill>(DCMS);
        protected IRepositoryReadOnly<FinanceReceiveAccountBillAccounting> FinanceReceiveAccountBillAccountingMappingRepository_RO => _getter.RO<FinanceReceiveAccountBillAccounting>(DCMS);
        protected IRepositoryReadOnly<SpecificationAttribute> SpecificationAttributesRepository_RO => _getter.RO<SpecificationAttribute>(DCMS);
        protected IRepositoryReadOnly<SplitProductBill> SplitProductBillsRepository_RO => _getter.RO<SplitProductBill>(DCMS);
        protected IRepositoryReadOnly<GenericAttribute> GenericAttributesRepository_RO => _getter.RO<GenericAttribute>(DCMS);
        protected IRepositoryReadOnly<PurchaseReturnItem> PurchaseReturnItemsRepository_RO => _getter.RO<PurchaseReturnItem>(DCMS);
        protected IRepositoryReadOnly<SplitProductItem> SplitProductItemsRepository_RO => _getter.RO<SplitProductItem>(DCMS);
        protected IRepositoryReadOnly<GiveQuota> GiveQuotaRepository_RO => _getter.RO<GiveQuota>(DCMS);
        protected IRepositoryReadOnly<DispatchItem> DispatchItemsRepository_RO => _getter.RO<DispatchItem>(DCMS);
        protected IRepositoryReadOnly<RetainPhoto> RetainPhotosRepository_RO => _getter.RO<RetainPhoto>(DCMS);
        protected IRepositoryReadOnly<StatisticalTypes> StatisticalTypesRepository_RO => _getter.RO<StatisticalTypes>(DCMS);
        protected IRepositoryReadOnly<ReceivableDetail> FinanceReceivableDetailRepository_RO => _getter.RO<ReceivableDetail>(DCMS);
        protected IRepositoryReadOnly<UserLineTierAssign> UserLineTierAssignRepository_RO => _getter.RO<UserLineTierAssign>(DCMS);
        protected IRepositoryReadOnly<ReturnBillAccounting> ReturnBillAccountingRepository_RO => _getter.RO<ReturnBillAccounting>(DCMS);
        protected IRepositoryReadOnly<StockInOutDetails> StockInOutDetailsRepository_RO => _getter.RO<StockInOutDetails>(DCMS);

        protected IRepositoryReadOnly<ExchangeBill> ExchangeBillsRepository_RO => _getter.RO<ExchangeBill>(DCMS);
        protected IRepositoryReadOnly<ExchangeItem> ExchangeItemsRepository_RO => _getter.RO<ExchangeItem>(DCMS);

        protected IRepositoryReadOnly<UserAssessment> UserAssessmentsRepository_RO => _getter.RO<UserAssessment>(DCMS);
        protected IRepositoryReadOnly<UserAssessmentsItems> UserAssessmentItemsRepository_RO => _getter.RO<UserAssessmentsItems>(DCMS);

        #endregion

        #region RW

        protected IRepository<VisitStore> VisitStoreRepository => _getter.RW<VisitStore>(DCMS);
        protected IRepository<GiveQuotaOption> GiveQuotaOptionRepository => _getter.RW<GiveQuotaOption>(DCMS);
        protected IRepository<StockEarlyWarning> StockEarlyWarningsRepository => _getter.RW<StockEarlyWarning>(DCMS);
        protected IRepository<StockFlow> StockFlowsRepository => _getter.RW<StockFlow>(DCMS);
        protected IRepository<InventoryAllTaskItem> InventoryAllTaskItemsRepository => _getter.RW<InventoryAllTaskItem>(DCMS);
        protected IRepository<StockInOutRecord> StockInOutRecordsRepository => _getter.RW<StockInOutRecord>(DCMS);
        protected IRepository<StockInOutDetails> StockInOutDetailsRepository => _getter.RW<StockInOutDetails>(DCMS);

        protected IRepository<DeliverySign> DeliverySignsRepository => _getter.RW<DeliverySign>(DCMS);
        protected IRepository<StockInOutRecordStockFlow> StockInOutRecordsStockFlowsMappingRepository => _getter.RW<StockInOutRecordStockFlow>(DCMS);
        protected IRepository<InventoryPartTaskItem> InventoryPartTaskItemsRepository => _getter.RW<InventoryPartTaskItem>(DCMS);
        protected IRepository<Stock> StocksRepository => _getter.RW<Stock>(DCMS);
        protected IRepository<InventoryProfitLossBill> InventoryProfitLossBillsRepository => _getter.RW<InventoryProfitLossBill>(DCMS);
        protected IRepository<RecentPrice> RecentPricesRepository => _getter.RW<RecentPrice>(DCMS);
        //protected IRepository<Store> StoreRepository => _getter.RW<Store>(DCMS);
        protected IRepository<InventoryProfitLossItem> InventoryProfitLossItemsRepository => _getter.RW<InventoryProfitLossItem>(DCMS);
        protected IRepository<ReturnBill> ReturnBillsRepository => _getter.RW<ReturnBill>(DCMS);
        protected IRepository<StoreMapping> StoreMappingRepository => _getter.RW<StoreMapping>(DCMS);
        protected IRepository<LineTierOption> LineTierOptionsRepository => _getter.RW<LineTierOption>(DCMS);
        protected IRepository<LineTier> LineTiersRepository => _getter.RW<LineTier>(DCMS);
        protected IRepository<InventoryAllTaskBill> InventoryAllTaskBillsRepository => _getter.RW<InventoryAllTaskBill>(DCMS);
        protected IRepository<FinanceReceiveAccountBill> FinanceReceiveAccountBillsRepository => _getter.RW<FinanceReceiveAccountBill>(DCMS);
        protected IRepository<Manufacturer> ManufacturerRepository => _getter.RW<Manufacturer>(DCMS);
        protected IRepository<Receivable> ReceivablesRepository => _getter.RW<Receivable>(DCMS);
        protected IRepository<InventoryPartTaskBill> InventoryPartTaskBillsRepository => _getter.RW<InventoryPartTaskBill>(DCMS);
        protected IRepository<TrialBalance> TrialBalancesRepository => _getter.RW<TrialBalance>(DCMS);
        protected IRepository<ClosingAccounts> ClosingAccountsRepository => _getter.RW<ClosingAccounts>(DCMS);
        protected IRepository<CostPriceSummery> CostPriceSummeryRepository => _getter.RW<CostPriceSummery>(DCMS);
        protected IRepository<CostPriceChangeRecords> CostPriceChangeRecordsRepository => _getter.RW<CostPriceChangeRecords>(DCMS);

        protected IRepository<PaymentReceiptBillAccounting> PaymentReceiptBillAccountingMappingRepository => _getter.RW<PaymentReceiptBillAccounting>(DCMS);
        protected IRepository<InventoryReportBill> InventoryReportBillsRepository => _getter.RW<InventoryReportBill>(DCMS);
        protected IRepository<AdvanceReceiptBill> AdvanceReceiptBillsRepository => _getter.RW<AdvanceReceiptBill>(DCMS);
        protected IRepository<PaymentReceiptBill> PaymentReceiptBillsRepository => _getter.RW<PaymentReceiptBill>(DCMS);
        protected IRepository<VoucherItem> VoucherItemsRepository => _getter.RW<VoucherItem>(DCMS);
        protected IRepository<PaymentReceiptItem> PaymentReceiptItemsRepository => _getter.RW<PaymentReceiptItem>(DCMS);
        protected IRepository<InventoryReportStoreQuantity> InventoryReportStoreQuantitiesRepository => _getter.RW<InventoryReportStoreQuantity>(DCMS);
        protected IRepository<CostExpenditureBill> CostExpenditureBillsRepository => _getter.RW<CostExpenditureBill>(DCMS);
        protected IRepository<WareHouse> WareHousesRepository => _getter.RW<WareHouse>(DCMS);
        protected IRepository<AccountingOption> AccountingOptionsRepository => _getter.RW<AccountingOption>(DCMS);
        protected IRepository<PercentagePlan> PercentagePlanRepository => _getter.RW<PercentagePlan>(DCMS);
        protected IRepository<NewsItem> NewsItemRepository => _getter.RW<NewsItem>(DCMS);
        protected IRepository<PickingItem> PickingItemsRepository => _getter.RW<PickingItem>(DCMS);
        protected IRepository<InventoryReportItem> InventoryReportItemsRepository => _getter.RW<InventoryReportItem>(DCMS);
        protected IRepository<PickingBill> PickingsRepository => _getter.RW<PickingBill>(DCMS);
        protected IRepository<Picture> PicturesRepository => _getter.RW<Picture>(DCMS);
        protected IRepository<FinancialIncomeItem> FinancialIncomeItemsRepository => _getter.RW<FinancialIncomeItem>(DCMS);

        //protected IRepository<Terminal> TerminalsRepository => _getter.RW<Terminal>(DCMS);
        protected IRepository<PricingStructure> PricingStructuresRepository => _getter.RW<PricingStructure>(DCMS);
        protected IRepository<PrintTemplate> PrintTemplatesRepository => _getter.RW<PrintTemplate>(DCMS);
        protected IRepository<ReturnItem> ReturnItemsRepository => _getter.RW<ReturnItem>(DCMS);
        protected IRepository<ProductAttribute> ProductAttributeRepository => _getter.RW<ProductAttribute>(DCMS);
        protected IRepository<ProductCombination> ProductCombinationsRepository => _getter.RW<ProductCombination>(DCMS);
        protected IRepository<ProductFlavor> ProductFlavorsRepository => _getter.RW<ProductFlavor>(DCMS);
        protected IRepository<ProductManufacturer> ProductManufacturersRepository => _getter.RW<ProductManufacturer>(DCMS);
        protected IRepository<Product> ProductsRepository => _getter.RW<Product>(DCMS);
        protected IRepository<PurchaseReturnBill> PurchaseReturnBillsRepository => _getter.RW<PurchaseReturnBill>(DCMS);
        protected IRepository<ProductCategory> ProductsCategoryMappingRepository => _getter.RW<ProductCategory>(DCMS);
        protected IRepository<InventoryReportSummary> InventoryReportSummariesRepository => _getter.RW<InventoryReportSummary>(DCMS);
        protected IRepository<ProductPicture> ProductsPicturesMappingRepository => _getter.RW<ProductPicture>(DCMS);
        protected IRepository<AccountingType> AccountingTypesRepository => _getter.RW<AccountingType>(DCMS);
        protected IRepository<ProductVariantAttribute> ProductsProductAttributeMappingRepository => _getter.RW<ProductVariantAttribute>(DCMS);
        protected IRepository<ReturnReservationBill> ReturnReservationBillsRepository => _getter.RW<ReturnReservationBill>(DCMS);
        protected IRepository<AdvancePaymentBillAccounting> AdvancePaymentBillAccountingMappingRepository => _getter.RW<AdvancePaymentBillAccounting>(DCMS);
        protected IRepository<SaleReservationBill> SaleReservationBillsRepository => _getter.RW<SaleReservationBill>(DCMS);
        protected IRepository<ProductSpecificationAttribute> ProductsSpecificationAttributeMappingRepository => _getter.RW<ProductSpecificationAttribute>(DCMS);
        protected IRepository<AdvancePaymentBill> AdvancePaymentBillsRepository => _getter.RW<AdvancePaymentBill>(DCMS);
        protected IRepository<ProductTierPricePlan> ProductTierPricePlansRepository => _getter.RW<ProductTierPricePlan>(DCMS);
        protected IRepository<AdvanceReceiptBillAccounting> AdvanceReceiptBillAccountingMappingRepository => _getter.RW<AdvanceReceiptBillAccounting>(DCMS);
        protected IRepository<Setting> SettingsRepository => _getter.RW<Setting>(DCMS);
        protected IRepository<ProductTierPrice> ProductTierPricesRepository => _getter.RW<ProductTierPrice>(DCMS);
        protected IRepository<ProductVariantAttributeCombination> ProductVariantAttributeCombinationRepository => _getter.RW<ProductVariantAttributeCombination>(DCMS);
        protected IRepository<AllocationBill> AllocationBillsRepository => _getter.RW<AllocationBill>(DCMS);
        protected IRepository<ProductVariantAttributeValue> ProductVariantAttributeValueRepository => _getter.RW<ProductVariantAttributeValue>(DCMS);
        protected IRepository<AllocationItem> AllocationItemsRepository => _getter.RW<AllocationItem>(DCMS);
        protected IRepository<ProfitSheet> ProfitSheetsRepository => _getter.RW<ProfitSheet>(DCMS);
        protected IRepository<BalanceSheet> BalanceSheetsRepository => _getter.RW<BalanceSheet>(DCMS);
        protected IRepository<PurchaseBillAccounting> PurchaseBillAccountingMappingRepository => _getter.RW<PurchaseBillAccounting>(DCMS);
        protected IRepository<Brand> BrandsRepository => _getter.RW<Brand>(DCMS);
        protected IRepository<GiveQuotaRecords> GiveQuotaRecordsRepository => _getter.RW<GiveQuotaRecords>(DCMS);
        protected IRepository<PurchaseBill> PurchaseBillsRepository => _getter.RW<PurchaseBill>(DCMS);
        protected IRepository<CampaignChannel> CampaignChannelMappingRepository => _getter.RW<CampaignChannel>(DCMS);
        protected IRepository<CampaignBuyProduct> CampaignBuyProductsRepository => _getter.RW<CampaignBuyProduct>(DCMS);
        protected IRepository<PurchaseReturnBillAccounting> PurchaseReturnBillAccountingMappingRepository => _getter.RW<PurchaseReturnBillAccounting>(DCMS);
        protected IRepository<CampaignGiveProduct> CampaignGiveProductsRepository => _getter.RW<CampaignGiveProduct>(DCMS);
        protected IRepository<Campaign> CampaignsRepository => _getter.RW<Campaign>(DCMS);
        protected IRepository<ReturnReservationItem> ReturnReservationItemsRepository => _getter.RW<ReturnReservationItem>(DCMS);
        protected IRepository<Rank> RanksRepository => _getter.RW<Rank>(DCMS);
        protected IRepository<CashReceiptBillAccounting> CashReceiptBillAccountingMappingRepository => _getter.RW<CashReceiptBillAccounting>(DCMS);
        protected IRepository<CashReceiptBill> CashReceiptBillsRepository => _getter.RW<CashReceiptBill>(DCMS);
        protected IRepository<NewsPicture> NewsPictureRepository => _getter.RW<NewsPicture>(DCMS);
        protected IRepository<RecordingVoucher> RecordingVouchersRepository => _getter.RW<RecordingVoucher>(DCMS);
        protected IRepository<CashReceiptItem> CashReceiptItemsRepository => _getter.RW<CashReceiptItem>(DCMS);
        protected IRepository<NewsCategory> NewsCategoryRepository => _getter.RW<NewsCategory>(DCMS);
        protected IRepository<RemarkConfig> RemarkConfigsRepository => _getter.RW<RemarkConfig>(DCMS);
        protected IRepository<Category> CategoriesRepository => _getter.RW<Category>(DCMS);
        protected IRepository<PurchaseReturnBillAccounting> ReturnBillAccountingMappingRepository => _getter.RW<PurchaseReturnBillAccounting>(DCMS);
        protected IRepository<Channel> ChannelsRepository => _getter.RW<Channel>(DCMS);
        protected IRepository<CombinationProductBill> CombinationProductBillsRepository => _getter.RW<CombinationProductBill>(DCMS);
        protected IRepository<CombinationProductItem> CombinationProductItemsRepository => _getter.RW<CombinationProductItem>(DCMS);
        protected IRepository<ReturnReservationBillAccounting> ReturnReservationBillAccountingMappingRepository => _getter.RW<ReturnReservationBillAccounting>(DCMS);
        protected IRepository<Combination> CombinationsRepository => _getter.RW<Combination>(DCMS);
        protected IRepository<SaleItem> SaleItemsRepository => _getter.RW<SaleItem>(DCMS);
        protected IRepository<CostAdjustmentBill> CostAdjustmentBillsRepository => _getter.RW<CostAdjustmentBill>(DCMS);
        protected IRepository<CostAdjustmentItem> CostAdjustmentItemsRepository => _getter.RW<CostAdjustmentItem>(DCMS);
        protected IRepository<SaleBillAccounting> SaleBillAccountingMappingRepository => _getter.RW<SaleBillAccounting>(DCMS);
        protected IRepository<CostContractBill> CostContractBillsRepository => _getter.RW<CostContractBill>(DCMS);
        protected IRepository<Percentage> PercentageRepository => _getter.RW<Percentage>(DCMS);
        protected IRepository<SaleBill> SaleBillsRepository => _getter.RW<SaleBill>(DCMS);
        protected IRepository<CostContractItem> CostContractItemsRepository => _getter.RW<CostContractItem>(DCMS);
        protected IRepository<CostExpenditureBillAccounting> CostExpenditureBillAccountingMappingRepository => _getter.RW<CostExpenditureBillAccounting>(DCMS);
        protected IRepository<PercentageRangeOption> PercentageRangeOptionRepository => _getter.RW<PercentageRangeOption>(DCMS);
        protected IRepository<SaleReservationBillAccounting> SaleReservationBillAccountingMappingRepository => _getter.RW<SaleReservationBillAccounting>(DCMS);
        protected IRepository<CostExpenditureItem> CostExpenditureItemsRepository => _getter.RW<CostExpenditureItem>(DCMS);
        protected IRepository<ProductPrice> ProductPricesRepository => _getter.RW<ProductPrice>(DCMS);
        protected IRepository<SaleReservationItem> SaleReservationItemsRepository => _getter.RW<SaleReservationItem>(DCMS);
        protected IRepository<DispatchBill> DispatchBillsRepository => _getter.RW<DispatchBill>(DCMS);
        protected IRepository<PurchaseItem> PurchaseItemsRepository => _getter.RW<PurchaseItem>(DCMS);
        protected IRepository<ScrapProductBill> ScrapProductBillsRepository => _getter.RW<ScrapProductBill>(DCMS);
        protected IRepository<District> DistrictsRepository => _getter.RW<District>(DCMS);
        protected IRepository<ScrapProductItem> ScrapProductItemsRepository => _getter.RW<ScrapProductItem>(DCMS);
        protected IRepository<FinancialIncomeBillAccounting> FinancialIncomeBillAccountingMappingRepository => _getter.RW<FinancialIncomeBillAccounting>(DCMS);
        protected IRepository<SpecificationAttributeOption> SpecificationAttributeOptionsRepository => _getter.RW<SpecificationAttributeOption>(DCMS);
        protected IRepository<FinancialIncomeBill> FinancialIncomeBillsRepository => _getter.RW<FinancialIncomeBill>(DCMS);
        protected IRepository<FinanceReceiveAccountBillAccounting> FinanceReceiveAccountBillAccountingMappingRepository => _getter.RW<FinanceReceiveAccountBillAccounting>(DCMS);
        protected IRepository<SpecificationAttribute> SpecificationAttributesRepository => _getter.RW<SpecificationAttribute>(DCMS);
        protected IRepository<SplitProductBill> SplitProductBillsRepository => _getter.RW<SplitProductBill>(DCMS);
        protected IRepository<GenericAttribute> GenericAttributesRepository => _getter.RW<GenericAttribute>(DCMS);
        protected IRepository<PurchaseReturnItem> PurchaseReturnItemsRepository => _getter.RW<PurchaseReturnItem>(DCMS);
        protected IRepository<SplitProductItem> SplitProductItemsRepository => _getter.RW<SplitProductItem>(DCMS);
        protected IRepository<GiveQuota> GiveQuotaRepository => _getter.RW<GiveQuota>(DCMS);
        protected IRepository<DispatchItem> DispatchItemsRepository => _getter.RW<DispatchItem>(DCMS);
        protected IRepository<RetainPhoto> RetainPhotosRepository => _getter.RW<RetainPhoto>(DCMS);
        protected IRepository<StatisticalTypes> StatisticalTypesRepository => _getter.RW<StatisticalTypes>(DCMS);
        protected IRepository<ReceivableDetail> FinanceReceivableDetailRepository => _getter.RW<ReceivableDetail>(DCMS);
        protected IRepository<UserLineTierAssign> UserLineTierAssignRepository => _getter.RW<UserLineTierAssign>(DCMS);
        protected IRepository<ReturnBillAccounting> ReturnBillAccountingRepository => _getter.RW<ReturnBillAccounting>(DCMS);
        protected IRepository<ExchangeBill> ExchangeBillsRepository => _getter.RW<ExchangeBill>(DCMS);
        protected IRepository<ExchangeItem> ExchangeItemsRepository => _getter.RW<ExchangeItem>(DCMS);
        protected IRepository<UserAssessment> UserAssessmentsRepository => _getter.RW<UserAssessment>(DCMS);
        protected IRepository<UserAssessmentsItems> UserAssessmentsItemsRepository => _getter.RW<UserAssessmentsItems>(DCMS);
        #endregion


        #endregion

    }

}
