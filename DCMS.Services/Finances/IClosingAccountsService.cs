using DCMS.Core;
using DCMS.Core.Domain.Finances;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Finances
{
    public interface IClosingAccountsService
    {
        CostPriceSummery CostCostPriceSummery(int id);
        CostPriceChangeRecords CostPriceChangeRecords(int id);
        bool CheckClosingAccounts(int? storeId, DateTime? date = null);
        void DeleteClosingAccounts(ClosingAccounts closingAccounts);
        void DeleteCostPriceChangeRecords(CostPriceChangeRecords costPriceChangeRecords);
        void DeleteCostPriceSummery(CostPriceSummery costPriceSummery);
        IList<CostPriceSummery> GetCostPriceSummeriesByProductId(int? storeId, DateTime period, int productId);
        CostPriceSummery GetCostPriceSummeriesByProductId(int? storeId, DateTime period, int productId, int unitId);
        IList<CostPriceSummery> GetCostPriceSummeries(int? storeId, DateTime period);
        IList<CostPriceSummery> GetLastCostPriceSummeries(int? storeId, DateTime period);
        void DeleteCostPriceSummery(int? storeId, DateTime period);
        IList<ClosingAccounts> GetAll(int? storeId);
        IList<CostPriceChangeRecords> GetChangeRecords(int? storeId);
        IPagedList<ClosingAccounts> GetClosingAccounts(int? storeId, DateTime? date = null, int pageIndex = 0, int pageSize = int.MaxValue);
        ClosingAccounts GetClosingAccountsById(int id);
        IPagedList<CostPriceChangeRecords> GetCostPriceChangeRecordss(int? storeId, int? costPriceSummeryId = 0, DateTime? date = null, int pageIndex = 0, int pageSize = int.MaxValue);
        CostPriceChangeRecords CostPriceChangeRecords(int storeId, DateTime createdOnUtc, int billTypeId, int billId, int productId, int unitId);
        IList<CostPriceSummery> GetCostPriceSummeries(int? storeId);
        IPagedList<CostPriceSummery> GetCostPriceSummeries(int? storeId, int? productId = 0, string productName = "", DateTime? date = null, int pageIndex = 0, int pageSize = int.MaxValue);
        void InsertClosingAccounts(ClosingAccounts closingAccounts);
        void InsertCostPriceChangeRecords(CostPriceChangeRecords costPriceChangeRecords);
        void InsertCostPriceChangeRecords(List<CostPriceChangeRecords> costPriceChangeRecords);
        void InsertCostPriceSummery(CostPriceSummery costPriceSummery);
        void UpdateClosingAccounts(ClosingAccounts closingAccounts);
        void UpdateCostPriceChangeRecords(CostPriceChangeRecords costPriceChangeRecords);
        void UpdateCostPriceSummery(CostPriceSummery costPriceSummery);
        bool IsClosed(int? storeId, DateTime? date = null);
        bool IsLocked(int? storeId, DateTime? date = null);
        ClosingAccounts GetClosingAccountsByPeriod(int? storeId, DateTime? date = null);
        ClosingAccounts ComputeLastPeriod(int? storeId, ClosingAccounts period = null);
        ClosingAccounts ComputeNextPeriod(int? storeId, ClosingAccounts period = null);

        IList<CostPriceSummery> ExportCostPriceSummery(int? storeId, int? productId = 0, string productName = "", DateTime? date = null);
        IList<CostPriceChangeRecords> ExportGetCostPriceChangeRecordss(int? storeId, int? costPriceSummeryId = 0, DateTime? date = null);
    }
}