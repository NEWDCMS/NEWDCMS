using DCMS.Core;
using DCMS.Core.Caching;
using DCMS.Core.Domain.Finances;
using DCMS.Core.Events;
using DCMS.Services.Events;

namespace DCMS.Web.Infrastructure.Cache
{
    public partial class FinancesCacheEventConsumer :
        //AdvancePaymentBill
        IConsumer<EntityInsertedEvent<AdvancePaymentBill>>,
         IConsumer<EntityUpdatedEvent<AdvancePaymentBill>>,
         IConsumer<EntityDeletedEvent<AdvancePaymentBill>>,

        //AdvancePaymentBillAccounting
        IConsumer<EntityInsertedEvent<AdvancePaymentBillAccounting>>,
         IConsumer<EntityUpdatedEvent<AdvancePaymentBillAccounting>>,
         IConsumer<EntityDeletedEvent<AdvancePaymentBillAccounting>>,

         //AdvanceReceiptBill
         IConsumer<EntityInsertedEvent<AdvanceReceiptBill>>,
         IConsumer<EntityUpdatedEvent<AdvanceReceiptBill>>,
         IConsumer<EntityDeletedEvent<AdvanceReceiptBill>>,

         //AdvanceReceiptBillAccounting
         IConsumer<EntityInsertedEvent<AdvanceReceiptBillAccounting>>,
         IConsumer<EntityUpdatedEvent<AdvanceReceiptBillAccounting>>,
         IConsumer<EntityDeletedEvent<AdvanceReceiptBillAccounting>>,

        //BalanceSheet
        IConsumer<EntityInsertedEvent<BalanceSheet>>,
         IConsumer<EntityUpdatedEvent<BalanceSheet>>,
         IConsumer<EntityDeletedEvent<BalanceSheet>>,

        //BalanceSheetExport
        IConsumer<EntityInsertedEvent<BalanceSheetExport>>,
         IConsumer<EntityUpdatedEvent<BalanceSheetExport>>,
         IConsumer<EntityDeletedEvent<BalanceSheetExport>>,

        //CashReceiptBill
        IConsumer<EntityInsertedEvent<CashReceiptBill>>,
         IConsumer<EntityUpdatedEvent<CashReceiptBill>>,
         IConsumer<EntityDeletedEvent<CashReceiptBill>>,

        //CashReceiptItem
        IConsumer<EntityInsertedEvent<CashReceiptItem>>,
         IConsumer<EntityUpdatedEvent<CashReceiptItem>>,
         IConsumer<EntityDeletedEvent<CashReceiptItem>>,

        //CashReceiptBillAccounting
        IConsumer<EntityInsertedEvent<CashReceiptBillAccounting>>,
         IConsumer<EntityUpdatedEvent<CashReceiptBillAccounting>>,
         IConsumer<EntityDeletedEvent<CashReceiptBillAccounting>>,

        //ClosingAccounts
        IConsumer<EntityInsertedEvent<ClosingAccounts>>,
         IConsumer<EntityUpdatedEvent<ClosingAccounts>>,
         IConsumer<EntityDeletedEvent<ClosingAccounts>>,

        //CostPriceSummery
        IConsumer<EntityInsertedEvent<CostPriceSummery>>,
         IConsumer<EntityUpdatedEvent<CostPriceSummery>>,
         IConsumer<EntityDeletedEvent<CostPriceSummery>>,

        //CostPriceChangeRecords
        IConsumer<EntityInsertedEvent<CostPriceChangeRecords>>,
         IConsumer<EntityUpdatedEvent<CostPriceChangeRecords>>,
         IConsumer<EntityDeletedEvent<CostPriceChangeRecords>>,

        //CostContractBill
        IConsumer<EntityInsertedEvent<CostContractBill>>,
         IConsumer<EntityUpdatedEvent<CostContractBill>>,
         IConsumer<EntityDeletedEvent<CostContractBill>>,

        //CostContractItem
        IConsumer<EntityInsertedEvent<CostContractItem>>,
         IConsumer<EntityUpdatedEvent<CostContractItem>>,
         IConsumer<EntityDeletedEvent<CostContractItem>>,

