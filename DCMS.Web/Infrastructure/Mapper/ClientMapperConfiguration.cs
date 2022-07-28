using AutoMapper;
using DCMS.Core.Domain.Campaigns;
using DCMS.Core.Domain.Census;
using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Finances;
using DCMS.Core.Domain.Plan;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Purchases;
using DCMS.Core.Domain.Report;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Domain.Security;
using DCMS.Core.Domain.Stores;
using DCMS.Core.Domain.Terminals;
using DCMS.Core.Domain.Users;
using DCMS.Core.Domain.WareHouses;
using DCMS.Core.Infrastructure.Mapper;
using DCMS.ViewModel.Models.Campaigns;
using DCMS.ViewModel.Models.Configuration;
using DCMS.ViewModel.Models.Finances;
using DCMS.ViewModel.Models.Global.Common;
using DCMS.ViewModel.Models.Plan;
using DCMS.ViewModel.Models.Products;
using DCMS.ViewModel.Models.Purchases;
using DCMS.ViewModel.Models.Report;
using DCMS.ViewModel.Models.Sales;
using DCMS.ViewModel.Models.Stores;
using DCMS.ViewModel.Models.Terminals;
using DCMS.ViewModel.Models.Users;
using DCMS.ViewModel.Models.Visit;
using DCMS.ViewModel.Models.WareHouses;
using DCMS.Web.Framework.Models;
using CPoduct = DCMS.Core.Domain.Products;
using LineTier = DCMS.Core.Domain.Visit.LineTier;
using LineTierOption = DCMS.Core.Domain.Visit.LineTierOption;
using UserLineTierAssign = DCMS.Core.Domain.Visit.UserLineTierAssign;
using DCMS.ViewModel.Models.CRM;
using DCMS.Core.Domain.CRM;

namespace DCMS.Web.Infrastructure.Mapper
{
    public class ClientMapperConfiguration : Profile, IOrderedMapperProfile
    {

