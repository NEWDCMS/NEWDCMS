using DCMS.Core;
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;


namespace DCMS.ViewModel.Models.Finances
{

    public partial class PaymentReceiptBillListModel : BaseModel
    {
        public PaymentReceiptBillListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            DynamicColumns = new List<string>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<PaymentReceiptBillModel> Items { get; set; }
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


        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }

    }

    /// <summary>
    /// 用于表示付款单据
    /// </summary>
    public class PaymentReceiptBillModel : BaseEntityModel
    {
        public PaymentReceiptBillModel()
        {
            PaymentReceiptBillAccountings = new List<PaymentReceiptBillAccountingModel>();
        }

        public int BillTypeEnumId { get; set; }

        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }
        public string BillBarCode { get; set; }


        [HintDisplayName("付款人", "付款人")]
        public int Draweer { get; set; } = 0;
        public SelectList Draweers { get; set; }
        public string DraweerName { get; set; }


        [HintDisplayName("供应商", "供应商")]
        public int ManufacturerId { get; set; } = 0;
        public string ManufacturerName { get; set; }


        [HintDisplayName("付款日期", "付款日期")]
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

        [HintDisplayName("优惠金额", "优惠金额")]
        public decimal? DiscountAmount { get; set; } = 0;


        [HintDisplayName("剩余金额", "剩余金额")]
        public decimal? AmountOwedAfterReceipt { get; set; } = 0;


        [HintDisplayName("打印数", "打印数")]
        public int? PrintNum { get; set; } = 0;


        [HintDisplayName("现金", "默认收款账户")]
        public int CollectionAccount { get; set; } = 0;
        [HintDisplayName("收款金额", "默认收款金额")]
        public decimal? CollectionAmount { get; set; } = 0;
        [HintDisplayName("数据来源", "数据来源")]
        public int Operation { get; set; } = 0;




        [HintDisplayName("单据类型", "单据类型")]
        public int? BillTypeId { get; set; }
        public SelectList BillTypes { get; set; }

        [HintDisplayName("开始日期", "开始日期")]
        //[UIHint("DateTime")]
        public DateTime StartTime { get; set; }

        [HintDisplayName("截止日期", "截止日期")]
        //[UIHint("DateTime")]
        public DateTime EndTime { get; set; }




        #region //以下用于隐藏域

        /// <summary>
        /// 单据总金额
        /// </summary>
        public decimal? TotalAmount { get; set; } = 0;


        /// <summary>
        /// 单据当前总优惠金额
        /// </summary>
        public decimal? TotalDiscountAmount { get; set; } = 0;


        /// <summary>
        /// 单据当前总已付金额
        /// </summary>
        public decimal? TotalPaymentedAmount { get; set; } = 0;


        /// <summary>
        /// 单据当前总欠款金额
        /// </summary>
        public decimal? TotalArrearsAmount { get; set; } = 0;


        /// <summary>
        /// 单据当前总本次优惠金额
        /// </summary>
        public decimal? TotalDiscountAmountOnce { get; set; } = 0;

        /// <summary>
        /// 单据当前总本次付款金额
        /// </summary>
        public decimal? TotalReceivableAmountOnce { get; set; } = 0;

        /// <summary>
        /// 单据当前总付款后尚欠金额
        /// </summary>
        public decimal? TotalAmountOwedAfterReceipt { get; set; } = 0;



        #endregion



        /// <summary>
        /// 项目
        /// </summary>
        public IList<PaymentReceiptItemModel> Items { get; set; } = new List<PaymentReceiptItemModel>();

        /// <summary>
        /// 付款前欠款金额
        /// </summary>
        public decimal? BeforArrearsAmount { get; set; } = 0;

        /// <summary>
        /// 付款账户
        /// </summary>
        public IList<PaymentReceiptBillAccountingModel> PaymentReceiptBillAccountings { get; set; }
    }


    /// <summary>
    /// 用于表示付款单据项目
    /// </summary>
    public class PaymentReceiptItemModel : BaseEntityModel
    {

        [HintDisplayName("收款单", "收款单Id")]
        public int PaymentReceiptBillId { get; set; } = 0;


        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }
        /// <summary>
        /// 单据编号
        /// </summary>
        public int BillId { get; set; }


        [HintDisplayName("单据类型", "单据类型")]
        public int BillTypeId { get; set; } = 0;
        public string BillTypeName { get; set; }
        public BillTypeEnum BillTypeEnum
        {
            get { return (BillTypeEnum)BillTypeId; }
            set { BillTypeId = (int)value; }
        }
        public string BillLink { get; set; }



        [HintDisplayName("开单日期", "开单日期")]
        public DateTime MakeBillDate { get; set; }

        [HintDisplayName("单据金额", "单据金额")]
        public decimal? Amount { get; set; } = 0;

        [HintDisplayName("优惠金额", "优惠金额")]
        public decimal? DiscountAmount { get; set; } = 0;

        [HintDisplayName("已付金额", "已付金额")]
        public decimal? PaymentedAmount { get; set; } = 0;


        [HintDisplayName("尚欠金额", "尚欠金额")]
        public decimal? ArrearsAmount { get; set; } = 0;

        [HintDisplayName("本次优惠金额", "本次优惠金额")]
        public decimal? DiscountAmountOnce { get; set; } = 0;


        [HintDisplayName("本次付款金额", "本次付款金额")]
        public decimal? ReceivableAmountOnce { get; set; } = 0;


        [HintDisplayName("付款后尚欠金额", "付款后尚欠金额")]
        public decimal? AmountOwedAfterReceipt { get; set; } = 0;

        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }

        [HintDisplayName("创建日期", "创建日期")]
        public DateTime CreatedOnUtc { get; set; }


    }


    /// <summary>
    ///  付款账户（付款单据科目映射表）
    /// </summary>
    public class PaymentReceiptBillAccountingModel : BaseAccountModel
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
    public class PaymentReceiptUpdateModel : BaseBalance
    {
        [HintDisplayName("付款人", "付款人")]
        public int Draweer { get; set; } = 0;


        [HintDisplayName("供应商", "供应商")]
        public int ManufacturerId { get; set; } = 0;


        [HintDisplayName("优惠金额", "优惠金额")]
        public decimal? DiscountAmount { get; set; } = 0;


        [HintDisplayName("剩余金额", "剩余金额")]
        public decimal? AmountOwedAfterReceipt { get; set; } = 0;


        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }

        /// <summary>
        /// 操作源
        /// </summary>
        public int? Operation { get; set; }

        /// <summary>
        /// 项目
        /// </summary>
        public List<PaymentReceiptItemModel> Items { get; set; }

        /// <summary>
        /// 收款账户
        /// </summary>
        public List<PaymentReceiptBillAccountingModel> Accounting { get; set; }

    }

}
