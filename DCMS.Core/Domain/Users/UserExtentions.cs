using System;
using System.Linq;
using DCMS.Core.Infrastructure;
using DCMS.Core.Configuration;


namespace DCMS.Core.Domain.Users
{
    public static class UserExtentions
    {
        #region User role

        /// <summary>
        /// 是否具有有效的用户角色（注意：取消延迟加载不可用）
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="userRoleSystemName"></param>
        /// <param name="onlyActiveUserRoles"></param>
        /// <returns>Result</returns>
        public static bool IsInUserRole(this User user,
            string userRoleSystemName, bool onlyActiveUserRoles = true)
        {
            if (user == null)
            {
                return false; 
            }

            if (string.IsNullOrEmpty(userRoleSystemName))
            {
                return false; 
            }

            if (user.UserUserRoles == null || user.UserUserRoles.Count == 0)
            {
                return user.UserRoles.Select(u => u)?.FirstOrDefault(cr => (!onlyActiveUserRoles || cr.Active) && (cr.SystemName == userRoleSystemName)) != null;
            }
            else
            {
                var urs = user.UserRoles.Where(s => user.UserUserRoles.Select(s => s.UserRoleId).Contains(s.Id));
                return urs.FirstOrDefault(cr => (!onlyActiveUserRoles || cr.Active) && (cr.SystemName == userRoleSystemName)) != null;

                //return user.UserUserRoles.Select(u => u.UserRole)?.FirstOrDefault(cr => (!onlyActiveUserRoles || cr.Active) && (cr.SystemName == userRoleSystemName)) != null;
            }
        }

        /// <summary>
        /// 是否管理员（注意：取消延迟加载不可用）
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="onlyActiveUserRoles"</param>
        /// <returns>Result</returns>
        public static bool IsAdmin(this User user, bool onlyActiveUserRoles = true)
        {
            return IsInUserRole(user, DCMSDefaults.Administrators, onlyActiveUserRoles);
        }


        /// <summary>
        /// 是否业务员（注意：取消延迟加载不可用）
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="onlyActiveUserRoles"></param>
        /// <returns>Result</returns>
        public static bool IsSalesman(this User user, bool onlyActiveUserRoles = true)
        {
            return IsInUserRole(user, DCMSDefaults.Salesmans, onlyActiveUserRoles);
        }



        #endregion

    }
}
