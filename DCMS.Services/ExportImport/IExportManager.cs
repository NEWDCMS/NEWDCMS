using DCMS.Core.Domain.Census;
using DCMS.Core.Domain.Finances;
using DCMS.Core.Domain.Products;
using DCMS.Core.Domain.Purchases;
using DCMS.Core.Domain.Report;
using DCMS.Core.Domain.Sales;
using DCMS.Core.Domain.Terminals;
using DCMS.Core.Domain.Users;
using DCMS.Core.Domain.Visit;
using DCMS.Core.Domain.WareHouses;
using System.Collections.Generic;

namespace DCMS.Services.ExportImport
{
    public interface IExportManager
    {

        #region 销售

        #region 销售单据
        //销售订单
        byte[] ExportSaleReservationBillToXlsx(IList<SaleReservationBill> saleReservationBills, int store);
        //退货订单
        byte[] ExportReturnReservationBillToXlsx(IList<ReturnReservationBill> returnReservationBills, int store);
        //销售单
        byte[] ExportSaleBillToXlsx(IList<SaleBill> saleBills, int store);
        //退货单
        byte[] ExportReturnBillToXlsx(IList<ReturnBill> returnBills, int store);

        #endregion

        #region 销售报表
        //销售明细表
        byte[] ExportSaleReportItemToXlsx(IList<SaleReportItem> saleReportItems);
        //销售汇总按商品
        byte[] ExportSaleReportSummaryProductToXlsx(IList<SaleReportSummaryProduct> saleReportItems);
        //销售汇总按客户
        byte[] ExportSaleReportSummaryCustomerToXlsx(IList<SaleReportSummaryCustomer> saleReportItems, int store);
        //销售汇总按业务员
        byte[] ExportSaleReportSummaryBusinessUserToXlsx(IList<SaleReportSummaryBusinessUser> saleReportItems, int store);
        //销售汇总按客户/商品
        byte[] ExportSaleReportSummaryCustomerProductToXlsx(IList<SaleReportSummaryCustomerProduct> saleReportItems);
        //销售汇总按仓库
        byte[] ExportSaleReportSummaryWareHouseToXlsx(IList<SaleReportSummaryWareHouse> saleReportItems, int store);
        //销售汇总按品牌
        byte[] ExportSaleReportSummaryBrandToXlsx(IList<SaleReportSummaryBrand> saleReportItems, int store);
        //订单明细
        byte[] ExportSaleReportOrderItemToXlsx(IList<SaleReportOrderItem> saleReportOrderItems);
        //订单汇总按商品
        byte[] ExportSaleReportSummaryOrderProductToXlsx(IList<SaleReportSummaryOrderProduct> saleReportOrderItems);
        //费用合同明细表
        byte[] ExportSaleReportCostContractItemToXlsx(IList<SaleReportCostContractItem> saleReportCostContractItems);
        //赠品汇总
        byte[] ExportSaleReportSummaryGiveQuotaToXlsx(IList<GiveQuotaRecordsSummery> saleReportOrderItems);
        //热销排行榜
        byte[] ExportSaleReportHotSaleToXlsx(IList<SaleReportHotSale> saleReportOrderItems);
        //销售商品成本利润
        byte[] ExportSaleReportProductCostProfitToXlsx(IList<SaleReportProductCostProfit> saleReportProductCostProfits);
        #endregion

        #endregion

        #region 采购

        #region 采购单据
        //采购单
        byte[] ExportPurchaseBillToXlsx(IList<PurchaseBill> purchaseBills, int store);
        //采购退货单
        byte[] ExportPurchaseReturnBillToXlsx(IList<PurchaseReturnBill> purchaseBills, int store);
        #endregion

        #region 采购报表
        //采购明细表
        byte[] ExportPurchaseReportItemToXlsx(IList<PurchaseReportItem> purchaseReportItems);
        //采购汇总（按商品）
        byte[] ExportPurchaseReportSummaryProductToXlsx(IList<PurchaseReportSummaryProduct> purchaseReportItems);
        //采购汇总（按供应商）
        byte[] ExportPurchaseReportSummaryManufacturerToXlsx(IList<PurchaseReportSummaryManufacturer> purchaseReportItems, int store);
        #endregion

        #endregion

        #region 仓库

