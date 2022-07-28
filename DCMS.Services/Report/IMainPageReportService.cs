using DCMS.Core.Domain.Report;
using DCMS.Core.Domain.Sales;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Report
{
    public interface IMainPageReportService
    {
        int GetAllocationCount(int storeId, int[] userIds);
        AllStoreDashboard GetAllStoreDashboard();
        List<HotSaleRanking> GetAllStoreHotOrderRanking(int itop, int itype);
        List<HotSaleRanking> GetAllStoreHotSaleRanking(int itop, int itype);
        List<AllStoreOrderTotal> GetAllStoreOrderTotal(int storeId);
        AllStoreSaleInformation GetAllStoreSaleInformation();
        List<AllStoreUnfinishedOrder> GetAllStoreUnfinishedOrder(int storeId, int itop, int itype);
        int GetCashReceiptCount(int storeId, int[] userIds);
        int GetChangeCount(int storeId, int[] userIds);
        DashboardReport GetDashboardReport(int? storeId, int[] businessUserIds, bool include = true);
        int GetDispatchCount(int storeId, int[] userIds);
        IList<MonthSaleReport> GetMonthSaleReport(int? storeId, DateTime? startTime, DateTime? endTime, int[] brandIds, int[] businessUserIds);
        IList<SalePercentReport> GetSalePercentReport(int? storeId, int type, int[] businessUserIds);

         IList<BussinessVisitStoreReport> GetBussinessVisitStoreReport(int? storeId, DateTime? startTime, DateTime? endTime, int[] businessUserIds);
        int GetOrderCount(int storeId, int[] userIds);
        int GetOrderCountByStore(int? storeId, int orderStatus, int dateDiff);
        int GetPendingCount(int storeId, int[] userIds);
        int GetReturnCount(int storeId, int[] userIds);
        int GetSaleCount(int storeId, int[] userIds);
    }
}