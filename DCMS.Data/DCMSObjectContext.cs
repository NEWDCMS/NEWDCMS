using DCMS.Core.Domain.Census;
using DCMS.Core.Domain.Common;
using DCMS.Core.Domain.Finances;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Purchases;
using DCMS.Core.Domain.Report;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Domain.Users;
using DCMS.Core.Domain.WareHouses;
using DCMS.Data.Mapping;
using DCMS.Data.Mapping.Account;
using DCMS.Data.Mapping.BILLs;
using DCMS.Data.Mapping.Campaigns;
using DCMS.Data.Mapping.Census;
using DCMS.Data.Mapping.CRM;
using DCMS.Data.Mapping.CSMS;
using DCMS.Data.Mapping.Finances;
using DCMS.Data.Mapping.Logging;
using DCMS.Data.Mapping.Media;
using DCMS.Data.Mapping.News;
using DCMS.Data.Mapping.Plan;
using DCMS.Data.Mapping.Products;
using DCMS.Data.Mapping.Security;
using DCMS.Data.Mapping.Setting;
using DCMS.Data.Mapping.Stores;
using DCMS.Data.Mapping.Tasks;
using DCMS.Data.Mapping.Terminals;
using DCMS.Data.Mapping.TSS;
using DCMS.Data.Mapping.Users;
using DCMS.Data.Mapping.Visit;
using DCMS.Data.Mapping.Chat;
using DCMS.Data.Mapping.WareHouses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using UserMap = DCMS.Data.Mapping.Users.UserMap;
using Microsoft.EntityFrameworkCore.Query;
using Pomelo.EntityFrameworkCore.MySql.Query;
using Pomelo.EntityFrameworkCore.MySql.Storage.ValueComparison;

namespace DCMS.Data
{

    public class BoolToIntConverter : ValueConverter<bool, int>
    {
        public BoolToIntConverter(ConverterMappingHints mappingHints = null) : base(v => Convert.ToInt32(v), v => Convert.ToBoolean(v), mappingHints)
        {
        }

        public static ValueConverterInfo DefaultInfo { get; } = new ValueConverterInfo(typeof(bool), typeof(int), i => new BoolToIntConverter(i.MappingHints));
    }


    public class DbContextBase : DbContext
    {
        #region DbQuery -> DbSet
        //AUTH
        public DbSet<IntQueryType> IntQueryTypes { get; set; }
        public DbSet<DecimalQueryType> DecimalQueryTypes { get; set; }
        public DbSet<StringQueryType> StringQueryTypes { get; set; }
        public DbSet<UserQueryType> UserQueryTypes { get; set; }


