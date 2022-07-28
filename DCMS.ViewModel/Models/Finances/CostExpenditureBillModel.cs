using DCMS.Core;
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;


namespace DCMS.ViewModel.Models.Finances
{


    public partial class CostExpenditureBillListModel : BaseModel
    {
        public CostExpenditureBillListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Lists = new List<CostExpenditureBillModel>();
            DynamicColumns = new List<string>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<CostExpenditureBillModel> Lists { get; set; }
        public List<string> DynamicColumns { get; set; }



        [HintDisplayName("员工", "员工")]
        public int? EmployeeId { get; set; } = 0;
        public string EmployeeName { get; set; }
        public SelectList Employees { get; set; }


        [HintDisplayName("客户", "客户")]
        public int CustomerId { get; set; } = 0;
        public string CustomerName { get; set; }



        [HintDisplayName("费用类别", "费用类别")]
        public int AccountingOptionId { get; set; } = 0;
        public string AccountingOptionName { get; set; }

        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }


        [HintDisplayName("状态(审核)", "状态(审核)")]
        public bool? AuditedStatus { get; set; }


        [HintDisplayName("开始日期", "开始日期")]
        public DateTime? StartTime { get; set; }


        [HintDisplayName("截止日期", "截止日期")]
        public DateTime? EndTime { get; set; }


        [HintDisplayName(" 显示红冲的数据", " 显示红冲的数据")]
        public bool? ShowReverse { get; set; }


        [HintDisplayName("按审核时间", " 按审核时间")]
        public bool? SortByAuditedTime { get; set; }



        [HintDisplayName("备注", " 备注")]
        public string Remark { get; set; }

    }

    /// <summary>
    /// 用于表示费用支出单据
    /// </summary>
    public class CostExpenditureBillModel : BaseEntityModel
    {
        public CostExpenditureBillModel()
        {
            CostExpenditureBillAccountings = new List<CostExpenditureBillAccountingModel>();
            Items = new List<CostExpenditureItemModel>();
        }

        public int BillTypeEnumId { get; set; }

        [HintDisplayName("单据编号", " 单据编号")]
        public string BillNumber { get; set; }
        public string BillBarCode { get; set; }

        [HintDisplayName("员工", "员工")]
        public int EmployeeId { get; set; } = 0;
        public string EmployeeName { get; set; }
        public SelectList Employees { get; set; }


        [HintDisplayName("客户", "客户")]
        public int CustomerId { get; set; } = 0;
        public string CustomerName { get; set; }
        public string CustomerCode { get; set; }


        [HintDisplayName("费用类别", "费用类别")]
        public int AccountingOptionId { get; set; } = 0;
        public string AccountingOptionName { get; set; }



        [HintDisplayName("操作日期", " 操作日期")]
        public DateTime CreatedOnUtc { get; set; }


        [HintDisplayName("付款日期", " 付款日期")]
        public DateTime? PayDate { get; set; }


        [HintDisplayName("备注", " 备注")]
        public string Remark { get; set; }

        [HintDisplayName("制单人", "制单人")]
        public int MakeUserId { get; set; } = 0;
        public string MakeUserName { get; set; }

        [HintDisplayName("审核人", "审核人")]
        public int? AuditedUserId { get; set; } = 0;
        public string AuditedUserName { get; set; }

        [HintDisplayName(" 状态(审核)", "  状态(审核)")]
        public bool AuditedStatus { get; set; }
        public string AuditedStatusName { get; set; }

        [HintDisplayName("审核时间", " 审核时间")]
        public DateTime? AuditedDate { get; set; }

        [HintDisplayName("红冲人", " 红冲人")]
        public int? ReversedUserId { get; set; } = 0;

        [HintDisplayName("红冲状态", " 红冲状态")]
        public bool ReversedStatus { get; set; }

        [HintDisplayName("红冲时间", " 红冲时间")]
        public DateTime? ReversedDate { get; set; }

        [HintDisplayName("上交状态", "上交状态")]
        public bool? HandInStatus { get; set; }

        [HintDisplayName("上交时间", "上交时间")]
        public DateTime? HandInDate { get; set; }

        [HintDisplayName("打印数", " 打印数")]
        public int? PrintNum { get; set; } = 0;
        [HintDisplayName("数据来源", "数据来源")]
        public int Operation { get; set; } = 0;

        public decimal OweCash { get; set; } = 0;

        /// <summary>
        /// 单据总金额
        /// </summary>
        public decimal? TotalAmount { get; set; } = 0;
        /// <summary>
        /// 收款账户
        /// </summary>
        public IList<CostExpenditureBillAccountingModel> CostExpenditureBillAccountings { get; set; }

        /// <summary>
        /// 项目
        /// </summary>
        public IList<CostExpenditureItemModel> Items { get; set; }

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
    /// 用于表示费用支出单据项目
    /// </summary>
    public class CostExpenditureItemModel : BaseEntityModel
    {

        [HintDisplayName("费用支出单", " 费用支出单Id")]
        public int CostExpenditureBillId { get; set; } = 0;

        [HintDisplayName("费用类别", " 费用类别")]
        public int AccountingOptionId { get; set; } = 0;
        public string AccountingOptionName { get; set; }

        [HintDisplayName("客户", " 客户")]
        public int CustomerId { get; set; } = 0;
        public string CustomerName { get; set; }


        [HintDisplayName("费用合同", " 费用合同")]
        public int CostContractId { get; set; } = 0;
        public string CostContractName { get; set; }

        [HintDisplayName("月份", " 月份")]
        public int Month { get; set; }

        [HintDisplayName("金额", " 金额")]
        public decimal? Amount { get; set; } = 0;


        [HintDisplayName("备注", " 备注")]
        public string Remark { get; set; }


        [HintDisplayName("创建日期", " 创建日期")]
        public DateTime CreatedOnUtc { get; set; }

    }


    /// <summary>
    /// 费用支出账户（费用支出单据科目映射表）
    /// </summary>
    public class CostExpenditureBillAccountingModel : BaseAccountModel
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
    public class CostExpenditureUpdateModel : BaseBalance
    {
        public string BillNumber { get; set; }
        /// <summary>
        /// 员工
        /// </summary>
        public int EmployeeId { get; set; } = 0;

        /// <summary>
        /// 客户
        /// </summary>
        public int CustomerId { get; set; } = 0;

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 操作源
        /// </summary>
        public int? Operation { get; set; }

        public decimal OweCash { get; set; }

        /// <summary>
        /// 项目
        /// </summary>
        public List<CostExpenditureItemModel> Items { get; set; }

        /// <summary>
        /// 支出账户
        /// </summary>
        public List<CostExpenditureBillAccountingModel> Accounting { get; set; }

    }

}
