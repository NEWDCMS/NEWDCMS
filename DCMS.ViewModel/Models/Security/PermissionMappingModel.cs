using DCMS.ViewModel.Models.Users;
using DCMS.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel;


namespace DCMS.ViewModel.Models.Security
{
    public partial class PermissionMappingModel : BaseModel
    {
        public PermissionMappingModel()
        {
            AvailablePermissions = new List<PermissionRecordModel>();
            AvailableUserRoles = new List<UserRoleModel>();
            Allowed = new Dictionary<string, IDictionary<int, bool>>();
        }

        /// <summary>
        /// 有效权限
        /// </summary>
        public IList<PermissionRecordModel> AvailablePermissions { get; set; }

        /// <summary>
        /// 有效角色
        /// </summary>
        public IList<UserRoleModel> AvailableUserRoles { get; set; }

        /// <summary>
        /// 允许记录
        /// </summary>
        //[permission system name] / [user role id] / [allowed]
        public IDictionary<string, IDictionary<int, bool>> Allowed { get; set; }
    }


    public partial class PermissionRecordListModel : BaseEntityModel
    {
        public PermissionRecordListModel()
        {
            PagingFilteringContext = new PagingFilteringModel();
        }

        public PagingFilteringModel PagingFilteringContext { get; set; }
        public IList<PermissionRecordModel> PermissionRecords { get; set; }


        public SelectList Stores { get; set; }

        [DisplayName("名称")]
        public string Name { get; set; }
        [DisplayName("系统名")]
        public string SystemName { get; set; }
    }

}