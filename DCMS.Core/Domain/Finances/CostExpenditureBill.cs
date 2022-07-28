using DCMS.Core.Domain.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


namespace DCMS.Core.Domain.Finances
{

    /// <summary>
    /// 用于表示费用支出单据
    /// </summary>
    public class CostExpenditureBill : BaseBill<CostExpenditureItem>
    {

        public CostExpenditureBill()
        {
            BillType = BillTypeEnum.CostExpenditureBill;
        }

        private ICollection<CostExpenditureBillAccounting> _costExpenditureBillAccountings;

        /// <summary>
        /// 客户Id
        /// </summary>
        public int TerminalId { get; set; }

        /// <summary>
        /// 员工
        /// </summary>
        public int EmployeeId { get; set; }


        /// <summary>
        /// 付款日期
        /// </summary>
        public DateTime? PayDate { get; set; }


        /// <summary>
        /// 收入金额
        /// </summary>
        public decimal SumAmount { get; set; }

        /// <summary>
        /// 欠款金额
        /// </summary>
        public decimal OweCash { get; set; }

        /// <summary>
        /// 优惠金额
        /// </summary>
        public decimal DiscountAmount { get; set; }

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

        ///// <summary>
        ///// (导航)费用支出单项目
        ///// </summary>
        //public virtual ICollection<CostExpenditureItem> CostExpenditureItems
        //{
        //    get { return _costExpenditureItems ?? (_costExpenditureItems = new List<CostExpenditureItem>()); }
        //    protected set { _costExpenditureItems = value; }
        //}


        /// <summary>
        /// (导航)费用支出账户
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<CostExpenditureBillAccounting> CostExpenditureBillAccountings
        {
            get { return _costExpenditureBillAccountings ?? (_costExpenditureBillAccountings = new List<CostExpenditureBillAccounting>()); }
            protected set { _costExpenditureBillAccountings = value; }
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

        /// <summary>
        /// 签收状态：0待签收，1已签收，2拒收
        /// </summary>
        public int SignStatus { get; set; }
    }


    /// <summary>
    /// 用于表示费用支出单据项目
    /// </summary>
    public class CostExpenditureItem : BaseEntity
    {

        /// <summary>
        /// 费用支出单Id
        /// </summary>
        public int CostExpenditureBillId { get; set; }

        /// <summary>
        /// 费用类别
        /// </summary>
        public int AccountingOptionId { get; set; }

        /// <summary>
        /// 客户
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// 费用合同
        /// </summary>
        public int CostContractId { get; set; }

        /// <summary>
        /// 月份
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// 金额
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


        public virtual CostExpenditureBill CostExpenditureBill { get; set; }

    }


    /// <summary>
    ///  费用支出账户（费用支出单据科目映射表）
    /// </summary>
    public class CostExpenditureBillAccounting : BaseAccount
    {
        private int CostExpenditureBillId;
        //(导航) 会计科目
        public virtual AccountingOption AccountingOption { get; set; }
        //(导航)费用支出单据
        public virtual CostExpenditureBill CostExpenditureBill { get; set; }
    }


    /// <summary>
    /// 项目保存或者编辑
    /// </summary>
    public class CostExpenditureBillUpdate : BaseEntity
    {
        public string BillNumber { get; set; }
        /// <summary>
        /// 员工
        /// </summary>
        public int EmployeeId { get; set; } = 0;
        /// <summary>
        /// 客户
        /// </summary>
        public int CustomerId { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 欠款金额
        /// </summary>
        public decimal OweCash { get; set; }

        /// <summary>
        /// 操作源
        /// </summary>
        public int? Operation { get; set; } = 0;

        /// <summary>
        /// 项目
        /// </summary>
        public List<CostExpenditureItem> Items { get; set; }

        /// <summary>
        /// 支出账户
        /// </summary>
        public List<CostExpenditureBillAccounting> Accounting { get; set; }

    }

}
