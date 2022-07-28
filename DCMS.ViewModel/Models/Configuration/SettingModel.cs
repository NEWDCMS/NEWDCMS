using DCMS.Core.Domain.Configuration;
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using System.Collections.Generic;


namespace DCMS.ViewModel.Models.Configuration
{



    public partial class SettingModel : BaseEntityModel
    {
        [HintDisplayName("配置名", "配置名")]

        public string Name { get; set; }

        [HintDisplayName("值", "配置值")]

        public string Value { get; set; }

        [HintDisplayName("经销商", "经销商")]
        public string Store { get; set; }

    }




    /// <summary>
    /// 财务设置
    /// </summary>
    public partial class FinanceSettingModel : BaseEntityModel
    {
        public FinanceSettingModel()
        {
            Options = new List<AccountingOptionModel>();
            //销售
            SaleFinanceAccountingMap = new FinanceAccountingMap();
            SaleReservationBillFinanceAccountingMap = new FinanceAccountingMap();
            //退货
            ReturnFinanceAccountingMap = new FinanceAccountingMap();
            ReturnReservationFinanceAccountingMap = new FinanceAccountingMap();

            //采购
            PurchaseFinanceAccountingMap = new FinanceAccountingMap();
            PurchaseReturnFinanceAccountingMap = new FinanceAccountingMap();

            //盘点盈亏
            InventoryProfitLossFinanceAccountingMap = new FinanceAccountingMap();
            //报损
            ScrapProductFinanceAccountingMap = new FinanceAccountingMap();

            //收款
            ReceiptFinanceAccountingMap = new FinanceAccountingMap();
            //预收款
            AdvanceReceiptFinanceAccountingMap = new FinanceAccountingMap();
            //付款
            PaymentFinanceAccountingMap = new FinanceAccountingMap();
            //预付款
            AdvancePaymentFinanceAccountingMap = new FinanceAccountingMap();

            //费用
            CostExpenditureFinanceAccountingMap = new FinanceAccountingMap();
            //收入
            FinancialIncomeFinanceAccountingMap = new FinanceAccountingMap();
            //费用合同
            CostContractAccountingMap = new FinanceAccountingMap();
        }

        public List<AccountingOptionModel> Options { get; set; }

        [HintDisplayName("销售单(收款账户)", "")]
        public FinanceAccountingMap SaleFinanceAccountingMap { get; set; }
        [HintDisplayName("销售订单(收款账户)", "")]
        public FinanceAccountingMap SaleReservationBillFinanceAccountingMap { get; set; }

        [HintDisplayName("退货单(收款账户)", "")]
        public FinanceAccountingMap ReturnFinanceAccountingMap { get; set; }
        [HintDisplayName("退货订单(收款账户)", "")]
        public FinanceAccountingMap ReturnReservationFinanceAccountingMap { get; set; }

        [HintDisplayName("采购单(付款账户)", "")]
        public FinanceAccountingMap PurchaseFinanceAccountingMap { get; set; }

        [HintDisplayName("采购退货单(付款账户)", "")]
        public FinanceAccountingMap PurchaseReturnFinanceAccountingMap { get; set; }


        [HintDisplayName("盘点盈亏单(盘点盈亏账户)", "")]
        public FinanceAccountingMap InventoryProfitLossFinanceAccountingMap { get; set; }
        [HintDisplayName("报损单(报损账户)", "")]
        public FinanceAccountingMap ScrapProductFinanceAccountingMap { get; set; }


        [HintDisplayName("收款单(收款账户)", "")]
        public FinanceAccountingMap ReceiptFinanceAccountingMap { get; set; }

        [HintDisplayName("预收款单(收款账户)", "")]
        public FinanceAccountingMap AdvanceReceiptFinanceAccountingMap { get; set; }

        [HintDisplayName("付款单(付款账户)", "")]
        public FinanceAccountingMap PaymentFinanceAccountingMap { get; set; }

        [HintDisplayName("预付款单(付款账户)", "")]
        public FinanceAccountingMap AdvancePaymentFinanceAccountingMap { get; set; }



        [HintDisplayName("费用支出（支出账户）", "")]
        public FinanceAccountingMap CostExpenditureFinanceAccountingMap { get; set; }

        [HintDisplayName("财务收入（收款账户）", "")]
        public FinanceAccountingMap FinancialIncomeFinanceAccountingMap { get; set; }

        [HintDisplayName("收款对账（会计科目）", "")]
        public FinanceAccountingMap FinanceReceiveAccountingAccountingMap { get; set; }

        [HintDisplayName("费用合同（会计科目）", "")]
        public FinanceAccountingMap CostContractAccountingMap { get; set; }


    }
}