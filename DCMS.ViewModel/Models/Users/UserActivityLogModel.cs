using DCMS.Web.Framework;
using DCMS.Web.Framework.Models;
using System;


namespace DCMS.ViewModel.Models.Users
{

    public partial class UserActivityLogModel : BaseEntityModel
    {
        #region Properties

        [HintDisplayName("Admin.Users.Users.ActivityLog.ActivityLogType", "")]
        public string ActivityLogTypeName { get; set; }

        [HintDisplayName("Admin.Users.Users.ActivityLog.Comment", "")]
        public string Comment { get; set; }

        [HintDisplayName("Admin.Users.Users.ActivityLog.CreatedOn", "")]
        public DateTime CreatedOn { get; set; }

        [HintDisplayName("Admin.Users.Users.ActivityLog.IpAddress", "")]
        public string IpAddress { get; set; }

        #endregion
    }
}