        #region 仓库单据
        //调拨单
        byte[] ExportAllocationBillToXlsx(IList<AllocationBill> allocationBills);
        //盘点盈亏单
        byte[] ExportInventoryProfitLossBillToXlsx(IList<InventoryProfitLossBill> inventoryProfitLossBills);
        //成本调价单
        byte[] ExportCostAdjustmentBillToXlsx(IList<CostAdjustmentBill> costAdjustmentBills);
        //报损单
        byte[] ExportScrapProductBillToXlsx(IList<ScrapProductBill> scrapProductBills);
        //盘点单整仓
        byte[] ExportInventoryAllTaskBillToXlsx(IList<InventoryAllTaskBill> inventoryAllTaskBills);
        //盘点单部分
        byte[] ExportInventoryPartTaskBillToXlsx(IList<InventoryPartTaskBill> inventoryPartTaskBills);
        //组合单
        byte[] ExportCombinationProductBillToXlsx(IList<CombinationProductBill> combinationProductBills);
        //拆分单
        byte[] ExportSplitProductBillToXlsx(IList<SplitProductBill> splitProductBills);
        #endregion

        #region 库存报表
        //库存表
        byte[] ExportStockListToXlsx(IList<StockReportProduct> stockReportProducts);
        byte[] ExportGenerateStockListToXlsx(IList<StockReportProduct> stockReportProducts);
        //库存变化汇总表
        byte[] ExportChangeSummaryToXlsx(IList<StockChangeSummary> stockChangeSummaries);
        byte[] ExportCostPriceSummeryToXlsx(IList<CostPriceSummery> CostPriceSummery);
        byte[] ExportCostPriceChangeRecordsToXlsx(IList<CostPriceChangeRecords> CostPriceChangeRecords);
        //库存变化汇总表按单据
        byte[] ExportChangeByOrderToXlsx(IList<StockChangeSummaryOrder> stockChangeSummaryOrders);
        //门店库存上报表
        byte[] ExportStockReportListToXlsx(IList<InventoryReportList> inventoryReportLists);
        //门店库存上报汇总表
        byte[] ExportStockReportAllToXlsx(IList<InventoryReportList> inventoryReportLists);
        //调拨明细表
        byte[] ExportAllocationDetailsToXlsx(IList<AllocationDetailsList> allocationDetails);
        //调拨汇总表按商品
        byte[] ExportAllocationDetailsByProductToXlsx(IList<AllocationDetailsList> allocationDetails);
        #endregion

        #region 库存提醒
        //库存滞销报表
        byte[] ExportStockUnsalableToXlsx(IList<StockUnsalable> stockUnsalables);
        //库存预警表
        byte[] ExportEarlyWarningToXlsx(IList<EarlyWarning> earlyWarnings);
        //临期预警表
        byte[] ExportExpirationWarningToXlsx(IList<ExpirationWarning> expirationWarnings);
        #endregion

        #endregion

        #region 财务

        #region 财务单据
        //收款单
        byte[] ExportReceiptCashBillToXlsx(IList<CashReceiptBill> cashReceiptBills, int store);
        //付款单
        byte[] ExportPaymentReceiptBillToXlsx(IList<PaymentReceiptBill> paymentReceiptBills, int store);
        //预收款单
        byte[] ExportAdvanceReceiptBillToXlsx(IList<AdvanceReceiptBill> advanceReceiptBills, int store);
        //预付款单
        byte[] ExportAdvancePaymentBillToXlsx(IList<AdvancePaymentBill> advancePaymentBills, int store);
        //费用支出
        byte[] ExportCostExpenditureBillToXlsx(IList<CostExpenditureBill> costExpenditureBills, int store);
        //费用合同
        byte[] ExportCostContractBillToXlsx(IList<CostContractBill> costContractBills);
        //财务收入
        byte[] ExportFinancialIncomeBillToXlsx(IList<FinancialIncomeBill> financialIncomeBills, int store);
        #endregion

        #region 财务凭证
        //录入凭证
        byte[] ExportRecordingVoucherToXlsx(IList<RecordingVoucher> RecordingVouchers);
        #endregion

        #region 财务报表
        //科目余额表
        byte[] ExportTrialBalanceToXlsx(IList<TrialBalanceExport> trialBalances);
        //资产负债表
        byte[] ExportBalanceSheetToXlsx(IList<BalanceSheetExport> balancesheets);
        //利润表
        byte[] ExportProfitSheetToXlsx(IList<ProfitSheetExport> profitsheet);
        #endregion

