using DCMS.Core;
using DCMS.Core.Domain.Products;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Products
{
    public interface IGiveQuotaService
    {
        void DeleteGiveQuota(GiveQuota giveQuotas);
        void DeleteGiveQuotaOption(GiveQuotaOption giveQuotaOption);
        IList<GiveQuotaOption> GetAllGiveQuotaOptions(int? giveId);

        /// <summary>
        /// 获取主管额度
        /// </summary>
        /// <param name="year">日期</param>
        /// <param name="leaderId">主管id</param>
        /// <returns></returns>
        IList<GiveQuotaOption> GetGiveQuotaOptions(int year, int leaderId);

        IList<GiveQuota> GetAllGiveQuotas();
        IList<GiveQuota> GetAllGiveQuotas(int? store);
        IPagedList<GiveQuota> GetAllGiveQuotas(int? store, int? year, int? userId, int pageIndex = 0, int pageSize = int.MaxValue);
        GiveQuota GetGiveQuotaById(int? store, int giveQuotasId);
        GiveQuota GetGiveQuotaByStoreIdUserIdYear(int storeId, int userId, int year);
        GiveQuotaOption GetGiveQuotaOptionById(int giveQuotaOptionId);
        IList<GiveQuotaOption> GetGiveQuotaOptionsByIds(int[] idArr);

        IList<GiveQuotaOption> GetGiveQuotaOptionByQuotaId(int? giveQuotaId);
        IList<GiveQuotaOption> GetGiveQuotaOptions(int? userId, int? year);
        IList<GiveQuota> GetGiveQuotas(int? userId, int? year);
        IList<GiveQuota> GetGiveQuotasByIds(int[] sIds);
        void InsertGiveQuota(GiveQuota giveQuotas);
        void UpdateGiveQuota(GiveQuota giveQuotas);

        void InsertGiveQuotaOption(GiveQuotaOption giveQuotaOption);
        void UpdateGiveQuotaOption(GiveQuotaOption giveQuotaOption);

        void DeleteGiveQuotaRecords(GiveQuotaRecords giveQuotaRecords);
        IPagedList<GiveQuotaRecords> GetAllGiveQuotaRecords(int? store, int? businessUserId, int? productid, int? customerId, int? catagoryId, int? costingCalCulateMethodId, int? giveTypeId, DateTime? start, DateTime? end, int pageIndex = 0, int pageSize = int.MaxValue);

        IPagedList<GiveQuotaRecordsSummery> GetAllGiveQuotaRecordsSummeries(int? store, int? businessUserId, int? productid, string productName, int? customerId, string terminalName, int? catagoryId, int? costingCalCulateMethodId, int? giveTypeId, DateTime? start, DateTime? end, int pageIndex = 0, int pageSize = int.MaxValue);

        IList<GiveQuotaRecordsSummery> GetAllGiveQuotaRecordsSummeries(int? store, int? businessUserId, int? productid, int? customerId, int? catagoryId, int? costingCalCulateMethodId, int? giveTypeId, DateTime? start, DateTime? end);
        IList<GiveQuotaRecords> GetAllGiveQuotaRecords(int? store);
        GiveQuotaRecords GetQuotaRecordsRepositoryById(int? store, int giveQuotaRecordsId);

        IList<GiveQuotaRecords> GetQuotaRecordsByBillId(int billId);

        void InsertGiveQuotaRecords(GiveQuotaRecords giveQuotaRecords);
        void UpdateGiveQuotaRecords(GiveQuotaRecords giveQuotaRecords);

        BaseResult BillCreateOrUpdate(int storeId, int userId, int? giveQuotaId, GiveQuota giveQuota, GiveQuotaUpdate data, List<GiveQuotaOption> items, bool isAdmin = false);

        IList<GiveQuotaRecords> GetQuotaRecordsByType(int? storeId, int customerId, int? giveTypeId, int?costContractId);

        /// <summary>
        /// 获取主管赠品余额
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="giveQuota"></param>
        /// <returns></returns>
        IList<GiveQuotaOption> GetGiveQuotaBalances(int? storeId, int year, int userId, int? giveQuotaId=0);

    }
}