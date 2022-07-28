using DCMS.Core;
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace DCMS.ViewModel.Models.Finances
{

    public partial class AdvanceReceiptBillListModel : BaseModel
    {
        public AdvanceReceiptBillListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            DynamicColumns = new List<string>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<AdvanceReceiptBillModel> Items { get; set; }
        public List<string> DynamicColumns { get; set; }


        [HintDisplayName("客户", "客户")]
        public int CustomerId { get; set; } = 0;
        public string CustomerName { get; set; }


        [HintDisplayName("收款人", "收款人")]
        public int? Payeer { get; set; } = 0;
        [HintDisplayName("收款人", "收款人")]
        public string PayeerName { get; set; }
        public SelectList Payeers { get; set; }



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


        [HintDisplayName("预收款账户", "预收款账户")]
        public int? AccountingOptionId { get; set; } = 0;
        [HintDisplayName("预收款账户", "预收款账户")]
        public string AccountingOptionName { get; set; }
        public SelectList AccountingOptions { get; set; }

    }


    /// <summary>
    /// 用于表示预收款单据
    /// </summary>
    public class AdvanceReceiptBillModel : BaseEntityModel
    {
        public AdvanceReceiptBillModel()
        {
            Items = new List<AdvanceReceiptBillAccountingModel>();
        }

        public int BillTypeEnumId { get; set; }


        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }
        public string BillBarCode { get; set; }



        [HintDisplayName("客户", "客户")]
        public int CustomerId { get; set; } = 0;
        public string CustomerName { get; set; }


        [HintDisplayName("收款人", "收款人")]
        public int? Payeer { get; set; } = 0;
        [HintDisplayName("收款人", "收款人")]
        public string PayeerName { get; set; }
        public SelectList Payeers { get; set; }

        [HintDisplayName("收款日期", "收款日期")]
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

        [HintDisplayName("上交状态", "上交状态")]
        public bool? HandInStatus { get; set; }

        [HintDisplayName("上交时间", "上交时间")]
        public DateTime? HandInDate { get; set; }

        [HintDisplayName("预收款账户", "预收款账户")]
        public int? AccountingOptionId { get; set; } = 0;
        public string AccountingOptionName { get; set; }
        public SelectList AccountingOptions { get; set; }



        [HintDisplayName("预收款金额", "预收款金额")]
        public decimal? AdvanceAmount { get; set; } = 0;


        [HintDisplayName("优惠金额", "优惠金额")]
        public decimal? DiscountAmount { get; set; } = 0;


        [HintDisplayName("打印数", "打印数")]
        public int? PrintNum { get; set; } = 0;
        [HintDisplayName("数据来源", "数据来源")]
        public int Operation { get; set; } = 0;


        [HintDisplayName("欠款金额", "欠款金额")]
        public decimal? OweCash { get; set; } = 0;

        [HintDisplayName("未回款金", "未回款金")]
        public decimal? OutstandingPayment { get; set; } = 0;

        /// <summary>
        /// 收款账户
        /// </summary>
        public IList<AdvanceReceiptBillAccountingModel> Items { get; set; }


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
    public class AdvanceReceiptBillAccountingModel : BaseAccountModel
    {
        public string Name { get; set; }

        /// <summary>
        /// 是否默认
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// 副本
        /// </summary>
        public bool Copy { get; set; }

    }



    public class AdvanceReceiptUpdateModel : BaseEntityModel
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
        /// 欠款金额
        /// </summary>
        public decimal OweCash { get; set; } = 0;

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 预收款账户
        /// </summary>
        public int AccountingOptionId { get; set; } = 0;

        /// <summary>
        /// 操作源
        /// </summary>
        public int? Operation { get; set; }

        /// <summary>
        /// 收款账户
        /// </summary>
        public List<AdvanceReceiptBillAccountingModel> Accounting { get; set; }

    }
}
