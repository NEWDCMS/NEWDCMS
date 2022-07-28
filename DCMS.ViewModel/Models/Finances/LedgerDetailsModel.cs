using DCMS.Core.Domain.Configuration;
using DCMS.Core.Domain.Finances;
using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using System;
using System.Collections.Generic;

namespace DCMS.ViewModel.Models.Finances
{

    public class LedgerDetailsListModel
    {
        public List<AccountingOptionTree<LedgerDetails>> Trees { get; set; } = new List<AccountingOptionTree<LedgerDetails>>();

        [HintDisplayName("开始日期", "开始日期")]
        public DateTime StartTime { get; set; }


        [HintDisplayName("截止日期", "截止日期")]
        public DateTime EndTime { get; set; }


        /// <summary>
        /// 期初余额借方金额
        /// </summary>
        public decimal? StartBalanceDebitAmount { get; set; } = 0;
        /// <summary>
        /// 期初余额贷方金额
        /// </summary>
        public decimal? StartBalanceCreditAmount { get; set; } = 0;
        /// <summary>
        /// 余额
        /// </summary>
        public decimal? StartBalanceAmount { get; set; } = 0;
        /// <summary>
        /// 方向
        /// </summary>
        public string StartBalanceDirection { get; set; } = "";

        public List<GroupMonths> GroupMonths { get; set; } = new List<GroupMonths>();

    }


    public class GroupMonths : IEquatable<GroupMonths>
    {
        public int Year { get; set; } = 0;
        public int Month { get; set; } = 0;

        public DateTime LastDay { get; set; }

        /// <summary>
        /// 明细账
        /// </summary>
        public List<LedgerDetailsModel> Items { get; set; } = new List<LedgerDetailsModel>();

        /// <summary>
        /// 本期合计借方金额
        /// </summary>
        public decimal? CurBalanceDebitAmount { get; set; } = 0;
        /// <summary>
        /// 本期合计贷方金额
        /// </summary>
        public decimal? CurBalanceCreditAmount { get; set; } = 0;
        /// <summary>
        /// 本期合计余额
        /// </summary>
        public decimal? CurBalanceAmount { get; set; } = 0;
        /// <summary>
        /// 本期方向
        /// </summary>
        public string CurBalanceDirection { get; set; } = "";


        /// <summary>
        /// 本年合计借方金额
        /// </summary>
        public decimal? YearBalanceDebitAmount { get; set; } = 0;
        /// <summary>
        /// 本年合计贷方金额
        /// </summary>
        public decimal? YearBalanceCreditAmount { get; set; } = 0;
        /// <summary>
        /// 余额
        /// </summary>
        public decimal? YearBalanceAmount { get; set; } = 0;
        /// <summary>
        /// 方向
        /// </summary>
        public string YearBalanceDirection { get; set; } = "";



        #region override


        public override int GetHashCode()
        {
            return Year.GetHashCode() ^ Month.GetHashCode();
        }
        public bool Equals(GroupMonths other)
        {
            return Year == other.Year && Month == other.Month;
        }

        #endregion
    }


    /// <summary>
    /// 由于表示明细账
    /// </summary>
    public class LedgerDetailsModel : BaseEntityModel
    {

        [HintDisplayName("日期", "会计期间")]
        public DateTime RecordTime { get; set; }

        [HintDisplayName("记账凭证", "记账凭证")]
        public int RecordingVoucherId { get; set; } = 0;

        public string BillNumber { get; set; }
        public string BillLink { get; set; }

        [HintDisplayName("借贷方向", "会计术语,主要方向借、贷、平, 当借方金额大于贷方金额 方向为借\n\r，当贷方金额大于借方金额 方向为贷\n  借贷相等时 方向为平")]
        public string Direction { get; set; }

        [HintDisplayName("凭证字(记)", "凭证字(记)")]
        public string RecordName { get; set; }

        [HintDisplayName("凭证号", "凭证号")]
        public int RecordNumber { get; set; } = 0;

        [HintDisplayName("摘要", "从凭证中获取到对应的摘要")]
        public string Summary { get; set; }

        [HintDisplayName("会计科目", "会计科目")]
        public int AccountingOptionId { get; set; } = 0;
        public string AccountingOptionName { get; set; }

        [HintDisplayName("借方金额", "借方金额")]
        public decimal? DebitAmount { get; set; } = 0;

        [HintDisplayName("贷方金额", "贷方金额")]
        public decimal? CreditAmount { get; set; } = 0;

        [HintDisplayName("余额", "一般显示为正数，计算方式：当方向为借时 余额= 借方金额-贷方金额， 当方向为贷时 余额= 贷方金额-借方金额")]
        public decimal? Balances { get; set; } = 0;

        public List<VoucherItemModel> Items { get; set; } = new List<VoucherItemModel>();
    }

}