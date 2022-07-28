using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Finances;
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;



namespace DCMS.ViewModel.Models.Finances
{

    /// <summary>
    /// 用于表示科目余额(试算表)
    /// </summary>
    public class TrialBalanceListModel
    {
        public List<AccountingOptionTree<TrialBalance>> Trees { get; set; } = new List<AccountingOptionTree<TrialBalance>>();

        [HintDisplayName("总期初余额(借方)", "总期初余额(借方)")]
        public decimal? TotalInitialBalanceDebit { get; set; } = 0;


        [HintDisplayName("总期初余额(贷方)", "总期初余额(贷方)")]
        public decimal? TotalInitialBalanceCredit { get; set; } = 0;



        [HintDisplayName("总本期发生额(借方)", "总本期发生额(借方)")]
        public decimal? TotalPeriodBalanceDebit { get; set; } = 0;


        [HintDisplayName("总本期发生额(贷方)", "总本期发生额(贷方)")]
        public decimal? TotalPeriodBalanceCredit { get; set; } = 0;



        [HintDisplayName("总期末余额(借方)", "总期末余额(借方)")]
        public decimal? TotalEndBalanceDebit { get; set; } = 0;


        [HintDisplayName("总期末余额(贷方)", "总期末余额(贷方)")]
        public decimal? TotalEndBalanceCredit { get; set; } = 0;

        public string RecordTime { get; set; }
        public SelectList Dates { get; set; }

    }

    /// <summary>
    /// 用于表示科目余额(试算表)
    /// </summary>
    public class TrialBalanceModel : BaseEntityModel
    {

        [HintDisplayName("科目类别", "科目类别")]
        public int AccountingTypeId { get; set; } = 0;
        public string AccountingTypeName { get; set; }


        [HintDisplayName("财务科目", "财务科目")]
        public int AccountingOptionId { get; set; } = 0;
        public string AccountingOptionName { get; set; }
        public string AccountingOptionCode { get; set; }

        [HintDisplayName("结转期数（短日期）", "结转期数（短日期）")]
        public DateTime PeriodDate { get; set; }


        [HintDisplayName("期初余额(借方)", "期初余额(借方)")]
        public decimal? InitialBalanceDebit { get; set; } = 0;


        [HintDisplayName("期初余额(贷方)", "期初余额(贷方)")]
        public decimal? InitialBalanceCredit { get; set; } = 0;



        [HintDisplayName("本期发生额(借方)", "本期发生额(借方)")]
        public decimal? PeriodBalanceDebit { get; set; } = 0;


        [HintDisplayName("本期发生额(贷方)", "本期发生额(贷方)")]
        public decimal? PeriodBalanceCredit { get; set; } = 0;



        [HintDisplayName("期末余额(借方)", "期末余额(借方)")]
        public decimal? EndBalanceDebit { get; set; } = 0;


        [HintDisplayName("期末余额(贷方)", "期末余额(贷方)")]
        public decimal? EndBalanceCredit { get; set; } = 0;


        [HintDisplayName("创建日期", "创建日期")]
        public DateTime CreatedOnUtc { get; set; }

    }

}