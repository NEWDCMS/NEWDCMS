using DCMS.Core.Domain.Report;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Report
{
    public interface IMarketReportService
    {

        IList<MarketReportTerminalActive> GetMarketReportTerminalActive(int? storeId, int? noVisitDayMore, int? terminalId, string terminalName, int? noSaleDayMore, int? districtId, int? staffUserId);

        IList<MarketReportTerminalValueAnalysis> GetMarketReportTerminalValueAnalysis(int? storeId, int? terminalId, string terminalName, int? districtId);

        IList<MarketReportTerminalLossWarning> GetMarketReportTerminalLossWarning(int? storeId, int? terminalId, string terminalName, int? districtId);

        IList<MarketReportShopRate> GetMarketReportShopRate(int? storeId, int? productId, string productName, int? categoryId, int? brandId, int? districtId, DateTime? startTime, DateTime? endTime, int? bussinessUserId);


    }
}