        //CostExpenditureBill
        IConsumer<EntityInsertedEvent<CostExpenditureBill>>,
         IConsumer<EntityUpdatedEvent<CostExpenditureBill>>,
         IConsumer<EntityDeletedEvent<CostExpenditureBill>>,

        //CostExpenditureItem
        IConsumer<EntityInsertedEvent<CostExpenditureItem>>,
         IConsumer<EntityUpdatedEvent<CostExpenditureItem>>,
         IConsumer<EntityDeletedEvent<CostExpenditureItem>>,

        //CostExpenditureBillAccounting
        IConsumer<EntityInsertedEvent<CostExpenditureBillAccounting>>,
         IConsumer<EntityUpdatedEvent<CostExpenditureBillAccounting>>,
         IConsumer<EntityDeletedEvent<CostExpenditureBillAccounting>>,

        //FinancialIncomeBill
        IConsumer<EntityInsertedEvent<FinancialIncomeBill>>,
         IConsumer<EntityUpdatedEvent<FinancialIncomeBill>>,
         IConsumer<EntityDeletedEvent<FinancialIncomeBill>>,

        //FinancialIncomeItem
        IConsumer<EntityInsertedEvent<FinancialIncomeItem>>,
         IConsumer<EntityUpdatedEvent<FinancialIncomeItem>>,
         IConsumer<EntityDeletedEvent<FinancialIncomeItem>>,

        //FinancialIncomeBillAccounting
        IConsumer<EntityInsertedEvent<FinancialIncomeBillAccounting>>,
         IConsumer<EntityUpdatedEvent<FinancialIncomeBillAccounting>>,
         IConsumer<EntityDeletedEvent<FinancialIncomeBillAccounting>>,

        //PaymentReceiptBill
        IConsumer<EntityInsertedEvent<PaymentReceiptBill>>,
         IConsumer<EntityUpdatedEvent<PaymentReceiptBill>>,
         IConsumer<EntityDeletedEvent<PaymentReceiptBill>>,

        //PaymentReceiptItem
        IConsumer<EntityInsertedEvent<PaymentReceiptItem>>,
         IConsumer<EntityUpdatedEvent<PaymentReceiptItem>>,
         IConsumer<EntityDeletedEvent<PaymentReceiptItem>>,

        //PaymentReceiptBillAccounting
        IConsumer<EntityInsertedEvent<PaymentReceiptBillAccounting>>,
         IConsumer<EntityUpdatedEvent<PaymentReceiptBillAccounting>>,
         IConsumer<EntityDeletedEvent<PaymentReceiptBillAccounting>>,

        //ProfitSheet
        IConsumer<EntityInsertedEvent<ProfitSheet>>,
         IConsumer<EntityUpdatedEvent<ProfitSheet>>,
         IConsumer<EntityDeletedEvent<ProfitSheet>>,

        //ProfitSheetExport
        IConsumer<EntityInsertedEvent<ProfitSheetExport>>,
         IConsumer<EntityUpdatedEvent<ProfitSheetExport>>,
         IConsumer<EntityDeletedEvent<ProfitSheetExport>>,

        //RecordingVoucher
        IConsumer<EntityInsertedEvent<RecordingVoucher>>,
         IConsumer<EntityUpdatedEvent<RecordingVoucher>>,
         IConsumer<EntityDeletedEvent<RecordingVoucher>>,

        //VoucherItem
        IConsumer<EntityInsertedEvent<VoucherItem>>,
         IConsumer<EntityUpdatedEvent<VoucherItem>>,
         IConsumer<EntityDeletedEvent<VoucherItem>>,

        //ScanCashReceiptRecords
        IConsumer<EntityInsertedEvent<ScanCashReceiptRecords>>,
         IConsumer<EntityUpdatedEvent<ScanCashReceiptRecords>>,
         IConsumer<EntityDeletedEvent<ScanCashReceiptRecords>>,

        //TrialBalance
        IConsumer<EntityInsertedEvent<TrialBalance>>,
         IConsumer<EntityUpdatedEvent<TrialBalance>>,
         IConsumer<EntityDeletedEvent<TrialBalance>>,

        //TrialBalanceExport
        IConsumer<EntityInsertedEvent<TrialBalanceExport>>,
         IConsumer<EntityUpdatedEvent<TrialBalanceExport>>,
         IConsumer<EntityDeletedEvent<TrialBalanceExport>>,