        //DCMS
        public DbSet<DictType> DictTypes { get; set; }
        public DbSet<MonthSaleReport> MonthSaleReports { get; set; }
        public DbSet<SalePercentReport> SalePercentReports { get; set; }
        public DbSet<BussinessVisitStoreReport> BussinessVisitStoreReports { get; set; }
        public DbSet<DashboardReport> DashboardReports { get; set; }
        public DbSet<MarketReportTerminalActive> MarketReportTerminalActives { get; set; }
        public DbSet<MarketReportTerminalValueAnalysis> MarketReportTerminalValueAnalysis { get; set; }
        public DbSet<MarketReportTerminalLossWarning> MarketReportTerminalLossWarnings { get; set; }
        public DbSet<MarketReportShopRate> MarketReportShopRates { get; set; }
        public DbSet<AllStoreDashboard> AllStoreDashboards { get; set; }
        public DbSet<AllStoreOrderTotal> AllStoreOrderTotals { get; set; }
        public DbSet<AllStoreUnfinishedOrder> AllStoreUnfinishedOrders { get; set; }
        public DbSet<CustomerAccountDealings> CustomerAccountDealingss { get; set; }
        public DbSet<FundReportCustomerReceiptCash> FundReportCustomerReceiptCashs { get; set; }
        public DbSet<FundReportManufacturerAccount> FundReportManufacturerAccounts { get; set; }
        public DbSet<FundReportManufacturerPayCash> FundReportManufacturerPayCashs { get; set; }
        public DbSet<FundReportAdvanceReceiptOverage> FundReportAdvanceReceiptOverages { get; set; }
        public DbSet<FundReportAdvancePaymentOverage> FundReportAdvancePaymentOverages { get; set; }
        public DbSet<StaffReportBusinessUserAchievement> StaffReportBusinessUserAchievements { get; set; }
        public DbSet<StaffReportPercentageSummary> StaffReportPercentageSummarys { get; set; }
        public DbSet<StaffReportPercentageItem> StaffReportPercentageItems { get; set; }
        public DbSet<StaffReportBusinessUserVisitRecord> StaffReportBusinessUserVisitRecords { get; set; }
        public DbSet<StaffReportBusinessUserVisitSuccess> StaffReportBusinessUserVisitSuccess { get; set; }
        public DbSet<StockReportProduct> StockReportProducts { get; set; }
        public DbSet<QueryVisitStoreAndTracking> QueryVisitStoreAndTrackings { get; set; }
        public DbSet<GiveQuotaRecordsSummery> GiveQuotaRecordsSummerys { get; set; }
         public DbSet<PurchaseReportItem> PurchaseReportItems { get; set; }
        public DbSet<PurchaseReportSummaryManufacturerQuery> PurchaseReportSummaryManufacturerQuerys { get; set; }
        public DbSet<PurchaseReportSummaryProduct> PurchaseReportSummaryProducts { get; set; }
        public DbSet<SaleReportItem> SaleReportItems { get; set; }
        public DbSet<SaleReportSummaryCustomerQuery> SaleReportSummaryCustomerQuerys { get; set; }
        public DbSet<SaleReportSummaryBusinessUserQuery> SaleReportSummaryBusinessUserQuerys { get; set; }
        public DbSet<SaleReportSummaryCustomerProduct> SaleReportSummaryCustomerProducts { get; set; }
        public DbSet<SaleReportSummaryWareHouseQuery> SaleReportSummaryWareHouseQuerys { get; set; }
        public DbSet<SaleReportSummaryBrandQuery> SaleReportSummaryBrandQuerys { get; set; }
        public DbSet<SaleReportOrderItem> SaleReportOrderItems { get; set; }
        public DbSet<SaleReportSummaryOrderProduct> SaleReportSummaryOrderProducts { get; set; }
        public DbSet<SaleReportCostContractItem> SaleReportCostContractItems { get; set; }
        public DbSet<SaleReportHotSale> SaleReportHotSales { get; set; }
        public DbSet<SaleReportSaleQuantityTrend> SaleReportSaleQuantityTrends { get; set; }
        public DbSet<SaleReportSummaryProduct> SaleReportSummaryProducts { get; set; }
        public DbSet<SaleReportBusinessDaily> SaleReportBusinessDailys { get; set; }
        public DbSet<CustomerRanking> CustomerRankings { get; set; }
        public DbSet<StockChangeSummary> StockChangeSummarys { get; set; }
        public DbSet<StockChangeSummaryOrder> StockChangeSummaryOrders { get; set; }
        public DbSet<InventoryReportList> InventoryReportLists { get; set; }
        public DbSet<InventoryReportList> AllocationDetailsLists { get; set; }
        public DbSet<EarlyWarning> EarlyWarnings { get; set; }
        public DbSet<ExpirationWarning> ExpirationWarnings { get; set; }

        public DbSet<StockQuery> StockQuerys { get; set; }
        public DbSet<ProductView> ProductViews { get; set; }
        public DbSet<StaffSaleQuery> StaffSaleQuerys { get; set; }
        public DbSet<VisitSummeryQuery> VisitSummeryQuerys { get; set; }
        public DbSet<CashReceiptItemView> CashReceiptItemViews { get; set; }
        public DbSet<BillCashReceiptSummary> BillCashReceiptSummaries { get; set; }
        public DbSet<FinanceReceiveAccountView> FinanceReceiveAccountViews { get; set; }

        public DbSet<UserAssessment> UserAssessments { get; set; }
        public DbSet<UserAssessmentsItems> UserAssessmentsItems { get; set; }
        public DbSet<GiveSummery> GiveSummeries { get; set; }

        public DbSet<BusinessUserVisitOfYear> BusinessUserVisitOfYear { get; set; }

        //CENSUS
        public DbSet<ReachQuery> ReachQueries { get; set; }
        public DbSet<ReachOnlineQuery> ReachOnlineQueries { get; set; }
        
        //SKD
        #endregion

