using DCMS.Core.Domain.Security;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DCMS.Core.Domain.Users
{
    /// <summary>
    /// 用户角色
    /// </summary>
    public class UserRole : BaseEntity
    {
        private ICollection<PermissionRecordRoles> _permissionRecordRoles;
        private ICollection<ModuleRole> _moduleRoles;
        private ICollection<UserGroupUserRole> _userGroupUserRoles;
        private IList<Module> _modules;



        /// <summary>
        /// 角色名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 是否有效
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Active { get; set; }

        /// <summary>
        /// 是否系统角色
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool IsSystemRole { get; set; }

        /// <summary>
        /// 系统名称
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 是否启用必须在指定时间后更改密码
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool EnablePasswordLifetime { get; set; }

        [Column(TypeName = "BIT(1)")]
        public bool UseACLPc { get; set; }

        [Column(TypeName = "BIT(1)")]
        public bool UseACLMobile { get; set; }


        #region 导航属性


        [JsonIgnore]
        public virtual IList<Module> Modules
        {
            get => _modules ?? (_modules = ModuleRoles.Select(mapping => mapping.Module).ToList());
        }


        /// <summary>
        /// 获取/设置模块
        /// </summary>
        public virtual ICollection<ModuleRole> ModuleRoles
        {
            get { return _moduleRoles ?? (_moduleRoles = new List<ModuleRole>()); }
            protected set { _moduleRoles = value; }
        }

        /// <summary>
        /// 获取/设置权限
        /// </summary>
        public virtual ICollection<PermissionRecordRoles> PermissionRecordRoles
        {
            get { return _permissionRecordRoles ?? (_permissionRecordRoles = new List<PermissionRecordRoles>()); }
            set { _permissionRecordRoles = value; }
        }


        /// <summary>
        /// 用户组
        /// </summary>
        public virtual ICollection<UserGroupUserRole> UserGroupUserRoles
        {
            get { return _userGroupUserRoles ?? (_userGroupUserRoles = new List<UserGroupUserRole>()); }
            protected set { _userGroupUserRoles = value; }
        }


        #endregion



        #region 方法


        public void AddModuleRoles(ModuleRole mr)
        {
            ModuleRoles.Add(mr);
            _moduleRoles = null;
        }


        public void RemoveAddModuleRoles(ModuleRole mr)
        {
            ModuleRoles.Remove(mr);
            _moduleRoles = null;
        }


        #endregion
    }
}