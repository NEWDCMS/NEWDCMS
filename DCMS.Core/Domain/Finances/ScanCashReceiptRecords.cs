using System;

namespace DCMS.Core.Domain.Finances
{
    /// <summary>
    /// 扫码收款明细记录
    /// </summary>
    public class ScanCashReceiptRecords : BaseEntity
    {

        /// <summary>
        /// 客户
        /// </summary>
        public int TerminalId { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        public int TerminalName { get; set; }

        /// <summary>
        /// 收款时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }


        /// <summary>
        /// 收款员工
        /// </summary>
        public int Payee { get; set; }

        /// <summary>
        /// 到账时间
        /// </summary>
        public DateTime TimeOfReceipt { get; set; }

        /// <summary>
        /// 支付流水
        /// </summary>
        public string Paymentflow { get; set; }

        /// <summary>
        /// 付款方式
        /// </summary>
        public int PaymentMethod { get; set; }

        /// <summary>
        /// 收款方式
        /// </summary>
        public string CollectionMethod { get; set; }

        /// <summary>
        /// 收款账户 
        /// </summary>
        public int CollectionAccount { get; set; }

        /// <summary>
        /// 收款金额
        /// </summary>
        public decimal CollectionAmount { get; set; }

        /// <summary>
        /// 实际到账金额
        /// </summary>
        public decimal ActualAmountReceived { get; set; }

        /// <summary>
        /// 费率
        /// </summary>
        public decimal Rate { get; set; }
    }
}
