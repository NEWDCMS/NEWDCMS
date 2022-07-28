using DCMS.Core.Domain.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;

namespace DCMS.Core.Domain.Sales
{
    /// <summary>
    /// 对账视图
    /// </summary>
    public class FinanceReceiveAccountView
    {
        /// <summary>
        /// 经销商Id
        /// </summary>
        public int StoreId { get; set; }

        public int TerminalId { get; set; }
        public string TerminalName { get; set; }

        /// <summary>
        /// 单据编号
        /// </summary>
        public string BillNumber { get; set; }

        /// <summary>
        /// 单据类型
        /// </summary>
        public int BillType { get; set; }

        /// <summary>
        /// 业务员
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 上交状态
        /// </summary>
        public int HandInStatus { get; set; }


        /// <summary>
        /// 待交金额
        /// </summary>
        public decimal PaidAmount { get; set; }
        /// <summary>
        /// 电子支付金额
        /// </summary>
        public decimal EPaymentAmount { get; set; }

        #region 销售收款 = 销售金额-预收款-欠款

        public decimal SaleAmountSum { get; set; }
        public decimal SaleAmount { get; set; }
        public decimal SaleAdvanceReceiptAmount { get; set; }
        public decimal SaleOweCashAmount { get; set; }

        #endregion

        #region 退货款 =退款金额-预收款-欠款

        public decimal ReturnAmountSum { get; set; }
        public decimal ReturnAmount { get; set; }
        public decimal ReturnAdvanceReceiptAmount { get; set; }
        public decimal ReturnOweCashAmount { get; set; }

        #endregion


        #region 收欠款 =应收金额-预收款

        public decimal ReceiptCashOweCashAmountSum { get; set; }
        public decimal ReceiptCashReceivableAmount { get; set; }
        public decimal ReceiptCashAdvanceReceiptAmount { get; set; }

        #endregion


        #region 收预收款 =预收金额-欠款

        public decimal AdvanceReceiptSum { get; set; }
        public decimal AdvanceReceiptAmount { get; set; }
        public decimal AdvanceReceiptOweCashAmount { get; set; }

        #endregion

        #region 费用支出 = 支出金额-欠款

        public decimal CostExpenditureSum { get; set; }
        public decimal CostExpenditureAmount { get; set; }
        public decimal CostExpenditureOweCashAmount { get; set; }

        #endregion

        /// <summary>
        /// 单据ID
        /// </summary>
        public int BillId { get; set; }

        /// <summary>
        /// 支付方式
        /// </summary>
        public string AccountingOptions { get; set; }



        public List<FinanceReceiveAccountBillAccounting> Accounts
        {
            get
            {
                var accs = new List<FinanceReceiveAccountBillAccounting>();
                var lists = AccountingOptions?.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                if (lists != null && lists.Count > 0)
                {
                    foreach (var acc in lists)
                    {
                        var fabc = new FinanceReceiveAccountBillAccounting();
                        var accArry = acc.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                        if (accArry.Count > 0)
                        {
                            fabc.AccountingOptionId = Convert.ToInt32(accArry[0]);
                        }

                        if (accArry.Count > 1)
                        {
                            fabc.AccountingOption = new AccountingOption { Id = fabc.AccountingOptionId, Name = accArry[1] };
                        }

                        if (accArry.Count > 2)
                        {
                            fabc.CollectionAmount = Convert.ToDecimal(accArry[2]);
                        }

                        accs.Add(fabc);
                    }
                }

                return accs;
            }
        }


        /// <summary>
        /// 优惠金额
        /// </summary>
        public int PreferentialAmount { get; set; }


        /// <summary>
        /// 销售商品(含赠)
        /// </summary>
        public int SaleProductCount { get; set; }
        /// <summary>
        /// 赠送商品
        /// </summary>
        public int GiftProductCount { get; set; }
        /// <summary>
        /// 退货商品
        /// </summary>
        public int ReturnProductCount { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }
    }

    /// <summary>
    /// 收款对账单
    /// </summary>
    public class FinanceReceiveAccountBill : BaseBill
    {
        /// <summary>
        /// 业务员
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 上交状态
        /// </summary>
        public int HandInStatus { get; set; }
        public FinanceReceiveAccountStatus FinanceReceiveAccountStatus
        {
            get { return (FinanceReceiveAccountStatus)HandInStatus; }
            set { HandInStatus = (int)value; }
        }

        /// <summary>
        /// 待交金额
        /// </summary>
        public decimal PaidAmount { get; set; }
        /// <summary>
        /// 电子支付金额
        /// </summary>
        public decimal EPaymentAmount { get; set; }

