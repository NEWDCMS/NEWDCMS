using DCMS.Core.Domain.Report;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Report
{
    public interface IStaffReportService
    {

        IList<StaffReportBusinessUserAchievement> GetStaffReportBusinessUserAchievement(int? storeId, int? categoryId, int? wareHouseId, int? terminalId, int? topNumber, DateTime? startTime, DateTime? endTime, bool status = false);
        IList<StaffReportPercentageSummary> GetStaffReportPercentageSummary(int? storeId, DateTime? startTime, DateTime? endTime, int? staffUserId, int? categoryId, int? productId);
        IList<StaffReportPercentageItem> GetStaffReportPercentageItem(int? storeId, int userType, DateTime? startTime, DateTime? endTime, int? staffUserId, int? categoryId, int? productId);
        IList<StaffSaleQuery> StaffSaleQuery(int? storeId, int? businessUserId, DateTime? startTime = null, DateTime? endTime = null);
        IList<VisitSummeryQuery> GetVisitSummeryQuery(int type, int? storeId, int? businessUserId, DateTime? start = null, DateTime? end = null);
        IList<BusinessUserVisitOfYear> GetBusinessUserVisitOfYearList(int? storeId, int year, int month);
    }
}