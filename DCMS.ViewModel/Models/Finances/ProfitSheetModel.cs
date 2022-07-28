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
    /// 用于表示利润表
    /// </summary>
    public class ProfitSheetListModel
    {
        /// <summary>
        /// 收入
        /// </summary>
        public ProfitSheetTotalModel IncomeTrees { get; set; } = new ProfitSheetTotalModel();

        /// <summary>
        /// 支出
        /// </summary>
        public ProfitSheetTotalModel ExpenditureTrees { get; set; } = new ProfitSheetTotalModel();

        public decimal? PretaxAccumulatedAmount { get; set; } = 0;
        public decimal? PretaxCurrentAmount { get; set; } = 0;

        public string RecordTime { get; set; }
        public SelectList Dates { get; set; }
    }


    public class ProfitSheetTotalModel
    {
        public decimal? TotalAccumulatedAmountOfYear { get; set; } = 0;
        public decimal? TotalCurrentAmount { get; set; } = 0;
        public List<AccountingOptionTree<ProfitSheet>> Trees { get; set; } = new List<AccountingOptionTree<ProfitSheet>>();

    }

    /// <summary>
    /// 用于表示利润表
    /// </summary>
    public class ProfitSheetModel : BaseEntityModel
    {

        [HintDisplayName("科目类别", "科目类别")]
        public int AccountingTypeId { get; set; } = 0;
        public string AccountingTypeName { get; set; }


        [HintDisplayName("财务科目", "财务科目")]
        public int AccountingOptionId { get; set; } = 0;
        public string AccountingOptionName { get; set; }
        public string AccountingOptionCode { get; set; }

        [HintDisplayName("结转期数", "结转期数（短日期）")]
        public DateTime PeriodDate { get; set; }


        [HintDisplayName("本年累计金额", "本年累计金额")]
        public decimal? AccumulatedAmountOfYear { get; set; } = 0;


        [HintDisplayName("本期金额", "本期金额")]
        public decimal? CurrentAmount { get; set; } = 0;


        [HintDisplayName("创建日期", "创建日期")]
        public DateTime CreatedOnUtc { get; set; }
    }


}