using DCMS.Core;
using DCMS.Core.Domain.Census;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DCMS.Services.Census
{
    public interface IVisitStoreService
    {
        //业务员拜访记录
        IList<VisitStore> GetVisitRecords(int? store, int? businessUserId, int? terminalId, string terminalName, int? districtId, int? channelId, int? visitTypeId, DateTime? start = null, DateTime? end = null, int pageIndex = 0, int pageSize = int.MaxValue);
        //业务员拜访达成
        IList<VisitStore> GetVisitReacheds(int? store, int? businessUserId, int? lineId, DateTime? start = null, DateTime? end = null);
        IPagedList<VisitStore> GetAllVisitRecords(int? store, int? businessUserId, int? terminalId, string terminalName, int? districtId, int? channelId, int? visitTypeId, DateTime? start = null, DateTime? end = null, int pageIndex = 0, int pageSize = int.MaxValue);
        IPagedList<VisitStore> GetAllVisitReacheds(int? store, int? businessUserId, IList<int> lineIds, DateTime? start = null, DateTime? end = null, int pageIndex = 0, int pageSize = int.MaxValue);

        VisitStore GetLastRecord(int? store, int? visitid, int? terminalId, int? businessUserId);

        VisitStore GetRecordById(int? store, int? visitId);
        VisitStore GetUserLastRecord(int? store, int? terminalId, int? businessUserId);
        IList<VisitStore> GetVisitRecordsByUserid(int? store, int? businessUserId);

        //客户拜访活跃度排行
        IList<VisitStore> GetCustomerActivityRanking(int? store, int? businessUserId = 0, int? terminalId = 0);

        //获取门头照片
        IList<DoorheadPhoto> GetDoorheadPhotoByVisitId(int visitId = 0);
        //获取陈列照片
        IList<DisplayPhoto> GetDisplayPhotoByVisitId(int visitId = 0);
        //添加门头照片
        int InsertDoorheadPhoto(DoorheadPhoto photo);
        //添加陈列照片
        int InsertDisplayPhoto(DisplayPhoto photo);
        void InsertVisitStore(VisitStore model);
        void UpdateVisitStore(VisitStore model);
        IList<VisitStore> GetLastVisitRecordsByTerminalId(int? store, int? terminalId);
        IList<VisitStore> GetVisitRecordsByUserid(int? store, int? businessUserId, DateTime? date);
        IList<VisitStore> GetVisitRecordsByUserid(int? store, int? businessUserId, DateTime? start, DateTime? end);
        VisitStore GetUserLastOutRecord(int? store, int? businessUserId);
        VisitStore CheckOut(int? store, int? terminalId, int? businessUserId);
        IQueryable<QueryVisitStoreAndTracking> GetQueryVisitStoreAndTracking(int? store = 0, int? businessUserId = 0, DateTime? start = null, DateTime? end = null);
        IQueryable<QueryVisitStoreAndTracking> GetQueryVisitStoreAndTrackingForWeb(int? store = 0, int? businessUserId = 0, DateTime? start = null, DateTime? end = null);
        BaseResult SignInVisitStore(int? storeId, int userId, VisitStore visitStore, VisitStore data, bool isAdmin = false);
        IList<ReachQuery> GetReachs(int? store, int? userId = 0, DateTime? start = null, DateTime? end = null);
        IList<ReachOnlineQuery> GetLineReachs(int? store, int? userId = 0);
    }
}
