using DCMS.Core.Domain.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


namespace DCMS.Core.Domain.Finances
{

    /// <summary>
    /// 用于表示预收款单据
    /// </summary>
    public class AdvanceReceiptBill : BaseBill<AdvanceReceiptBillAccounting>
    {
        public AdvanceReceiptBill()
        {
            BillType = BillTypeEnum.AdvanceReceiptBill;
        }

        /// <summary>
        /// 收款人
        /// </summary>
        public int Payeer { get; set; }

        /// <summary>
        /// 客户
        /// </summary>
        public int CustomerId { get; set; }


        /// <summary>
        /// 预收款账户
        /// </summary>
        public int? AccountingOptionId { get; set; } = 0;

        /// <summary>
        /// 预收款金额
        /// </summary>
        public decimal? AdvanceAmount { get; set; }

        /// <summary>
        /// 优惠金额
        /// </summary>
        public decimal? DiscountAmount { get; set; }


        /// <summary>
        /// 欠款金额
        /// </summary>
        public decimal? OweCash { get; set; }

        /// <summary>
        /// 打印数
        /// </summary>
        public int? PrintNum { get; set; } = 0;

        /// <summary>
        /// 操作源
        /// </summary>
        public int? Operation { get; set; } = 0;
        public OperationEnum Operations
        {
            get { return (OperationEnum)Operation; }
            set { Operation = (int)value; }
        }


        /// <summary>
        /// 上交状态
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool? HandInStatus { get; set; } = false;

        /// <summary>
        /// 上交时间
        /// </summary>
        public DateTime? HandInDate { get; set; }

        /// <summary>
        /// 记账凭证
        /// </summary>
        public int VoucherId { get; set; }


        /// <summary>
        /// 收款状态
        /// </summary>
        public int ReceiptStatus { get; set; }
        public ReceiptStatus ReceivedStatus
        {
            get { return (ReceiptStatus)ReceiptStatus; }
            set { ReceiptStatus = (int)value; }
        }
    }


    /// <summary>
    ///  预收款账户（预收款单据科目映射表）
    /// </summary>
    public class AdvanceReceiptBillAccounting : BaseAccount
    {
        private int AdvanceReceiptBillId;

        /// <summary>
        /// 客户
        /// </summary>
        public int TerminalId { get; set; }

        /// <summary>
        /// 副本
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Copy { get; set; }

        //(导航) 会计科目
        public virtual AccountingOption AccountingOption { get; set; }
        //(导航) 预收款单据
        public virtual AdvanceReceiptBill AdvanceReceiptBill { get; set; }
    }


    public class AdvanceReceiptBillUpdate : BaseEntity
    {
        public string BillNumber { get; set; }
        /// <summary>
        /// 客户
        /// </summary>
        public int CustomerId { get; set; } = 0;

        /// <summary>
        /// 收款人
        /// </summary>
        public int? Payeer { get; set; } = 0;


        /// <summary>
        /// 预收款金额
        /// </summary>
        public decimal? AdvanceAmount { get; set; } = 0;


        /// <summary>
        /// 优惠金额
        /// </summary>
        public decimal? DiscountAmount { get; set; } = 0;

        /// <summary>
        /// 欠款
        /// </summary>
        public decimal? OweCash { get; set; } = 0;

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 操作源
        /// </summary>
        public int? Operation { get; set; } = 0;

        /// <summary>
        /// 预收款账户
        /// </summary>
        public int AccountingOptionId { get; set; } = 0;


        /// <summary>
        /// 收款账户
        /// </summary>
        public List<AdvanceReceiptBillAccounting> Accounting { get; set; }

    }

}
