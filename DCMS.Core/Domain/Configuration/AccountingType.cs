using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DCMS.Core.Domain.Configuration
{

    /// <summary>
    /// 表示科目类别
    /// </summary>
    public class AccountingType : BaseEntity
    {

        private ICollection<AccountingOption> _accountingOptions;

        /// <summary>
        /// 类别名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Enabled { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int? DisplayOrder { get; set; } = 0;

        /// <summary>
        /// 会计科目（AccountingEnum 枚举类型）
        /// </summary>
        public int AccountingId { get; set; }


        /// <summary>
        ///导航属性
        /// </summary>
        public virtual ICollection<AccountingOption> AccountingOptions
        {
            get { return _accountingOptions ?? (_accountingOptions = new List<AccountingOption>()); }
            protected set { _accountingOptions = value; }
        }
    }



    /// <summary>
    /// 表示科目项目
    /// </summary>
    public partial class AccountingOption : BaseEntity
    {

        /// <summary>
        /// 科目类别
        /// </summary>
        public int AccountingTypeId { get; set; }

        /// <summary>
        /// 科目代码类型
        /// </summary>
        public int? AccountCodeTypeId { get; set; } = 0;

        /// <summary>
        /// 父模块Id
        /// </summary>
        public int? ParentId { get; set; } = 0;


        /// <summary>
        /// "此属性只能用于递归排序，禁止使用 Number 关联科目"
        /// </summary>
        public int Number { get; set; } = 0;

        /// <summary>
        /// 科目名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 科目代码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int? DisplayOrder { get; set; } = 0;

        /// <summary>
        /// 是否启用
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Enabled { get; set; }

        /// <summary>
        /// 是否默认账户
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool? IsDefault { get; set; }

        [Column(TypeName = "BIT(1)")]
        public bool? IsLeaf { get; set; }

        /// <summary>
        /// 是否自定义会计科目
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool? IsCustom { get; set; }

        /// <summary>
        /// 导航属性
        /// </summary>
        [JsonIgnore]
        public virtual AccountingType AccountingType { get; set; }

        /// <summary>
        /// 编码级次
        /// </summary>
        public int CodeLevel { get; set; }

        /// <summary>
        /// 末级标志
        /// </summary>
        public int EndFlag { get; set; }

        /// <summary>
        /// 余额方向(借,贷) 
        /// </summary>
        public string BalanceOri { get; set; }

        [NotMapped]
        public bool Selected { get; set; }

        /// <summary>
        /// 初始余额
        /// </summary>
        public decimal InitBalance { get; set; } = 0;
    }


    /// <summary>
    /// 表示单据财务科目项目配置
    /// </summary>
    public class FinanceAccountingOptionSetting
    {
        /// <summary>
        /// 销售单(收款账户) 配置
        /// </summary>
        public IList<AccountingOption> SaleBillAccountingOptions { get; set; } = new List<AccountingOption>();

        /// <summary>
        /// 销售订单(收款账户) 配置
        /// </summary>
        public IList<AccountingOption> SaleReservationBillAccountingOptions { get; set; } = new List<AccountingOption>();

        /// <summary>
        /// 退货单(收款账户) 配置
        /// </summary>
        public IList<AccountingOption> ReturnBillAccountingOptions { get; set; } = new List<AccountingOption>();

        /// <summary>
        /// 退货订单(收款账户) 配置
        /// </summary>
        public IList<AccountingOption> ReturnReservationBillAccountingOptions { get; set; } = new List<AccountingOption>();

        /// <summary>
        /// 收款单(收款账户) 配置
        /// </summary>
        public IList<AccountingOption> ReceiptAccountingOptions { get; set; } = new List<AccountingOption>();

        /// <summary>
        /// 付款单(付款账户) 配置
        /// </summary>
        public IList<AccountingOption> PaymentAccountingOptions { get; set; } = new List<AccountingOption>();

        /// <summary>
        /// 预收款单(收款账户) 配置
        /// </summary>
        public IList<AccountingOption> AdvanceReceiptAccountingOptions { get; set; } = new List<AccountingOption>();

        /// <summary>
        /// 预付款单(付款账户) 配置
        /// </summary>
        public IList<AccountingOption> AdvancePaymentAccountingOptions { get; set; } = new List<AccountingOption>();

        /// <summary>
        /// 采购单(付款账户) 配置
        /// </summary>
        public IList<AccountingOption> PurchaseBillAccountingOptions { get; set; } = new List<AccountingOption>();

        /// <summary>
        /// 采购退货单(付款账户) 配置
        /// </summary>
        public IList<AccountingOption> PurchaseReturnBillAccountingOptions { get; set; } = new List<AccountingOption>();

        /// <summary>
        /// 费用支出（支出账户） 配置
        /// </summary>
        public IList<AccountingOption> CostExpenditureAccountingOptions { get; set; } = new List<AccountingOption>();

        /// <summary>
        /// 财务收入（收款账户）  配置
        /// </summary>
        public IList<AccountingOption> FinancialIncomeAccountingOptions { get; set; } = new List<AccountingOption>();

        /// <summary>
        /// 收款对账（会计科目）
        /// </summary>
        public IList<AccountingOption> FinanceReceiveAccountingOptions { get; set; } = new List<AccountingOption>();
    }


    public class Accounting
    {
        public string Name { get; set; }

        /// <summary>
        /// 会计科目
        /// </summary>
        public int AccountingOptionId { get; set; } = 0;

        /// <summary>
        /// 金额
        /// </summary>
        public decimal CollectionAmount { get; set; } = 0;

    }


    /// <summary>
    /// 表示会计科目树
    /// </summary>
    public class AccountingTree
    {
        public int Id { get; set; }
        public int StoreId { get; set; }
        public int AccountingTypeId { get; set; }
        public int? AccountCodeTypeId { get; set; } = 0;
        public int? ParentId { get; set; } = 0;
        public int Number { get; set; } = 0;
        public string Name { get; set; }
        public string Code { get; set; }
        public int? DisplayOrder { get; set; } = 0;
        public bool Enabled { get; set; }
        public bool? IsDefault { get; set; }
        public bool? IsLeaf { get; set; }
        public bool? IsCustom { get; set; }
        public List<AccountingTree> Children { get; set; }
    }


    public class AccountingOptionTree
    {
        public bool Visible { get; set; }
        public AccountingOption Option { get; set; }
        public int MaxItems { get; set; } = 0;
        public List<AccountingOptionTree> Children { get; set; }
    }


    public class AccountingOptionTree<T> where T : BaseAccount
    {
        public bool Visible { get; set; }
        public AccountingOption Option { get; set; }
        public int MaxItems { get; set; } = 0;
        public List<AccountingOptionTree<T>> Children { get; set; }
        public T Bearer { get; set; } = default;
    }

}
