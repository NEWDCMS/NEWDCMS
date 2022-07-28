using DCMS.Core.Domain.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


namespace DCMS.Core.Domain.Finances
{

    /// <summary>
    /// 用于表示预付款单据
    /// </summary>
    public class AdvancePaymentBill : BaseBill<AdvancePaymentBillAccounting>
    {

        public AdvancePaymentBill()
        {
            BillType = BillTypeEnum.AdvancePaymentBill;
        }

        /// <summary>
        /// 付款人
        /// </summary>
        public int Draweer { get; set; }

        /// <summary>
        /// 供应商
        /// </summary>
        public int ManufacturerId { get; set; }

        /// <summary>
        /// 付款类型(预付款)
        /// </summary>
        public int PaymentType { get; set; }


        /// <summary>
        /// 打印数
        /// </summary>
        public int? PrintNum { get; set; } = 0;


        /// <summary>
        /// 预付款账户
        /// </summary>
        public int? AccountingOptionId { get; set; } = 0;

        /// <summary>
        /// 预付款金额
        /// </summary>
        public decimal? AdvanceAmount { get; set; }

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
        /// 记账凭证
        /// </summary>
        public int VoucherId { get; set; }
    }

    /// <summary>
    ///  预付款账户（预付款单据科目映射表）
    /// </summary>
    public class AdvancePaymentBillAccounting : BaseAccount
    {

        private int AdvancePaymentBillId;
        /// <summary>
        /// 供应商
        /// </summary>
        public int ManufacturerId { get; set; }

        /// <summary>
        /// 副本
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Copy { get; set; }

        //(导航) 会计科目
        [JsonIgnore]
        public virtual AccountingOption AccountingOption { get; set; }
        //(导航) 预付款单据
        [JsonIgnore]
        public virtual AdvancePaymentBill AdvancePaymentBill { get; set; }
    }


    /// <summary>
    /// 保存或者编辑
    /// </summary>
    public class AdvancePaymenBillUpdate : BaseEntity
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
        /// 预付款金额
        /// </summary>
        public decimal? AdvanceAmount { get; set; } = 0;

        public int PaymentType { get; set; } = 0;

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
        /// 付款账户
        /// </summary>
        public List<AdvancePaymentBillAccounting> Accounting { get; set; }

    }

}
