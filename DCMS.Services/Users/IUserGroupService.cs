using DCMS.Core;
using DCMS.Core.Domain.Users;
using System.Collections.Generic;

namespace DCMS.Services.Security
{
    public interface IUserGroupService
    {
        void DeleteUserGroup(UserGroup userGroup);
        IPagedList<UserGroup> GetAllUserGroups(int? store, string userGroupname = null, int pageIndex = 0, int pageSize = int.MaxValue);
        UserGroup GetUserGroupById(int userGroupId);
        UserGroup GetUserGroupByName(string name);
        IList<UserGroup> GetUserGroupsByIds(int[] userGroupIds);
        List<UserGroup> GetUserGroupsByStore(int? store);
        void InsertUserGroup(UserGroup userGroup);
        void UpdateUserGroup(UserGroup userGroup);
        void InsertUserGroupUser(int groupId, int userId);
        void RemoveUserGroupUser(int groupId, int userId);
        void InsertUserGroupUserRole(int groupId, int userRoleId);
        void RemoveUserGroupUserRole(int groupId, int userRoleId);

        IList<UserGroupUser> GetUserGroupUsersByUserId(int userId);
        IList<UserGroupUserRole> GetUserGroupUserRoleByGroupId(int groupId);
    }
}