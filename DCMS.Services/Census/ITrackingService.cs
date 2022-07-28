using DCMS.Core;
using DCMS.Core.Domain.Census;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Census
{
    public interface ITrackingService
    {
        IList<Tracking> GetAll(int? storeId);
        Tracking GetTrackingById(int? store, int id);
        IList<Tracking> GetTrackings(int? storeId, int? businessUserId, DateTime? start, DateTime? end);
        IList<Tracking> GetTrackingByIds(int[] idArr);
        BaseResult InsertTracking(Tracking tracking);
        BaseResult InsertTrackings(List<Tracking> trackings);
        void DeleteTracking(Tracking tracking);
        void UpdateTracking(Tracking tracking);
    }
}
