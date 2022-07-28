using DCMS.Core.Domain.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


namespace DCMS.Core.Domain.Finances
{
    /// <summary>
    /// 用于表示科目余额(试算表)
    /// </summary>
    public class TrialBalance : BaseAccount
    {
        /// <summary>
        /// 结转期数（短日期）
        /// </summary>
        public DateTime PeriodDate { get; set; }

        /// <summary>
        /// 期初余额(借方)
        /// </summary>
        public decimal? InitialBalanceDebit { get; set; }

        /// <summary>
        /// 期初余额(贷方)
        /// </summary>
        public decimal? InitialBalanceCredit { get; set; }


        /// <summary>
        /// 本期发生额(借方)
        /// </summary>
        public decimal? PeriodBalanceDebit { get; set; }

        /// <summary>
        /// 本期发生额(贷方)
        /// </summary>
        public decimal? PeriodBalanceCredit { get; set; }


        /// <summary>
        /// 期末余额(借方)
        /// </summary>
        public decimal? EndBalanceDebit { get; set; }

        /// <summary>
        /// 期末余额(贷方)
        /// </summary>
        public decimal? EndBalanceCredit { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// 根据科目的类型，判断余额方向是借方或者贷方
        /// </summary>
        [NotMapped]
        public DirectionsTypeEnum DirectionsType { get; set; }

        [NotMapped]
        public AccountingOption AccountingOption { get; set; }

    }


    /// <summary>
    /// 标示科目余额树
    /// </summary>
    public class TrialBalanceTree
    {
        public AccountingOption AccountingOption { get; set; }
        public TrialBalance TrialBalance { get; set; }
        public List<TrialBalanceTree> Children { get; set; }
    }


    /// <summary>
    /// 用于表示科目余额
    /// </summary>
    public class TrialBalanceExport : BaseEntity
    {

        public int AccountingTypeId { get; set; } = 0;

        public string AccountingTypeName { get; set; }

        public int AccountingOptionId { get; set; } = 0;

        public string AccountingOptionName { get; set; }

        public string AccountingOptionCode { get; set; }

        public DateTime PeriodDate { get; set; }

        public decimal? InitialBalanceDebit { get; set; } = 0;

        public decimal? InitialBalanceCredit { get; set; } = 0;

        public decimal? PeriodBalanceDebit { get; set; } = 0;

        public decimal? PeriodBalanceCredit { get; set; } = 0;

        public decimal? EndBalanceDebit { get; set; } = 0;

        public decimal? EndBalanceCredit { get; set; } = 0;

        public DateTime CreatedOnUtc { get; set; }

    }


    /// <summary>
    /// 用于表示明细分类账
    /// </summary>
    public class LedgerDetails : BaseAccount
    {

        /// <summary>
        /// 记账日期
        /// </summary>
        public DateTime RecordTime { get; set; }

        /// <summary>
        /// 记账凭证
        /// </summary>
        public int RecordingVoucherId { get; set; } = 0;

        /// <summary>
        /// 借贷方向：会计术语,主要方向借、贷、平, 当借方金额大于贷方金额 方向为借\n\，当贷方金额大于借方金额 方向为贷\n  借贷相等时 方向为平
        /// </summary>
        public string Direction { get; set; }

        /// <summary>
        /// 凭证字(记)
        /// </summary>
        public string RecordName { get; set; }

        /// <summary>
        /// 凭证号
        /// </summary>
        public int RecordNumber { get; set; } = 0;

        /// <summary>
        /// 摘要：从凭证中获取到对应的摘要
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// 借方金额
        /// </summary>
        public decimal? DebitAmount { get; set; } = 0;

        /// <summary>
        /// 贷方金额
        /// </summary>
        public decimal? CreditAmount { get; set; } = 0;

        /// <summary>
        /// 余额：一般显示为正数，计算方式：当方向为借时 余额= 借方金额-贷方金额， 当方向为贷时 余额= 贷方金额-借方金额
        /// </summary>
        public decimal? Balances { get; set; } = 0;

    }

}
