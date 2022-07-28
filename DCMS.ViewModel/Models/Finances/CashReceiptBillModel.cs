using DCMS.Core;
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DCMS.ViewModel.Models.Finances
{


    /// <summary>
    /// 用于表示收款单据列表
    /// </summary>
    public partial class CashReceiptBillListModel : BaseModel
    {
        public CashReceiptBillListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Lists = new List<CashReceiptBillModel>();
            DynamicColumns = new List<string>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<CashReceiptBillModel> Lists { get; set; }
        public List<string> DynamicColumns { get; set; }


        [HintDisplayName("收款人", "收款人")]
        public int? Payeer { get; set; } = 0;
        [HintDisplayName("收款人", "收款人")]
        public string PayeerName { get; set; }
        public SelectList Payeers { get; set; }



        [HintDisplayName("客户", "客户")]
        public int CustomerId { get; set; } = 0;
        public string CustomerName { get; set; }


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


        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }

    }

    /// <summary>
    /// 用于表示收款单据
    /// </summary>
    //[Validator(typeof(CashReceiptBillValidator))]
    public class CashReceiptBillModel : BaseEntityModel
    {

        public CashReceiptBillModel()
        {
            Items = new List<CashReceiptItemModel>();
            CashReceiptBillAccountings = new List<CashReceiptBillAccountingModel>();
        }

        public int BillTypeEnumId { get; set; }

        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }
        public string BillBarCode { get; set; }



        [HintDisplayName("收款人", "收款人")]
        public int? Payeer { get; set; } = 0;
        [HintDisplayName("收款人", "收款人")]
        public string PayeerName { get; set; }
        public SelectList Payeers { get; set; }


        [HintDisplayName("客户", "客户")]
        public int CustomerId { get; set; } = 0;
        [HintDisplayName("客户", "客户")]
        public string CustomerName { get; set; }
        [HintDisplayName("终端编号", "终端编号")]
        public string CustomerPointCode { get; set; }


        [HintDisplayName("开单日期", "开单日期")]
        public DateTime? CreatedOnUtc { get; set; }


        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }


        [HintDisplayName("制单人", "制单人")]
        public int MakeUserId { get; set; } = 0;
        [HintDisplayName("制单人", "制单人")]
        public string MakeUserName { get; set; }

        [HintDisplayName("状态(审核)", "状态(审核)")]
        public bool AuditedStatus { get; set; }


        [HintDisplayName("审核时间", "审核时间")]
        public DateTime? AuditedDate { get; set; }


        [HintDisplayName("审核人", "审核人")]
        public int AuditedUserId { get; set; } = 0;
        [HintDisplayName("审核人", "审核人")]
        public string AuditedUserName { get; set; }

        [HintDisplayName("红冲状态", "红冲状态")]
        public bool ReversedStatus { get; set; }

        [HintDisplayName("红冲时间", "红冲时间")]
        public DateTime? ReversedDate { get; set; }

        [HintDisplayName("上交状态", "上交状态")]
        public bool? HandInStatus { get; set; }

        [HintDisplayName("上交时间", "上交时间")]
        public DateTime? HandInDate { get; set; }

        //[HintDisplayName("总优惠金额", "总优惠金额(本次优惠金额总和)")]
        //public decimal? TotalDiscountAmount { get; set; } = 0;


        [HintDisplayName("现金", "默认收款账户")]
        public int CollectionAccount { get; set; } = 0;
        [HintDisplayName("收款金额", "默认收款金额")]
        public decimal? CollectionAmount { get; set; } = 0;


        //[HintDisplayName("剩余金额", "剩余金额(收款后尚欠金额总和)")]
        //public decimal? TotalAmountOwedAfterReceipt { get; set; } = 0;



        [HintDisplayName("单据类型", "单据类型")]
        public int? BillTypeId { get; set; }
        public SelectList BillTypes { get; set; }


        [HintDisplayName("开始日期", "开始日期")]
        [UIHint("DateTime")]
        public DateTime StartTime { get; set; }

        [HintDisplayName("截止日期", "截止日期")]
        [UIHint("DateTime")]
        public DateTime EndTime { get; set; }


        [HintDisplayName("打印数", "打印数")]
        public int? PrintNum { get; set; } = 0;
        [HintDisplayName("数据来源", "数据来源")]
        public int Operation { get; set; } = 0;

        /// <summary>
        /// 欠款
        /// </summary>
        public decimal OweCash { get; set; }

        /// <summary>
        /// 应收
        /// </summary>
        public decimal ReceivableAmount { get; set; }

        /// <summary>
        /// 优惠
        /// </summary>
        public decimal PreferentialAmount { get; set; }

        /// <summary>
        /// 项目
        /// </summary>
        public IList<CashReceiptItemModel> Items { get; set; }

        #region //以下用于隐藏域

        /// <summary>
        /// 单据总金额
        /// </summary>
        public decimal? TotalAmount { get; set; } = 0;


        /// <summary>
        /// 单据当前总优惠金额
        /// </summary>
        public decimal? TotalSubDiscountAmount { get; set; } = 0;


        /// <summary>
        /// 单据当前总已收金额
        /// </summary>
        public decimal? TotalPaymentedAmount { get; set; } = 0;


        /// <summary>
        /// 单据当前总欠款金额
        /// </summary>
        public decimal? TotalTotalArrearsAmount { get; set; } = 0;


        /// <summary>
        /// 单据当前总本次优惠金额
        /// </summary>
        public decimal? TotalDiscountAmountOnce { get; set; } = 0;

        /// <summary>
        /// 单据当前总本次收款金额
        /// </summary>
        public decimal? TotalReceivableAmountOnce { get; set; } = 0;

        /// <summary>
        /// 单据当前总收款后尚欠金额
        /// </summary>
        public decimal? TotalSubAmountOwedAfterReceipt { get; set; } = 0;

        #endregion


        /// <summary>
        /// 收款前欠款金额
        /// </summary>
        public decimal? BeforArrearsAmount { get; set; } = 0;

        /// <summary>
        /// 收款账户
        /// </summary>
        public IList<CashReceiptBillAccountingModel> CashReceiptBillAccountings { get; set; }

        /// <summary>
        /// 允许预收款支付成负数
        /// </summary>
        public bool AllowAdvancePaymentsNegative { get; set; }
    }


    /// <summary>
    /// 用于表示收款单据项目
    /// </summary>
    public class CashReceiptItemModel : BaseEntityModel
    {
        public int BillId { get; set; } = 0;

        [HintDisplayName("收款单", "收款单Id")]
        public int CashReceiptBillId { get; set; } = 0;


        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }

        [HintDisplayName("单据类型", "单据类型")]
        public int BillTypeId { get; set; } = 0;
        public string BillTypeName { get; set; }
        public BillTypeEnum BillTypeEnum
        {
            get { return (BillTypeEnum)BillTypeId; }
            set { BillTypeId = (int)value; }
        }
        public string BillLink { get; set; }


        [HintDisplayName("开单日期", "开单日期(这里是单据的开出时间)")]
        public DateTime MakeBillDate { get; set; }


        [HintDisplayName("单据金额", "单据金额")]
        public decimal? Amount { get; set; } = 0;

        [HintDisplayName("优惠金额", "优惠金额")]
        public decimal? DiscountAmount { get; set; } = 0;


        [HintDisplayName("已收金额", "已收金额")]
        public decimal? PaymentedAmount { get; set; } = 0;


        [HintDisplayName("欠款金额", "欠款金额")]
        public decimal? ArrearsAmount { get; set; } = 0;


        [HintDisplayName("本次优惠金额", "本次优惠金额")]
        public decimal? DiscountAmountOnce { get; set; } = 0;


        [HintDisplayName("本次收款金额", "本次收款金额")]
        public decimal? ReceivableAmountOnce { get; set; } = 0;

        [HintDisplayName("收款后尚欠金额", "收款后尚欠金额")]
        public decimal? AmountOwedAfterReceipt { get; set; } = 0;


        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }


        [HintDisplayName("创建日期", "创建日期")]
        public DateTime CreatedOnUtc { get; set; }

    }


    /// <summary>
    ///  收款账户（销售单科目映射表）
    /// </summary>
    public class CashReceiptBillAccountingModel : BaseAccountModel
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
    public class CashReceiptUpdateModel : BaseBalance
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
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 操作源
        /// </summary>
        public int? Operation { get; set; }

        /// <summary>
        /// 欠款
        /// </summary>
        public decimal OweCash { get; set; }

        /// <summary>
        /// 应收
        /// </summary>
        public decimal ReceivableAmount { get; set; }

        /// <summary>
        /// 优惠
        /// </summary>
        public decimal PreferentialAmount { get; set; }

        /// <summary>
        /// 项目
        /// </summary>
        public List<CashReceiptItemModel> Items { get; set; }

        /// <summary>
        /// 收款账户
        /// </summary>
        public List<CashReceiptBillAccountingModel> Accounting { get; set; }

    }




}
