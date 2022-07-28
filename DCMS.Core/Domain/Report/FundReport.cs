using DCMS.Core.Domain.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


namespace DCMS.Core.Domain.Report
{

    /// <summary>
    /// 客户往来账
    /// </summary>
    public class CustomerAccountDealings
    {
        public int StoreId { get; set; }

        /// <summary>
        /// 单据Id
        /// </summary>
        public int? BillId { get; set; }

        /// <summary>
        /// 单据编号
        /// </summary>
        public string BillNumber { get; set; }

        /// <summary>
        /// 客户Id
        /// </summary>
        public int? TerminalId { get; set; }

        /// <summary>
        /// 片区Id
        /// </summary>
        public int DistrictId { get; set; }
        /// <summary>
        /// 渠道Id
        /// </summary>
        public int ChannelId { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        public string TerminalName { get; set; }

        /// <summary>
        /// 客户编码
        /// </summary>
        public string TerminalCode { get; set; }

        /// <summary>
        /// 单据类型Id
        /// </summary>
        public int? BillTypeId { get; set; }

        /// <summary>
        /// 单价类型名称
        /// </summary>
        public string BillTypeName { get; set; }

        /// <summary>
        /// 发生日期
        /// </summary>
        public DateTime? TransactionDate { get; set; }

        /// <summary>
        /// 单据金额
        /// </summary>
        public decimal? BillAmount { get; set; } = 0;

        /// <summary>
        /// 优惠金额
        /// </summary>
        public decimal? PreferentialAmount { get; set; } = 0;

        /// <summary>
        /// 收款金额
        /// </summary>
        public decimal? CashReceiptAmount { get; set; } = 0;

        /// <summary>
        /// 应收款减（（（单据金额-优惠金额）-收款金额）<0）
        /// </summary>
        public decimal? ReceivableAmountSubtract { get; set; } = 0;

        /// <summary>
        /// 应收款加（（（单据金额-优惠金额）-收款金额）>0）
        /// </summary>
        public decimal? ReceivableAmountAdd { get; set; } = 0;


        /// <summary>
        /// 应收款余额 =（上期应收款余额）期末应收款余额 + （-应收款减）+ 应收款增 
        /// </summary>
        public decimal? ReceivableAmountOverage { get; set; } = 0;

        /// <summary>
        /// 本次预收款
        /// </summary>
        public decimal? CurAdvanceAmount { get; set; } = 0;

        /// <summary>
        /// 预收款减
        /// </summary>
        public decimal? AdvancePaymentAmountSubtract { get; set; } = 0;

        /// <summary>
        /// 预收款加
        /// </summary>
        public decimal? AdvancePaymentAmountAdd { get; set; } = 0;

        /// <summary>
        /// 预收款余额 =  （上期预收款余额余额）期末预收款余额 + （-预收款减）+ 预收款增 
        /// </summary>
        public decimal? AdvancePaymentAmountOverage { get; set; } = 0;

        /// <summary>
        /// 订货款余额
        /// </summary>
        public decimal? SubscribeCashAmountOverage { get; set; } = 0;

        /// <summary>
        /// 往来账余额=预收款余额-应收款余额
        /// </summary>
        public decimal? AccountAmountOverage { get; set; } = 0;

        /// <summary>
        /// 备注（单据的备注）
        /// </summary>
        public string Remark { get; set; }


    }


    #region 客户应收款

    /// <summary>
    /// 客户应收款
    /// </summary>
    public class FundReportCustomerReceiptCash
    {
        public int BillId { get; set; }
        public int StoreId { get; set; }
        public int BillTypeId { get; set; }
        public string BillNumber { get; set; }
        public string BillTypeName { get; set; }
        /// <summary>

        /// 账期天数
        /// </summary>
        public int sDay { get; set; }