        #endregion

        #region 档案
        byte[] ExportProductsToXlsx(IEnumerable<DCMS.Core.Domain.Products.Product> products);
        byte[] ExportUsersToXlsx(IList<User> users);
        string ExportUsersToXml(IList<User> users);
        byte[] ExportCategoriesToXlsx(IEnumerable<Category> categories); //商品类别
        byte[] ExportBrandsToXlsx(IEnumerable<Brand> brands); //品牌
        byte[] ExportDistrictsToXlsx(IEnumerable<District> districts); //片区
        byte[] ExportChannelsToXlsx(IEnumerable<Channel> channels); //渠道
        byte[] ExportRanksToXlsx(IEnumerable<Rank> ranks); //终端等级
        byte[] ExportLineTiersToXlsx(IEnumerable<LineTier> lines); //线路
        byte[] ExportManufacturersToXlsx(IEnumerable<Manufacturer> manufacturers); //供应商
        byte[] ExportWareHousesToXlsx(IEnumerable<WareHouse> wareHouses); //仓库
        byte[] ExportTerminalsToXlsx(IEnumerable<Terminal> terminals); //终端档案
        byte[] ExportStocksToXlsx(IEnumerable<Stock> stocks); //库存
        byte[] ExportTrialBalancesToXlsx(IEnumerable<TrialBalance> trialBalances); //科目期末余额
        byte[] ExportReceivablesToXlsx(IEnumerable<Receivable> Receivables); //应收款期初余额

        #endregion

        #region 报表

        #region 资金报表
        //客户往来账
        byte[] ExportFundReportCustomerAccountToXlsx(IList<CustomerAccountDealings> customerAccountDealings);
        //客户应收款
        byte[] ExportFundReportCustomerReceiptCashToXlsx(IList<FundReportCustomerReceiptCash> fundReportCustomerReceiptCashes);
        //供应商往来账
        byte[] ExportFundReportManufacturerAccountToXlsx(IList<FundReportManufacturerAccount> fundReportManufacturerAccounts);
        //供应商应付款
        byte[] ExportFundReportManufacturerPayCashToXlsx(IList<FundReportManufacturerPayCash> fundReportManufacturerPayCashes);
        //预收款余额
        byte[] ExportFundReportAdvanceReceiptOverageToXlsx(IList<FundReportAdvanceReceiptOverage> fundReportAdvanceReceiptOverages, int store);
        //预付款余额
        byte[] ExportFundReportAdvancePaymentOverageToXlsx(IList<FundReportAdvancePaymentOverage> fundReportAdvancePaymentOverages);
        #endregion

        #region 员工报表
        //业务员业绩
        byte[] ExportStaffReportBusinessUserAchievementToXlsx(IList<StaffReportBusinessUserAchievement> staffReportBusinessUserAchievements);
        //员工提成汇总表
        byte[] ExportStaffReportPercentageSummaryToXlsx(IList<StaffReportPercentageSummary> staffReportPercentageSummaries);
        //业务员拜访记录
        byte[] ExportBusinessUserVisitingRecordToXlsx(IList<VisitStore> visitStores);
        #endregion

        #region 市场报表
        //客户活跃度
        byte[] ExportMarketReportTerminalActiveToXlsx(IList<MarketReportTerminalActive> marketReportTerminalActives);
        //客户价值分析
        byte[] ExportMarketReportTerminalValueAnalysisToXlsx(IList<MarketReportTerminalValueAnalysis> marketReportTerminalValueAnalysiss);

        //客户流失预警
        byte[] ExportMarketReportTerminalLossWarningToXlsx(IList<MarketReportTerminalValueAnalysis> marketReportTerminalLossWarnings);
        //铺市率报表
        byte[] ExportMarketReportShopRateToXlsx(IList<MarketReportShopRate> marketReportShopRates);
        #endregion

        #endregion

        #region 提成

        byte[] ExportStaffSaleQueryToXlsx(IList<StaffSaleQuery> staffSales, int store);
        byte[] ExportVisitSummeryQueryToXlsx(IList<VisitSummeryQuery> summeryQueries, int store);

        #endregion

        #region 业务员拜访统计
        byte[] ExportBusinessUserVisitOfYear(IEnumerable<BusinessUserVisitOfYear> bussinessVisit, int year, int month);
        #endregion
    }
}