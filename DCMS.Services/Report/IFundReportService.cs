using DCMS.Core.Domain.Report;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Report
{
    public interface IFundReportService
    {

        IList<CustomerAccountDealings> GetFundReportCustomerAccount(int? storeId, int? districtId, int? channelId, int? terminalId, string terminalName, string billNumber,
            int? billTypeId, DateTime? startTime, DateTime? endTime, string remark);

        IList<FundReportCustomerReceiptCash> GetFundReportCustomerReceiptCash(int? storeId, int? channelId, int? bussinessUserId, int? districtId, int? terminalId, string terminalName,
            int? moreDay, DateTime? startTime, DateTime? endTime, string remark);

        IList<FundReportManufacturerAccount> GetFundReportManufacturerAccount(int? storeId, string billNumber, int? billTypeId, int? manufacturerId, string remark, DateTime? startTime, DateTime? endTime);

        IList<FundReportManufacturerPayCash> GetFundReportManufacturerPayCash(int? storeId, int? bussinessUserId, int? moreDay, DateTime? startTime, DateTime? endTime);

        IList<FundReportAdvanceReceiptOverage> GetFundReportAdvanceReceiptOverage(int? storeId, int? terminalId);


        IList<FundReportAdvancePaymentOverage> GetFundReportAdvancePaymentOverage(int? storeId, int? manufacturerId, DateTime? endTime);


    }
}