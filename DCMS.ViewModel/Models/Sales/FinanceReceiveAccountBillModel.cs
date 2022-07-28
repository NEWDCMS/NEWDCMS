using DCMS.Core;
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DCMS.Core.Domain.Sales;

namespace DCMS.ViewModel.Models.Sales
{
    public partial class FinanceReceiveAccountBillListModel : BaseModel
    {
        public PagingFilteringModel PagingFilteringContext { get; set; } = new PagingFilteringModel();
        /// <summary>
        /// 未上交
        /// </summary>
        public IList<FinanceReceiveAccountBillModel> Lists { get; set; } = new List<FinanceReceiveAccountBillModel>();

        /// <summary>
        /// 已上交
        /// </summary>
        public IList<FinanceReceiveAccountBillModel> SubmitLists { get; set; } = new List<FinanceReceiveAccountBillModel>(); 

        public List<string> DynamicColumns { get; set; } = new List<string>();

        #region  用于数据检索

        [HintDisplayName("员工", "员工")]
        public int? EmployeeId { get; set; } = 0;
        public SelectList Employees { get; set; }

        [HintDisplayName("收款人", "收款人")]
        public int? PayeerId { get; set; } = 0;
        public SelectList Payeers { get; set; }

        [DisplayName("开始时间")]
        [UIHint("DateTime")] public DateTime StartTime { get; set; }

        [DisplayName("开始时间")]
        [UIHint("DateTime")] public DateTime EndTime { get; set; }

        [DisplayName("单据号")]
        public string BillNumber { get; set; }

        [DisplayName("支付方式")]
        public int? PaymentId { get; set; } = 0;
        public SelectList Payments { get; set; }

        public int BillType { get; set; }

        public bool Gift { get; set; }

        #endregion
    }


    public class FinanceReceiveAccountBillModel : BaseEntityModel
    {
        public int TerminalId { get; set; }
        public string TerminalName { get; set; }

        public string BillNumber { get; set; }
        public string BillLink { get; set; }

        /// <summary>
        /// 单据类型
        /// </summary>
        public int BillType { get; set; }
        public int BillTypeId { get; set; }
        /// <summary>
        /// 业务员
        /// </summary>
        public int UserId { get; set; }
        public string UserName { get; set; }

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
        /// 支付方式
        /// </summary>
        public List<FinanceReceiveAccountBillAccounting> Accounts { get; set; } = new List<FinanceReceiveAccountBillAccounting>();

        /// <summary>
        /// 优惠金额
        /// </summary>
        public int PreferentialAmount { get; set; }

        /// <summary>
        /// 销售商品(含赠)
        /// </summary>
        public int SaleProductCount { get; set; }
        public List<RankProduct> SaleProducts { get; set; } = new List<RankProduct>();

        /// <summary>
        /// 赠送商品
        /// </summary>
        public int GiftProductCount { get; set; }
        public List<RankProduct> GiftProducts { get; set; } = new List<RankProduct>();
        /// <summary>
        /// 退货商品
        /// </summary>
        public int ReturnProductCount { get; set; }
        public List<RankProduct> ReturnProducts { get; set; } = new List<RankProduct>();
        /// <summary>
        /// 上交单据的交易日期
        /// </summary>
        public DateTime HandInTransactionDate { get; set; }


        /// <summary>
        /// 单据时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

    }



    /// <summary>
    /// 收款对账上交
    /// </summary>
    public class FinanceReceiveAccountBillSubmitModel
    {
        public IList<FinanceReceiveAccountBillModel> Items { get; set; } = new List<FinanceReceiveAccountBillModel>();
    }

}