        /// <summary>
        /// 客户Id
        /// </summary>
        public int? TerminalId { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        public string TerminalName { get; set; }

        /// <summary>
        /// 客户编码
        /// </summary>
        public string TerminalCode { get; set; }


        /// <summary>
        /// 累计欠款
        /// </summary>
        public decimal? OweCase { get; set; }

        /// <summary>
        /// 销售金额
        /// </summary>
        public decimal? SaleAmount { get; set; }

        /// <summary>
        /// 退货金额
        /// </summary>
        public decimal? ReturnAmount { get; set; }

        /// <summary>
        /// 净销售额=(销售金额-退货金额)
        /// </summary>
        public decimal? NetAmount { get; set; }

        /// <summary>
        /// 首次欠款时间
        /// </summary>
        public DateTime? FirstOweCaseDate { get; set; }

        /// <summary>
        /// 末次欠款时间
        /// </summary>
        public DateTime? LastOweCaseDate { get; set; }

        public DateTime? CreatedOnUtc { get; set; }


        /// <summary>
        /// 单据金额
        /// </summary>
        public decimal? Amount { get; set; } = 0;
        /// <summary>
        /// 优惠金额
        /// </summary>
        public decimal? DiscountAmount { get; set; } = 0;
        /// <summary>
        /// 已收/已付金额
        /// </summary>
        public decimal? PaymentedAmount { get; set; } = 0;




        [NotMapped]
        public Dictionary<int, int> Bills { get; set; } = new Dictionary<int, int>();

    }

    #endregion

    #region 供应商往来账

    /// <summary>
    /// 供应商来账
    /// </summary>
    public class FundReportManufacturerAccount
    {

        /// <summary>
        /// 单据Id
        /// </summary>
        public int? BillId { get; set; }

        /// <summary>
        /// 单据编号
        /// </summary>
        public string BillNumber { get; set; }

        /// <summary>
        /// 供应商Id
        /// </summary>
        public int ManufacturerId { get; set; }

        /// <summary>
        /// 供应商名称
        /// </summary>
        public string ManufacturerName { get; set; }

        /// <summary>
        /// 单据类型Id
        /// </summary>
        public int? BillTypeId { get; set; }

        /// <summary>
        /// 单价类型名称
        /// </summary>
        public string BillTypeName { get; set; }

        /// <summary>
        /// 发生日期
        /// </summary>
        public DateTime? TransactionDate { get; set; }

        /// <summary>
        /// 单据金额
        /// </summary>
        public decimal? BillAmount { get; set; }

        /// <summary>
        /// 优惠金额
        /// </summary>
        public decimal? PreferentialAmount { get; set; }

        /// <summary>
        /// 付款金额
        /// </summary>
        public decimal? PayCashAmount { get; set; }

        /// <summary>
        /// 应付款减（（（单据金额-优惠金额）-收款金额）<0）
        /// </summary>
        public decimal? PayAmountSubtract { get; set; }

        /// <summary>
        /// 应付款加（（（单据金额-优惠金额）-收款金额）>0）
        /// </summary>
        public decimal? PayAmountAdd { get; set; }

        /// <summary>
        /// 应付款余额
        /// </summary>
        public decimal? PayAmountOverage { get; set; }


        /// <summary>
        /// 本次预付款
        /// </summary>
        public decimal? CurAdvancePayAmount { get; set; } = 0;

        /// <summary>
        /// 预付款减
        /// </summary>
        public decimal? AdvancePayAmountSubtract { get; set; }

        /// <summary>
        /// 预付款加
        /// </summary>
        public decimal? AdvancePayAmountAdd { get; set; }

        /// <summary>
        /// 预付款余额
        /// </summary>
        public decimal? AdvancePayAmountOverage { get; set; }

        /// <summary>
        /// 往来账余额=预付款余额-应付款余额
        /// </summary>
        public decimal? AccountAmountOverage { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }


    }

    #endregion

    #region 供应商应付款

    /// <summary>
    /// 供应商应付款
    /// </summary>
    public class FundReportManufacturerPayCash
    {
        public int BillId { get; set; }

        public int BillType { get; set; }

        /// <summary>
        /// 账期天数
        /// </summary>
        public int sDay { get; set; }