        public DbContextBase(DbContextOptions<DbContextBase> options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.HasCharSet("utf8mb4", DelegationModes.ApplyToColumns);
            modelBuilder.UseCollation("utf8mb4");
            modelBuilder.UseGuidCollation("utf8mb4_0900_ai_ci");


            #region Query View

  
            modelBuilder.Entity<IntQueryType>().HasNoKey().ToView("IntQueryTypes");
            modelBuilder.Entity<DecimalQueryType>().HasNoKey().ToView("DecimalQueryTypes");

            //modelBuilder.Entity<StringQueryType>()
            //    .HasNoKey()
            //    .ToView("StringQueryTypes")
            //    .Property(s => s.Value)
            //    .HasColumnType("VARCHAR");


			modelBuilder.Entity<StringQueryType>(entity =>
			{
				entity.ToView("StringQueryTypes");
                entity.HasNoKey();
                entity.Property(v => v.Value).HasColumnType("VARCHAR");
			});


			modelBuilder.Entity<UserQueryType>().HasNoKey().ToView("UserQueryTypes");
            modelBuilder.Entity<ProductView>().HasNoKey().ToView("ProductViews");
            modelBuilder.Entity<BusinessAnalysisQuery>().HasNoKey().ToView("BusinessAnalysisQueries");
            modelBuilder.Entity<DictType>().HasNoKey().ToView("DictTypes");
     

            modelBuilder.Entity<MonthSaleReport>().HasNoKey().ToView("MonthSaleReports");
            modelBuilder.Entity<SalePercentReport>().HasNoKey().ToView("SalePercentReports");
            modelBuilder.Entity<BussinessVisitStoreReport>().HasNoKey().ToView("BussinessVisitStoreReports");
            modelBuilder.Entity<DashboardReport>().HasNoKey().ToView("DashboardReports");
            modelBuilder.Entity<MarketReportTerminalActive>().HasNoKey().ToView("MarketReportTerminalActives");
            modelBuilder.Entity<MarketReportTerminalValueAnalysis>().HasNoKey().ToView("MarketReportTerminalValueAnalysis");
            modelBuilder.Entity<MarketReportTerminalLossWarning>().HasNoKey().ToView("MarketReportTerminalLossWarnings");
            modelBuilder.Entity<MarketReportShopRate>().HasNoKey().ToView("MarketReportShopRates");
            modelBuilder.Entity<AllStoreDashboard>().HasNoKey().ToView("AllStoreDashboards");
            modelBuilder.Entity<AllStoreOrderTotal>().HasNoKey().ToView("AllStoreOrderTotals");
            modelBuilder.Entity<AllStoreUnfinishedOrder>().HasNoKey().ToView("AllStoreUnfinishedOrders");
            modelBuilder.Entity<CustomerAccountDealings>().HasNoKey().ToView("CustomerAccountDealingss");
            modelBuilder.Entity<FundReportCustomerReceiptCash>().HasNoKey().ToView("FundReportCustomerReceiptCashs");
            modelBuilder.Entity<FundReportManufacturerAccount>().HasNoKey().ToView("FundReportManufacturerAccounts");
            modelBuilder.Entity<FundReportManufacturerPayCash>().HasNoKey().ToView("FundReportManufacturerPayCashs");
            modelBuilder.Entity<FundReportAdvanceReceiptOverage>().HasNoKey().ToView("FundReportAdvanceReceiptOverages");
            modelBuilder.Entity<FundReportAdvancePaymentOverage>().HasNoKey().ToView("FundReportAdvancePaymentOverages");


            modelBuilder.Entity<StaffReportBusinessUserAchievement>()
                .HasNoKey()
                .ToView("StaffReportBusinessUserAchievements")
                .Property(s => s.NetAmount).HasColumnType("decimal(18,4)")
				.UseCollation("utf8mb4")
                .HasCharSet("utf8mb4");

           

            modelBuilder.Entity<StaffSaleQuery>().HasNoKey().ToView("StaffSaleQuerys");
            modelBuilder.Entity<VisitSummeryQuery>().HasNoKey().ToView("VisitSummeryQuerys");

            modelBuilder.Entity<BusinessUserVisitOfYear>().HasNoKey().ToView("BusinessUserVisitOfYears");

            modelBuilder.Entity<StaffReportPercentageSummary>().HasNoKey().ToView("StaffReportPercentageSummarys");
            modelBuilder.Entity<StaffReportPercentageItem>().HasNoKey().ToView("StaffReportPercentageItems");
            modelBuilder.Entity<StaffReportBusinessUserVisitRecord>().HasNoKey().ToView("StaffReportBusinessUserVisitRecords");
            modelBuilder.Entity<StaffReportBusinessUserVisitSuccess>().HasNoKey().ToView("StaffReportBusinessUserVisitSuccess");

            modelBuilder.Entity<StockReportProduct>()
                .Ignore(c => c.CurrentQuantityPart)
                .Ignore(c => c.UsableQuantityPart)
                .Ignore(c => c.OrderQuantityPart)
                .HasNoKey()
                .ToView("StockReportProducts");

            modelBuilder.Entity<QueryVisitStoreAndTracking>().HasNoKey().ToView("QueryVisitStoreAndTrackings");

            modelBuilder.Entity<GiveQuotaRecordsSummery>()
              .Ignore(c => c.GeneralQuantityTuple)
              .Ignore(c => c.OrderQuantityTuple)
              .Ignore(c => c.PromotionalQuantityTuple)
               .Ignore(c => c.ContractQuantityTuple)
              .HasNoKey()
              .ToView("GiveQuotaRecordsSummerys");

            modelBuilder.Entity<GiveSummery>().HasNoKey().ToView("GiveSummeries");

            modelBuilder.Entity<PurchaseReportItem>().HasNoKey().ToView("PurchaseReportItems");
            modelBuilder.Entity<PurchaseReportSummaryManufacturerQuery>().HasNoKey().ToView("PurchaseReportSummaryManufacturerQuerys");
            modelBuilder.Entity<PurchaseReportSummaryProduct>().HasNoKey().ToView("PurchaseReportSummaryProducts");
            modelBuilder.Entity<SaleReportItem>().HasNoKey().ToView("SaleReportItems");
            modelBuilder.Entity<SaleReportSummaryCustomerQuery>().HasNoKey().ToView("SaleReportSummaryCustomerQuerys");
            modelBuilder.Entity<SaleReportSummaryBusinessUserQuery>().HasNoKey().ToView("SaleReportSummaryBusinessUserQuerys");
            modelBuilder.Entity<SaleReportSummaryCustomerProduct>().HasNoKey().ToView("SaleReportSummaryCustomerProducts");
            modelBuilder.Entity<SaleReportSummaryWareHouseQuery>().HasNoKey().ToView("SaleReportSummaryWareHouseQuerys");
            modelBuilder.Entity<SaleReportSummaryBrandQuery>().HasNoKey().ToView("SaleReportSummaryBrandQuerys");
            modelBuilder.Entity<SaleReportOrderItem>().HasNoKey().ToView("SaleReportOrderItems");
            modelBuilder.Entity<SaleReportSummaryOrderProduct>().HasNoKey().ToView("SaleReportSummaryOrderProducts");
            modelBuilder.Entity<SaleReportCostContractItem>().HasNoKey().ToView("SaleReportCostContractItems");
            modelBuilder.Entity<SaleReportHotSale>().HasNoKey().ToView("SaleReportHotSales");
            modelBuilder.Entity<SaleReportSaleQuantityTrend>().HasNoKey().ToView("SaleReportSaleQuantityTrends");
            modelBuilder.Entity<SaleReportSummaryProduct>().HasNoKey().ToView("SaleReportSummaryProducts");
            modelBuilder.Entity<SaleReportBusinessDaily>().HasNoKey().ToView("SaleReportBusinessDailys");
            modelBuilder.Entity<CustomerRanking>().HasNoKey().ToView("CustomerRankings");
            modelBuilder.Entity<StockChangeSummary>().HasNoKey().ToView("StockChangeSummarys");


            modelBuilder.Entity<StockChangeSummaryOrder>()
                .Ignore(c => c.WareHouseName)
                .Ignore(c => c.Difference)
                .Ignore(c => c.UsableQuantityChangePart)
                .Ignore(c => c.UsableQuantityChangeAfterPart)
                .HasNoKey()
                .ToView("StockChangeSummaryOrders");

            modelBuilder.Entity<InventoryReportList>().HasNoKey().ToView("InventoryReportLists");

            modelBuilder.Entity<AllocationDetailsList>()
                .Ignore(c => c.LinkUrl)
                .HasNoKey()
                .ToView("AllocationDetailsLists");

            modelBuilder.Entity<EarlyWarning>().HasNoKey().ToView("EarlyWarnings");
            modelBuilder.Entity<ExpirationWarning>().HasNoKey().ToView("ExpirationWarnings");

            modelBuilder.Entity<StockQuery>().HasNoKey().ToView("StockQuerys");
            modelBuilder.Entity<StockQty>().HasNoKey().ToView("StockQtys");
            modelBuilder.Entity<StockQtySummery>().HasNoKey().ToView("StockQtySummerys");
            modelBuilder.Entity<CashReceiptItemView>().HasNoKey().ToView("CashReceiptItemViews");
            modelBuilder.Entity<BillCashReceiptSummary>().HasNoKey().ToView("BillCashReceiptSummaries");
            modelBuilder.Entity<FinanceReceiveAccountView>()
                .Ignore(c => c.StoreId)
                .HasNoKey()
                .ToView("FinanceReceiveAccountViews");
            modelBuilder.Entity<StockInOutRecordQuery>().HasNoKey().ToView("StockInOutRecordQuerys");
            modelBuilder.Entity<StockFlowQuery>().HasNoKey().ToView("StockFlowQuerys");
            modelBuilder.Entity<ReachQuery>().HasNoKey().ToView("ReachQueries");
            modelBuilder.Entity<ReachOnlineQuery>().HasNoKey().ToView("ReachOnlineQueries");
            #endregion

            #region Map

            #region 身份
            modelBuilder.ApplyConfiguration(new BranchMap());
            modelBuilder.ApplyConfiguration(new UserGroupMap());
            modelBuilder.ApplyConfiguration(new UserGroupUserMap());
            modelBuilder.ApplyConfiguration(new UserGroupUserRoleMap());
            modelBuilder.ApplyConfiguration(new UserMap());
            modelBuilder.ApplyConfiguration(new RefreshTokenMap());
            modelBuilder.ApplyConfiguration(new UserDistrictsMap());
            modelBuilder.ApplyConfiguration(new UserPasswordMap());
            modelBuilder.ApplyConfiguration(new UserAttributeMap());
            modelBuilder.ApplyConfiguration(new UserAttributeValueMap());
            modelBuilder.ApplyConfiguration(new UserUserRoleMap());
            modelBuilder.ApplyConfiguration(new UserRoleMap());
            modelBuilder.ApplyConfiguration(new AclRecordMap());
            modelBuilder.ApplyConfiguration(new ModuleMap());
            modelBuilder.ApplyConfiguration(new ModuleRoleMap());
            modelBuilder.ApplyConfiguration(new PermissionRecordMap());
            modelBuilder.ApplyConfiguration(new PermissionRecordRolesMap());
            modelBuilder.ApplyConfiguration(new ActivityLogMap());
            modelBuilder.ApplyConfiguration(new ActivityLogTypeMap());
            modelBuilder.ApplyConfiguration(new LogMap());
            modelBuilder.ApplyConfiguration(new DataChannelPermissionMap());
            modelBuilder.ApplyConfiguration(new PartnerMap());
            modelBuilder.ApplyConfiguration(new PrivateMessageMap());
            #endregion 

            #region 系统

           
            modelBuilder.ApplyConfiguration(new StoreMappingMap());
            modelBuilder.ApplyConfiguration(new GenericAttributeMap());
            modelBuilder.ApplyConfiguration(new ScheduleTaskMap());
            modelBuilder.ApplyConfiguration(new PictureMap());
            modelBuilder.ApplyConfiguration(new CorporationsMap());

            #endregion

            #region 配置

            modelBuilder.ApplyConfiguration(new SettingMap());
            modelBuilder.ApplyConfiguration(new AccountingTypeMap());
            modelBuilder.ApplyConfiguration(new AccountingOptionMap());
            modelBuilder.ApplyConfiguration(new ProductManufacturerMap());
            modelBuilder.ApplyConfiguration(new SpecificationAttributeMap());
            modelBuilder.ApplyConfiguration(new SpecificationAttributeOptionMap());
            modelBuilder.ApplyConfiguration(new StockEarlyWarningMap());
            modelBuilder.ApplyConfiguration(new PricingStructureMap());
            modelBuilder.ApplyConfiguration(new PrintTemplateMap());
            modelBuilder.ApplyConfiguration(new RemarkConfigMap());



            #endregion

            #region 销售映射

            modelBuilder.ApplyConfiguration(new FinanceReceiveAccountBillMap());
            modelBuilder.ApplyConfiguration(new FinanceReceiveAccountBillAccountingMap());

            modelBuilder.ApplyConfiguration(new DispatchItemMap());
            modelBuilder.ApplyConfiguration(new DispatchBillMap());
            modelBuilder.ApplyConfiguration(new PickingItemMap());
            modelBuilder.ApplyConfiguration(new PickingMap());

            modelBuilder.ApplyConfiguration(new ReturnBillMap());
            modelBuilder.ApplyConfiguration(new ReturnItemMap());
            modelBuilder.ApplyConfiguration(new ReturnBillAccountingMap());

            modelBuilder.ApplyConfiguration(new ReturnReservationBillMap());
            modelBuilder.ApplyConfiguration(new ReturnReservationItemMap());
            modelBuilder.ApplyConfiguration(new ReturnReservationBillAccountingMap());

            modelBuilder.ApplyConfiguration(new SaleBillMap());
            modelBuilder.ApplyConfiguration(new SaleItemMap());
            modelBuilder.ApplyConfiguration(new SaleBillAccountingMap());
            modelBuilder.ApplyConfiguration(new DeliverySignMap());
            modelBuilder.ApplyConfiguration(new RetainPhotoMap());

            modelBuilder.ApplyConfiguration(new SaleReservationBillMap());
            modelBuilder.ApplyConfiguration(new SaleReservationItemMap());
            modelBuilder.ApplyConfiguration(new SaleReservationBillAccountingMap());

            modelBuilder.ApplyConfiguration(new ExchangeBillMap());
            modelBuilder.ApplyConfiguration(new ExchangeItemMap());

            #endregion

            #region 采购映射

            modelBuilder.ApplyConfiguration(new PurchaseBillMap());
            modelBuilder.ApplyConfiguration(new PurchaseItemMap());
            modelBuilder.ApplyConfiguration(new PurchaseBillAccountingMap());

            modelBuilder.ApplyConfiguration(new PurchaseReturnBillMap());
            modelBuilder.ApplyConfiguration(new PurchaseReturnItemMap());
            modelBuilder.ApplyConfiguration(new PurchaseReturnBillAccountingMap());

            #endregion

            #region 财务映射

            //财务映射
            //modelBuilder.ApplyConfiguration(new FinanceBillMap());

            //收款单
            modelBuilder.ApplyConfiguration(new CashReceiptBillMap());
            modelBuilder.ApplyConfiguration(new CashReceiptItemMap());
            modelBuilder.ApplyConfiguration(new CashReceiptBillAccountingMap());

            //财务收入
            modelBuilder.ApplyConfiguration(new FinancialIncomeBillMap());
            modelBuilder.ApplyConfiguration(new FinancialIncomeItemMap());
            modelBuilder.ApplyConfiguration(new FinancialIncomeBillAccountingMap());

            //付款单据
            modelBuilder.ApplyConfiguration(new PaymentReceiptBillMap());
            modelBuilder.ApplyConfiguration(new PaymentReceiptItemMap());
            modelBuilder.ApplyConfiguration(new PaymentReceiptBillAccountingMap());

            //费用支出
            modelBuilder.ApplyConfiguration(new CostExpenditureBillMap());
            modelBuilder.ApplyConfiguration(new CostExpenditureItemMap());
            modelBuilder.ApplyConfiguration(new CostExpenditureBillAccountingMap());

            //费用合同
            modelBuilder.ApplyConfiguration(new CostContractBillMap());
            modelBuilder.ApplyConfiguration(new CostContractItemMap());

            //预收款
            modelBuilder.ApplyConfiguration(new AdvanceReceiptBillMap());
            modelBuilder.ApplyConfiguration(new AdvanceReceiptBillAccountingMap());

            //预付款
            modelBuilder.ApplyConfiguration(new AdvancePaymentBillMap());
            modelBuilder.ApplyConfiguration(new AdvancePaymentBillAccountingMap());

            //记账凭证
            modelBuilder.ApplyConfiguration(new RecordingVoucherMap());
            modelBuilder.ApplyConfiguration(new VoucherItemMap());

            //期末结转
            modelBuilder.ApplyConfiguration(new ClosingAccountsMap());
            modelBuilder.ApplyConfiguration(new CostPriceSummeryMap());
            modelBuilder.ApplyConfiguration(new CostPriceChangeRecordsMap());

            //科目余额
            modelBuilder.ApplyConfiguration(new TrialBalanceMap());
            modelBuilder.ApplyConfiguration(new ProfitSheetMap());
            modelBuilder.ApplyConfiguration(new BalanceSheetMap());

            #endregion

            #region 仓库

            #region 仓库单据

            //仓库
            modelBuilder.ApplyConfiguration(new WareHouseMap());
            //库存
            modelBuilder.ApplyConfiguration(new StockMap());
            modelBuilder.ApplyConfiguration(new StockInOutRecordMap());
            modelBuilder.ApplyConfiguration(new StockFlowMap());
            modelBuilder.ApplyConfiguration(new StockInOutRecordStockFlowMap());
            modelBuilder.ApplyConfiguration(new StockInOutDetailsMap());


            //库存商品组合单
            modelBuilder.ApplyConfiguration(new CombinationProductBillMap());
            modelBuilder.ApplyConfiguration(new CombinationProductItemMap());

            //调拨单
            modelBuilder.ApplyConfiguration(new AllocationBillMap());
            modelBuilder.ApplyConfiguration(new AllocationItemMap());

            //成本调价单
            modelBuilder.ApplyConfiguration(new CostAdjustmentBillMap());
            modelBuilder.ApplyConfiguration(new CostAdjustmentItemMap());

            //盘点任务(整仓)
            modelBuilder.ApplyConfiguration(new InventoryAllTaskBillMap());
            modelBuilder.ApplyConfiguration(new InventoryAllTaskItemMap());

            //盘点任务(部分)
            modelBuilder.ApplyConfiguration(new InventoryPartTaskBillMap());
            modelBuilder.ApplyConfiguration(new InventoryPartTaskItemMap());

            //盘点盈亏单
            modelBuilder.ApplyConfiguration(new InventoryProfitLossBillMap());
            modelBuilder.ApplyConfiguration(new InventoryProfitLossItemMap());

            //商品报损单
            modelBuilder.ApplyConfiguration(new ScrapProductBillMap());
            modelBuilder.ApplyConfiguration(new ScrapProductItemMap());

            //商品拆分单
            modelBuilder.ApplyConfiguration(new SplitProductBillMap());
            modelBuilder.ApplyConfiguration(new SplitProductItemMap());

            //门店库存上报
            //modelBuilder.ApplyConfiguration(new SubmitFromStore_ReportingMap());
            modelBuilder.ApplyConfiguration(new InventoryReportBillMap());
            modelBuilder.ApplyConfiguration(new InventoryReportItemMap());
            modelBuilder.ApplyConfiguration(new InventoryReportStoreQuantityMap());
            modelBuilder.ApplyConfiguration(new InventoryReportSummaryMap());


            #endregion

            #region 库存
            #endregion

            #region 库存提醒
            #endregion
            #endregion

            #region 商品相关
            //商品图片
            modelBuilder.ApplyConfiguration(new ProductPictureMap());
            //商品映射
            modelBuilder.ApplyConfiguration(new ProductMap());
            //商品类别
            modelBuilder.ApplyConfiguration(new CategoryMap());
            //商品分类映射
            modelBuilder.ApplyConfiguration(new ProductCategoryMap());
            //商品价格
            modelBuilder.ApplyConfiguration(new ProductPriceMap());
            //商品口味
            modelBuilder.ApplyConfiguration(new ProductFlavorMap());
            //组合类型
            modelBuilder.ApplyConfiguration(new CombinationMap());
            //组合商品
            modelBuilder.ApplyConfiguration(new ProductCombinationMap());

            //商品属性
            modelBuilder.ApplyConfiguration(new ProductAttributeMap());
            //商品规格属性
            modelBuilder.ApplyConfiguration(new ProductSpecificationAttributeMap());
            //商品变体组合
            modelBuilder.ApplyConfiguration(new ProductVariantAttributeCombinationMap());
            //商品变体属性
            modelBuilder.ApplyConfiguration(new ProductVariantAttributeMap());
            modelBuilder.ApplyConfiguration(new ProductVariantAttributeValueMap());
            //商品品牌
            modelBuilder.ApplyConfiguration(new BrandMap());

            //商品层次价格
            modelBuilder.ApplyConfiguration(new ProductTierPriceMap());
            //商品层次价格方案
            modelBuilder.ApplyConfiguration(new ProductTierPricePlanMap());
            #endregion

            #region 往来相关

            //片区
            modelBuilder.ApplyConfiguration(new DistrictMap());
            //渠道
            modelBuilder.ApplyConfiguration(new ChannelMap());
            //等级
            modelBuilder.ApplyConfiguration(new RankMap());
            //供应商
            modelBuilder.ApplyConfiguration(new ManufacturerMap());

            modelBuilder.ApplyConfiguration(new ReceivableMap());
            //应收款明细
            modelBuilder.ApplyConfiguration(new ReceivableDetailMap());
            #endregion

            #region 员工相关
            //统计类型
            modelBuilder.ApplyConfiguration(new StatisticalTypeMap());

            //提成方案
            modelBuilder.ApplyConfiguration(new PercentagePlanMap());
            modelBuilder.ApplyConfiguration(new PercentageMap());
            modelBuilder.ApplyConfiguration(new PercentageRangeOptionMap());

            //赠品额度
            modelBuilder.ApplyConfiguration(new GiveQuotaMap());
            modelBuilder.ApplyConfiguration(new GiveQuotaOptionMap());
            modelBuilder.ApplyConfiguration(new GiveQuotaRecordsMap());


            //促销活动
            modelBuilder.ApplyConfiguration(new CampaignMap());
            modelBuilder.ApplyConfiguration(new CampaignChannelMap());
            modelBuilder.ApplyConfiguration(new CampaignBuyProductMap());
            modelBuilder.ApplyConfiguration(new CampaignGiveProductMap());

            //上次价格
            modelBuilder.ApplyConfiguration(new RecentPriceMap());

            //线路
            modelBuilder.ApplyConfiguration(new LineTierMap());
            modelBuilder.ApplyConfiguration(new LineTierOptionMap());
            modelBuilder.ApplyConfiguration(new UserLineTierAssignMap());

            modelBuilder.ApplyConfiguration(new VisitStoreMap());

            #endregion

            #region 消息相关
            //消息类别
            modelBuilder.ApplyConfiguration(new NewsNewsCategoryMap());
            //消息
            modelBuilder.ApplyConfiguration(new NewsItemMap());
            //消息图片
            modelBuilder.ApplyConfiguration(new NewsPictureMap());
            #endregion

            #region 拜访
            modelBuilder.ApplyConfiguration(new QueuedMessageMap());
            modelBuilder.ApplyConfiguration(new EmailAccountMap());
            modelBuilder.ApplyConfiguration(new TrackingMap());
            modelBuilder.ApplyConfiguration(new DisplayPhotoMap());
            modelBuilder.ApplyConfiguration(new DoorheadPhotoMap());
            #endregion

            #region 推送
            modelBuilder.ApplyConfiguration(new QueuedMessageMap());
            modelBuilder.ApplyConfiguration(new EmailAccountMap());
            modelBuilder.ApplyConfiguration(new QueuedEmailMap());
            modelBuilder.ApplyConfiguration(new ScheduleTaskMap());
            #endregion

            #region 服务支持

            modelBuilder.ApplyConfiguration(new FeedbackMap());
            modelBuilder.ApplyConfiguration(new MarketFeedbackMap());

            modelBuilder.ApplyConfiguration(new ChatRoomMap());
            modelBuilder.ApplyConfiguration(new MessageMap());
            modelBuilder.ApplyConfiguration(new ChatUserMap());

            #endregion

            #region CRM集成

            modelBuilder.ApplyConfiguration(new TerminalMap());
            modelBuilder.ApplyConfiguration(new NewTerminalsMap());
            modelBuilder.ApplyConfiguration(new StoreMap());

            modelBuilder.ApplyConfiguration(new CRM_RELATIONMap());
            modelBuilder.ApplyConfiguration(new CRM_RETURNMap());
            modelBuilder.ApplyConfiguration(new CRM_ORGMap());
            modelBuilder.ApplyConfiguration(new CRM_BPMap());
            modelBuilder.ApplyConfiguration(new CRM_ZSNTM0040Map());
            modelBuilder.ApplyConfiguration(new CRM_HEIGHT_CONFMap());
            modelBuilder.ApplyConfiguration(new CRM_BUSTATyMap());

            #endregion

            #region OCMS集成

            modelBuilder.ApplyConfiguration(new OCMS_CharacterSettingMap());
            modelBuilder.ApplyConfiguration(new OCMS_ProductsMap());

            #endregion


            #region CSMS集成

            modelBuilder.ApplyConfiguration(new TerminalSignReportMap());
            modelBuilder.ApplyConfiguration(new OrderDetailMap());

            #endregion


            #endregion

            base.OnModelCreating(modelBuilder);
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }

}