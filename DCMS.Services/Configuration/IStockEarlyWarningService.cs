using DCMS.Core;
using DCMS.Core.Domain.Configuration;
using System.Collections.Generic;

namespace DCMS.Services.Configuration
{
    public interface IStockEarlyWarningService
    {
        void DeleteStockEarlyWarning(StockEarlyWarning stockEarlyWarnings);
        IList<StockEarlyWarning> GetAllStockEarlyWarnings(int? store);
        IPagedList<StockEarlyWarning> GetAllStockEarlyWarnings(int? store, string name = null, int pageIndex = 0, int pageSize = int.MaxValue);
        StockEarlyWarning GetStockEarlyWarningById(int? store, int stockEarlyWarningsId);
        IList<StockEarlyWarning> GetStockEarlyWarningsByIds(int[] sIds);
        void InsertStockEarlyWarning(StockEarlyWarning stockEarlyWarnings);
        void UpdateStockEarlyWarning(StockEarlyWarning stockEarlyWarnings);
        bool CheckExists(int productId, int wareHouseId);

        bool CheckExists(int stockEarlyWarningsId, int productId, int wareHouseId);

    }
}