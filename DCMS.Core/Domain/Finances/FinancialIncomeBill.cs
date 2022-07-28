using DCMS.Core.Domain.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DCMS.Core.Domain.Finances
{

    /// <summary>
    /// 用于表示财务收入单据(其它收入)
    /// </summary>
    public class FinancialIncomeBill : BaseBill<FinancialIncomeItem>
    {
        public FinancialIncomeBill()
        {
            BillType = BillTypeEnum.FinancialIncomeBill;
        }

        //private ICollection<FinancialIncomeItem> _financialIncomeItems;
        private ICollection<FinancialIncomeBillAccounting> _financialIncomeBillAccountings;


        /// <summary>
        /// 业务员
        /// </summary>
        public int SalesmanId { get; set; }


        /// <summary>
        /// 客户Id
        /// </summary>
        public int TerminalId { get; set; }

        /// <summary>
        /// 供应商
        /// </summary>
        public int ManufacturerId { get; set; }

        /// <summary>
        /// 支出金额
        /// </summary>
        public decimal SumAmount { get; set; }

        /// <summary>
        /// 欠款金额
        /// </summary>
        public decimal OweCash { get; set; }

        /// <summary>
        ///优化金额
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// 打印数
        /// </summary>
        public int? PrintNum { get; set; } = 0;


        /// <summary>
        /// 付款状态
        /// </summary>
        public int PayStatus { get; set; }
        public PayStatus PaymentStatus
        {
            get { return (PayStatus)PayStatus; }
            set { PayStatus = (int)value; }
        }


        /// <summary>
        /// 收款状态
        /// </summary>
        public int ReceiptStatus { get; set; }
        public ReceiptStatus ReceivedStatus
        {
            get { return (ReceiptStatus)ReceiptStatus; }
            set { ReceiptStatus = (int)value; }
        }

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
        /// (导航)财务收入账户
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<FinancialIncomeBillAccounting> FinancialIncomeBillAccountings
        {
            get { return _financialIncomeBillAccountings ?? (_financialIncomeBillAccountings = new List<FinancialIncomeBillAccounting>()); }
            protected set { _financialIncomeBillAccountings = value; }
        }

        /// <summary>
        /// 记账凭证
        /// </summary>
        public int VoucherId { get; set; }
    }


    /// <summary>
    /// 用于表示财务收入单据项目
    /// </summary>
    public class FinancialIncomeItem : BaseEntity
    {

        /// <summary>
        /// 财务收入单Id
        /// </summary>
        public int FinancialIncomeBillId { get; set; }


        /// <summary>
        /// 收入类别
        /// </summary>
        public int AccountingOptionId { get; set; }


        /// <summary>
        /// 客户/供应商 类型
        /// </summary>
        public int CustomerOrManufacturerType { get; set; }

        /// <summary>
        /// 客户/供应商
        /// </summary>
        public int CustomerOrManufacturerId { get; set; }


        /// <summary>
        /// 单据金额
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }


        //(导航) 财务收入单据
        public virtual FinancialIncomeBill FinancialIncomeBill { get; set; }

    }


    /// <summary>
    ///  财务收入账户（财务收入单据科目映射表）
    /// </summary>
    public class FinancialIncomeBillAccounting : BaseAccount
    {

        private int FinancialIncomeBillId;

        //(导航) 会计科目
        public virtual AccountingOption AccountingOption { get; set; }
        //(导航) 财务收入单据
        public virtual FinancialIncomeBill FinancialIncomeBill { get; set; }
    }

    /// <summary>
    /// 项目保存或者编辑
    /// </summary>
    public class FinancialIncomeBillUpdate : BaseEntity
    {
        public string BillNumber { get; set; }
        /// <summary>
        /// 业务员
        /// </summary>
        public int SalesmanId { get; set; } = 0;

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 客户Id
        /// </summary>
        public int TerminalId { get; set; }

        /// <summary>
        /// 供应商
        /// </summary>
        public int ManufacturerId { get; set; }

        public decimal OweCash { get; set; }


        /// <summary>
        /// 项目
        /// </summary>
        public List<FinancialIncomeItem> Items { get; set; }

        /// <summary>
        /// 支出账户
        /// </summary>
        public List<FinancialIncomeBillAccounting> Accounting { get; set; }

    }

}
