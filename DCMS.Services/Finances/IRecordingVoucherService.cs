using DCMS.Core;
using DCMS.Core.Domain.Finances;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Finances
{
    public interface IRecordingVoucherService : IVoucherService
    {
        void DeleteRecordingVoucher(RecordingVoucher recordingVoucher);
        void DeleteVoucherItem(VoucherItem voucherItem);
        IList<RecordingVoucher> GetAllRecordingVouchers(int? store);

        /// <summary>
        /// 根据经销商、单据状态、单据类型获取单据凭证
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="deleteStatus"></param>
        /// <param name="billTypeEnum"></param>
        /// <returns></returns>
        IList<RecordingVoucher> GetAllRecordingVouchersByStoreIdBillType(int storeId, bool? deleteStatus, BillTypeEnum billTypeEnum);

        IPagedList<RecordingVoucher> GetAllRecordingVouchers(int? store, int? makeuserId, int? generateMode, string billNumber = "", string summary = "", bool? status = null, DateTime? start = null, DateTime? end = null, int? billTypeId = null, string recordName = "", int? accountingOptionId = null, int pageIndex = 0, int pageSize = int.MaxValue);

        RecordingVoucher GetRecordingVoucherById(int? store, int recordingVoucherId, bool isInclulude=false);

        RecordingVoucher GetRecordingVoucher(int storeId, int billTypeId, string billNumber);
        List<RecordingVoucher> GetRecordingVouchers(int storeId, int billTypeId, string billNumber);
        IList<VoucherItem> GetVoucherItemsByAccounts(int? storeId, int[] accountsIds, DateTime? start, DateTime? end);
        IList<VoucherItem> GetVoucherItemsByRecordingVoucherId(int? storeId, int recordingVoucherId);
        VoucherItem GetVoucherItemById(int? store, int voucherItemId);
        IPagedList<VoucherItem> GetVoucherItemsByRecordingVoucherId(int recordingVoucherId, int? userId, int? storeId, int pageIndex, int pageSize);
        void InsertRecordingVoucher(RecordingVoucher recordingVoucher);
        void InsertVoucherItem(VoucherItem voucherItem);
        void UpdateRecordingVoucher(RecordingVoucher recordingVoucher);
        void UpdateVoucherItem(VoucherItem voucherItem);
        void DeleteVoucherItemWithVoucher(RecordingVoucher recordingVoucher);
        void DeleteRecordingVoucherFromPeriod(int? storeId, DateTime? period, string billNumber);
        IList<RecordingVoucher> GetLikeRecordingVoucherFromPeriod(int? storeId, DateTime? period, string billNumber);
        int GetRecordingVoucherNumber(int? store, DateTime period);
        bool CreateRecordingVoucher(int? store, int? makeUserId, RecordingVoucher recordingVoucher);
        IList<RecordingVoucher> GetRecordingVoucherFromPeriod(int? storeId, DateTime? period);
        Tuple<decimal, decimal, decimal> GetInitiallBalance(int? storeId, int accountingOptionId, DateTime? first, DateTime? last, decimal balance);
        BaseResult BillCreateOrUpdate(int storeId, int userId, int? billId, RecordingVoucher recordingVoucher, RecordingVoucherUpdate data, List<VoucherItem> items, bool isAdmin = false);

        BaseResult Auditing(int storeId, int userId, RecordingVoucher recordingVoucher);
        BaseResult Reverse(int userId, RecordingVoucher recordingVoucher);

        IList<VoucherItem> GetVoucherItemsByAccountingOptionIdFromPeriod(int? storeId, int accountingOptionId, DateTime? period);
        IList<VoucherItem> GetVoucherItemsByAccountCodeTypeIdFromPeriod(int? storeId, int accountCodeTypeId, DateTime? period);
        IList<VoucherItem> GetVoucherItemsByAccountingOptionIdFromPeriod(int? storeId, int accountingOptionId, DateTime? period, string numberName);
        IList<VoucherItem> GetVoucherItemsByAccountingOptionIdFromPeriod(int? storeId, int accountingOptionId, DateTime? start, DateTime? end, string numberName);

        VoucherItem GetPeriodLossSettle(int? storeId, int accountingOptionId, DateTime? period);
    }
}