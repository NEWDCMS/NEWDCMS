using DCMS.Core.Domain.Report;
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DCMS.ViewModel.Models.Report
{

    #region 客户往来账
    /// <summary>
    /// 客户往来账
    /// </summary>
    public class FundReportCustomerAccountListModel : BaseModel
    {
        public FundReportCustomerAccountListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<CustomerAccountDealings>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<CustomerAccountDealings> Items { get; set; }

        #region  用于条件检索

        [HintDisplayName("客户片区", "客户片区")]
        public int? DistrictId { get; set; } = 0;
        public SelectList Districts { get; set; }

        [HintDisplayName("客户渠道", "客户渠道")]
        public int? ChannelId { get; set; } = 0;
        public SelectList Channels { get; set; }

        [HintDisplayName("客户Id", "客户Id")]
        public int? TerminalId { get; set; } = 0;
        [HintDisplayName("客户", "客户")]
        public string TerminalName { get; set; }

        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }

        [HintDisplayName("单据类型", "单据类型")]
        public int? BillTypeId { get; set; } = 0;
        public string BillTypeName { get; set; }
        public SelectList BillTypes { get; set; }

        [DisplayName("开始时间")]

        public DateTime? StartTime { get; set; }

        [DisplayName("结束时间")]

        public DateTime? EndTime { get; set; }

        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }

        #endregion


    }
    #endregion

    #region 客户应收款
    /// <summary>
    /// 客户应收款
    /// </summary>
    public class FundReportCustomerReceiptCashListModel : BaseModel
    {
        public FundReportCustomerReceiptCashListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<FundReportCustomerReceiptCash>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<FundReportCustomerReceiptCash> Items { get; set; }

        #region  用于条件检索

        [HintDisplayName("客户渠道", "客户渠道")]
        public int? ChannelId { get; set; } = 0;
        public SelectList Channels { get; set; }

        [HintDisplayName("业务员", "业务员")]
        public int? BusinessUserId { get; set; } = 0;
        public SelectList BusinessUsers { get; set; }

        [HintDisplayName("客户片区", "客户片区")]
        public int? DistrictId { get; set; } = 0;
        public SelectList Districts { get; set; }

        [HintDisplayName("客户Id", "客户Id")]
        public int? TerminalId { get; set; } = 0;
        [HintDisplayName("客户", "客户")]
        public string TerminalName { get; set; }

        [HintDisplayName("账期大于", "账期大于")]
        public int? MoreDay { get; set; } = 0;

        [DisplayName("开始时间")]

        public DateTime? StartTime { get; set; }

        [DisplayName("结束时间")]

        public DateTime? EndTime { get; set; }

        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }

        #endregion

    }
    #endregion

    #region 供应商往来账
    /// <summary>
    /// 供应商往来账
    /// </summary>
    public class FundReportManufacturerAccountListModel : BaseModel
    {
        public FundReportManufacturerAccountListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<FundReportManufacturerAccount>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<FundReportManufacturerAccount> Items { get; set; }

        #region  用于条件检索

        [HintDisplayName("单据编号", "单据编号")]
        public string BillNumber { get; set; }

        [HintDisplayName("单据类型", "单据类型")]
        public int? BillTypeId { get; set; } = 0;
        public string BillTypeName { get; set; }
        public SelectList BillTypes { get; set; }

        [HintDisplayName("供应商", "供应商")]
        public int? ManufacturerId { get; set; } = 0;
        public SelectList Manufacturers { get; set; }

        [HintDisplayName("备注", "备注")]
        public string Remark { get; set; }

        [DisplayName("开始时间")]

        public DateTime? StartTime { get; set; }

        [DisplayName("结束时间")]

        public DateTime? EndTime { get; set; }

        #endregion

    }
    #endregion

    #region 供应商应付款
    /// <summary>
    /// 供应商应付款
    /// </summary>
    public class FundReportManufacturerPayCashListModel : BaseModel
    {
        public FundReportManufacturerPayCashListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<FundReportManufacturerPayCash>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<FundReportManufacturerPayCash> Items { get; set; }

        #region  用于条件检索

        [HintDisplayName("业务员", "业务员")]
        public int? BusinessUserId { get; set; } = 0;
        public SelectList BusinessUsers { get; set; }

        [HintDisplayName("账期大于", "账期大于")]
        public int? MoreDay { get; set; } = 0;

        [DisplayName("开始时间")]

        public DateTime? StartTime { get; set; }

        [DisplayName("结束时间")]

        public DateTime? EndTime { get; set; }
        #endregion

        #region 小计、合计

        /// <summary>
        /// 累计欠款（总）
        /// </summary>
        public decimal? TotalSumOweCase { get; set; } = 0;

        #endregion

    }
    #endregion

    #region 预收款余额
    /// <summary>
    /// 预收款余额
    /// </summary>
    public class FundReportAdvanceReceiptOverageListModel : BaseModel
    {
        public FundReportAdvanceReceiptOverageListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<FundReportAdvanceReceiptOverage>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<FundReportAdvanceReceiptOverage> Items { get; set; }
        public List<DCMS.Core.Domain.Configuration.AccountingOption> DynamicColumns { get; set; } = new List<DCMS.Core.Domain.Configuration.AccountingOption>();
        //public List<string> DynamicColumns { get; set; }


        #region  用于条件检索

        [HintDisplayName("客户Id", "客户Id")]
        public int? TerminalId { get; set; } = 0;
        [HintDisplayName("客户", "客户")]
        public string TerminalName { get; set; }

        #endregion

    }
    #endregion

    #region 预付款余额
    /// <summary>
    /// 预付款余额
    /// </summary>
    public class FundReportAdvancePaymentOverageListModel : BaseModel
    {
        public FundReportAdvancePaymentOverageListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
            Items = new List<FundReportAdvancePaymentOverage>();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<FundReportAdvancePaymentOverage> Items { get; set; }
        public List<DCMS.Core.Domain.Configuration.AccountingOption> DynamicColumns { get; set; } = new List<DCMS.Core.Domain.Configuration.AccountingOption>();
        #region  用于条件检索

        [HintDisplayName("供应商", "供应商")]
        public int? ManufacturerId { get; set; } = 0;
        public SelectList Manufacturers { get; set; }
        [DisplayName("结束时间")]

        public DateTime? EndTime { get; set; }
        #endregion

    }
    #endregion
}