using DCMS.Core;
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;


namespace DCMS.ViewModel.Models.Finances
{

    public partial class FinancialIncomeBillListModel : BaseModel
    {
        public FinancialIncomeBillListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            DynamicColumns = new List<string>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<FinancialIncomeBillModel> Items { get; set; }
        public List<string> DynamicColumns { get; set; }

        [HintDisplayName("业务员", "业务员")]
        public int? SalesmanId { get; set; } = 0;
        public string SalesmanName { get; set; }
        public SelectList Salesmans { get; set; }

        [HintDisplayName("客户/供应商类型", "客户/供应商类型")]
        public int CustomerOrManufacturerType { get; set; } = 0;

        [HintDisplayName("客户", "客户")]
        public int CustomerId { get; set; } = 0;
        public string CustomerName { get; set; }

        [HintDisplayName("供应商", "供应商")]
        public int ManufacturerId { get; set; } = 0;
        public string ManufacturerName { get; set; }

        [HintDisplayName("状态(审核)", "状态(审核)")]
        public bool? AuditedStatus { get; set; }

        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }

        [HintDisplayName("开始日期", "开始日期")]
        public DateTime? StartTime { get; set; }


        [HintDisplayName("截止日期", "截止日期")]
        public DateTime? EndTime { get; set; }


        [HintDisplayName(" 显示红冲的数据", " 显示红冲的数据")]
        public bool? ShowReverse { get; set; }


        [HintDisplayName("按审核时间", " 按审核时间")]
        public bool? SortByAuditedTime { get; set; }


        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }
    }

    /// <summary>
    /// 用于表示财务收入单据(其它收入)
    /// </summary>
    public class FinancialIncomeBillModel : BaseEntityModel
    {
        public FinancialIncomeBillModel()
        {
            FinancialIncomeBillAccountings = new List<FinancialIncomeBillAccountingModel>();
            Items = new List<FinancialIncomeItemModel>();
        }

        /// <summary>
        /// 单据枚举
        /// </summary>
        public int BillTypeEnumId { get; set; }

        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }
        public string BillBarCode { get; set; }


        [HintDisplayName("业务员", "业务员")]
        public int SalesmanId { get; set; } = 0;
        public string SalesmanName { get; set; }
        public SelectList Salesmans { get; set; }


        /// <summary>
        /// 供应商
        /// </summary>
        public int ManufacturerId { get; set; }
        [HintDisplayName("供应商", "供应商")]
        public string ManufacturerName { get; set; }

        /// <summary>
        /// 客户Id
        /// </summary>
        public int TerminalId { get; set; }

        [HintDisplayName("客户", "客户")]
        public string TerminalName { get; set; }


        [HintDisplayName("收入类别", "收入类别")]
        public int AccountingOptionId { get; set; } = 0;
        public string AccountingOptionName { get; set; }


        [HintDisplayName("开单日期", "开单日期")]
        public DateTime CreatedOnUtc { get; set; }

        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }

        [HintDisplayName("制单人", "制单人")]
        public int MakeUserId { get; set; } = 0;
        public string MakeUserName { get; set; }

        [HintDisplayName("审核人", "审核人")]
        public int? AuditedUserId { get; set; } = 0;
        public string AuditedUserName { get; set; }

        [HintDisplayName("状态(审核)", "状态(审核)")]
        public bool AuditedStatus { get; set; }

        [HintDisplayName("审核时间", "审核时间")]
        public DateTime? AuditedDate { get; set; }

        [HintDisplayName("红冲人", "红冲人")]
        public int? ReversedUserId { get; set; } = 0;

        [HintDisplayName("红冲状态", "红冲状态")]
        public bool ReversedStatus { get; set; }

        [HintDisplayName("红冲时间", "红冲时间")]
        public DateTime? ReversedDate { get; set; }

        [HintDisplayName("打印数", "打印数")]
        public int? PrintNum { get; set; } = 0;
        [HintDisplayName("数据来源", "数据来源")]
        public int Operation { get; set; } = 0;

        public decimal OweCash { get; set; } = 0;


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
        /// 项目
        /// </summary>
        public IList<FinancialIncomeItemModel> Items { get; set; }

        /// <summary>
        /// 收款账户
        /// </summary>
        public IList<FinancialIncomeBillAccountingModel> FinancialIncomeBillAccountings { get; set; }

    }


    /// <summary>
    /// 用于表示财务收入单据项目
    /// </summary>
    public class FinancialIncomeItemModel : BaseEntityModel
    {

        [HintDisplayName("财务收入单", "财务收入单Id")]
        public int FinancialIncomeBillId { get; set; } = 0;


        [HintDisplayName("收入类别", "收入类别")]
        public int AccountingOptionId { get; set; } = 0;
        public string AccountingOptionName { get; set; }

        [HintDisplayName("客户/供应商类型", "客户/供应商类型")]
        public int CustomerOrManufacturerType { get; set; } = 0;

        [HintDisplayName("客户/供应商", "客户/供应商")]
        public int CustomerOrManufacturerId { get; set; } = 0;

        public string CustomerName { get; set; }
        public string ManufacturerName { get; set; }

        [HintDisplayName("单据金额", "单据金额")]
        public decimal? Amount { get; set; } = 0;

        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }


        [HintDisplayName("创建日期", "创建日期")]
        public DateTime CreatedOnUtc { get; set; }



    }


    /// <summary>
    ///  财务收入账户（财务收入单据科目映射表）
    /// </summary>
    public class FinancialIncomeBillAccountingModel : BaseAccountModel
    {
        public string Name { get; set; }

        /// <summary>
        /// 是否默认
        /// </summary>
        public bool IsDefault { get; set; }

        public int AccountCodeTypeId { get; set; }

    }



    /// <summary>
    /// 项目保存或者编辑
    /// </summary>
    public class FinancialIncomeUpdateModel : BaseBalance
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

        public int ManufacturerId { get; set; } = 0;
        public int TerminalId { get; set; } = 0;

        public decimal OweCash { get; set; }

        /// <summary>
        /// 项目
        /// </summary>
        public List<FinancialIncomeItemModel> Items { get; set; }

        /// <summary>
        /// 支出账户
        /// </summary>
        public List<FinancialIncomeBillAccountingModel> Accounting { get; set; }

        /// <summary>
        /// 预付款余额
        /// </summary>
        public decimal AdvancedPaymentsAmount { get; set; }

    }

}
