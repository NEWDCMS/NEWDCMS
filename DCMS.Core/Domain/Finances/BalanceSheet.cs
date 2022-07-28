using DCMS.Core.Domain.Configuration;
using System;
using System.Collections.Generic;


namespace DCMS.Core.Domain.Finances
{

    /// <summary>
    /// 用于表示资产负债表
    /// </summary>
    public class BalanceSheet : BaseAccount
    {

        /// <summary>
        /// 结转期数（短日期）
        /// </summary>
        public DateTime PeriodDate { get; set; }

        /// <summary>
        /// 行次:此处行次并不是出报表的实际的行数,只是显示用的用来符合国人习惯
        /// </summary>
        public int LineNum { get; set; }

        /// <summary>
        /// 年初余额
        /// </summary>
        public decimal? InitialBalance { get; set; }


        /// <summary>
        /// 期末余额
        /// </summary>
        public decimal? EndBalance { get; set; }


        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }
    }


    /// <summary>
    /// 标示资产负债树
    /// </summary>
    public class BalanceSheetTree
    {
        public AccountingOption AccountingOption { get; set; }
        public BalanceSheet BalanceSheet { get; set; }
        public int AccountingType { get; set; }
        public List<BalanceSheetTree> Children { get; set; }
    }

    /// <summary>
    /// 用于表示资产负债表
    /// </summary>
    public class BalanceSheetExport : BaseEntity
    {

        public int AccountingTypeId { get; set; } = 0;
        public string AccountingTypeName { get; set; }

        public int AccountingOptionId { get; set; } = 0;
        public string AccountingOptionName { get; set; }
        public string AccountingOptionCode { get; set; }

        /// <summary>
        /// 结转期数（短日期）
        /// </summary>
        public DateTime PeriodDate { get; set; }

        /// <summary>
        /// 年初余额
        /// </summary>
        public decimal? InitialBalance { get; set; } = 0;

        /// <summary>
        /// 期末余额
        /// </summary>
        public decimal? EndBalance { get; set; } = 0;

        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }
    }


}