        /// <summary>
        /// 供应商Id
        /// </summary>
        public int? ManufacturerId { get; set; }

        /// <summary>
        /// 供应商名称
        /// </summary>
        public string ManufacturerName { get; set; }

        /// <summary>
        /// 客户编码
        /// </summary>
        public string ManufacturerCode { get; set; }


        /// <summary>
        /// 累计欠款
        /// </summary>
        public decimal? OweCase { get; set; }

        /// <summary>
        /// 销售金额
        /// </summary>
        public decimal? SaleAmount { get; set; }

        /// <summary>
        /// 退货金额
        /// </summary>
        public decimal? ReturnAmount { get; set; }

        /// <summary>
        /// 净销售额=(销售金额-退货金额)
        /// </summary>
        public decimal? NetAmount { get; set; }

        /// <summary>
        /// 首次欠款时间
        /// </summary>
        public DateTime? FirstOweCaseDate { get; set; }

        /// <summary>
        /// 末次欠款时间
        /// </summary>
        public DateTime? LastOweCaseDate { get; set; }

        public DateTime? CreatedOnUtc { get; set; }

        [NotMapped]

        public Dictionary<int, int> Bills { get; set; } = new Dictionary<int, int>();

    }

    #endregion


    #region 预收款余额

    /// <summary>
    /// 预收款余额
    /// </summary>
    public class FundReportAdvanceReceiptOverage
    {

        /// <summary>
        /// 客户Id
        /// </summary>
        public int? TerminalId { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        public string TerminalName { get; set; }

        /// <summary>
        /// 客户编码
        /// </summary>
        public string TerminalCode { get; set; }

        /// <summary>
        /// 预收款账户
        /// </summary>
        public int AccountingOptionId { get; set; }
        /// <summary>
        /// 预收款金额
        /// </summary>
        public decimal? AdvanceReceiptAmount { get; set; } = 0;

        /// <summary>
        /// 预收款余额 = 所有 预收款账户 对应的 预收款金额 之和
        /// </summary>
        public decimal? AdvanceReceiptOverageAmount { get; set; } = 0;

        /// <summary>
        /// 应收款余额 = 所有未收款销售单销售金额 - 所有未收款退货单退货金额
        /// </summary>
        public decimal? ReceivableOverageAmount { get; set; } = 0;

        /// <summary>
        /// 余额 = 预收款余额- 应收款余额

        /// </summary>
        public decimal? OverageAmount { get; set; } = 0;


        /// <summary>
        /// 用于分割多个收款账户
        /// </summary>
        public string Accounts { get; set; }

        [NotMapped]
        public List<Accounting> AccountingOptions { get; set; } = new List<Accounting>();

    }

    #endregion

    #region 预付款余额

    /// <summary>
    /// 预付款余额
    /// </summary>
    public class FundReportAdvancePaymentOverage
    {

        /// <summary>
        /// 供应商Id
        /// </summary>
        public int ManufacturerId { get; set; }

        /// <summary>
        /// 供应商名称
        /// </summary>
        public string ManufacturerName { get; set; }

        /// <summary>
        /// 所有预付款单金额总和
        /// </summary>
        public decimal? AdvancePaymentAmount { get; set; }

        /// <summary>
        /// 所有采购（使用预付款支付）的账户金额总和
        /// </summary>
        public decimal? PurchaseAdvancePaymentAmount { get; set; }

        /// <summary>
        /// 所有采购退货（使用预付款支付）的账户金额总和
        /// </summary>
        public decimal? PurchaseReturnAdvancePaymentAmount { get; set; }

        /// <summary>
        /// 余额  =  所有预付款单金额总和 - 所有采购（使用预付款支付）的账户金额总和  +  所有采购退货（使用预付款支付）的账户金额总和
        /// </summary>
        public decimal? OverageAmount { get; set; }


        /// <summary>
        /// 用于分割多个收款账户
        /// </summary>
        public string Accounts { get; set; }
        [NotMapped]
        public List<Accounting> AccountingOptions { get; set; } = new List<Accounting>();

    }

    #endregion


}
