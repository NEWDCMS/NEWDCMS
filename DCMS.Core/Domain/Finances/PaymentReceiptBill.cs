using DCMS.Core.Domain.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;


namespace DCMS.Core.Domain.Finances
{

    /// <summary>
    /// 用于表示付款单据
    /// </summary>
    public class PaymentReceiptBill : BaseBill<PaymentReceiptItem>
    {
        public PaymentReceiptBill()
        {
            BillType = BillTypeEnum.PaymentReceiptBill;
        }


        //private ICollection<PaymentReceiptItem> _paymentReceiptItems;
        private ICollection<PaymentReceiptBillAccounting> _paymentReceiptBillAccountings;


        /// <summary>
        /// 付款人
        /// </summary>
        public int Draweer { get; set; }

        /// <summary>
        /// 供应商
        /// </summary>
        public int ManufacturerId { get; set; }

        /// <summary>
        /// 付款日期
        /// </summary>
        //public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        //public string Remark { get; set; }

        ///// <summary>
        ///// 制单人
        ///// </summary>
        //public int MakeUserId { get; set; }


        ///// <summary>
        ///// 审核人
        ///// </summary>
        //public int? AuditedUserId { get; set; }
        ///// <summary>
        ///// 状态(审核)
        ///// </summary>
        //[Column(TypeName = "BIT(1)")]
        //public bool AuditedStatus { get; set; }

        ///// <summary>
        ///// 审核时间
        ///// </summary>
        //public DateTime? AuditedDate { get; set; }


        ///// <summary>
        ///// 红冲人
        ///// </summary>
        //public int? ReversedUserId { get; set; }

        ///// <summary>
        ///// 红冲状态
        ///// </summary>
        //[Column(TypeName = "BIT(1)")] public bool ReversedStatus { get; set; }

        ///// <summary>
        ///// 红冲时间
        ///// </summary>
        //public DateTime? ReversedDate { get; set; }


        /// <summary>
        /// 优惠金额
        /// </summary>
        public decimal? DiscountAmount { get; set; }


        /// <summary>
        /// 剩余金额
        /// </summary>
        public decimal? AmountOwedAfterReceipt { get; set; }


        /// <summary>
        /// 打印数
        /// </summary>
        public int? PrintNum { get; set; }

        /// <summary>
        /// 操作源
        /// </summary>
        public int? Operation { get; set; }
        public OperationEnum Operations
        {
            get { return (OperationEnum)Operation; }
            set { Operation = (int)value; }
        }

        ///// <summary>
        ///// (导航)付款单项目
        ///// </summary>
        //public virtual ICollection<PaymentReceiptItem> PaymentReceiptItems
        //{
        //    get { return _paymentReceiptItems ?? (_paymentReceiptItems = new List<PaymentReceiptItem>()); }
        //    protected set { _paymentReceiptItems = value; }
        //}


        /// <summary>
        /// (导航)付款账户
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<PaymentReceiptBillAccounting> PaymentReceiptBillAccountings
        {
            get { return _paymentReceiptBillAccountings ?? (_paymentReceiptBillAccountings = new List<PaymentReceiptBillAccounting>()); }
            protected set { _paymentReceiptBillAccountings = value; }
        }

        /// <summary>
        /// 记账凭证
        /// </summary>
        public int VoucherId { get; set; }
    }


    /// <summary>
    /// 用于表示付款单据项目
    /// </summary>
    public class PaymentReceiptItem : BaseEntity
    {

        /// <summary>
        /// 收款单Id
        /// </summary>
        public int PaymentReceiptBillId { get; set; }

        /// <summary>
        /// 单据编号
        /// </summary>
        public string BillNumber { get; set; }

        /// <summary>
        /// 单据编号
        /// </summary>
        public int BillId { get; set; }

        /// <summary>
        /// 单据类型
        /// </summary>
        public int BillTypeId { get; set; }
        public BillTypeEnum BillTypeEnum
        {
            get { return (BillTypeEnum)BillTypeId; }
            set { BillTypeId = (int)value; }
        }

        /// <summary>
        /// 开单日期(这里是单据的开出时间)
        /// </summary>
        public DateTime MakeBillDate { get; set; }

        /// <summary>
        /// 单据金额
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        ///优惠金额
        /// </summary>
        public decimal? DiscountAmount { get; set; }

        /// <summary>
        /// 已付金额
        /// </summary>
        public decimal? PaymentedAmount { get; set; }

        /// <summary>
        /// 尚欠金额
        /// </summary>
        public decimal? ArrearsAmount { get; set; }

        /// <summary>
        /// 本次优惠金额
        /// </summary>
        public decimal? DiscountAmountOnce { get; set; }

        /// <summary>
        /// 本次付款金额
        /// </summary>
        public decimal? ReceivableAmountOnce { get; set; }

        /// <summary>
        /// 付款后尚欠金额
        /// </summary>
        public decimal? AmountOwedAfterReceipt { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }


        public virtual PaymentReceiptBill PaymentReceiptBill { get; set; }

    }


    /// <summary>
    ///  付款账户（付款单据科目映射表）
    /// </summary>
    public class PaymentReceiptBillAccounting : BaseAccount
    {
        private int PaymentReceiptBillId;
        /// <summary>
        /// 供应商
        /// </summary>
        public int ManufacturerId { get; set; }

        //(导航) 会计科目
        public virtual AccountingOption AccountingOption { get; set; }
        //(导航) 付款单据
        public virtual PaymentReceiptBill PaymentReceiptBill { get; set; }
    }

    /// <summary>
    /// 项目保存或者编辑
    /// </summary>
    public class PaymentReceiptBillUpdate : BaseEntity
    {
        public string BillNumber { get; set; }
        /// <summary>
        /// 付款人
        /// </summary>
        public int Draweer { get; set; } = 0;


        /// <summary>
        /// 供应商
        /// </summary>
        public int ManufacturerId { get; set; } = 0;


        /// <summary>
        /// 优惠金额
        /// </summary>
        public decimal? DiscountAmount { get; set; } = 0;


        /// <summary>
        /// 剩余金额
        /// </summary>
        public decimal? AmountOwedAfterReceipt { get; set; } = 0;


        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 操作源
        /// </summary>
        public int? Operation { get; set; }

        /// <summary>
        /// 项目
        /// </summary>
        public List<PaymentReceiptItem> Items { get; set; }

        /// <summary>
        /// 收款账户
        /// </summary>
        public List<PaymentReceiptBillAccounting> Accounting { get; set; }

    }

}
