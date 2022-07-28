using DCMS.Core.Configuration;
using System.Collections.Generic;

namespace DCMS.Core.Domain.Configuration
{
    public class FinanceSetting : ISettings
    {

        #region 销售
        /// <summary>
        /// 销售单(收款账户) 配置
        /// </summary>
        public string SaleBillAccountingOptionConfiguration { get; set; }

        /// <summary>
        /// 销售订单(收款账户) 配置
        /// </summary>
        public string SaleReservationBillAccountingOptionConfiguration { get; set; }

        /// <summary>
        /// 退货单(收款账户) 配置
        /// </summary>
        public string ReturnBillAccountingOptionConfiguration { get; set; }

        /// <summary>
        /// 退货订单(收款账户) 配置
        /// </summary>
        public string ReturnReservationBillAccountingOptionConfiguration { get; set; }
        #endregion

        #region 采购
        /// <summary>
        /// 采购单(付款账户) 配置
        /// </summary>
        public string PurchaseBillAccountingOptionConfiguration { get; set; }

        /// <summary>
        /// 采购退货单(付款账户) 配置
        /// </summary>
        public string PurchaseReturnBillAccountingOptionConfiguration { get; set; }
        #endregion

        #region 仓储

        /// <summary>
        /// 盘点盈亏单(盘点盈亏账户) 配置
        /// </summary>
        public string InventoryProfitLossAccountingOptionConfiguration { get; set; }

        /// <summary>
        /// 报损单(报损账户) 配置
        /// </summary>
        public string ScrapProductAccountingOptionConfiguration { get; set; }

        #endregion

        #region 财务
        /// <summary>
        /// 收款单(收款账户) 配置
        /// </summary>
        public string ReceiptAccountingOptionConfiguration { get; set; }

        /// <summary>
        /// 付款单(付款账户) 配置
        /// </summary>
        public string PaymentAccountingOptionConfiguration { get; set; }

        /// <summary>
        /// 预收款单(收款账户) 配置
        /// </summary>
        public string AdvanceReceiptAccountingOptionConfiguration { get; set; }

        /// <summary>
        /// 预付款单(付款账户) 配置
        /// </summary>
        public string AdvancePaymentAccountingOptionConfiguration { get; set; }

        /// <summary>
        /// 费用支出（支出账户） 配置
        /// </summary>
        public string CostExpenditureAccountingOptionConfiguration { get; set; }

        /// <summary>
        /// 财务收入（收款账户）  配置
        /// </summary>
        public string FinancialIncomeAccountingOptionConfiguration { get; set; }
        /// <summary>
        /// 费用合同（会计科目）  配置
        /// </summary>
        public string CostContractAccountingOptionConfiguration { get; set; }
        #endregion

    }


    public class FinanceAccountingMap
    {
        public FinanceAccountingMap()
        {
            Options = new List<AccountingOption>();
        }

        /// <summary>
        /// 启用的科目项
        /// </summary>
        public IList<AccountingOption> Options { get; set; }

        /// <summary>
        /// 默认科目
        /// </summary>
        public int DefaultOption { get; set; }

        /// <summary>
        /// 借方科目
        /// </summary>
        public int DebitOption { get; set; }

        /// <summary>
        /// 贷方科目
        /// </summary>
        public int CreditOption { get; set; }
    }
}
