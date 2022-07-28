using DCMS.Core;
using DCMS.Core.Domain.Finances;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Finances
{
    public interface ITrialBalanceService
    {
        IList<TrialBalance> GetAll(int? storeId);
        TrialBalance GetTrialBalanceById(int id);
        TrialBalance FindTrialBalance(int? storeId, int accountingTypeId, int accountingOptionId, DateTime? PeriodDate);
        IPagedList<TrialBalance> GetTrialBalances(int? storeId, int? accountOptionId = 0, int? accountTypeId = 0, DateTime? periodTime = null, int pageIndex = 0, int pageSize = int.MaxValue);
        IList<TrialBalance> GetTrialBalanceByIds(int[] idArr);
        void InsertTrialBalance(TrialBalance trialBalance);
        void InsertTrialBalances(IList<TrialBalance> trialBalances);
        void DeleteTrialBalance(TrialBalance trialBalance);
        void UpdateTrialBalance(TrialBalance trialBalance);
        void UpdateTrialBalances(IList<TrialBalance> trialBalances);
        TrialBalance GetTrialBalance(int store, int accountingTypeId, int accountingOptionId, DateTime periodDate);
        IList<TrialBalance> GetTrialBalances(int? store, int? AccountOptionId, DateTime? recordTime);
        IList<TrialBalance> TryGetTrialBalances(int? storeId, DateTime? period);
        bool HasChilds(int accountingOptionId);
        TrialBalance GetYearBegainTrialBalance(int? store, int accountingOptionId, DateTime? periodDate);
    }
}
