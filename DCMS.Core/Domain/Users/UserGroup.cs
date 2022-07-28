using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DCMS.Core.Domain.Users
{

    /// <summary>
    /// 用户组
    /// </summary>
    public class UserGroup : BaseEntity
    {

        private ICollection<UserGroupUserRole> _userGroupUserRoles;
        private ICollection<UserGroupUser> _userGroupUsers;
        private IList<User> _users;
        private IList<UserRole> _userRoles;



        /// <summary>
        /// 组名
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { set; get; }

        /// <summary>
        /// 排序
        /// </summary>
        public int OrderSort { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [Column(TypeName = "BIT(1)")]
        public bool Enabled { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        #region 导航属性

        [JsonIgnore]
        public virtual IList<User> Users
        {
            get => _users ?? (_users = UserGroupUsers.Select(mapping => mapping.User).ToList());
        }

        [JsonIgnore]
        public virtual IList<UserRole> UserRoles
        {
            get => _userRoles ?? (_userRoles = UserGroupUserRoles.Select(mapping => mapping.UserRole).ToList());
        }

        /// <summary>
        /// 组角色集合
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<UserGroupUserRole> UserGroupUserRoles
        {
            get { return _userGroupUserRoles ?? (_userGroupUserRoles = new List<UserGroupUserRole>()); }
            protected set { _userGroupUserRoles = value; }
        }

        /// <summary>
        /// 组用户集合
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<UserGroupUser> UserGroupUsers
        {
            get { return _userGroupUsers ?? (_userGroupUsers = new List<UserGroupUser>()); }
            protected set { _userGroupUsers = value; }
        }

        #endregion



        #region 方法

        public void AddUserGroupUserRole(UserGroupUserRole userGroupUserRole)
        {
            UserGroupUserRoles.Add(userGroupUserRole);
            _userRoles = null;
        }
        public void RemoveUserGroupUserRole(UserGroupUserRole userGroupUserRole)
        {
            UserGroupUserRoles.Remove(userGroupUserRole);
            _userRoles = null;
        }


        public void AddUserGroupUser(UserGroupUser userGroupUser)
        {
            UserGroupUsers.Add(userGroupUser);
            _users = null;
        }
        public void RemoveUserGroupUser(UserGroupUser userGroupUser)
        {
            UserGroupUsers.Remove(userGroupUser);
            _users = null;
        }


        #endregion
    }



    public partial class UserGroupUserRole : BaseEntity
    {

        public int UserRole_Id { get; set; }
        public int UserGroup_Id { get; set; }


        [JsonIgnore]
        public virtual UserRole UserRole { get; set; }
        [JsonIgnore]
        public virtual UserGroup UserGroup { get; set; }
    }
}
