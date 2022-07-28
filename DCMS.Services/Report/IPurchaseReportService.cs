using DCMS.Core.Domain.Purchases;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Report
{
    public interface IPurchaseReportService
    {
        IList<PurchaseReportItem> GetPurchaseReportItem(int? storeId, int? makeuserId, int? productId, string productName, int? categoryId, int? manufacturerId, int? wareHouseId,
            string billNumber, int? purchaseTypeId, DateTime? startTime, DateTime? endTime, string remark);

        IList<PurchaseReportSummaryProduct> GetPurchaseReportSummaryProduct(int? storeId, int? categoryId, int? productId, string productName, int? manufacturerId, int? wareHouseId, DateTime? startTime, DateTime? endTime);

        IList<PurchaseReportSummaryManufacturer> GetPurchaseReportSummaryManufacturer(int? storeId, DateTime? startTime, DateTime? endTime, int? manufacturerId, Dictionary<int, string> dic);

    }
}