        //LedgerDetails
        IConsumer<EntityInsertedEvent<LedgerDetails>>,
         IConsumer<EntityUpdatedEvent<LedgerDetails>>,
         IConsumer<EntityDeletedEvent<LedgerDetails>>
    {

        protected readonly IStaticCacheManager _cacheManager;
        public FinancesCacheEventConsumer(IStaticCacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        #region AdvancePaymentBill
        public void HandleEvent(EntityInsertedEvent<AdvancePaymentBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.ADVANCEPAYMENTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<AdvancePaymentBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.ADVANCEPAYMENTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<AdvancePaymentBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.ADVANCEPAYMENTBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region AdvancePaymentBillAccounting
        public void HandleEvent(EntityInsertedEvent<AdvancePaymentBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.ADVANCEPAYMENTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<AdvancePaymentBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.ADVANCEPAYMENTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<AdvancePaymentBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.ADVANCEPAYMENTBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region AdvanceReceiptBill
        public void HandleEvent(EntityInsertedEvent<AdvanceReceiptBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.ADVANCEPRCEIPTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<AdvanceReceiptBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.ADVANCEPRCEIPTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<AdvanceReceiptBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.ADVANCEPRCEIPTBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region AdvanceReceiptBillAccounting
        public void HandleEvent(EntityInsertedEvent<AdvanceReceiptBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.ADVANCEPAYMENTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<AdvanceReceiptBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.ADVANCEPAYMENTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<AdvanceReceiptBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.ADVANCEPAYMENTBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region BalanceSheet
        public void HandleEvent(EntityInsertedEvent<BalanceSheet> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<BalanceSheet> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<BalanceSheet> eventMessage)
        {
        }
        #endregion

        #region BalanceSheetExport
        public void HandleEvent(EntityInsertedEvent<BalanceSheetExport> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<BalanceSheetExport> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<BalanceSheetExport> eventMessage)
        {
        }
        #endregion

        #region CashReceiptBill
        public void HandleEvent(EntityInsertedEvent<CashReceiptBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.CASHRECEIPTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<CashReceiptBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.CASHRECEIPTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<CashReceiptBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.CASHRECEIPTBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region CashReceiptItem
        public void HandleEvent(EntityInsertedEvent<CashReceiptItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.CASHRECEIPTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<CashReceiptItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.CASHRECEIPTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<CashReceiptItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.CASHRECEIPTBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region CashReceiptBillAccounting
        public void HandleEvent(EntityInsertedEvent<CashReceiptBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.CASHRECEIPTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<CashReceiptBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.CASHRECEIPTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<CashReceiptBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.CASHRECEIPTBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region ClosingAccounts
        public void HandleEvent(EntityInsertedEvent<ClosingAccounts> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<ClosingAccounts> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<ClosingAccounts> eventMessage)
        {
        }
        #endregion

        #region CostPriceSummery
        public void HandleEvent(EntityInsertedEvent<CostPriceSummery> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<CostPriceSummery> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<CostPriceSummery> eventMessage)
        {
        }
        #endregion

        #region CostPriceChangeRecords
        public void HandleEvent(EntityInsertedEvent<CostPriceChangeRecords> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<CostPriceChangeRecords> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<CostPriceChangeRecords> eventMessage)
        {
        }
        #endregion

        #region CostContractBill
        public void HandleEvent(EntityInsertedEvent<CostContractBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.COSTCONTRACTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<CostContractBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.COSTCONTRACTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<CostContractBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.COSTCONTRACTBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region CostContractItem
        public void HandleEvent(EntityInsertedEvent<CostContractItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.COSTCONTRACTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<CostContractItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.COSTCONTRACTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<CostContractItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.COSTCONTRACTBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region CostExpenditureBill
        public void HandleEvent(EntityInsertedEvent<CostExpenditureBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.COSTEXPENDITUREBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<CostExpenditureBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.COSTEXPENDITUREBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<CostExpenditureBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.COSTEXPENDITUREBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region CostExpenditureItem
        public void HandleEvent(EntityInsertedEvent<CostExpenditureItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.COSTEXPENDITUREBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<CostExpenditureItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.COSTEXPENDITUREBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<CostExpenditureItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.COSTEXPENDITUREBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region CostExpenditureBillAccounting
        public void HandleEvent(EntityInsertedEvent<CostExpenditureBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.COSTEXPENDITUREBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<CostExpenditureBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.COSTEXPENDITUREBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<CostExpenditureBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.COSTEXPENDITUREBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region FinancialIncomeBill
        public void HandleEvent(EntityInsertedEvent<FinancialIncomeBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.FINANCIALINCOMEBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<FinancialIncomeBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.FINANCIALINCOMEBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<FinancialIncomeBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.FINANCIALINCOMEBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region FinancialIncomeItem
        public void HandleEvent(EntityInsertedEvent<FinancialIncomeItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.FINANCIALINCOMEBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<FinancialIncomeItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.FINANCIALINCOMEBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<FinancialIncomeItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.FINANCIALINCOMEBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region FinancialIncomeBillAccounting
        public void HandleEvent(EntityInsertedEvent<FinancialIncomeBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.FINANCERECEIVEACCOUNTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<FinancialIncomeBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.FINANCERECEIVEACCOUNTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<FinancialIncomeBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.FINANCERECEIVEACCOUNTBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region PaymentReceiptBill
        public void HandleEvent(EntityInsertedEvent<PaymentReceiptBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PAYMENTRECEIPTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<PaymentReceiptBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PAYMENTRECEIPTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<PaymentReceiptBill> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PAYMENTRECEIPTBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region PaymentReceiptItem
        public void HandleEvent(EntityInsertedEvent<PaymentReceiptItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PAYMENTRECEIPTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<PaymentReceiptItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PAYMENTRECEIPTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<PaymentReceiptItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PAYMENTRECEIPTBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region PaymentReceiptBillAccounting
        public void HandleEvent(EntityInsertedEvent<PaymentReceiptBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PAYMENTRECEIPTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<PaymentReceiptBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PAYMENTRECEIPTBILL_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<PaymentReceiptBillAccounting> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.PAYMENTRECEIPTBILL_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region ProfitSheet
        public void HandleEvent(EntityInsertedEvent<ProfitSheet> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<ProfitSheet> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<ProfitSheet> eventMessage)
        {
        }
        #endregion

        #region ProfitSheetExport
        public void HandleEvent(EntityInsertedEvent<ProfitSheetExport> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<ProfitSheetExport> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<ProfitSheetExport> eventMessage)
        {
        }
        #endregion

        #region RecordingVoucher
        public void HandleEvent(EntityInsertedEvent<RecordingVoucher> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.RECORDINGVOUCHER_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<RecordingVoucher> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.RECORDINGVOUCHER_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<RecordingVoucher> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.RECORDINGVOUCHER_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region VoucherItem
        public void HandleEvent(EntityInsertedEvent<VoucherItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.RECORDINGVOUCHER_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityUpdatedEvent<VoucherItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.RECORDINGVOUCHER_PK, eventMessage.Entity.StoreId));
        }
        public void HandleEvent(EntityDeletedEvent<VoucherItem> eventMessage)
        {
            _cacheManager.RemoveByPrefix(string.Format(DCMSDefaults.RECORDINGVOUCHER_PK, eventMessage.Entity.StoreId));
        }
        #endregion

        #region ScanCashReceiptRecords
        public void HandleEvent(EntityInsertedEvent<ScanCashReceiptRecords> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<ScanCashReceiptRecords> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<ScanCashReceiptRecords> eventMessage)
        {
        }
        #endregion

        #region TrialBalance
        public void HandleEvent(EntityInsertedEvent<TrialBalance> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<TrialBalance> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<TrialBalance> eventMessage)
        {
        }
        #endregion

        #region TrialBalanceExport
        public void HandleEvent(EntityInsertedEvent<TrialBalanceExport> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<TrialBalanceExport> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<TrialBalanceExport> eventMessage)
        {
        }
        #endregion

        #region LedgerDetails
        public void HandleEvent(EntityInsertedEvent<LedgerDetails> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdatedEvent<LedgerDetails> eventMessage)
        {
        }
        public void HandleEvent(EntityDeletedEvent<LedgerDetails> eventMessage)
        {
        }
        #endregion

    }
}