        #region 销售收款 = 销售金额-预收款-欠款

        public decimal SaleAmountSum { get; set; }
        public decimal SaleAmount { get; set; }
        public decimal SaleAdvanceReceiptAmount { get; set; }
        public decimal SaleOweCashAmount { get; set; }

        #endregion

        #region 退货款 =退款金额-预收款-欠款

        public decimal ReturnAmountSum { get; set; }
        public decimal ReturnAmount { get; set; }
        public decimal ReturnAdvanceReceiptAmount { get; set; }
        public decimal ReturnOweCashAmount { get; set; }

        #endregion


        #region 收欠款 =应收金额-预收款

        public decimal ReceiptCashOweCashAmountSum { get; set; }
        public decimal ReceiptCashReceivableAmount { get; set; }
        public decimal ReceiptCashAdvanceReceiptAmount { get; set; }

        #endregion


        #region 收预收款 =预收金额-欠款

        public decimal AdvanceReceiptSum { get; set; }
        public decimal AdvanceReceiptAmount { get; set; }
        public decimal AdvanceReceiptOweCashAmount { get; set; }

        #endregion

        #region 费用支出 = 支出金额-欠款

        public decimal CostExpenditureSum { get; set; }
        public decimal CostExpenditureAmount { get; set; }
        public decimal CostExpenditureOweCashAmount { get; set; }

        #endregion

        /// <summary>
        /// 单据ID
        /// </summary>
        public int BillId { get; set; }

        /// <summary>
        /// 操作源
        /// </summary>
        public int? Operation { get; set; }

        /// <summary>
        /// 支付方式
        /// </summary>
        [NotMapped]
        public List<FinanceReceiveAccountBillAccounting> Accounts { get; set; }

        /// <summary>
        /// 优惠金额
        /// </summary>
        public int PreferentialAmount { get; set; }


        /// <summary>
        /// 销售商品(含赠)
        /// </summary>
        public int SaleProductCount { get; set; }
        /// <summary>
        /// 赠送商品
        /// </summary>
        public int GiftProductCount { get; set; }
        /// <summary>
        /// 退货商品
        /// </summary>
        public int ReturnProductCount { get; set; }

        /// <summary>
        /// 上交单据的交易日期
        /// </summary>
        public DateTime HandInTransactionDate { get; set; }


        private ICollection<FinanceReceiveAccountBillAccounting> _financeReceiveAccountBillAccountings;
        /// <summary>
        /// (导航)收款账户
        /// </summary>
        public virtual ICollection<FinanceReceiveAccountBillAccounting> FinanceReceiveAccountBillAccountings
        {
            get { return _financeReceiveAccountBillAccountings ?? (_financeReceiveAccountBillAccountings = new List<FinanceReceiveAccountBillAccounting>()); }
            protected set { _financeReceiveAccountBillAccountings = value; }
        }

    }

    /// <summary>
    ///  收款账户（销售单科目映射表）
    /// </summary>
    public class FinanceReceiveAccountBillAccounting : BaseAccount
    {
        private int FinanceReceiveAccountBillId;

        //(导航) 会计科目
        public virtual AccountingOption AccountingOption { get; set; }
        //(导航) 销售单
        public virtual FinanceReceiveAccountBill FinanceReceiveAccountBill { get; set; }
    }

    /// <summary>
    /// 对账商品汇总
    /// </summary>
    public class RankProduct
    {
        public int BusinessUserId { get; set; }
        /// <summary>
        /// 商品Id
        /// </summary>
        public int ProductId { get; set; }
        /// <summary>
        /// 商品名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 商品编码
        /// </summary>
        public string ProductCode { get; set; }

        /// <summary>
        /// 商品类别
        /// </summary>
        public string CategoryName { get; set; }
        public int CategoryId { get; set; }

        /// <summary>
        /// 单位换算
        /// </summary>
        public string UnitConversion { get; set; }

        /// <summary>
        /// 大单位转化量
        /// </summary>
        public int BigQuantity { get; set; }

        /// <summary>
        /// 中单位转换量
        /// </summary>
        public int StrokeQuantity { get; set; }

        /// <summary>
        /// 数量(含赠)
        /// </summary>
        public int? Quantity { get; set; } = 0;

        /// <summary>
        /// 单位名称
        /// </summary>
        public string UnitName { get; set; }

        /// <summary>
        /// 单据交易量金额
        /// </summary>
        public decimal? Amount { get; set; } = 0;

        /// <summary>
        /// 是否赠品
        /// </summary>
        public bool Gift { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }
    }
}
