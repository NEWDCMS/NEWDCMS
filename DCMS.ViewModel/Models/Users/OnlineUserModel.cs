using DCMS.Web.Framework.Models;
using System;
using System.ComponentModel;


namespace DCMS.ViewModel.Models.Users
{
    public partial class OnlineUserModel : BaseEntityModel
    {
        [DisplayName("用户信息")]
        public string UserInfo { get; set; }

        [DisplayName("IP地址")]
        public string LastIpAddress { get; set; }

        [DisplayName("位置")]
        public string Location { get; set; }

        [DisplayName("最后活动时间")]
        public DateTime LastActivityDate { get; set; }

        [DisplayName("最后访问时间")]
        public string LastVisitedPage { get; set; }
    }
}