        public ClientMapperConfiguration()
        {

            #region 系统

            //用户
            CreateMap<User, UserModel>();
            CreateMap<UserModel, User>();

            CreateMap<PermissionRecord, PermissionRecordModel>();
            CreateMap<PermissionRecordModel, PermissionRecord>();


            CreateMap<User, UserAuthenticationModel>();
            CreateMap<UserAuthenticationModel, User>();

            CreateMap<UserRole, UserRoleModel>();
            CreateMap<UserRoleModel, UserRole>()
            .ForMember(dest => dest.PermissionRecordRoles, mo => mo.Ignore());

            CreateMap<Store, StoreModel>();
            CreateMap<StoreModel, Store>();

            //机构
            CreateMap<Branch, BranchModel>();
            CreateMap<BranchModel, Branch>();

            //
            CreateMap<DataChannelPermission, DataChannelPermissionModel>();
            CreateMap<DataChannelPermissionModel, DataChannelPermission>();

            //模块
            // {"Missing type map configuration or unsupported mapping.\r\n\r\n
            //Mapping types:
            //Module -> BaseModule
            //DCMS.Core.Domain.Security.Module -> DCMS.ViewModel.Models.Users.BaseModule

            CreateMap<Module, ModuleModel>()
            .ForMember(dest => dest.LayoutPosition, mo => mo.Ignore());
            CreateMap<ModuleModel, Module>()
                .ForMember(dest => dest.ParentModule, mo => mo.Ignore())
                .ForMember(dest => dest.LayoutPosition, mo => mo.Ignore())
                .ForMember(dest => dest.ChildModules, mo => mo.Ignore());

            CreateMap<Module, BaseModule>();
            CreateMap<BaseModule, Module>()
                .ForMember(dest => dest.ParentModule, mo => mo.Ignore())
                .ForMember(dest => dest.LayoutPosition, mo => mo.Ignore())
                .ForMember(dest => dest.ChildModules, mo => mo.Ignore());


            //首页
            CreateMap<DashboardReport, DashboardReportModel>();
            CreateMap<DashboardReportModel, DashboardReport>();


            CreateMap<Partner, PartnerModel>().ForMember(m => m.PasswordFormats, m => m.Ignore());
            CreateMap<PartnerModel, Partner>();

            #endregion

            #region 销售

            //换货单
            CreateMap<ExchangeBill, ExchangeBillModel>();
            CreateMap<ExchangeBillModel, ExchangeBill>();
            CreateMap<ExchangeItem, ExchangeItemModel>();
            CreateMap<ExchangeItemModel, ExchangeItem>();

            CreateMap<ExchangeBill, SaleReservationBill>();
            CreateMap<SaleReservationBill, ExchangeBill>();

            CreateMap<SaleReservationItem, ExchangeItem>();
            CreateMap<ExchangeItem, SaleReservationItem>();

            CreateMap<ExchangeBillUpdate, ExchangeBillUpdateModel>();
            CreateMap<ExchangeBillUpdateModel, ExchangeBillUpdate>();

            //销售订单
            CreateMap<SaleReservationBill, SaleReservationBillModel>();
            CreateMap<SaleReservationBillModel, SaleReservationBill>();
            CreateMap<SaleReservationItem, SaleReservationItemModel>();
            CreateMap<SaleReservationItemModel, SaleReservationItem>();
            CreateMap<SaleReservationBillAccounting, SaleReservationBillAccountingModel>();
            CreateMap<SaleReservationBillAccountingModel, SaleReservationBillAccounting>();
            CreateMap<SaleReservationBillUpdate, SaleReservationBillUpdateModel>();
            CreateMap<SaleReservationBillUpdateModel, SaleReservationBillUpdate>();


            //销售单
            CreateMap<SaleBill, SaleBillModel>();
            CreateMap<SaleBillModel, SaleBill>();
            CreateMap<SaleItem, SaleItemModel>();
            CreateMap<SaleItemModel, SaleItem>();
            CreateMap<SaleBillAccounting, SaleBillAccountingModel>();
            CreateMap<SaleBillAccountingModel, SaleBillAccounting>();
            CreateMap<SaleBillUpdate, SaleBillUpdateModel>();
            CreateMap<SaleBillUpdateModel, SaleBillUpdate>();


            //退货订单
            CreateMap<ReturnReservationBill, ReturnReservationBillModel>();
            CreateMap<ReturnReservationBillModel, ReturnReservationBill>();
            CreateMap<ReturnReservationItem, ReturnReservationItemModel>();
            CreateMap<ReturnReservationItemModel, ReturnReservationItem>();
            CreateMap<ReturnReservationBillAccounting, ReturnReservationBillAccountingModel>();
            CreateMap<ReturnReservationBillAccountingModel, ReturnReservationBillAccounting>();
            CreateMap<ReturnReservationBillUpdate, ReturnReservationBillUpdateModel>();
            CreateMap<ReturnReservationBillUpdateModel, ReturnReservationBillUpdate>();


            //退货单
            CreateMap<ReturnBill, ReturnBillModel>();
            CreateMap<ReturnBillModel, ReturnBill>();
            CreateMap<ReturnItem, ReturnItemModel>();
            CreateMap<ReturnItemModel, ReturnItem>();
            CreateMap<ReturnBillAccounting, ReturnBillAccountingModel>();
            CreateMap<ReturnBillAccountingModel, ReturnBillAccounting>();
            CreateMap<ReturnBillUpdate, ReturnBillUpdateModel>();
            CreateMap<ReturnBillUpdateModel, ReturnBillUpdate>();



            //车辆对货单
            //收款对账单
            CreateMap<FinanceReceiveAccountBill, FinanceReceiveAccountBillModel>();
            CreateMap<FinanceReceiveAccountBillModel, FinanceReceiveAccountBill>();
            //CreateMap<FinanceReceiveAccountBillAccounting, FinanceReceiveAccountBillAccountingModel>();
            //CreateMap<FinanceReceiveAccountBillAccountingModel, FinanceReceiveAccountBillAccounting>();


            //仓库分拣单
            CreateMap<PickingBill, PickingBillModel>();
            CreateMap<PickingBillModel, PickingBill>();
            //CreateMap<PickingItem, PickingItemModel>();
            //CreateMap<PickingItemModel, PickingItem>();


            //装车调度单
            CreateMap<DispatchBill, DispatchBillModel>();
            CreateMap<DispatchBillModel, DispatchBill>();

            CreateMap<DispatchItem, DispatchItemModel>();
            CreateMap<DispatchItemModel, DispatchItem>();

            #endregion

            #region 采购

            //采购单
            CreateMap<PurchaseBill, PurchaseBillModel>()
            .ForMember(dest => dest.PurchaseBillAccountings, mo => mo.Ignore());
            CreateMap<PurchaseBillModel, PurchaseBill>();
            CreateMap<PurchaseItem, PurchaseItemModel>();
            CreateMap<PurchaseItemModel, PurchaseItem>();
            CreateMap<PurchaseBillAccounting, PurchaseBillAccountingModel>();
            CreateMap<PurchaseBillAccountingModel, PurchaseBillAccounting>();
            CreateMap<PurchaseBillUpdate, PurchaseBillUpdateModel>();
            CreateMap<PurchaseBillUpdateModel, PurchaseBillUpdate>();

            //采购退货单
            CreateMap<PurchaseReturnBill, PurchaseReturnBillModel>();
            CreateMap<PurchaseReturnBillModel, PurchaseReturnBill>();
            CreateMap<PurchaseReturnItem, PurchaseReturnItemModel>();
            CreateMap<PurchaseReturnItemModel, PurchaseReturnItem>();
            CreateMap<PurchaseReturnBillAccounting, PurchaseReturnBillAccountingModel>();
            CreateMap<PurchaseReturnBillAccountingModel, PurchaseReturnBillAccounting>();
            CreateMap<PurchaseReturnBillUpdate, PurchaseReturnBillUpdateModel>();
            CreateMap<PurchaseReturnBillUpdateModel, PurchaseReturnBillUpdate>();


            #endregion

            #region 财务

            

            //科目类别
            CreateMap<AccountingType, AccountingTypeModel>();
            CreateMap<AccountingTypeModel, AccountingType>();
            CreateMap<AccountingOption, AccountingOptionModel>();
            CreateMap<AccountingOptionModel, AccountingOption>();

            //收款单
            CreateMap<CashReceiptBill, CashReceiptBillModel>();
            CreateMap<CashReceiptBillModel, CashReceiptBill>();
            CreateMap<CashReceiptItem, CashReceiptItemModel>();
            CreateMap<CashReceiptItemModel, CashReceiptItem>();
            CreateMap<CashReceiptBillAccounting, CashReceiptBillAccountingModel>();
            CreateMap<CashReceiptBillAccountingModel, CashReceiptBillAccounting>();
            CreateMap<FinanceSetting, FinanceSettingModel>();
            CreateMap<FinanceSettingModel, FinanceSetting>();
            CreateMap<CashReceiptBillUpdate, CashReceiptUpdateModel>();
            CreateMap<CashReceiptUpdateModel, CashReceiptBillUpdate>();


            //费用支出
            CreateMap<CostExpenditureBill, CostExpenditureBillModel>();
            CreateMap<CostExpenditureBillModel, CostExpenditureBill>();
            CreateMap<CostExpenditureItem, CostExpenditureItemModel>();
            CreateMap<CostExpenditureItemModel, CostExpenditureItem>();
            CreateMap<CostExpenditureBillAccounting, CostExpenditureBillAccountingModel>();
            CreateMap<CostExpenditureBillAccountingModel, CostExpenditureBillAccounting>();
            CreateMap<CostExpenditureBillUpdate, CostExpenditureUpdateModel>();
            CreateMap<CostExpenditureUpdateModel, CostExpenditureBillUpdate>();


            //财务收入
            CreateMap<FinancialIncomeBill, FinancialIncomeBillModel>();
            CreateMap<FinancialIncomeBillModel, FinancialIncomeBill>();
            CreateMap<FinancialIncomeItem, FinancialIncomeItemModel>();
            CreateMap<FinancialIncomeItemModel, FinancialIncomeItem>();
            CreateMap<FinancialIncomeBillAccounting, FinancialIncomeBillAccountingModel>();
            CreateMap<FinancialIncomeBillAccountingModel, FinancialIncomeBillAccounting>();
            CreateMap<FinancialIncomeBillUpdate, FinancialIncomeUpdateModel>();
            CreateMap<FinancialIncomeUpdateModel, FinancialIncomeBillUpdate>();

            //付款单
            CreateMap<PaymentReceiptBill, PaymentReceiptBillModel>();
            CreateMap<PaymentReceiptBillModel, PaymentReceiptBill>();
            CreateMap<PaymentReceiptItem, PaymentReceiptItemModel>();
            CreateMap<PaymentReceiptItemModel, PaymentReceiptItem>();
            CreateMap<PaymentReceiptBillAccounting, PaymentReceiptBillAccountingModel>();
            CreateMap<PaymentReceiptBillAccountingModel, PaymentReceiptBillAccounting>();
            CreateMap<PaymentReceiptBillUpdate, PaymentReceiptUpdateModel>();
            CreateMap<PaymentReceiptUpdateModel, PaymentReceiptBillUpdate>();


            //预付款
            CreateMap<AdvancePaymentBill, AdvancePaymentBillModel>();
            CreateMap<AdvancePaymentBillModel, AdvancePaymentBill>();
            CreateMap<AdvancePaymentBillAccounting, AdvancePaymentBillAccountingModel>();
            CreateMap<AdvancePaymentBillAccountingModel, AdvancePaymentBillAccounting>();
            CreateMap<AdvancePaymenBillUpdate, AdvancePaymenUpdateModel>();
            CreateMap<AdvancePaymenUpdateModel, AdvancePaymenBillUpdate>();


            //预收款
            CreateMap<AdvanceReceiptBill, AdvanceReceiptBillModel>();
            CreateMap<AdvanceReceiptBillModel, AdvanceReceiptBill>();
            CreateMap<AdvanceReceiptBillAccounting, AdvanceReceiptBillAccountingModel>();
            CreateMap<AdvanceReceiptBillAccountingModel, AdvanceReceiptBillAccounting>();
            CreateMap<AdvanceReceiptBillUpdate, AdvanceReceiptUpdateModel>();
            CreateMap<AdvanceReceiptUpdateModel, AdvanceReceiptBillUpdate>();


            //费用合同
            CreateMap<CostContractBill, CostContractBillModel>();
            CreateMap<CostContractBillModel, CostContractBill>();
            CreateMap<CostContractItem, CostContractItemModel>();
            CreateMap<CostContractItemModel, CostContractItem>();
            CreateMap<CostContractBillUpdate, CostContractUpdateModel>();
            CreateMap<CostContractUpdateModel, CostContractBillUpdate>();


            //记账凭证
            CreateMap<RecordingVoucher, RecordingVoucherModel>();
            CreateMap<RecordingVoucherModel, RecordingVoucher>();
            CreateMap<VoucherItem, VoucherItemModel>();
            CreateMap<VoucherItemModel, VoucherItem>();


            //期末结转
            CreateMap<ProfitSheet, ProfitSheetModel>();
            CreateMap<ProfitSheetModel, ProfitSheet>();

            CreateMap<TrialBalance, TrialBalanceModel>();
            CreateMap<TrialBalanceModel, TrialBalance>();

            CreateMap<BalanceSheet, BalanceSheetModel>();
            CreateMap<BalanceSheetModel, BalanceSheet>();


            #endregion

            #region 仓库单据

            //仓库
            CreateMap<WareHouse, WareHouseModel>().ForMember(dest => dest.WareHouseAccess, mo => mo.Ignore());
            CreateMap<WareHouseModel, WareHouse>().ForMember(dest => dest.WareHouseAccess, mo => mo.Ignore());

            //库存
            CreateMap<Stock, StockModel>();
            CreateMap<StockModel, Stock>();

            CreateMap<StockInOutRecord, StockInOutRecordModel>();
            CreateMap<StockInOutRecordModel, StockInOutRecord>();

            CreateMap<StockFlow, StockFlowModel>();
            CreateMap<StockFlowModel, StockFlow>();


            //单据
            CreateMap<CombinationProductBill, CombinationProductBillModel>();
            CreateMap<CombinationProductBillModel, CombinationProductBill>();
            CreateMap<CombinationProductItem, CombinationProductItemModel>();
            CreateMap<CombinationProductItemModel, CombinationProductItem>();
            CreateMap<CombinationProductBillUpdate, CombinationProductUpdateModel>();
            CreateMap<CombinationProductUpdateModel, CombinationProductBillUpdate>();


            CreateMap<AllocationBill, AllocationBillModel>();
            CreateMap<AllocationBillModel, AllocationBill>();
            CreateMap<AllocationItem, AllocationItemModel>();
            CreateMap<AllocationItemModel, AllocationItem>();
            CreateMap<AllocationBillUpdate, AllocationUpdateModel>();
            CreateMap<AllocationUpdateModel, AllocationBillUpdate>();


            CreateMap<CostAdjustmentBill, CostAdjustmentBillModel>();
            CreateMap<CostAdjustmentBillModel, CostAdjustmentBill>();
            CreateMap<CostAdjustmentItem, CostAdjustmentItemModel>();
            CreateMap<CostAdjustmentItemModel, CostAdjustmentItem>();
            CreateMap<CostAdjustmentBillUpdate, CostAdjustmentUpdateModel>();
            CreateMap<CostAdjustmentUpdateModel, CostAdjustmentBillUpdate>();


            CreateMap<InventoryAllTaskBill, InventoryAllTaskBillModel>();
            CreateMap<InventoryAllTaskBillModel, InventoryAllTaskBill>();
            CreateMap<InventoryAllTaskItem, InventoryAllTaskItemModel>();
            CreateMap<InventoryAllTaskItemModel, InventoryAllTaskItem>();
            CreateMap<InventoryAllTaskBillUpdate, InventoryAllTaskUpdateModel>();
            CreateMap<InventoryAllTaskUpdateModel, InventoryAllTaskBillUpdate>();


            CreateMap<InventoryPartTaskBill, InventoryPartTaskBillModel>();
            CreateMap<InventoryPartTaskBillModel, InventoryPartTaskBill>();
            CreateMap<InventoryPartTaskItem, InventoryPartTaskItemModel>();
            CreateMap<InventoryPartTaskItemModel, InventoryPartTaskItem>();
            CreateMap<InventoryPartTaskBillUpdate, InventoryPartTaskUpdateModel>();
            CreateMap<InventoryPartTaskUpdateModel, InventoryPartTaskBillUpdate>();


            CreateMap<ScrapProductBill, ScrapProductBillModel>();
            CreateMap<ScrapProductBillModel, ScrapProductBill>();
            CreateMap<ScrapProductItem, ScrapProductItemModel>();
            CreateMap<ScrapProductItemModel, ScrapProductItem>();
            CreateMap<ScrapProductBillUpdate, ScrapProductBillModel>();
            CreateMap<ScrapProductBillModel, ScrapProductBillUpdate>();
            CreateMap<ScrapProductBillUpdate, ScrapProductUpdateModel>();
            CreateMap<ScrapProductUpdateModel, ScrapProductBillUpdate>();


            CreateMap<SplitProductBill, SplitProductBillModel>();
            CreateMap<SplitProductBillModel, SplitProductBill>();
            CreateMap<SplitProductItem, SplitProductItemModel>();
            CreateMap<SplitProductItemModel, SplitProductItem>();
            CreateMap<SplitProductBillUpdate, SplitProductUpdateModel>();
            CreateMap<SplitProductUpdateModel, SplitProductBillUpdate>();


            CreateMap<InventoryProfitLossBill, InventoryProfitLossBillModel>();
            CreateMap<InventoryProfitLossBillModel, InventoryProfitLossBill>();
            CreateMap<InventoryProfitLossItem, InventoryProfitLossItemModel>();
            CreateMap<InventoryProfitLossItemModel, InventoryProfitLossItem>();
            CreateMap<InventoryProfitLossBillUpdate, InventoryProfitLossUpdateModel>();
            CreateMap<InventoryProfitLossUpdateModel, InventoryProfitLossBillUpdate>();


            //门店库存上报
            CreateMap<InventoryReportBill, InventoryReportBillModel>();
            CreateMap<InventoryReportBillModel, InventoryReportBill>();

            CreateMap<InventoryReportItem, InventoryReportItemModel>();
            CreateMap<InventoryReportItemModel, InventoryReportItem>();

            CreateMap<InventoryReportStoreQuantity, InventoryReportStoreQuantityModel>();
            CreateMap<InventoryReportStoreQuantityModel, InventoryReportStoreQuantity>();

            CreateMap<InventoryReportSummary, InventoryReportSummaryModel>();
            CreateMap<InventoryReportSummaryModel, InventoryReportSummary>();


            #endregion

            #region 商品相关
            //产品类别
            CreateMap<Category, CategoryModel>()

            .ForMember(dest => dest.StatisticalTypes, mo => mo.Ignore())
            .ForMember(dest => dest.ParentList, mo => mo.Ignore());
            CreateMap<CategoryModel, Category>();

            //商品
            CreateMap<CPoduct.Product, ProductModel>()
            .ForMember(dest => dest.ProductTierPrices, mo => mo.Ignore());

            CreateMap<ProductModel, CPoduct.Product>()
            .ForMember(dest => dest.LowStockActivity, mo => mo.Ignore())
            .ForMember(dest => dest.ManageInventoryMethod, mo => mo.Ignore());

            CreateMap<ProductPrice, ProductPriceModel>();
            CreateMap<ProductPriceModel, ProductPrice>();

            //层次价格
            CreateMap<ProductTierPrice, ProductTierPriceModel>();
            CreateMap<ProductTierPriceModel, ProductTierPrice>();

            //价格方案
            CreateMap<ProductTierPricePlan, ProductTierPricePlanModel>();
            CreateMap<ProductTierPricePlanModel, ProductTierPricePlan>();


            //口味
            CreateMap<ProductFlavor, ProductFlavorModel>();
            CreateMap<ProductFlavorModel, ProductFlavor>();

            //统计类别
            CreateMap<StatisticalTypes, StatisticalTypeModel>();
            CreateMap<StatisticalTypeModel, StatisticalTypes>();

            //品牌
            CreateMap<Brand, BrandModel>();
            CreateMap<BrandModel, Brand>();

            //商品配置
            CreateMap<ProductSetting, ProductSettingModel>();
            CreateMap<ProductSettingModel, ProductSetting>();


            //规格属性
            CreateMap<SpecificationAttribute, SpecificationAttributeModel>();
            //.ForMember(dest => dest.StoreList, mo => mo.Ignore());
            CreateMap<SpecificationAttributeModel, SpecificationAttribute>()
            .ForMember(dest => dest.SpecificationAttributeOptions, mo => mo.Ignore());
            CreateMap<SpecificationAttributeOption, SpecificationAttributeOptionModel>()
                .ForMember(dest => dest.NumberOfAssociatedProducts, mo => mo.Ignore());
            CreateMap<SpecificationAttributeOptionModel, SpecificationAttributeOption>()
                .ForMember(dest => dest.SpecificationAttribute, mo => mo.Ignore())
                .ForMember(dest => dest.ProductSpecificationAttributes, mo => mo.Ignore());
            CreateMap<SpecificationAttributeOption, SpecificationAttributeModel>();

            //商品组合
            CreateMap<Combination, CombinationModel>();
            CreateMap<CombinationModel, Combination>();
            CreateMap<ProductCombination, ProductCombinationModel>();
            CreateMap<ProductCombinationModel, ProductCombination>();
            #endregion

            #region 往来相关
            //终端
            CreateMap<Terminal, TerminalModel>();
            CreateMap<TerminalModel, Terminal>();

            //片区
            CreateMap<District, DistrictModel>();
            CreateMap<DistrictModel, District>();

            //渠道
            CreateMap<Channel, ChannelModel>();
            CreateMap<ChannelModel, Channel>();

            //终端等级
            CreateMap<Rank, RankModel>();
            CreateMap<RankModel, Rank>();

            //供应商档案
            CreateMap<Manufacturer, ManufacturerModel>()
            ;
            CreateMap<ManufacturerModel, Manufacturer>();

            //应收款
            CreateMap<Receivable, ReceivableModel>();
            CreateMap<ReceivableModel, Receivable>();


            //拜访
            CreateMap<VisitStore, VisitStoreModel>();
            CreateMap<VisitStoreModel, VisitStore>();

            #endregion

            #region 员工相关
            //提成
            CreateMap<PercentagePlan, PercentagePlanModel>();
            CreateMap<PercentagePlanModel, PercentagePlan>();
            CreateMap<UserAssessment, UserAssessmentModel>();
            CreateMap<UserAssessmentsItems, UserAssessmentItemModel>();
            CreateMap<UserAssessmentModel, UserAssessment>();
            CreateMap<UserAssessmentItemModel, UserAssessmentsItems>();
            //提成
            CreateMap<Percentage, PercentageModel>()

            .ForMember(dest => dest.CalCulateMethod, mo => mo.Ignore())
            .ForMember(dest => dest.CostingCalCulateMethod, mo => mo.Ignore())
            .ForMember(dest => dest.QuantityCalCulateMethod, mo => mo.Ignore());
            //提成
            CreateMap<PercentageModel, Percentage>()
            .ForMember(dest => dest.CalCulateMethod, mo => mo.Ignore())
            .ForMember(dest => dest.CostingCalCulateMethod, mo => mo.Ignore())
            .ForMember(dest => dest.QuantityCalCulateMethod, mo => mo.Ignore());
            //提成
            CreateMap<PercentageRangeOption, PercentageRangeOptionModel>();
            CreateMap<PercentageRangeOptionModel, PercentageRangeOption>();

            //赠品
            CreateMap<GiveQuota, GiveQuotaModel>();
            CreateMap<GiveQuotaModel, GiveQuota>();
            CreateMap<GiveQuotaOption, GiveQuotaOptionModel>();
            CreateMap<GiveQuotaOptionModel, GiveQuotaOption>();
            CreateMap<GiveQuotaUpdate, GiveQuotaUpdateModel>();
            CreateMap<GiveQuotaUpdateModel, GiveQuotaUpdate>();

            //赠品汇总
            CreateMap<GiveQuotaRecords, GiveQuotaRecordsModel>();
            CreateMap<GiveQuotaRecordsModel, GiveQuotaRecords>();


            //活动
            ///Missing type map configuration or unsupported mapping. Mapping types: CampaignUpdateModel -> CampaignUpdate DCMS.ViewModel.Models.Campaigns.CampaignUpdateModel -> DCMS.Core.Domain.Campaigns.Campaig
            CreateMap<Campaign, CampaignModel>();
            CreateMap<CampaignModel, Campaign>();
            CreateMap<CampaignUpdateModel, CampaignUpdate>();
            CreateMap<CampaignUpdate, CampaignUpdateModel>();


            //赠品
            CreateMap<CampaignGiveProduct, CampaignGiveProductModel>();
            CreateMap<CampaignGiveProductModel, CampaignGiveProduct>();
            CreateMap<CampaignBuyProduct, CampaignBuyProductModel>();
            CreateMap<CampaignBuyProductModel, CampaignBuyProduct>();


            //近价
            CreateMap<RecentPrice, RecentPriceModel>();
            CreateMap<RecentPriceModel, RecentPrice>();

            //拜访线路
            CreateMap<LineTier, LineTierModel>();
            CreateMap<LineTierModel, LineTier>();
            //线路访问
            CreateMap<LineTierOption, LineTierOptionModel>();
            CreateMap<LineTierOptionModel, LineTierOption>();
            //业务员线路分配
            CreateMap<UserLineTierAssign, UserLineTierAssignModel>();
            CreateMap<UserLineTierAssignModel, UserLineTierAssign>();
            #endregion

            #region 系统设置

            //APP打印设置
            CreateMap<APPPrintSetting, APPPrintSettingModel>();
            CreateMap<APPPrintSettingModel, APPPrintSetting>();

            CreateMap<StockEarlyWarning, StockEarlyWarningModel>();
            CreateMap<StockEarlyWarningModel, StockEarlyWarning>();

            CreateMap<PCPrintSetting, PCPrintSettingModel>();
            CreateMap<PCPrintSettingModel, PCPrintSetting>();

            CreateMap<CompanySetting, CompanySettingModel>();
            CreateMap<CompanySettingModel, CompanySetting>();

            CreateMap<PricingStructure, PricingStructureModel>();
            CreateMap<PricingStructureModel, PricingStructure>();

            CreateMap<PrintTemplate, PrintTemplateModel>();
            CreateMap<PrintTemplateModel, PrintTemplate>();

            CreateMap<RemarkConfig, RemarkConfigModel>();
            CreateMap<RemarkConfigModel, RemarkConfig>();

            #endregion

            CreateMap<ClosingAccounts, ClosingAccountsModel>()
              .ForMember(dest => dest.HasPrecursor, mo => mo.Ignore())
              .ForMember(dest => dest.HasSuccessor, mo => mo.Ignore());

            //商品查询视图
            CreateMap<Product, ProductView>();
            CreateMap<ProductView, Product>()
            .ForMember(dest => dest.ProductCategories, mo => mo.Ignore())
            .ForMember(dest => dest.ProductSpecificationAttributes, mo => mo.Ignore())
            .ForMember(dest => dest.ProductVariantAttributes, mo => mo.Ignore())
            .ForMember(dest => dest.ProductVariantAttributeCombinations, mo => mo.Ignore())
            .ForMember(dest => dest.ProductPrices, mo => mo.Ignore())
            .ForMember(dest => dest.ProductTierPrices, mo => mo.Ignore())
            .ForMember(dest => dest.ProductPictures, mo => mo.Ignore())
            .ForMember(dest => dest.Stocks, mo => mo.Ignore())
            .ForMember(dest => dest.ProductFlavors, mo => mo.Ignore());

            //收款单据查询视图
            CreateMap<CashReceiptItem, CashReceiptItemView>();
            CreateMap<CashReceiptItemView, CashReceiptItem>();

            CreateMap<BillCashReceiptSummary, BillSummaryModel>();
            CreateMap<BillSummaryModel, BillCashReceiptSummary>();

            //对账
            CreateMap<FinanceReceiveAccountBill, FinanceReceiveAccountBillModel>();
            CreateMap<FinanceReceiveAccountBillModel, FinanceReceiveAccountBill>();

            CreateMap<DeliverySignUpdate, DeliverySignUpdateModel>();
            CreateMap<DeliverySignUpdateModel, DeliverySignUpdate>();
            CreateMap<DeliverySign, DeliverySignModel>();
            CreateMap<DeliverySignModel, DeliverySign>();

            CreateMap<Tracking, TrackingModel>();
            CreateMap<TrackingModel, Tracking>();


            #region CRM

            CreateMap<CRM_RELATION, CRM_RELATIONModel>();
            CreateMap<CRM_ORG, CRM_RETURNModel>();
            CreateMap<CRM_ORG, CRM_ORGModel>();
            CreateMap<CRM_BP, CRM_BPModel>();
            CreateMap<CRM_ZSNTM0040, CRM_ZSNTM0040Model>();
            CreateMap<CRM_HEIGHT_CONF, CRM_HEIGHT_CONFModel>();
            CreateMap<CRM_BUSTAT, CRM_BUSTATModel>();

            CreateMap<CRM_RELATIONModel, CRM_RELATION>();
            CreateMap<CRM_RETURNModel, CRM_RETURN>();
            CreateMap<CRM_ORGModel, CRM_ORG>();
            CreateMap<CRM_BPModel, CRM_BP>();
            CreateMap<CRM_ZSNTM0040Model, CRM_ZSNTM0040>();
            CreateMap<CRM_HEIGHT_CONFModel, CRM_HEIGHT_CONF>();
            CreateMap<CRM_BUSTATModel, CRM_BUSTAT>();

            #endregion


            //添加通用映射规则
            //ForAllMaps((mapConfiguration, map) =>
            //{

            //    if (typeof(ISettingsModel).IsAssignableFrom(mapConfiguration.DestinationType))
            //    {
            //        map.ForMember(nameof(ISettingsModel.ActiveStoreScopeConfiguration), options => options.Ignore());
            //    }

            //    //if (typeof(ILocalizedModel).IsAssignableFrom(mapConfiguration.DestinationType))
            //    //{
            //    //    map.ForMember(nameof(ILocalizedModel<ILocalizedModel>.Locales), options => options.Ignore());
            //    //}

            //    //if (typeof(IStoreMappingSupported).IsAssignableFrom(mapConfiguration.DestinationType))
            //    //{
            //    //    map.ForMember(nameof(IStoreMappingSupported.LimitedToStores), options => options.Ignore());
            //    //}

            //    //if (typeof(IStoreMappingSupportedModel).IsAssignableFrom(mapConfiguration.DestinationType))
            //    //{
            //    //    map.ForMember(nameof(IStoreMappingSupportedModel.AvailableStores), options => options.Ignore());
            //    //    map.ForMember(nameof(IStoreMappingSupportedModel.SelectedStoreIds), options => options.Ignore());
            //    //}

            //    if (typeof(IAclSupported).IsAssignableFrom(mapConfiguration.DestinationType))
            //    {
            //        map.ForMember(nameof(IAclSupported.SubjectToAcl), options => options.Ignore());
            //    }

            //    if (typeof(IAclSupportedModel).IsAssignableFrom(mapConfiguration.DestinationType))
            //    {
            //        map.ForMember(nameof(IAclSupportedModel.AvailableUserRoles), options => options.Ignore());
            //        map.ForMember(nameof(IAclSupportedModel.SelectedUserRoleIds), options => options.Ignore());
            //    }
            //});


        }

        public int Order => 0;

    }
}