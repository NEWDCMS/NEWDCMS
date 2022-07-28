using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DCMS.ViewModel.Models.Finances
{

    public partial class AdvancePaymentBillListModel : BaseModel
    {
        public AdvancePaymentBillListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            DynamicColumns = new List<string>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<AdvancePaymentBillModel> Items { get; set; }
        public List<string> DynamicColumns { get; set; }



        [HintDisplayName("付款人", "付款人")]
        public int? Draweer { get; set; } = 0;
        public string DraweerName { get; set; }
        public SelectList Draweers { get; set; }


        [HintDisplayName("供应商", "供应商")]
        public int? ManufacturerId { get; set; } = 0;
        public string ManufacturerName { get; set; }
        public SelectList Manufacturers { get; set; }



        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }
        public string BillBarCode { get; set; }


        [HintDisplayName("状态(审核)", "状态(审核)")]
        public bool? AuditedStatus { get; set; }


        [HintDisplayName("打印状态", "打印状态")]
        public bool PrintStatus { get; set; }


        [HintDisplayName("开始日期", "开始日期")]
        public DateTime? StartTime { get; set; }


        [HintDisplayName("截止日期", "截止日期")]
        public DateTime? EndTime { get; set; }


        [HintDisplayName(" 显示红冲的数据", " 显示红冲的数据")]
        public bool? ShowReverse { get; set; }


        [HintDisplayName("按审核时间", " 按审核时间")]
        public bool? SortByAuditedTime { get; set; }


        [HintDisplayName("预付款账户", "预收款账户")]
        public int? AccountingOptionId { get; set; } = 0;
        [HintDisplayName("预付款账户", "预收款账户")]
        public string AccountingOptionName { get; set; }
        public SelectList AccountingOptions { get; set; }


        [HintDisplayName("预付款金额", "预收款金额")]
        public decimal? AdvanceAmount { get; set; } = 0;


        [HintDisplayName("优惠金额", "优惠金额")]
        public decimal? DiscountAmount { get; set; }


        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }
    }


    /// <summary>
    /// 用于表示预付款单据
    /// </summary>
    public class AdvancePaymentBillModel : BaseEntityModel
    {
        public AdvancePaymentBillModel()
        {
            Items = new List<AdvancePaymentBillAccountingModel>();
        }

        public int BillTypeEnumId { get; set; }

        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }
        public string BillBarCode { get; set; }

        [HintDisplayName("付款人", "付款人")]
        public int Draweer { get; set; } = 0;
        public string DraweerName { get; set; }
        public SelectList Draweers { get; set; }


        [HintDisplayName("供应商", "供应商")]
        public int ManufacturerId { get; set; } = 0;
        public string ManufacturerName { get; set; }
        public SelectList Manufacturers { get; set; }

        [HintDisplayName("付款日期", "付款日期")]
        [UIHint("DateTime")]
        public DateTime CreatedOnUtc { get; set; }


        [HintDisplayName("付款类型(预付款)", "付款类型(预付款)")]
        public int PaymentType { get; set; } = 0;


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


        [HintDisplayName("预付款账户", "预收款账户")]
        public int? AccountingOptionId { get; set; } = 0;
        public string AccountingOptionName { get; set; }
        public SelectList AccountingOptions { get; set; }


        [HintDisplayName("预付款金额", "预收款金额")]
        public decimal? AdvanceAmount { get; set; } = 0;


        [HintDisplayName("数据来源", "数据来源")]
        public int Operation { get; set; } = 0;

        /// <summary>
        /// 付款账户
        /// </summary>
        public IList<AdvancePaymentBillAccountingModel> Items { get; set; }

    }

    /// <summary>
    ///  预付款账户（预付款单据科目映射表）
    /// </summary>
    public class AdvancePaymentBillAccountingModel : BaseAccountModel
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

    /// <summary>
    /// 保存或者编辑
    /// </summary>
    public class AdvancePaymenUpdateModel : BaseEntityModel
    {
        public string BillNumber { get; set; }
        [HintDisplayName("付款人", "付款人")]
        public int Draweer { get; set; } = 0;


        [HintDisplayName("供应商", "供应商")]
        public int ManufacturerId { get; set; } = 0;


        [HintDisplayName("预付款金额", "预付款金额")]
        public decimal? AdvanceAmount { get; set; } = 0;


        public int PaymentType { get; set; } = 0;

        [HintDisplayName("备注", "备注")]
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
        /// 付款账户
        /// </summary>
        public List<AdvancePaymentBillAccountingModel> Accounting { get; set; }

    }

}
