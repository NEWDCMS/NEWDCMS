using DCMS.Core.Domain.Configuration;
using System;
using System.Collections.Generic;

namespace DCMS.Core.Domain.Finances
{

    /// <summary>
    /// 用于表示利润表
    /// </summary>
    public class ProfitSheet : BaseAccount
    {

        /// <summary>
        /// 行次:此处行次并不是出报表的实际的行数,只是显示用的用来符合国人习惯
        /// </summary>
        public int LineNum { get; set; }


        /// <summary>
        /// 结转期数（短日期）
        /// </summary>
        public DateTime PeriodDate { get; set; }


        /// <summary>
        /// 本年累计金额
        /// </summary>
        public decimal? AccumulatedAmountOfYear { get; set; }

        /// <summary>
        /// 本期金额
        /// </summary>
        public decimal? CurrentAmount { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }
    }

    /// <summary>
    /// 标示利润树
    /// </summary>
    public class ProfitSheetTree
    {
        public AccountingOption AccountingOption { get; set; }
        public ProfitSheet ProfitSheet { get; set; }
        public int AccountingType { get; set; }
        public List<ProfitSheetTree> Children { get; set; }
    }

    /// <summary>
    /// 用于表示利润表
    /// </summary>
    public class ProfitSheetExport : BaseEntity
    {

        /// <summary>
        /// 科目类别
        /// </summary>
        public int AccountingTypeId { get; set; } = 0;
        public string AccountingTypeName { get; set; }

        /// <summary>
        /// 财务科目
        /// </summary>
        public int AccountingOptionId { get; set; } = 0;
        public string AccountingOptionName { get; set; }
        public string AccountingOptionCode { get; set; }

        /// <summary>
        /// 结转期数（短日期）
        /// </summary>
        public DateTime PeriodDate { get; set; }

        /// <summary>
        /// 本年累计金额
        /// </summary>
        public decimal? AccumulatedAmountOfYear { get; set; } = 0;

        /// <summary>
        /// 本期金额
        /// </summary>
        public decimal? CurrentAmount { get; set; } = 0;

        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }
    }

}
