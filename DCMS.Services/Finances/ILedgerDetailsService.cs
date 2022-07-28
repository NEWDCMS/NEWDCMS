using DCMS.Core.Domain.Finances;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Finances
{
    public interface ILedgerDetailsService
    {
        /// <summary>
        /// 获取明细账
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="accountsIds"></param>
        /// <param name="first"></param>
        /// <param name="last"></param>
        /// <returns></returns>
        IList<VoucherItem> GetLedgerDetailsByOptions(int? storeId, int[] accountsIds, DateTime? first, DateTime? last);
        /// <summary>
        /// 获取资产负债表
        /// </summary>
        /// <param name="store"></param>
        /// <param name="recordTime"></param>
        /// <returns></returns>
        IList<BalanceSheet> GetBalanceSheets(int? store, DateTime? recordTime);
        /// <summary>
        /// 获取本年利润
        /// </summary>
        /// <param name="store"></param>
        /// <param name="recordTime"></param>
        /// <returns></returns>
        IList<ProfitSheet> GetProfitSheets(int? store, DateTime? recordTime);

        void InsertBalanceSheets(IList<BalanceSheet> balanceSheets);
        void InsertBalanceSheet(BalanceSheet balanceSheet);
        void InsertProfitSheets(IList<ProfitSheet> profitSheets);
        void InsertProfitSheet(ProfitSheet profitSheet);
        void UpdateProfitSheet(ProfitSheet profitSheet);
        void UpdateBalanceSheet(BalanceSheet balanceSheet);
        BalanceSheet FindBalanceSheet(int? storeId, int accountingTypeId, int accountingOptionId, DateTime? PeriodDate);
        ProfitSheet FindProfitSheet(int? storeId, int accountingTypeId, int accountingOptionId, DateTime? PeriodDate);
    }
}
