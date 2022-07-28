using DCMS.Core;
using DCMS.Core.Domain.Logging;
using DCMS.Core.Domain.Users;
using System;
using System.Collections.Generic;

namespace DCMS.Services.Logging
{
    public partial interface IUserActivityService
    {
        void InsertActivityType(ActivityLogType activityLogType);
        void UpdateActivityType(ActivityLogType activityLogType);
        void DeleteActivityType(ActivityLogType activityLogType);
        IList<ActivityLogType> GetAllActivityTypes(int? store);
        ActivityLogType GetActivityTypeById(int activityLogTypeId);
        ActivityLog InsertActivity(string systemKeyword, string comment, params object[] commentParams);
        ActivityLog InsertActivity(User user, string systemKeyword, string comment, BaseEntity entity = null);
        ActivityLog InsertActivity(string systemKeyword,
            string comment, User user, params object[] commentParams);
        void DeleteActivity(ActivityLog activityLog);
        IPagedList<ActivityLog> GetAllActivities(DateTime? createdOnFrom,
            DateTime? createdOnTo, int? userId,
            int activityLogTypeId, int pageIndex, int pageSize);
        ActivityLog GetActivityById(int activityLogId);
        void ClearAllActivities();
    }
}
